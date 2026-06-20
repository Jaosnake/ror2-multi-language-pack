using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine;

namespace R2API;

internal static class CyrillicFontSupport
{
    private static TMP_FontAsset _cachedEmbeddedFont;
    private static AssetBundle _cachedEmbeddedFontBundle;
    private static TMP_FontAsset _vanillaDefaultFont;
    private static TMP_FontAsset _cyrillicFont;
    private static TMP_FontAsset _bombardierDefaultFont;
    private static AssetBundle _cyrillicFontBundle;
    private static bool _cyrillicFontProbeDone;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HGTextMeshProUGUI), "OnCurrentLanguageChanged")]
    private static void OnLanguageChanged()
    {
        _vanillaDefaultFont ??= HGTextMeshProUGUI.defaultLanguageFont;

        var customFont = GetCustomFontForCurrentLanguage();
        HGTextMeshProUGUI.defaultLanguageFont = customFont ?? _vanillaDefaultFont;
        ApplyCurrentFontToActiveText();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TextMeshProUGUI), "LoadFontAsset")]
    private static void TextMeshProUGUI_LoadFontAsset_Postfix(TextMeshProUGUI __instance)
    {
        var font = GetCyrillicFont();
        if (font == null || __instance == null) return;

        var current = __instance.font;
        if (current == null) return;

        var defaultFont = _bombardierDefaultFont ?? _vanillaDefaultFont;
        if (current == defaultFont || current.name.IndexOf("Bomb", StringComparison.OrdinalIgnoreCase) >= 0)
            __instance.font = font;
    }

    internal static TMP_FontAsset GetCyrillicFont()
    {
        if (_cyrillicFontProbeDone)
            return _cyrillicFont;

        _cyrillicFontProbeDone = true;
        _cyrillicFont = TryLoadCyrillicFontBundle()
            ?? TryGetExternalCyrillicFont()
            ?? TryLoadEmbeddedFont("ukrainianfont");

        if (_cyrillicFont != null)
            LanguagePlugin.Logger?.LogInfo("Fonte cirilica do PELE ativa.");
        else
            LanguagePlugin.Logger?.LogWarning("Nenhuma fonte cirilica encontrada; caracteres ucranianos podem aparecer como '?'.");

        return _cyrillicFont;
    }

    private static TMP_FontAsset GetCustomFontForCurrentLanguage()
    {
        var current = Language.currentLanguageName;
        if (current == null) return null;
        if (!"uk".Equals(current, StringComparison.OrdinalIgnoreCase)) return null;
        return GetCyrillicFont();
    }

    private static TMP_FontAsset TryGetExternalCyrillicFont()
    {
        try
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = asm.GetType("AnotherOneCyrillicFont.AnotherOneCyrillicFont", throwOnError: false);
                if (type == null) continue;

                var fontField = type.GetField("fontBomb", BindingFlags.Static | BindingFlags.Public);
                var defaultField = type.GetField("fontBombDefault", BindingFlags.Static | BindingFlags.Public);
                var font = fontField?.GetValue(null) as TMP_FontAsset;
                var defaultFont = defaultField?.GetValue(null) as TMP_FontAsset;

                if (defaultFont != null)
                    _bombardierDefaultFont = defaultFont;
                if (font != null)
                    return font;
            }
        }
        catch (Exception ex)
        {
            LanguagePlugin.Logger?.LogWarning("TryGetExternalCyrillicFont falhou: " + ex.Message);
        }

        return null;
    }

    private static TMP_FontAsset TryLoadCyrillicFontBundle()
    {
        try
        {
            var bundlePath = FindCyrillicFontBundlePath();
            if (bundlePath == null) return null;

            _cyrillicFontBundle = AssetBundle.LoadFromFile(bundlePath);
            if (_cyrillicFontBundle == null)
            {
                LanguagePlugin.Logger?.LogWarning("TryLoadCyrillicFontBundle: AssetBundle retornou null.");
                return null;
            }

            var font = _cyrillicFontBundle.LoadAsset<TMP_FontAsset>("Assets/CyrillicFont/tmpBombDropshadow.asset")
                ?? _cyrillicFontBundle.LoadAllAssets<TMP_FontAsset>().FirstOrDefault();

            if (font != null)
                LanguagePlugin.Logger?.LogInfo("Fonte cirilica carregada de bundle PELE.");

            return font;
        }
        catch (Exception ex)
        {
            LanguagePlugin.Logger?.LogWarning("TryLoadCyrillicFontBundle falhou: " + ex.Message);
            return null;
        }
    }

    private static string FindCyrillicFontBundlePath()
    {
        var preferred = new[]
        {
            System.IO.Path.Combine(Paths.PluginPath, "PELE", "Fonts", "cyrillicfont"),
            System.IO.Path.Combine(Paths.PluginPath, "PELE", "cyrillicfont"),
        };

        foreach (var path in preferred)
        {
            if (File.Exists(path))
                return path;
        }

        return Directory.GetFiles(Paths.PluginPath, "cyrillicfont", SearchOption.AllDirectories)
            .FirstOrDefault();
    }

    private static TMP_FontAsset TryLoadEmbeddedFont(string resourceName)
    {
        if (_cachedEmbeddedFont != null) return _cachedEmbeddedFont;

        try
        {
            if (_cachedEmbeddedFontBundle == null)
            {
                var stream = TryGetResourceStream(resourceName);
                if (stream == null)
                {
                    LanguagePlugin.Logger?.LogWarning("TryLoadEmbeddedFont: recurso embutido nao encontrado: " + resourceName);
                    return null;
                }

                using (stream)
                    _cachedEmbeddedFontBundle = AssetBundle.LoadFromStream(stream);

                if (_cachedEmbeddedFontBundle == null)
                {
                    LanguagePlugin.Logger?.LogWarning("TryLoadEmbeddedFont: AssetBundle retornou null: " + resourceName);
                    return null;
                }
            }

            var font = _cachedEmbeddedFontBundle.LoadAsset<TMP_FontAsset>("Assets/PELE-Font.asset")
                ?? _cachedEmbeddedFontBundle.LoadAsset<TMP_FontAsset>("PELE-Font")
                ?? _cachedEmbeddedFontBundle.LoadAsset<TMP_FontAsset>("ukrainianfont")
                ?? _cachedEmbeddedFontBundle.LoadAllAssets<TMP_FontAsset>().FirstOrDefault();

            if (font == null)
            {
                LanguagePlugin.Logger?.LogWarning("TryLoadEmbeddedFont: TMP_FontAsset nao encontrado no bundle '" + resourceName + "'");
                return null;
            }

            _cachedEmbeddedFont = font;
            LanguagePlugin.Logger?.LogInfo("Fonte cirilica embutida carregada.");
            return _cachedEmbeddedFont;
        }
        catch (Exception ex)
        {
            LanguagePlugin.Logger?.LogWarning("TryLoadEmbeddedFont falhou: " + ex.Message);
            return null;
        }
    }

    private static void ApplyCurrentFontToActiveText()
    {
        var font = HGTextMeshProUGUI.defaultLanguageFont;
        if (font == null) return;

        foreach (var text in Resources.FindObjectsOfTypeAll<HGTextMeshProUGUI>())
        {
            if (text == null) continue;
            if (!text.gameObject.scene.IsValid()) continue;
            text.font = font;
            text.SetAllDirty();
        }
    }

    private static Stream TryGetResourceStream(string resourceName)
    {
        var asm = Assembly.GetExecutingAssembly();
        return asm.GetManifestResourceStream("R2API.Language." + resourceName)
            ?? asm.GetManifestResourceStream("PELE." + resourceName);
    }
}
