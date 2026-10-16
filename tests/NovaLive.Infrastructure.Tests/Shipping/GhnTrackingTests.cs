using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NovaLive.Infrastructure.Shipping.Ghn;

namespace NovaLive.Infrastructure.Tests.Shipping;

public sealed class GhnTrackingTests
{
    private static GhnShippingAdapter Create(HttpMessageHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("https://example.test") },
            Options.Create(new GhnOptions()), NullLogger<GhnShippingAdapter>.Instance);

    [Fact]
    public async Task Detail_UsesEncodedCodeAndMapsCurrentStatusAndHistory()
    {
        using var handler = new ShippingStubHandler((request, _) =>
        {
            request.Method.Should().Be(HttpMethod.Get);
            request.RequestUri!.AbsolutePath.Should().Be("/shiip/public-api/v2/shipping-order/detail");
            request.RequestUri.Query.Should().Contain("order_code=TRACK%26one");
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(
                """{"code":200,"data":{"status":"delivering","updated_date":"2026-10-16T12:00:00+07:00","log":[{"status":"picking","updated_date":"2026-10-15T00:00:00Z"},{"status":"delivering","updated_date":"2026-10-16T05:00:00Z"}]}}""",
                global::System.Text.Encoding.UTF8, "application/json") });
        });
        var result = await Create(handler).GetTrackingAsync("TRACK&one");
        result.IsSuccess.Should().BeTrue(); result.Value!.Status.Should().Be("delivering");
        result.Value.Events.Select(e => e.Status).Should().Equal("picking", "delivering");
        result.Value.Events.Should().OnlyContain(e => e.OccurredAt.Offset == TimeSpan.Zero);
    }

    [Theory]
    [InlineData(500, "{}")]
    [InlineData(200, "{")]
    [InlineData(200, """{"code":400,"data":null}""")]
    [InlineData(200, """{"code":200,"data":{}}""")]
    public async Task FailedHttpOrMalformedPayload_IsResult(int status, string json)
    {
        using var handler = new ShippingStubHandler((_, _) => Task.FromResult(new HttpResponseMessage((HttpStatusCode)status)
        { Content = new StringContent(json, global::System.Text.Encoding.UTF8, "application/json") }));
        (await Create(handler).GetTrackingAsync("TRACK")).Error.Code.Should().Be("Shipping.ProviderError");
    }

    [Fact]
    public async Task NetworkFailure_IsResult()
    {
        using var handler = new ShippingStubHandler((_, _) => throw new HttpRequestException("offline"));
        (await Create(handler).GetTrackingAsync("TRACK")).IsFailure.Should().BeTrue();
    }
}
