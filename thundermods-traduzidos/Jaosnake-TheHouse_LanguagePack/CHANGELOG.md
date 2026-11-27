# Changelog

## 1.0.4
- Fixed raw `{0}` and `{1}` placeholders that appeared in-game across The House's skill, shot, and card descriptions (such as Standard Shot `500%`, Piercing Shot `750%`, Buck Shot `130% x 16`, Ace Shot `2500%`, CardArts: Sweep `700%` / `15%`, Bust `35%` / `100%`, and 5 of Coins `40%` / `400%`) as well as missing numbers on Ante cards and BulletArts: Quickdraw (`60%` faster, `55%` less damage).
- Fixed corrupted card, deck, ending, and lore texts across all 15 languages — including the full survivor lore dialogue, victory/defeat quotes, Snake Eyes, Russian Roulette, The Star, The Tower, Joker, and 10 of Swords — and added the missing passive Note tooltip.
- Standardized package metadata and README to clarify that [P.E.L.E](https://thunderstore.io/c/riskofrain2/p/Jaosnake/PELE/) is optional and only needed for Ukrainian, Esperanto, or Latin.

## 1.0.3
- Updated for official JavAngle-TheHouse-2.3.3.
- Expanded translation across all 15 languages (cards, roulette, shots, lore, and achievements).
- Fixed file fragmentation and normalized naming to `TheHouse_<lang>.language`.
- Added Esperanto (`eo` with x-system) and Latin (`la`).

## 1.0.2
- Fixed mojibake, accent issues, language tokens, and untranslated English fallbacks.
- Rebuilt the R2API.Language package for manual Thunderstore upload.

## 1.0.1
- Fixed credits that were polluting the interface due to an error during file upload.
