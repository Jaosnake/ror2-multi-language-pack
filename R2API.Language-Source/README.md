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

## Importante: substitui R2API.Language

Este pacote **nao deve ser instalado lado a lado** com outro
`R2API.Language.dll`.

O PELE usa o mesmo GUID do `R2API.Language` original:

```text
com.bepis.r2api.language
```

Isso e intencional. O objetivo e substituir a DLL do pacote
`RiskofThunder-R2API_Language`, mantendo compatibilidade com mods que dependem
de R2API.Language, mas adicionando o carregamento JSON, idiomas customizados,
hot reload e suporte de fonte do PELE.

Instale/deploy somente neste caminho:

```text
BepInEx/plugins/RiskofThunder-R2API_Language/R2API.Language/R2API.Language.dll
```

Nao coloque outra copia em:

```text
BepInEx/plugins/R2API.Language.dll
BepInEx/plugins/PELE/R2API.Language.dll
BepInEx/plugins/<qualquer outra pasta>/R2API.Language.dll
```

Se duas DLLs com o mesmo GUID forem carregadas, o BepInEx pode resolver o
plugin errado, carregar hooks duplicados ou quebrar a ordem de inicializacao.
O log correto deve mostrar apenas uma ocorrencia de `R2API.Language.dll`.

## Estado funcional salvo

Versao de release inicial testada no jogo em 2026-06-20:

- BepInEx carrega `R2API.Language (Jaosnake fork) 1.0.0`.
- Startup carregou `23828` tokens do PELE.
- O menu do jogo aceitou troca para `eo`, `la` e `uk`.
- `eo` e `la` usam `CultureInfo("en")` como fallback.
- `uk` usa `CultureInfo("uk")`.
- Hot reload manual por `F5` fica ativo por padrao.
- Janela de debug do PELE abre por `F6` somente se `EnableDebugMenu=true`.

## O que este fork corrige

- Evita DLL duplicada: o build so substitui a DLL original de
  `RiskofThunder-R2API_Language`.
- Intercepta `Language.SetCurrentLanguage` para idiomas customizados.
- Cria instancias de `Language` com o nome real do idioma, em vez de criar
  `en` e tentar alterar campos internos depois. Essa responsabilidade fica em
  `CustomLanguageRegistration.cs`.
- Le JSON do PELE tanto em formato plano quanto no formato nativo do RoR2
  atraves de `PeleJsonLoader.cs`:

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
- Aplica suporte nativo a fonte cirilica via `CyrillicFontSupport.cs`, usando
  primeiro `BepInEx/plugins/PELE/Fonts/cyrillicfont`.
- Valida no startup os arquivos esperados do PELE via
  `PeleStartupDiagnostics.cs`.

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

## Configuracao de release

O arquivo BepInEx gerado para este plugin permite:

```ini
[PELE]
EnableHotReload = true
EnableDebugMenu = false
EnableVerboseLogging = false
```

- `EnableHotReload`: mantem F5 e watcher de arquivos ativos.
- `EnableDebugMenu`: habilita a janela F6. Para release publica, o padrao e
  `false`.
- `EnableVerboseLogging`: habilita logs extras de hooks/layout. Para release
  publica, o padrao e `false`.

## Artefatos de release

Para publicacao/empacotamento, a pasta usada pelo `thunderstore.toml` e:

```text
ReleaseOutput/
├─ R2API.Language.dll
└─ ukrainianfont
```

`bin/`, `obj/` e `_build/` sao locais e nao entram no pacote.

## Como validar no log

Abra:

```text
BepInEx/LogOutput.log
```

Procure por:

```text
Loading [R2API.Language (Jaosnake fork) 1.0.0]
R2API.Language (Jaosnake fork) inicializado!
PELE/Language encontrado: ...
Tokens PELE por idioma: la=..., eo=..., uk=...
PELE/Fonts/cyrillicfont encontrado.
DLL R2API.Language unica detectada.
PELE JSONs carregados no startup (... tokens)
Hot-Reload habilitado! Pressione F5 para recarregar manualmente.
```

Se aparecer outro `R2API.Language.dll` carregando antes/depois deste fork,
remova a copia duplicada e deixe somente a DLL no pacote
`RiskofThunder-R2API_Language`.

## Documentacao tecnica

Antes de refatorar hooks ou UI, leia:

```text
docs/HOOKS.md
docs/MANUAL_TESTS.md
docs/STABILIZATION_PLAN.md
```

- `HOOKS.md` explica cada hook/patch e o contrato que ele precisa preservar.
- `MANUAL_TESTS.md` e o checklist de regressao manual no jogo.
- `STABILIZATION_PLAN.md` define a ordem segura para separar arquivos e limpar
  responsabilidades.

## Estrutura

```text
R2API.Language-Source/
├─ R2API.Language.csproj
├─ LanguagePlugin.cs
├─ CustomLanguageRegistration.cs
├─ CyrillicFontSupport.cs
├─ LanguagePauseMenu.cs
├─ PeleJsonLoader.cs
├─ PeleStartupDiagnostics.cs
├─ LanguageAPI.cs
├─ LanguageDebugUI.cs
├─ LanguageHotReload.cs
├─ LanguageNames.cs
├─ docs/
├─ *.cs
├─ ukrainianfont
└─ README.md
```
