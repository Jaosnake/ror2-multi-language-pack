# Custom Language Notes: Ukrainian and Polish

This note exists because not every language key in these packs is shown by the vanilla Risk of Rain 2 language menu.

## What R2API.Language Does

R2API.Language scans `.language` files anywhere under `BepInEx/plugins/`.

If a file contains:

```json
{
  "ua": {
    "TOKEN_NAME": "Translated text"
  }
}
```

R2API can load those tokens without the file crashing, even if the base game does not show that language in the menu.

## What The Vanilla Game Menu Does

Risk of Rain 2 only shows languages that are registered in the game's language list or added by another mod.

That means:

- `ua` / `UA` Ukrainian tokens can load, but Ukrainian will not appear in the normal language menu by default.
- `pl` Polish tokens can load, but Polish may also need a menu/locale enabler depending on the setup.

## Practical Result

These translations are safe to ship as language data, but a player may need an extra mod that adds the language to the selector before they can choose it in-game.

Without a selector/locale enabler:

- R2API can still read the file.
- The tokens exist in memory.
- The player cannot normally select that language from the vanilla menu.

## Why Keep Them

- They are useful for players using custom locale/menu mods.
- They let future packages support the language without reworking the translation files.
- They keep the translation data ready if Risk of Rain 2 or a mod adds the locale later.
