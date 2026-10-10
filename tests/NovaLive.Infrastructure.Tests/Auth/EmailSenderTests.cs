using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Auth;
using NovaLive.Domain.Common;
using NovaLive.Infrastructure.Auth;

namespace NovaLive.Infrastructure.Tests.Auth;

public sealed class EmailSenderTests
{
    private const string Code = "819274";
    private const string Key = "test-only-api-key";

    [Theory]
    [InlineData(OtpType.EmailVerify, "xác thực tài khoản")]
    [InlineData(OtpType.ForgotPassword, "đặt lại mật khẩu")]
    public async Task TypedClient_SendsExpectedRequest_WithoutLoggingSecrets(OtpType purpose, string description)
    {
        var logs = new CaptureLogs();
        using var handler = new StubHandler(async (request, ct) =>
        {
            request.Method.Should().Be(HttpMethod.Post);
            request.RequestUri.Should().Be(new Uri("https://api.resend.com/emails"));
            request.Headers.Authorization!.Scheme.Should().Be("Bearer");
            request.Headers.Authorization.Parameter.Should().Be(Key);
            request.Content!.Headers.ContentType!.MediaType.Should().Be("application/json");
            using var body = JsonDocument.Parse(await request.Content.ReadAsStringAsync(ct));
            body.RootElement.GetProperty("from").GetString().Should().Be("NovaLive Test <sender@example.com>");
            body.RootElement.GetProperty("to")[0].GetString().Should().Be("buyer@example.com");
            body.RootElement.GetProperty("subject").GetString().Should().Contain(description);
            body.RootElement.GetProperty("html").GetString().Should().Contain(description).And.Contain(Code).And.Contain("5 phút");
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        using var host = BuildHost("Production", "Resend", Key, "sender@example.com", logs, handler);
        await host.StartAsync();
        var sender = host.Services.GetRequiredService<IEmailSender>();
        sender.Should().BeOfType<ResendEmailSender>();
        host.Services.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(ResendEmailSender))
            .Timeout.Should().Be(TimeSpan.FromSeconds(10));
        await sender.SendOtpAsync("buyer@example.com", Code, purpose);
        handler.Calls.Should().Be(1);
        AssertSafe(logs);
    }

    [Theory]
    [InlineData(500, true, 2)]
    [InlineData(503, false, 2)]
    [InlineData(400, false, 1)]
    [InlineData(401, false, 1)]
    public async Task HttpFailures_RetryOnlyServerErrors_AndLogSafely(int status, bool recovers, int expectedCalls)
    {
        var logs = new CaptureLogs();
        var keys = new List<string>();
        using var handler = new StubHandler((request, _) =>
        {
            keys.Add(request.Headers.GetValues("Idempotency-Key").Single());
            return Task.FromResult(new HttpResponseMessage(keys.Count == 2 && recovers ? HttpStatusCode.OK : (HttpStatusCode)status)
            {
                Content = new StringContent(Code + Key), ReasonPhrase = Code + Key
            });
        });
        using var client = new HttpClient(handler);
        var sender = CreateSender(client, logs);
        Func<Task> send = () => sender.SendOtpAsync("buyer@example.com", Code, OtpType.EmailVerify);
        if (recovers) await send();
        else await send.Should().ThrowAsync<InvalidOperationException>();
        handler.Calls.Should().Be(expectedCalls);
        keys.Distinct().Should().ContainSingle();
        if (expectedCalls == 2) logs.Entries.Should().Contain(x => x.Level == LogLevel.Warning && x.Text.Contains("retrying once"));
        if (!recovers) logs.Entries.Should().Contain(x => x.Level == LogLevel.Error);
        AssertSafe(logs);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Timeout_RetriesOnce_AndLogsSafely(bool recovers)
    {
        var logs = new CaptureLogs();
        var calls = 0;
        using var handler = new StubHandler(async (_, ct) =>
        {
            if (++calls == 1 || !recovers) await Task.Delay(Timeout.Infinite, ct);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        using var client = new HttpClient(handler) { Timeout = TimeSpan.FromMilliseconds(50) };
        Func<Task> send = () => CreateSender(client, logs).SendOtpAsync("buyer@example.com", Code, OtpType.EmailVerify);
        if (recovers) await send();
        else await send.Should().ThrowAsync<TimeoutException>();
        handler.Calls.Should().Be(2);
        logs.Entries.Should().Contain(x => x.Level == LogLevel.Warning);
        if (!recovers) logs.Entries.Should().Contain(x => x.Level == LogLevel.Error);
        AssertSafe(logs);
    }

    [Fact]
    public async Task CallerCancellation_IsNotRetried()
    {
        using var cancellation = new CancellationTokenSource();
        using var handler = new StubHandler((_, ct) =>
        {
            cancellation.Cancel();
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });
        using var client = new HttpClient(handler);
        Func<Task> send = () => CreateSender(client, new CaptureLogs()).SendOtpAsync("buyer@example.com", Code, OtpType.EmailVerify, cancellation.Token);
        await send.Should().ThrowAsync<OperationCanceledException>();
        handler.Calls.Should().Be(1);
    }

    [Fact]
    public async Task TransportException_DoesNotLogProviderMessage()
    {
        var logs = new CaptureLogs();
        using var handler = new StubHandler((_, _) => throw new HttpRequestException(Code + Key));
        using var client = new HttpClient(handler);
        Func<Task> send = () => CreateSender(client, logs).SendOtpAsync("buyer@example.com", Code, OtpType.EmailVerify);
        await send.Should().ThrowAsync<InvalidOperationException>().WithMessage("Email delivery failed.");
        logs.Entries.Should().Contain(x => x.Level == LogLevel.Error);
        AssertSafe(logs);
    }

    [Theory]
    [InlineData("Production", "Resend", "", "sender@example.com")]
    [InlineData("Production", "Resend", Key, "")]
    [InlineData("Development", "Resend", "", "")]
    [InlineData("Production", "Logging", "", "")]
    [InlineData("Production", "", "", "")]
    [InlineData("Development", "unsupported", Key, "sender@example.com")]
    public async Task InvalidConfiguration_FailsAtHostStartup(string environment, string provider, string key, string from)
    {
        using var host = BuildHost(environment, provider, key, from, new CaptureLogs());
        Func<Task> start = () => host.StartAsync();
        await start.Should().ThrowAsync<OptionsValidationException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("Logging")]
    public async Task Development_UsesLoggingSenderWithoutCredentials(string provider)
    {
        var logs = new CaptureLogs();
        using var host = BuildHost("Development", provider, "", "", logs);
        await host.StartAsync();
        var sender = host.Services.GetRequiredService<IEmailSender>();
        sender.Should().BeOfType<LoggingEmailSender>();
        await sender.SendOtpAsync("buyer@example.com", Code, OtpType.EmailVerify);
        logs.Entries.Should().Contain(x => x.Text.Contains("delivery suppressed"));
        AssertSafe(logs);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExhaustedDelivery_RetainsRegistrationAndOtp_AndLogsFailure(bool timeout)
    {
        var logs = new CaptureLogs();
        using var handler = new StubHandler((_, _) => timeout
            ? throw new TaskCanceledException(Code + Key)
            : Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { Content = new StringContent(Code + Key) }));
        using var client = new HttpClient(handler);
        await using var host = new AuthTestHost();
        await host.InitializeAsync(services =>
        {
            services.AddSingleton<IEmailSender>(CreateSender(client, logs));
            services.AddLogging(builder => builder.AddProvider(logs));
        });
        var result = await host.Send(new RegisterUserCommand(new("new@example.com", "0912345678", "Password123", "New")));
        result.IsSuccess.Should().BeTrue();
        await host.WithDb(async db =>
        {
            (await db.Users.CountAsync()).Should().Be(1);
            (await db.UserOtps.CountAsync()).Should().Be(1);
        });
        handler.Calls.Should().Be(2);
        logs.Entries.Should().Contain(x => x.Level == LogLevel.Error && x.Text.Contains("failed for user"));
        AssertSafe(logs);
    }

    private static ResendEmailSender CreateSender(HttpClient client, CaptureLogs logs) => new(client,
        Options.Create(new EmailOptions { Provider = "Resend", ApiKey = Key, FromAddress = "sender@example.com", FromName = "NovaLive Test" }),
        new Logger<ResendEmailSender>(new LoggerFactory([logs])));

    private static IHost BuildHost(string environment, string provider, string key, string from, CaptureLogs logs, StubHandler? handler = null) =>
        new HostBuilder().UseEnvironment(environment)
            .ConfigureAppConfiguration(builder => builder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = new string('s', 32), ["Jwt:Issuer"] = "tests", ["Jwt:Audience"] = "tests",
                ["Otp:Pepper"] = new string('p', 32), ["Email:Provider"] = provider, ["Email:ApiKey"] = key,
                ["Email:FromAddress"] = from, ["Email:FromName"] = "NovaLive Test"
            }))
            .ConfigureLogging(builder => builder.ClearProviders().SetMinimumLevel(LogLevel.Trace).AddProvider(logs))
            .ConfigureServices((context, services) =>
            {
                services.AddAuthServices(context.Configuration);
                // Isolate email startup from unrelated database/token services, retaining DI validation.
                services.Replace(ServiceDescriptor.Singleton(Mock.Of<IJwtTokenService>()));
                services.Replace(ServiceDescriptor.Singleton(Mock.Of<IPermissionProvider>()));
                if (handler is not null) services.AddHttpClient<ResendEmailSender>().ConfigurePrimaryHttpMessageHandler(() => handler);
            }).Build();

    private static void AssertSafe(CaptureLogs logs) => string.Join("\n", logs.Entries.Select(x => x.Text))
        .Should().NotContain(Code).And.NotContain(Key);

    private sealed class StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        public int Calls { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            return send(request, cancellationToken);
        }
    }

    private sealed class CaptureLogs : ILoggerProvider
    {
        public global::System.Collections.Concurrent.ConcurrentBag<(LogLevel Level, string Text)> Entries { get; } = [];
        public ILogger CreateLogger(string categoryName) => new CaptureLogger(this);
        public void Dispose() { }
        private sealed class CaptureLogger(CaptureLogs logs) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
                => logs.Entries.Add((logLevel, formatter(state, exception) + exception));
        }
    }
}
