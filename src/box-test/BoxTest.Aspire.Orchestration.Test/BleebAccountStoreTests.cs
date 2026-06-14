using Bleeb.Emulator.Api.Services;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class BleebAccountStoreTests
{
    [Fact]
    public void Upsert_AddsAccountThatCanBeRetrieved()
    {
        var store = new BleebAccountStore();

        store.Upsert("foo", "for", "foz");

        Assert.True(store.TryGet("foo", out var endpoints));
        Assert.Equal("for", endpoints.Bar);
        Assert.Equal("foz", endpoints.Baz);
    }

    [Fact]
    public void Upsert_UpdatesExistingAccount()
    {
        var store = new BleebAccountStore();
        store.Upsert("foo", "for", "foz");

        store.Upsert("foo", "updated-bar", "updated-baz");

        Assert.True(store.TryGet("foo", out var endpoints));
        Assert.Equal("updated-bar", endpoints.Bar);
        Assert.Equal("updated-baz", endpoints.Baz);
    }

    [Fact]
    public void TryGet_IsCaseInsensitive()
    {
        var store = new BleebAccountStore();
        store.Upsert("foo", "for", "foz");

        Assert.True(store.TryGet("FOO", out _));
    }
}
