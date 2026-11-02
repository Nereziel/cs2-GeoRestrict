# GeoRestrict 1.2 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ship v1.2 as one localized CounterStrikeSharp plugin with selectable HTTP or LocalDb geolocation and automatic release packaging.

**Architecture:** Merge MaxMind lookup into the existing `GeoRestrict` project and route lookups through a tested provider enum. Use CounterStrikeSharp native localization files for player-facing kick text. Remove the legacy LocalDb project and automate one-ZIP release creation from a `VERSION` bump.

**Tech Stack:** .NET 10, CounterStrikeSharp.API 1.0.379, MaxMind.GeoIP2 6.1.0, xUnit v3, GitHub Actions.

**Spec:** `docs/superpowers/specs/2026-11-02-georestrict-1.2.md`

## Global Constraints
- Release version is `1.2.0`, GitHub release name `v1.2`, tag `release-1.2`.
- One plugin/package only: `GeoRestrict`.
- Providers are `Http` and `LocalDb`; missing or invalid values fall back to `Http`.
- Do not bundle `GeoLite2-Country.mmdb`.
- Use CounterStrikeSharp native localization with `en`, `cs`, `sk`, `de`, `pl`.
- Existing HTTP `GeoRestrict.json` files without `Provider` must continue working.

## Review Focus
- Invalid/mixed-case provider values must not crash and must resolve predictably.
- LocalDb mode with a missing database must follow `AllowFromUnknown` rather than silently switching to HTTP.
- Localization must fall back to server culture if a player object is unavailable.
- Translation files must actually be copied into release output.
- Release automation must not create duplicate `release-1.2` releases on rerun.

---

### Task 1: Provider selection policy

**Files:**
- Modify: `GeoRestrict.Tests/GeoRestrictionPolicyTests.cs`
- Modify: `Shared/GeoRestrictionPolicy.cs`

**Interfaces:**
- Produces: `GeoRestrictionProvider` enum and `GeoRestrictionPolicy.ParseProvider(string?)`.
- Consumed by: Task 2 main plugin routing.

- [x] **Step 1:** Add tests asserting `Http`, case-insensitive `LocalDb`, null, empty and unknown values.
- [x] **Step 2:** Run CI/test and verify the new tests fail because provider support does not exist.
- [x] **Step 3:** Add `GeoRestrictionProvider` and minimal parser implementation.
- [x] **Step 4:** Run the full test suite and verify it passes.

### Task 2: Merge HTTP and LocalDb into one plugin and localize kick text

**Files:**
- Modify: `GeoRestrict/GeoRestrict.cs`
- Modify: `GeoRestrict/GeoRestrict.csproj`
- Create: `GeoRestrict/lang/en.json`
- Create: `GeoRestrict/lang/cs.json`
- Create: `GeoRestrict/lang/sk.json`
- Create: `GeoRestrict/lang/de.json`
- Create: `GeoRestrict/lang/pl.json`

**Interfaces:**
- Consumes: `GeoRestrictionPolicy.ParseProvider` from Task 1.
- Produces: one plugin supporting both providers and native CSS localization.

- [x] **Step 1:** Add MaxMind.GeoIP2 6.1.0 and translation copy rules to the main project.
- [x] **Step 2:** Add `Provider` config defaulting to `Http`; remove configurable `KickMessage` from runtime behavior.
- [x] **Step 3:** Route LocalDb lookups through one persistent `DatabaseReader`; keep HTTP lookups asynchronous.
- [x] **Step 4:** Resolve `kick.not_allowed` with `Localizer.ForPlayer` when possible and server `Localizer` otherwise, then sanitize before `kickid`.
- [x] **Step 5:** Add the five translation JSON files.
- [x] **Step 6:** Build and run all tests; require zero build errors and all tests passing.

### Task 3: Remove legacy project and update repository metadata

**Files:**
- Delete: `GeoRestrictLocalDb/GeoRestrictLocalDb.cs`
- Delete: `GeoRestrictLocalDb/GeoRestrictLocalDb.csproj`
- Modify: `GeoRestrict.sln`
- Modify: `.github/dependabot.yml`
- Modify: `README.md`

**Interfaces:**
- Consumes: single-plugin behavior from Task 2.
- Produces: repository and documentation matching the v1.2 package layout.

- [x] **Step 1:** Remove the LocalDb project from the solution and repository.
- [x] **Step 2:** Remove its Dependabot entry.
- [x] **Step 3:** Rewrite README around one plugin, both providers, native localization and LocalDb migration.
- [x] **Step 4:** Build/test again to verify no stale project references remain.

### Task 4: Release automation

**Files:**
- Create: `VERSION`
- Create: `RELEASE_NOTES.md`
- Create: `.github/workflows/release.yml`
- Modify: `.github/workflows/ci.yml`

**Interfaces:**
- Consumes: one buildable plugin from Tasks 1-3.
- Produces: `GeoRestrict.zip` and GitHub release `v1.2` / `release-1.2` when `VERSION` changes on `main`.

- [x] **Step 1:** Set `VERSION` to `1.2.0` and add v1.2 notes.
- [x] **Step 2:** Add a workflow with `contents: write` that restores, builds, tests, publishes to `package/GeoRestrict`, removes debug symbols, verifies `lang/*.json`, zips the folder, and creates the release only when the tag does not already exist.
- [x] **Step 3:** Ensure CI runs on pull requests and `main` without relying on the old `chore/**` branch pattern.
- [x] **Step 4:** Validate workflow syntax and package upload/download through GitHub checks.

### Task 5: Integration and release

**Files:** none beyond fixes required by verification.

**Interfaces:**
- Consumes: Tasks 1-4.
- Produces: merged v1.2 changes and published GitHub release.

- [x] **Step 1:** Open a PR from `prep/release-1.2` to `main`.
- [x] **Step 2:** Verify the latest PR HEAD has green build/test checks.
- [x] **Step 3:** Review the complete diff for accidental duplicate plugin/project artifacts.
- [ ] **Step 4:** Squash merge after green verification.
- [ ] **Step 5:** Verify the release workflow on the merge commit and confirm release `v1.2` contains exactly one `GeoRestrict.zip` asset.
