using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.UseCases.Auth.Commands.Login;
using NovaLive.Application.UseCases.Auth.Commands.Refresh;
using NovaLive.Domain.Common;
using NovaLive.Domain.Users;
namespace NovaLive.Infrastructure.Tests.Auth;

public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("NOVALIVE_TEST_POSTGRES")))
            Skip = "Set NOVALIVE_TEST_POSTGRES to a disposable PostgreSQL server connection to run provider-specific tests.";
    }
}
public sealed class PostgresAuthTests
{
    [PostgresFact]
    public async Task ConcurrentRefresh_OnlyOneSucceeds_AndReuseRevokesTheWinningToken()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        await host.AddUser();
        var login = await host.Send(new LoginCommand(new("buyer@example.com", "Password123")));
        var command = new RefreshTokenCommand(new(login.Value!.RefreshToken));
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task<Result<NovaLive.Contracts.V1.Auth.AuthResponse>> Refresh()
        {
            await start.Task;
            return await host.Send(command);
        }
        var first = Task.Run(Refresh);
        var second = Task.Run(Refresh);
        start.SetResult();
        var results = await Task.WhenAll(first, second);
        results.Count(result => result.IsSuccess).Should().Be(1);
        results.Single(result => result.IsFailure).Error.Type.Should().Be(ErrorType.Unauthorized);
        await host.WithDb(async db => (await db.RefreshTokens.ToListAsync()).Should().HaveCount(2).And.OnlyContain(token => token.RevokedAt != null));
    }
    [PostgresFact]
    public async Task UniqueViolation_IsTranslatedToConflict_AndTransactionRollsBack()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        await host.AddUser();
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var persistence = scope.ServiceProvider.GetRequiredService<IAuthPersistence>();
        var result = await scope.ServiceProvider.GetRequiredService<ITransactionManager>().ExecuteAsync(async () =>
        {
            db.Users.Add(new User("buyer@example.com", "hash", "Race loser", "0987654321"));
            var error = await persistence.SaveRegistrationAsync(default);
            return error is null ? Result.Success() : Result.Failure(error);
        }, false, default);
        result.Error.Type.Should().Be(ErrorType.AlreadyExists);
        await host.WithDb(async context => (await context.Users.CountAsync()).Should().Be(1));
    }
}
