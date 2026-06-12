# Sandswept LookingGlass Localization Patch

This is a small compatibility patch for players who use **Sandswept** together with **LookingGlass** and one of Jaosnake's translation packs.

LookingGlass adds extra stat lines to item and equipment tooltips. Some of those lines are created directly by code instead of coming from normal R2API.Language translation files. Because of that, the main Sandswept language pack can translate the item description correctly, but LookingGlass can still show labels such as `Pool Damage:` or `Pool Shield Damage:` in English.

This package fixes only those extra LookingGlass stat labels for Sandswept.

## What This Patch Does

- Detects Sandswept item and equipment stat definitions registered in LookingGlass.
- Replaces hardcoded English stat labels with the current game language.
- Updates again when the game language changes.
- Leaves normal item names, item descriptions, equipment descriptions, and achievement names untouched.
- Does not modify Sandswept, LookingGlass, or Risk of Rain 2 files on disk.

## When You Need This

Install this package if all of these are true:

- You have Sandswept installed.
- You have LookingGlass installed.
- You are using a non-English language pack and LookingGlass item stats still show English labels.

You do **not** need this package if you do not use LookingGlass.

## Supported Languages

- English
- Brazilian Portuguese
- German
- Spanish
- French
- Italian
- Japanese
- Korean
- Polish
- Russian
- Turkish
- Ukrainian
- Simplified Chinese
- Traditional Chinese

## Manual Installation

1. Install the dependencies listed on the Thunderstore page.
2. Install your Sandswept translation pack.
3. Extract this package into your profile's `BepInEx/plugins` folder.
4. The final path should contain:

```text
BepInEx/plugins/Jaosnake-SandGlass/SandGlass.dll
```

## Configuration

After the first launch, BepInEx can generate a config file:

```text
BepInEx/config/jaosnake.sandglass.cfg
```

Available options:

- `Enable`: turns the patch on or off.
- `LogReplacements`: logs how many labels were changed. This is disabled by default.

## Notes

This is not a replacement for the Sandswept translation pack. It is an optional compatibility patch for LookingGlass.

The main Sandswept language files translate the normal game text. This plugin handles the extra stat labels that LookingGlass builds separately in code.
## Loading Time Note

Installing additional plugins can slightly increase the game's loading time. This is normal.
