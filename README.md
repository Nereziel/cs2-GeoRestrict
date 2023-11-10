# cs2-GeoRestrict
GeoRestrict is Http based on https://api.country.is/
GeoRestrictLocalDb is based on maxminds database file

## Description:
- Allows CS2 server owners to block/whitelist players from a country based on IP
- Requires [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp) Metamod plugin installed on your cs2 server
- GeoRestrictLocalDb requires [GeoLite2-Country.mmdb](https://www.maxmind.com/en/home) which can be downloaded at official site after registration (included in zip)

## Installation:
- put folder GeoRestrict of GeoRestrictLocalDb into `addons/counterstrikesharp/plugins`
- if using GeoRestrictLocalDb put `GeoLite2-Country.mmdb` into `addons/counterstrikesharp/plugins/GeoRestrict`
- edit `GeoRestrict.json` or `GeoRestrictLocalDb.json` as you need

## Update IP database only for GeoRestrictLocalDb:
- Download newest `GeoLite2-Country.mmdb` and replace the old one

## Configuration:
- in plugin directory `GeoRestrict.json` or `GeoRestrictLocalDb.json`\
`IsoCountries` - supports Alpha-2 Counry codes for ex. US, DE, CZ [(More on Wikipedia)](https://en.wikipedia.org/wiki/List_of_ISO_3166_country_codes)\
`Whitelist` - true = which countries are allowed\
`Whitelist` - false = which countries are not allowed\
`AllowFromUnknown` - If country from IP is not detected, allow them od disallow.

- GeoRestrict.json example
```json
{
	"IsoCountries":["CZ","SK"], 
	"Whitelist":true,
	"AllowFromUnknown":true,
	"KickMessage":"You are not allowed to play on this server from your country."
}
```
