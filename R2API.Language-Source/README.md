# R2API.Language - Jaosnake fork

Fork do R2API.Language com suporte PELE (idiomas customizados) e hot-reload.

PELE foi incorporado neste único plugin. Dados de tradução permanecem em `BepInEx/plugins/PELE/Language/`.

## Funcionalidades

- **Hot-Reload**: edite `.language` ou `.json` (PELE) e veja mudanças sem reiniciar
- **Idiomas customizados**: `la`, `eo`, `uk` registrados automaticamente
- **Fallback CultureInfo**: idiomas sem cultura real (`la`, `eo`) usam `"en"`
- **Fonte Ukraniana**: carregada de AssetBundle embarcado
- **Validador**, **Detector de duplicatas**, **Analisador de uso**, **Compilador**
- **F5**: recarrega manualmente todos os arquivos
- **Thread-safe**: `ReaderWriterLockSlim` em todos os dicionários

## Build

```cmd
dotnet build --configuration Release
```

DLL em `bin/Release/netstandard2.1/R2API.Language.dll`
