using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using RoR2;
using UnityEngine;

namespace R2API;

internal static class CustomLanguageRegistration
{
    private static readonly string[] LegacyLanguages = { "la", "eo", "uk" };
    private static readonly string[] AdditionalLanguages = { "th", "hi", "ar", "vi", "el" };
    private static readonly Dictionary<string, Sprite> SelectorFlags = new(StringComparer.OrdinalIgnoreCase);
    private static readonly FieldInfo SelfNameField = AccessTools.Field(typeof(Language), "<selfName>k__BackingField");
    private static readonly FieldInfo IconField = AccessTools.Field(typeof(Language), "<iconSprite>k__BackingField");
    private static readonly string[] CultureFallbackLangs = { "la", "eo" };

    internal static string[] GetRegisteredCustomLangs()
    {
        var languages = new string[LegacyLanguages.Length + AdditionalLanguages.Length];
        LegacyLanguages.CopyTo(languages, 0);
        AdditionalLanguages.CopyTo(languages, LegacyLanguages.Length);
        return languages;
    }

    internal static void RegisterSelectorFlag(string code, Sprite flag)
    {
        SelectorFlags[code] = flag;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Language), "GetAllLanguages")]
    internal static void EnsureAdditionalLanguages()
    {
        var languages = GetLanguagesByName();
        if (languages == null) return;

        foreach (var code in AdditionalLanguages)
            EnsureAdditionalLanguage(languages, code);
    }

    private static Language EnsureAdditionalLanguage(Dictionary<string, Language> languages, string code)
    {
        if (!languages.TryGetValue(code, out var language))
        {
            language = CreateLanguage(code);
            if (language == null) return null;
            languages[code] = language;
            LanguagePlugin.Logger?.LogInfo("Idioma registrado no seletor: " + code);
        }

        if (string.IsNullOrEmpty(language.selfName))
            SelfNameField?.SetValue(language, LanguageNames.GetFriendlyName(code));
        if (language.iconSprite == null && SelectorFlags.TryGetValue(code, out var flag))
            IconField?.SetValue(language, flag);

        return language;
    }
    internal static string[] GetFallbackLangs() => (string[])CultureFallbackLangs.Clone();

    internal static string FixCultureName(string name)
    {
        if (Array.IndexOf(CultureFallbackLangs, name) >= 0) return "en";
        return name;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Language), "FindLanguageByName")]
    private static void FindLanguageByNamePostfix(string __0, ref Language __result)
    {
        var additionalIndex = Array.FindIndex(AdditionalLanguages,
            code => string.Equals(code, __0, StringComparison.OrdinalIgnoreCase));
        if (additionalIndex >= 0)
        {
            var languages = GetLanguagesByName();
            if (languages != null)
                __result = EnsureAdditionalLanguage(languages, AdditionalLanguages[additionalIndex]) ?? __result;
            return;
        }

        if (__result != null || Array.IndexOf(LegacyLanguages, __0) < 0) return;
        var dict = GetLanguagesByName();
        if (dict == null) return;
        if (dict.TryGetValue(__0, out var existing))
        {
            __result = existing;
            return;
        }

        var langObj = CreateLanguage(__0);
        if (langObj == null) return;
        dict[__0] = langObj;
        __result = langObj;
        LanguagePlugin.LogVerbose("Lazy-registrado: " + __0);
    }

    private static Dictionary<string, Language> GetLanguagesByName()
    {
        var field = typeof(Language).GetField("languagesByName", BindingFlags.Static | BindingFlags.NonPublic);
        return (Dictionary<string, Language>)field?.GetValue(null);
    }

    private static Language CreateLanguage(string lang)
    {
        try
        {
            var ctor = typeof(Language).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(string) }, null);
            if (ctor == null)
            {
                LanguagePlugin.Logger?.LogError("CreateLanguage: Construtor Language(string) nao encontrado para " + lang);
                return null;
            }

            var langObj = (Language)ctor.Invoke(new object[] { lang });

            var nameField = typeof(Language).GetField("name", BindingFlags.Instance | BindingFlags.Public)
                ?? typeof(Language).GetField("_name", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? typeof(Language).GetField("name", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? typeof(Language).GetField("<name>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);

            if (nameField == null)
            {
                foreach (var f in typeof(Language).GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    if (f.FieldType != typeof(string)) continue;
                    if ((string)f.GetValue(langObj) == lang)
                    {
                        nameField = f;
                        break;
                    }
                }
            }

            nameField?.SetValue(langObj, lang);

            var setFolders = typeof(Language).GetMethod("SetFolders", BindingFlags.Instance | BindingFlags.NonPublic);
            var peleDir = PelePaths.LanguageRoot(Paths.PluginPath);
            var subPath = System.IO.Path.Combine(peleDir, lang);
            if (Directory.Exists(subPath))
                setFolders?.Invoke(langObj, new object[] { new[] { subPath } });

            var finalName = nameField != null ? (string)nameField.GetValue(langObj) : "(field not found)";
            LanguagePlugin.LogVerbose("CreateLanguage: '" + lang + "' ok (nameField='" + finalName + "')");
            return langObj;
        }
        catch (Exception ex)
        {
            LanguagePlugin.Logger?.LogError("CreateLanguage falhou para " + lang + ": " + (ex.InnerException?.Message ?? ex.Message));
            LanguagePlugin.Logger?.LogError("  Stack: " + (ex.InnerException?.StackTrace ?? ex.StackTrace));
            return null;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Language), "SetCurrentLanguage")]
    private static bool SetCurrentLanguagePrefix(string newCurrentLanguageName)
    {
        if (Array.IndexOf(LegacyLanguages, newCurrentLanguageName) < 0) return true;

        try
        {
            var lang = Language.FindLanguageByName(newCurrentLanguageName);
            if (lang == null) return true;

            if (Language.currentLanguage != null && Language.currentLanguage != Language.english)
                Language.currentLanguage.UnloadStrings();

            var langNameField = typeof(Language).GetField("<currentLanguageName>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic)
                ?? typeof(Language).GetField("currentLanguageName", BindingFlags.Static | BindingFlags.NonPublic)
                ?? typeof(Language).GetField("_currentLanguageName", BindingFlags.Static | BindingFlags.NonPublic);
            langNameField?.SetValue(null, newCurrentLanguageName);

            var langField = typeof(Language).GetField("<currentLanguage>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic)
                ?? typeof(Language).GetField("currentLanguage", BindingFlags.Static | BindingFlags.NonPublic)
                ?? typeof(Language).GetField("_currentLanguage", BindingFlags.Static | BindingFlags.NonPublic)
                ?? typeof(Language).GetField("currentLanguage", BindingFlags.Static | BindingFlags.Public);
            langField?.SetValue(null, lang);

            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(FixCultureName(lang.name));
            lang.LoadStrings();

            var evt = typeof(Language).GetField("onCurrentLanguageChanged", BindingFlags.Static | BindingFlags.NonPublic)
                ?.GetValue(null) as Action;
            evt?.Invoke();

            LanguagePlugin.LogVerbose("SetCurrentLanguage interceptado: '" + newCurrentLanguageName + "' (CultureInfo='" + CultureInfo.CurrentCulture.Name + "')");
        }
        catch (Exception ex)
        {
            LanguagePlugin.Logger?.LogError("SetCurrentLanguagePrefix falhou: " + (ex.InnerException?.Message ?? ex.Message));
            return true;
        }

        return false;
    }
}
