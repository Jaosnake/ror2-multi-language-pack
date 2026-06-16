# PELE - Diagnóstico e Fix (RESOLVIDO)

## Objetivo
Ter **Latim (la)** e **Esperanto (eo)** como idiomas nativos selecionáveis no menu do Risk of Rain 2.

## Arquitetura do mod PELE
- Mod único tipo BepInEx plugin (C# DLL), como o [Damglador/Risk-of-Rain-2-UA](https://github.com/Damglador/Risk-of-Rain-2-UA)
- Plugin: `plugins/PELE/PELE.dll`
- Traduções: `plugins/PELE/Language/la/` (46 JSONs) + `Language/eo/` (46 JSONs) + `Language/uk/` (78 JSONs)
- Registro: `Language.collectLanguageRootFolders` + Harmony `FindLanguageByName` postfix

## Problemas encontrados e resolvidos

### 1. CultureNotFoundException (RESOLVIDO)
`new CultureInfo("la")` lança exceção porque `"la"`/`"eo"` não são culturas .NET válidas.
**Fix:** Harmony Transpiler em `Language.SetCurrentLanguage` que insere `FixCultureName(string)` antes do `newobj CultureInfo`, substituindo `"la"`/`"eo"` por `""`.

### 2. SimpleJSON corrompido (RESOLVIDO)
`plugins\R2API\libs\SimpleJSON.dll` estava quebrado (modificado pelo mod Chaos em desenvolvimento).
**Fix:** Deletar o arquivo e reinstalar R2API_Core pelo r2modman.

### 3. Prefix method NULL (RESOLVIDO)
`GetMethod` usava `BindingFlags.NonPublic` para método `public`.
**Fix:** `BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic`

### 4. Crash 6% loading (RESOLVIDO)
Causado pelo SimpleJSON.dll corrompido, não pelo PELE.

## Versão final: PELE v1.3.0
- `PreRegisterCustomLanguages`: registra `la`/`eo` via reflection no `languagesByName`
- `collectLanguageRootFolders`: registra pasta `Language/`
- `FindLanguageByNamePostfix`: fallback para encontrar idiomas customizados
- `SetCurrentLanguageTranspiler`: injecta `FixCultureName` antes de `newobj CultureInfo`

## Para recompilar
```powershell
# Dependências:
# BepInEx: profiles\Default\BepInEx\core\BepInEx.dll
# Harmony: profiles\Default\BepInEx\core\0Harmony.dll
# RoR2: D:\SteamLibrary\steamapps\common\Risk of Rain 2\Risk of Rain 2_Data\Managed\
dotnet build --configuration Release PELE.csproj
# Output: bin\Release\netstandard2.1\PELE.dll
# Destino: plugins\PELE\PELE.dll
```
