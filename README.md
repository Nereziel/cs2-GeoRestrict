# cs2-GeoRestrict

Country based access restriction plugin for Counter-Strike 2 servers using [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp).

GeoRestrict can resolve player countries either through [api.country.is](https://api.country.is/) or locally using a [MaxMind GeoLite2 Country](https://dev.maxmind.com/geoip/geolite2-free-geolocation-data) database. The provider is selected in one configuration file.

## Description

- Allows CS2 server owners to whitelist or block countries based on player IP address
- One plugin with selectable `Http` or `LocalDb` geolocation provider
- Supports IPv4 and IPv6
- Uses exact, case-insensitive ISO 3166-1 alpha-2 country codes
- Configurable handling of unknown or unresolved IP addresses
- Native CounterStrikeSharp localization for player-facing kick messages
- Includes English, Czech, Slovak, German and Polish translations
- Requires [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp)
- Built for **.NET 10** and current CounterStrikeSharp releases

## Installation

1. Copy the `GeoRestrict` folder into:
   `addons/counterstrikesharp/plugins/`
2. Start the server once to generate `GeoRestrict.json` if it does not already exist.
3. Edit `GeoRestrict.json` as needed.

### HTTP provider

`Http` is the default provider and requires outbound HTTPS access to:

`https://api.country.is/`

No additional database file is required.

### LocalDb provider

1. Download the latest `GeoLite2-Country.mmdb` from MaxMind.
2. Put the file directly into the `GeoRestrict` plugin directory.
3. Set `"Provider": "LocalDb"` in `GeoRestrict.json`.
4. Reload the plugin or restart the server.

`GeoLite2-Country.mmdb` is **not included** in the release ZIP. GeoRestrict does not silently fall back to HTTP when `LocalDb` is selected and the database is missing; the lookup is treated as unknown and `AllowFromUnknown` decides whether the player is allowed.

## Updating the IP database

Only applies when using `Provider: "LocalDb"`.

Download the newest `GeoLite2-Country.mmdb` from MaxMind, replace the existing file in the `GeoRestrict` plugin directory, and reload the plugin or restart the server.

## Configuration

- `Provider` - `Http` or `LocalDb`; defaults to `Http`
- `IsoCountries` - ISO 3166-1 alpha-2 country codes, for example `US`, `DE`, `CZ` ([country code list](https://en.wikipedia.org/wiki/ISO_3166-1_alpha-2))
- `Whitelist` - `true` = only listed countries are allowed
- `Whitelist` - `false` = listed countries are blocked
- `AllowFromUnknown` - allow or reject players whose country cannot be detected

Example using the HTTP provider:

```json
{
  "Provider": "Http",
  "IsoCountries": ["CZ", "SK"],
  "Whitelist": true,
  "AllowFromUnknown": true
}
```

Example using the local MaxMind database:

```json
{
  "Provider": "LocalDb",
  "IsoCountries": ["CZ", "SK"],
  "Whitelist": true,
  "AllowFromUnknown": true
}
```

Provider names are case-insensitive. Missing or invalid provider values fall back to `Http`.

## Localization

GeoRestrict uses CounterStrikeSharp's native localization system. Included languages:

- `en` - English
- `cs` - Czech
- `sk` - Slovak
- `de` - German
- `pl` - Polish

Players can use CounterStrikeSharp's `!lang` or `css_lang` command to select their language. If the player language is not available, CounterStrikeSharp falls back to the server language.

The kick message is stored under the `kick.not_allowed` key in the files inside `GeoRestrict/lang/`. You can edit or add translation files using the normal CounterStrikeSharp localization format.

## Upgrading from v1.1

### Existing GeoRestrict / HTTP users

Existing `GeoRestrict.json` files continue to work. If `Provider` is not present, GeoRestrict uses `Http`.

The old `KickMessage` setting is no longer used; the message now comes from the CounterStrikeSharp localization files.

### Existing GeoRestrictLocalDb users

The separate `GeoRestrictLocalDb` plugin has been merged into `GeoRestrict`.

1. Remove the old `GeoRestrictLocalDb` plugin folder.
2. Install the new `GeoRestrict` plugin.
3. Copy your country list, `Whitelist` and `AllowFromUnknown` settings into `GeoRestrict.json`.
4. Add `"Provider": "LocalDb"`.
5. Move or download `GeoLite2-Country.mmdb` into the new `GeoRestrict` plugin directory.

## Notes

- HTTP lookups run asynchronously and do not block the game thread while waiting for the API.
- LocalDb keeps one MaxMind database reader open while the plugin is loaded.
- NuGet and GitHub Actions dependencies are monitored by Dependabot.
