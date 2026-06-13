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
    "ITEM_FEATHERED_PLUME_NAME",
    "ITEM_FEATHERED_PLUME_PICKUP",
    "ITEM_FEATHERED_PLUME_DESCRIPTION",
    "ITEM_ZENITH_ACCELERATOR_NAME",
    "ITEM_ZENITH_ACCELERATOR_PICKUP",
    "ITEM_ZENITH_ACCELERATOR_DESCRIPTION",
    "ITEM_SHIELDING_CORE_NAME",
    "ITEM_SHIELDING_CORE_PICKUP",
    "ITEM_SHIELDING_CORE_DESCRIPTION",
    "AETHERIUM_SHIELDING_CORE_ACHIEVEMENT_NAME",
    "AETHERIUM_SHIELDING_CORE_ACHIEVEMENT_DESC",
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
        _locale, table = table_from(data)
        missing = sorted(AETHERIUM_REQUIRED - set(table or {}))
        if missing:
            fail(errors, path, "missing required Aetherium tokens: " + ", ".join(missing))


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
