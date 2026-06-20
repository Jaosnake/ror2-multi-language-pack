# P.E.L.E - Plugin for Enhanced Language Extension

P.E.L.E is a language expansion layer for Risk of Rain 2. It was created because
the base game was never designed around community-made languages like
Ukrainian, Esperanto, Latin, or any future custom language a translation team
may want to add.

Risk of Rain 2 can load many official languages, and many mods already use
`R2API.Language` for normal `.language` files. The problem starts when a
language is not part of the game's original language list, needs extra glyphs,
needs a menu entry, or needs translations from many different mods to behave as
one coherent language pack. Ukrainian exposed that problem first: Cyrillic text
can work, but the game still needs help registering the language, showing it in
the UI, loading the right tokens, and refreshing everything without restarting.

P.E.L.E solves that by replacing the language module with a customized
`R2API.Language` fork that still supports existing mods, while adding a P.E.L.E
translation pipeline on top.

## What P.E.L.E Adds

- Custom language registration for:
  - `uk` - Ukrainian
  - `eo` - Esperanto
  - `la` - Latin
- P.E.L.E JSON translation packs loaded from
  `BepInEx/plugins/PELE/Language/<language>/*.json`.
- Translation priority for P.E.L.E tokens when the same token exists in another
  language pack.
- A main-menu language selector that can show custom languages.
- A pause-menu `Language` button with keyboard, mouse, and controller hints.
- F5 hot reload for P.E.L.E JSON files and normal `.language` files.
- Native Ukrainian/Cyrillic font support, without requiring
  `AnotherOneCyrillicFont`.
- Startup checks for missing folders, missing font data, and duplicate language
  DLLs.

## Why Esperanto and Latin?

Ukrainian is the main practical target: it proves that P.E.L.E can support a
language the base game does not handle cleanly by itself.

Esperanto and Latin are the first experimental custom languages shipped with
P.E.L.E. They are useful test languages because they are not base-game languages,
they force the language menu and loader to behave like a real extension system,
and they give translators a repeatable way to test mod support beyond the
official language list.

In this release, P.E.L.E ships the first three-language custom test set:

```text
Ukrainian + Esperanto + Latin
```

The package includes every P.E.L.E translation file currently shipped in this
repository, even for mods you do not have installed. Risk of Rain 2 only uses
tokens when the matching game content or mod asks for them, so keeping all packs
together is intentional: you can install a supported mod later and its P.E.L.E
translations are already there.

## Important: This Replaces R2API.Language

P.E.L.E is **not** a second language plugin to install next to the normal
`R2API.Language.dll`.

This package is meant to replace the DLL from:

```text
RiskofThunder-R2API_Language
```

P.E.L.E intentionally keeps the original BepInEx plugin GUID:

```text
com.bepis.r2api.language
```

That is required so mods that depend on `R2API.Language` keep working normally.
To BepInEx and to other mods, P.E.L.E is still the language API they expect; it
just has extra language support built in.

Only one `R2API.Language.dll` must be loaded. The intended path is:

```text
BepInEx/plugins/RiskofThunder-R2API_Language/R2API.Language/R2API.Language.dll
```

Do **not** install another copy in paths like:

```text
BepInEx/plugins/R2API.Language.dll
BepInEx/plugins/PELE/R2API.Language.dll
BepInEx/plugins/SomeOtherFolder/R2API.Language.dll
```

If two DLLs with the same GUID are loaded, BepInEx may choose the wrong one,
run duplicate hooks, or initialize language systems in the wrong order.

The `PELE` folder included with this package is data only:

```text
BepInEx/plugins/PELE/
├─ Fonts/
└─ Language/
```

It must not contain an old `PELE.dll`, `PELE.deps.json`, or `PELE.pdb`.

## Dependencies

Thunderstore installs these dependencies automatically:

```text
bbepis-BepInExPack
RiskofThunder-HookGenPatcher
RiskofThunder-R2API_Core
```

`RiskofThunder-R2API_Language` is not listed as a separate dependency because
P.E.L.E provides the replacement `R2API.Language.dll` itself. Installing another
copy of `R2API.Language.dll` side-by-side can cause duplicate plugin loading.

## Included Three-Language Mod Support

The mods below have P.E.L.E translation packs for all three custom languages in
this release: Ukrainian, Esperanto, and Latin.

"Complete P.E.L.E support" here means this package includes matching P.E.L.E JSON
files for `uk`, `eo`, and `la` for that mod. It does not mean the original mod
author officially ships those languages.

This list was checked against the P.E.L.E language files in this GitHub repository,
not against the mods currently installed in a local r2modman profile.

| Mod | Thunderstore | Ukrainian | Esperanto | Latin |
| --- | --- | --- | --- | --- |
| Alloyed Armorer | [TatertotticusSquad-Alloyed_Armorer](https://thunderstore.io/c/riskofrain2/p/TatertotticusSquad/Alloyed_Armorer/) | Yes | Yes | Yes |
| Arsonist | [PopcornFactory-Arsonist_Mod](https://thunderstore.io/c/riskofrain2/p/PopcornFactory/Arsonist_Mod/) | Yes | Yes | Yes |
| Assassin | [HasteReapr-AssassinMod](https://thunderstore.io/c/riskofrain2/p/HasteReapr/AssassinMod/) | Yes | Yes | Yes |
| Banshee | [tsuyoikenko-Banshee](https://thunderstore.io/c/riskofrain2/p/tsuyoikenko/Banshee/) | Yes | Yes | Yes |
| Bastian | [TeamSillyGuy-Bastian](https://thunderstore.io/c/riskofrain2/p/TeamSillyGuy/Bastian/) | Yes | Yes | Yes |
| Belmont | [rob-Belmont](https://thunderstore.io/c/riskofrain2/p/rob/Belmont/) | Yes | Yes | Yes |
| Chaos Angeloid | [DragonycksModdingComms-Chaos](https://thunderstore.io/c/riskofrain2/p/DragonycksModdingComms/Chaos/) | Yes | Yes | Yes |
| Cloudburst | [TeamCloudburst-Cloudburst](https://thunderstore.io/c/riskofrain2/p/TeamCloudburst/Cloudburst/) | Yes | Yes | Yes |
| Dancer | [nayDPz-Dancer](https://thunderstore.io/c/riskofrain2/p/nayDPz/Dancer/) | Yes | Yes | Yes |
| Dante | [rob-Dante](https://thunderstore.io/c/riskofrain2/p/rob/Dante/) | Yes | Yes | Yes |
| Deputy | [Bog-Deputy](https://thunderstore.io/c/riskofrain2/p/Bog/Deputy/) | Yes | Yes | Yes |
| Driver | [public_ParticleSystem-Driver](https://thunderstore.io/c/riskofrain2/p/public_ParticleSystem/Driver/) | Yes | Yes | Yes |
| Enforcer | [EnforcerGang-Enforcer](https://thunderstore.io/c/riskofrain2/p/EnforcerGang/Enforcer/) | Yes | Yes | Yes |
| HAND OVERCLOCKED | [EnforcerGang-HAND_OVERCLOCKED](https://thunderstore.io/c/riskofrain2/p/EnforcerGang/HAND_OVERCLOCKED/) | Yes | Yes | Yes |
| HEL P | [rob-HEL_P](https://thunderstore.io/c/riskofrain2/p/rob/HEL_P/) | Yes | Yes | Yes |
| Henry | [TheTimesweeper-HenryMod](https://thunderstore.io/c/riskofrain2/p/TheTimesweeper/HenryMod/) | Yes | Yes | Yes |
| Heretic | [Moffein-Heretic](https://thunderstore.io/c/riskofrain2/p/Moffein/Heretic/) | Yes | Yes | Yes |
| HUNK | [rob-HUNK](https://thunderstore.io/c/riskofrain2/p/rob/HUNK/) | Yes | Yes | Yes |
| Interrogator | [tsuyoikenko-Interrogator](https://thunderstore.io/c/riskofrain2/p/tsuyoikenko/Interrogator/) | Yes | Yes | Yes |
| Lee Hyperreal | [PopcornFactory-Lee_Hyperreal](https://thunderstore.io/c/riskofrain2/p/PopcornFactory/Lee_Hyperreal/) | Yes | Yes | Yes |
| Miner Unearthed | [EnforcerGang-MinerUnearthed](https://thunderstore.io/c/riskofrain2/p/EnforcerGang/MinerUnearthed/) | Yes | Yes | Yes |
| Mortician | [Bog-Mortician](https://thunderstore.io/c/riskofrain2/p/Bog/Mortician/) | Yes | Yes | Yes |
| Myst | [JavAngle-Myst](https://thunderstore.io/c/riskofrain2/p/JavAngle/Myst/) | Yes | Yes | Yes |
| Paladin | [Paladin_Alliance-PaladinMod](https://thunderstore.io/c/riskofrain2/p/Paladin_Alliance/PaladinMod/) | Yes | Yes | Yes |
| Pathfinder | [Bog-Pathfinder](https://thunderstore.io/c/riskofrain2/p/Bog/Pathfinder/) | Yes | Yes | Yes |
| Pilot | [EnforcerGang-Pilot](https://thunderstore.io/c/riskofrain2/p/EnforcerGang/Pilot/) | Yes | Yes | Yes |
| Ravager | [public_ParticleSystem-Ravager](https://thunderstore.io/c/riskofrain2/p/public_ParticleSystem/Ravager/) | Yes | Yes | Yes |
| Red Alert | [TheTimesweeper-Red_Alert](https://thunderstore.io/c/riskofrain2/p/TheTimesweeper/Red_Alert/) | Yes | Yes | Yes |
| Rifter | [toastyTeam-Rifter](https://thunderstore.io/c/riskofrain2/p/toastyTeam/Rifter/) | Yes | Yes | Yes |
| RiskyTweaks | [Risky_Lives-RiskyTweaks](https://thunderstore.io/c/riskofrain2/p/Risky_Lives/RiskyTweaks/) | Yes | Yes | Yes |
| Rocket | [EnforcerGang-Rocket](https://thunderstore.io/c/riskofrain2/p/EnforcerGang/Rocket/) | Yes | Yes | Yes |
| Sandswept | [SandsweptTeam-Sandswept](https://thunderstore.io/c/riskofrain2/p/SandsweptTeam/Sandswept/) | Yes | Yes | Yes |
| Seamstress | [tsuyoikenko-Seamstress](https://thunderstore.io/c/riskofrain2/p/tsuyoikenko/Seamstress/) | Yes | Yes | Yes |
| Sniper Classic | [EnforcerGang-SniperClassic](https://thunderstore.io/c/riskofrain2/p/EnforcerGang/SniperClassic/) | Yes | Yes | Yes |
| Sorceress | [Frosthex-SorceressMod](https://thunderstore.io/c/riskofrain2/p/Frosthex/SorceressMod/) | Yes | Yes | Yes |
| Spearman | [SaucySquash-Spearman](https://thunderstore.io/c/riskofrain2/p/SaucySquash/Spearman/) | Yes | Yes | Yes |
| Starstorm 2 | [TeamMoonstorm-Starstorm2](https://thunderstore.io/c/riskofrain2/p/TeamMoonstorm/Starstorm2/) | Yes | Yes | Yes |
| Wanderer | [tsuyoikenko-Wanderer](https://thunderstore.io/c/riskofrain2/p/tsuyoikenko/Wanderer/) | Yes | Yes | Yes |

## How Translation Priority Works

When Risk of Rain 2 asks for a token, P.E.L.E resolves it in this order:

1. Temporary overlay tokens.
2. P.E.L.E custom-language tokens.
3. The normal game or mod language system.

That means P.E.L.E wins when it has the same token. Existing mod translation packs
remain useful as fallback data when P.E.L.E does not provide a token.

## Manual Installation

If you install manually, place files like this:

```text
BepInEx/plugins/
├─ RiskofThunder-R2API_Language/
│  └─ R2API.Language/
│     └─ R2API.Language.dll
└─ PELE/
   ├─ Fonts/
   └─ Language/
```

Then start the game and check `BepInEx/LogOutput.log`.

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

## Configuration

BepInEx generates the config file for this plugin. The release defaults are:

```ini
[PELE]
EnableHotReload = true
EnableDebugMenu = false
EnableVerboseLogging = false
```

- `EnableHotReload`: enables F5 reload and file watcher reload.
- `EnableDebugMenu`: enables the F6 P.E.L.E debug window.
- `EnableVerboseLogging`: enables extra hook and layout diagnostic logs.

## Translation File Layout

P.E.L.E translations live outside the DLL:

```text
BepInEx/plugins/PELE/Language/<language>/*.json
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

## Building From Source

Local build command:

```powershell
dotnet build C:\Users\Jaosnake\Desktop\PELE_Project\github_repo_latest\R2API.Language-Source\R2API.Language.csproj -c Release
```

The project deploy target copies the generated DLL to the active r2modman
profile's `RiskofThunder-R2API_Language` folder. This is intentional and keeps
the runtime profile from loading duplicate `R2API.Language.dll` files.

## Technical Documentation

Before changing hooks or UI behavior, read:

```text
docs/HOOKS.md
docs/MANUAL_TESTS.md
docs/STABILIZATION_PLAN.md
```

These files document hook contracts, manual regression testing, and the
stabilization rules for future P.E.L.E changes.
