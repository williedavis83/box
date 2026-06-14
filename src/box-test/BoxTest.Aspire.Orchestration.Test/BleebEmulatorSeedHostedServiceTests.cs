using System.Net;
using Blabber.Emulator.Options;
using Blabber.Emulator.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class BleebEmulatorSeedHostedServiceTests
{
    [Fact]
    public async Task StartAsync_PostsConfiguredAccountsToEmulator()
    {
        var handler = new RecordingHandler();
        var hostedService = CreateHostedService(
            new BlabberEmulatorOptions
            {
                BleebEmulatorBaseUri = new Uri("http://bleeb-emulator.test/"),
                Accounts =
                [
                    new() { Account = "foo", Bar = "for", Baz = "foz" },
                    new() { Account = "fee", Bar = "fer", Baz = "fez" },
                ],
            },
            handler);

        await hostedService.StartAsync(CancellationToken.None);

        Assert.Equal(2, handler.Requests.Count);
        Assert.All(handler.Requests, request => Assert.Equal(HttpMethod.Post, request.Method));
        Assert.Contains(handler.Requests, request => request.RequestUri?.AbsolutePath == "/account");
    }

    [Fact]
    public async Task StartAsync_SkipsWhenBaseUriMissing()
    {
        var handler = new RecordingHandler();
        var hostedService = CreateHostedService(
            new BlabberEmulatorOptions
            {
                Accounts = [new() { Account = "foo", Bar = "for", Baz = "foz" }],
            },
            handler);

        await hostedService.StartAsync(CancellationToken.None);

        Assert.Empty(handler.Requests);
    }

    private static BleebEmulatorSeedHostedService CreateHostedService(
        BlabberEmulatorOptions options,
        RecordingHandler handler) =>
        new(
            Options.Create(options),
            new StubHttpClientFactory(handler),
            NullLogger<BleebEmulatorSeedHostedService>.Instance);

    private sealed class StubHttpClientFactory(RecordingHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler, disposeHandler: false);
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
        }
    }
}
