# Ukrainian (UA) Translation — Technical Note

## Why Ukrainian Is Listed Separately

Ukrainian (`UA`) is **not** a language natively supported by Risk of Rain 2.  
The game's built-in language list (in `StreamingAssets/Language/`) only includes:

`en`, `es`, `fr`, `de`, `it`, `ja`, `ko`, `pl`, `pt-BR`, `ru`, `tr`, `zh-CN`, `zh-TW`

## How It Still Works

The `.language` file with key `"ua"` is loaded by **R2API Language** regardless of whether the game officially knows about the locale. R2API scans for any `.language` file in `BepInEx/plugins/` and applies the tokens.

**However**, because Ukrainian is not in the game's native language list:

1. The game's **Settings → Language** menu will **not** show "Українська" as an option.
2. To actually use these translations, you need an additional mod that adds Ukrainian to the language selection UI (e.g. "LanguageUI" mods or a custom mod that registers the locale).
3. Without such a mod, the translations exist in the game's memory but are never selected by the player.

## Compatibility

- ✅ R2API Language loads the file without errors
- ✅ Tokens are registered and can be used by other mods
- ❌ The in-game language switcher does not expose Ukrainian natively
- ❗ Requires a third-party locale enabler to be selectable

## Why Include UA Then?

- The Starstorm 2 language pack already supports UA, and the same user base may want it for Sandswept
- Mod developers can reference `"ua"` tokens programmatically
- Future game updates or mods may add native UA support
