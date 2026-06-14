using Blabber.Lib;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class BlabberTests
{
    private static readonly Uri BleebBaseUri = new("http://bleeb.test/");

    [Theory]
    [InlineData("foo", "for", "foz")]
    [InlineData("fee", "fer", "fez")]
    public async Task Blabber_ReturnsBleebResponses(string account, string expectedBar, string expectedBaz)
    {
        using var handler = new StubBleebHandler();
        using var httpClient = new HttpClient(handler) { BaseAddress = BleebBaseUri };
        var blabber = new Blabber.Lib.Blabber(account, BleebBaseUri, httpClient);

        Assert.Equal(expectedBar, await blabber.Bar());
        Assert.Equal(expectedBaz, await blabber.Baz());
    }

    [Fact]
    public async Task Blabber_UnknownAccount_Throws()
    {
        using var handler = new StubBleebHandler();
        using var httpClient = new HttpClient(handler) { BaseAddress = BleebBaseUri };
        var blabber = new Blabber.Lib.Blabber("missing", BleebBaseUri, httpClient);

        await Assert.ThrowsAsync<BlabberException>(() => blabber.Bar());
    }

    private sealed class StubBleebHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath.Trim('/') ?? string.Empty;

            var response = path switch
            {
                "bar/foo" => CreateTextResponse("for"),
                "baz/foo" => CreateTextResponse("foz"),
                "bar/fee" => CreateTextResponse("fer"),
                "baz/fee" => CreateTextResponse("fez"),
                _ => new HttpResponseMessage(System.Net.HttpStatusCode.NotFound),
            };

            return Task.FromResult(response);
        }

        private static HttpResponseMessage CreateTextResponse(string value) =>
            new(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(value),
            };
    }
}
