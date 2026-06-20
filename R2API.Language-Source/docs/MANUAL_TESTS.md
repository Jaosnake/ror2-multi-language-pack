# PELE manual regression checklist

Use este checklist depois de qualquer mudanca em hooks, fontes, hot reload ou UI.

## Setup

- Iniciar pelo perfil r2modman `Default`.
- Confirmar que existe apenas uma `R2API.Language.dll` carregada.
- Confirmar que `BepInEx/plugins/PELE/Language` existe.
- Confirmar que `BepInEx/plugins/PELE/Fonts/cyrillicfont` existe.

## Startup log

Validar em `BepInEx/LogOutput.log`:

- `Loading [R2API.Language (Jaosnake fork) ...]`
- `R2API.Language (Jaosnake fork) inicializado!`
- `Pasta PELE/Language registrada: ...`
- `PELE/Language encontrado: ...`
- `Tokens PELE por idioma: la=..., eo=..., uk=...`
- `PELE/Fonts/cyrillicfont encontrado.`
- `DLL R2API.Language unica detectada.`
- `PELE JSONs carregados no startup (... tokens)`
- `Hot-Reload habilitado!`
- Se testar ucraniano: `Fonte cirilica carregada de bundle PELE.` ou `Fonte cirilica do PELE ativa.`

## Main menu language selector

Testar no seletor vanilla do canto superior:

- `pt-BR` mostra bandeira do Brasil e texto correto.
- `la` mostra bandeira do Vaticano e `Lingua Latina`.
- `eo` mostra bandeira do Esperanto e `Esperanto`.
- `uk` mostra bandeira da Ucrania e `Українська` sem `?`.
- O idioma selecionado no topo acompanha a troca.

## Pause menu Language

Abrir menu pause e clicar em `Language`/texto localizado.

Validar:

- O botao aparece traduzido no idioma atual.
- O dialogo abre sem excecao.
- O grid contem apenas idiomas, sem botao `Cancelar` dentro.
- A legenda de atalhos fica fora da caixa, abaixo dela.
- Teclado/mouse mostra `Clique esquerdo`/equivalente + `Esc`.
- Controle mostra glyphs reais do controle para aplicar/cancelar.
- Trocar para `pt-BR`, `la`, `eo`, `uk` aplica imediatamente.
- Fechar com `Esc` nao deixa tela escura travada.
- Fechar com botao B nao deixa tela escura travada.

## Hot reload

- Alterar um JSON pequeno em `PELE/Language/pt-BR`.
- Pressionar `F5`.
- Confirmar no log que o reload concluiu.
- Confirmar que o texto atualizado aparece sem reiniciar o jogo.

## Debug UI

- Pressionar `F6`.
- Confirmar que a janela aparece.
- Testar troca de idioma pela janela.
- Fechar/reabrir sem travar input.

## Character select / modded tokens

- Abrir selecao de personagens.
- Verificar tokens de personagens/mods em `pt-BR`.
- Trocar para `uk`.
- Separar problemas:
  - Se so texto de mod tem `?`, revisar traducao/encoding do arquivo desse mod.
  - Se UI base/seletor tem `?`, revisar fonte/hook do PELE.

## Failure signs

Investigar se ocorrer:

- Tela escura permanente apos fechar dialogo.
- Duas DLLs `R2API.Language.dll` no log.
- `Could not load files for language ... Falling back to "en"` apos PELE registrar pasta.
- `AssetBundle ... already loaded` repetindo.
- `MPEventSystem is invalid` vindo da legenda de input.
- Bandeiras ausentes no seletor vanilla.
