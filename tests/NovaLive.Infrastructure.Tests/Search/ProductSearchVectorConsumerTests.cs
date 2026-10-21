using FluentAssertions;
using MassTransit;
using MediatR;
using Moq;
using NovaLive.Application.UseCases.Products;
using NovaLive.Application.UseCases.Products.Commands.RebuildProductSearchVector;
using NovaLive.Contracts.V1.Products;
using NovaLive.Domain.Common;
using NovaLive.Infrastructure.Search;

namespace NovaLive.Infrastructure.Tests.Search;

public sealed class ProductSearchVectorConsumerTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Consume_MapsSpuIdAndCancellation_AndPropagatesFailure(bool success)
    {
        var spuId = Guid.NewGuid();
        using var cancellation = new CancellationTokenSource();
        var sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.Is<RebuildProductSearchVectorCommand>(c => c.SpuId == spuId), cancellation.Token))
            .ReturnsAsync(success ? Result.Success() : Result.Failure(ProductErrors.SearchIndexRebuildFailed));
        var context = new Mock<ConsumeContext<ProductUpdatedEvent>>();
        context.SetupGet(c => c.Message).Returns(new ProductUpdatedEvent(spuId, Guid.NewGuid(), "SkuUpdated", Guid.NewGuid()));
        context.SetupGet(c => c.CancellationToken).Returns(cancellation.Token);
        var consumer = new ProductSearchVectorConsumer(sender.Object);

        if (success) await consumer.Consume(context.Object);
        else await FluentActions.Awaiting(() => consumer.Consume(context.Object)).Should()
            .ThrowAsync<InvalidOperationException>().WithMessage("*Product.SearchIndexRebuildFailed*");

        sender.Verify(s => s.Send(It.Is<RebuildProductSearchVectorCommand>(c => c.SpuId == spuId), cancellation.Token), Times.Once);
    }

    [Fact]
    public async Task Consume_DuplicateAndOutOfOrderEvents_AlwaysRequestsRebuildFromCurrentState()
    {
        var spuId = Guid.NewGuid();
        var newest = new ProductUpdatedEvent(spuId, Guid.NewGuid(), "Updated", OccurredAt:
            new DateTimeOffset(2026, 10, 21, 0, 0, 0, TimeSpan.Zero));
        var older = newest with { Action = "Created", OccurredAt = newest.OccurredAt.AddMinutes(-1) };
        var sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.Is<RebuildProductSearchVectorCommand>(c => c.SpuId == spuId), default))
            .ReturnsAsync(Result.Success());
        var consumer = new ProductSearchVectorConsumer(sender.Object);

        foreach (var message in new[] { newest, newest, older })
        {
            var context = new Mock<ConsumeContext<ProductUpdatedEvent>>();
            context.SetupGet(c => c.Message).Returns(message);
            await consumer.Consume(context.Object);
        }

        sender.Verify(s => s.Send(It.Is<RebuildProductSearchVectorCommand>(c => c.SpuId == spuId), default), Times.Exactly(3));
    }

    [Fact]
    public async Task Consume_SenderThrows_PropagatesToTransport()
    {
        var sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.IsAny<RebuildProductSearchVectorCommand>(), default))
            .ThrowsAsync(new TimeoutException("DB unavailable"));
        var context = new Mock<ConsumeContext<ProductUpdatedEvent>>();
        context.SetupGet(c => c.Message).Returns(new ProductUpdatedEvent(Guid.NewGuid(), Guid.NewGuid(), "Updated"));
        var consumer = new ProductSearchVectorConsumer(sender.Object);

        await FluentActions.Awaiting(() => consumer.Consume(context.Object)).Should().ThrowAsync<TimeoutException>();
    }
}
