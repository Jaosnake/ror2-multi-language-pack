# HANDOVER — ror2-multi-language-pack

## Links

- **Repositório GitHub**: https://github.com/Jaosnake/ror2-multi-language-pack
- **Branch**: `main`
- **Clone local**: `C:\Users\Jaosnake\AppData\Local\Temp\opencode\ror2-multi-language-pack`
- **Plugins r2modman**: `C:\Users\Jaosnake\AppData\Roaming\r2modmanPlus-local\RiskOfRain2\profiles\Default\BepInEx\plugins`
- **Cache Thunderstore (fontes limpas)**: `C:\Users\Jaosnake\AppData\Roaming\r2modmanPlus-local\RiskOfRain2\cache`
- **Backup mais recente**: `C:\Users\Jaosnake\Downloads\ror2-multi-language-pack-ESTADO-ATUAL-20260602-074343.zip`

---

## Estrutura do Projeto

```
ror2-multi-language-pack/
├─ README.md                          # Documentação principal
├─ Starstorm2/                         # Pacote Thunderstore do Starstorm 2
│   ├─ README.md
│   ├─ manifest.json
│   ├─ icon.png
│   └─ Language/                       # 8 pastas de idiomas
├─ Sandswept/                          # Pacote Thunderstore do Sandswept
│   ├─ README.md
│   ├─ manifest.json
│   ├─ icon.png
│   └─ Translations/                   # 14 arquivos .language
└─ mods/                               # 31 mods individuais
    ├─ Alfabeticamente organizados/
    │   ├─ Bog-Deputy/
    │   ├─ Bog-Mortician/
    │   ├─ ...
    │   └─ tsuyoikenko-Interrogator/
    └─ Cada mod contém:
        ├─ pt-BR-ModName.language      # Carregado pelo R2API
        ├─ Language/en/*.txt           # Original inglês (KEY: VALUE)
        ├─ Language/pt-BR/*.txt        # Tradução pt-BR
        └─ Language/{lang}/*.txt       # Outros idiomas
```

---

## Arquitetura de Carregamento de Tradução

O jogo Risk of Rain 2 carrega traduções de DUAS formas:

### 1. R2API Language API (arquivos `.language`)
- Escaneia `BepInEx/plugins/**/*.language`
- Usa `SimpleJSON.Parse()` (mais tolerante que JSON padrão)
- Formato esperado: `{ "pt-BR": { "TOKEN": "valor" } }` — JSON válido com código do idioma como chave top-level
- A chave especial `"strings"` é um fallback genérico (aplica a todos os idiomas)

### 2. Sistema nativo do jogo (arquivos `.txt`)
- Carregado pelos mods individualmente via suas DLLs
- Formato `KEY: VALUE` (linha por linha)
- Localizados em `Language/{locale}/` dentro da pasta de cada mod

---

## Idiomas Suportados por Mod

Para cada mod, os idiomas comuns são: `en`, `pt-BR`, `es-419`, `FR`, `ru`, `zh-CN`, `ja`

Alguns mods adicionais: `de`, `IT`, `ko`, `tr`, `UK` (quantidade varia por mod)

O mod SorceressMod tem apenas `en`.

Starstorm2: 8 idiomas (pt-BR, UA, KO, FR, RU, zh-CN, es-419, TR)
Sandswept: 14 idiomas (pt-BR, RU, DE, ES, FR, IT, PL, JA, KO, zh-CN, zh-TW, TR, EN, UA)

---

## Problemas Conhecidos e Estado Atual

### 1. CRÍTICO — Acentos Portugueses Substituídos por `?`
**Arquivos afetados**: Quase todos os `.language` e `.txt` de tradução.
**Causa**: Arquivos salvos em ASCII 7-bit em sessão anterior — caracteres acentuados (`á`, `é`, `ç`, `õ`, etc.) foram irreversivelmente substituídos por `?` literal (byte 0x3F).

**Mods com cache limpo (JÁ RESTAURADOS)** — acentos corrigidos a partir do cache Thunderstore:
- EnforcerGang-Enforcer ✅
- EnforcerGang-HAND_OVERCLOCKED ✅
- EnforcerGang-MinerUnearthed ✅
- EnforcerGang-Pilot ✅
- EnforcerGang-Rocket ✅
- Paladin_Alliance-PaladinMod ✅
- TheTimesweeper-Red_Alert ✅
- TeamCloudburst-Cloudburst ✅
- Risky_Lives-RiskyTweaks ✅ (Interactors files)

**Mods SEM cache (ainda corrompidos)** — precisam reescrita/regeração manual:
- tsuyoikenko-Banshee (48 `?` no pt-BR)
- HasteReapr-AssassinMod (112 `?` no pt-BR)
- public_ParticleSystem-Ravager (109 `?` no pt-BR)
- rob-Belmont (99 `?` no pt-BR)
- rob-Dante (75 `?` no pt-BR)

### 2. RISCOS DA RESTAURAÇÃO POR CACHE
Durante uma tentativa de restauração, um script copiou 1031 arquivos do cache Thunderstore para o plugins usando apenas o NOME DO ARQUIVO como critério (ex: `Enforcer.txt`). Isso fez com que arquivos de idiomas DIFERENTES sobrescrevessem uns aos outros (ex: conteúdo chinês foi copiado para pasta pt-BR). Uma segunda restauração mais cuidadosa (133 arquivos, casando por diretório de idioma) foi feita, MAS NÃO HÁ GARANTIA de que todos os arquivos estão no idioma correto. 

**Recomendação**: Verificar todos os arquivos que foram restaurados do cache para confirmar que o conteúdo corresponde ao idioma da pasta.

### 3. JSONs com Chaves Desbalanceadas (JÁ CORRIGIDOS)
Todos os seguintes foram corrigidos:
- `rob-Belmont/zh-CN/belmont.language`: faltava `}` final ✅
- `rob-HUNK/FR-HUNK.language`: `}` extra ✅
- `Jaosnake-Starstorm2_LanguagePack/SS2Lang_pt-br.language`: 1 `}` extra ✅
- `RiskyTweaks/Interactors.txt` (5 arquivos): linhas órfãs removidas ✅
- Causavam o crash: `JSON Parse: Too many closing brackets`

### 4. Escapes Inválidos em JSON (CORRIGIDOS)
- `Paladin/IT/paladin.txt`: `\'` → `'` ✅
- `The_Bozos/RobomandoLangFiles.language`: `\ ` solitário removido ✅

### 5. UTF-8 BOM (CORRIGIDO)
34 arquivos tinham BOM — todos removidos ✅

### 6. Windows-1252 Misturado com UTF-8 (CORRIGIDO)
16 arquivos `.language` estavam em Win1252 — convertidos para UTF-8 ✅

### 7. The House — Chaves Totalmente Erradas (CORRIGIDO)
O mod JavAngle-TheHouse tinha chaves pt-BR que NÃO correspondiam às chaves do jogo:
- `STANDARDSPREAD_NAME` → deveria ser `PRIMARY_PUNCH_NAME`
- E mais 14 chaves de habilidades erradas
- DESCRIPTION com JSON quebrado (`\` no final da linha, aspas sem escape)
- **Corrigido manualmente** ✅

### 8. Créditos Inline — 5 Mods com Tradução Comunitária (CORRIGIDO)
Estes mods tinham tradução pt-BR ORIGINAL feita pela comunidade (não pelo Jaosnake):
- Enforcer → original por **Kauzok**
- HAND_OVERCLOCKED → original por **Kauzok**
- MinerUnearthed → original por **Donitodorito**
- Red_Alert → original por **Kauzok**
- Cloudburst → original por **Kauzok**

O crédito `"Traduzido por: Jaosnake"` foi removido dos arquivos e os tradutores originais foram creditados no README. Os demais 24 mods mantiveram o crédito pois Jaosnake é o tradutor original.

### 9. README (CORRIGIDO)
- Seção pt-BR atualizada com Kauzok e Donitodorito ✅
- Seção "Community translators" adicionada (21 tradutores nominais) ✅
- Aviso de loading time adicionado aos 3 READMEs ✅
- Linha `"All translation tokens contain embedded credit inline"` removida ✅

---

## Pendências para a Próxima IA

### Prioridade Máxima
1. **Regenerar/reescrever os acentos portugueses** nos 5 mods sem cache (Banshee, AssassinMod, Ravager, rob-Belmont, rob-Dante). O texto já existe em português — só os acentos estão trocados por `?`. Pode-se usar contexto para adivinhar a maioria (ex: `n?o` → `não`, `a?rea` → `aérea`, etc.).
2. **Verificar se todos os arquivos restaurados do cache estão no idioma correto** — especialmente os que podem ter sido sobrescritos com conteúdo de idioma errado.

### Prioridade Média
3. **Verificar a consistência de chaves** entre os arquivos `.language` e `.txt` para cada mod. O The House estava com chaves erradas — verificar se há outros.
4. **Sincronizar plugins ↔ repo** depois das correções.

### Prioridade Baixa
5. **Adicionar os mods faltantes** ao repo: atualmente há 31 mods no repo. O plugins pode ter outros (RiskyFixes, SneedHooks, etc.) — decidir se devem entrar também.
6. **Testar em jogo** para confirmar que não há crashes de JSON parsing.

### Observações Técnicas

- **SimpleJSON** (usado pelo R2API e pelo jogo) é mais tolerante que JSON padrão: aceita trailing commas, comentários, chaves sem aspas, etc. Mas NÃO aceita `\` (barra invertida solitária) ou `}` extra.
- **Encoding**: SEMPRE UTF-8 sem BOM. `[System.IO.File]::WriteAllText($path, $content, (New-Object System.Text.UTF8Encoding $false))` no PowerShell.
- **Win1252 → UTF-8**: `[System.Text.Encoding]::GetEncoding(1252).GetString($bytes)` + WriteAllText com UTF8Encoding $false.
- **Join-Path no PS5.1**: Só aceita 2 argumentos. Usar concatenação de strings (`"$base\$subdir"`) em vez de `Join-Path $base "subdir" $item`.
- **PowerShell `$var:`**: Em strings com interpolacão, `$var:` é interpretado como drive (ex: `$env:Path`). Usar `${var}` ou `-f` operator.
- **Não usar `Select-String` com padrões** que contenham aspas simples dentro de strings com aspas simples no PowerShell.

---

## Commits Recentes no GitHub

```
a4c506f fix Paladin IT escape, sync remaining fixes
386d6c0 fix extra braces in RiskyTweaks Interactors (5 files)
33439e8 fix missing closing brace in rob-Belmont zh-CN
db13b54 fix unbalanced brace in rob-HUNK FR-HUNK.language
5eb29dc fix The House pt-BR: correct keys and fix DESCRIPTION
8a26752 add loading time warning to READMEs
4f7384b fix invalid escape in RobomandoLangFiles.language
ec02439 remove BOM: Red_Alert (13 files) + Banshee en
6d37585 remove BOM: Assassin, Paladin, Ravager, rob, Cloudburst (10 files)
4713334 remove BOM: EnforcerGang mods (6 files)
99f5992 remove BOM: EnforcerGang-Enforcer (4 files)
ef258fc fix encoding: tsuyoikenko-Interrogator pt-BR
6dd5214 fix encoding: Cloudburst UK
... (encoding fixes para 21 arquivos)
574bb94 fix encoding: Bog-Deputy pt-BR-Deputy.language
21376b2 Remove inline translator credits from 5 mods
```

---

## Comandos Úteis

```powershell
# Verificar brace balance em todos os arquivos
$plugins = "C:\...\plugins"
Get-ChildItem $plugins -Include *.language,*.txt -Recurse | ForEach-Object {
    $c = [System.IO.File]::ReadAllText($_.FullName)
    $o=0;$cl=0; for ($i=0;$i -lt $c.Length;$i++) { if($c[$i]-eq'{'){$o++}elseif($c[$i]-eq'}'){$cl++} }
    if ($o -ne $cl) { Write-Host "$($_.Name): $o/$cl" }
}

# Verificar acentos vs ? corrompidos
Get-ChildItem $plugins -Include *.language,*.txt -Recurse | ForEach-Object {
    $c = [System.IO.File]::ReadAllText($_.FullName)
    $q=0;$na=0
    for ($i=0;$i -lt $c.Length;$i++) { $cp=[int]$c[$i]; if($cp-eq0x3F){$q++}elseif($cp-gt0x7F-and$cp-ne0xFFFD){$na++} }
    if ($na -eq 0 -and $q -gt 5) { Write-Host "CORROMPIDO: $($_.Name) - ?=$q" }
}

# Commit e push
git add -A && git commit -m "mensagem" && git push origin main
```
