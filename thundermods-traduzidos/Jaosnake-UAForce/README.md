# UAForce

UAForce lets players use Ukrainian translation packs in Risk of Rain 2 even though the base game does not show Ukrainian in the normal language menu.

This plugin does not translate anything by itself. It only tells the game to use the Ukrainian language code so R2API.Language can load Ukrainian translation files from installed language packs.

## What You Need

Install these in the same r2modman profile:

- R2API_Language
- Risk Of Options
- UAForce
- One or more translation packs that include Ukrainian files

## Simple Usage

1. Open r2modman.
2. Select your Risk of Rain 2 profile.
3. Click **Start modded**.
4. Open the in-game mod options menu from **Risk Of Options**.
5. Select **UAForce**.
6. Turn **Enable** on.
7. Keep **LanguageCode** as `UA`.
8. Press **Apply now**.

If a mod only loads its language files when the game starts, restart the game after pressing **Apply now**.

## Important

You do not need to change your Windows language.

You do not need to select Ukrainian in the Risk of Rain 2 language menu.

Risk of Rain 2 does not officially show Ukrainian in the vanilla language menu. UAForce changes the internal language code directly.

## Turning It Off

To stop forcing Ukrainian:

1. Open the Risk Of Options menu.
2. Select **UAForce**.
3. Turn **Enable** off.
4. Press **Apply now**.

UAForce will try to return to the language that was active before it forced Ukrainian. If it cannot detect that language, it uses the fallback language code from the config.

## Config File

The config file is created after launching the game once:

```text
BepInEx/config/jaosnake.uaforce.cfg
```

Most players do not need to edit this file manually. Use the Risk Of Options menu instead.

## Options

- `Enable`: turns Ukrainian forcing on or off.
- `LanguageCode`: the language code to force. Keep this as `UA` unless a specific pack tells you otherwise.
- `OnlyWhenSystemLocaleIsUkrainian`: only forces Ukrainian if Windows is already using a Ukrainian UI culture. This is off by default.
- `FallbackLanguageCode`: language to use when UAForce is turned off and the previous language is unknown. Default is `en`.

## Troubleshooting

If the game is still in English:

- Make sure the translation pack you installed actually includes Ukrainian.
- Make sure R2API_Language is installed.
- Make sure Risk Of Options is installed.
- Open UAForce in Risk Of Options and press **Apply now**.
- Restart the game if the mod only loads language files during startup.

If a specific item, skill, or menu is still in English, that text may not be exposed through R2API.Language, or that mod may not have a Ukrainian translation for that token yet.
## Loading Time Note

Installing additional plugins can slightly increase the game's loading time. This is normal.
