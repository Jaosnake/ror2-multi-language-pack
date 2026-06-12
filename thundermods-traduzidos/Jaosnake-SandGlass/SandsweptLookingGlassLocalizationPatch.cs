using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using LookingGlass.ItemStatsNameSpace;
using RoR2;
using UnityEngine;

namespace Jaosnake.SandsweptLookingGlassLocalizationPatch
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("droppod.lookingglass", "1.15.2")]
    [BepInDependency("com.TeamSandswept.Sandswept", "1.4.0")]
    public sealed class SandsweptLookingGlassLocalizationPatch : BaseUnityPlugin
    {
        private const string PluginGuid = "jaosnake.sandglass";
        private const string PluginName = "Sandswept LookingGlass Localization Patch";
        private const string PluginVersion = "1.0.0";

        private static readonly Dictionary<string, Dictionary<string, string>> LabelsByLanguage = BuildLanguageMaps();
        private static readonly Dictionary<string, List<string>> KnownPrefixesByEnglishLabel = BuildKnownPrefixes();

        private ConfigEntry<bool> enablePatch;
        private ConfigEntry<bool> logReplacements;
        private Coroutine delayedApplyRoutine;

        private void Awake()
        {
            enablePatch = Config.Bind("General", "Enable", true, "Enable Sandswept LookingGlass stat label localization.");
            logReplacements = Config.Bind("General", "LogReplacements", false, "Log how many LookingGlass labels were patched.");

            RoR2Application.onLoadFinished += OnLoadFinished;
            Language.onCurrentLanguageChanged += OnCurrentLanguageChanged;
            delayedApplyRoutine = StartCoroutine(DelayedApply());
        }

        private void OnDestroy()
        {
            RoR2Application.onLoadFinished -= OnLoadFinished;
            Language.onCurrentLanguageChanged -= OnCurrentLanguageChanged;

            if (delayedApplyRoutine != null)
            {
                StopCoroutine(delayedApplyRoutine);
                delayedApplyRoutine = null;
            }
        }

        private void OnLoadFinished()
        {
            ApplyPatch("load finished");
        }

        private void OnCurrentLanguageChanged()
        {
            ApplyPatch("language changed");
        }

        private IEnumerator DelayedApply()
        {
            for (var i = 0; i < 20; i++)
            {
                ApplyPatch("delayed apply");
                yield return new WaitForSeconds(0.5f);
            }
        }

        private void ApplyPatch(string reason)
        {
            if (!enablePatch.Value)
            {
                return;
            }

            var targetLabels = GetLabelsForCurrentLanguage();
            var replacements = 0;

            try
            {
                replacements += PatchItemDefinitions();
                replacements += PatchEquipmentDefinitions();

                if (logReplacements.Value && replacements > 0)
                {
                    Logger.LogInfo($"Patched {replacements} Sandswept LookingGlass label(s), reason: {reason}, language: {Language.currentLanguageName}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"Failed to patch Sandswept LookingGlass labels during {reason}: {ex}");
            }
        }

        private static Dictionary<string, string> GetLabelsForCurrentLanguage()
        {
            var language = NormalizeLanguageName(Language.currentLanguageName);

            if (LabelsByLanguage.TryGetValue(language, out var labels))
            {
                return labels;
            }

            if (language.StartsWith("ES", StringComparison.OrdinalIgnoreCase))
            {
                return LabelsByLanguage["ES"];
            }

            if (language.StartsWith("ZH", StringComparison.OrdinalIgnoreCase))
            {
                return LabelsByLanguage["ZH-CN"];
            }

            return LabelsByLanguage["EN"];
        }

        private static int PatchItemDefinitions()
        {
            var replacements = 0;

            if (ItemDefinitions.allItemDefinitions == null)
            {
                return 0;
            }

            foreach (var pair in ItemDefinitions.allItemDefinitions)
            {
                var itemDef = ItemCatalog.GetItemDef((ItemIndex)pair.Key);
                if (itemDef == null || !IsSandsweptDefinition(itemDef.name, itemDef.nameToken, itemDef.descriptionToken))
                {
                    continue;
                }

                replacements += PatchDescriptions(pair.Value);
            }

            return replacements;
        }

        private static int PatchEquipmentDefinitions()
        {
            var replacements = 0;

            if (ItemDefinitions.allEquipmentDefinitions == null)
            {
                return 0;
            }

            foreach (var pair in ItemDefinitions.allEquipmentDefinitions)
            {
                var equipmentDef = EquipmentCatalog.GetEquipmentDef((EquipmentIndex)pair.Key);
                if (equipmentDef == null || !IsSandsweptDefinition(equipmentDef.name, equipmentDef.nameToken, equipmentDef.descriptionToken))
                {
                    continue;
                }

                replacements += PatchDescriptions(pair.Value);
            }

            return replacements;
        }

        private static int PatchDescriptions(ItemStatsDef statsDef)
        {
            if (statsDef == null || statsDef.descriptions == null)
            {
                return 0;
            }

            var labels = GetLabelsForCurrentLanguage();
            var replacements = 0;

            for (var i = 0; i < statsDef.descriptions.Count; i++)
            {
                var current = statsDef.descriptions[i];
                var patched = PatchDescription(current, labels);
                if (!string.Equals(current, patched, StringComparison.Ordinal))
                {
                    statsDef.descriptions[i] = patched;
                    replacements++;
                }
            }

            return replacements;
        }

        private static string PatchDescription(string description, Dictionary<string, string> labels)
        {
            if (string.IsNullOrEmpty(description))
            {
                return description;
            }

            foreach (var englishLabel in LabelsByLanguage["EN"].Keys)
            {
                if (!KnownPrefixesByEnglishLabel.TryGetValue(englishLabel, out var knownPrefixes))
                {
                    continue;
                }

                foreach (var prefix in knownPrefixes)
                {
                    if (!description.StartsWith(prefix, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var targetPrefix = labels.TryGetValue(englishLabel, out var localizedLabel) ? localizedLabel : englishLabel;
                    return targetPrefix + description.Substring(prefix.Length);
                }
            }

            return description;
        }

        private static bool IsSandsweptDefinition(params string[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrEmpty(value) && value.IndexOf("SANDSWEPT", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static string NormalizeLanguageName(string languageName)
        {
            if (string.IsNullOrWhiteSpace(languageName))
            {
                return "EN";
            }

            var normalized = languageName.Trim().Replace('_', '-').ToUpperInvariant();
            if (normalized == "PTBR" || normalized == "PT-BR")
            {
                return "PT-BR";
            }

            return normalized;
        }

        private static Dictionary<string, List<string>> BuildKnownPrefixes()
        {
            var result = new Dictionary<string, List<string>>(StringComparer.Ordinal);

            foreach (var languageMap in LabelsByLanguage.Values)
            {
                foreach (var pair in languageMap)
                {
                    if (!result.TryGetValue(pair.Key, out var prefixes))
                    {
                        prefixes = new List<string>();
                        result[pair.Key] = prefixes;
                    }

                    if (!prefixes.Contains(pair.Value))
                    {
                        prefixes.Add(pair.Value);
                    }
                }
            }

            return result;
        }

        private static Dictionary<string, Dictionary<string, string>> BuildLanguageMaps()
        {
            var maps = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

            maps["EN"] = Map(
                "Extra Jump Chance: ", "Extra Jump Chance: ",
                "Max Extra Jumps: ", "Max Extra Jumps: ",
                "Base Regen: ", "Base Regen: ",
                "Level Scaled: ", "Level Scaled: ",
                "Extra Cloaked Chests: ", "Extra Cloaked Chests: ",
                "Healing: ", "Healing: ",
                "Special Skill Cooldown Reduction: ", "Special Skill Cooldown Reduction: ",
                "Decay Chance: ", "Decay Chance: ",
                "Movement Speed: ", "Movement Speed: ",
                "Fire Chance: ", "Fire Chance: ",
                "Base Damage: ", "Base Damage: ",
                "Crit Damage: ", "Crit Damage: ",
                "Extra Item Chance: ", "Extra Item Chance: ",
                "UwU~ Love is Love! ", "UwU~ Love is Love! ",
                "Barrier Decay Reduction: ", "Barrier Decay Reduction: ",
                "Stun and Burn Chance: ", "Stun and Burn Chance: ",
                "TOTAL Damage: ", "TOTAL Damage: ",
                "Burn Chance: ", "Burn Chance: ",
                "Damage Reduction: ", "Damage Reduction: ",
                "Attack Speed Reduction: ", "Attack Speed Reduction: ",
                "Tidal Cataclysm Chance: ", "Tidal Cataclysm Chance: ",
                "Explosion Radius: ", "Explosion Radius: ",
                "Curse Amount: ", "Curse Amount: ",
                "Interactable and Monster Count Increase: ", "Interactable and Monster Count Increase: ",
                "Health Loss Per Decay Stack: ", "Health Loss Per Decay Stack: ",
                "Maximum Decay Amount: ", "Maximum Decay Amount: ",
                "Pool Damage: ", "Pool Damage: ",
                "Pool Shield Damage: ", "Pool Shield Damage: ",
                "Hemorrhage Chance: ", "Hemorrhage Chance: ",
                "Javelin Damage: ", "Javelin Damage: ",
                "Stun to Freeze Chance: ", "Stun to Freeze Chance: ",
                "Challenge of the Mountain Chance: ", "Challenge of the Mountain Chance: ",
                "Link Damage: ", "Link Damage: ",
                "Missile Count: ", "Missile Count: ",
                "Maximum Missile Count: ", "Maximum Missile Count: ",
                "Extra Item Count: ", "Extra Item Count: ",
                "Overall Chest Reopen Count: ", "Overall Chest Reopen Count: ",
                "Maximum Chest Reopen Count Per Chest: ", "Maximum Chest Reopen Count Per Chest: ",
                "Difficulty Increase vs Rainstorm: ", "Difficulty Increase vs Rainstorm: ",
                "Difficulty Increase vs Selected Difficulty: ", "Difficulty Increase vs Selected Difficulty: ",
                "Plating: ", "Plating: ");

            maps["PT-BR"] = Map(
                "Extra Jump Chance: ", "Chance de salto extra: ",
                "Max Extra Jumps: ", "Máximo de saltos extras: ",
                "Base Regen: ", "Regeneração base: ",
                "Level Scaled: ", "Escala com nível: ",
                "Extra Cloaked Chests: ", "Baús camuflados extras: ",
                "Healing: ", "Cura: ",
                "Special Skill Cooldown Reduction: ", "Redução de recarga da habilidade especial: ",
                "Decay Chance: ", "Chance de decomposição: ",
                "Movement Speed: ", "Velocidade de movimento: ",
                "Fire Chance: ", "Chance de incendiar: ",
                "Base Damage: ", "Dano base: ",
                "Crit Damage: ", "Dano crítico: ",
                "Extra Item Chance: ", "Chance de item extra: ",
                "UwU~ Love is Love! ", "UwU~ Amor é amor! ",
                "Barrier Decay Reduction: ", "Redução de decaimento da barreira: ",
                "Stun and Burn Chance: ", "Chance de atordoar e incendiar: ",
                "TOTAL Damage: ", "Dano TOTAL: ",
                "Burn Chance: ", "Chance de queimadura: ",
                "Damage Reduction: ", "Redução de dano: ",
                "Attack Speed Reduction: ", "Redução de velocidade de ataque: ",
                "Tidal Cataclysm Chance: ", "Chance de Cataclismo das Marés: ",
                "Explosion Radius: ", "Raio da explosão: ",
                "Curse Amount: ", "Quantidade de maldição: ",
                "Interactable and Monster Count Increase: ", "Aumento de interagíveis e monstros: ",
                "Health Loss Per Decay Stack: ", "Perda de vida por acúmulo de decomposição: ",
                "Maximum Decay Amount: ", "Máximo de decomposição: ",
                "Pool Damage: ", "Dano da poça: ",
                "Pool Shield Damage: ", "Dano de escudo da poça: ",
                "Hemorrhage Chance: ", "Chance de hemorragia: ",
                "Javelin Damage: ", "Dano do dardo: ",
                "Stun to Freeze Chance: ", "Chance de atordoamento virar congelamento: ",
                "Challenge of the Mountain Chance: ", "Chance de Desafio da Montanha: ",
                "Link Damage: ", "Dano de vínculo: ",
                "Missile Count: ", "Quantidade de mísseis: ",
                "Maximum Missile Count: ", "Quantidade máxima de mísseis: ",
                "Extra Item Count: ", "Quantidade de itens extras: ",
                "Overall Chest Reopen Count: ", "Total de reaberturas de baús: ",
                "Maximum Chest Reopen Count Per Chest: ", "Máximo de reaberturas por baú: ",
                "Difficulty Increase vs Rainstorm: ", "Aumento de dificuldade vs. Tempestade: ",
                "Difficulty Increase vs Selected Difficulty: ", "Aumento de dificuldade vs. dificuldade escolhida: ",
                "Plating: ", "Blindagem: ");

            maps["DE"] = Map(
                "Extra Jump Chance: ", "Chance auf Extrasprung: ",
                "Max Extra Jumps: ", "Maximale Extrasprünge: ",
                "Base Regen: ", "Grundregeneration: ",
                "Level Scaled: ", "Skaliert mit Level: ",
                "Extra Cloaked Chests: ", "Zusätzliche getarnte Truhen: ",
                "Healing: ", "Heilung: ",
                "Special Skill Cooldown Reduction: ", "Abklingzeitreduktion der Spezialfähigkeit: ",
                "Decay Chance: ", "Verfallchance: ",
                "Movement Speed: ", "Bewegungsgeschwindigkeit: ",
                "Fire Chance: ", "Brandchance: ",
                "Base Damage: ", "Grundschaden: ",
                "Crit Damage: ", "Kritischer Schaden: ",
                "Extra Item Chance: ", "Chance auf Zusatzgegenstand: ",
                "UwU~ Love is Love! ", "UwU~ Liebe ist Liebe! ",
                "Barrier Decay Reduction: ", "Barrierenverfall-Reduktion: ",
                "Stun and Burn Chance: ", "Chance auf Betäubung und Verbrennung: ",
                "TOTAL Damage: ", "GESAMT-Schaden: ",
                "Burn Chance: ", "Verbrennungschance: ",
                "Damage Reduction: ", "Schadensreduktion: ",
                "Attack Speed Reduction: ", "Angriffstempo-Reduktion: ",
                "Tidal Cataclysm Chance: ", "Chance auf Gezeitenkataklysmus: ",
                "Explosion Radius: ", "Explosionsradius: ",
                "Curse Amount: ", "Fluchmenge: ",
                "Interactable and Monster Count Increase: ", "Erhöhung von Interagierbaren und Monstern: ",
                "Health Loss Per Decay Stack: ", "Lebensverlust pro Verfallsstapel: ",
                "Maximum Decay Amount: ", "Maximaler Verfall: ",
                "Pool Damage: ", "Lachenschaden: ",
                "Pool Shield Damage: ", "Lachenschildschaden: ",
                "Hemorrhage Chance: ", "Hämorrhagiechance: ",
                "Javelin Damage: ", "Speerschaden: ",
                "Stun to Freeze Chance: ", "Chance, Betäubung in Einfrieren umzuwandeln: ",
                "Challenge of the Mountain Chance: ", "Chance auf Herausforderung des Berges: ",
                "Link Damage: ", "Verbindungsschaden: ",
                "Missile Count: ", "Raketenanzahl: ",
                "Maximum Missile Count: ", "Maximale Raketenanzahl: ",
                "Extra Item Count: ", "Zusätzliche Gegenstände: ",
                "Overall Chest Reopen Count: ", "Gesamte Truhen-Wiederöffnungen: ",
                "Maximum Chest Reopen Count Per Chest: ", "Max. Wiederöffnungen pro Truhe: ",
                "Difficulty Increase vs Rainstorm: ", "Schwierigkeitsanstieg ggü. Rainstorm: ",
                "Difficulty Increase vs Selected Difficulty: ", "Schwierigkeitsanstieg ggü. gewählter Schwierigkeit: ",
                "Plating: ", "Panzerung: ");

            maps["ES"] = Map(
                "Extra Jump Chance: ", "Probabilidad de salto extra: ",
                "Max Extra Jumps: ", "Saltos extra máximos: ",
                "Base Regen: ", "Regeneración base: ",
                "Level Scaled: ", "Escala con el nivel: ",
                "Extra Cloaked Chests: ", "Cofres camuflados extra: ",
                "Healing: ", "Curación: ",
                "Special Skill Cooldown Reduction: ", "Reducción de recarga de la habilidad especial: ",
                "Decay Chance: ", "Probabilidad de deterioro: ",
                "Movement Speed: ", "Velocidad de movimiento: ",
                "Fire Chance: ", "Probabilidad de quemar: ",
                "Base Damage: ", "Daño base: ",
                "Crit Damage: ", "Daño crítico: ",
                "Extra Item Chance: ", "Probabilidad de objeto extra: ",
                "UwU~ Love is Love! ", "UwU~ ¡El amor es amor! ",
                "Barrier Decay Reduction: ", "Reducción del deterioro de barrera: ",
                "Stun and Burn Chance: ", "Probabilidad de aturdir y quemar: ",
                "TOTAL Damage: ", "Daño TOTAL: ",
                "Burn Chance: ", "Probabilidad de quemadura: ",
                "Damage Reduction: ", "Reducción de daño: ",
                "Attack Speed Reduction: ", "Reducción de velocidad de ataque: ",
                "Tidal Cataclysm Chance: ", "Probabilidad de Cataclismo de Marea: ",
                "Explosion Radius: ", "Radio de explosión: ",
                "Curse Amount: ", "Cantidad de maldición: ",
                "Interactable and Monster Count Increase: ", "Aumento de interactuables y monstruos: ",
                "Health Loss Per Decay Stack: ", "Pérdida de vida por acumulación de deterioro: ",
                "Maximum Decay Amount: ", "Deterioro máximo: ",
                "Pool Damage: ", "Daño de charco: ",
                "Pool Shield Damage: ", "Daño de escudo del charco: ",
                "Hemorrhage Chance: ", "Probabilidad de hemorragia: ",
                "Javelin Damage: ", "Daño de jabalina: ",
                "Stun to Freeze Chance: ", "Probabilidad de convertir aturdimiento en congelación: ",
                "Challenge of the Mountain Chance: ", "Probabilidad de Desafío de la Montaña: ",
                "Link Damage: ", "Daño de vínculo: ",
                "Missile Count: ", "Cantidad de misiles: ",
                "Maximum Missile Count: ", "Cantidad máxima de misiles: ",
                "Extra Item Count: ", "Cantidad de objetos extra: ",
                "Overall Chest Reopen Count: ", "Reaperturas totales de cofres: ",
                "Maximum Chest Reopen Count Per Chest: ", "Máx. reaperturas por cofre: ",
                "Difficulty Increase vs Rainstorm: ", "Aumento de dificultad vs. Rainstorm: ",
                "Difficulty Increase vs Selected Difficulty: ", "Aumento de dificultad vs. dificultad elegida: ",
                "Plating: ", "Blindaje: ");

            maps["FR"] = Map(
                "Extra Jump Chance: ", "Chance de saut supplémentaire : ",
                "Max Extra Jumps: ", "Sauts supplémentaires max. : ",
                "Base Regen: ", "Régénération de base : ",
                "Level Scaled: ", "S'adapte au niveau : ",
                "Extra Cloaked Chests: ", "Coffres camouflés supplémentaires : ",
                "Healing: ", "Soin : ",
                "Special Skill Cooldown Reduction: ", "Réduction du temps de recharge de la compétence spéciale : ",
                "Decay Chance: ", "Chance de décomposition : ",
                "Movement Speed: ", "Vitesse de déplacement : ",
                "Fire Chance: ", "Chance d'embraser : ",
                "Base Damage: ", "Dégâts de base : ",
                "Crit Damage: ", "Dégâts critiques : ",
                "Extra Item Chance: ", "Chance d'objet supplémentaire : ",
                "UwU~ Love is Love! ", "UwU~ L'amour est l'amour ! ",
                "Barrier Decay Reduction: ", "Réduction du déclin de barrière : ",
                "Stun and Burn Chance: ", "Chance d'étourdir et de brûler : ",
                "TOTAL Damage: ", "Dégâts TOTAUX : ",
                "Burn Chance: ", "Chance de brûlure : ",
                "Damage Reduction: ", "Réduction des dégâts : ",
                "Attack Speed Reduction: ", "Réduction de la vitesse d'attaque : ",
                "Tidal Cataclysm Chance: ", "Chance de Cataclysme des marées : ",
                "Explosion Radius: ", "Rayon d'explosion : ",
                "Curse Amount: ", "Quantité de malédiction : ",
                "Interactable and Monster Count Increase: ", "Augmentation des interactifs et des monstres : ",
                "Health Loss Per Decay Stack: ", "Perte de vie par cumul de décomposition : ",
                "Maximum Decay Amount: ", "Décomposition maximale : ",
                "Pool Damage: ", "Dégâts de flaque : ",
                "Pool Shield Damage: ", "Dégâts de bouclier de flaque : ",
                "Hemorrhage Chance: ", "Chance d'hémorragie : ",
                "Javelin Damage: ", "Dégâts de javelot : ",
                "Stun to Freeze Chance: ", "Chance de convertir l'étourdissement en gel : ",
                "Challenge of the Mountain Chance: ", "Chance de Défi de la montagne : ",
                "Link Damage: ", "Dégâts de lien : ",
                "Missile Count: ", "Nombre de missiles : ",
                "Maximum Missile Count: ", "Nombre maximal de missiles : ",
                "Extra Item Count: ", "Nombre d'objets supplémentaires : ",
                "Overall Chest Reopen Count: ", "Réouvertures totales de coffres : ",
                "Maximum Chest Reopen Count Per Chest: ", "Réouvertures max. par coffre : ",
                "Difficulty Increase vs Rainstorm: ", "Hausse de difficulté vs Rainstorm : ",
                "Difficulty Increase vs Selected Difficulty: ", "Hausse de difficulté vs difficulté choisie : ",
                "Plating: ", "Blindage : ");

            maps["IT"] = Map(
                "Extra Jump Chance: ", "Probabilità di salto extra: ",
                "Max Extra Jumps: ", "Salti extra massimi: ",
                "Base Regen: ", "Rigenerazione base: ",
                "Level Scaled: ", "Scala con il livello: ",
                "Extra Cloaked Chests: ", "Forzieri occultati extra: ",
                "Healing: ", "Cura: ",
                "Special Skill Cooldown Reduction: ", "Riduzione ricarica abilità speciale: ",
                "Decay Chance: ", "Probabilità di decadimento: ",
                "Movement Speed: ", "Velocità di movimento: ",
                "Fire Chance: ", "Probabilità di incendiare: ",
                "Base Damage: ", "Danno base: ",
                "Crit Damage: ", "Danno critico: ",
                "Extra Item Chance: ", "Probabilità di oggetto extra: ",
                "UwU~ Love is Love! ", "UwU~ L'amore è amore! ",
                "Barrier Decay Reduction: ", "Riduzione decadimento barriera: ",
                "Stun and Burn Chance: ", "Probabilità di stordire e bruciare: ",
                "TOTAL Damage: ", "Danno TOTALE: ",
                "Burn Chance: ", "Probabilità di bruciatura: ",
                "Damage Reduction: ", "Riduzione danni: ",
                "Attack Speed Reduction: ", "Riduzione velocità d'attacco: ",
                "Tidal Cataclysm Chance: ", "Probabilità di Cataclisma Mareale: ",
                "Explosion Radius: ", "Raggio esplosione: ",
                "Curse Amount: ", "Quantità di maledizione: ",
                "Interactable and Monster Count Increase: ", "Aumento di interagibili e mostri: ",
                "Health Loss Per Decay Stack: ", "Perdita salute per accumulo di decadimento: ",
                "Maximum Decay Amount: ", "Decadimento massimo: ",
                "Pool Damage: ", "Danno della pozza: ",
                "Pool Shield Damage: ", "Danno allo scudo della pozza: ",
                "Hemorrhage Chance: ", "Probabilità di emorragia: ",
                "Javelin Damage: ", "Danno del giavellotto: ",
                "Stun to Freeze Chance: ", "Probabilità di convertire stordimento in congelamento: ",
                "Challenge of the Mountain Chance: ", "Probabilità di Sfida della Montagna: ",
                "Link Damage: ", "Danno del legame: ",
                "Missile Count: ", "Numero di missili: ",
                "Maximum Missile Count: ", "Numero massimo di missili: ",
                "Extra Item Count: ", "Numero di oggetti extra: ",
                "Overall Chest Reopen Count: ", "Riaperture totali dei forzieri: ",
                "Maximum Chest Reopen Count Per Chest: ", "Riaperture max per forziere: ",
                "Difficulty Increase vs Rainstorm: ", "Aumento difficoltà vs Rainstorm: ",
                "Difficulty Increase vs Selected Difficulty: ", "Aumento difficoltà vs difficoltà scelta: ",
                "Plating: ", "Corazzatura: ");

            maps["JA"] = Map(
                "Extra Jump Chance: ", "追加ジャンプ確率: ",
                "Max Extra Jumps: ", "最大追加ジャンプ数: ",
                "Base Regen: ", "基本回復: ",
                "Level Scaled: ", "レベルでスケール: ",
                "Extra Cloaked Chests: ", "追加の透明チェスト: ",
                "Healing: ", "回復: ",
                "Special Skill Cooldown Reduction: ", "特殊スキルのクールダウン短縮: ",
                "Decay Chance: ", "崩壊確率: ",
                "Movement Speed: ", "移動速度: ",
                "Fire Chance: ", "炎上確率: ",
                "Base Damage: ", "基本ダメージ: ",
                "Crit Damage: ", "クリティカルダメージ: ",
                "Extra Item Chance: ", "追加アイテム確率: ",
                "UwU~ Love is Love! ", "UwU~ 愛は愛! ",
                "Barrier Decay Reduction: ", "バリア減衰軽減: ",
                "Stun and Burn Chance: ", "スタンと炎上の確率: ",
                "TOTAL Damage: ", "合計ダメージ: ",
                "Burn Chance: ", "火傷確率: ",
                "Damage Reduction: ", "ダメージ軽減: ",
                "Attack Speed Reduction: ", "攻撃速度低下: ",
                "Tidal Cataclysm Chance: ", "潮汐大災害の確率: ",
                "Explosion Radius: ", "爆発範囲: ",
                "Curse Amount: ", "呪い量: ",
                "Interactable and Monster Count Increase: ", "インタラクト対象とモンスター数増加: ",
                "Health Loss Per Decay Stack: ", "崩壊スタックごとの体力減少: ",
                "Maximum Decay Amount: ", "最大崩壊量: ",
                "Pool Damage: ", "プールダメージ: ",
                "Pool Shield Damage: ", "プールシールドダメージ: ",
                "Hemorrhage Chance: ", "出血確率: ",
                "Javelin Damage: ", "ジャベリンダメージ: ",
                "Stun to Freeze Chance: ", "スタンを凍結に変える確率: ",
                "Challenge of the Mountain Chance: ", "山の試練の確率: ",
                "Link Damage: ", "リンクダメージ: ",
                "Missile Count: ", "ミサイル数: ",
                "Maximum Missile Count: ", "最大ミサイル数: ",
                "Extra Item Count: ", "追加アイテム数: ",
                "Overall Chest Reopen Count: ", "合計チェスト再開封数: ",
                "Maximum Chest Reopen Count Per Chest: ", "チェストごとの最大再開封数: ",
                "Difficulty Increase vs Rainstorm: ", "Rainstorm比の難易度上昇: ",
                "Difficulty Increase vs Selected Difficulty: ", "選択難易度比の難易度上昇: ",
                "Plating: ", "装甲: ");

            maps["KO"] = Map(
                "Extra Jump Chance: ", "추가 점프 확률: ",
                "Max Extra Jumps: ", "최대 추가 점프: ",
                "Base Regen: ", "기본 재생: ",
                "Level Scaled: ", "레벨 비례: ",
                "Extra Cloaked Chests: ", "추가 은폐 상자: ",
                "Healing: ", "치유: ",
                "Special Skill Cooldown Reduction: ", "특수 기술 쿨다운 감소: ",
                "Decay Chance: ", "부패 확률: ",
                "Movement Speed: ", "이동 속도: ",
                "Fire Chance: ", "화상 확률: ",
                "Base Damage: ", "기본 피해: ",
                "Crit Damage: ", "치명타 피해: ",
                "Extra Item Chance: ", "추가 아이템 확률: ",
                "UwU~ Love is Love! ", "UwU~ 사랑은 사랑! ",
                "Barrier Decay Reduction: ", "보호막 감소 완화: ",
                "Stun and Burn Chance: ", "기절 및 화상 확률: ",
                "TOTAL Damage: ", "총 피해: ",
                "Burn Chance: ", "화상 확률: ",
                "Damage Reduction: ", "피해 감소: ",
                "Attack Speed Reduction: ", "공격 속도 감소: ",
                "Tidal Cataclysm Chance: ", "해일 대격변 확률: ",
                "Explosion Radius: ", "폭발 반경: ",
                "Curse Amount: ", "저주량: ",
                "Interactable and Monster Count Increase: ", "상호작용 대상 및 몬스터 수 증가: ",
                "Health Loss Per Decay Stack: ", "부패 중첩당 체력 손실: ",
                "Maximum Decay Amount: ", "최대 부패량: ",
                "Pool Damage: ", "웅덩이 피해: ",
                "Pool Shield Damage: ", "웅덩이 보호막 피해: ",
                "Hemorrhage Chance: ", "출혈 확률: ",
                "Javelin Damage: ", "투창 피해: ",
                "Stun to Freeze Chance: ", "기절을 빙결로 전환할 확률: ",
                "Challenge of the Mountain Chance: ", "산의 도전 확률: ",
                "Link Damage: ", "연결 피해: ",
                "Missile Count: ", "미사일 수: ",
                "Maximum Missile Count: ", "최대 미사일 수: ",
                "Extra Item Count: ", "추가 아이템 수: ",
                "Overall Chest Reopen Count: ", "전체 상자 재개봉 수: ",
                "Maximum Chest Reopen Count Per Chest: ", "상자당 최대 재개봉 수: ",
                "Difficulty Increase vs Rainstorm: ", "Rainstorm 대비 난이도 증가: ",
                "Difficulty Increase vs Selected Difficulty: ", "선택 난이도 대비 난이도 증가: ",
                "Plating: ", "장갑: ");

            maps["PL"] = Map(
                "Extra Jump Chance: ", "Szansa na dodatkowy skok: ",
                "Max Extra Jumps: ", "Maks. dodatkowe skoki: ",
                "Base Regen: ", "Podstawowa regeneracja: ",
                "Level Scaled: ", "Skaluje się z poziomem: ",
                "Extra Cloaked Chests: ", "Dodatkowe ukryte skrzynie: ",
                "Healing: ", "Leczenie: ",
                "Special Skill Cooldown Reduction: ", "Skrócenie odnowienia umiejętności specjalnej: ",
                "Decay Chance: ", "Szansa na rozkład: ",
                "Movement Speed: ", "Szybkość ruchu: ",
                "Fire Chance: ", "Szansa podpalenia: ",
                "Base Damage: ", "Obrażenia bazowe: ",
                "Crit Damage: ", "Obrażenia krytyczne: ",
                "Extra Item Chance: ", "Szansa na dodatkowy przedmiot: ",
                "UwU~ Love is Love! ", "UwU~ Miłość to miłość! ",
                "Barrier Decay Reduction: ", "Redukcja zaniku bariery: ",
                "Stun and Burn Chance: ", "Szansa ogłuszenia i podpalenia: ",
                "TOTAL Damage: ", "CAŁKOWITE obrażenia: ",
                "Burn Chance: ", "Szansa poparzenia: ",
                "Damage Reduction: ", "Redukcja obrażeń: ",
                "Attack Speed Reduction: ", "Redukcja szybkości ataku: ",
                "Tidal Cataclysm Chance: ", "Szansa na Kataklizm Przypływu: ",
                "Explosion Radius: ", "Promień eksplozji: ",
                "Curse Amount: ", "Ilość klątwy: ",
                "Interactable and Monster Count Increase: ", "Wzrost liczby interaktywnych obiektów i potworów: ",
                "Health Loss Per Decay Stack: ", "Utrata zdrowia za każdy stos rozkładu: ",
                "Maximum Decay Amount: ", "Maksymalny rozkład: ",
                "Pool Damage: ", "Obrażenia kałuży: ",
                "Pool Shield Damage: ", "Obrażenia tarczy kałuży: ",
                "Hemorrhage Chance: ", "Szansa krwotoku: ",
                "Javelin Damage: ", "Obrażenia oszczepu: ",
                "Stun to Freeze Chance: ", "Szansa zmiany ogłuszenia w zamrożenie: ",
                "Challenge of the Mountain Chance: ", "Szansa na Wyzwanie Góry: ",
                "Link Damage: ", "Obrażenia połączenia: ",
                "Missile Count: ", "Liczba pocisków: ",
                "Maximum Missile Count: ", "Maks. liczba pocisków: ",
                "Extra Item Count: ", "Liczba dodatkowych przedmiotów: ",
                "Overall Chest Reopen Count: ", "Łączna liczba ponownych otwarć skrzyń: ",
                "Maximum Chest Reopen Count Per Chest: ", "Maks. ponowne otwarcia na skrzynię: ",
                "Difficulty Increase vs Rainstorm: ", "Wzrost trudności względem Rainstorm: ",
                "Difficulty Increase vs Selected Difficulty: ", "Wzrost trudności względem wybranej trudności: ",
                "Plating: ", "Opancerzenie: ");

            maps["RU"] = Map(
                "Extra Jump Chance: ", "Шанс дополнительного прыжка: ",
                "Max Extra Jumps: ", "Макс. дополнительных прыжков: ",
                "Base Regen: ", "Базовая регенерация: ",
                "Level Scaled: ", "Масштабируется с уровнем: ",
                "Extra Cloaked Chests: ", "Дополнительные скрытые сундуки: ",
                "Healing: ", "Лечение: ",
                "Special Skill Cooldown Reduction: ", "Сокращение перезарядки особого навыка: ",
                "Decay Chance: ", "Шанс разложения: ",
                "Movement Speed: ", "Скорость передвижения: ",
                "Fire Chance: ", "Шанс поджога: ",
                "Base Damage: ", "Базовый урон: ",
                "Crit Damage: ", "Критический урон: ",
                "Extra Item Chance: ", "Шанс дополнительного предмета: ",
                "UwU~ Love is Love! ", "UwU~ Любовь есть любовь! ",
                "Barrier Decay Reduction: ", "Снижение распада барьера: ",
                "Stun and Burn Chance: ", "Шанс оглушения и поджога: ",
                "TOTAL Damage: ", "ОБЩИЙ урон: ",
                "Burn Chance: ", "Шанс ожога: ",
                "Damage Reduction: ", "Снижение урона: ",
                "Attack Speed Reduction: ", "Снижение скорости атаки: ",
                "Tidal Cataclysm Chance: ", "Шанс Приливного катаклизма: ",
                "Explosion Radius: ", "Радиус взрыва: ",
                "Curse Amount: ", "Величина проклятия: ",
                "Interactable and Monster Count Increase: ", "Увеличение интерактивных объектов и монстров: ",
                "Health Loss Per Decay Stack: ", "Потеря здоровья за стак разложения: ",
                "Maximum Decay Amount: ", "Максимальное разложение: ",
                "Pool Damage: ", "Урон лужи: ",
                "Pool Shield Damage: ", "Урон лужи по щиту: ",
                "Hemorrhage Chance: ", "Шанс кровотечения: ",
                "Javelin Damage: ", "Урон копья: ",
                "Stun to Freeze Chance: ", "Шанс превратить оглушение в заморозку: ",
                "Challenge of the Mountain Chance: ", "Шанс Испытания горы: ",
                "Link Damage: ", "Урон связи: ",
                "Missile Count: ", "Количество ракет: ",
                "Maximum Missile Count: ", "Макс. количество ракет: ",
                "Extra Item Count: ", "Количество доп. предметов: ",
                "Overall Chest Reopen Count: ", "Всего повторных открытий сундуков: ",
                "Maximum Chest Reopen Count Per Chest: ", "Макс. повторных открытий на сундук: ",
                "Difficulty Increase vs Rainstorm: ", "Рост сложности относительно Rainstorm: ",
                "Difficulty Increase vs Selected Difficulty: ", "Рост сложности относительно выбранной сложности: ",
                "Plating: ", "Бронепластина: ");

            maps["TR"] = Map(
                "Extra Jump Chance: ", "Ekstra zıplama şansı: ",
                "Max Extra Jumps: ", "Maks. ekstra zıplama: ",
                "Base Regen: ", "Temel yenilenme: ",
                "Level Scaled: ", "Seviyeye göre ölçeklenir: ",
                "Extra Cloaked Chests: ", "Ekstra gizli sandıklar: ",
                "Healing: ", "İyileştirme: ",
                "Special Skill Cooldown Reduction: ", "Özel yetenek bekleme süresi azalması: ",
                "Decay Chance: ", "Çürüme şansı: ",
                "Movement Speed: ", "Hareket hızı: ",
                "Fire Chance: ", "Yakma şansı: ",
                "Base Damage: ", "Temel hasar: ",
                "Crit Damage: ", "Kritik hasar: ",
                "Extra Item Chance: ", "Ekstra eşya şansı: ",
                "UwU~ Love is Love! ", "UwU~ Aşk aşktır! ",
                "Barrier Decay Reduction: ", "Bariyer azalması düşüşü: ",
                "Stun and Burn Chance: ", "Sersemletme ve yakma şansı: ",
                "TOTAL Damage: ", "TOPLAM hasar: ",
                "Burn Chance: ", "Yanma şansı: ",
                "Damage Reduction: ", "Hasar azaltma: ",
                "Attack Speed Reduction: ", "Saldırı hızı azaltma: ",
                "Tidal Cataclysm Chance: ", "Gelgit Felaketi şansı: ",
                "Explosion Radius: ", "Patlama yarıçapı: ",
                "Curse Amount: ", "Lanet miktarı: ",
                "Interactable and Monster Count Increase: ", "Etkileşimli nesne ve canavar sayısı artışı: ",
                "Health Loss Per Decay Stack: ", "Her çürüme yükü başına can kaybı: ",
                "Maximum Decay Amount: ", "Maksimum çürüme miktarı: ",
                "Pool Damage: ", "Havuz hasarı: ",
                "Pool Shield Damage: ", "Havuz kalkan hasarı: ",
                "Hemorrhage Chance: ", "Kanama şansı: ",
                "Javelin Damage: ", "Cirit hasarı: ",
                "Stun to Freeze Chance: ", "Sersemletmeyi donmaya çevirme şansı: ",
                "Challenge of the Mountain Chance: ", "Dağın Meydan Okuması şansı: ",
                "Link Damage: ", "Bağ hasarı: ",
                "Missile Count: ", "Füze sayısı: ",
                "Maximum Missile Count: ", "Maksimum füze sayısı: ",
                "Extra Item Count: ", "Ekstra eşya sayısı: ",
                "Overall Chest Reopen Count: ", "Toplam sandık tekrar açma sayısı: ",
                "Maximum Chest Reopen Count Per Chest: ", "Sandık başına maks. tekrar açma: ",
                "Difficulty Increase vs Rainstorm: ", "Rainstorm'a göre zorluk artışı: ",
                "Difficulty Increase vs Selected Difficulty: ", "Seçilen zorluğa göre zorluk artışı: ",
                "Plating: ", "Zırh kaplama: ");

            maps["UA"] = Map(
                "Extra Jump Chance: ", "Шанс додаткового стрибка: ",
                "Max Extra Jumps: ", "Макс. додаткових стрибків: ",
                "Base Regen: ", "Базове відновлення: ",
                "Level Scaled: ", "Масштабується з рівнем: ",
                "Extra Cloaked Chests: ", "Додаткові приховані скрині: ",
                "Healing: ", "Лікування: ",
                "Special Skill Cooldown Reduction: ", "Скорочення перезарядки особливої навички: ",
                "Decay Chance: ", "Шанс розпаду: ",
                "Movement Speed: ", "Швидкість руху: ",
                "Fire Chance: ", "Шанс підпалу: ",
                "Base Damage: ", "Базова шкода: ",
                "Crit Damage: ", "Критична шкода: ",
                "Extra Item Chance: ", "Шанс додаткового предмета: ",
                "UwU~ Love is Love! ", "UwU~ Любов є любов! ",
                "Barrier Decay Reduction: ", "Зменшення спадання бар'єра: ",
                "Stun and Burn Chance: ", "Шанс оглушити й підпалити: ",
                "TOTAL Damage: ", "ЗАГАЛЬНА шкода: ",
                "Burn Chance: ", "Шанс опіку: ",
                "Damage Reduction: ", "Зменшення шкоди: ",
                "Attack Speed Reduction: ", "Зменшення швидкості атаки: ",
                "Tidal Cataclysm Chance: ", "Шанс Припливного катаклізму: ",
                "Explosion Radius: ", "Радіус вибуху: ",
                "Curse Amount: ", "Кількість прокляття: ",
                "Interactable and Monster Count Increase: ", "Збільшення інтерактивних об'єктів і монстрів: ",
                "Health Loss Per Decay Stack: ", "Втрата здоров'я за стак розпаду: ",
                "Maximum Decay Amount: ", "Максимальна кількість розпаду: ",
                "Pool Damage: ", "Шкода калюжі: ",
                "Pool Shield Damage: ", "Шкода калюжі по щиту: ",
                "Hemorrhage Chance: ", "Шанс кровотечі: ",
                "Javelin Damage: ", "Шкода списа: ",
                "Stun to Freeze Chance: ", "Шанс перетворити оглушення на замороження: ",
                "Challenge of the Mountain Chance: ", "Шанс Виклику гори: ",
                "Link Damage: ", "Шкода зв'язку: ",
                "Missile Count: ", "Кількість ракет: ",
                "Maximum Missile Count: ", "Макс. кількість ракет: ",
                "Extra Item Count: ", "Кількість додаткових предметів: ",
                "Overall Chest Reopen Count: ", "Загальна кількість повторних відкриттів скринь: ",
                "Maximum Chest Reopen Count Per Chest: ", "Макс. повторних відкриттів на скриню: ",
                "Difficulty Increase vs Rainstorm: ", "Зростання складності відносно Rainstorm: ",
                "Difficulty Increase vs Selected Difficulty: ", "Зростання складності відносно обраної складності: ",
                "Plating: ", "Бронювання: ");

            maps["ZH-CN"] = Map(
                "Extra Jump Chance: ", "额外跳跃概率：",
                "Max Extra Jumps: ", "最大额外跳跃次数：",
                "Base Regen: ", "基础回复：",
                "Level Scaled: ", "随等级提升：",
                "Extra Cloaked Chests: ", "额外隐形箱子：",
                "Healing: ", "治疗：",
                "Special Skill Cooldown Reduction: ", "特殊技能冷却缩减：",
                "Decay Chance: ", "衰败概率：",
                "Movement Speed: ", "移动速度：",
                "Fire Chance: ", "点燃概率：",
                "Base Damage: ", "基础伤害：",
                "Crit Damage: ", "暴击伤害：",
                "Extra Item Chance: ", "额外物品概率：",
                "UwU~ Love is Love! ", "UwU~ 爱就是爱！",
                "Barrier Decay Reduction: ", "屏障衰减降低：",
                "Stun and Burn Chance: ", "眩晕并点燃概率：",
                "TOTAL Damage: ", "总伤害：",
                "Burn Chance: ", "燃烧概率：",
                "Damage Reduction: ", "伤害减免：",
                "Attack Speed Reduction: ", "攻击速度降低：",
                "Tidal Cataclysm Chance: ", "潮汐灾变概率：",
                "Explosion Radius: ", "爆炸半径：",
                "Curse Amount: ", "诅咒量：",
                "Interactable and Monster Count Increase: ", "可交互物与怪物数量增加：",
                "Health Loss Per Decay Stack: ", "每层衰败生命损失：",
                "Maximum Decay Amount: ", "最大衰败量：",
                "Pool Damage: ", "池伤害：",
                "Pool Shield Damage: ", "池护盾伤害：",
                "Hemorrhage Chance: ", "出血概率：",
                "Javelin Damage: ", "标枪伤害：",
                "Stun to Freeze Chance: ", "眩晕转冻结概率：",
                "Challenge of the Mountain Chance: ", "山之挑战概率：",
                "Link Damage: ", "链接伤害：",
                "Missile Count: ", "导弹数量：",
                "Maximum Missile Count: ", "最大导弹数量：",
                "Extra Item Count: ", "额外物品数量：",
                "Overall Chest Reopen Count: ", "总箱子重开次数：",
                "Maximum Chest Reopen Count Per Chest: ", "每个箱子最大重开次数：",
                "Difficulty Increase vs Rainstorm: ", "相对 Rainstorm 难度提升：",
                "Difficulty Increase vs Selected Difficulty: ", "相对所选难度提升：",
                "Plating: ", "护甲板：");

            maps["ZH-TW"] = Map(
                "Extra Jump Chance: ", "額外跳躍機率：",
                "Max Extra Jumps: ", "最大額外跳躍次數：",
                "Base Regen: ", "基礎恢復：",
                "Level Scaled: ", "隨等級提升：",
                "Extra Cloaked Chests: ", "額外隱形箱子：",
                "Healing: ", "治療：",
                "Special Skill Cooldown Reduction: ", "特殊技能冷卻縮減：",
                "Decay Chance: ", "衰敗機率：",
                "Movement Speed: ", "移動速度：",
                "Fire Chance: ", "點燃機率：",
                "Base Damage: ", "基礎傷害：",
                "Crit Damage: ", "暴擊傷害：",
                "Extra Item Chance: ", "額外物品機率：",
                "UwU~ Love is Love! ", "UwU~ 愛就是愛！",
                "Barrier Decay Reduction: ", "屏障衰減降低：",
                "Stun and Burn Chance: ", "暈眩並點燃機率：",
                "TOTAL Damage: ", "總傷害：",
                "Burn Chance: ", "燃燒機率：",
                "Damage Reduction: ", "傷害減免：",
                "Attack Speed Reduction: ", "攻擊速度降低：",
                "Tidal Cataclysm Chance: ", "潮汐災變機率：",
                "Explosion Radius: ", "爆炸半徑：",
                "Curse Amount: ", "詛咒量：",
                "Interactable and Monster Count Increase: ", "可互動物與怪物數量增加：",
                "Health Loss Per Decay Stack: ", "每層衰敗生命損失：",
                "Maximum Decay Amount: ", "最大衰敗量：",
                "Pool Damage: ", "池傷害：",
                "Pool Shield Damage: ", "池護盾傷害：",
                "Hemorrhage Chance: ", "出血機率：",
                "Javelin Damage: ", "標槍傷害：",
                "Stun to Freeze Chance: ", "暈眩轉冰凍機率：",
                "Challenge of the Mountain Chance: ", "山之挑戰機率：",
                "Link Damage: ", "連結傷害：",
                "Missile Count: ", "飛彈數量：",
                "Maximum Missile Count: ", "最大飛彈數量：",
                "Extra Item Count: ", "額外物品數量：",
                "Overall Chest Reopen Count: ", "總箱子重開次數：",
                "Maximum Chest Reopen Count Per Chest: ", "每個箱子最大重開次數：",
                "Difficulty Increase vs Rainstorm: ", "相對 Rainstorm 難度提升：",
                "Difficulty Increase vs Selected Difficulty: ", "相對所選難度提升：",
                "Plating: ", "護甲板：");

            return maps;
        }

        private static Dictionary<string, string> Map(params string[] values)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);

            for (var i = 0; i + 1 < values.Length; i += 2)
            {
                result[values[i]] = values[i + 1];
            }

            return result;
        }
    }
}

