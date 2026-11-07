# Changelog

## 1.0.5 - DLC4 Seekers of the Storm Localization and Quality Audit

- Complete localization of DLC4 (Seekers of the Storm) for Ukrainian (uk), Esperanto (eo), and Latin (la), covering all 903 keys (2,709 localized tokens).
- Full audit and quality review of all new survivors (Seeker, Chef, False Son), skills, items, equipment, monsters, bosses, and epistolary lore.
- Harmonized stage names, subtitles, and environment logs with official Risk of Rain 2 canon standards.
- Corrected TextMeshPro styling tags and repaired formatting discrepancies from base game files.
- Zero numerical discrepancies and 100% placeholder compatibility across all three languages.

## 1.0.4 - Documentation and translation update

- Update SniperClassic Spotter descriptions and other verified text corrections in Ukrainian, Esperanto and Latin.

- Synchronize SniperClassic translations for Ukrainian, Esperanto and Latin.

- Document separately installed compatible packs for Caeli Imperium, Celestial War Tank, Ganondorf and The House.

- Synchronize Sandswept translations for Ukrainian, Esperanto and Latin.

- Synchronize Paladin translations for Ukrainian, Esperanto and Latin.

- Synchronize MinerUnearthed translations for Ukrainian, Esperanto and Latin.

- Synchronize Wanderer translations for Ukrainian, Esperanto and Latin.

- Synchronize Spearman translations for Ukrainian, Esperanto and Latin.

- Add Goku, Vegeta and Trunks to the list of included three-language mod support.
- Link directly to the three character translation packs in the KingModUtilities integration section.
- Synchronize Belmont, Dante, Driver, Enforcer and Seamstress translations for Ukrainian, Esperanto and Latin.

## 1.0.3 - Loader and optional mod integration

- Repair the separate KingModUtilities integration, including translated tutorials, colored highlights and transformation menu labels.

- Recognize P.E.L.E language files and the Cyrillic font in mod-manager installations as well as the direct folder layout.
- Resolve Ukrainian `UA` and `uk` language codes as aliases while preserving exact-code priority.
- Include an optional KingModUtilities integration for Goku, Vegeta, and Trunks, plus associated three-language files.
- Update included translation files and document current coverage limitations.

## 1.0.2 - README cleanup

- TOO MUCH STUFF IN README LOL GO TO WIKI.

## 1.0.1 - Package metadata update

- Updates the Thunderstore package description to clearly mention native Ukrainian, Esperanto, and Latin support.
- Keeps the internal BepInEx plugin version at `2.0.0` so P.E.L.E continues replacing `RiskofThunder-R2API_Language` safely.

## 1.0.0 - P.E.L.E initial release

- Ships as a replacement for `RiskofThunder-R2API_Language`'s `R2API.Language.dll`, not as a second side-by-side plugin.
- Uses internal BepInEx plugin version `2.0.0` so the fork wins over upstream `R2API.Language 1.1.0` during manual side-by-side r2modman tests.
- Adds P.E.L.E JSON language loading from `BepInEx/plugins/PELE/Language/<lang>/*.json`.
- Gives P.E.L.E translations priority over normal mod/game language fallbacks when a token exists in P.E.L.E.
- Adds custom language support for Latin (`la`), Esperanto (`eo`), and Ukrainian (`uk`).
- Adds native Cyrillic font support for Ukrainian without requiring `AnotherOneCyrillicFont`.
- Adds a pause-menu `Language` button with controller/keyboard hints.
- Adds F5 hot reload for `.language` and P.E.L.E JSON files.
- Adds startup sanity checks for P.E.L.E folders, icons, language manifests, font bundle, and duplicate DLLs.
- Keeps the F6 debug window available behind `EnableDebugMenu=false` by default for release builds.
- Keeps verbose hook/layout logs behind `EnableVerboseLogging=false` by default.
