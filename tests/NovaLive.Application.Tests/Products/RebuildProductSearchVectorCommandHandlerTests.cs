using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NovaLive.Application.Abstractions.Search;
using NovaLive.Application.UseCases.Products;
using NovaLive.Application.UseCases.Products.Commands.RebuildProductSearchVector;

namespace NovaLive.Application.Tests.Products;

public sealed class RebuildProductSearchVectorCommandHandlerTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    public async Task Handle_UpdatedOrMissingSpu_Succeeds(int affectedRows)
    {
        var spuId = Guid.NewGuid();
        using var cancellation = new CancellationTokenSource();
        var indexer = new Mock<IProductSearchIndexer>();
        indexer.Setup(i => i.RebuildSearchVectorAsync(spuId, cancellation.Token)).ReturnsAsync(affectedRows);
        var logger = new Mock<ILogger<RebuildProductSearchVectorCommandHandler>>();
        var handler = new RebuildProductSearchVectorCommandHandler(indexer.Object, logger.Object);

        var result = await handler.Handle(new RebuildProductSearchVectorCommand(spuId), cancellation.Token);

        result.IsSuccess.Should().BeTrue();
        indexer.Verify(i => i.RebuildSearchVectorAsync(spuId, cancellation.Token), Times.Once);
        if (affectedRows == 0)
        {
            logger.Verify(l => l.Log(LogLevel.Information, It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains(spuId.ToString())),
                It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Handle_IndexerFailure_ReturnsFailure(bool timeoutCancellation)
    {
        var spuId = Guid.NewGuid();
        var indexer = new Mock<IProductSearchIndexer>();
        Exception failure = timeoutCancellation ? new OperationCanceledException("DB timeout") : new TimeoutException("DB timeout");
        indexer.Setup(i => i.RebuildSearchVectorAsync(spuId, default)).ThrowsAsync(failure);
        var handler = new RebuildProductSearchVectorCommandHandler(indexer.Object,
            Mock.Of<ILogger<RebuildProductSearchVectorCommandHandler>>());

        var result = await handler.Handle(new RebuildProductSearchVectorCommand(spuId), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProductErrors.SearchIndexRebuildFailed);
    }

    [Fact]
    public async Task Handle_HostCancellation_PropagatesCancellation()
    {
        var spuId = Guid.NewGuid();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var indexer = new Mock<IProductSearchIndexer>();
        indexer.Setup(i => i.RebuildSearchVectorAsync(spuId, cancellation.Token))
            .ThrowsAsync(new OperationCanceledException(cancellation.Token));
        var handler = new RebuildProductSearchVectorCommandHandler(indexer.Object,
            Mock.Of<ILogger<RebuildProductSearchVectorCommandHandler>>());

        await FluentActions.Awaiting(() => handler.Handle(new RebuildProductSearchVectorCommand(spuId), cancellation.Token))
            .Should().ThrowAsync<OperationCanceledException>();
    }
}
