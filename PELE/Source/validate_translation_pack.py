"""Validate translation JSON against a reference in the same token format."""

import argparse
import json
import re
from collections import Counter
from pathlib import Path


PLACEHOLDER = re.compile(r"(?<!\{)\{([0-9]+|[A-Za-z_][\w.]*)\}(?!\})|%([A-Z][A-Z0-9_]+)%")
TAG = re.compile(r'<(/?)(style|color|size|link|mark|sprite|b|i|u|s|br)(?=[\s=>])([^>]*)>', re.I)
UNPAIRED = {"sprite", "br"}


def unique_object(pairs):
    result = {}
    for key, value in pairs:
        if key in result:
            raise ValueError(f"duplicate JSON key: {key}")
        result[key] = value
    return result


def load_tokens(path, language="strings", *, game_reference=False):
    text = path.read_text(encoding="utf-8-sig")
    if game_reference:
        # Some shipped English files contain trailing commas. Preserve quoted text.
        text = re.sub(r'"(?:[^"\\]|\\.)*"|,(?=\s*[}\]])',
                      lambda match: "" if match[0] == "," else match[0], text)
    data = json.loads(text, object_pairs_hook=unique_object)
    if not isinstance(data, dict):
        raise ValueError("expected an object")
    tokens = data.get(language, data)
    if not isinstance(tokens, dict) or any(not isinstance(value, str) for value in tokens.values()):
        raise ValueError("expected string tokens")
    return tokens


def markup(text):
    tags = Counter()
    opened = []
    for match in TAG.finditer(text):
        closing, tag, attributes = match.groups()
        tag = tag.lower()
        if closing:
            if not opened or opened.pop() != tag:
                raise ValueError(f"unexpected closing tag </{tag}>")
        else:
            tags[tag, attributes.strip()] += 1
            if tag not in UNPAIRED:
                opened.append(tag)
    if opened:
        raise ValueError(f"unclosed tags: {', '.join(opened)}")
    return tags


def compare(reference, translated):
    for token, text in translated.items():
        if token not in reference:
            yield token, "missing from reference"
            continue
        original = reference[token]
        if Counter(PLACEHOLDER.findall(text)) != Counter(PLACEHOLDER.findall(original)):
            yield token, "placeholder names or counts differ"
        try:
            if markup(text) != markup(original):
                yield token, "markup tags or attributes differ"
        except ValueError as error:
            yield token, str(error)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("reference", type=Path)
    parser.add_argument("translations", type=Path, nargs="+")
    options = parser.parse_args()
    reference = load_tokens(options.reference)
    failed = False
    for path in options.translations:
        try:
            issues = list(compare(reference, load_tokens(path)))
        except ValueError as error:
            issues = [("", str(error))]
        for token, error in issues:
            print(f"{path.name} {token}: {error}")
        failed |= bool(issues)
    return int(failed)


def validate_pilot(root, game_english, starstorm_english):
    base = {}
    for filename in ("Main.json", "Difficulty.json", "Objectives.json"):
        base.update(load_tokens(game_english / filename, game_reference=True))

    # The reference for strings created by P.E.L.E. is its own English menu.
    menu = (root / "Core/LanguagePauseMenu.cs").read_text(encoding="utf-8-sig")
    base.update(re.findall(r'AddOrUpdateToken\("([A-Z_]+)", "([^"]*)"\);', menu))
    starstorm = load_tokens(starstorm_english, "en")
    issues = []
    for language in ("el", "vi"):
        for path, reference in (
            (root / f"BaseGame/{language}/Pilot.json", base),
            (root / f"Starstorm2/{language}/SS2Lang_{language}.json", starstorm),
        ):
            issues.extend(f"{language}/{path.name} {token}: {error}"
                          for token, error in compare(reference, load_tokens(path)))
    if issues:
        raise ValueError("\n".join(issues))


if __name__ == "__main__":
    raise SystemExit(main())
