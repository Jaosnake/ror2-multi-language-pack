# Relatório de Auditoria Forense e Resolução Técnica: Robomando, Lee Hyperreal, Sorceress e Drow Ranger

**Data:** 07 de Novembro de 2026  
**Ambiente:** Risk of Rain 2 (v1.3.X / SotS / DLC4), BepInEx 5.4.2122, R2API.Language (Jaosnake Fork), P.E.L.E (Prompt Expanded Language Engine)  
**Perfis Auditados:** `Default` (`Thunderstore`), `teste`, Instalação Direta Steam

---

## 1. Sumário Executivo e Diagnóstico das Imagens Enviadas

Nas capturas de tela fornecidas pelo usuário foram observadas três anomalias críticas:
1. **Robomando (Japonês, Turco, Alemão):** Os nomes de habilidades estavam traduzidos, porém as descrições de *Single Fire* (Disparo Único) e *De-Escalate* (Desescalar) apareciam em **inglês** (`Agile. Shoot for 100% damage.` e `Agile. Stunning. Fire a small, piercing electric pulse for 300% damage.`), enquanto as outras duas habilidades (*Evasive Maneuver* e *Re-Wire*) estavam perfeitamente traduzidas no idioma nativo.
2. **Robomando (Esperanto):** Absolutamente **todos** os textos (nome, subtítulo, 4 habilidades e skins) apareciam 100% em **inglês**.
3. **Lee Hyperreal (Esperanto):** O personagem aparecia com o nome **"Phantom Ranger"** e todas as habilidades e passivas em **inglês** (`Orb and Ammo System`, `Hypermatrix System`, `Default VFX`, `Armament Barrage`, `Snipe Stance`).

---

## 2. Anatomia e Causa-Raiz de Cada Anomalia

### A. Robomando (`The_Bozos-RobomandoMod`)

#### 1. Por que apenas as habilidades 1 e 2 ficavam em inglês em japonês/turco/alemão?
Ao descompilar o assembly `RobomandoMod.dll` fornecido pelo usuário, identificou-se a seguinte implementação no módulo `RobomandoTokens`:
```csharp
public static readonly string[] dynamicTokens = new string[4] {
    "RAT_ROBOMANDO_PRIMARY_SHOT_DESCRIPTION",
    "RAT_ROBOMANDO_SECONDARY_ZAP_DESCRIPTION",
    "RAT_ROBOMANDO_SECONDARY_BOMB_DESCRIPTION",
    "RAT_ROBOMANDO_SPECIAL_OVERWIRE_DESCRIPTION"
};
```
O mod possui um listener no evento de troca de idioma do RoR2 (`Language.onCurrentLanguageChanged`):
```csharp
private static void SetNewAbilityToken(string token, object param)
{
    string text = Language.currentLanguageName;
    if (!DynamicTextDict.ContainsKey(text))
        text = "default"; // <--- FALLBACK PARA INGLÊS!
    
    string text2 = DynamicTextDict[text][token];
    if (text2 != null)
    {
        ModLanguage.SetTemporaryValue(token, StringUtils.FormatWith(text2, null, param));
    }
}
```
E dentro de `ModLanguage.SetTemporaryValue`:
```csharp
overlays[token] = LanguageAPI.AddOverlay(token, value);
```
**O Mecanismo da Falha:**
- `DynamicTextDict` é carregado **exclusivamente** lendo o arquivo `The_Bozos-RobomandoMod/plugins/RobomandoLangFiles.language`.
- Antes da nossa intervenção às 18:21, esse arquivo continha chaves com sufixos alterados (`_DYNAMIC`), ou faltavam as seções nativas completas formatadas com `{0}%` para as linguagens selecionadas.
- Ao mudar para Japonês (`ja`), Turco (`tr`) ou Alemão (`de`), o mod tentava buscar o token dinâmico no dicionário. Como o formato ou chave não casava, caía no fallback `text = "default"`.
- O mod então gerava a string em inglês formatada e chamava `LanguageAPI.AddOverlay(token, text2)`.
- No `LanguageAPI.cs` do RoR2, **`OverlayLanguage` tem prioridade absoluta de resolução sobre `CustomLanguage`**:
  ```csharp
  if (TryGetToken(OverlayLanguage, lang, token, out var v)) return v; // <--- Retorna o Overlay em inglês!
  if (TryGetToken(CustomLanguage, lang, token, out v)) return v;
  ```
- Por isso, mesmo que o pacote de idioma tivesse o token traduzido, o *Overlay* em inglês do Robomando passava na frente e forçava a descrição em inglês.
- As habilidades 3 (*Evasive Maneuver*) e 4 (*Re-Wire*) **não** fazem parte de `dynamicTokens`, portanto nunca recebiam overlay e exibiam a tradução correta da linguagem.

#### 2. Por que o Esperanto ficou 100% em inglês?
- O arquivo upstream `RobomandoLangFiles.language` original simplesmente **não possuía a seção `"eo"`**.
- Além disso, o pacote não contava com o arquivo JSON de mapeamento injetado no diretório P.E.L.E (`PELE/Language/eo/`).

---

### B. Lee Hyperreal (`PopcornFactory-Lee_Hyperreal`)

#### 1. Por que o personagem se chamava "Phantom Ranger"?
Ao analisar a fonte descompilada `media_1794084789703_a527280b.blob` (DLL do Lee Hyperreal), linha 18636:
```csharp
internal static string GenerateName()
{
    Random random = new Random();
    int index = random.Next(StaticValues.prefix.Count);   // Ex: "Phantom", "Void", "Quantum"
    string text = StaticValues.prefix[index];
    int index2 = random.Next(StaticValues.suffix.Count); // Ex: "Ranger", "Hunter", "Striker"
    string text2 = StaticValues.suffix[index2];
    return text + " " + text2;                           // Gera "Phantom Ranger"!
}
```
E no registro de tokens (linha 18655):
```csharp
LanguageAPI.Add(text2 + "NAME", Helpers.Lee("Lee: Hyperreal", text, Config.isBlueName.Value));
```
E na função `Helpers.Lee`:
```csharp
if (Config.loreMode.Value)
{
    return altDesc; // Retorna o nome procedural aleatório gerado!
}
return desc; // Retorna "Lee: Hyperreal"!
```
No arquivo de configuração `BepInEx/config/com.PopcornFactory.LeeHyperrealMod.cfg`, a opção `Lore Mode` estava definida como `true`:
```ini
[06 - Extras]
## Does the following: Applies more Lore friendly names...
Lore Mode = true
```
Isso fazia o jogo ignorar "Lee: Hyperreal" e batizar o sobrevivente aleatoriamente como **"Phantom Ranger"**!

#### 2. Por que em Esperanto tudo ficou em inglês?
- O mod Lee Hyperreal não possui suporte nativo a `.language` nem escuta `collectLanguageRootFolders`. Ele registra tokens hardcoded em inglês via `LanguageAPI.Add()` no startup.
- O pacote de idiomas anterior não havia compilado os arquivos `lee_hyperreal_eo.language` nem `Jaosnake-Lee_Hyperreal_LanguagePack.json` dentro da pasta `PELE/Language/eo/`.
- Sem esses arquivos, o RoR2 e o R2API não tinham qualquer entrada de tradução para `eo`, caindo de volta nos tokens em inglês originais da DLL.

---

### C. Sorceress (`Frosthex-SorceressMod`)

#### 1. Erro de Sintaxe JSON no Arquivo Interno:
Na auditoria automatizada via script `scratch/check_all_json.py`, detectamos uma violação de sintaxe fatal em `Frosthex-SorceressMod/plugins/Language/SorceressMod.language`:
```json
135:     "ACHIEVEMENT_FROSTHEX_ASTERIA_AIRTIMEACHIEVEMENT_NAME" : "Sorceress: Can't Touch This",
136:     "ACHIEVEMENT_FROSTHEX_ASTERIA_AIRTIMEACHIEVEMENT_DESCRIPTION" : "As Sorceress, stay airborne for 30 seconds without landing."
137:   }
138:   "zh-CN": {
```
**Faltava uma vírgula entre o fechamento do bloco `}` e a chave `"zh-CN": {`!**
O analisador `SimpleJSON` do R2API falhava silenciosamente com exceção de parse, cancelando a leitura do arquivo inteiro e impedindo o carregamento de quaisquer idiomas adicionais.

---

## 3. Correções Aplicadas e Status de Validação

| Mod | Problema Detectado | Solução Aplicada | Status de Validação (`Default`) |
| :--- | :--- | :--- | :--- |
| **Robomando** | Overlay forçava inglês nas skills 1 e 2; falta de `eo` | 26 seções sincronizadas em `RobomandoLangFiles.language`, corrigidos placeholders `{0}%`, adicionado `eo` e `la` (34 tokens cada) e PELE JSON | **100% OK** (Testado e Verificado) |
| **Lee Hyperreal** | `Lore Mode = true` gerava "Phantom Ranger"; falta de `eo`/`la` | `Lore Mode = false` no config de perfil e Steam; criados `lee_hyperreal_eo.language` e `lee_hyperreal_la.language` (91 tokens cada) + PELE JSON | **100% OK** (Testado e Verificado) |
| **Sorceress** | Vírgula ausente no JSON upstream; apenas 4 línguas no upstream | Sintaxe JSON corrigida; sincronizado `SorceressMod.language` completo (16 línguas) em `Frosthex-SorceressMod` e no LanguagePack | **100% OK** (Testado e Verificado) |
| **Drow Ranger** | Mod solicitado pendente de tradução | 32 tokens canônicos extraídos de `DotaHero.dll`, traduzidos via 3 subagentes paralelos em 15 idiomas, empacotados e injetados nos perfis e Steam | **100% OK** (Testado e Verificado) |

---
