using System;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using NS_KingModUtilities;
using RoR2;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PELE.KingDBZFix
{
    [BepInPlugin("jaosnake.pele.kingdbzfix", "PELE - KingModUtilities Localization Fix", "1.0.0")]
    [BepInDependency("com.Kingpinush.KingModUtilities", BepInDependency.DependencyFlags.HardDependency)]
    public class KingDBZFixPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;

            // Safe soft-dependency check
            if (!BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Kingpinush.KingModUtilities"))
            {
                Log.LogInfo("KingModUtilities não detectado. Patch de localização DBZ ignorado.");
                return;
            }

            try
            {
                _harmony = new Harmony("jaosnake.pele.kingdbzfix");
                _harmony.PatchAll(typeof(KingDBZFixPlugin));
                _harmony.PatchAll(typeof(TransformationsMenuPatches));
                _harmony.PatchAll(typeof(FormattingPatches));
                _harmony.PatchAll(typeof(TutorialPatches));
                Log.LogInfo("PELE - KingModUtilities Localization Fix (Goku, Vegeta, Trunks) carregado com sucesso!");
            }
            catch (Exception ex)
            {
                Log.LogError("Erro ao aplicar patch no KingModUtilities: " + ex);
            }
        }
    }

    public static class TransformationsMenuPatches
    {
        [HarmonyPatch(typeof(UI_TransformationsMenu), "UpdateTooltip")]
        [HarmonyPostfix]
        public static void Postfix_UpdateTooltip(UI_TransformationsMenu __instance)
        {
            try
            {
                if (__instance == null) return;
                var tr = Traverse.Create(__instance);

                // 1. Transformation Title
                var transNameText = tr.Field<Text>("m_mainToolTipTransformationNameText")?.Value;
                if (transNameText != null && !string.IsNullOrEmpty(transNameText.text))
                {
                    transNameText.text = RoR2.Language.GetString(transNameText.text, transNameText.text);
                }

                // 2. Skill Name
                var skillNameText = tr.Field<Text>("m_mainToolTipSkillNameText")?.Value;
                if (skillNameText != null && !string.IsNullOrEmpty(skillNameText.text))
                {
                    skillNameText.text = RoR2.Language.GetString(skillNameText.text, skillNameText.text);
                }

                // 3. Skill Description
                var skillText = tr.Field<Text>("m_mainToolTipSkillText")?.Value;
                if (skillText != null && !string.IsNullOrEmpty(skillText.text))
                {
                    skillText.text = LocalizeSkillDescription(skillText.text);
                }

                // 4. Stats Tooltip
                var statsText = tr.Field<Text>("m_mainTooltipText")?.Value;
                if (statsText != null && !string.IsNullOrEmpty(statsText.text))
                {
                    statsText.text = LocalizeStatsText(statsText.text);
                }

                // 5. Buttons
                var removeBtn = tr.Field<Button>("m_removeTransformationButton")?.Value;
                if (removeBtn != null)
                {
                    var tmp = removeBtn.GetComponentInChildren<Text>();
                    if (tmp != null && tmp.text == "REMOVE")
                    {
                        tmp.text = RoR2.Language.GetString("KINGMOD_REMOVE", "REMOVE");
                    }
                }

                var showExtraBtn = tr.Field<Button>("m_showExtraTransformationButton")?.Value;
                if (showExtraBtn != null)
                {
                    var tmp = showExtraBtn.GetComponentInChildren<Text>();
                    if (tmp != null && (tmp.text == "TOGGLE EXTRA" || tmp.text.StartsWith("TOGGLE")))
                    {
                        tmp.text = RoR2.Language.GetString("KINGMOD_TOGGLE_EXTRA", "TOGGLE EXTRA");
                    }
                }

            }
            catch (Exception ex)
            {
                KingDBZFixPlugin.Log.LogWarning("Erro em Postfix_UpdateTooltip: " + ex.Message);
            }
        }

        public static string LocalizeSkillDescription(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            // Direct token lookup
            var direct = RoR2.Language.GetString(text, text);
            if (direct != text) return direct;

            // Handle "Costs <color=#03f4fc>200 Ki</color>\n..." pattern
            var m = Regex.Match(text, @"^Costs\s+(<color=[^>]+>\d+\s*Ki</color>)\s*\n([\s\S]*)$", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                var kiPart = m.Groups[1].Value;
                var body = m.Groups[2].Value.Trim();

                var costPrefix = RoR2.Language.GetString("KINGMOD_COSTS_PREFIX", "Costs ");
                var localizedBody = RoR2.Language.GetString(body, body);

                return costPrefix + kiPart + "\n" + localizedBody;
            }

            return text;
        }

        public static string LocalizeStatsText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            text = text.Replace("Total Mastery Levels Required: ", RoR2.Language.GetString("KINGMOD_TOTAL_MASTERY_REQUIRED", "Total Mastery Levels Required: "));
            text = text.Replace("Total Mastery levels", RoR2.Language.GetString("KINGMOD_TOTAL_MASTERY_LEVELS", "Total Mastery levels"));
            text = text.Replace("Ki Drain: ", RoR2.Language.GetString("KINGMOD_KI_DRAIN", "Ki Drain: "));
            text = text.Replace(" Per Second", RoR2.Language.GetString("KINGMOD_PER_SECOND", " Per Second"));
            text = text.Replace("Charge Power: ", RoR2.Language.GetString("KINGMOD_CHARGE_POWER", "Charge Power: "));
            text = text.Replace("Damage: ", RoR2.Language.GetString("KINGMOD_STAT_DAMAGE", "Damage: "));
            text = text.Replace("Armor: ", RoR2.Language.GetString("KINGMOD_STAT_ARMOR", "Armor: "));
            text = text.Replace("Health Regen: ", RoR2.Language.GetString("KINGMOD_STAT_HEALTH_REGEN", "Health Regen: "));
            text = text.Replace("Attack Speed: ", RoR2.Language.GetString("KINGMOD_STAT_ATTACK_SPEED", "Attack Speed: "));
            text = text.Replace("Movement Speed: ", RoR2.Language.GetString("KINGMOD_STAT_MOVE_SPEED", "Movement Speed: "));
            text = text.Replace("Max Health: ", RoR2.Language.GetString("KINGMOD_STAT_MAX_HEALTH", "Max Health: "));
            text = text.Replace("Mastered ", RoR2.Language.GetString("KINGMOD_MASTERED", "Mastered "));
            text = text.Replace("Power Level: ", RoR2.Language.GetString("KINGMOD_POWER_LEVEL", "Power Level: "));
            text = text.Replace(" has no stat boosts...", RoR2.Language.GetString("KINGMOD_NO_STAT_BOOSTS", " has no stat boosts..."));
            text = text.Replace("All Forms unlocked?", RoR2.Language.GetString("KINGMOD_ALL_FORMS_UNLOCKED", "All Forms unlocked?"));
            text = text.Replace(" Mastery level(s) until next unlock", RoR2.Language.GetString("KINGMOD_UNTIL_NEXT_UNLOCK", " Mastery level(s) until next unlock"));

            return text;
        }
    }

    public static class FormattingPatches
    {
        [HarmonyPatch(typeof(MainPlugin), "CreateKiCostDescription")]
        [HarmonyPostfix]
        public static void Postfix_CreateKiCostDescription(string numValue, ref string __result)
        {
            var format = RoR2.Language.GetString("KINGMOD_COSTS_KI_FORMAT", "");
            if (!string.IsNullOrEmpty(format))
            {
                try { __result = string.Format(format, numValue); }
                catch (FormatException ex) { KingDBZFixPlugin.Log.LogWarning("Invalid localization format: " + ex.Message); }
            }
        }

        [HarmonyPatch(typeof(MainPlugin), "CreateDamageDescription")]
        [HarmonyPostfix]
        public static void Postfix_CreateDamageDescription(string numValue, ref string __result)
        {
            var format = RoR2.Language.GetString("KINGMOD_DAMAGE_FORMAT", "");
            if (!string.IsNullOrEmpty(format))
            {
                try { __result = string.Format(format, numValue); }
                catch (FormatException ex) { KingDBZFixPlugin.Log.LogWarning("Invalid localization format: " + ex.Message); }
            }
        }


    }

    public static class TutorialPatches
    {
        [HarmonyPatch(typeof(PopUpTextHandler), "AddNewPopUpText")]
        [HarmonyPrefix]
        public static void Prefix_AddNewPopUpText(ref string newText)
        {
            if (!string.IsNullOrEmpty(newText))
            {
                newText = RoR2.Language.GetString(newText, newText);
            }
        }

        [HarmonyPatch(typeof(RoR2.UI.HUD), "Awake")]
        [HarmonyPostfix]
        public static void Postfix_HUD_Awake(RoR2.UI.HUD __instance)
        {
            try
            {
                if (__instance == null) return;
                var tmps = __instance.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var tmp in tmps)
                {
                    if (tmp == null || string.IsNullOrEmpty(tmp.text)) continue;

                    if (tmp.text.Contains("Enable/Disable Tutorials by pressing F1"))
                    {
                        tmp.text = RoR2.Language.GetString("KINGMOD_TUTORIAL_F1", "Enable/Disable Tutorials by pressing F1");
                    }
                    else if (tmp.text.Contains("With Form Mastery"))
                    {
                        tmp.text = RoR2.Language.GetString("KINGMOD_TUTORIAL_FORM_MASTERY", tmp.text);
                    }
                    else if (tmp.text.Contains("Once your mastery level is high enough"))
                    {
                        tmp.text = RoR2.Language.GetString("KINGMOD_TUTORIAL_TRANSFORM_TRIGGER", tmp.text);
                    }
                    else if (tmp.text.Contains("While transformed, you have access to an Ultimate Skill"))
                    {
                        tmp.text = RoR2.Language.GetString("KINGMOD_TUTORIAL_ULTIMATE_SKILL", tmp.text);
                    }
                    else if (tmp.text.Contains("Ki is a resource that your skills use"))
                    {
                        tmp.text = RoR2.Language.GetString("KINGMOD_TUTORIAL_KI_RESOURCE", tmp.text);
                    }
                    else if (tmp.text.Contains("Gain Zenkai's by dying"))
                    {
                        tmp.text = RoR2.Language.GetString("KINGMOD_TUTORIAL_ZENKAI", tmp.text);
                    }
                    else if (tmp.text.Contains("Gain Light Charge by hitting enemies"))
                    {
                        tmp.text = RoR2.Language.GetString("KINGMOD_TUTORIAL_LIGHT_CHARGE", tmp.text);
                    }
                }
            }
            catch { }
        }
    }
}
