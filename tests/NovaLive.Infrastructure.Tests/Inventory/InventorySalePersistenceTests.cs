using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.Inventory.Commands.ConfirmInventorySale;
using NovaLive.Domain.Common;
using NovaLive.Infrastructure.Tests.Auth;

namespace NovaLive.Infrastructure.Tests.Inventory;

public sealed class InventorySalePersistenceTests
{
    [Fact]
    public async Task CommitAndReplayAfterCacheEviction_DeductsOnceWithExactLedger()
    {
        await using var host = new AuthTestHost(); await host.InitializeAsync(InventoryLifecycleFixture.Register);
        var payment = await InventoryLifecycleFixture.SeedAsync(host);
        var command = new ConfirmInventorySaleCommand(payment.Id);
        (await host.Send(command)).IsSuccess.Should().BeTrue();
        await host.Services.GetRequiredService<IIdempotencyService>().RemoveAsync("response:" + command.IdempotencyKey);
        (await host.Send(command)).IsSuccess.Should().BeTrue();
        await host.WithDb(async db =>
        {
            (await db.Inventories.ToListAsync()).Should().OnlyContain(i => i.QtyOnHand == 8 && i.ReservedQty == 0);
            (await db.InventoryHistories.ToListAsync()).Should().HaveCount(2).And.OnlyContain(h =>
                h.ChangeType == InventoryChangeType.SaleConfirmed && h.QtyBefore == 10 && h.ReservedBefore == 2
                && h.QtyChange == -2 && h.ReservedChange == -2 && h.QtyAfter == 8 && h.CreatedBy == null);
        });
    }

    [Fact]
    public async Task FailureAfterStockUpdate_RollsBackStockAndLedgerAndDoesNotCacheSuccess_ThenRetrySucceeds()
    {
        var flash = new Mock<IFlashSaleStockService>();
        flash.SetupSequence(f => f.ConfirmSoldAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(NovaLive.Application.UseCases.Inventory.InventoryErrors.ReservationMismatch))
            .ReturnsAsync(Result.Success());
        await using var host = new AuthTestHost();
        await host.InitializeAsync(s => { InventoryLifecycleFixture.Register(s); s.AddSingleton(flash.Object); });
        var payment = await InventoryLifecycleFixture.SeedAsync(host);
        var command = new ConfirmInventorySaleCommand(payment.Id);
        (await host.Send(command)).IsFailure.Should().BeTrue();
        await host.WithDb(async db =>
        {
            (await db.Inventories.ToListAsync()).Should().OnlyContain(i => i.QtyOnHand == 10 && i.ReservedQty == 2);
            (await db.InventoryHistories.CountAsync()).Should().Be(0);
        });
        (await host.Services.GetRequiredService<IIdempotencyService>().GetResponseAsync<Result>(command.IdempotencyKey))
            .Should().BeNull();
        (await host.Send(command)).IsSuccess.Should().BeTrue();
    }
}
