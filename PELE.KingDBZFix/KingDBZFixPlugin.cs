using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
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
                PatchGroup(typeof(TransformationsMenuPatches));
                PatchGroup(typeof(FormattingPatches));
                PatchGroup(typeof(MenuHudPatches));
                PatchGroup(typeof(TutorialPatches));

                // Hook language change to inject in-memory dictionary
                RoR2.Language.onCurrentLanguageChanged += OnLanguageChanged;
                InjectFallbackTokens();

                Log.LogInfo("PELE - KingModUtilities Localization Fix (Goku, Vegeta, Trunks) carregado com sucesso!");
            }
            catch (Exception ex)
            {
                Log.LogError("Erro ao aplicar patch no KingModUtilities: " + ex);
            }
        }

        private void PatchGroup(Type patchType)
        {
            try { _harmony.PatchAll(patchType); }
            catch (Exception ex) { Log?.LogError("Falha ao aplicar " + patchType.Name + ": " + ex); }
        }

        private void OnDestroy()
        {
            RoR2.Language.onCurrentLanguageChanged -= OnLanguageChanged;
            _harmony?.UnpatchSelf();
        }

        private static void OnLanguageChanged()
        {
            InjectFallbackTokens();
        }

        public static void InjectFallbackTokens()
        {
            try
            {
                var curLang = RoR2.Language.currentLanguage;
                if (curLang == null) return;
                string lang = curLang.name;
                if (string.IsNullOrEmpty(lang)) lang = "en";

                var dict = DBZLocalizationDictionary.GetDictionaryForLanguage(lang);
                if (dict != null)
                {
                    foreach (var kvp in dict)
                    {
                        if (!curLang.TokenIsRegistered(kvp.Key) || curLang.GetLocalizedStringByToken(kvp.Key) == kvp.Key)
                        {
                            curLang.SetStringByToken(kvp.Key, kvp.Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log?.LogWarning("Erro ao injetar tokens de fallback: " + ex.Message);
            }
        }

        public static string SafeGetString(string token, string fallback = null)
        {
            if (string.IsNullOrEmpty(token)) return fallback ?? "";
            if (token == "Transformations") token = "KINGMOD_TRANSFORMATIONS";
            if (MenuTranslations.TryGet(RoR2.Language.currentLanguageName, token, out string menuText)) return menuText;
            // Tutorial strings must not be shadowed by English generic tokens in other packs.
            if (token.StartsWith("KINGMOD_TUTORIAL_", StringComparison.Ordinal) || token == "KINGMOD_POPUP_ADDED_FORMAT" || token == "KINGMOD_ULTIMATE_SKILL_TITLE")
            {
                string language = RoR2.Language.currentLanguageName;
                if (TutorialTranslations.TryGet(language, token, out string translated)) return translated;
                return fallback ?? token;
            }
            try
            {
                var curLang = RoR2.Language.currentLanguage;
                if (curLang != null && curLang.TokenIsRegistered(token))
                {
                    string res = curLang.GetLocalizedStringByToken(token);
                    if (!string.IsNullOrEmpty(res) && res != token)
                    {
                        return res;
                    }
                }
                else
                {
                    string res = RoR2.Language.GetString(token);
                    if (!string.IsNullOrEmpty(res) && res != token)
                    {
                        return res;
                    }
                }
            }
            catch { }

            try
            {
                string lang = RoR2.Language.currentLanguageName;
                if (string.IsNullOrEmpty(lang)) lang = "en";
                var dict = DBZLocalizationDictionary.GetDictionaryForLanguage(lang);
                if (dict != null && dict.TryGetValue(token, out string val))
                {
                    return val;
                }
            }
            catch { }

            return fallback ?? token;
        }
    }

    public static class TransformationsMenuPatches
    {
        private static readonly FieldInfo _fTransName = typeof(UI_TransformationsMenu).GetField("m_mainToolTipTransformationNameText", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        private static readonly FieldInfo _fSkillName = typeof(UI_TransformationsMenu).GetField("m_mainToolTipSkillNameText", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        private static readonly FieldInfo _fSkillText = typeof(UI_TransformationsMenu).GetField("m_mainToolTipSkillText", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        private static readonly FieldInfo _fMainTooltipText = typeof(UI_TransformationsMenu).GetField("m_mainTooltipText", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        private static readonly FieldInfo _fRemoveBtn = typeof(UI_TransformationsMenu).GetField("m_removeTransformationButton", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        private static readonly FieldInfo _fShowExtraBtn = typeof(UI_TransformationsMenu).GetField("m_showExtraTransformationButton", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        [HarmonyPatch(typeof(UI_TransformationsMenu), "UpdateTooltip")]
        [HarmonyPostfix]
        public static void Postfix_UpdateTooltip(UI_TransformationsMenu __instance)
        {
            try
            {
                if (__instance == null) return;

                // This label is a static prefab string, not the dynamic skill name below it.
                var skillNameAnchor = _fSkillName?.GetValue(__instance) as Text;
                var menuRoot = skillNameAnchor != null ? skillNameAnchor.transform.root : null;
                foreach (var label in menuRoot != null ? menuRoot.GetComponentsInChildren<Text>(true) : Array.Empty<Text>())
                {
                    if (label == null) continue;
                    string title = null;
                    if (label.gameObject.name == "Tooltip_main_skill_title_text")
                        title = KingDBZFixPlugin.SafeGetString("KINGMOD_ULTIMATE_SKILL_TITLE", "ULTIMATE SKILL");
                    else if (label.gameObject.name == "Tooltip_main_transformation_title_text")
                        title = KingDBZFixPlugin.SafeGetString("KINGMOD_TRANSFORMATION", "TRANSFORMATION");
                    if (title != null && label.text != title) label.text = title;
                }

                // 1. Transformation Title
                var transNameText = _fTransName?.GetValue(__instance) as Text;
                if (transNameText != null && !string.IsNullOrEmpty(transNameText.text))
                {
                    transNameText.text = KingDBZFixPlugin.SafeGetString(transNameText.text, transNameText.text);
                }

                // 2. Skill Name
                var skillNameText = _fSkillName?.GetValue(__instance) as Text;
                if (skillNameText != null && !string.IsNullOrEmpty(skillNameText.text))
                {
                    skillNameText.text = KingDBZFixPlugin.SafeGetString(skillNameText.text, skillNameText.text);
                }

                // 3. Skill Description
                var skillText = _fSkillText?.GetValue(__instance) as Text;
                if (skillText != null && !string.IsNullOrEmpty(skillText.text))
                {
                    skillText.text = LocalizeSkillDescription(skillText.text);
                }

                // 4. Stats Tooltip
                var statsText = _fMainTooltipText?.GetValue(__instance) as Text;
                if (statsText != null && !string.IsNullOrEmpty(statsText.text))
                {
                    statsText.text = LocalizeStatsText(statsText.text);
                }

                // 5. Buttons
                var removeBtn = _fRemoveBtn?.GetValue(__instance) as Button;
                if (removeBtn != null)
                {
                    var txt = removeBtn.GetComponentInChildren<Text>();
                    if (txt != null)
                    {
                        string label = KingDBZFixPlugin.SafeGetString("KINGMOD_REMOVE", "REMOVE");
                        if (txt.text != label) txt.text = label;
                    }
                }

                var showExtraBtn = _fShowExtraBtn?.GetValue(__instance) as Button;
                if (showExtraBtn != null)
                {
                    var txt = showExtraBtn.GetComponentInChildren<Text>();
                    if (txt != null)
                    {
                        string label = KingDBZFixPlugin.SafeGetString("KINGMOD_TOGGLE_EXTRA", "TOGGLE EXTRA");
                        if (txt.text != label) txt.text = label;
                    }
                }
            }
            catch (Exception ex)
            {
                KingDBZFixPlugin.Log?.LogWarning("Erro em Postfix_UpdateTooltip: " + ex.Message);
            }
        }

        [HarmonyPatch(typeof(UI_TransformationsMenu), "CreateTransformationElement")]
        [HarmonyPostfix]
        public static void Postfix_CreateTransformationElement(ref DynamicArray<GameObject> __0, MainTransformationState __3)
        {
            try
            {
                if (__3 == null) return;
                int count = __0.GetCount();
                if (count > 0)
                {
                    GameObject go = __0.AccessElement(count - 1);
                    if (go != null)
                    {
                        var texts = go.GetComponentsInChildren<Text>(true);
                        foreach (var txt in texts)
                        {
                            if (txt != null && !string.IsNullOrEmpty(txt.text) && txt.text == __3.GetDisplayName())
                            {
                                txt.text = KingDBZFixPlugin.SafeGetString(txt.text, txt.text);
                            }
                        }
                    }
                }
            }
            catch { }
        }

        public static string LocalizeSkillDescription(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var direct = KingDBZFixPlugin.SafeGetString(text, null);
            if (!string.IsNullOrEmpty(direct) && direct != text) return direct;

            var m = Regex.Match(text, @"^Costs\s+(<color=[^>]+>\d+\s*Ki</color>)\s*\n([\s\S]*)$", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                var kiPart = m.Groups[1].Value;
                var body = m.Groups[2].Value.Trim();

                var costPrefix = KingDBZFixPlugin.SafeGetString("KINGMOD_COSTS_PREFIX", "Costs ");
                var localizedBody = KingDBZFixPlugin.SafeGetString(body, body);

                return costPrefix + kiPart + "\n" + localizedBody;
            }

            return text;
        }

        public static string LocalizeStatsText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            text = SafeReplace(text, "Total Mastery Levels Required: ", "KINGMOD_TOTAL_MASTERY_REQUIRED", "Total Mastery Levels Required: ");
            text = SafeReplace(text, "Total Mastery levels", "KINGMOD_TOTAL_MASTERY_LEVELS", "Total Mastery levels");
            text = SafeReplace(text, "Ki Drain: ", "KINGMOD_KI_DRAIN", "Ki Drain: ");
            text = SafeReplace(text, " Per Second", "KINGMOD_PER_SECOND", " Per Second");
            text = SafeReplace(text, "Charge Power: ", "KINGMOD_CHARGE_POWER", "Charge Power: ");
            text = SafeReplace(text, "Damage: ", "KINGMOD_STAT_DAMAGE", "Damage: ");
            text = SafeReplace(text, "Armor: ", "KINGMOD_STAT_ARMOR", "Armor: ");
            text = SafeReplace(text, "Health Regen: ", "KINGMOD_STAT_HEALTH_REGEN", "Health Regen: ");
            text = SafeReplace(text, "Attack Speed: ", "KINGMOD_STAT_ATTACK_SPEED", "Attack Speed: ");
            text = SafeReplace(text, "Movement Speed: ", "KINGMOD_STAT_MOVE_SPEED", "Movement Speed: ");
            text = SafeReplace(text, "Max Health: ", "KINGMOD_STAT_MAX_HEALTH", "Max Health: ");
            text = SafeReplace(text, "Mastered ", "KINGMOD_MASTERED", "Mastered ");
            text = SafeReplace(text, "Power Level: ", "KINGMOD_POWER_LEVEL", "Power Level: ");
            text = SafeReplace(text, " has no stat boosts...", "KINGMOD_NO_STAT_BOOSTS", " has no stat boosts...");
            text = SafeReplace(text, "All Forms unlocked?", "KINGMOD_ALL_FORMS_UNLOCKED", "All Forms unlocked?");
            text = SafeReplace(text, " Mastery level(s) until next unlock", "KINGMOD_UNTIL_NEXT_UNLOCK", " Mastery level(s) until next unlock");

            return text;
        }

        private static string SafeReplace(string text, string target, string token, string fallback)
        {
            if (!text.Contains(target)) return text;
            string rep = KingDBZFixPlugin.SafeGetString(token, fallback);
            if (target.EndsWith(" ") && !rep.EndsWith(" "))
            {
                rep += " ";
            }
            if (target.StartsWith(" ") && !rep.StartsWith(" "))
            {
                rep = " " + rep;
            }
            return text.Replace(target, rep);
        }
    }

    public static class FormattingPatches
    {
        [HarmonyPatch(typeof(MainPlugin), "CreateKiCostDescription")]
        [HarmonyPostfix]
        public static void Postfix_CreateKiCostDescription(string numValue, ref string __result)
        {
            var format = KingDBZFixPlugin.SafeGetString("KINGMOD_COSTS_KI_FORMAT", null);
            if (!string.IsNullOrEmpty(format))
            {
                try { __result = string.Format(format, numValue); }
                catch { }
            }
        }

        [HarmonyPatch(typeof(MainPlugin), "CreateDamageDescription")]
        [HarmonyPostfix]
        public static void Postfix_CreateDamageDescription(string numValue, ref string __result)
        {
            var format = KingDBZFixPlugin.SafeGetString("KINGMOD_DAMAGE_FORMAT", null);
            if (!string.IsNullOrEmpty(format))
            {
                try { __result = string.Format(format, numValue); }
                catch { }
            }
        }
    }

    public static class TutorialPatches
    {
        private static float _nextHudCheckTime = 0f;
        private sealed class TutorialSnapshot { internal string Source; internal string Last; }
        private static readonly ConditionalWeakTable<UnityEngine.Object, TutorialSnapshot> Sources = new ConditionalWeakTable<UnityEngine.Object, TutorialSnapshot>();

        private static string TranslateComponent(UnityEngine.Object component, string current)
        {
            if (component == null || string.IsNullOrEmpty(current)) return current;
            Sources.TryGetValue(component, out var snapshot);
            string source = snapshot != null && (current == snapshot.Last || current == snapshot.Source) ? snapshot.Source : current;
            string token = TutorialMatcher.FindToken(source);
            if (token == null) return current;
            // Preserve the original colored prefab text when English is selected.
            string language = RoR2.Language.currentLanguageName;
            bool isEnglish = string.Equals(language, "en", StringComparison.OrdinalIgnoreCase);
            string translated = isEnglish ? source : KingDBZFixPlugin.SafeGetString(token, source);
            if (!isEnglish && translated != source) translated = TutorialHighlight.Apply(language, token, translated);
            if (snapshot == null) snapshot = Sources.GetValue(component, _ => new TutorialSnapshot());
            snapshot.Source = source;
            snapshot.Last = translated;
            return translated;
        }

        [HarmonyPatch(typeof(PopUpTextHandler), "AddNewPopUpText")]
        [HarmonyPrefix]
        public static void Prefix_AddNewPopUpText(ref string newText)
        {
            if (string.IsNullOrEmpty(newText)) return;
            if (newText.StartsWith("Added ", StringComparison.Ordinal) && newText.EndsWith("!", StringComparison.Ordinal) && newText.Length > 7)
            {
                string form = newText.Substring(6, newText.Length - 7);
                string format = KingDBZFixPlugin.SafeGetString("KINGMOD_POPUP_ADDED_FORMAT", "Added {0}!");
                try { newText = string.Format(format, form); } catch (FormatException) { }
            }
            else if (newText == "Tutorials Enabled")
            {
                newText = KingDBZFixPlugin.SafeGetString("KINGMOD_TUTORIALS_ENABLED", "Tutorials Enabled");
            }
            else if (newText == "Tutorials Disabled")
            {
                newText = KingDBZFixPlugin.SafeGetString("KINGMOD_TUTORIALS_DISABLED", "Tutorials Disabled");
            }
            else
            {
                newText = KingDBZFixPlugin.SafeGetString(newText, newText);
            }
        }

        [HarmonyPatch(typeof(UI_Element), "RefreshTutorialUI")]
        [HarmonyPostfix]
        public static void Postfix_RefreshTutorialUI(UI_Element __instance)
        {
            if (__instance == null) return;
            try
            {
                var tr = Traverse.Create(__instance);
                var tut = tr.Field("m_tutorial")?.GetValue() as GameObject;
                if (tut != null) LocalizeTutorialUI(tut);
                var uiGO = tr.Field("m_uiGO")?.GetValue() as GameObject;
                if (uiGO != null) LocalizeTutorialUI(uiGO);
            }
            catch { }
        }

        [HarmonyPatch(typeof(UIManager), "SetupHUD")]
        [HarmonyPostfix]
        public static void Postfix_SetupHUD(Transform selfTransform)
        {
            if (selfTransform != null)
            {
                try { LocalizeTutorialUI(selfTransform.gameObject); }
                catch { }
            }
        }

        [HarmonyPatch(typeof(UIManager), "RefreshAllTutorialUI")]
        [HarmonyPostfix]
        public static void Postfix_RefreshAllTutorialUI(UIManager __instance)
        {
            try
            {
                if (RoR2.UI.HUD.readOnlyInstanceList != null)
                {
                    foreach (var hud in RoR2.UI.HUD.readOnlyInstanceList)
                    {
                        if (hud != null) LocalizeTutorialUI(hud.gameObject);
                    }
                }
            }
            catch { }
        }

        [HarmonyPatch(typeof(RoR2.UI.HUD), "Update")]
        [HarmonyPostfix]
        public static void Postfix_HUD_Update(RoR2.UI.HUD __instance)
        {
            if (__instance == null) return;
            if (Time.unscaledTime > _nextHudCheckTime)
            {
                _nextHudCheckTime = Time.unscaledTime + 1.0f;
                try { LocalizeTutorialUI(__instance.gameObject); }
                catch { }
            }
        }

        public static void LocalizeTutorialUI(GameObject root)
        {
            if (root == null) return;

            var texts = root.GetComponentsInChildren<Text>(true);
            if (texts != null)
            {
                foreach (var t in texts)
                {
                    if (t != null && !string.IsNullOrEmpty(t.text))
                    {
                        string translated = TranslateComponent(t, t.text);
                        if (translated != t.text) t.text = translated;
                    }
                }
            }

            var tmps = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            if (tmps != null)
            {
                foreach (var tmp in tmps)
                {
                    if (tmp != null && !string.IsNullOrEmpty(tmp.text))
                    {
                        string translated = TranslateComponent(tmp, tmp.text);
                        if (translated != tmp.text) tmp.text = translated;
                    }
                }
            }
        }

        public static string LocalizeTutorialString(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var token = TutorialMatcher.FindToken(text);
            return token == null ? text : KingDBZFixPlugin.SafeGetString(token, text);
        }
    }
}
