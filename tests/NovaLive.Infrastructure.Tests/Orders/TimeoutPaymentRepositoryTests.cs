using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Common;
using NovaLive.Domain.Payments;
using NovaLive.Infrastructure.Tests.Auth;
using NovaLive.Infrastructure.Tests.Inventory;

namespace NovaLive.Infrastructure.Tests.Orders;

public sealed class TimeoutPaymentRepositoryTests
{
    [Fact]
    public async Task QueryAndClaim_OnlyDuePendingOnline_RespectFallbackBoundaryAndBatch()
    {
        await using var host = new AuthTestHost(); await host.InitializeAsync(InventoryLifecycleFixture.Register);
        var now = host.Clock.UtcNow;
        Payment Make(PaymentMethod method, int age, DateTimeOffset? expiry, PaymentStatus status = PaymentStatus.Pending)
        {
            var p = Payment.Create(Guid.NewGuid(), method, 100, now.AddMinutes(-age));
            typeof(Payment).GetProperty("ExpiredAt")!.SetValue(p, expiry);
            if (status == PaymentStatus.Success) p.MarkSuccess(Guid.NewGuid().ToString(), now);
            return p;
        }
        var oldest = Make(PaymentMethod.MoMo, 40, now.AddMinutes(-10));
        var legacy = Make(PaymentMethod.VietQR, 30, null);
        var boundary = Make(PaymentMethod.VietQR, 15, null);
        var explicitBoundary = Make(PaymentMethod.MoMo, 1, now);
        var excluded = new[] {
            Make(PaymentMethod.COD, 100, now.AddMinutes(-50)),
            Make(PaymentMethod.MoMo, 100, now.AddMinutes(-50), PaymentStatus.Success),
            Make(PaymentMethod.MoMo, 14, null),
            Make(PaymentMethod.VietQR, 40, now.AddMinutes(1)) };
        await host.WithDb(async db =>
        {
            db.Payments.AddRange(excluded.Concat([oldest, legacy, boundary, explicitBoundary]));
            await db.SaveChangesAsync();
        });
        using var scope = host.Services.CreateScope();
        var payments = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
        (await payments.GetExpiredOnlineIdsAsync(now, TimeSpan.FromMinutes(15), 2, default)).Should().Equal(oldest.Id, legacy.Id);
        (await payments.GetExpiredOnlineIdsAsync(now, TimeSpan.FromMinutes(15), 100, default))
            .Should().Equal(oldest.Id, legacy.Id, boundary.Id, explicitBoundary.Id);
        foreach (var excludedPayment in excluded)
            (await payments.TryExpirePendingOnlineAsync(excludedPayment.Id, now, TimeSpan.FromMinutes(15), default)).Should().BeFalse();
        (await payments.TryExpirePendingOnlineAsync(oldest.Id, now, TimeSpan.FromMinutes(15), default)).Should().BeTrue();
        (await payments.TryExpirePendingOnlineAsync(oldest.Id, now, TimeSpan.FromMinutes(15), default)).Should().BeFalse();
    }
}
