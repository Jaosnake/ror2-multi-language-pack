## 1.0.3
- Target compatibility updated to Paladin_Alliance-PaladinMod-2.3.2.
- Complete revision and audit of 85 tokens across all 15 languages.
- Added Esperanto (`eo`) and Latin (`la`).
- Fixed root language dictionary keys in `paladin_FR.language` (`fr` -> `FR`), `paladin_IT.language` (`it` -> `IT`), `paladin_RU.language` (`ru` -> `RU`), and `paladin_UA.language` (`uk` -> `UA`) for R2API.Language detection.
- Fixed 4 legacy unclosed/malformed style and color tags in all 15 files:
  - `PALADIN_DESCRIPTION`: properly closed `<color=#CCD3E0>`.
  - `PALADIN_UTILITY_DASH_DESCRIPTION`: corrected malformed `<style=cIsHealing>` tag to `</style>`.
  - `KEYWORD_SWORDBEAM`: properly closed `<style=cSub>`.
  - `KEYWORD_TORPOR`: properly closed `<style=cSub>`.
- 100% tag balance, placeholder parity, and UTF-8 zero BOM.

## 1.0.2
- Fixed mojibake, accent issues, language tokens, and untranslated English fallbacks.
- Rebuilt the R2API.Language package for manual Thunderstore upload.

## 1.0.1
- Fixed credits that were polluting the interface due to an error during file upload.
