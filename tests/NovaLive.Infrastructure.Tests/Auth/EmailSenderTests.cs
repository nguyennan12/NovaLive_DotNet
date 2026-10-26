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
using NovaLive.Application.UseCases.Auth.Commands.Register;
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
            WebUtility.HtmlDecode(body.RootElement.GetProperty("html").GetString())
                .Should().Contain(description).And.Contain(Code).And.Contain("5 phút");
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"id\":\"test-message-id\"}") };
        });
        using var host = BuildHost("Production", "Resend", Key, "sender@example.com", logs, handler);
        await host.StartAsync();
        var sender = host.Services.GetRequiredService<IEmailSender>();
        sender.Should().BeOfType<ResendEmailSender>();
        host.Services.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(ResendEmailSender))
            .Timeout.Should().Be(TimeSpan.FromSeconds(10));
        await sender.SendOtpAsync("buyer@example.com", Code, purpose);
        handler.Calls.Should().Be(1);
        logs.Entries.Should().Contain(x => x.Level == LogLevel.Information && x.Text.Contains("test-message-id"));
        AssertSafe(logs);
    }

    [Theory]
    [InlineData(500, true, 2)]
    [InlineData(503, false, 2)]
    [InlineData(400, false, 1)]
    [InlineData(401, false, 1)]
    [InlineData(403, false, 1)]
    [InlineData(422, false, 1)]
    public async Task HttpFailures_RetryOnlyServerErrors_AndLogSafely(int status, bool recovers, int expectedCalls)
    {
        var logs = new CaptureLogs();
        var keys = new List<string>();
        using var handler = new StubHandler((request, _) =>
        {
            keys.Add(request.Headers.GetValues("Idempotency-Key").Single());
            return Task.FromResult(new HttpResponseMessage(keys.Count == 2 && recovers ? HttpStatusCode.OK : (HttpStatusCode)status)
            {
                Content = new StringContent("{\"message\":\"domain not verified " + Code + Key + "\"}"), ReasonPhrase = Code + Key
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
        logs.Entries.Should().Contain(x => x.Level == LogLevel.Error && x.Text.Contains($"HTTP {status}")
            && x.Text.Contains("domain not verified") && x.Text.Contains("[REDACTED]"));
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
    [InlineData("Development", "Both", "", "sender@example.com")]
    [InlineData("Production", "Both", Key, "")]
    [InlineData("Production", "", "", "")]
    [InlineData("Development", "unsupported", Key, "sender@example.com")]
    public async Task InvalidConfiguration_FailsAtHostStartup(string environment, string provider, string key, string from)
    {
        using var host = BuildHost(environment, provider, key, from, new CaptureLogs());
        Func<Task> start = () => host.StartAsync();
        var failure = await start.Should().ThrowAsync<OptionsValidationException>();
        var expected = provider is "Resend" or "Both"
            ? "Email:ApiKey and Email:FromAddress" : "Email:Provider must be one of Logging, Resend, Both";
        failure.Which.Failures.Should().Contain(x => x.Contains(expected));
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public async Task Logging_WithoutOptIn_DoesNotLogOtpOrRequireCredentials(string environment)
    {
        var logs = new CaptureLogs();
        using var host = BuildHost(environment, "Logging", "", "", logs);
        await host.StartAsync();
        var sender = host.Services.GetRequiredService<IEmailSender>();
        sender.Should().BeOfType<LoggingEmailSender>();
        await sender.SendOtpAsync("buyer@example.com", Code, OtpType.EmailVerify);
        AssertSafe(logs);
    }

    [Theory]
    [InlineData("Production", true, false, false)]
    [InlineData("Production", true, true, true)]
    [InlineData("Production", false, true, false)]
    [InlineData("Development", true, false, true)]
    [InlineData("Development", false, false, false)]
    [InlineData("Staging", true, false, true)]
    public async Task OtpLogging_RequiresExplicitFlags(string environment, bool logOtp, bool allowInProduction, bool shouldLog)
    {
        var logs = new CaptureLogs();
        using var host = BuildHost(environment, "Logging", "", "", logs, logOtp: logOtp, allowInProduction: allowInProduction);
        await host.StartAsync();
        await host.Services.GetRequiredService<IEmailSender>().SendOtpAsync("buyer@example.com", Code, OtpType.EmailVerify);
        if (shouldLog) logs.Entries.Should().Contain(x => x.Level == LogLevel.Information && x.Text.Contains(Code));
        else AssertSafe(logs);
        if (environment == "Production" && logOtp && !allowInProduction)
            logs.Entries.Should().Contain(x => x.Level == LogLevel.Warning && x.Text.Contains("logging suppressed in Production")
                && !x.Text.Contains(Code));
    }

    [Theory]
    [InlineData("Both")]
    [InlineData("both")]
    public async Task Both_ResolvesComposite_LogsOtpAndSendsRealEmail(string provider)
    {
        var logs = new CaptureLogs();
        using var handler = new StubHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"id\":\"both-message-id\"}")
        }));
        using var host = BuildHost("Development", provider, Key, "sender@example.com", logs, handler, logOtp: true);
        await host.StartAsync();
        var sender = host.Services.GetRequiredService<IEmailSender>();
        sender.Should().BeOfType<CompositeEmailSender>();
        await sender.SendOtpAsync("buyer@example.com", Code, OtpType.EmailVerify);
        handler.Calls.Should().Be(1);
        logs.Entries.Should().Contain(x => x.Text.Contains(Code));
        logs.Entries.Should().Contain(x => x.Level == LogLevel.Information && x.Text.Contains("both-message-id")
            && !x.Text.Contains(Code) && !x.Text.Contains(Key));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Composite_CallsEverySenderInOrder_AndLogsFailuresWithoutSecrets(bool firstFails, bool secondFails)
    {
        var logs = new CaptureLogs();
        var calls = new List<int>();
        var first = new Mock<IEmailSender>(MockBehavior.Strict);
        var second = new Mock<IEmailSender>(MockBehavior.Strict);
        using var cancellation = new CancellationTokenSource();
        first.Setup(x => x.SendOtpAsync("buyer@example.com", Code, OtpType.EmailVerify, cancellation.Token))
            .Returns(() => { calls.Add(1); return firstFails ? Task.FromException(new InvalidOperationException(Code + Key)) : Task.CompletedTask; });
        second.Setup(x => x.SendOtpAsync("buyer@example.com", Code, OtpType.EmailVerify, cancellation.Token))
            .Returns(() => { calls.Add(2); return secondFails ? Task.FromException(new InvalidOperationException(Code + Key)) : Task.CompletedTask; });
        using var factory = new LoggerFactory([logs]);
        var sender = new CompositeEmailSender([first.Object, second.Object], factory.CreateLogger<CompositeEmailSender>());
        await sender.SendOtpAsync("buyer@example.com", Code, OtpType.EmailVerify, cancellation.Token);
        calls.Should().Equal(1, 2);
        first.VerifyAll();
        second.VerifyAll();
        logs.Entries.Count(x => x.Level == LogLevel.Error).Should().Be((firstFails ? 1 : 0) + (secondFails ? 1 : 0));
        AssertSafe(logs);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task ExhaustedDelivery_RetainsRegistrationAndOtp_AndLogsFailure(bool timeout, bool composite)
    {
        var logs = new CaptureLogs();
        using var handler = new StubHandler((_, _) => timeout
            ? throw new TaskCanceledException(Code + Key)
            : Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { Content = new StringContent("delivery failed " + Key) }));
        using var client = new HttpClient(handler);
        var other = new Mock<IEmailSender>();
        await using var host = new AuthTestHost();
        await host.InitializeAsync(services =>
        {
            IEmailSender sender = CreateSender(client, logs);
            if (composite) sender = new CompositeEmailSender([sender, other.Object], new Logger<CompositeEmailSender>(new LoggerFactory([logs])));
            services.AddSingleton(sender);
            services.AddLogging(builder => builder.AddProvider(logs));
        });
        var result = await host.Send(new RegisterCommand(new("new@example.com", "0912345678", "Password123", "New")));
        result.IsSuccess.Should().BeTrue();
        await host.WithDb(async db =>
        {
            (await db.Users.CountAsync()).Should().Be(1);
            (await db.UserOtps.CountAsync()).Should().Be(1);
        });
        handler.Calls.Should().Be(2);
        if (composite) other.Verify(x => x.SendOtpAsync("new@example.com", It.IsAny<string>(), OtpType.EmailVerify, It.IsAny<CancellationToken>()), Times.Once);
        logs.Entries.Should().Contain(x => x.Level == LogLevel.Error && x.Text.Contains(composite ? "Continuing with remaining senders" : "failed for user"));
        AssertSafe(logs);
    }

    private static ResendEmailSender CreateSender(HttpClient client, CaptureLogs logs) => new(client,
        Options.Create(new EmailOptions { Provider = "Resend", ApiKey = Key, FromAddress = "sender@example.com", FromName = "NovaLive Test" }),
        new Logger<ResendEmailSender>(new LoggerFactory([logs])));

    private static IHost BuildHost(string environment, string provider, string key, string from, CaptureLogs logs, StubHandler? handler = null,
        bool logOtp = false, bool allowInProduction = false) =>
        new HostBuilder().UseEnvironment(environment)
            .ConfigureAppConfiguration(builder => builder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = new string('s', 32), ["Jwt:Issuer"] = "tests", ["Jwt:Audience"] = "tests",
                ["Otp:Pepper"] = new string('p', 32), ["Email:Provider"] = provider, ["Email:ApiKey"] = key,
                ["Email:FromAddress"] = from, ["Email:FromName"] = "NovaLive Test",
                ["Email:LogOtp"] = logOtp.ToString(), ["Email:AllowOtpLoggingInProduction"] = allowInProduction.ToString()
            }))
            .ConfigureLogging(builder => builder.ClearProviders().SetMinimumLevel(LogLevel.Trace).AddProvider(logs))
            .ConfigureServices((context, services) =>
            {
                services.AddAuthServices(context.Configuration);
                // Isolate email startup from unrelated database/token services, retaining DI validation.
                services.Replace(ServiceDescriptor.Singleton(Mock.Of<IJwtTokenService>()));
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
