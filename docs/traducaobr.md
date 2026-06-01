# Starstorm 2 — Translation Master Reference

> Master translation management file for ALL languages of the Starstorm 2 mod for Risk of Rain 2.

**Reference (EN wiki):** <https://starstorm2.wiki.gg/>

---

## Core Rules (ALL Languages)

1. **EN (American) is the ABSOLUTE source of truth.** Any divergence from EN, the translation is corrected to match EN. Never the opposite.
2. **PT-BR is the SECONDARY reference.** Since PT-BR was meticulously audited token-by-token against EN (100% completion), it serves as an additional cross-reference for how EN should be interpreted.
3. **Every language must match EN in token structure.** If EN has a token, the translation must have it. If EN removes a token, the translation must remove it.
4. **Lore + DESCRIPTION (briefing/tips)** must be verified alongside skills — always check both sources.
5. **Skins + achievement names** with puns/references stay in English (game falls back to EN token). Only translate them if the EN token is a plain descriptive name.
6. **Entity/proper nouns**: Ultra Mithrix, Agarthan, Faust — keep in EN. Generic entities (Archer Bug, Clay Monger, etc.) should be translated.
7. **Placeholder tokens**: `WIP`, `[temp]`, `TBD`, `IDK`, `lol` — these are dev placeholders and should remain as-is or match EN exactly.
8. **HTML/RichText tags**: `<style=cIsDamage>`, `<color=#FFCCED>`, `<link=\"textWavy\">`, etc. — preserve ALL tags exactly.
9. **Param tokens**: `{0}`, `{1}`, `{2}` — preserve indices exactly as in EN. Do NOT reorder.
10. **Lore text**: Must correspond exactly to EN content. Creative translation is allowed for idioms/natural flow, but the factual content must match.

---

## Source Languages

| Language | Status | Role |
|----------|--------|------|
| **EN** (English) | ✅ 47/47 files, 100% | **Absolute source of truth** |
| **PT-BR** (Português Brasil) | ✅ 47/47 files, 100% | **Secondary reference** — fully audited against EN |

---

## Per-Language Status

| Language | Files Present | Missing | Stubs/Incomplete | Completion | Format |
|----------|--------------|---------|-----------------|------------|--------|
| **EN** | 47/47 | 0 | 0 | **100%** | Per-file |
| **PT-BR** | 47/47 | 0 | 0 | **100%** | Per-file |
| **RU** | 41/47 | 6 | 0 | **87.2%** | Per-file |
| **zh-CN** | 41 topics | 6 | **9 stubs** | **87.2%** (9 stubs) | Per-file |
| **ko** | 34/47 | 13 | 0 | **72.3%** | Per-file |
| **FR** | 28/47 | 19 | 0 | **59.6%** | Per-file |
| **es-419** | 1 combined | N/A | Unknown | Unknown | Combined |
| **tr** | 1 combined | N/A | Unknown | Unknown | Combined |
| **UA** | **0/47** | **47** | N/A | **0%** | Per-file |

---

## Methodology

1. Read EN file for the category
2. Read PT-BR file for cross-reference
3. Translate all tokens to target language
4. Verify: token count matches EN, no missing keys, no extra keys
5. For lore: ensure factual content matches EN
6. For numbers: use target language formatting conventions
7. For achievements: puns/references stay in EN; descriptive names get translated

---

## Per-Language Translation Notes

### es-419 (Spanish - Latin America)
- Uses single-file combined format: `SS2Language_es-419.json`
- Has `[NOTRANSLATION]` markers on untranslated tokens — these MUST be translated
- Maintain existing formatting conventions

### FR (French)
- Per-file format matching EN structure
- 19 files missing (characters, unlocks, etc.)
- All 28 existing files are properly sized (no stubs)

### ko (Korean)
- Per-file format matching EN structure
- 13 files missing
- All 34 existing files are properly sized

### RU (Russian)
- Per-file format matching EN structure
- 6 files missing (Drones, ItemsCurio, SurvivorDUT, SurvivorWarden, UnlocksCyborg, WikiFormat)
- All 41 existing files are properly sized

### tr (Turkish)
- Uses single-file combined format: `SS2Language_tr.json`
- Has `[NOTRANSLATION]` markers — these MUST be translated
- Maintain existing formatting conventions

### UA (Ukrainian)
- **FOLDER DOES NOT EXIST** — only `.meta` file remains
- Must create folder + all 47 files from scratch
- Follow EN structure exactly

### zh-CN (Chinese - Simplified)
- Per-file format with mixed naming (`_zh.json` vs `_zh-CN.json`)
- 9 files are stubs (under 20% of EN content):
  - SurvivorCyborg2_zh.json (3.2%) — only NAME + BODY_NAME
  - SurvivorKnight_zh.json (14.5%) — only 5 strings
  - SurvivorMULE_zh-CN.json (1.5%) — only NAME
  - SurvivorNemBandit_zh.json (6.5%) — only NAME
  - SurvivorNemCaptain_zh.json (2.2%) — only NAME + SUBTITLE
  - SurvivorNemExecutioner_zh-CN.json (9.0%) — only NAME + SUBTITLE
  - SurvivorNemHuntress_zh.json (6.3%) — only BODY_NAME + BODY_SUBTITLE
  - SurvivorPyro_zh-CN.json (16.6%) — some skills but missing many
  - UnlocksItems_zh.json (46.2%) — borderline incomplete
- 6 topics entirely missing

---

## Progress Tracking

### Completed
- [x] **PT-BR**: 47/47 files, fully audited, ready as secondary reference
- [x] **traducaobr.md**: Moved to project root, expanded to multi-language scope

### In Progress
- [ ] **UA**: Creating folder + all 47 files from scratch
- [ ] **FR**: Creating 19 missing files
- [ ] **ko**: Creating 13 missing files
- [ ] **RU**: Creating 6 missing files
- [ ] **zh-CN**: Filling 9 stubs + creating 6 missing topics
- [ ] **es-419**: Auditing + translating [NOTRANSLATION] tokens
- [ ] **tr**: Auditing + translating [NOTRANSLATION] tokens

### Blocked
- (none)

---

## Quick Reference: File List (47 EN files)

```
SS2Lang_Artifacts_en.json
SS2Lang_Difficulties_en.json
SS2Lang_Drones_en.json
SS2Lang_Elites_en.json
SS2Lang_Equip_en.json
SS2Lang_Events_en.json
SS2Lang_Expansion_en.json
SS2Lang_Interactables_en.json
SS2Lang_ItemsBoss_en.json
SS2Lang_ItemsCurio_en.json
SS2Lang_ItemsLunar_en.json
SS2Lang_ItemsT1_en.json
SS2Lang_ItemsT2_en.json
SS2Lang_ItemsT3_en.json
SS2Lang_NonSurvivorBodies_en.json
SS2Lang_Rules_en.json
SS2Lang_Skins_en.json
SS2Lang_Stages_en.json
SS2Lang_SurvivorBorg_en.json
SS2Lang_SurvivorChirr_en.json
SS2Lang_SurvivorCyborg2_en.json
SS2Lang_SurvivorDUT_en.json
SS2Lang_SurvivorExecutioner_en.json
SS2Lang_SurvivorExecutioner2_en.json
SS2Lang_SurvivorKnight_en.json
SS2Lang_SurvivorMULE_en.json
SS2Lang_SurvivorNemBandit_en.json
SS2Lang_SurvivorNemCaptain_en.json
SS2Lang_SurvivorNemCommando_en.json
SS2Lang_SurvivorNemExecutioner_en.json
SS2Lang_SurvivorNemHuntress_en.json
SS2Lang_SurvivorNemmando_en.json
SS2Lang_SurvivorNemMercenary_en.json
SS2Lang_SurvivorPyro_en.json
SS2Lang_SurvivorWarden_en.json
SS2Lang_UnlocksArtifacts_en.json
SS2Lang_UnlocksBorg_en.json
SS2Lang_UnlocksChirr_en.json
SS2Lang_UnlocksCyborg_en.json
SS2Lang_UnlocksEquips_en.json
SS2Lang_UnlocksExecutioner_en.json
SS2Lang_UnlocksItems_en.json
SS2Lang_UnlocksNemmando_en.json
SS2Lang_UnlocksPyro_en.json
SS2Lang_UnlocksVanilla_en.json
SS2Lang_Variants_en.json
SS2Lang_WikiFormat_en.json
```
