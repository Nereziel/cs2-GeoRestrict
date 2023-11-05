using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using MaxMind.GeoIP2;
using System.Text.Json;

namespace GeoRestrict
{
    public class GeoRestrict : BasePlugin
    {
        public override string ModuleName => "GeoRestrict";
        public override string ModuleAuthor => "Nereziel";
        public override string ModuleDescription => "Allows server owners to block/whitelist players from a country based on IP.";
        public override string ModuleVersion => "1.0";

        private Config cfg = null!;
        public override void Load(bool hotReload)
        {
            cfg = LoadConfig();
            RegisterListener<Listeners.OnClientConnect>(OnClientConnect);
        }

        private void OnClientConnect(int playerSlot, string name, string ipAddress)
        {
            var ip = ipAddress.Split(":");
            var country = CheckCountry(ip[0]);
            var countries = cfg.IsoCountries;
            bool exists = countries!.Any(s => s.Contains(country));
            
            if(cfg.AllowFromUnknown && country == "UNKNOWN")
            {
                Log($"Player {name} has connected from {country}");
                return;
            }
            if (cfg.Whitelist && !exists)
            {
                //TODO: kick reason not whitelisted country
                Server.ExecuteCommand($"kickid {playerSlot}");
                Log($"Player {name} ({country}) has been kicked because he not from Whitelisted country");
            }
            else if(!cfg.Whitelist && exists)
            {
                //TODO: kick reason blacklisted country
                Server.ExecuteCommand($"kickid {playerSlot}");
                Log($"Player {name} ({country}) has been kicked because he is from Blacklisted country");
            }
        }
        private string CheckCountry(string ipAddress)
        {
            var mmdDbFile = Path.Combine(ModuleDirectory, "GeoLite2-Country.mmdb");
            using var reader = new DatabaseReader(mmdDbFile);
            try
            {
                var country = reader.Country(ipAddress);
                return country.Country.IsoCode!;
            }
            catch (Exception)
            {
                return "UNKOWN";
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

            var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath))!;

            return config;
        }
        private static Config CreateConfig(string configPath)
        {
            var config = new Config
            {
                IsoCountries = new List<string> { "CZ", "SK" },
                Whitelist = true,
                AllowFromUnknown = true,
            };

            File.WriteAllText(configPath, JsonSerializer.Serialize(config));

            return config;
        }
        internal class Config
        {
            public List<string>? IsoCountries { get; set; }
            public bool Whitelist { get; set; }
            public bool AllowFromUnknown { get; set; }
        }
    }
}