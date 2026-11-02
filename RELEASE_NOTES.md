## GeoRestrict v1.2

- Updated to .NET 10 and CounterStrikeSharp.API 1.0.379.
- Merged the HTTP and MaxMind LocalDb variants into one `GeoRestrict` plugin.
- Added `Provider: "Http"` and `Provider: "LocalDb"` configuration.
- Added IPv4 and IPv6 address handling and exact case-insensitive ISO country matching.
- HTTP geolocation lookups are asynchronous and no longer block the game thread.
- LocalDb reuses one MaxMind database reader while the plugin is loaded.
- Added native CounterStrikeSharp localization with English, Czech, Slovak, German and Polish translations.
- Added automated tests, CI, release packaging and Dependabot dependency monitoring.

### Upgrade notes

Existing HTTP `GeoRestrict.json` configurations continue to work and default to the `Http` provider when `Provider` is missing.

The separate `GeoRestrictLocalDb` plugin has been removed. LocalDb users should install `GeoRestrict`, set `"Provider": "LocalDb"`, and place `GeoLite2-Country.mmdb` in the `GeoRestrict` plugin directory.

`KickMessage` is no longer read from the configuration file. The kick message is now provided through CounterStrikeSharp localization files under `lang/`.

The release contains one `GeoRestrict.zip`. `GeoLite2-Country.mmdb` and CounterStrikeSharp itself are not bundled.
