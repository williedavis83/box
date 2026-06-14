using Blabber.Emulator;
using Blabber.Lib;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class FakeBlabberTests
{
    [Fact]
    public async Task BarAndBaz_PrependFakeToRealOutputs()
    {
        var fake = new FakeBlabber(new StubBlabber("for", "foz"));

        Assert.Equal("Fake for", await fake.Bar());
        Assert.Equal("Fake foz", await fake.Baz());
    }

    private sealed class StubBlabber(string bar, string baz) : IBlabber
    {
        public Task<string> Bar(CancellationToken cancellationToken = default) =>
            Task.FromResult(bar);

        public Task<string> Baz(CancellationToken cancellationToken = default) =>
            Task.FromResult(baz);
    }
}
