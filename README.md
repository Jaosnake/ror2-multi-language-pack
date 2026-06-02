# ror2-multi-language-pack

## Overview
A **multi‑language pack** repository for **Risk of Rain 2** mods. It gathers translation files for supported mods into a single, Thunderstore‑ready monorepo. Each mod's language pack is completely self‑contained and can be built and published independently.

---

## Available Mods
| Mod | Translation status | Thunderstore package |
|-----|-------------------|----------------------|
| **Starstorm 2** | ✅ Complete – 8 languages | `Starstorm2/` (ready to zip) |
| **Sandswept** | ✅ Complete – 13 languages | `Sandswept/` (ready to zip) |

---

## Supported Languages
- 🇧🇷 PT‑BR (Brazilian Portuguese)
- 🇺🇦 UA (Ukrainian)
- 🇰🇷 KO (Korean)
- 🇫🇷 FR (French)
- 🇷🇺 RU (Russian)
- 🇨🇳 zh‑CN (Simplified Chinese)
- 🇪🇸 es‑419 (Latin American Spanish)
- 🇹🇷 TR (Turkish)

---

## Repository Layout
```
ror2-multi-language-pack/
├─ README.md               # This file
├─ LICENSE.txt             # GPL‑3.0
├─ docs/
│   └─ traducaobr.md       # Translation reference guide
├─ Starstorm2/             # Thunderstore package for Starstorm 2
│   ├─ manifest.json
│   ├─ icon.png
│   ├─ README.md           # Package‑specific readme
│   └─ Language/           # 8 language folders, 47 JSON files each
├─ Sandswept/              # Thunderstore package for Sandswept
│   ├─ manifest.json
│   ├─ icon.png
│   ├─ README.md           # Package‑specific readme
│   └─ Translations/       # 13 .language files (all locales)
└─ docs/
    └─ traducaobr.md       # Translation reference guide
```
Each subfolder is a **stand‑alone Thunderstore package** – simply zip its contents (manifest, icon, README, Language folder) and upload.

---

## Getting Started
1. **Clone the repo**
   ```bash
   git clone https://github.com/Jaosnake/ror2-multi-language-pack.git
   cd ror2-multi-language-pack
   ```
2. **Build a package** (example for Starstorm 2):
   ```bash
   cd Starstorm2
   zip -r ../Starstorm2.zip *
   ```
   The resulting `Starstorm2.zip` can be uploaded to Thunderstore.
3. **Add or update translations** – edit the JSON files under `Language/<lang>/` and run the sync script (if needed) to keep keys consistent across languages.

---

## Contributing
- Fork the repository.
- Create a branch for your changes.
- Ensure translation keys stay in sync (all languages must have the same set of keys).
- Submit a pull request.

---

## License
This project is licensed under the **GNU General Public License v3.0**.

---

## Credits
- **Translations**: Jaosnake
- **Original mods**: TeamMoonstorm (Starstorm 2), TeamSandswept (Sandswept) and their respective authors.
