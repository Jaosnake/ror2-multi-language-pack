# PELE/R2API.Language hook map

Este documento registra os pontos em que o PELE altera o comportamento do RoR2.
Antes de refatorar qualquer arquivo abaixo, confirme qual contrato do hook esta
sendo preservado.

## LanguageAPI.cs

### `On.RoR2.Language.GetLocalizedStringByToken`

Objetivo: dar prioridade para tokens carregados pelo PELE antes do fallback
original do jogo.

Contrato:
- Procurar primeiro em overlays temporarios.
- Procurar depois em tokens customizados do PELE.
- Usar o idioma atual de `self.name`.
- Cair no metodo original se o PELE nao tiver o token.

Risco:
- Se este hook falhar, traducoes do PELE deixam de sobrescrever traducoes dos
  mods/originais.

### `On.RoR2.Language.TokenIsRegistered`

Objetivo: fazer o jogo reconhecer tokens PELE como tokens validos.

Contrato:
- Retornar `true` para tokens existentes em overlay ou em `CustomLanguage`.
- Cair no metodo original para tokens que o PELE nao conhece.

Risco:
- UI/mods podem tratar textos PELE como ausentes se o hook for removido.

## LanguagePlugin.cs

### `Language.collectLanguageRootFolders`

Objetivo: registrar `BepInEx/plugins/PELE/Language` como raiz nativa de idiomas
do RoR2.

Contrato:
- Adicionar a pasta PELE somente se ela existir.
- Nao remover outras pastas registradas por outros mods.
- Desregistrar no `OnDisable`/`OnDestroy`.

Risco:
- Se registrar tarde demais, `LanguageConVar` pode tentar carregar `pt-BR` ou
  custom languages antes da pasta existir e cair para `en`.

### `HarmonyPostfix Language.FindLanguageByName`

Objetivo: criar `Language` para idiomas customizados (`la`, `eo`, `uk`) quando
o jogo ainda nao os conhece.

Contrato:
- So agir se `__result` for `null`.
- So criar idiomas em `CustomLangs`.
- Configurar pastas com `SetFolders` se `PELE/Language/<lang>` existir.
- Preservar o `name` real do idioma.

Risco:
- Sem este hook, `LanguageConVar.SetString("uk")` pode rejeitar o idioma custom.

### `HarmonyPrefix Language.SetCurrentLanguage`

Objetivo: permitir troca para idiomas customizados com cultura fallback correta.

Contrato:
- Deixar o metodo vanilla rodar para idiomas nativos.
- Interceptar apenas `la`, `eo`, `uk`.
- Atualizar `currentLanguageName` e `currentLanguage`.
- Usar `CultureInfo("en")` para `la` e `eo`.
- Usar `CultureInfo("uk")` para `uk`.
- Carregar strings e disparar `onCurrentLanguageChanged`.
- Retornar `false` somente quando a troca custom foi concluida.

Risco:
- Erro aqui pode travar troca de idioma, quebrar UI atual ou deixar o seletor
  vanilla mostrando idioma errado.

## CyrillicFontSupport.cs

### `HarmonyPostfix HGTextMeshProUGUI.OnCurrentLanguageChanged`

Objetivo: aplicar fonte custom/fonte cirilica quando o idioma precisa.

Contrato:
- Guardar a fonte vanilla antes de sobrescrever.
- Para `uk`, usar a fonte cirilica do PELE quando existir.
- Para outros idiomas, restaurar a fonte vanilla.
- Reaplicar fonte nos textos ativos apos troca.

Risco:
- Sobrescrever fonte de outro mod pode gerar `?` em cirilico.
- Aplicar fonte em textos errados pode quebrar glyphs de input.

### `HarmonyPostfix TextMeshProUGUI.LoadFontAsset`

Objetivo: substituir textos novos que usam Bombardier por uma fonte com
caracteres cirilicos.

Contrato:
- So trocar se a fonte atual for a Bombardier/default.
- Usar primeiro `PELE/Fonts/cyrillicfont`.
- Nao depender de `AnotherOneCyrillicFont`.

Risco:
- Se for agressivo demais, pode trocar fontes especiais de outros mods.

## PeleJsonLoader.cs

### `Reload`

Objetivo: carregar os JSONs do PELE por idioma e registrar tokens no
`LanguageAPI`.

Contrato:
- Ler `BepInEx/plugins/PELE/Language/<lang>/*.json`.
- Aceitar JSON plano e JSON com objeto `strings`.
- Ignorar metadados `language`, `strings` e chaves iniciadas por `_`.
- Nao impedir o startup se um arquivo especifico estiver invalido.

Risco:
- Erro aqui remove as traducoes PELE, mas nao deve quebrar o carregamento de
`.language` dos mods.

### `CountTokens`

Objetivo: gerar resumo de startup sem alterar estado.

Contrato:
- Contar tokens usando a mesma regra de manifest do `Reload`.
- Silenciar erro de contagem isolado; erros detalhados pertencem ao reload.

## PeleStartupDiagnostics.cs

### `LogOnce`

Objetivo: dar um snapshot acionavel do ambiente PELE no startup.

Contrato:
- Rodar uma vez por ciclo do plugin.
- Verificar `PELE/Language`, `language.json`, `icon.png` e
  `PELE/Fonts/cyrillicfont`.
- Avisar se houver mais de uma `R2API.Language.dll`.
- Nao modificar arquivos nem registrar tokens.

Risco:
- Se ficar barulhento demais, o log perde utilidade para diagnostico real.

## LanguagePauseButton.cs

### `On.RoR2.UI.PauseScreenController.Awake`

Objetivo: adicionar botao Language no menu pause.

Contrato:
- Clonar um botao existente para manter estilo/navegacao.
- Usar token localizado `PELE_LANGUAGE_BUTTON`.
- Registrar callback para abrir o dialogo PELE.
- Nao criar botoes duplicados em um mesmo painel.

Risco:
- Hierarquia do pause menu pode mudar em update do jogo/mods.

### `SimpleDialogBox.Create`

Objetivo: mostrar grid de idiomas no pause.

Contrato:
- Usar `SimpleDialogBox.Create(EventSystem.current as MPEventSystem)`.
- Destruir `dialog.rootObject`, nao apenas `dialog.gameObject`.
- Fechar com `Esc` ou botao B sem deixar overlay escuro travado.
- Botoes de idioma devem aplicar imediatamente.

Risco:
- Destruir o objeto errado deixa a tela escura e bloqueada.

### `InputBindingDisplayController`

Objetivo: mostrar atalhos dinamicos de controle abaixo do dialogo.

Contrato:
- Usar actions do jogo (`UISubmit`, `UICancel`) para gamepad.
- Para teclado/mouse, mostrar texto fixo/localizado (`Clique esquerdo`, `Esc`).
- A legenda fica fora da caixa do dialogo.

Risco:
- `UISubmit` em teclado aparece como Return; por isso teclado/mouse nao deve
  depender desse glyph.

## ConsoleCommands.cs

### `On.RoR2.Console.InitConVarsCoroutine`

Objetivo: registrar comandos PELE apos a inicializacao do console.

Contrato:
- Rodar `orig`.
- Registrar comandos uma vez.
- Nao quebrar o init do console se reflection falhar.

Risco:
- Falha aqui remove `/reloadlang`, `/langdebug`, `/langstatus`, mas nao deve
  impedir o jogo de iniciar.

## LanguageHotReload.cs

### `FileSystemWatcher`

Objetivo: detectar alteracoes em `.language` e `PELE/Language/**/*.json`.

Contrato:
- Debounce para evitar reload parcial.
- `F5` deve continuar funcionando mesmo se watcher falhar.
- Hot reload deve recarregar LanguageAPI e JSONs do PELE.

Risco:
- Reload no momento errado pode limpar tokens temporariamente ou reentrar em
  locks.
