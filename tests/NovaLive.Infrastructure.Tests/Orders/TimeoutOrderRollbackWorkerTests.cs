using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.Inventory;
using NovaLive.Application.UseCases.Orders;
using NovaLive.Application.UseCases.Orders.Commands.ExpireUnpaidPayment;
using NovaLive.Domain.Common;
using NovaLive.Infrastructure.System;
using NovaLive.Infrastructure.Tests.Auth;
using NovaLive.Infrastructure.Tests.Inventory;

namespace NovaLive.Infrastructure.Tests.Orders;

public sealed class TimeoutOrderRollbackWorkerTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Tick_SelectsBoundedBatch_DispatchesInSeparateScopes_AndContinuesAfterFailure(bool throws)
    {
        var sender = new Mock<ISender>();
        var calls = new List<Guid>(); var scopes = 0;
        sender.Setup(s => s.Send(It.IsAny<ExpireUnpaidPaymentCommand>(), It.IsAny<CancellationToken>()))
            .Returns((ExpireUnpaidPaymentCommand command, CancellationToken _) =>
            {
                calls.Add(command.PaymentId);
                if (calls.Count == 1 && throws) throw new IOException("test failure");
                return Task.FromResult(calls.Count == 1 ? Result.Failure(InventoryErrors.ReservationMismatch) : Result.Success());
            });
        await using var host = new AuthTestHost();
        await host.InitializeAsync(s =>
        {
            InventoryLifecycleFixture.Register(s);
            s.AddScoped<ISender>(_ => { scopes++; return sender.Object; });
        });
        var first = await InventoryLifecycleFixture.SeedAsync(host, paid: false);
        host.Clock.UtcNow = host.Clock.UtcNow.AddMinutes(1);
        var second = await InventoryLifecycleFixture.SeedAsync(host, paid: false);
        host.Clock.UtcNow = host.Clock.UtcNow.AddMinutes(1);
        await InventoryLifecycleFixture.SeedAsync(host, paid: false);
        await InventoryLifecycleFixture.SeedAsync(host, paid: false, method: PaymentMethod.COD);
        using var worker = new TimeoutOrderRollbackWorker(host.Services.GetRequiredService<IServiceScopeFactory>(), host.Clock,
            Options.Create(new OrderTimeoutOptions { BatchSize = 2 }), NullLogger<TimeoutOrderRollbackWorker>.Instance);
        await worker.RunOnceAsync(default);
        calls.Should().Equal(first.Id, second.Id);
        scopes.Should().Be(2);
    }

    [Fact]
    public async Task Cancellation_DoesNotDispatch()
    {
        var sender = new Mock<ISender>(MockBehavior.Strict);
        await using var host = new AuthTestHost();
        await host.InitializeAsync(s => { InventoryLifecycleFixture.Register(s); s.AddSingleton(sender.Object); });
        using var worker = new TimeoutOrderRollbackWorker(host.Services.GetRequiredService<IServiceScopeFactory>(), host.Clock,
            Options.Create(new OrderTimeoutOptions()), NullLogger<TimeoutOrderRollbackWorker>.Instance);
        using var stop = new CancellationTokenSource(); stop.Cancel();
        await FluentActions.Awaiting(() => worker.RunOnceAsync(stop.Token)).Should().ThrowAsync<OperationCanceledException>();
        sender.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ScanException_DoesNotFaultHostedWorker_AndShutdownIsRespected()
    {
        var payments = new Mock<NovaLive.Application.Abstractions.Persistence.Repositories.IPaymentRepository>();
        var scanned = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        payments.Setup(p => p.GetExpiredOnlineIdsAsync(It.IsAny<DateTimeOffset>(), It.IsAny<TimeSpan>(),
                It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(() => { scanned.TrySetResult(); throw new IOException("scan failed"); });
        var services = new ServiceCollection(); services.AddSingleton(payments.Object);
        using var provider = services.BuildServiceProvider();
        var clock = new Mock<IDateTimeProvider>(); clock.SetupGet(c => c.UtcNow).Returns(new DateTimeOffset(2026, 10, 21, 0, 0, 0, TimeSpan.Zero));
        using var worker = new TimeoutOrderRollbackWorker(provider.GetRequiredService<IServiceScopeFactory>(), clock.Object,
            Options.Create(new OrderTimeoutOptions()), NullLogger<TimeoutOrderRollbackWorker>.Instance);
        await worker.StartAsync(default);
        await scanned.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await worker.StopAsync(default);
        worker.ExecuteTask!.IsCompletedSuccessfully.Should().BeTrue();
    }
}
