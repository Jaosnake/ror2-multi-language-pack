# PELE - Plugin for Enhanced Language Extension

PELE is a Risk of Rain 2 language extension built on a customized
`R2API.Language`. It keeps normal `.language` loading support and adds PELE JSON
language packs, custom in-game languages, hot reload, and bundled Ukrainian font
support.

## Important: This Replaces R2API.Language

PELE is **not** a second language plugin to install side-by-side with the normal
`R2API.Language.dll`.

This fork intentionally keeps the original BepInEx plugin GUID:

```text
com.bepis.r2api.language
```

That is required so mods that depend on `R2API.Language` keep working while PELE
adds its own behavior on top.

Only one `R2API.Language.dll` must be loaded by BepInEx. The intended target is:

```text
BepInEx/plugins/RiskofThunder-R2API_Language/R2API.Language/R2API.Language.dll
```

Do **not** install another copy in paths like:

```text
BepInEx/plugins/R2API.Language.dll
BepInEx/plugins/PELE/R2API.Language.dll
BepInEx/plugins/SomeOtherFolder/R2API.Language.dll
```

If two DLLs with the same GUID are loaded, BepInEx may pick the wrong plugin,
run duplicate hooks, or initialize language systems in the wrong order.

## Features

- Loads PELE JSON files from `BepInEx/plugins/PELE/Language/<language>/*.json`.
- Gives PELE translations priority over normal game/mod language fallbacks when
  the same token exists in PELE.
- Supports custom language codes:
  - `la` - Latin
  - `eo` - Esperanto
  - `uk` - Ukrainian
- Adds a pause-menu `Language` button.
- Supports keyboard/mouse and controller hints in the language dialog.
- Supports F5 hot reload for `.language` files and PELE JSON files.
- Includes native Cyrillic font support for Ukrainian, without requiring
  `AnotherOneCyrillicFont`.
- Provides optional debug UI through `EnableDebugMenu=true`.

## Translation File Layout

PELE translations live outside the DLL:

```text
BepInEx/plugins/PELE/Language/<language>/*.json
```

The repository stores them here:

```text
PELE/Language/
```

Supported JSON format:

```json
{
  "strings": {
    "TOKEN_NAME": "Translated text"
  }
}
```

Flat JSON token objects are also supported. Metadata keys such as `language`,
`strings`, and keys starting with `_` are ignored as translation tokens.

## Translation Priority

When Risk of Rain 2 asks for a token, PELE resolves it in this order:

1. Temporary overlay tokens.
2. PELE/custom language tokens.
3. The original game or mod language system.

That means PELE wins when it has the same token. Existing mod translation packs
remain useful as fallback data when PELE does not provide a token.

## Configuration

BepInEx generates the config file for this plugin. The release defaults are:

```ini
[PELE]
EnableHotReload = true
EnableDebugMenu = false
EnableVerboseLogging = false
```

- `EnableHotReload`: enables F5 reload and file watcher reload.
- `EnableDebugMenu`: enables the F6 PELE debug window.
- `EnableVerboseLogging`: enables extra hook/layout diagnostic logs.

## Manual Installation

1. Install the normal R2API package set required by your mod profile.
2. Replace the existing `R2API.Language.dll` at:

```text
BepInEx/plugins/RiskofThunder-R2API_Language/R2API.Language/R2API.Language.dll
```

3. Copy the PELE folder to:

```text
BepInEx/plugins/PELE/
```

The `PELE` folder should contain data only:

```text
PELE/
├─ Fonts/
└─ Language/
```

It should **not** contain an old `PELE.dll`, `PELE.deps.json`, or `PELE.pdb`.
Those files belonged to an older standalone PELE plugin and must not be shipped
with this release.

4. Start the game and check `BepInEx/LogOutput.log`.

Expected log markers:

```text
Loading [R2API.Language (Jaosnake fork) 1.0.0]
R2API.Language (Jaosnake fork) inicializado!
PELE/Language encontrado: ...
Tokens PELE por idioma: la=..., eo=..., uk=...
PELE/Fonts/cyrillicfont encontrado.
DLL R2API.Language unica detectada.
PELE JSONs carregados no startup (... tokens)
Hot-Reload habilitado! Pressione F5 para recarregar manualmente.
```

## Thunderstore Package Structure

Thunderstore requires these files at the root of the package zip:

```text
icon.png
README.md
manifest.json
```

This release also includes:

```text
CHANGELOG.md
plugins/
```

The icon is a PNG file at exactly `256x256`.

## Build

Local build command:

```powershell
dotnet build C:\Users\Jaosnake\Desktop\PELE_Project\github_repo_latest\R2API.Language-Source\R2API.Language.csproj -c Release
```

The project deploy target copies the generated DLL to the active r2modman
profile's `RiskofThunder-R2API_Language` folder. This is intentional and keeps
the runtime profile from loading duplicate `R2API.Language.dll` files.

## Release Artifacts

`ReleaseOutput/` contains the files used for the package payload:

```text
ReleaseOutput/
├─ R2API.Language.dll
└─ ukrainianfont
```

`bin/`, `obj/`, `_build/`, and local test zips are not part of the package.

## Technical Documentation

Before changing hooks or UI behavior, read:

```text
docs/HOOKS.md
docs/MANUAL_TESTS.md
docs/STABILIZATION_PLAN.md
```

These files document the hook contracts, manual regression checklist, and
stabilization rules for future PELE changes.
