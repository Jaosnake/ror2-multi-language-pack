using System;
using System.Collections.Generic;
using System.IO;
using Path = System.IO.Path;
using System.Linq;
using BepInEx;
using RoR2;
using R2API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[BepInPlugin("jaosnake.pele.language.support", "PELE Language Support", "1.0.1")]
[BepInDependency("jaosnake.pele.language.core", "2.0.0")]
public sealed partial class LanguagePreview : BaseUnityPlugin
{
    private bool ready;
    private ExtendedScriptFont FallbackFont;
    private bool settingsLoaded;
    private float next;
    private string lastLanguage;
    private string renderer;
    private bool renderAttempted;
    private LanguageRenderWorker worker;
    private readonly Dictionary<TextMeshProUGUI, RawImage> rendered =
        new Dictionary<TextMeshProUGUI, RawImage>();
    private readonly Dictionary<TextMeshProUGUI, float> originalAlpha =
        new Dictionary<TextMeshProUGUI, float>();

    private void Awake()
    {
        try
        {
            var assets = Path.Combine(Path.GetDirectoryName(Info.Location), "assets");
            renderer = Path.Combine(Path.GetDirectoryName(Info.Location), "renderer", "ShapeText.exe");
            LoadSelectorFlags(assets);
            RoR2Application.onLoadFinished += OnGameLoaded;
            ready = true;
            FallbackFont = new ExtendedScriptFont(Path.Combine(assets, "NotoSans.ttf"), Logger);
        }
        catch (Exception error) { Logger.LogError(error); }
    }

    private void LateUpdate()
    {
        if (!ready || Time.unscaledTime < next || Language.currentLanguage == null) return;
        next = Time.unscaledTime + 0.25f;
        var lang = Language.currentLanguage.name;
        if (worker != null) worker.Update(Time.unscaledTime);
        if (lastLanguage != lang)
        {
            Logger.LogInfo("Idioma ativo: " + lang);
            bool refreshHud = lastLanguage != null;
            lastLanguage = lang;
            FallbackFont?.EnsureRegistered();
            CustomLanguageRegistration.EnsureAdditionalLanguages();
            foreach (var label in rendered.Keys.ToArray()) RemoveOverlay(label);
            if (refreshHud) RefreshHudLanguage();
        }
        if (UsesShapeText(lang)) RenderComplexText(lang);
    }


}
