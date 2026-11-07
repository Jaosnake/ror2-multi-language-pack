# Instruções atuais — traduções RoR2 e P.E.L.E

Revisado em 03/11/2026 por leitura do código local e inspeção do ZIP P.E.L.E 1.0.4. Esta documentação distingue código fonte, pacote distribuído e execução; não declara que o código local foi compilado ou que todas as funções foram testadas no jogo.

## Regras que devem ser lidas antes de trabalhar

- `E:/Revisão RoR2 Traducao/_relatorios_auditoria/REGRAS_TRADUCAO_ROR2.md`, sobretudo seções 12 e 13.
- `E:/Revisão RoR2 Traducao/_relatorios_auditoria/GUIA_VERIFICACAO_INDEPENDENTE_TRADUCOES.md` para identificação de fontes, extração e confronto de traduções.
- `E:/Revisão RoR2 Traducao/_relatorios_auditoria/ACOMPANHAMENTO_ENVIO_CODEX.md` para estado de pacotes e decisões do usuário.

As instruções atuais do usuário prevalecem. Não publicar, fazer push ou alterar pacotes em `ja enviados` sem instrução explícita. Para tradução use o ícone padrão e descrição curta das regras mestras; P.E.L.E, C.E.N.I e Sandswept têm ícones próprios. P.E.L.E é obrigatório para Goku, Vegeta e Trunks; para os demais pacotes é opcional, necessário para ucraniano, esperanto e latim conforme instrução do usuário. Não iniciar nem retomar tarefas pausadas por conta deste documento.

O antigo AGENTS foi preservado como `../_relatorios_auditoria/AGENTS_HISTORICO_ANTES_AUDITORIA_ARQUITETURA_20261103.md`. Suas contagens, inventários, caminhos de build e planos são históricos; não devem orientar alterações sem nova verificação. Não usar UK, traduções anteriores ou blobs como única fonte original. Nomes e tokens devem ser confirmados nos arquivos oficiais e no fluxo real de registro, inclusive concatenações.

## Arquitetura identificada no código local

| Componente | Evidência | Comportamento identificado |
|---|---|---|
| Núcleo de linguagem | `R2API.Language-Source/LanguageAPI.cs`, `LanguagePlugin.cs` | Fork em `R2API.Language.dll`, GUID `com.bepis.r2api.language`, versão de plugin declarada `2.0.0`; ID Harmony `jaosnake.r2api.language`. |
| Integração Dragon Ball | `PELE.KingDBZFix/KingDBZFixPlugin.cs` | DLL **separada** `PELE.KingDBZFix.dll`, plugin `jaosnake.pele.kingdbzfix` versão declarada `1.0.0`; patches de menus, HUD, tutoriais e formatação para Goku, Vegeta e Trunks. |
| Idiomas adicionais | `CustomLanguageRegistration.cs` | `la`, `eo`, `uk`, criação lazy em `FindLanguageByName` e interceptação de `SetCurrentLanguage`. |
| Caminhos/alias | `PelePaths.cs` | Busca raízes direta `plugins/PELE/Language` e alternativa `plugins/Jaosnake-PELE/PELE/Language`; alias bidirecional `uk`/`UA` na consulta de tokens. |
| JSON P.E.L.E | `PeleJsonLoader.cs` | Lê JSONs de cada pasta de idioma; aceita objeto `strings` ou pares planos com exclusão de metadados; registra em `CustomLanguage`. |
| Fontes | `CyrillicFontSupport.cs` | Tenta bundle cirílico externo, fonte de outro assembly compatível e recurso embutido como fallback. Não presumir que apenas a fonte embutida basta em toda instalação. |
| Recarga e diagnóstico | `LanguageHotReload.cs`, `LanguagePlugin.cs`, `ConsoleCommands.cs`, `LanguageDebugUI.cs` | Watchers, F5 e comandos `reloadlang`, `langdebug`, `langstatus`; janela de debug com F6, quando criada/habilitada. |

O ZIP local `Jaosnake-PELE-1.0.4.zip` contém as duas DLLs, além de fontes externas. `1.0.4` é a versão do pacote, `2.0.0` é a constante de versão do plugin de linguagem e `1.1.0.0` é o AssemblyVersion no projeto C#: esses números têm funções distintas. Não trocar um pelo outro automaticamente.

A DLL KingDBZFix declara dependência BepInEx **HardDependency** de `com.Kingpinush.KingModUtilities`. Existe também uma verificação dentro de Awake cujo comentário diz soft dependency; o atributo é a evidência de dependência rígida. Sem KingModUtilities, o carregador pode não carregar esse plugin separado; não interpretar isso como necessidade de KingModUtilities para o núcleo de idiomas. A confirmação do efeito no jogo exige log/runtime. As traduções próprias dos três personagens continuam sendo pacotes separados.

## Cultura, idioma e precedência

`FixCultureName` converte `la`/`eo` em `en` para `CultureInfo`. O código de `SetCurrentLanguage` mantém o idioma solicitado como idioma do jogo e carrega suas strings. Portanto, **fallback cultural não significa trocar a interface inteira para inglês**. `uk` passa sem esse fallback cultural. O alias `UA`/`uk` em `PelePaths` é usado na consulta de tokens; não presumir que todo ponto de entrada normalize os dois códigos da mesma maneira.

Em `Language_GetLocalizedStringByToken`, a ordem identificada é `OverlayLanguage`, depois `CustomLanguage`, depois consulta original. Cada dicionário tenta idioma, alias e `generic`. Os JSONs de `PeleJsonLoader` e arquivos `.language` podem escrever no mesmo dicionário de tokens; podem substituir textos conforme o fluxo de carga. É incorreto garantir que não interfiram entre si. Investigar duplicatas, ordem real e recarga antes de sincronizar traduções. Não presumir ordenação estável de arquivos retornados pelo sistema.

## Formatos e carga

Para pacotes novos, preservar o formato comprovado do carregador da versão alvo. Nos pacotes JSON `.language`, indicar explicitamente o idioma, por exemplo:

```json
{"pt-BR": {"TOKEN_CONFIRMADO": "Texto traduzido"}}
```

`strings` é tratado como `generic` no parser de `.language` do código local; não representa automaticamente o idioma do nome do arquivo. Nos JSONs P.E.L.E a pasta define o idioma, por exemplo `PELE/Language/eo/arquivo.json` com `{"strings": {...}}`.

**Atenção ao formato chave=valor:** `LanguageFileHelper` tem parser simples e dedução por nome, mas o caminho inicial `LanguageAPI.AddPath -> Add -> LoadFile` usa JSON; a recarga por `ReloadAllLanguageFiles` usa o helper. Não afirmar compatibilidade universal de chave=valor só porque um parser existe. Não converter Starstorm2 ou Sandswept com base em descrições históricas: abrir os arquivos atuais e seguir seus carregadores e formatação dinâmica, especialmente MSU e LookingGlass.

Use UTF-8 sem BOM, JSON estrito sem duplicatas, preserve significado de placeholders, tags, cores e nomes conforme as regras mestras. Validadores devem distinguir sintaxe, cobertura, semântica e teste no jogo. Não deduzir que um idioma está completo por contagem ou paridade.

## Recarga, comandos e teste

`EnableHotReload` tem padrão true no código; F5 recarrega arquivos e JSONs P.E.L.E. Os watchers observam `.language` e JSONs na raiz P.E.L.E encontrada e usam debounce. `EnableDebugMenu` tem padrão false; F6 não comprova ausência do plugin se a janela não foi criada. Os comandos são registrados como `reloadlang`, `langdebug`, `langstatus`; o prefixo e acesso ao console dependem da interface utilizada. Verificar logs do registro e respostas reais, sem presumir que um atalho habilita o console do jogo.

Recarga de textos não substitui reinicialização para testar DLL atualizada. Após alteração, testar o idioma e a interface afetada, placeholders, destaques e conflitos; registrar se o teste foi executado. O usuário controla a tela e a inicialização quando assim solicitar.

## Build e instalação: identificar antes de executar

`R2API.Language-Source/R2API.Language.csproj` usa netstandard2.1, assembly `R2API.Language`, AssemblyVersion `1.1.0.0`. Seus caminhos GameManagedPath, referências e ProfilePluginPath contêm caminhos locais antigos, inclusive `C:/Users/Jaosnake/.../r2modmanPlus-local`. O target `DeployAfterBuild` copia automaticamente a DLL para ProfilePluginPath. **Antes de compilar, conferir e ajustar conscientemente esses caminhos e o destino de deploy dentro do escopo autorizado.** Não executar comandos históricos de build/deploy sem ler o projeto.

O perfil conhecido do usuário fica em `C:/Users/jaosn/AppData/Roaming/Thunderstore Mod Manager/DataFolder/RiskOfRain2/profiles/Default`. Encontrar arquivos reais recursivamente e conferir GUID/hash para identificar qual DLL é carregada; não instalar cópia adicional do núcleo com o mesmo GUID. Preservar backups fora das pastas de plugins.

Na inspeção de 03/11/2026, o gerenciador instalou o pacote em `plugins/Jaosnake-PELE/plugins/...`. Esse caminho não corresponde literalmente às duas raízes de JSON listadas no `PelePaths.cs` local. Isso é uma diferença a investigar, não prova de que o jogo falha nem autorização para mover arquivos sem avaliar a DLL carregada. As DLLs instaladas conferem por hash com o ZIP 1.0.4; os artefatos locais `_build` e `ReleaseOutput` têm hashes diferentes do núcleo no ZIP. Não assumir equivalência entre fonte, build e distribuição, nem alegar que esta auditoria validou o binário inteiro.

## Inventários e evidências

O código local contém 18 arquivos C# no diretório principal `R2API.Language-Source`, não os 13 do guia antigo. Em `PELE/Language`, a contagem observada é 83 JSONs em cada pasta eo, la e uk, e 4 em cada pasta pt-BR e tr. São contagens de arquivos da cópia de trabalho, não comprovação de cobertura, validade, qualidade ou conteúdo distribuído no ZIP.

Para hashes, manifesto do ZIP e fontes lidas, consultar `../_relatorios_auditoria/EVIDENCIAS_AUDITORIA_GUIAS_ARQUITETURA_20261103.json` e `../_relatorios_auditoria/RELATORIO_AUDITORIA_GUIAS_ARQUITETURA_20261103.md`. Recalcular após mudanças. Registros antigos de “100% traduzido” e “não tocar nestes mods” não substituem as instruções atuais do usuário nem uma auditoria baseada nos arquivos oficiais.

## Regra obrigatória: dependências antes de instalar para teste

Siga as seções 14 e 15 de `../_relatorios_auditoria/REGRAS_TRADUCAO_ROR2.md` e a seção 6 de `../_relatorios_auditoria/GUIA_VERIFICACAO_INDEPENDENTE_TRADUCOES.md`. Antes de instalar, confira manifestos oficiais e dependências transitivas contra DLLs e patchers realmente presentes, habilitados e compatíveis no perfil; reutilize os existentes, evitando duplicação. Preserve a estrutura completa do ZIP, inclusive `BepInEx/patchers`, e não sobrescreva componentes compartilhados sem investigar dependentes e criar backup. Após reiniciar, confirme no log carregamento/aplicação de patchers e plugins, erros e hooks duplicados; teste as funções afetadas e registre evidência e limites.

Em 04/11/2026, a ausência de `R2API.Skills.Patcher.dll` causou abas de habilidades vazias e interrompeu a inicialização do Sandswept. DLLs R2API.Language original e fork P.E.L.E com a mesma identidade instalaram hooks concorrentes; isolar reversivelmente a cópia original no perfil teste foi seguido de exibição em ucraniano. Isso não comprova compatibilidade geral. Nunca suponha que um plugin ignorado pelo BepInEx não tenha sua assembly carregada por outro mod.

Ainda no Sandswept, textos em ucraniano carregaram mas vários efeitos e histórias eram incompatíveis com o original; confronte com o código/IL oficial, por token e idioma, incluindo valores configuráveis. Colocar `{0}` em um texto substituído após formatação pode exibir marcadores; não congelar os valores padrão como se fossem dinâmicos. Ensaios isolados, revisão semântica e testes em jogo são verificações distintas. Consulte o acompanhamento e o inventário de divergências antes de preparar pacotes.
