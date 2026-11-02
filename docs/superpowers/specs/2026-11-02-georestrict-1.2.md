# GeoRestrict 1.2 Design

## Intent
Release GeoRestrict v1.2 as one CounterStrikeSharp plugin instead of separate HTTP and LocalDB variants. The provider is selected in `GeoRestrict.json`, and player-facing kick text uses CounterStrikeSharp's native localization system.

## Goals
- Keep a single `GeoRestrict` plugin and package.
- Support `Provider: "Http"` and `Provider: "LocalDb"`.
- Default missing/unknown provider values to `Http` so existing HTTP configs continue to work.
- Reuse the current whitelist/blacklist, IPv4/IPv6 and unknown-country behavior.
- Use `Localizer` and `lang/<culture>.json` for the kick message.
- Ship English, Czech, Slovak, German and Polish translations.
- Produce one `GeoRestrict.zip` automatically for v1.2 releases.
- Do not bundle `GeoLite2-Country.mmdb`; LocalDb users supply it themselves.

## Non-goals
- No automatic HTTP fallback when `LocalDb` is selected but the database is missing.
- No custom language selector outside CounterStrikeSharp's `!lang` / `css_lang` behavior.
- No change to country policy semantics.

## Architecture
`GeoRestrict` owns both an `HttpClient` and an optional MaxMind `DatabaseReader`. A parsed provider enum routes each lookup to either country.is or the local MaxMind database. Policy decisions remain in `Shared/GeoRestrictionPolicy.cs` so provider parsing and country rules can be unit tested without CounterStrikeSharp runtime dependencies.

CounterStrikeSharp's native `Localizer` resolves `kick.not_allowed`. When a valid player object exists, `Localizer.ForPlayer(...)` chooses that player's culture; otherwise the server culture is used. Translation JSON files are copied to the plugin output by the project file.

The legacy `GeoRestrictLocalDb` project is removed from the solution and Dependabot configuration. README documents the one-plugin installation and the LocalDb migration path.

## Release
`VERSION` contains `1.2.0`. A release workflow triggered by changes to `VERSION` on `main` builds, tests, publishes `GeoRestrict`, packages one ZIP, and creates GitHub release `v1.2` with tag `release-1.2` if that release does not already exist.

## Approaches considered
- Keep two plugins: lowest migration effort, but duplicates code and packaging. Rejected.
- One plugin with `Http`/`LocalDb`: selected; explicit and predictable.
- Add `Auto` fallback: convenient but hides operational failures and makes behavior less predictable. Rejected for v1.2.
