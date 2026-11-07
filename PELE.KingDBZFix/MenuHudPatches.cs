using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using NS_KingModUtilities;
using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace PELE.KingDBZFix
{
    internal static class MenuHudPatches
    {
        private static readonly FieldInfo OverviewField = typeof(TrainingMenuUI).GetField("m_overviewButtonText", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        private static readonly FieldInfo TransformationField = typeof(UI_TransformationDisplay).GetField("m_transformationText", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        private static readonly FieldInfo PowerField = typeof(UI_TransformationDisplay).GetField("m_powerLevelText", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        private static readonly FieldInfo MasteryField = typeof(UI_TransformationDisplay).GetField("m_masteryLevelSection", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        private sealed class LabelSnapshot { internal string Source; internal string Last; }
        private static readonly ConditionalWeakTable<Text, LabelSnapshot> Sources = new ConditionalWeakTable<Text, LabelSnapshot>();

        [HarmonyPatch(typeof(TrainingMenuUI), "OnCharacterSelected")]
        [HarmonyPostfix]
        private static void OnCharacterSelected(TrainingMenuUI __instance)
        {
            try
            {
                var label = OverviewField?.GetValue(__instance) as HGTextMeshProUGUI;
                if (label == null) return;
                string value = KingDBZFixPlugin.SafeGetString("KINGMOD_TRANSFORMATIONS", "Transformations");
                if (label.text != value) label.text = value;
            }
            catch (Exception ex) { KingDBZFixPlugin.Log?.LogWarning("Erro ao localizar aba de transformacoes: " + ex.Message); }
        }

        [HarmonyPatch(typeof(UI_TransformationDisplay), "DrawElement")]
        [HarmonyPostfix]
        private static void DrawElement(UI_TransformationDisplay __instance)
        {
            try
            {
                var form = TransformationField?.GetValue(__instance) as Text;
                if (form != null)
                {
                    Sources.TryGetValue(form, out var state);
                    if (form.text == "Base Form" || (state != null && form.text == state.Last))
                    {
                        var value = KingDBZFixPlugin.SafeGetString("KINGMOD_BASE_FORM", "Base Form");
                        if (state == null) state = Sources.GetValue(form, _ => new LabelSnapshot());
                        state.Source = "Base Form"; state.Last = value;
                        if (form.text != value) form.text = value;
                    }
                }
                var power = PowerField?.GetValue(__instance) as Text;
                if (power != null && !string.IsNullOrEmpty(power.text))
                {
                    Sources.TryGetValue(power, out var state);
                    var source = state != null && power.text == state.Last ? state.Source : power.text;
                    var translated = TransformationsMenuPatches.LocalizeStatsText(source);
                    if (translated != source || (state != null && power.text == state.Last))
                    {
                        if (state == null) state = Sources.GetValue(power, _ => new LabelSnapshot());
                        state.Source = source; state.Last = translated;
                        if (power.text != translated) power.text = translated;
                    }
                }
                var section = MasteryField?.GetValue(__instance) as Transform;
                if (section != null)
                    foreach (var label in section.GetComponentsInChildren<Text>(true))
                        if (label != null && label.gameObject.name == "MasteryLevel_TitleText")
                        {
                            var value = KingDBZFixPlugin.SafeGetString("KINGMOD_LEVEL", "Level");
                            if (label.text != value) label.text = value;
                        }
            }
            catch (Exception ex) { KingDBZFixPlugin.Log?.LogWarning("Erro ao localizar HUD de transformacoes: " + ex.Message); }
        }
    }
}
