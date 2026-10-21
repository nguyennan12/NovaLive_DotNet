using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NovaLive.Application.UseCases.Orders;
using NovaLive.Infrastructure.System;

namespace NovaLive.Infrastructure.Tests.Orders;

public sealed class OrderTimeoutOptionsTests
{
    [Fact]
    public void Defaults_MatchTimeoutSpecification()
    {
        var options = new OrderTimeoutOptions();
        options.PollInterval.Should().Be(TimeSpan.FromMinutes(1));
        options.PaymentTimeout.Should().Be(TimeSpan.FromMinutes(15));
        options.BatchSize.Should().Be(100);
    }

    [Theory]
    [InlineData("00:02:00", "00:25:00", 7)]
    [InlineData("00:01:00", "00:15:00", 100)]
    public void InfrastructureRegistration_BindsOptionsAndRegistersWorker(string poll, string timeout, int batch)
    {
        using var provider = Provider(poll, timeout, batch);
        var options = provider.GetRequiredService<IOptions<OrderTimeoutOptions>>().Value;
        options.PollInterval.Should().Be(TimeSpan.Parse(poll));
        options.PaymentTimeout.Should().Be(TimeSpan.Parse(timeout)); options.BatchSize.Should().Be(batch);
    }

    [Theory]
    [InlineData("00:00:00", "00:15:00", 100)]
    [InlineData("00:01:00", "00:00:00", 100)]
    [InlineData("00:01:00", "00:15:00", 0)]
    public void InvalidSettings_FailValidation(string poll, string timeout, int batch)
    {
        using var provider = Provider(poll, timeout, batch);
        FluentActions.Invoking(() => provider.GetRequiredService<IOptions<OrderTimeoutOptions>>().Value)
            .Should().Throw<OptionsValidationException>();
    }

    private static ServiceProvider Provider(string poll, string timeout, int batch)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test",
            ["Redis:ConnectionString"] = "localhost:6379",
            ["OrderTimeout:PollInterval"] = poll, ["OrderTimeout:PaymentTimeout"] = timeout,
            ["OrderTimeout:BatchSize"] = batch.ToString()
        }).Build();
        var services = new ServiceCollection(); services.AddLogging(); services.AddInfrastructure(configuration);
        services.Should().Contain(d => d.ServiceType == typeof(IHostedService) && d.ImplementationType == typeof(TimeoutOrderRollbackWorker));
        return services.BuildServiceProvider();
    }
}
