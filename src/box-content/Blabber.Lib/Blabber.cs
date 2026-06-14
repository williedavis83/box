namespace Blabber.Lib;

public sealed class Blabber : IBlabber
{
    private readonly string _account;
    private readonly HttpClient _httpClient;
    private readonly Uri _bleebBaseUri;

    public Blabber(string account, Uri bleebBaseUri, HttpClient httpClient)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(account);
        ArgumentNullException.ThrowIfNull(bleebBaseUri);
        ArgumentNullException.ThrowIfNull(httpClient);

        _account = account;
        _bleebBaseUri = bleebBaseUri;
        _httpClient = httpClient;
    }

    public Task<string> Bar(CancellationToken cancellationToken = default) =>
        GetEndpointAsync("bar", cancellationToken);

    public Task<string> Baz(CancellationToken cancellationToken = default) =>
        GetEndpointAsync("baz", cancellationToken);

    private async Task<string> GetEndpointAsync(string endpoint, CancellationToken cancellationToken)
    {
        var requestUri = new Uri(_bleebBaseUri, $"{endpoint}/{_account}");

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(requestUri, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new BlabberException($"Failed to reach Bleeb at '{requestUri}'.", ex);
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new BlabberException($"Bleeb account '{_account}' was not found.");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new BlabberException(
                $"Bleeb returned {(int)response.StatusCode} for '{requestUri}'.");
        }

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
