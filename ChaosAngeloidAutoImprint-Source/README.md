# ChaosAuto - Auto Imprint para ChaosAngeloid

## O que é
Mod que automatiza a skill **F (Imprint/PickMaster)** do personagem **ChaosAngeloid**. Minions são ressuscitados automaticamente ao matar inimigos, com sistema de fila, prioridade elite, decay de vida, e auto-cast do buff a cada 30s.

---

## Como instalar

### Requisitos
- Risk of Rain 2
- BepInEx (instalado via r2modman)
- Mod **ChaosAngeloid** (Dragonyck)
- Mod **R2API** (já incluso como dependência do ChaosAngeloid)

### Passos
1. Coloque `ChaosAuto.dll` em:  
   `BepInEx\plugins\Jaosnake-ChaosAngeloidAutoImprint\`
2. O `manifest.json` já deve existir na mesma pasta
3. Inicie o jogo — o mod carrega automaticamente

### Verificação
- Abra o console do BepInEx (F1) e procure por `[ChaosAuto]`
- Ao jogar com ChaosAngeloid, mate inimigos — minions devem aparecer

---

## Como funciona (resumo pra leigo)

| O que | Como |
|-------|------|
| **Ativação** | Automática. Só jogar com ChaosAngeloid |
| **Spawn de minion** | Mata um bicho → revive como aliado por 3 min |
| **Máximo** | 2 minions: 1 terrestre + 1 voador |
| **Tipos** | Não duplica mesmo tipo de bicho |
| **Fila** | Se slots cheios, próximo vai pra fila (1 vaga) |
| **Elite** | Elite substitui minion normal (smart replace) |
| **Blacklist** | Besouro e Wisp são ignorados |
| **Brilho** | Laranja = normal, Vermelho = marcado pelo buff |
| **Buff F (PickMaster)** | Auto-cast a cada 30s em aliado aleatório 45m |
| **Decay** | Minion perde vida gradualmente até morrer em 3 min |
| **Morte do minion** | Morre naturalmente, sem derrubar HP do player |
| **Respawn** | 5s de espera após morte de minion |
| **Sem range pra spawn** | Minion sempre nasce perto de você |

---

## Comportamento detalhado

### Spawn
- Inimigo morre → verifica tipo, movimento, elite
- Slot livre + tipo diferente → spawn imediato
- Slot ocupado → fila (1 vaga)
- Fila cheia + candidato elite → substitui aliado normal mais fraco

### Decay de vida (3 minutos)
- `baseMaxHealth` reduz progressivamente até 1
- `health` capado no `baseMaxHealth` a cada frame
- Imune a cura: o HP máximo diminui junto
- Após 180s, HP = 0 → morte natural

### Auto-cast PickMaster (F)
- A cada 30s (cooldown da skill)
- Escolhe aleatoriamente 1 aliado do time player em 45m
- Inclui: nossos minions, companions, drones, etc.
- Não marca quem já tem MasterMarkBehaviour
- Tooltip: "Auto Imprint"

### Proteção anti-revive
- Harmony patch intercepta `CharacterMaster_TryReviveOnBodyDeath`
- Remove `MasterMarkBehaviour` antes do Chaos mod verificar
- Player NÃO toma dano nem revive quando minion morre

---

## Compilação

```powershell
dotnet build "C:\Users\Jaosnake\AppData\Local\Temp\opencode\ChaosAuto.csproj" -c Release
Copy-Item "C:\Users\Jaosnake\AppData\Local\Temp\opencode\bin\Release\netstandard2.1\ChaosAuto.dll" "...\BepInEx\plugins\Jaosnake-ChaosAngeloidAutoImprint\ChaosAuto.dll" -Force
```

---

## Arquitetura do Código Atual

### `ChaosAngeloidAutoImprintPlugin` (BepInPlugin)
- Hook `On.RoR2.CharacterBody.Start` no Awake
- Verifica se o body tem `ChaosAngeloid.Behaviour` via `HasChaosBehaviour()`
- Adiciona `AutoImprintBehaviour` no jogador

### `ResurrectedMinion` (MonoBehaviour)
- Tag component vazio — identifica corpos que já foram ressuscitados
- Usado no `OnEnemyDeath` pra evitar re-queue (anti-loop)

### `AutoImprintBehaviour` (MonoBehaviour)
**Campos de reflection:**
- `fiMaster` → `ChaosAngeloid.Behaviour.master`
- `masterMarkBuff` → `ChaosAngeloid.Prefabs.masterMarkBuff` (BuffDef)
- `tempOverlayType` → `ChaosAngeloid.TemporaryOverlayBehaviour`
- `tempOverlayTemporary` → field `temporary` (bool)
- `addOverlayMethod` → method `AddOverlay(Material)`
- `masterHighlight` → `ChaosAngeloid.Prefabs.masterHighlight` (Material)

**Fluxo `OnEnemyDeath()`:**
1. Filtros: null, player, champion, boss, player team, já-ressuscitado (`ResurrectedMinion`), fora do range (25m)
2. Lista negra: ignora `BeetleBody`, `WispBody`
3. Sem aliado vivo:
   - Se `respawnDelay <= 0` E fila vazia → `SpawnMinion()` imediato
   - Caso contrário → vai pra fila
4. Aliado vivo + mesmo tipo → skip
5. Fila cheia → smart replace (substitui o aliado mais fraco por elite)

**Fluxo `FixedUpdate()`:**
1. Aliado vivo: verifica se morreu (respawnDelay = 5s), expirou (KillAlly + delay), ou atualiza master
2. Sem aliado: se `respawnDelay > 0`, decrementa; quando chega a 0 → `SpawnFromQueue()`
3. Sem aliado + sem delay → `SpawnFromQueue()`

**Fluxo `SpawnMinion(GameObject prefab, CharacterMaster victimMaster, bool isElite)`:**
1. MasterSummon com `teamIndexOverride = Player`
2. `MinionLeash` item
3. Cópia equipamento elite (se `isElite`)
4. Boost 10%: `baseMaxHealth`, `baseMoveSpeed`, `baseAttackSpeed`
5. `ResurrectedMinion` component, `masterMarkBuff`, glow overlay
6. BaseAI ativado (followPattern 2-10m, maxDistance 130m)
7. Armazena `allyOriginalMaxHealth` (referência pro decay)
8. `allyIsEliteStatus` = `isElite`
9. allyTimer = 180s

**Fluxo `SweepDeadAllies()` (decay progressivo):**
1. Aliado morto/null → RemoveAllyAt, anyDied=true
2. Timer > 0: reduz `baseMaxHealth` e capa `health` proporcionalmente
3. Timer <= 0: força `Networkhealth = 0`
4. Morte natural → Harmony patch remove MMB antes do Chaos reviver

**Fluxo `AutoCastPickMaster()`:**
1. Usa `fiMasterSkill` (behaviour.masterSkill = F/PickMaster)
2. Varre `CharacterBody.readOnlyInstancesList` por aliados time player 45m
3. Filtra sem MasterMarkBehaviour, player, mortos
4. Escolhe aleatório, seta `targetHurtbox`, executa

**Refatoração (v4 — código modular):**
- **`IsEliteEnemy(CharacterMaster)`**: detecta elite pelo inventory (confiável, não `vb.isElite`)
- **`IsValidVictim()`**: filtros de morte extraídos pra método único
- **`TrySmartReplace(GameObject, CharacterMaster, bool)`**: substituição inteligente
- **`SweepDeadAllies()`**: varredura de aliados mortos/expirados
- **`CheckSlotAvailability()`**: verificação de slot (canSpawn, movementSlotFull, sameTypeAsAny)
- **`FindWeakestNonEliteAllyIndex()`**: encontra aliado normal mais fraco
- **`RemoveAllyAt()`**: helper para remoção consistente de aliados
- **`FireMasterSkill(CharacterBody)`**: executa PickMaster
- Código ~460 linhas

---

## Histórico de Problemas e Soluções

### 1. FieldAccessException: `body.transform` inacessível
- **Sintoma**: `FieldAccessException: Field RoR2.CharacterBody:transform' is inaccessible`
- **Causa**: Código ofuscado do RoR2 esconde `transform`
- **Solução**: Substituiu `body.transform.forward` por `body.characterDirection.forward`

### 2. Minion morria imediatamente após SpawnBody
- **Sintoma**: Log `Ally spawned successfully` seguido de morte no mesmo frame
- **Causa**: `selected.SpawnBody()` reusa um CharacterMaster morto com estado de morte
- **Solução final**: `MasterSummon` cria master NOVO do prefab (`MasterCatalog.GetMasterPrefab`)

### 3. Loop infinito de re-queue do mesmo mob
- **Sintoma**: Mesmo besouro ressuscitado → morria → re-enfileirado → loop
- **Causa**: OnCharacterDeathGlobal pegava a morte do minion e re-enfileirava
- **Solução**: `ResurrectedMinion` component tag → OnEnemyDeath ignora

### 4. Brilho roxo sem efeito (só masterMarkBuff não bastava)
- **Sintoma**: masterMarkBuff adicionado mas sem glow no modelo 3D
- **Causa**: O buff só dá o ícone roxo no HUD. O glow visual vem do `TemporaryOverlayBehaviour` com `masterHighlight`
- **Solução**: Adicionou `TemporaryOverlayBehaviour` com `masterHighlight` via reflection

### 5. Minions muito fracos
- **Sintoma**: Minions morriam rápido demais
- **Solução**: Boost de 10% em `baseMaxHealth`, `baseMoveSpeed`, `baseAttackSpeed`

### 6. Delay entre minions
- **Sintoma**: Aliado morria e o próximo spawnava instantaneamente
- **Solução**: `respawnDelay = 5s` entre morte e próximo spawn

### 7. Language.english crash no boot (removido)
- **Solução**: Tooltip agora usa `R2API.LanguageAPI.Add("token", "valor", "pt-BR")` via reflection

### 8. Smart Replace passava victimBody errado
- **Solução**: `TrySmartReplace` recebe `CharacterMaster` + `bool isElite` do elite original

### 9. Código morto: `HandleSpawnOrQueueOrReplace`
- **Solução**: Método removido

### 10. `CheckSlotAvailability` parâmetro redundante
- **Solução**: Removeu `alliesList`

### 11. Auto-cast usava skill R em vez de F
- **Sintoma**: Castava `Tainted Fireball` (R) em vez de `PickMaster` (F)
- **Solução**: `fiMasterSkill` via reflection → `behaviour.masterSkill.ExecuteIfReady()`

### 12. Revive do Chaos mod derrubando HP do player pra 1
- **Sintoma**: Quando minion marcado morria, player revivia com 1 HP (mecânica `CharacterMaster_TryReviveOnBodyDeath`)
- **Causa**: `TryReviveOnBodyDeath` roda ANTES do `OnCharacterDeath` — nosso Destroy do MMB chegava tarde
- **Solução**: Harmony prefix patch no método `CharacterMaster_TryReviveOnBodyDeath` do Chaos mod — remove MMB antes do Chaos verificar

### 13. Minions não despawnavam corretamente (corpo zumbi)
- **Sintoma**: `KillAlly` (Suicide + DestroyBody) deixava corpo ou não removia completamente
- **Solução**: Substituído por **decay progressivo de vida** ao longo de 180s
  - `baseMaxHealth` e `health` reduzidos proporcionalmente a cada frame
  - Referência: `allyOriginalMaxHealth` (HP do spawn com boost de 10%)
  - Imune a cura: `baseMaxHealth` diminui junto, curas não ultrapassam o cap
  - Morte natural quando HP chega a 0; Harmony patch bloqueia o revive

### 14. Auto-cast só pegava nossos minions
- **Solução**: Varre `CharacterBody.readOnlyInstancesList` — inclui companions, drones, etc.
- Range: 45m (`PICKMASTER_RANGE`, mesmo do scan passivo do Chaos)

---

## Sistema de Fila

| Estado | Ação |
|--------|------|
| Sem aliado, sem delay, fila vazia | `SpawnMinion()` imediato no próximo morte |
| Sem aliado, delay ativo | Morte vai pra fila (slot 2) |
| Aliado vivo, tipo diferente | Vai pra fila (slot 2) |
| Fila cheia (1 slot) | Skip → `TrySmartReplace()` (substitui o aliado mais fraco por elite) |
| Aliado morre | `respawnDelay = 5s` |
| Delay expira | `SpawnFromQueue()` |

## Prioridade e Substituição Inteligente

**Prioridade (Elite > Normal):**
- Elite (`isElite == true`) sempre tem prioridade sobre aliados normais
- Se um candidato elite aparece e há um slot livre, ele spawna imediatamente
- Elite minions são rastreados em `allyIsEliteStatus` para prioridade decisions

**Smart Replace:**
- Quando fila cheia e um novo candidato elite aparece:
  - O **aliado mais fraco (normal)** é substituído com o novo elite
  - O aliado antigo é morto via `KillAlly()` (dispara suicídio + limpeza)
  - O novo elite spawna em seu lugar
- **Log**: `[ChaosAuto] Replaced weak ally with elite: <prefab.name>`

**Blacklist:**
- `BeetleBody`, `WispBody` são ignorados completamente (não entram no pool)
- Aplicado antes da prioridade e da substituição inteligente

---

## Decompile: Arquitetura do ChaosAngeloid.dll

### Classes Relevantes

| Classe | Função |
|--------|--------|
| `Behaviour` | Componente no jogador. Campo `master` (CharacterBody alvo) |
| `PickMaster` | EntityState do F skill. Adiciona `MasterMarkBehaviour` + `TemporaryOverlayBehaviour` no alvo |
| `MasterMarkBehaviour` | Aplica `masterMarkBuff`, overlay, transfere 10% stats do dono pro alvo |
| `TemporaryOverlayBehaviour` | Overlay visual. `AddOverlay(Material)`, campo `temporary` (bool) |
| `Prefabs` | `masterMarkBuff` (BuffDef), `masterHighlight` (Material roxo), `allyHighlight` (Material azul) |
| `EntityScanBehaviour` | Scan passivo 45m com BullseyeSearch |

### PickMaster (F skill)
```csharp
behaviour.master = behaviour.targetHurtbox.healthComponent.body;
((Component)behaviour.master).gameObject.AddComponent<MasterMarkBehaviour>().owner = player;
((Component)behaviour.master).gameObject.AddComponent<TemporaryOverlayBehaviour>();
temporaryOverlayBehaviour.temporary = false;
temporaryOverlayBehaviour.AddOverlay(Prefabs.masterHighlight);
```

### Hook de Dano (20% bonus)
```csharp
if (self.body.HasBuff(Prefabs.masterDmgBuff) && damageInfo.attacker.GetComponent<MasterMarkBehaviour>())
{
    damageInfo.damage *= 1.2f;
}
```

---

## Referência: MasterSummon

```csharp
MasterSummon summon = new MasterSummon
{
    masterPrefab = prefab,
    position = spawnPos,
    rotation = Quaternion.identity,
    summonerBodyObject = body.gameObject,
    teamIndexOverride = TeamIndex.Player,
    ignoreTeamMemberLimit = false,
    inventoryToCopy = body.inventory
};
CharacterMaster newMaster = summon.Perform();
```

---

## Notas

- Classes do ChaosAngeloid.dll são `internal` — acessadas via `Assembly.GetType()` + reflection
- `masterHighlight` usa remap texture `texBehemothRamp.png` (vermelho/roxo), FresnelPower 0.66, AlphaBoost 6
- Nenhum mod no Thunderstore faz exatamente o mesmo sistema (hook de morte + MasterSummon + fila + glow)
- RalseiSurvivor é o mais próximo: skill "Pacify" converte inimigos <50% HP em aliados (max 3)
- `SCAN_RADIUS = 25f` — o `EntityScanBehaviour` do mod original usa 45m se quiser alinhar

---

## Atualizações Recentes (v2)

### 2 aliados simultâneos + Divisão Terrestre/Voador
- `MAX_ALLIES = 2`: suporta até 2 minions vivos ao mesmo tempo
- **1 slot terrestre** + **1 slot voador**: baseado em `CharacterBody.isFlying`
- Se o slot de movimento já está ocupado → vai pra fila (se houver vaga)
- **Tipo diverso**: mesmo `GetBodyType()` não duplica entre aliados vivos

### AI Ativada (antiga: `ai.enabled = false`)
- `SetBaseAIEnabled(true)` via reflection + `leader.gameObject = player`
- `followPattern` 2-10m de distância do jogador
- `enemyAttentionDuration = 6s` — reavalia alvo a cada 6s
- `fullVision = false` — não vê atrás
- `AISkillDriver.maxDistance` capped em **130m**
- Minions usam as próprias skills (AISkillDriver nativo do prefab)

### PickMaster Automático (F skill)
- **1ª marca**: 5s após o primeiro minion spawnar → `TriggerFirstPickMaster()`
- **Seguintes**: `AutoCastPickMaster()` a cada 30s (cooldown da skill) escolhe **aleatoriamente** entre minions sem marca
- Usa `behaviour.masterSkill` (NÃO `skillLocator.special` — que é a R)
- Seta `behaviour.targetHurtbox = ally.mainHurtBox` antes de executar
- 0.7s depois → `RefreshMinionGlows()` (re-aplica glow após PickMaster destruir o Tob)
- **Novo nome**: `Auto Imprint` (em vez de `Imprint`)
- **Nova descrição**: `<style=cIsDamage>Auto</style>. Automatically imprints an unmarked ally every <style=cIsUtility>30s</style>, granting <style=cIsDamage>10% stats</style>.`

### Glow por Estado (Laranja ↔ Vermelho)
- **masterHighlight (laranja)**: minion **sem** `MasterMarkBehaviour`
- **masterHighlight + _TintColor vermelho**: minion **com** `MasterMarkBehaviour`
- Parâmetros atuais:
  - `_AlphaBoost = 6` (reduzido de 10)
  - `_FresnelPower = 0.66` (aumentado de 0.4)
  - Vermelho: `_TintColor = Color(1, 0, 0, 0.3)`
- `RefreshMinionGlows()` roda a cada 1s no FixedUpdate + após PickMaster

### Elite Equipment
- Elite detectado via `IsEliteEnemy(master)` — verifica `master.inventory.currentEquipmentIndex != None`
- `SpawnMinion` copia `currentEquipmentIndex` + `alternateEquipmentIndex` do `victimMaster` pro minion
- Minions elite mantêm buffs visuais (fogo, gelo, elétrico, etc.)

### Prioridade de Elite
- `isElite` é rastreado em `allyIsEliteStatus` para cada minion
- Elite sempre tem prioridade sobre minions normais
- Smart replace substitui normal por elite quando fila cheia

### Reflection
| Campo | Uso |
|-------|-----|
| `fiMaster` | `Behaviour.master` — alvo atual do PickMaster |
| `fiTargetHurtbox` | `Behaviour.targetHurtbox` — setar antes de executar a skill |
| `masterMarkType` | `MasterMarkBehaviour` — checar se minion já tem a marca |
| `masterMarkOwner` | `MasterMarkBehaviour.owner` — setar dono da marca |
| `masterMarkBuff` | `Prefabs.masterMarkBuff` — buff do HUD |
| `overlayType` | `TemporaryOverlayBehaviour` — glow 3D |
| `overlayTemporary` | `TemporaryOverlayBehaviour.temporary` |
| `overlayAddMethod` | `TemporaryOverlayBehaviour.AddOverlay(Material)` |
| `overlayMaterial` | `Prefabs.masterHighlight` — material base do glow |

### Logs de Debug
- `[ChaosAuto] Spawning: ... [ELITE] fly=True/False` — spawn imediato (com tag elite se for o caso)
- `[ChaosAuto] Queued: ... fly=True/False` — fila por delay/outro motivo
- `[ChaosAuto] Ally died, X remaining, 5s delay` — morte de aliado
- `[ChaosAuto] Spawning from queue: ...` — spawn vindo da fila
- `[ChaosAuto] Replaced weak ally with: ...` — smart replace

---

## Testes Recentes (16/06/2026 - Final)

### Funcionando ✅
- **Blacklist**: BeetleBody, WispBody ignorados
- **Spawn + fila**: ground/flying, type diversity, queue 1 slot
- **Timer 180s**: decay progressivo de HP (imune a cura)
- **Range**: sem limite pra spawn (minion nasce perto do player), PickMaster 45m
- **Sem crash no boot**: Harmony + R2API.LanguageAPI
- **Sem dano no player**: Harmony patch bloqueia revive do Chaos

### Testando / Aguardando confirmacao ⏳
- **Elite equipment**: copia `currentEquipmentIndex` + `alternateEquipmentIndex`
- **Smart Replace**: elite substitui normal quando fila cheia
- **Auto-cast PickMaster**: usa `behaviour.masterSkill` (F), aleatório entre todos aliados 45m
- **Glow**: laranja (sem marca) / vermelho (com marca)

---

## Correções da Auditoria (16/06/2026)

### Bug 7: Crash no boot (Language.english nulo → freeze 6% loading)
- **Causa**: `Language.english.SetStringByToken` no `Start()` do plugin rodava durante boot do BepInEx
- **Solução**: Removido completamente. Tooltip cosmético não vale o risco de crash

### Bug 8: Elite sem poderes visuais
- **Sintoma**: Elite ressuscitado não tinha fogo/gelo/etc
- **Causa**: Código de cópia de `currentEquipmentIndex` e `alternateEquipmentIndex` foi perdido na refatoração
- **Solução**: Adicionado bloco em `SpawnMinion`:
  ```csharp
  if (isElite && victimMaster?.inventory != null && newMaster.inventory != null)
  {
      vi.currentEquipmentIndex → ni.SetEquipmentIndex
      vi.alternateEquipmentIndex → ni.SetEquipmentIndexForSlot
  }
  ```

### Bug 9: vb.isElite sempre false no hook de morte
- **Sintoma**: Smart replace nunca substituía, elite marcado como normal
- **Causa**: O jogo remove o buff de elite ANTES do evento `OnCharacterDeath`
- **Solução**: Novo método `IsEliteEnemy(CharacterMaster)` que verifica `master.inventory.currentEquipmentIndex != EquipmentIndex.None`

### Bug 10: Métodos com assinatura errada
- `SpawnMinion` agora recebe `(GameObject prefab, CharacterMaster victimMaster, bool isElite)` — não mais `CharacterBody`
- `TrySmartReplace` agora recebe `(GameObject prefab, CharacterMaster victimMaster, bool isElite)`
- `IsEliteEnemy(CharacterMaster)` — novo helper

### Bug 11: HandleSpawnOrQueueOrReplace removido (código morto)
- Método com 40% de código nunca executado (verificação duplicada do OnEnemyDeath)
- Lógica de fila/replace movida direto pro OnEnemyDeath