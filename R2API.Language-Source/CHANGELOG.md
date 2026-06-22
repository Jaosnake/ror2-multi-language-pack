# Changelog

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
