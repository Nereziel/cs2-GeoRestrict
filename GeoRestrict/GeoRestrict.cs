using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Text.Json;
using Nexd.Rest;
using Newtonsoft.Json;
using System.Text.Json.Nodes;

namespace GeoRestrict
{
    public class GeoRestrict : BasePlugin
    {
        public override string ModuleName => "GeoRestrict";
        public override string ModuleAuthor => "Nereziel";
        public override string ModuleDescription => "Allows server owners to block/whitelist players from a country based on IP.";
        public override string ModuleVersion => "1.1";
        private Config cfg = null!;
        private CountryIsAPI api = new();
        public override void Load(bool hotReload)
        {
            cfg = LoadConfig();
            RegisterListener<Listeners.OnClientConnect>(OnClientConnect);
        }

        private void OnClientConnect(int playerSlot, string name, string ipAddress)
        {
            var ip = ipAddress.Split(":");
            var country = api.GetCountryFromIP(ip[0]);
            var countries = cfg.IsoCountries;
            if (cfg.AllowFromUnknown && country == null)
            {
                Log($"Player {name} has connected from Unknown");
                return;
            }
            bool exists = countries!.Any(s => s.Contains(country.Country));
            if (cfg.Whitelist && !exists)
            {
                Server.ExecuteCommand($"kickid {playerSlot} \"{cfg.KickMessage}\"");
                Log($"Player {name} ({country}) has been kicked because he is not from Whitelisted country");
            }
            else if (!cfg.Whitelist && exists)
            {
                Server.ExecuteCommand($"kickid {playerSlot} \"{cfg.KickMessage}\"");
                Log($"Player {name} ({country}) has been kicked because he is from Blacklisted country");
            }
        }

        private static void Log(string message)
        {
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private Config LoadConfig()
        {
            var configPath = Path.Combine(ModuleDirectory, "GeoRestrict.json");

            if (!File.Exists(configPath)) return CreateConfig(configPath);

            var config = System.Text.Json.JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath))!;

            return config;
        }
        private static Config CreateConfig(string configPath)
        {
            var config = new Config
            {
                IsoCountries = new List<string> { "CZ", "SK" },
                Whitelist = true,
                AllowFromUnknown = true,
                KickMessage = "You are not allowed to play on this server from your country.",
            };

            File.WriteAllText(configPath, System.Text.Json.JsonSerializer.Serialize(config));

            return config;
        }
        internal class Config
        {
            public List<string>? IsoCountries { get; set; }
            public bool Whitelist { get; set; }
            public bool AllowFromUnknown { get; set; }
            public string KickMessage { get; set; }
        }
        internal class CountryData : IJsonObject
        {
            [JsonProperty("ip")]
            public string IP;

            [JsonProperty("country")]
            public string Country;
        }

        internal class CountryIsAPI : HttpAPI
        {
            private static readonly string ApiURL = "https://api.country.is";

            public CountryIsAPI() : base(ApiURL)
            { }

            public CountryData? GetCountryFromIP(string ip)
            {
                try
                {
                    return this.SendRequest<CountryData>($"/{ip}");
                }
                catch
                {
                    return null;
                }
            }

            public async Task<CountryData?> GetCountryFromIPAsync(string ip)
            {
                return await this.SendRequestAsync<CountryData>($"/{ip}");
            }
        }
    }
}