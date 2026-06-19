# R2API.Language - Jaosnake PELE fork

Esta pasta guarda a versao funcional do fork de `R2API.Language` usado pelo
PELE. O objetivo deste fork e manter o carregamento normal de `.language` dos
mods e, ao mesmo tempo, habilitar idiomas customizados do PELE via JSON:

```text
eo - Esperanto
la - Latin
uk - Ukrainian
```

As traducoes ficam fora deste projeto de codigo, em:

```text
BepInEx/plugins/PELE/Language/<idioma>/*.json
```

No repositorio, esses arquivos estao em:

```text
../PELE/Language/
```

## Estado funcional salvo

Versao testada no jogo em 2026-06-19:

- BepInEx carregou `R2API.Language (Jaosnake fork) 2.1.1`.
- Startup carregou `23828` tokens do PELE.
- O menu do jogo aceitou troca para `eo`, `la` e `uk`.
- `eo` e `la` usam `CultureInfo("en")` como fallback.
- `uk` usa `CultureInfo("uk")`.
- Hot reload manual por `F5` fica ativo.
- Janela de debug do PELE abre por `F6`.

## O que este fork corrige

- Evita DLL duplicada: o build so substitui a DLL original de
  `RiskofThunder-R2API_Language`.
- Intercepta `Language.SetCurrentLanguage` para idiomas customizados.
- Cria instancias de `Language` com o nome real do idioma, em vez de criar
  `en` e tentar alterar campos internos depois.
- Le JSON do PELE tanto em formato plano quanto no formato nativo do RoR2:

```json
{
  "strings": {
    "TOKEN": "Texto"
  }
}
```

- Ignora metadados como `language`, `strings` e chaves iniciadas por `_` na
  contagem de tokens.
- Remove deadlock no overlay de linguagem ao evitar reentrada no mesmo
  `ReaderWriterLockSlim`.
- Mantem o lock estatico vivo no shutdown para evitar erro tardio de unload.

## Build local

Requisitos esperados nesta maquina:

```text
Risk of Rain 2:
C:\Program Files (x86)\Steam\steamapps\common\Risk of Rain 2

r2modman profile:
C:\Users\Jaosnake\AppData\Roaming\r2modmanPlus-local\RiskOfRain2\profiles\Default
```

Compilar:

```powershell
dotnet build C:\Users\Jaosnake\Desktop\PELE_Project\github_repo_latest\R2API.Language-Source\R2API.Language.csproj -c Release
```

O `.csproj` tem um alvo `DeployAfterBuild` que copia a DLL gerada para:

```text
BepInEx/plugins/RiskofThunder-R2API_Language/R2API.Language/R2API.Language.dll
```

Isto e intencional. Nao coloque outra copia de `R2API.Language.dll` solta em
`BepInEx/plugins`, porque BepInEx pode resolver dois plugins com o mesmo GUID.

## Arquivo precompilado

Depois do build, mantenha uma copia versionada em:

```text
../_build/R2API.Language.dll
```

Essa copia serve como artefato rapido para recuperar a versao funcional sem
recompilar.

## Como validar no log

Abra:

```text
BepInEx/LogOutput.log
```

Procure por:

```text
Loading [R2API.Language (Jaosnake fork) 2.1.1]
R2API.Language (Jaosnake fork) inicializado!
PELE JSONs carregados no startup (... tokens)
Hot-Reload habilitado! Pressione F5 para recarregar manualmente.
SetCurrentLanguage interceptado: 'eo'
SetCurrentLanguage interceptado: 'la'
SetCurrentLanguage interceptado: 'uk'
```

Se aparecer outro `R2API.Language.dll` carregando antes/depois deste fork,
remova a copia duplicada e deixe somente a DLL no pacote
`RiskofThunder-R2API_Language`.

## Estrutura

```text
R2API.Language-Source/
├─ R2API.Language.csproj
├─ LanguagePlugin.cs
├─ LanguageAPI.cs
├─ LanguageDebugUI.cs
├─ LanguageHotReload.cs
├─ LanguageNames.cs
├─ *.cs
├─ ukrainianfont
└─ README.md
```
