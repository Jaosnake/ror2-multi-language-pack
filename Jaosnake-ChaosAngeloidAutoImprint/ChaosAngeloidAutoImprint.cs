using BepInEx;
using RoR2;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Networking;
using System.Linq;
using HarmonyLib;

namespace ChaosAngeloidAutoImprint
{
    [BepInPlugin("com.jaosnake.ChaosAngeloidAutoImprint", "ChaosAngeloid Auto Imprint", "1.0.0")]
    [BepInDependency("com.Dragonyck.ChaosAngeloid")]
    public class ChaosAngeloidAutoImprintPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            PatchChaosRevive();

            On.RoR2.CharacterBody.Start += (orig, self) =>
            {
                orig(self);
                if (self.isPlayerControlled && HasChaosBehaviour(self))
                {
                    self.gameObject.AddComponent<AutoImprintBehaviour>();
                }
            };
        }

        private static void PatchChaosRevive()
        {
            try
            {
                var chaosAsm = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "ChaosAngeloid");
                if (chaosAsm == null) return;

                foreach (var t in chaosAsm.GetTypes())
                {
                    var method = t.GetMethod("CharacterMaster_TryReviveOnBodyDeath", 
                        BindingFlags.Static | BindingFlags.NonPublic);
                    if (method == null) continue;

                    var harmony = new Harmony("com.jaosnake.ChaosAngeloidAutoImprint.revivefix");
                    harmony.Patch(method, prefix: new HarmonyMethod(typeof(ChaosAngeloidAutoImprintPlugin),
                        nameof(Prefix_TryReviveOnBodyDeath)));
                    break;
                }
            }
            catch { }
        }

        private static bool Prefix_TryReviveOnBodyDeath(CharacterMaster self, CharacterBody body)
        {
            if (body != null && body.gameObject.GetComponent<ResurrectedMinion>() != null)
            {
                var mmbType = GetCachedMMBType();
                if (mmbType != null)
                {
                    var mmb = body.gameObject.GetComponent(mmbType);
                    if (mmb != null) UnityEngine.Object.Destroy(mmb);
                }
            }
            return true;
        }

        private static Type cachedMMBType;
        private static Type GetCachedMMBType()
        {
            if (cachedMMBType != null) return cachedMMBType;
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (a.GetName().Name == "ChaosAngeloid")
                {
                    cachedMMBType = a.GetType("ChaosAngeloid.MasterMarkBehaviour");
                    break;
                }
            }
            return cachedMMBType;
        }

        private static bool HasChaosBehaviour(CharacterBody body)
        {
            foreach (var c in body.GetComponents<Component>())
            {
                var t = c.GetType();
                if (t.Name == "Behaviour" && t.Namespace == "ChaosAngeloid")
                    return true;
            }
            return false;
        }
    }

    public class ResurrectedMinion : MonoBehaviour { }

    public class AutoImprintBehaviour : MonoBehaviour
    {
        private static readonly HashSet<string> Blacklist = new HashSet<string>
        {
            "BeetleBody",
            "WispBody",
        };

        private CharacterBody body;
        private readonly List<CharacterBody> allies = new List<CharacterBody>();
        private readonly List<float> allyTimers = new List<float>();
        private readonly List<bool> allyIsEliteStatus = new List<bool>();
        private readonly List<float> allyOriginalMaxHealth = new List<float>();
        private float respawnDelay;
        private readonly Queue<GameObject> pendingQueue = new Queue<GameObject>();
        private object chaosBehaviour;
        private FieldInfo fiMaster;
        private FieldInfo fiTargetHurtbox;
        private FieldInfo fiMasterSkill;
        private Type masterMarkBehaviourType;
        private FieldInfo masterMarkOwner;
        private BuffDef masterMarkBuff;
        private Type overlayType;
        private FieldInfo overlayTemporary;
        private MethodInfo overlayAddMethod;
        private Material overlayMaterial;
        private Material glowMatOrange;
        private Material glowMatRed;
        private float glowRefreshTimer;

        private const float ALLY_DURATION = 180f;
        private const float RESPAWN_DELAY = 5f;
        private const int MAX_ALLIES = 2;
        private const float PICKMASTER_RANGE = 45f;

        private void Awake() { body = GetComponent<CharacterBody>(); FindChaosFields(); }

        private void FindChaosFields()
        {
            try
            {
                foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (a.GetName().Name == "ChaosAngeloid")
                    {
                        var t = a.GetType("ChaosAngeloid.Behaviour");
                        if (t != null)
                        {
                            fiMaster = t.GetField("master", BindingFlags.Public | BindingFlags.Instance);
                            fiTargetHurtbox = t.GetField("targetHurtbox", BindingFlags.Public | BindingFlags.Instance);
                            fiMasterSkill = t.GetField("masterSkill", BindingFlags.Public | BindingFlags.Instance);
                        }
                        var prefabsType = a.GetType("ChaosAngeloid.Prefabs");
                        if (prefabsType != null)
                        {
                            var fi = prefabsType.GetField("masterMarkBuff", BindingFlags.Static | BindingFlags.NonPublic);
                            if (fi != null) masterMarkBuff = fi.GetValue(null) as BuffDef;
                            var hl = prefabsType.GetField("masterHighlight", BindingFlags.Static | BindingFlags.NonPublic);
                            if (hl != null) overlayMaterial = hl.GetValue(null) as Material;
                        }
                        overlayType = a.GetType("ChaosAngeloid.TemporaryOverlayBehaviour");
                        if (overlayType != null)
                        {
                            overlayTemporary = overlayType.GetField("temporary", BindingFlags.Public | BindingFlags.Instance);
                            overlayAddMethod = overlayType.GetMethod("AddOverlay", new Type[] { typeof(Material) });
                        }
                        masterMarkBehaviourType = a.GetType("ChaosAngeloid.MasterMarkBehaviour");
                        if (masterMarkBehaviourType != null)
                            masterMarkOwner = masterMarkBehaviourType.GetField("owner", BindingFlags.Public | BindingFlags.Instance);
                        break;
                    }
                }
            }
            catch { }
        }

        private static bool tooltipSet = false;

        private void Start()
        {
            if (body.hasAuthority)
                On.RoR2.GlobalEventManager.OnCharacterDeath += OnCharacterDeathGlobal;
            chaosBehaviour = GetComponent("ChaosAngeloid.Behaviour");
            InitGlowMaterials();

            if (!tooltipSet)
            {
                tooltipSet = true;
                var langApi = Type.GetType("R2API.LanguageAPI, R2API");
                var add = langApi?.GetMethod("Add", new Type[] { typeof(string), typeof(string), typeof(string) });
                add?.Invoke(null, new object[] { "CHAOS__F", "Auto Imprint", "pt-BR" });
                add?.Invoke(null, new object[] { "CHAOS__F_DESCRIPTION", "<style=cIsDamage>Auto</style>. Marks a random nearby ally as master every <style=cIsUtility>30s</style>. Grants <style=cIsDamage>+10% stats</style>. <style=cIsUtility>45m range</style>.", "pt-BR" });
            }
        }

        private static string GetBodyType(string name)
        {
            if (name.EndsWith("(Clone)")) name = name.Substring(0, name.Length - 7);
            return name;
        }

        private void OnCharacterDeathGlobal(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            if (damageReport?.victimBody != null && masterMarkBehaviourType != null)
            {
                var mmb = damageReport.victimBody.gameObject.GetComponent(masterMarkBehaviourType);
                if (mmb != null) Destroy(mmb);
            }
            orig(self, damageReport);
            OnEnemyDeath(damageReport);
        }

        private void OnEnemyDeath(DamageReport report)
        {
            if (report?.victim == null) return;
            var vb = report.victimBody;
            if (vb == null || vb == body) return;
            if (vb.isPlayerControlled || vb.isChampion || vb.isBoss) return;
            if (vb.teamComponent.teamIndex == TeamIndex.Player) return;
            if (vb.GetComponent<ResurrectedMinion>() != null) return;

            if (Blacklist.Contains(GetBodyType(vb.name))) return;

            var master = report.victimMaster;
            if (master == null) return;

            GameObject prefab = MasterCatalog.GetMasterPrefab(master.masterIndex);
            if (prefab == null) return;

            bool isFlying = vb.isFlying;
            string enemyType = GetBodyType(vb.name);
            bool isElite = IsEliteEnemy(master);

            var slotInfo = CheckSlotAvailability(vb);

            if (slotInfo.canSpawn && !slotInfo.movementSlotFull && !slotInfo.sameTypeAsAny)
            {
                Debug.Log("[ChaosAuto] Spawning: " + vb.name + (isElite ? " [ELITE]" : "") + " fly=" + isFlying);
                SpawnMinion(prefab, master, isElite);
                return;
            }

            lock (pendingQueue)
            {
                if (pendingQueue.Count < 1)
                {
                    pendingQueue.Enqueue(prefab);
                    Debug.Log("[ChaosAuto] Queued: " + vb.name + " fly=" + isFlying);
                    return;
                }
            }

            TrySmartReplace(prefab, master, isElite);
        }

        private static bool IsEliteEnemy(CharacterMaster master)
        {
            return master?.inventory != null && master.inventory.currentEquipmentIndex != EquipmentIndex.None;
        }

        private (bool canSpawn, bool movementSlotFull, bool sameTypeAsAny) CheckSlotAvailability(CharacterBody vb)
        {
            int groundCount = allies.Count(a => a != null && a.healthComponent != null && a.healthComponent.alive && !a.isFlying);
            int flyingCount = allies.Count(a => a != null && a.healthComponent != null && a.healthComponent.alive && a.isFlying);
            bool movementSlotFull = vb.isFlying ? flyingCount >= 1 : groundCount >= 1;
            bool canSpawnNow = allies.Count(a => a != null && a.healthComponent != null && a.healthComponent.alive) < MAX_ALLIES;
            bool sameTypeAsAny = allies.Any(a => a != null && GetBodyType(a.name) == GetBodyType(vb.name));

            return (canSpawnNow, movementSlotFull, sameTypeAsAny);
        }

        private void TrySmartReplace(GameObject prefab, CharacterMaster victimMaster, bool isElite)
        {
            int weakestIdx = FindWeakestNonEliteAllyIndex();
            if (weakestIdx < 0) return;

            if (isElite == allyIsEliteStatus[weakestIdx]) return;
            if (!isElite && allyIsEliteStatus[weakestIdx]) return;

            var oldAlly = allies[weakestIdx];
            KillAlly(oldAlly);
            RemoveAllyAt(weakestIdx);
            SpawnMinion(prefab, victimMaster, isElite);
            Debug.Log("[ChaosAuto] Replaced weak ally with: " + prefab.name);
        }

        private int FindWeakestNonEliteAllyIndex()
        {
            int weakestIdx = -1;
            for (int i = 0; i < allies.Count; i++)
            {
                if (allies[i] == null || allies[i].healthComponent == null || !allies[i].healthComponent.alive) continue;
                if (allyIsEliteStatus[i]) continue;
                if (weakestIdx < 0) weakestIdx = i;
            }
            return weakestIdx;
        }

        private void RemoveAllyAt(int index)
        {
            allies.RemoveAt(index);
            allyTimers.RemoveAt(index);
            allyIsEliteStatus.RemoveAt(index);
            allyOriginalMaxHealth.RemoveAt(index);
        }

        private void FixedUpdate()
        {
            if (!body.hasAuthority || !body.isPlayerControlled) return;

            bool anyDied = SweepDeadAllies();
            if (anyDied)
            {
                Debug.Log("[ChaosAuto] Ally died, " + allies.Count + " remaining, 5s delay");
                respawnDelay = RESPAWN_DELAY;
                UpdateChaosMaster();
                return;
            }

            if (respawnDelay > 0f)
            {
                respawnDelay -= Time.fixedDeltaTime;
                if (respawnDelay <= 0f)
                {
                    respawnDelay = 0f;
                    SpawnFromQueue();
                }
                return;
            }

            if (allies.Count < MAX_ALLIES)
                SpawnFromQueue();

            AutoCastPickMaster();

            glowRefreshTimer -= Time.fixedDeltaTime;
            if (glowRefreshTimer <= 0f)
            {
                glowRefreshTimer = 1f;
                RefreshMinionGlows();
            }
        }

        private bool SweepDeadAllies()
        {
            bool anyDied = false;
            for (int i = allies.Count - 1; i >= 0; i--)
            {
                var ally = allies[i];
                if (ally == null || ally.healthComponent == null || !ally.healthComponent.alive)
                {
                    RemoveAllyAt(i);
                    anyDied = true;
                    continue;
                }

                allyTimers[i] -= Time.fixedDeltaTime;
                var hc = ally.healthComponent;

                if (allyTimers[i] <= 0f)
                {
                    hc.Networkhealth = 0f;
                    continue;
                }

                float ratio = allyTimers[i] / ALLY_DURATION;
                float target = allyOriginalMaxHealth[i] * ratio;
                float capped = Mathf.Max(1f, target);
                ally.baseMaxHealth = capped;
                if (hc.health > capped)
                    hc.Networkhealth = capped;
            }
            return anyDied;
        }
        private void AutoCastPickMaster()
        {
            if (fiMasterSkill == null || chaosBehaviour == null || fiTargetHurtbox == null || masterMarkBehaviourType == null) return;
            var skill = fiMasterSkill.GetValue(chaosBehaviour) as GenericSkill;
            if (skill == null || !skill.IsReady()) return;

            var candidates = new List<CharacterBody>();
            foreach (var ally in CharacterBody.readOnlyInstancesList)
            {
                if (ally == null || !ally.healthComponent || !ally.healthComponent.alive) continue;
                if (ally.teamComponent.teamIndex != TeamIndex.Player) continue;
                if (ally.isPlayerControlled) continue;
                if (ally.gameObject.GetComponent(masterMarkBehaviourType) != null) continue;
                if (Vector3.Distance(body.corePosition, ally.corePosition) > PICKMASTER_RANGE) continue;
                candidates.Add(ally);
            }

            if (candidates.Count == 0) return;
            var target = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            if (target.mainHurtBox == null) return;

            fiTargetHurtbox.SetValue(chaosBehaviour, target.mainHurtBox);
            skill.ExecuteIfReady();
            Invoke("RefreshMinionGlows", 0.7f);
        }
        private void InitGlowMaterials()
        {
            if (overlayMaterial == null) return;
            glowMatOrange = new Material(overlayMaterial);
            glowMatOrange.SetFloat("_AlphaBoost", 10f);
            glowMatOrange.SetFloat("_FresnelPower", 0.4f);

            glowMatRed = new Material(overlayMaterial);
            glowMatRed.SetFloat("_AlphaBoost", 10f);
            glowMatRed.SetFloat("_FresnelPower", 0.4f);
            glowMatRed.SetColor("_TintColor", new Color(1f, 0f, 0f, 0.5f));
        }

        private void ApplyGlow(CharacterBody target)
        {
            if (target == null || overlayType == null || overlayAddMethod == null) return;
            bool isMarked = masterMarkBehaviourType != null && target.gameObject.GetComponent(masterMarkBehaviourType) != null;
            var mat = isMarked ? glowMatRed : glowMatOrange;
            var tob = target.gameObject.GetComponent(overlayType);
            if (tob != null) Destroy(tob);
            var overlay = target.gameObject.AddComponent(overlayType);
            if (overlayTemporary != null) overlayTemporary.SetValue(overlay, false);
            overlayAddMethod.Invoke(overlay, new object[] { new Material(mat) });
        }

        private void RefreshMinionGlows()
        {
            foreach (var ally in allies)
            {
                if (ally == null || !ally.healthComponent.alive) continue;
                ApplyGlow(ally);
            }
        }

        private void SpawnFromQueue()
        {
            GameObject prefab;
            lock (pendingQueue)
            {
                if (pendingQueue.Count == 0) return;
                prefab = pendingQueue.Dequeue();
            }
            Debug.Log("[ChaosAuto] Spawning from queue: " + prefab.name);
            SpawnMinion(prefab, null, false);
        }

        private void SpawnMinion(GameObject prefab, CharacterMaster victimMaster, bool isElite)
        {
            try
            {
                Vector3 spawnPos = body.corePosition + (body.characterDirection ? body.characterDirection.forward : Vector3.forward) * 3f + Vector3.up * 1f;

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
                if (newMaster == null) return;

                CharacterBody newBody = newMaster.GetBody();
                if (newBody == null) return;

                newMaster.inventory.GiveItem(RoR2Content.Items.MinionLeash);

                if (isElite && victimMaster?.inventory != null && newMaster.inventory != null)
                {
                    var vi = victimMaster.inventory;
                    var ni = newMaster.inventory;
                    if (vi.currentEquipmentIndex != EquipmentIndex.None) ni.SetEquipmentIndex(vi.currentEquipmentIndex);
                    if (vi.alternateEquipmentIndex != EquipmentIndex.None) ni.SetEquipmentIndexForSlot(vi.alternateEquipmentIndex, 1);
                }

                newBody.baseMaxHealth *= 1.1f;
                newBody.baseMoveSpeed *= 1.1f;
                newBody.baseAttackSpeed *= 1.1f;

                HealthComponent hc = newBody.healthComponent;
                if (hc != null) hc.health = hc.fullCombinedHealth;

                newBody.gameObject.AddComponent<ResurrectedMinion>();

                if (masterMarkBuff != null && NetworkServer.active)
                    newBody.AddBuff(masterMarkBuff);

                if (overlayType != null && overlayAddMethod != null && overlayMaterial != null)
                    ApplyGlow(newBody);

                EnableAI(newMaster);

                allies.Add(newBody);
                allyTimers.Add(ALLY_DURATION);
                allyIsEliteStatus.Add(isElite);
                allyOriginalMaxHealth.Add(newBody.baseMaxHealth);
                UpdateChaosMaster();
            }
            catch (Exception e)
            {
                Debug.LogError("[AutoImprint] Spawn error: " + e.ToString());
            }
        }

        private void UpdateChaosMaster()
        {
            if (fiMaster == null || chaosBehaviour == null) return;
            var alive = allies.FirstOrDefault(a => a != null && a.healthComponent != null && a.healthComponent.alive);
            fiMaster.SetValue(chaosBehaviour, alive);
        }

        private void EnableAI(CharacterMaster master)
        {
            var ai = master.GetComponent("BaseAI");
            if (ai == null) return;
            var t = ai.GetType();
            t.GetMethod("SetBaseAIEnabled")?.Invoke(ai, new object[] { true });
            var leaderProp = t.GetProperty("leader");
            if (leaderProp != null)
            {
                var leader = leaderProp.GetValue(ai);
                leader.GetType().GetProperty("gameObject")?.SetValue(leader, body.gameObject);
            }
            var fp = t.GetField("followPattern");
            if (fp != null)
            {
                var val = fp.GetValue(ai);
                var valType = val.GetType();
                valType.GetField("minHorizontalDistance")?.SetValue(val, 2f);
                valType.GetField("maxHorizontalDistance")?.SetValue(val, 10f);
                fp.SetValue(ai, val);
            }
            t.GetField("enemyAttentionDuration")?.SetValue(ai, 8f);
        }

        private void KillAlly(CharacterBody ally)
        {
            if (ally == null) return;
            if (masterMarkBehaviourType != null)
            {
                var mmb = ally.gameObject.GetComponent(masterMarkBehaviourType);
                if (mmb != null) Destroy(mmb);
            }
            if (ally.healthComponent != null)
                ally.healthComponent.Suicide(null, null, DamageType.Generic);
            var master = ally.master;
            if (master != null && NetworkServer.active)
                master.DestroyBody();
            UpdateChaosMaster();
        }
    }
}