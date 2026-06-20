# Changelog

## 1.0.0 - PELE initial release

- Adds PELE JSON language loading from `BepInEx/plugins/PELE/Language/<lang>/*.json`.
- Gives PELE translations priority over normal mod/game language fallbacks when a token exists in PELE.
- Adds custom language support for Latin (`la`), Esperanto (`eo`), and Ukrainian (`uk`).
- Adds native Cyrillic font support for Ukrainian without requiring `AnotherOneCyrillicFont`.
- Adds a pause-menu `Language` button with controller/keyboard hints.
- Adds F5 hot reload for `.language` and PELE JSON files.
- Adds startup sanity checks for PELE folders, icons, language manifests, font bundle, and duplicate DLLs.
- Keeps the F6 debug window available behind `EnableDebugMenu=false` by default for release builds.
- Keeps verbose hook/layout logs behind `EnableVerboseLogging=false` by default.
