# PELE stabilization plan

Objetivo: estabilizar antes de crescer. Cada fase deve terminar com build,
teste manual e commit pequeno.

## Fase 1 - Documentar e congelar comportamento atual

- Manter `docs/HOOKS.md` atualizado.
- Manter `docs/MANUAL_TESTS.md` atualizado.
- Registrar no README onde ficam os docs.
- Nao adicionar novas features sem teste manual basico.

## Fase 2 - Separar responsabilidades

Arquivos alvo:

- `CyrillicFontSupport.cs`
  - status: feito
  - suporte a `PELE/Fonts/cyrillicfont`
  - patch de `TextMeshProUGUI.LoadFontAsset`
  - aplicacao/restauracao de fonte

- `CustomLanguageRegistration.cs`
  - status: pendente
  - mover `FindLanguageByNamePostfix`
  - mover criacao de `Language`
  - mover `SetCurrentLanguagePrefix`

- `PeleJsonLoader.cs`
  - status: feito
  - `ReloadPeleJsonFiles` fica como fachada em `LanguagePlugin`
  - parsing de JSON plano e `strings`
  - contagem de tokens PELE

- `LanguagePauseMenu.cs`
  - status: parcial
  - manter/renomear `LanguagePauseButton`
  - deixar somente UI pause/dialogo/legenda

Regra: mover codigo sem alterar comportamento primeiro. Refatorar logica depois.

## Fase 3 - Sanity checks de startup

Criar um validador leve:

- status: feito em `PeleStartupDiagnostics.cs`

- Verificar `PELE/Language`.
- Verificar `PELE/Language/la/language.json`.
- Verificar `PELE/Language/eo/language.json`.
- Verificar `PELE/Language/uk/language.json`.
- Verificar `icon.png` dos idiomas custom.
- Verificar `PELE/Fonts/cyrillicfont`.
- Logar avisos claros e acionaveis.

## Fase 4 - Logs sem spam

Padronizar logs:

- Uma linha para pasta PELE registrada. Status: feito.
- Uma linha para total de tokens por idioma. Status: feito.
- Uma linha para fonte usada. Status: feito.
- Uma linha para DLL/deploy. Status: feito para DLL unica.
- Avisos com prefixo claro quando algo faltar.

Evitar logs por frame, por texto ou por evento repetido.

## Fase 5 - Ferramentas de traducao

Adicionar comandos/validadores offline:

- tokens duplicados por idioma.
- tokens faltando entre idiomas.
- placeholder mismatch: `{0}`, `{1}`.
- tags quebradas: `<style=...>`, `<color=...>`.
- encoding invalido.
- arquivos `.language` e JSON misturados.

## Fase 6 - Avaliar troca/expansao de hook framework

So considerar MonoDetour ou outro framework se:

- Harmony/On hooks ficarem dificeis de rastrear.
- Precisarmos de muitos IL hooks.
- Patches por tela virarem grupos grandes.
- Updates do RoR2 quebrarem os mesmos hooks repetidamente.

Antes disso, manter Harmony/On por simplicidade.
