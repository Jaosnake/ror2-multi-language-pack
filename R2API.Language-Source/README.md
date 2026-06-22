# P.E.L.E - Plugin for Enhanced Language Extension

P.E.L.E expands Risk of Rain 2 language support beyond the game's built-in
language list. It adds native support for Ukrainian, Esperanto, and Latin while
remaining compatible with mods that already depend on `R2API.Language`.

It was created because custom community languages need more than translated
text files: they need menu entries, font support, reliable token loading, and a
way to keep many mod translations working as one coherent language pack.

## What P.E.L.E Adds

- Native custom language support for Ukrainian (`uk`), Esperanto (`eo`), and
  Latin (`la`).
- P.E.L.E JSON translation packs for the base game and supported mods.
- Priority loading for P.E.L.E translations when another pack has the same
  token.
- Main-menu and pause-menu language selection.
- Keyboard, mouse, and controller hints in the language menu.
- F5 hot reload for P.E.L.E JSON files and normal `.language` files.
- Built-in Ukrainian/Cyrillic font support.
- Startup checks for missing folders, missing font data, and duplicate language
  DLLs.

## Screenshots

Main menu language selector:

![P.E.L.E main menu language selector](https://raw.githubusercontent.com/Jaosnake/ror2-multi-language-pack/main/R2API.Language-Source/docs/screenshots/main-menu-esperanto.png)

Pause-menu language dialog:

![P.E.L.E pause language dialog](https://raw.githubusercontent.com/Jaosnake/ror2-multi-language-pack/main/R2API.Language-Source/docs/screenshots/pause-language-menu.png)

Character select in Esperanto:

![P.E.L.E character select in Esperanto](https://raw.githubusercontent.com/Jaosnake/ror2-multi-language-pack/main/R2API.Language-Source/docs/screenshots/character-select-esperanto.png)

Character select in Ukrainian:

![P.E.L.E character select in Ukrainian](https://raw.githubusercontent.com/Jaosnake/ror2-multi-language-pack/main/R2API.Language-Source/docs/screenshots/character-select-ukrainian.png)

## Why These Languages?

Ukrainian is the main practical target because it needs clean custom-language
registration and Cyrillic font support. Esperanto and Latin are the first
experimental custom languages shipped with P.E.L.E, useful for testing the
loader, menus, and mod translation support outside the official language list.

This package includes every P.E.L.E translation file currently shipped in the
repository, even for mods you do not have installed. Risk of Rain 2 only uses a
token when matching game or mod content asks for it.

## Important

P.E.L.E replaces the `R2API.Language.dll` provided by
`RiskofThunder-R2API_Language`. It keeps the same BepInEx plugin GUID so mods
that depend on `R2API.Language` continue to work normally.

Do not install another copy of `R2API.Language.dll` side by side. Duplicate
language DLLs can cause duplicate hooks, wrong load order, or missing language
tokens.

## Dependencies

Thunderstore installs these automatically:

```text
bbepis-BepInExPack-5.4.2121
RiskofThunder-HookGenPatcher-1.2.9
RiskofThunder-R2API_Core-5.3.0
```

`RiskofThunder-R2API_Language` is not listed as a dependency because P.E.L.E
provides the replacement `R2API.Language.dll` itself.

## Translation Priority

When P.E.L.E provides a token, it wins over other language packs. If P.E.L.E
does not provide that token, the normal game/mod language fallback is used.

## Included Three-Language Mod Support

The mods below have P.E.L.E translation packs for all three custom languages in
this release: Ukrainian, Esperanto, and Latin.

"Complete P.E.L.E support" here means this package includes matching P.E.L.E JSON
files for `uk`, `eo`, and `la` for that mod. It does not mean the original mod
author officially ships those languages.

This list was checked against the P.E.L.E language files in this GitHub
repository, not against the mods currently installed in a local r2modman
profile.

All mods listed below include P.E.L.E JSON support for:

```text
Ukrainian (uk) | Esperanto (eo) | Latin (la)
```

| Mod | Thunderstore package |
| --- | --- |
| Alloyed Armorer | [TatertotticusSquad / Alloyed_Armorer](https://thunderstore.io/c/riskofrain2/p/TatertotticusSquad/Alloyed_Armorer/) |
| Arsonist | [PopcornFactory / Arsonist_Mod](https://thunderstore.io/c/riskofrain2/p/PopcornFactory/Arsonist_Mod/) |
| Assassin | [HasteReapr / AssassinMod](https://thunderstore.io/c/riskofrain2/p/HasteReapr/AssassinMod/) |
| Banshee | [tsuyoikenko / Banshee](https://thunderstore.io/c/riskofrain2/p/tsuyoikenko/Banshee/) |
| Bastian | [TeamSillyGuy / Bastian](https://thunderstore.io/c/riskofrain2/p/TeamSillyGuy/Bastian/) |
| Belmont | [rob / Belmont](https://thunderstore.io/c/riskofrain2/p/rob/Belmont/) |
| Chaos Angeloid | [DragonycksModdingComms / Chaos](https://thunderstore.io/c/riskofrain2/p/DragonycksModdingComms/Chaos/) |
| Cloudburst | [TeamCloudburst / Cloudburst](https://thunderstore.io/c/riskofrain2/p/TeamCloudburst/Cloudburst/) |
| Dancer | [nayDPz / Dancer](https://thunderstore.io/c/riskofrain2/p/nayDPz/Dancer/) |
| Dante | [rob / Dante](https://thunderstore.io/c/riskofrain2/p/rob/Dante/) |
| Deputy | [Bog / Deputy](https://thunderstore.io/c/riskofrain2/p/Bog/Deputy/) |
| Driver | [public_ParticleSystem / Driver](https://thunderstore.io/c/riskofrain2/p/public_ParticleSystem/Driver/) |
| Enforcer | [EnforcerGang / Enforcer](https://thunderstore.io/c/riskofrain2/p/EnforcerGang/Enforcer/) |
| HAND OVERCLOCKED | [EnforcerGang / HAND_OVERCLOCKED](https://thunderstore.io/c/riskofrain2/p/EnforcerGang/HAND_OVERCLOCKED/) |
| HEL P | [rob / HEL_P](https://thunderstore.io/c/riskofrain2/p/rob/HEL_P/) |
| Henry | [TheTimesweeper / HenryMod](https://thunderstore.io/c/riskofrain2/p/TheTimesweeper/HenryMod/) |
| Heretic | [Moffein / Heretic](https://thunderstore.io/c/riskofrain2/p/Moffein/Heretic/) |
| HUNK | [rob / HUNK](https://thunderstore.io/c/riskofrain2/p/rob/HUNK/) |
| Interrogator | [tsuyoikenko / Interrogator](https://thunderstore.io/c/riskofrain2/p/tsuyoikenko/Interrogator/) |
| Lee Hyperreal | [PopcornFactory / Lee_Hyperreal](https://thunderstore.io/c/riskofrain2/p/PopcornFactory/Lee_Hyperreal/) |
| Miner Unearthed | [EnforcerGang / MinerUnearthed](https://thunderstore.io/c/riskofrain2/p/EnforcerGang/MinerUnearthed/) |
| Mortician | [Bog / Mortician](https://thunderstore.io/c/riskofrain2/p/Bog/Mortician/) |
| Myst | [JavAngle / Myst](https://thunderstore.io/c/riskofrain2/p/JavAngle/Myst/) |
| Paladin | [Paladin_Alliance / PaladinMod](https://thunderstore.io/c/riskofrain2/p/Paladin_Alliance/PaladinMod/) |
| Pathfinder | [Bog / Pathfinder](https://thunderstore.io/c/riskofrain2/p/Bog/Pathfinder/) |
| Pilot | [EnforcerGang / Pilot](https://thunderstore.io/c/riskofrain2/p/EnforcerGang/Pilot/) |
| Ravager | [public_ParticleSystem / Ravager](https://thunderstore.io/c/riskofrain2/p/public_ParticleSystem/Ravager/) |
| Red Alert | [TheTimesweeper / Red_Alert](https://thunderstore.io/c/riskofrain2/p/TheTimesweeper/Red_Alert/) |
| Rifter | [toastyTeam / Rifter](https://thunderstore.io/c/riskofrain2/p/toastyTeam/Rifter/) |
| RiskyTweaks | [Risky_Lives / RiskyTweaks](https://thunderstore.io/c/riskofrain2/p/Risky_Lives/RiskyTweaks/) |
| Rocket | [EnforcerGang / Rocket](https://thunderstore.io/c/riskofrain2/p/EnforcerGang/Rocket/) |
| Sandswept | [SandsweptTeam / Sandswept](https://thunderstore.io/c/riskofrain2/p/SandsweptTeam/Sandswept/) |
| Seamstress | [tsuyoikenko / Seamstress](https://thunderstore.io/c/riskofrain2/p/tsuyoikenko/Seamstress/) |
| Sniper Classic | [EnforcerGang / SniperClassic](https://thunderstore.io/c/riskofrain2/p/EnforcerGang/SniperClassic/) |
| Sorceress | [Frosthex / SorceressMod](https://thunderstore.io/c/riskofrain2/p/Frosthex/SorceressMod/) |
| Spearman | [SaucySquash / Spearman](https://thunderstore.io/c/riskofrain2/p/SaucySquash/Spearman/) |
| Starstorm 2 | [TeamMoonstorm / Starstorm2](https://thunderstore.io/c/riskofrain2/p/TeamMoonstorm/Starstorm2/) |
| Wanderer | [tsuyoikenko / Wanderer](https://thunderstore.io/c/riskofrain2/p/tsuyoikenko/Wanderer/) |


## More Jaosnake Translations

P.E.L.E is designed to work alongside my existing Risk of Rain 2 translation
packs. Those packs remain fully supported and are still recommended for the
other languages they already cover.

[Jaosnake packages on Thunderstore](https://thunderstore.io/c/riskofrain2/p/Jaosnake/)

## Documentation

More technical details are available here:

- [Installation and compatibility](https://github.com/Jaosnake/ror2-multi-language-pack/blob/main/R2API.Language-Source/docs/WIKI.md#important-this-replaces-r2apilanguage)
- [Manual installation](https://github.com/Jaosnake/ror2-multi-language-pack/blob/main/R2API.Language-Source/docs/WIKI.md#manual-installation)
- [Configuration](https://github.com/Jaosnake/ror2-multi-language-pack/blob/main/R2API.Language-Source/docs/WIKI.md#configuration)
- [Translation file layout](https://github.com/Jaosnake/ror2-multi-language-pack/blob/main/R2API.Language-Source/docs/WIKI.md#translation-file-layout)
- [Technical hook notes](https://github.com/Jaosnake/ror2-multi-language-pack/blob/main/R2API.Language-Source/docs/HOOKS.md)
- [Manual test checklist](https://github.com/Jaosnake/ror2-multi-language-pack/blob/main/R2API.Language-Source/docs/MANUAL_TESTS.md)

## Questions and Support

For questions, suggestions, bug reports, or translation feedback, please open an
issue on GitHub:

[Jaosnake/ror2-multi-language-pack](https://github.com/Jaosnake/ror2-multi-language-pack)

---

## Support

[![ko-fi](https://img.shields.io/badge/Ko--fi-Support%20Me-%23FF5E5B?logo=ko-fi)](https://ko-fi.com/jaosnake)

