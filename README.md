# cs2-GeoRestrict

Country based access restriction plugin for Counter-Strike 2 servers using [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp).

Two variants are available:
- **GeoRestrict** - resolves player countries through [api.country.is](https://api.country.is/)
- **GeoRestrictLocalDb** - resolves player countries locally using a [MaxMind GeoLite2 Country](https://dev.maxmind.com/geoip/geolite2-free-geolocation-data) database

## Description

- Allows CS2 server owners to whitelist or block countries based on player IP address
- Supports IPv4 and IPv6
- Uses exact, case-insensitive ISO 3166-1 alpha-2 country codes
- Configurable handling of unknown or unresolved IP addresses
- Configurable kick message
- Requires [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp)
- Built for **.NET 10** and current CounterStrikeSharp releases

## Installation

### GeoRestrict

1. Copy the `GeoRestrict` plugin folder into:
   `addons/counterstrikesharp/plugins/`
2. Start the server once to generate `GeoRestrict.json` if it does not already exist.
3. Edit `GeoRestrict.json` as needed.

GeoRestrict requires outbound HTTPS access to `https://api.country.is/`.

### GeoRestrictLocalDb

1. Copy the `GeoRestrictLocalDb` plugin folder into:
   `addons/counterstrikesharp/plugins/`
2. Download the latest `GeoLite2-Country.mmdb` from MaxMind.
3. Put `GeoLite2-Country.mmdb` into the `GeoRestrictLocalDb` plugin directory.
4. Start the server once to generate `GeoRestrictLocalDb.json` if it does not already exist.
5. Edit `GeoRestrictLocalDb.json` as needed.

## Updating the IP database

Only applies to **GeoRestrictLocalDb**.

Download the newest `GeoLite2-Country.mmdb` from MaxMind and replace the existing database file in the plugin directory. Restart or reload the plugin afterwards.

## Configuration

Both variants use the same configuration options:

- `IsoCountries` - ISO 3166-1 alpha-2 country codes, for example `US`, `DE`, `CZ` ([country code list](https://en.wikipedia.org/wiki/ISO_3166-1_alpha-2))
- `Whitelist` - `true` = only listed countries are allowed
- `Whitelist` - `false` = listed countries are blocked
- `AllowFromUnknown` - allow or reject players whose country cannot be detected
- `KickMessage` - message shown when a player is rejected

Example:

```json
{
  "IsoCountries": ["CZ", "SK"],
  "Whitelist": true,
  "AllowFromUnknown": true,
  "KickMessage": "You are not allowed to play on this server from your country."
}
```

## Notes

- The HTTP version performs country lookups asynchronously and does not block the game thread while waiting for the API.
- The LocalDB version keeps the MaxMind database open while the plugin is loaded instead of reopening it for every connection.
- Existing `GeoRestrict.json` and `GeoRestrictLocalDb.json` configurations remain compatible with v2.
- NuGet and GitHub Actions dependencies are monitored by Dependabot.
