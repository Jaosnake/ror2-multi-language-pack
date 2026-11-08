# Changelog

## 1.0.4
- Fixed character select crash (`ArgumentException` duplicate key in `GetDynamicTextDict()`) by removing redundant dictionary section.
- Full compatibility confirmed for all 15 languages including Italian, Portuguese (BR), and P.E.L.E locales.

## 1.0.3
- Added complete Esperanto ('eo') and Latin ('la') translations (34 tokens each).
- Added PELE JSON files for Esperanto, Latin, and Ukrainian.
- Audited and corrected all 15 supported languages, eliminating machine translation flaws ('Over-Wire', 'Roll', 'Monsoon obliteration', etc.).
- Robust language key aliases added ('strings', 'default', 'uk', 'ua', 'fr', 'it', 'ru', 'es', etc.) preventing fallback to English.

## 1.0.2
- Refresh Robomando dynamic skill description coverage and local install layout.
- Fixed mojibake, accent issues, language tokens, and untranslated English fallbacks.
- Rebuilt the R2API.Language package for manual Thunderstore upload.

## 1.0.1
- Fixed credits that were polluting the interface due to an error during file upload.
