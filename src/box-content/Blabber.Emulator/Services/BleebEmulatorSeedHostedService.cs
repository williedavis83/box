using System.Net.Http.Json;
using Blabber.Emulator.Models;
using Blabber.Emulator.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blabber.Emulator.Services;

public sealed class BleebEmulatorSeedHostedService(
    IOptions<BlabberEmulatorOptions> options,
    IHttpClientFactory httpClientFactory,
    ILogger<BleebEmulatorSeedHostedService> logger) : IHostedService
{
    public const string HttpClientName = "BleebEmulator";

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (settings.BleebEmulatorBaseUri is null)
        {
            logger.LogInformation("Bleeb emulator seeding skipped because no base URI is configured.");
            return;
        }

        if (settings.Accounts.Count == 0)
        {
            logger.LogInformation("Bleeb emulator seeding skipped because no accounts are configured.");
            return;
        }

        var httpClient = httpClientFactory.CreateClient(HttpClientName);
        httpClient.BaseAddress = settings.BleebEmulatorBaseUri;

        foreach (var account in settings.Accounts)
        {
            if (string.IsNullOrWhiteSpace(account.Account)
                || string.IsNullOrWhiteSpace(account.Bar)
                || string.IsNullOrWhiteSpace(account.Baz))
            {
                logger.LogWarning(
                    "Skipping invalid Bleeb emulator account configuration for '{Account}'.",
                    account.Account);
                continue;
            }

            var response = await httpClient.PostAsJsonAsync(
                "account",
                new UpsertAccountRequest
                {
                    Account = account.Account,
                    Bar = account.Bar,
                    Baz = account.Baz,
                },
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Failed to seed Bleeb emulator account '{account.Account}': {(int)response.StatusCode}.");
            }

            logger.LogInformation(
                "Seeded Bleeb emulator account '{Account}'.",
                account.Account);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
