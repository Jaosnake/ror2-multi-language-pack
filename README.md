# ror2-multi-language-pack

## Overview
A **multi‑language pack** repository for **Risk of Rain 2** mods. It gathers translation files for supported mods into a single, Thunderstore‑ready monorepo. Each mod's language pack is completely self‑contained and can be built and published independently.

> ⚠️ **UA (Ukrainian)** is not a natively supported language in Risk of Rain 2.  
> See [UA-LANGUAGE-NOTE.md](Sandswept/UA-LANGUAGE-NOTE.md) for technical details.

---

## Available Mods
| Mod | Translation status | Location |
|-----|-------------------|----------|
| **Starstorm 2** | ✅ Complete – 8 languages | `Starstorm2/` (ready to zip) |
| **Sandswept** | ✅ Complete – 14 languages | `Sandswept/` (ready to zip) |
| **AssassinMod** | ✅ EN, pt-BR, es-419, FR, ru, zh-CN, ja | `mods/HasteReapr-AssassinMod/` |
| **Banshee** | ✅ EN, pt-BR, es-419, FR, ru, zh-CN, ja | `mods/tsuyoikenko-Banshee/` |
| **Bastian** | ✅ EN, pt-BR, es-419, FR, ru, zh-CN, ja | `mods/TeamSillyGuy-Bastian/` |
| **Belmont** | ✅ EN, pt-BR, es-419, FR, ru, zh-CN, ja | `mods/rob-Belmont/` |
| **Cadet** | ✅ EN, pt-BR, es-419, FR, ru, zh-CN, ja | `mods/tsuyoikenko-Cadet/` |
| **Cloudburst** | ✅ EN, pt-BR, es-419, FR, ru, zh-CN, ja | `mods/TeamCloudburst-Cloudburst/` |
| **Dancer** | ✅ EN, pt-BR, es-419, FR, ru, zh-CN, ja | `mods/nayDPz-Dancer/` |
| **Dante** | ✅ EN, pt-BR, es-419, FR, ru, zh-CN, ja | `mods/rob-Dante/` |
| **Deputy** | ✅ EN, pt-BR, es-419, FR, ru, zh-CN, ja | `mods/Bog-Deputy/` |
| **Driver** | ✅ pt-BR, es-419, FR, ru, zh-CN, ja | `mods/public_ParticleSystem-Driver/` |
| **Enforcer** | ✅ EN, pt-BR, es-419, FR, ru, zh-CN, ja | `mods/EnforcerGang-Enforcer/` |
| **HAND_OVERCLOCKED** | ✅ EN, pt-BR, es-419, FR, ru, zh-CN, ja | `mods/EnforcerGang-HAND_OVERCLOCKED/` |
| **HEL_P** | ✅ EN, pt-BR, es-419, FR, ru, zh-CN, ja | `mods/rob-HEL_P/` |
| **HUNK** | ✅ EN, pt-BR, es-419, FR, ru, zh-CN, ja | `mods/rob-HUNK/` |
| **Interrogator** | ✅ EN, pt-BR, es-419, FR, ru, zh-CN, ja | `mods/tsuyoikenko-Interrogator/` |
| **Lee_Hyperreal** | ✅ EN, pt-BR, es-419, FR, ru, zh-CN, ja | `mods/PopcornFactory-Lee_Hyperreal/` |
| **MinerUnearthed** | ✅ EN, pt-BR, es-419, FR, ru, zh-CN, ja | `mods/EnforcerGang-MinerUnearthed/` |
| **Mortician** | ✅ EN, pt-BR, es-419, FR, ru, zh-CN, ja | `mods/Bog-Mortician/` |
| **Myst** | ✅ EN, pt-BR, es-419, FR, ru, zh-CN, ja | `mods/JavAngle-Myst/` |
| **PaladinMod** | ✅ EN, pt-BR, es-419, FR, ru, zh-CN, ja | `mods/Paladin_Alliance-PaladinMod/` |
| **Pathfinder** | ✅ EN, pt-BR, es-419, FR, ru, zh-CN, ja | `mods/Bog-Pathfinder/` |
| **Pilot** | ✅ EN, pt-BR, es-419, FR, ru, zh-CN, ja | `mods/EnforcerGang-Pilot/` |
| **Ravager** | ✅ pt-BR, es-419, FR, ru, zh-CN, ja | `mods/public_ParticleSystem-Ravager/` |
| **Red_Alert** | ✅ EN, pt-BR, es-419, FR, ru, zh-CN, ja | `mods/TheTimesweeper-Red_Alert/` |
| **Rifter** | ✅ EN, pt-BR, es-419, FR, ru, zh-CN, ja | `mods/toastyTeam-Rifter/` |
| **RiskyTweaks** | ✅ EN, pt-BR, es-419, FR, ru, zh-CN, ja | `mods/Risky_Lives-RiskyTweaks/` |
| **RobomandoMod** | ✅ EN, pt-BR, es-419, FR, ru, zh-CN, ja | `mods/The_Bozos-RobomandoMod/` |
| **Rocket** | ✅ EN, pt-BR, es-419, FR, ru, zh-CN, ja | `mods/EnforcerGang-Rocket/` |
| **Arsonist** | ✅ EN, pt-BR, es-419, FR, ru, zh-CN, ja | `mods/PopcornFactory-Arsonist_Mod/` |
| **SorceressMod** | ✅ EN | `mods/Frosthex-SorceressMod/` |
| **TheHouse** | ✅ EN, pt-BR, es-419, FR, ru, zh-CN, ja | `mods/JavAngle-TheHouse/` |

---

## 📖 Choose Your Language | Escolha seu Idioma

Click on your language below to read the documentation in your preferred language:

**[🇧🇷 Português (Brasil)](#-português-brasil)** | **[🇺🇦 Українська](#-українська)** | **[🇰🇷 한국어](#-한국어)** | **[🇫🇷 Français](#-français)** | **[🇷🇺 Русский](#-русский)** | **[🇨🇳 中文](#-中文)** | **[🇪🇸 Español](#-español)** | **[🇹🇷 Türkçe](#-türkçe)**

---

## 🇧🇷 Português (Brasil)

### Sobre
Este repositório contém pacotes de tradução para mods de **Risk of Rain 2**, incluindo **Starstorm 2** e **Sandswept**, com suporte a múltiplos idiomas.

### Instalação
1. Tenha o mod original instalado via **r2modman** ou **Thunderstore**
2. Baixe este pacote e extraia na pasta `BepInEx/plugins/` do jogo
3. Selecione o idioma nas configurações do jogo (Settings → Language)

### Créditos
- **Mod Original**: TeamMoonstorm (Starstorm 2), TeamSandswept (Sandswept)
- **Tradução PT-BR**: Jaosnake (100% manual)
- **Demais idiomas**: Jaosnake com assistência de IA

### Licença
GPL-3.0

---

## 🇺🇦 Українська

### Про
Цей репозиторій містить пакети перекладів для модів **Risk of Rain 2**, включаючи **Starstorm 2** та **Sandswept**, з підтримкою кількох мов.

### Встановлення
1. Встановіть оригінальний мод через **r2modman** або **Thunderstore**
2. Завантажте цей пакет і розпакуйте в папку `BepInEx/plugins/` гри
3. Виберіть мову в налаштуваннях гри (Settings → Language)

> ⚠️ Українська мова не є рідною для Risk of Rain 2.  
> Докладніше: [UA-LANGUAGE-NOTE.md](Sandswept/UA-LANGUAGE-NOTE.md)

### Подяки
- **Оригінальний мод**: TeamMoonstorm (Starstorm 2), TeamSandswept (Sandswept)
- **Переклад**: Jaosnake за допомогою ШІ

### Ліцензія
GPL-3.0

---

## 🇰🇷 한국어

### 정보
이 저장소는 **Starstorm 2** 및 **Sandswept** 를 포함한 **Risk of Rain 2** 모드의 다국어 번역 팩을 제공합니다.

### 설치 방법
1. **r2modman** 또는 **Thunderstore** 를 통해 원래 모드를 설치하세요
2. 이 팩을 다운로드하여 게임의 `BepInEx/plugins/` 폴더에 압축을 풉니다
3. 게임 설정에서 언어를 선택하세요 (Settings → Language)

### 크레딧
- **원본 모드**: TeamMoonstorm (Starstorm 2), TeamSandswept (Sandswept)
- **번역**: Jaosnake (AI 지원)

### 라이선스
GPL-3.0

---

## 🇫🇷 Français

### À propos
Ce dépôt contient des packs de traduction pour les mods **Risk of Rain 2**, y compris **Starstorm 2** et **Sandswept**, avec prise en charge de plusieurs langues.

### Installation
1. Installez le mod original via **r2modman** ou **Thunderstore**
2. Téléchargez ce pack et extrayez-le dans le dossier `BepInEx/plugins/` du jeu
3. Sélectionnez la langue dans les paramètres du jeu (Settings → Language)

### Crédits
- **Mod original**: TeamMoonstorm (Starstorm 2), TeamSandswept (Sandswept)
- **Traduction**: Jaosnake (assisté par IA)

### Licence
GPL-3.0

---

## 🇷🇺 Русский

### О проекте
Этот репозиторий содержит пакеты переводов для модов **Risk of Rain 2**, включая **Starstorm 2** и **Sandswept**, с поддержкой нескольких языков.

### Установка
1. Установите оригинальный мод через **r2modman** или **Thunderstore**
2. Скачайте этот пакет и распакуйте в папку `BepInEx/plugins/` игры
3. Выберите язык в настройках игры (Settings → Language)

### Благодарности
- **Оригинальный мод**: TeamMoonstorm (Starstorm 2), TeamSandswept (Sandswept)
- **Перевод**: Jaosnake (с помощью ИИ)

### Лицензия
GPL-3.0

---

## 🇨🇳 中文

### 关于
此仓库包含 **Risk of Rain 2** 模组的多语言翻译包，包括 **Starstorm 2** 和 **Sandswept**。

### 安装方法
1. 通过 **r2modman** 或 **Thunderstore** 安装原始模组
2. 下载此包并解压到游戏的 `BepInEx/plugins/` 文件夹
3. 在游戏设置中选择语言（Settings → Language）

### 致谢
- **原始模组**: TeamMoonstorm (Starstorm 2), TeamSandswept (Sandswept)
- **翻译**: Jaosnake (AI 辅助)

### 许可证
GPL-3.0

---

## 🇪🇸 Español (Latinoamérica)

### Acerca de
Este repositorio contiene paquetes de traducción para mods de **Risk of Rain 2**, incluyendo **Starstorm 2** y **Sandswept**, con soporte para varios idiomas.

### Instalación
1. Instala el mod original a través de **r2modman** o **Thunderstore**
2. Descarga este paquete y extráelo en la carpeta `BepInEx/plugins/` del juego
3. Selecciona el idioma en la configuración del juego (Settings → Language)

### Créditos
- **Mod original**: TeamMoonstorm (Starstorm 2), TeamSandswept (Sandswept)
- **Traducción**: Jaosnake (asistido por IA)

### Licencia
GPL-3.0

---

## 🇹🇷 Türkçe

### Hakkında
Bu depo, **Starstorm 2** ve **Sandswept** dahil olmak üzere **Risk of Rain 2** modları için çok dilli çeviri paketleri içerir.

### Kurulum
1. Orijinal modu **r2modman** veya **Thunderstore** üzerinden kurun
2. Bu paketi indirin ve oyunun `BepInEx/plugins/` klasörüne çıkarın
3. Oyun ayarlarından dili seçin (Settings → Language)

### Katkıda Bulunanlar
- **Orijinal mod**: TeamMoonstorm (Starstorm 2), TeamSandswept (Sandswept)
- **Çeviri**: Jaosnake (Yapay Zeka destekli)

### Lisans
GPL-3.0

---

## Repository Layout
```
ror2-multi-language-pack/
├─ README.md               # This file
├─ LICENSE.txt             # GPL‑3.0
├─ docs/
│   └─ traducaobr.md       # Translation reference guide
├─ Starstorm2/             # Thunderstore package for Starstorm 2
│   ├─ manifest.json
│   ├─ icon.png
│   ├─ README.md           # Package‑specific readme
│   └─ Language/           # 8 language folders, 47 JSON files each
├─ Sandswept/              # Thunderstore package for Sandswept
│   ├─ manifest.json
│   ├─ icon.png
│   ├─ README.md           # Package‑specific readme
│   └─ Translations/       # 14 .language files (all locales)
└─ mods/                   # Translation files for 31 additional mods
    ├─ Bog-Deputy/         # (alphabetically sorted)
    ├─ Bog-Mortician/
    ├─ Bog-Pathfinder/
    ├─ ...
    └─ tsuyoikenko-Interrogator/
```
Starstorm2 and Sandswept are **stand‑alone Thunderstore packages** – simply zip their contents and upload.  
The `mods/` folder contains translation files for additional mods, organized alphabetically.

---

## Getting Started
1. **Clone the repo**
   ```bash
   git clone https://github.com/Jaosnake/ror2-multi-language-pack.git
   cd ror2-multi-language-pack
   ```
2. **Build a package** (example for Starstorm 2):
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
- **Original mods**: TeamMoonstorm (Starstorm 2), TeamSandswept (Sandswept) and their respective authors.