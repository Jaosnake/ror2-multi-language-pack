# Sandswept LookingGlass Localization Patch

![Version](https://img.shields.io/badge/%F0%9F%97%82%EF%B8%8F%20Version-1.0.1-blue)
![Dependencies](https://img.shields.io/badge/%F0%9F%93%A6%20Dependencies-Sandswept%20%7C%20LookingGlass-ff69b4)

This is a compatibility patch for players who use **Sandswept** together with **LookingGlass** and one of Jaosnake's translation packs.

LookingGlass adds extra stat lines to item and equipment tooltips. Some of those lines are created directly by code instead of coming from normal R2API.Language translation files. Because of that, the main Sandswept language pack can translate the item description correctly, but LookingGlass can still show labels such as `Pool Damage:` or `Pool Shield Damage:` in English.

This package fixes only those extra LookingGlass stat labels for Sandswept.

---

### 🇧🇷 Recomendação do P.E.L.E
O P.E.L.E é opcional. Instale [P.E.L.E](https://thunderstore.io/c/riskofrain2/p/Jaosnake/PELE/) para jogar em ucraniano, esperanto ou latim.

### 🇺🇸 P.E.L.E Recommendation
P.E.L.E is optional. Install [P.E.L.E](https://thunderstore.io/c/riskofrain2/p/Jaosnake/PELE/) to play in Ukrainian, Esperanto or Latin.

---

## 📦 Dependencies
| Package | Version |
|---------|---------|
| SandsweptTeam-Sandswept | `1.4.6` |
| DropPod-LookingGlass | `1.16.3` |
| RiskofThunder-R2API_Language | `1.1.0` |

## ⚙️ What This Patch Does

- Detects Sandswept item and equipment stat definitions registered in LookingGlass.
- Replaces hardcoded English stat labels with the current game language.
- Updates dynamically when the game language changes.
- Leaves normal item names, item descriptions, equipment descriptions, and achievement names untouched.
- Does not modify Sandswept, LookingGlass, or Risk of Rain 2 files on disk.

## 🎯 When You Need This

Install this package if all of these are true:
- You have Sandswept installed.
- You have LookingGlass installed.
- You are using a non-English language pack and LookingGlass item stats still show English labels.

You do **not** need this package if you do not use LookingGlass.

## 🌐 Supported Languages

- English (`en`)
- Brazilian Portuguese (`pt-BR`)
- German (`de`)
- Spanish (`es-ES`, `es-419`)
- French (`FR`)
- Italian (`IT`)
- Japanese (`ja`)
- Korean (`ko`)
- Polish (`pl`)
- Russian (`RU`)
- Turkish (`tr`)
- Ukrainian (`UA`, `uk`)
- Esperanto (`eo`)
- Latin (`la`)
- Simplified Chinese (`zh-CN`)
- Traditional Chinese (`zh-TW`)

## 📥 Manual Installation

1. Install the dependencies listed on the Thunderstore page.
2. Install your Sandswept translation pack.
3. Extract this package into your profile's `BepInEx/plugins` folder.
4. The final path should contain:

```text
BepInEx/plugins/Jaosnake-SandGlass/SandGlass.dll
```

## ⚙️ Configuration

After the first launch, BepInEx generates a config file:

```text
BepInEx/config/jaosnake.sandglass.cfg
```

Available options:
- `Enable`: turns the patch on or off.
- `LogReplacements`: logs how many labels were changed. This is disabled by default.

## 👤 Credits
| Role | Name |
|------|------|
| Original Mods | [SandsweptTeam](https://thunderstore.io/c/riskofrain2/p/SandsweptTeam/Sandswept/) & [DropPod](https://thunderstore.io/c/riskofrain2/p/DropPod/LookingGlass/) |
| Patch & Localizations | Jaosnake |

---

## ☕ Support
[![ko-fi](https://img.shields.io/badge/Ko--fi-Support%20Me-%23FF5E5B?logo=ko-fi)](https://ko-fi.com/jaosnake)
