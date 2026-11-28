# P.E.L.E. 1.0.10 source

- [Core](Core/) registers the additional languages, loads P.E.L.E. JSON translations and integrates with the official R2API.Language at runtime.
- [LanguageSupport](LanguageSupport/) manages fonts, flags, selectors, HUD refresh and the ShapeText process.
- [ShapeText source and Windows build instructions](LanguageSupport/README.md).
- [KingModUtilities compatibility plugin](../../PELE.KingDBZFix/) for the separate Goku, Vegeta and Trunks translation packs.
- [Base game and DLC translations](../Language/), [complex-script translations and assets](../LanguageSupport/), and [Cyrillic font](../Fonts/).
- [Greek and Vietnamese Starstorm 2 samples](Examples/Starstorm2/) for Coffee Bag, Guarding Amulet and Relic of Mass. These belong to the separate pilot LanguagePack.

The historical [R2API.Language fork](../../R2API.Language-Source/) predates this architecture. A 1.0.10 installation uses the official dependency and the separate P.E.L.E. assemblies. The [Thunderstore upload ZIP](../../mods/PELE/Jaosnake-PELE-1.0.10-THUNDERSTORE.zip) preserves the audited runtime code and resources. The public archive removes build-machine PDB paths from two P.E.L.E. DLLs. Its SHA-256 is `26ac366621730ae67908559bd8afb2704f607e4b821cf81af63c5c046aafc7b5`.

## Build the game plugins on Windows

Use a .NET SDK with C# 12 support. Set `PELE_GAME_MANAGED` to the game's `Risk of Rain 2_Data/Managed` directory and `PELE_BUILD_BEPINEX` to the BepInEx directory of a build profile with the dependencies in [manifest.json](../manifest.json). The core references a generated `MMHOOK_RoR2.dll` under `plugins/MMHOOK/` and Newtonsoft.Json from the RoR2BepInExPack. The example MSBuild properties also let you point to a separate Steam library or profile.

From this directory in PowerShell:

```powershell
dotnet build Core/R2API.Language.csproj -c Release "-p:GameManagedPath=$env:PELE_GAME_MANAGED" "-p:PeleBuildBepInEx=$env:PELE_BUILD_BEPINEX"
dotnet build LanguageSupport/LanguageSupport.csproj -c Release "-p:GameManaged=$env:PELE_GAME_MANAGED" "-p:ProfileBepInEx=$env:PELE_BUILD_BEPINEX"
```

The core must be built first. For distribution, pass `-p:DebugType=None` to both builds to omit build-machine paths from the PE debug directory. The projects write to their own `bin/Release/netstandard2.1/` directories. Keep game DLLs and the official R2API.Language DLL in the profile as references/dependencies. The archive records the exact distributed files and install paths. See the [technical manual (PT-BR)](../../docs/PELE.md) for runtime diagnosis.

## Validate a translation file

```text
python validate_translation_pack.py reference.json translation.json
```

Use a verified reference in the supported JSON format. The validator checks duplicate keys, placeholder names and counts, and supported markup in the translated keys. Review the meaning in context and verify the result in-game.
