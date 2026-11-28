using System;
using System.Collections.Generic;
using System.IO;
using Path = System.IO.Path;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using HarmonyLib;
using RoR2;
using RoR2.UI;
using R2API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class LanguagePreview
{
    private void RefreshHudLanguage()
    {
        // The game caches these strings until the stage, level, or objective changes.
        try
        {
            var lastStage = AccessTools.Field(typeof(StageCountDisplay), "lastStage");
            foreach (var stage in FindObjectsOfType<StageCountDisplay>())
                lastStage?.SetValue(stage, int.MinValue);

            var lastLevel = AccessTools.Field(typeof(AmbientLevelDisplay), "lastLevel");
            foreach (var level in FindObjectsOfType<AmbientLevelDisplay>())
                lastLevel?.SetValue(level, int.MinValue);

            var trackers = AccessTools.Field(typeof(ObjectivePanelController), "objectiveTrackers");
            var cachedString = AccessTools.Field(typeof(ObjectivePanelController.ObjectiveTracker), "cachedString");
            var generateString = AccessTools.Method(typeof(ObjectivePanelController.ObjectiveTracker), "GenerateString");
            if (trackers == null || cachedString == null || generateString == null)
            {
                Logger.LogWarning("Nao foi possivel atualizar os textos dos objetivos do HUD.");
                return;
            }

            foreach (var panel in FindObjectsOfType<ObjectivePanelController>())
            {
                if (!(trackers.GetValue(panel) is List<ObjectivePanelController.ObjectiveTracker> activeTrackers))
                    continue;

                foreach (var tracker in activeTrackers)
                {
                    try
                    {
                        cachedString.SetValue(tracker, generateString.Invoke(tracker, null));
                    }
                    catch (Exception error)
                    {
                        Logger.LogWarning("Falha ao atualizar um objetivo do HUD: " + (error.InnerException ?? error).Message);
                    }
                }
            }
            Logger.LogInfo("Textos de fase, nivel e objetivos do HUD atualizados apos a troca de idioma.");
        }
        catch (Exception error)
        {
            Logger.LogWarning("Falha ao atualizar o HUD apos a troca de idioma: " + error.Message);
        }
    }


}
