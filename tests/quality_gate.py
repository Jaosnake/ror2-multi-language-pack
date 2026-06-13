import json
import re
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
CHECK_ROOTS = [
    ROOT / "mods",
    ROOT / "Sandswept" / "Translations",
    ROOT / "Starstorm2" / "Language",
    ROOT / "thundermods-traduzidos",
]
CRITICAL_STARSTORM_DESC = {
    "SS2_ITEM_STRANGECAN_DESC",
    "SS2_ITEM_WATCHMETRONOME_DESC",
}
AETHERIUM_REQUIRED = {
    "AETHERIUM_EXPANSION_DEF_NAME",
    "AETHERIUM_EXPANSION_DEF_DESCRIPTION",
    "ITEM_ACCURSED_POTION_NAME",
    "ITEM_ACCURSED_POTION_PICKUP",
    "ITEM_ACCURSED_POTION_DESCRIPTION",
    "ITEM_ALIEN_MAGNET_NAME",
    "ITEM_ALIEN_MAGNET_PICKUP",
    "ITEM_ALIEN_MAGNET_DESCRIPTION",
    "ITEM_UNSTABLE_DESIGN_NAME",
    "ITEM_UNSTABLE_DESIGN_PICKUP",
    "ITEM_UNSTABLE_DESIGN_DESCRIPTION",
    "ITEM_HEART_OF_THE_VOID_NAME",
    "ITEM_HEART_OF_THE_VOID_PICKUP",
    "ITEM_HEART_OF_THE_VOID_DESCRIPTION",
    "ITEM_WEIGHTED_ANKLET_NAME",
    "ITEM_WEIGHTED_ANKLET_PICKUP",
    "ITEM_WEIGHTED_ANKLET_DESCRIPTION",
    "ITEM_FEATHERED_PLUME_NAME",
    "ITEM_FEATHERED_PLUME_PICKUP",
    "ITEM_FEATHERED_PLUME_DESCRIPTION",
    "ITEM_NAIL_BOMB_NAME",
    "ITEM_NAIL_BOMB_PICKUP",
    "ITEM_NAIL_BOMB_DESCRIPTION",
    "ITEM_BLOODTHIRSTY_SHIELD_NAME",
    "ITEM_BLOODTHIRSTY_SHIELD_PICKUP",
    "ITEM_BLOODTHIRSTY_SHIELD_DESCRIPTION",
    "ITEM_ENGINEERS_TOOLBELT_NAME",
    "ITEM_ENGINEERS_TOOLBELT_PICKUP",
    "ITEM_ENGINEERS_TOOLBELT_DESCRIPTION",
    "ITEM_SHARK_TEETH_NAME",
    "ITEM_SHARK_TEETH_PICKUP",
    "ITEM_SHARK_TEETH_DESCRIPTION",
    "ITEM_ZENITH_ACCELERATOR_NAME",
    "ITEM_ZENITH_ACCELERATOR_PICKUP",
    "ITEM_ZENITH_ACCELERATOR_DESCRIPTION",
    "ITEM_BLASTER_SWORD_NAME",
    "ITEM_BLASTER_SWORD_PICKUP",
    "ITEM_BLASTER_SWORD_DESCRIPTION",
    "ITEM_INSPIRING_DRONE_NAME",
    "ITEM_INSPIRING_DRONE_PICKUP",
    "ITEM_INSPIRING_DRONE_DESCRIPTION",
    "ITEM_WITCHES_RING_NAME",
    "ITEM_WITCHES_RING_PICKUP",
    "ITEM_WITCHES_RING_DESCRIPTION",
    "ITEM_SHIELDING_CORE_NAME",
    "ITEM_SHIELDING_CORE_PICKUP",
    "ITEM_SHIELDING_CORE_DESCRIPTION",
    "HIDDEN_ITEM_WEIGHTED_ANKLET_LIMITER_RELEASE_NAME",
    "HIDDEN_ITEM_WEIGHTED_ANKLET_LIMITER_RELEASE_PICKUP",
    "HIDDEN_ITEM_WEIGHTED_ANKLET_LIMITER_RELEASE_DESCRIPTION",
    "AETHERIUM_MONSTERS_UNSTABLE_DESIGN_CHIMERA_NAME",
    "EQUIPMENT_BELL_TOTEM_NAME",
    "EQUIPMENT_BELL_TOTEM_PICKUP",
    "EQUIPMENT_BELL_TOTEM_DESCRIPTION",
    "EQUIPMENT_JAR_OF_RESHAPING_NAME",
    "EQUIPMENT_JAR_OF_RESHAPING_PICKUP",
    "EQUIPMENT_JAR_OF_RESHAPING_DESCRIPTION",
    "INTERACTABLE_BELL_TOTEM_NAME",
    "INTERACTABLE_BELL_TOTEM_CONTEXT",
    "INTERACTABLE_BELL_TOTEM_INSPECT",
    "INTERACTABLE_BUFF_BRAZIER_NAME",
    "INTERACTABLE_BUFF_BRAZIER_CONTEXT",
    "INTERACTABLE_BUFF_BRAZIER_INSPECT",
    "AETHERIUM_ELITE_EQUIPMENT_AFFIX_SANGUINE_NAME",
    "AETHERIUM_ELITE_EQUIPMENT_AFFIX_SANGUINE_PICKUP",
    "AETHERIUM_ELITE_EQUIPMENT_AFFIX_SANGUINE_DESCRIPTION",
    "AETHERIUM_ELITE_AFFIX_SANGUINE_MODIFIER",
    "ARTIFACT_ARTIFACT_OF_LEONIDS_NAME",
    "ARTIFACT_ARTIFACT_OF_LEONIDS_DESCRIPTION",
    "ARTIFACT_ARTIFACT_OF_PROGRESSION_NAME",
    "ARTIFACT_ARTIFACT_OF_PROGRESSION_DESCRIPTION",
    "ARTIFACT_ARTIFACT_OF_REGRESSION_NAME",
    "ARTIFACT_ARTIFACT_OF_REGRESSION_DESCRIPTION",
    "ARTIFACT_ARTIFACT_OF_THE_JOURNEY_NAME",
    "ARTIFACT_ARTIFACT_OF_THE_JOURNEY_DESCRIPTION",
    "ARTIFACT_ARTIFACT_OF_THE_NIGHTMARE_NAME",
    "ARTIFACT_ARTIFACT_OF_THE_NIGHTMARE_DESCRIPTION",
    "ARTIFACT_ARTIFACT_OF_THE_TYRANT_NAME",
    "ARTIFACT_ARTIFACT_OF_THE_TYRANT_DESCRIPTION",
    "AETHERIUM_NAIL_BOMB_ACHIEVEMENT_NAME",
    "AETHERIUM_NAIL_BOMB_ACHIEVEMENT_DESC",
    "AETHERIUM_SHIELDING_CORE_ACHIEVEMENT_NAME",
    "AETHERIUM_SHIELDING_CORE_ACHIEVEMENT_DESC",
    "AETHERIUM_UNSTABLE_DESIGN_ACHIEVEMENT_NAME",
    "AETHERIUM_UNSTABLE_DESIGN_ACHIEVEMENT_DESC",
}
PLACEHOLDER_RE = re.compile(r"\{[0-9]+(?:[^}]*)?\}|[0-9]+__")
MOJIBAKE_RE = re.compile(
    r"Ã[¡¢£¤¥¦§¨©ª«¬®¯°±²³´µ¶·¸¹º»¼½¾¿]|Â[¡¢£¤¥¦§¨©ª«¬®¯°±²³´µ¶·¸¹º»¼½¾¿]|â[€€™€œ€“€”‹›„…¢]|ï¿½|�"
)


def iter_translation_files():
    seen = set()
    for root in CHECK_ROOTS:
        if not root.exists():
            continue
        for suffix in ("*.language", "*.json", "*.txt"):
            for path in root.rglob(suffix):
                if path in seen:
                    continue
                seen.add(path)
                yield path


def load_json(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def table_from(data):
    if isinstance(data, dict) and len(data) == 1:
        value = next(iter(data.values()))
        if isinstance(value, dict):
            return next(iter(data)), value
    if isinstance(data, dict) and isinstance(data.get("strings"), dict):
        return None, data["strings"]
    if isinstance(data, dict):
        return None, data
    return None, None


def fail(errors, path, message):
    errors.append(f"{path.relative_to(ROOT)}: {message}")


def check_json_parse(errors):
    for path in iter_translation_files():
        try:
            data = load_json(path)
        except Exception as exc:
            fail(errors, path, f"invalid JSON: {exc}")
            continue
        starstorm_json = (
            path.suffix == ".json"
            and (
                ROOT / "Starstorm2" / "Language" in path.parents
                or ROOT / "thundermods-traduzidos" / "Jaosnake-Starstorm2_ptBR_LanguagePack" / "pt-BR" in path.parents
            )
        )
        if starstorm_json and isinstance(data, dict) and "strings" in data and len(data) > 1:
            extra = ", ".join(sorted(k for k in data if k != "strings"))
            fail(errors, path, f"mixed top-level tables with 'strings': {extra}")


def check_token_regressions(errors):
    for path in iter_translation_files():
        try:
            data = load_json(path)
        except Exception:
            continue
        _locale, table = table_from(data)
        if not isinstance(table, dict):
            continue

        if table.get("ROB_DRIVER_BODY_NAME") not in (None, "Driver"):
            fail(errors, path, f"ROB_DRIVER_BODY_NAME must stay 'Driver', got {table['ROB_DRIVER_BODY_NAME']!r}")
        if table.get("MOFFEIN_HAND_BODY_NAME") not in (None, "HAN-D"):
            fail(errors, path, f"MOFFEIN_HAND_BODY_NAME must stay 'HAN-D', got {table['MOFFEIN_HAND_BODY_NAME']!r}")
        if table.get("SS2_ITEM_LIGHTNINGONKILL_NAME") not in (None, "Man-o'-War"):
            fail(errors, path, f"SS2_ITEM_LIGHTNINGONKILL_NAME must stay \"Man-o'-War\", got {table['SS2_ITEM_LIGHTNINGONKILL_NAME']!r}")

        for key in CRITICAL_STARSTORM_DESC:
            value = table.get(key)
            if isinstance(value, str) and PLACEHOLDER_RE.search(value):
                fail(errors, path, f"{key} still contains unformatted placeholders")


def check_aetherium_pack(errors):
    package = ROOT / "thundermods-traduzidos" / "Jaosnake-Aetherium_LanguagePack"
    if not package.exists():
        fail(errors, ROOT, "missing Jaosnake-Aetherium_LanguagePack")
        return
    files = sorted(package.glob("Aetherium_*.language"))
    if not files:
        fail(errors, package, "no Aetherium_*.language files found")
    for path in files:
        data = load_json(path)
        locale, table = table_from(data)
        missing = sorted(AETHERIUM_REQUIRED - set(table or {}))
        if missing:
            fail(errors, path, "missing required Aetherium tokens: " + ", ".join(missing))
        if locale != "en" and isinstance(table, dict):
            english_fallbacks = {
                "EQUIPMENT_BELL_TOTEM_NAME": "Bell Totem",
                "EQUIPMENT_JAR_OF_RESHAPING_NAME": "Jar Of Reshaping",
                "INTERACTABLE_BUFF_BRAZIER_NAME": "Buff Brazier",
                "AETHERIUM_ELITE_EQUIPMENT_AFFIX_SANGUINE_NAME": "Bloody Fealty",
                "ARTIFACT_ARTIFACT_OF_THE_TYRANT_NAME": "Artifact of the Tyrant",
            }
            for key, bad_value in english_fallbacks.items():
                if table.get(key) == bad_value:
                    fail(errors, path, f"{key} still uses English fallback {bad_value!r}")


def check_package_sizes(errors):
    checks = [
        (ROOT / "thundermods-traduzidos" / "Jaosnake-Starstorm2_LanguagePack", 100),
        (ROOT / "thundermods-traduzidos" / "Jaosnake-Starstorm2_ptBR_LanguagePack", 100),
    ]
    for root, minimum in checks:
        if not root.exists():
            fail(errors, root, "missing package directory")
            continue
        for path in root.glob("*.language"):
            data = load_json(path)
            _locale, table = table_from(data)
            size = len(table or {})
            if size < minimum:
                fail(errors, path, f"package looks truncated: {size} tokens, expected at least {minimum}")


def check_scoped_mojibake(errors):
    scoped = [
        ROOT / "thundermods-traduzidos" / "Jaosnake-Starstorm2_ptBR_LanguagePack",
        ROOT / "thundermods-traduzidos" / "Jaosnake-Aetherium_LanguagePack",
        ROOT / "thundermods-traduzidos" / "Jaosnake-Driver_LanguagePack",
        ROOT / "thundermods-traduzidos" / "Jaosnake-HAND_OVERCLOCKED_LanguagePack",
    ]
    for root in scoped:
        if not root.exists():
            continue
        for path in list(root.rglob("*.language")) + list(root.rglob("*.json")):
            text = path.read_text(encoding="utf-8-sig")
            if MOJIBAKE_RE.search(text):
                fail(errors, path, "contains mojibake marker")


def main():
    errors = []
    check_json_parse(errors)
    check_token_regressions(errors)
    check_aetherium_pack(errors)
    check_package_sizes(errors)
    check_scoped_mojibake(errors)
    if errors:
        print("QUALITY GATE FAILED")
        for error in errors[:200]:
            print("-", error)
        if len(errors) > 200:
            print(f"... {len(errors) - 200} more errors")
        return 1
    print("QUALITY GATE PASSED")
    return 0


if __name__ == "__main__":
    sys.exit(main())
