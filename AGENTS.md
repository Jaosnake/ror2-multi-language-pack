# ror2-multi-language-pack — Agent Reference

## Project Overview

Multi-language translation pack for Risk of Rain 2 mods. Supports 16+ languages across 30+ mods and 2 standalone DLC packs (Starstorm2, Sandswept).

## Architecture

### Two systems work together:

**1. Jaosnake `.language` files** → loaded by R2API.Language
- One folder per mod under `mods/<ModName>/`
- R2API auto-discovers all `.language` files in BepInEx plugins
- Format: `{"pt-BR": {"TOKEN": "texto"}}`
- Covers **mod-specific characters** (Bastian, Henry, Dancer, etc.)

**2. PELE plugin** → base game translations
- `PELE/PELE.dll` + `PELE/Language/la/`, `eo/`, `uk/`
- Registers `la`, `eo`, `uk` as native menu languages (replaces ULForce)
- Translates **base game UI**: menu, logbook, items, achievements, vanilla characters, DLCs
- Uses Harmony transpiler to fix `CultureInfo("la")`/`CultureInfo("eo")` crash

### Execution order:
1. PELE pre-registers languages via reflection into `Language.languagesByName`
2. PELE registers `Language/` folder via `collectLanguageRootFolders`
3. R2API.Language loads all `.language` files from all plugin folders
4. When player selects a language, both sources merge

## Key Files

| File | Purpose |
|------|---------|
| `PELE/PELE.dll` | BepInEx plugin v1.3.0 |
| `PELE/Language/la/` | 46 JSON files - Latin base game translations |
| `PELE/Language/eo/` | 46 JSON files - Esperanto base game translations |
| `PELE/Language/uk/` | 80 JSON files - Ukrainian base game translations |
| `mods/<Mod>/` | Per-mod `.language` files |
| `README.md` | User documentation, language guides |

## Common Tasks

### Adding a new language to PELE
1. Create `PELE/Language/<code>/language.json` with `selfname`
2. Create translation JSON files matching the `uk/` structure
3. Add `<code>` to `PelePlugin.cs` PreRegisterCustomLanguages array
4. Update transliterator in `SetCurrentLanguageTranspiler`
5. Recompile PELE.dll

### Adding translations for a new mod
1. Create `mods/<ModName>/` folder following existing mod structure
2. Create `.language` files with format `{"<code>": {TOKEN: text}}`
3. Both naming conventions accepted: `<code>-Mod.language` or `mod_<code>.language`

### Fixing corrupted translations (current PELE issue)
See `Desktop/pele_fix_pack.zip` — 150 Latin + 11 Esperanto tokens need fixing.
Fix instructions in `pele_fix_pack/AGENTS.md`.

## GitHub Workflow
```bash
git add -A
git commit -m "type: description"
git push
```

## Debugging
- BepInEx log: `profiles/Default/BepInEx/LogOutput.log`
- Search for "PELE" or "P.E.L.E" in log
- Plugin source: decompiled at `Desktop/PELE_backup_*/pele_src/`
