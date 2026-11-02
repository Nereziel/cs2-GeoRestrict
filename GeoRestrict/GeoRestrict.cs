using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using GeoRestrict.Shared;
using Microsoft.Extensions.Logging;

namespace GeoRestrict;

public sealed class GeoRestrict : BasePlugin
{
    private static readonly JsonSerializerOptions ConfigJsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("https://api.country.is/"),
        Timeout = TimeSpan.FromSeconds(5)
    };

    private Config _config = new();

    public override string ModuleName => "GeoRestrict";
    public override string ModuleAuthor => "Nereziel";
    public override string ModuleDescription => "Allows server owners to block/whitelist players from a country based on IP.";
    public override string ModuleVersion => "1.2.0";

    public override void Load(bool hotReload)
    {
        _config = LoadConfig();
        RegisterListener<Listeners.OnClientConnect>(OnClientConnect);
    }

    public override void Unload(bool hotReload)
    {
        _httpClient.Dispose();
    }

    private void OnClientConnect(int playerSlot, string name, string ipAddress)
    {
        var address = GeoRestrictionPolicy.ParseIpAddress(ipAddress);
        if (address is null)
        {
            ApplyDecision(playerSlot, name, null, null);
            return;
        }

        _ = HandleClientConnectAsync(playerSlot, name, address);
    }

    private async Task HandleClientConnectAsync(int playerSlot, string name, IPAddress address)
    {
        string? countryCode = null;

        try
        {
            var country = await _httpClient
                .GetFromJsonAsync<CountryData>(Uri.EscapeDataString(address.ToString()))
                .ConfigureAwait(false);

            countryCode = country?.Country;
        }
        catch (Exception exception)
        {
            Logger.LogWarning(exception, "Failed to resolve country for {IpAddress}", address);
        }

        ApplyDecision(playerSlot, name, address, countryCode);
    }

    private void ApplyDecision(int playerSlot, string name, IPAddress? expectedAddress, string? countryCode)
    {
        var allowed = GeoRestrictionPolicy.IsAllowed(
            countryCode,
            _config.IsoCountries,
            _config.Whitelist,
            _config.AllowFromUnknown);

        if (allowed)
        {
            Logger.LogInformation(
                "Player {PlayerName} connected from {CountryCode}",
                name,
                countryCode ?? "UNKNOWN");
            return;
        }

        Server.NextFrame(() =>
        {
            if (expectedAddress is not null)
            {
                var player = Utilities.GetPlayerFromSlot(playerSlot);
                var currentAddress = GeoRestrictionPolicy.ParseIpAddress(player?.IpAddress);

                if (player is null || !player.IsValid || currentAddress is null || !currentAddress.Equals(expectedAddress))
                {
                    return;
                }
            }

            var kickMessage = GeoRestrictionPolicy.SanitizeKickMessage(_config.KickMessage);
            Server.ExecuteCommand($"kickid {playerSlot} \"{kickMessage}\"");

            Logger.LogInformation(
                "Player {PlayerName} ({CountryCode}) was kicked by GeoRestrict policy",
                name,
                countryCode ?? "UNKNOWN");
        });
    }

    private Config LoadConfig()
    {
        var configPath = Path.Combine(ModuleDirectory, "GeoRestrict.json");

        if (!File.Exists(configPath))
        {
            return CreateConfig(configPath);
        }

        try
        {
            return JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath), ConfigJsonOptions) ?? new Config();
        }
        catch (JsonException exception)
        {
            Logger.LogError(exception, "GeoRestrict configuration is invalid; using defaults");
            return new Config();
        }
    }

    private static Config CreateConfig(string configPath)
    {
        var config = new Config();
        File.WriteAllText(configPath, JsonSerializer.Serialize(config, ConfigJsonOptions));
        return config;
    }

    internal sealed class Config
    {
        public List<string> IsoCountries { get; set; } = ["CZ", "SK"];
        public bool Whitelist { get; set; } = true;
        public bool AllowFromUnknown { get; set; } = true;
        public string KickMessage { get; set; } = "You are not allowed to play on this server from your country.";
    }

    private sealed class CountryData
    {
        [JsonPropertyName("country")]
        public string? Country { get; init; }
    }
}
