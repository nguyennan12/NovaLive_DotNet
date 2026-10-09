using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NovaLive.Domain.Users;
using NovaLive.Infrastructure.Persistence;
namespace NovaLive.Infrastructure.Tests.Auth;

public sealed class RefreshAtomicityTests
{
    [Fact]
    public async Task ConditionalRefreshUpdate_AllowsOnlyOneConcurrentConsumer()
    {
        // Separate physical connections exercise the same conditional SQL UPDATE used in production.
        var path = Path.Combine(Path.GetTempPath(), $"novalive-auth-{Guid.NewGuid():N}.db");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={path};Pooling=False;Default Timeout=30")
            .UseSnakeCaseNamingConvention().ReplaceService<IModelCustomizer, SqliteTimeModelCustomizer>().Options;
        var now = new DateTimeOffset(2026, 10, 8, 0, 0, 0, TimeSpan.Zero);
        var token = new RefreshToken(Guid.NewGuid(), "hash", null, null, now);
        try
        {
            await using (var db = new AppDbContext(options))
            {
                await db.Database.EnsureCreatedAsync();
                db.RefreshTokens.Add(token);
                await db.SaveChangesAsync();
            }
            using var ready = new CountdownEvent(2);
            var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            async Task<bool> Consume()
            {
                await using var db = new AppDbContext(options);
                ready.Signal();
                await start.Task;
                return await new AuthPersistence(db).ConsumeRefreshTokenAsync(token.Id, now, default);
            }
            var first = Task.Run(Consume);
            var second = Task.Run(Consume);
            ready.Wait(TimeSpan.FromSeconds(10)).Should().BeTrue();
            start.SetResult();
            var results = await Task.WhenAll(first, second);
            results.Count(success => success).Should().Be(1);
        }
        finally { File.Delete(path); }
    }
}

