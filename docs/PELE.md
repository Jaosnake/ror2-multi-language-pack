# Manual do P.E.L.E.

O **P.E.L.E. (Plugin for Enhanced Language Extension)** amplia a localização de Risk of Rain 2. Ele registra idiomas adicionais no jogo, carrega traduções, fornece fontes e adapta a interface para que o jogador possa mudar de idioma inclusive pelo menu de pausa. Os pacotes de tradução fornecem o texto de cada jogo ou mod. O P.E.L.E. fornece a infraestrutura para exibi-lo.

## 1. Qual versão está em uso

| Versão | Recursos cobertos por este manual |
| --- | --- |
| **1.0.9 pública** | Ucraniano, esperanto, latim, árabe padrão moderno, hindi e tailandês. Usa o pacote oficial `RiskofThunder-R2API_Language`. |
| **1.0.10 preparada para envio** | Mantém os seis idiomas e acrescenta suporte piloto a grego e vietnamita, com fontes, bandeiras, alguns textos do jogo base e uma amostra de três itens em um pack separado de Starstorm 2. Inclui as quatro melhorias da seção 6. |

O número do manifesto de uma compilação local pode coincidir com o da versão pública. Para identificar o que realmente roda, confira o ZIP de origem, o hash da DLL e o perfil no `BepInEx/LogOutput.log`. A lista do gerenciador, sozinha, não distingue os dois binários.

O [ZIP 1.0.10 preparado para envio](../mods/PELE/Jaosnake-PELE-1.0.10-THUNDERSTORE.zip) e as [instruções de compilação](../PELE/Source/README.md) estão neste repositório.

Este manual fica em `docs/PELE.md`, fora da árvore de arquivos do pacote. Mantenha-o no repositório e confira que ele está ausente do ZIP de distribuição.

## 2. Como usar

1. Instale o P.E.L.E. e as dependências declaradas no manifesto pelo gerenciador. Mantenha o R2API.Language oficial habilitado.
2. Instale o mod que deseja traduzir e o respectivo LanguagePack. Por exemplo, Sandswept e Starstorm 2 têm packs próprios. A versão piloto de Starstorm 2 contém Coffee Bag, Guarding Amulet e Relic of Mass em grego e vietnamita.
3. Escolha o idioma no seletor do menu principal ou use **Esc → Idioma**. Algumas telas podem mostrar os caracteres temporários de fonte enquanto o texto é preparado. A cobertura depende do pack e da versão.
4. Para conferir um texto durante o jogo, abra a tela onde ele aparece de verdade. Um token presente no disco ainda pode depender de formatação dinâmica, da fonte, do espaço disponível no HUD ou do carregador do mod.

Para idiomas nativos como PT-BR, um pack que use apenas o formato compatível com R2API.Language pode funcionar com o R2API.Language oficial, seguindo as dependências do próprio pack. Para os idiomas adicionais, o P.E.L.E. disponibiliza a seleção e o suporte de exibição. Goku, Vegeta e Trunks usam a integração separada `PELE.KingDBZFix.dll` para seus menus, HUD e tutoriais, além dos respectivos packs.

## 3. Como o texto chega à tela

1. O jogo ou o mod consulta uma **chave**, também chamada de **token**, como `PAUSE_RESUME`.
2. O sistema resolve essa chave no idioma atual com as traduções registradas, conforme a precedência aplicável. O R2API.Language oficial carrega arquivos `.language`. O núcleo do P.E.L.E. lê seus JSONs e registra seus tokens em uma camada de traduções temporária, chamada *overlay*, pela API oficial.
3. A integração da refatoração opera durante a execução, dentro da consulta do R2API.Language, preservando a oportunidade de mods como MysticsRisky2Utils formatarem o resultado. A instalação usa a DLL oficial intacta no disco. Preserve esse comportamento em qualquer alteração de compatibilidade.
4. O P.E.L.E. registra os idiomas extras e resolve o alias ucraniano `uk`/ `UA` nas consultas integradas. Alguns idiomas usam uma cultura alternativa para formatação de números ou datas. A cultura de formatação é independente do idioma exibido.
5. A interface usa as fontes do jogo e fontes alternativas conforme a escrita. Quando um texto de árabe, hindi ou tailandês requer composição adicional, o módulo de apresentação chama o ShapeText.

Chaves repetidas em JSONs do mesmo idioma seguem a ordem alfabética dos caminhos, com a última definição válida vencendo dentro dessa origem. A resolução final ainda depende das camadas do R2API, do jogo e do mod. Se um pack separado já mantém a tradução de um mod, evite duplicá-la no JSON interno do P.E.L.E. Use `langsource` na 1.0.10 para investigar.

## 4. Como criar arquivos de tradução

### JSON do P.E.L.E.

Use UTF-8 sem BOM, JSON válido e chaves únicas. O diretório determina o idioma. Use um nome que identifique a origem, como `MeuMod.json`, e agrupe os pares em `strings`. O exemplo abaixo usa um token do próprio jogo para um teste isolado.

```text
BepInEx/plugins/<pasta-do-pacote-PELE>/PELE/Language/vi/MeuTeste.json
```

```json
{
  "strings": {
    "PAUSE_RESUME": "Tiếp tục"
  }
}
```

Na refatoração, a primeira raiz procurada é `<pasta-do-pacote-PELE>/PELE/Language`, calculada a partir da localização da DLL do núcleo. As alternativas compatíveis são `BepInEx/plugins/PELE/Language` e `BepInEx/plugins/Jaosnake-PELE/PELE/Language`. O carregador usa uma raiz por vez, lê os JSONs diretamente em cada subpasta de idioma e usa a primeira raiz reconhecida. Colocar um JSON arbitrário em outra pasta de plugins não o adiciona a esse fluxo.

Idiomas do P.E.L.E. na refatoração: `uk`, `eo`, `la`, `ar`, `hi`, `th`, `el` e `vi`. Criar uma pasta com outro código adiciona arquivos ao disco. Para disponibilizar um novo idioma no seletor, é necessário registrar o código, fornecer nome e verificar fontes, bandeiras, formatação e troca para os idiomas existentes.

### Arquivo `.language` do R2API.Language

Use a raiz com o código explícito. O nome do arquivo serve para identificar o idioma e o mod para manutenção.

```text
BepInEx/plugins/<pasta-do-LanguagePack>/MeuMod_pt-BR.language
```

```json
{
  "pt-BR": {
    "MEUMOD_ITEM_EXEMPLO_NAME": "Amplificador",
    "MEUMOD_ITEM_EXEMPLO_DESC": "Aumenta o dano em <style=cIsDamage>{0}%</style>."
  }
}
```

Confirme as chaves em uma fonte oficial da versão alvo ou no código de registro do mod. As chaves `MEUMOD_...` acima são ilustrativas. Alguns mods gerenciam seus próprios arquivos e valores. Starstorm 2 usa Moonstorm Shared Utils (MSU), e seus arquivos e marcadores devem seguir o caminho de carga e formatação do mod. O F5 da refatoração recarrega os JSONs do P.E.L.E. Para testar `.language`, arquivos de carregadores próprios, DLLs ou fontes recém-instaladas, reinicie o jogo e confira o log.

### Tags, marcadores e pontuação

- Preserve marcadores como `{0}`, `{MoveSpeed}` e `%FEATHERKEY%` conforme a fonte e o formatador. Preserve o significado, a quantidade e as unidades de cada valor. Um número observado no jogo pode ser configurável.
- Preserve e confira tags como `<style=cIsDamage>…</style>`, `<color=…>…</color>`, `<size=…>…</size>`, `<link=…>…</link>`, `<mark=…>…</mark>` e `<sprite name="TP" tint=1>`. Em JSON, escreva as aspas internas como `\"`. Um `<sprite>` é autônomo. Tags que envolvem texto precisam de fechamento e aninhamento corretos. A fidelidade visual e a interatividade precisam ser conferidas na tela.
- Guarde árabe na ordem de leitura normal do texto, com a pontuação original necessária. O renderizador cuida da direção visual. Preserve marcas vocálicas e combinações do hindi e do tailandês. Retenha as interrogações intencionais de Void Fiend.

## 5. Para que serve o ShapeText

`ShapeText.exe` é um processo auxiliar do pacote que prepara a apresentação de escritas complexas. Ele compõe as letras e as marcas, trata a direção do árabe e gera uma imagem de texto ajustada à área do controle. O módulo `LanguageSupport.dll` envia pelo canal de entrada o texto já resolvido, com medidas, tamanho de fonte, alinhamento e marcações. O executável devolve o resultado pelo canal de saída. O jogo exibe o resultado sobre o controle e reaproveita imagens do cache.

O executável trata apresentação. A leitura dos packs, a seleção de idioma e a resolução do token ocorrem antes. Uma descrição com `{Radius}` ainda visível pede investigação do formatador do mod. Um texto correto no log que aparece separado, invertido, cortado ou como quadrados pede investigação da fonte, do ShapeText e do controle de UI.

Na configuração empacotada, o executável e suas fontes ficam em `PELE/LanguageSupport/renderer/` e `PELE/LanguageSupport/renderer/fonts/`, relativos à pasta de conteúdo do pacote. Grego e vietnamita usam fonte alternativa no TextMeshPro, sem passar pelo ShapeText.

## 6. Refatoração e quatro ferramentas de manutenção

A refatoração separa o núcleo `PELE.Language.Core.dll` da apresentação `LanguageSupport.dll`. No código, `PeleJsonLoader` e `PelePaths` cuidam da leitura, `OfficialLanguageIntegration` cuida da consulta integrada, `CustomLanguageRegistration` e `LanguagePauseMenu` cuidam da seleção. `LanguageRenderWorker`, `ComplexTextPresentation` e `HudLanguageRefresh` cuidam da composição, da interface e da atualização do HUD.

| Melhoria da 1.0.10 | Como usar para investigar |
| --- | --- |
| **Recarga com recuperação automática** | Edite o JSON e pressione **F5**. Com `EnableHotReload=true`, alterações também podem ser detectadas pelo observador de arquivos. Se um arquivo lido anteriormente falhar, o log identifica a origem e registra que conservou a última versão válida daquela sessão. Corrija o caminho, a codificação ou a sintaxe e recarregue. Valide com um segundo arquivo que tenha sido atualizado normalmente. |
| **Recuperação do ShapeText** | Procure por `ShapeText reiniciado` ou `ShapeText indisponível` no log. Após uma falha do processo ou ausência prolongada de resposta com pedidos pendentes, o módulo tenta reiniciar uma vez, reaproveita o cache e usa uma fonte nativa de contingência se a falha persistir. Essa fonte pode ter composição menos precisa. Em perfil isolado, compare o mesmo texto antes e depois e confirme que o jogo ainda responde. |
| **Origem de um token** | Execute `langsource PAUSE_RESUME` ou `langsource ITEM_MYSTICSITEMS_FLOW_DESC` no console com o idioma afetado ativo. A saída mostra o valor exibido, as camadas encontradas, JSONs duplicados e possíveis arquivos `.language`. O R2API não registra o nome do arquivo junto com cada entrada carregada. Trate os caminhos `.language` listados como candidatos e confirme o vencedor em perfil isolado. |
| **Validador antes de empacotar** | Na cópia que contém a refatoração, execute `python validate_translation_pack.py referencia.json traducao.json`. Use duas tabelas planas ou dois JSONs com `strings`, mantendo as chaves reais. A validação compara marcadores, tags e chaves, detecta JSON com duplicatas e retorna erro se houver divergência. A rotina de empacotamento dos pilotos também valida contra as referências em inglês do jogo, do P.E.L.E. e do pack Starstorm 2. Uma aprovação do validador complementa a revisão do significado e o teste em jogo. |

**Comandos e opções.** `langstatus` mostra idioma e estado da carga. `reloadlang` aciona a mesma rota de recarga dos JSONs. `langdebug` e **F6** dependem de `EnableDebugMenu=true`, desativado por padrão. Ative `EnableVerboseLogging` temporariamente se precisar de detalhes adicionais. O acesso ao console depende da configuração do jogo e do perfil. O log principal fica em `<pasta-do-perfil>/BepInEx/LogOutput.log`.

## 7. Roteiro para investigar e registrar um problema

1. **Identifique o caso.** Anote versão do jogo, versão e hash do P.E.L.E., versão do R2API.Language, perfil, mods e packs relevantes, idioma, tela e token. Copie só o trecho pertinente do log e retire caminhos pessoais antes de compartilhar.
2. **Reproduza em um perfil isolado.** Use o mesmo idioma e a mesma tela. Confira dependências e patchers nas versões instaladas. Teste o perfil mínimo com as dependências e acrescente o mod e seu pack para investigar uma interação. Para falhas na saída, preserve o log e o relatório de crash antes de relançar o jogo.
3. **Isole a etapa.** Token cru sugere falta de registro ou chave errada. Texto de outro pack sugere cobertura ou precedência. Um marcador cru sugere formatação. Texto correto com glifos ausentes ou desalinhados sugere fonte ou apresentação. Use `langstatus`, `langsource`, a referência oficial e o log para comprovar a hipótese.
4. **Corrija na origem.** Faça a menor alteração que resolva o caso. Preserve a DLL oficial do R2API.Language, os marcadores configuráveis e o caminho usado pelos outros mods. Para mudanças em código, reinicie e confira o binário carregado.
5. **Teste a correção e as regressões.** Repita o cenário original, troque para PT-BR e inglês, volte ao idioma afetado e repita o teste com F5 se ele fizer parte do fluxo. Conforme a alteração, teste valores dinâmicos, bandeiras, HUD, fontes, troca em partida e saída do jogo. Registre a diferença entre validação estática, teste com falha simulada e teste visual.
6. **Feche o registro.** Guarde problemas em investigação nos registros locais de trabalho. Traga para este manual soluções e comportamentos que possam ser reproduzidos e verificados.

### Modelo para copiar ao encontrar um defeito ou uma solução

```md
## [Componente] Resumo do caso

Estado — investigando / reproduzido / corrigido e verificado
Ambiente — jogo, perfil, pacotes, versões e hash do binário relevante
Reprodução — passos, idioma, tela e token
Esperado — o que deveria aparecer ou acontecer
Observado — texto, comportamento e fragmento relevante do log
Hipótese — onde a falha pode estar
Evidência — o que confirmamos e como
Correção — o que mudou e em qual componente
Regressão — cenários repetidos e resultado de cada um
Limites — configurações ou casos que ainda não foram verificados
```

Na auditoria da edição local, foram verificadas as trocas nos oito idiomas extras, inglês e PT-BR, a recuperação de um JSON truncado, a precedência entre arquivos, a reinicialização do ShapeText e a contingência por fonte após tempo limite simulado. Os três itens piloto de Starstorm 2 e o Fluxo Constante de RealerMysticsItems mantiveram valores dinâmicos nos cenários testados. Use esses casos como ponto de partida para futuras regressões.
