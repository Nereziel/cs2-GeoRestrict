# cs2-GeoRestrict

## Description:
- Allows CS2 server owners to block/whitelist players from a country based on IP
- Requires [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp) Metamod plugin installed on your cs2 server

## Installation:
- put folder GeoRestrict into `addons/counterstrikesharp/plugins`
- edit GeoRestrict.json as you need

## Configuration:
- in plugin directory `GeoRestrict.json`\
`IsoCountries` - supports Alpha-2 Counry codes for ex. US, DE, CZ [(More on Wikipedia)](https://en.wikipedia.org/wiki/List_of_ISO_3166_country_codes)\
`Whitelist` - true = which countries are allowed\
`Whitelist` - false = which countries are not allowed\
`AllowFromUnknown` - If country from IP is not detected, allow them od disallow.

- GeoRestrict.json example
```json
{
	"IsoCountries":["CZ","SK"], 
	"Whitelist":true,
	"AllowFromUnknown":true
}
```
