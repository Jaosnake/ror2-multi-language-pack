# Risk of Rain 2 Multi-Language Pack

Translation packs for several **Risk of Rain 2** mods, organized for GitHub, manual installation, and future Thunderstore publishing.

> **AI disclosure**: some translations were created with AI assistance and then reviewed/edited by Jaosnake. PT-BR files are manual or human-reviewed. This disclosure is kept here and in package READMEs for Thunderstore clarity.

## Quick Download

For most players:

1. Click **Code** -> **Download ZIP** on GitHub.
2. Extract the ZIP somewhere easy to find.
3. Open your r2modman profile folder:
   `Settings` -> `Browse profile folder` -> `BepInEx/plugins`
4. Copy only the translation folder for the mod you want.
5. Start the game and pick your language in the game's language menu.

Default r2modman path on Windows:

```text
C:\Users\<your-user>\AppData\Roaming\r2modmanPlus-local\RiskOfRain2\profiles\Default\BepInEx\plugins
```

## What To Copy

Keep each folder structure intact. Do not copy only one random file unless the package README says it is safe.

### Starstorm 2

Use the folder:

```text
Starstorm2/
```

This is a standalone package folder for **TeamMoonstorm-Starstorm2**. It contains:

```text
Starstorm2/
├─ manifest.json
├─ README.md
├─ icon.png
└─ Language/
   ├─ en/
   ├─ pt-BR/
   ├─ FR/
   ├─ RU/
   ├─ UA/
   ├─ ko/
   ├─ tr/
   ├─ zh-CN/
   └─ es-419/
```

### Sandswept

Use the folder:

```text
Sandswept/
```

This is a standalone package folder for **SandsweptTeam-Sandswept**. It contains `.language` files under:

```text
Sandswept/Translations/
```

### Other Mods

Use the matching folder inside:

```text
mods/
```

Example:

```text
mods/Bog-Deputy/
mods/EnforcerGang-Enforcer/
mods/rob-HUNK/
```

Copy the whole chosen folder into `BepInEx/plugins/`.

## Supported Language IDs

The repo uses the language identifiers expected by Risk of Rain 2, R2API.Language, or the original mod loader.

Common native or commonly used IDs:

```text
en, pt-BR, FR, RU, IT, de, ja, ko, tr, es-419, es-ES, zh-CN, zh-TW
```

Custom/non-native IDs included for packs that support them:

```text
UA, pl
```

Important:

- `UA` is Ukrainian. Risk of Rain 2 does not expose Ukrainian in the vanilla language selector.
- `pl` is Polish. It is loaded by R2API when present, but may also need a locale/menu enabler depending on the setup.
- Achievements and unlock text are intentionally kept in English.

More details: [Sandswept/UA-LANGUAGE-NOTE.md](Sandswept/UA-LANGUAGE-NOTE.md)

## Available Packages

| Package | Original mod | Folder | Notes |
|---|---|---|---|
| Starstorm 2 Language Pack | TeamMoonstorm-Starstorm2 | `Starstorm2/` | Standalone Thunderstore-ready package |
| Sandswept Language Pack | SandsweptTeam-Sandswept | `Sandswept/` | Standalone Thunderstore-ready package |
| Additional mod translations | Multiple mods | `mods/` | One folder per mod |

## Mods In `mods/`

These folders are meant to be copied as-is into `BepInEx/plugins/`:

```text
Bog-Deputy
Bog-Mortician
Bog-Pathfinder
EnforcerGang-Enforcer
EnforcerGang-HAND_OVERCLOCKED
EnforcerGang-MinerUnearthed
EnforcerGang-Pilot
EnforcerGang-Rocket
Frosthex-SorceressMod
HasteReapr-AssassinMod
JavAngle-Myst
JavAngle-TheHouse
nayDPz-Dancer
Paladin_Alliance-PaladinMod
PopcornFactory-Arsonist_Mod
PopcornFactory-Lee_Hyperreal
public_ParticleSystem-Driver
public_ParticleSystem-Ravager
Risky_Lives-RiskyTweaks
rob-Belmont
rob-Dante
rob-HEL_P
rob-HUNK
TeamCloudburst-Cloudburst
TeamSillyGuy-Bastian
The_Bozos-RobomandoMod
TheTimesweeper-Red_Alert
toastyTeam-Rifter
tsuyoikenko-Banshee
tsuyoikenko-Cadet
tsuyoikenko-Interrogator
```

## For GitHub Users

Clone the repo:

```bash
git clone https://github.com/Jaosnake/ror2-multi-language-pack.git
cd ror2-multi-language-pack
```

Create a Thunderstore ZIP from a package folder:

```bash
cd Starstorm2
zip -r ../Starstorm2_LanguagePack.zip .
```

The ZIP root must contain `manifest.json`, `README.md`, `icon.png`, and the translation files/folders.

## For Manual Install Users

If you do not use r2modman:

1. Install BepInEx for Risk of Rain 2.
2. Install the original mod first.
3. Open:

```text
Risk of Rain 2/BepInEx/plugins/
```

4. Copy the translation folder into `plugins`.
5. Launch the game.

R2API.Language automatically loads `.language` files anywhere under `BepInEx/plugins/`. Some original mods also load `Language/<locale>/*.txt` or `Language/<locale>/*.json` folders directly.

## Repository Layout

```text
ror2-multi-language-pack/
├─ README.md
├─ LICENSE.txt
├─ Starstorm2/
│  ├─ manifest.json
│  ├─ README.md
│  ├─ icon.png
│  └─ Language/
├─ Sandswept/
│  ├─ manifest.json
│  ├─ README.md
│  ├─ icon.png
│  └─ Translations/
└─ mods/
   └─ <one folder per translated mod>
```

## Contributing

Pull requests are welcome.

Rules for translation changes:

- Keep token keys unchanged.
- Keep style tags like `<style=...>`, `<color=...>`, `{0}`, `{1}`, and `\n`.
- Keep achievements/unlocks in English.
- Use the same language folder/key style already used by that mod.
- Do not remove the original mod credits.

## Credits

Translations and packaging: **Jaosnake**

Original mod authors include TeamMoonstorm, SandsweptTeam, Bog, EnforcerGang, Frosthex, HasteReapr, JavAngle, nayDPz, Paladin_Alliance, PopcornFactory, public_ParticleSystem, Risky_Lives, rob, TeamCloudburst, TeamSillyGuy, The_Bozos, TheTimesweeper, toastyTeam, and tsuyoikenko.

Community translation credits preserved from the original files include Kauzok, Donitodorito, StyleMyk, Meteorite1014, Lecarde/lecarde, punch, Bagre, WockyTheWolf/JunJun_w, FyreBW/Fyrebw, Damglador, CaffeinePain, Juhnter, Lonerdev, JunJun_W, Hexxedude, tymmey, Dice, Nikto0o, Rody/FallenTroop,锅巴, and PlNK.

## License

GPL-3.0. See [LICENSE.txt](LICENSE.txt).
