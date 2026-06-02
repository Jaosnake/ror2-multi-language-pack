# Sandswept Language Pack

Multi-language translation pack for **SandsweptTeam-Sandswept**.

Part of: [Jaosnake/ror2-multi-language-pack](https://github.com/Jaosnake/ror2-multi-language-pack)

> **AI disclosure**: PT-BR was translated manually/reviewed by Jaosnake. Other languages used AI assistance for initial generation and were human-reviewed.

## Install With r2modman

When this package is available on Thunderstore:

1. Install **SandsweptTeam-Sandswept**.
2. Install this language pack.
3. Start the game.
4. Select your language in the Risk of Rain 2 language menu.

## Manual Install

1. Download this repository as ZIP from GitHub.
2. Open the extracted `Sandswept/` folder.
3. Copy the whole `Sandswept/` folder into:

```text
BepInEx/plugins/
```

R2API.Language will find the `.language` files under `Translations/`.

For r2modman, the default profile path is usually:

```text
C:\Users\<your-user>\AppData\Roaming\r2modmanPlus-local\RiskOfRain2\profiles\Default\BepInEx\plugins
```

## Languages

| Language | R2API key | File |
|---|---:|---|
| English | `en` | `Translations/En-Sandswept.language` |
| Portuguese Brazil | `pt-BR` | `Translations/pt-BR-Sandswept.language` |
| Russian | `RU` | `Translations/Ru-Sandswept.language` |
| German | `de` | `Translations/De-Sandswept.language` |
| Spanish Spain | `es-ES` | `Translations/Es-Sandswept.language` |
| French | `fr` | `Translations/Fr-Sandswept.language` |
| Italian | `it` | `Translations/It-Sandswept.language` |
| Polish | `pl` | `Translations/Pl-Sandswept.language` |
| Japanese | `ja` | `Translations/Ja-Sandswept.language` |
| Korean | `ko` | `Translations/Ko-Sandswept.language` |
| Simplified Chinese | `zh-CN` | `Translations/Zh-CN-Sandswept.language` |
| Traditional Chinese | `zh-TW` | `Translations/Zh-TW-Sandswept.language` |
| Turkish | `tr` | `Translations/Tr-Sandswept.language` |
| Ukrainian | `ua` | `Translations/Ua-Sandswept.language` |

## Important Notes

- Achievements and unlocks are intentionally kept in English.
- Ukrainian (`ua`) is not exposed by the vanilla Risk of Rain 2 language selector. It requires a locale/menu enabler or a compatible custom language setup.
- Polish (`pl`) is included as an R2API language key. Depending on the game/mod setup, it may also require a menu/locale enabler to be selectable.
- Loading may take slightly longer because R2API reads extra `.language` files.

More details: [UA-LANGUAGE-NOTE.md](UA-LANGUAGE-NOTE.md)

## Compatibility

- Requires `SandsweptTeam-Sandswept-1.4.4` or newer.
- Requires R2API.Language. Sandswept already depends on R2API.Language.

## Package Contents

```text
manifest.json
README.md
icon.png
Translations/
```

For Thunderstore upload, zip the contents of this folder, not the parent repository.

## Credits

- Original mod: SandsweptTeam
- Translation and packaging: Jaosnake
- This package contains translation files only. It does not modify Sandswept source code.

## License

GPL-3.0, matching the original Sandswept licensing context.
