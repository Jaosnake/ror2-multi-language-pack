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
    private readonly Dictionary<string, ExtendedScriptFont> nativeFallbacks = new();

    private void EnableNativeFallback(string language)
    {
        if (!nativeFallbacks.TryGetValue(language, out var font))
        {
            var ranges = language switch
            {
                "ar" => new[] { (0x0600, 0x06FF) },
                "hi" => new[] { (0x0900, 0x097F) },
                _ => new[] { (0x0E00, 0x0E7F) }
            };
            var fontPath = Path.Combine(Path.GetDirectoryName(renderer), "fonts", language + ".ttf");
            nativeFallbacks.Add(language, font = new ExtendedScriptFont(fontPath, Logger, ranges));
        }
        font.EnsureRegistered();
    }

    private void StartRenderer()
    {
        if (renderAttempted) return;
        renderAttempted = true;
        if (!File.Exists(renderer)) return;
        try
        {
            worker = new LanguageRenderWorker(renderer, message => Logger.LogInfo(message),
                message => Logger.LogWarning(message));
        }
        catch (Exception error)
        {
            Logger.LogError("Falha ao iniciar o renderizador: " + error.Message);
        }
    }

    private void OnGameLoaded()
    {
        settingsLoaded = true;
    }

    private void OnDestroy()
    {
        RoR2Application.onLoadFinished -= OnGameLoaded;
        foreach (var label in rendered.Keys.ToArray())
            RemoveOverlay(label);
        if (worker != null) worker.Dispose();
        FallbackFont?.Dispose();
        foreach (var font in nativeFallbacks.Values) font.Dispose();
    }

    private void RemoveOverlay(TextMeshProUGUI label)
    {
        RawImage image;
        if (!rendered.TryGetValue(label, out image)) return;
        rendered.Remove(label);
        if (image != null) Destroy(image.gameObject);
        if (label != null) label.canvasRenderer.SetAlpha(originalAlpha[label]);
        originalAlpha.Remove(label);
    }


}
