using System.Net;
using System.Text.Json;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using GeoRestrict.Shared;
using MaxMind.GeoIP2;
using Microsoft.Extensions.Logging;

namespace GeoRestrictLocalDb;

public sealed class GeoRestrictLocalDb : BasePlugin
{
    private static readonly JsonSerializerOptions ConfigJsonOptions = new()
    {
        WriteIndented = true
    };

    private Config _config = new();
    private DatabaseReader? _databaseReader;

    public override string ModuleName => "GeoRestrict-LocalDB";
    public override string ModuleAuthor => "Nereziel";
    public override string ModuleDescription => "Allows server owners to block/whitelist players from a country based on IP. Requires a MaxMind GeoLite2 Country database.";
    public override string ModuleVersion => "1.2.0";

    public override void Load(bool hotReload)
    {
        _config = LoadConfig();

        var databasePath = Path.Combine(ModuleDirectory, "GeoLite2-Country.mmdb");
        if (File.Exists(databasePath))
        {
            _databaseReader = new DatabaseReader(databasePath);
        }
        else
        {
            Logger.LogError("MaxMind database not found at {DatabasePath}", databasePath);
        }

        RegisterListener<Listeners.OnClientConnect>(OnClientConnect);
    }

    public override void Unload(bool hotReload)
    {
        _databaseReader?.Dispose();
        _databaseReader = null;
    }

    private void OnClientConnect(int playerSlot, string name, string ipAddress)
    {
        var address = GeoRestrictionPolicy.ParseIpAddress(ipAddress);
        var countryCode = address is null ? null : GetCountryCode(address);

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

        KickPlayer(playerSlot, name, address, countryCode);
    }

    private string? GetCountryCode(IPAddress address)
    {
        if (_databaseReader is null)
        {
            return null;
        }

        try
        {
            return _databaseReader.Country(address.ToString()).Country.IsoCode;
        }
        catch (Exception exception)
        {
            Logger.LogDebug(exception, "No MaxMind country match for {IpAddress}", address);
            return null;
        }
    }

    private void KickPlayer(int playerSlot, string name, IPAddress? expectedAddress, string? countryCode)
    {
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
        var configPath = Path.Combine(ModuleDirectory, "GeoRestrictLocalDb.json");

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
            Logger.LogError(exception, "GeoRestrictLocalDb configuration is invalid; using defaults");
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
}
