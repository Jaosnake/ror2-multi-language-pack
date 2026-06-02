# Starstorm 2 Language Pack

Multi-language translation pack for **TeamMoonstorm-Starstorm2**.

Part of: [Jaosnake/ror2-multi-language-pack](https://github.com/Jaosnake/ror2-multi-language-pack)

> **AI disclosure**: PT-BR was reviewed line by line and manually edited by Jaosnake. Other languages used AI assistance for initial generation and were human-reviewed. Original credits are preserved where applicable.

## Install With r2modman

When this package is available on Thunderstore:

1. Install **TeamMoonstorm-Starstorm2**.
2. Install this language pack.
3. Start the game.
4. Select your language in the Risk of Rain 2 language menu.

## Manual Install

1. Download this repository as ZIP from GitHub.
2. Open the extracted `Starstorm2/` folder.
3. Copy the whole `Starstorm2/` folder into:

```text
BepInEx/plugins/
```

For r2modman, the default profile path is usually:

```text
C:\Users\<your-user>\AppData\Roaming\r2modmanPlus-local\RiskOfRain2\profiles\Default\BepInEx\plugins
```

## Languages

| Language | Folder | Notes |
|---|---|---|
| English | `Language/en/` | Base/reference |
| Portuguese Brazil | `Language/pt-BR/` | Reviewed line by line and manually edited |
| French | `Language/FR/` | Human-reviewed AI-assisted |
| Russian | `Language/RU/` | Human-reviewed AI-assisted |
| Ukrainian | `Language/UA/` | Custom/non-native game language |
| Korean | `Language/ko/` | Human-reviewed AI-assisted |
| Turkish | `Language/tr/` | Human-reviewed AI-assisted |
| Simplified Chinese | `Language/zh-CN/` | Human-reviewed AI-assisted |
| Latin American Spanish | `Language/es-419/` | Human-reviewed AI-assisted |

Each language folder contains 47 JSON files.

## Important Notes

- Achievements and unlocks are intentionally kept in English.
- Ukrainian (`UA`) is not exposed by the vanilla Risk of Rain 2 language selector. It requires a locale/menu enabler or a compatible custom language setup.
- Loading may take slightly longer because the game reads extra language files.

## Compatibility

- Requires `TeamMoonstorm-Starstorm2-0.6.38` or newer.
- Uses the original Starstorm 2 language folder loading system.

## Package Contents

```text
manifest.json
README.md
icon.png
Language/
```

For Thunderstore upload, zip the contents of this folder, not the parent repository.

## Credits

- Original mod: TeamMoonstorm
- PT-BR review/editing and packaging: Jaosnake
- Original credits remain preserved where applicable.
- This package contains translation files only. It does not modify Starstorm 2 source code.

## License

GPL-3.0, matching the original Starstorm 2 licensing context.
