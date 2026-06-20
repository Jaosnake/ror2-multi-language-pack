using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using RoR2;

namespace R2API;

internal static class CustomLanguageRegistration
{
    private static readonly string[] CustomLangs = { "la", "eo", "uk" };
    private static readonly string[] CultureFallbackLangs = { "la", "eo" };

    internal static string[] GetRegisteredCustomLangs() => (string[])CustomLangs.Clone();
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
        if (__result != null) return;
        if (Array.IndexOf(CustomLangs, __0) < 0) return;

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
        LanguagePlugin.Logger?.LogInfo("Lazy-registrado: " + __0);
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
            var peleDir = System.IO.Path.Combine(Paths.PluginPath, "PELE", "Language");
            var subPath = System.IO.Path.Combine(peleDir, lang);
            if (Directory.Exists(subPath))
                setFolders?.Invoke(langObj, new object[] { new[] { subPath } });

            var finalName = nameField != null ? (string)nameField.GetValue(langObj) : "(field not found)";
            LanguagePlugin.Logger?.LogInfo("CreateLanguage: '" + lang + "' ok (nameField='" + finalName + "')");
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
        if (Array.IndexOf(CustomLangs, newCurrentLanguageName) < 0) return true;

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

            LanguagePlugin.Logger?.LogInfo("SetCurrentLanguage interceptado: '" + newCurrentLanguageName + "' (CultureInfo='" + CultureInfo.CurrentCulture.Name + "')");
        }
        catch (Exception ex)
        {
            LanguagePlugin.Logger?.LogError("SetCurrentLanguagePrefix falhou: " + (ex.InnerException?.Message ?? ex.Message));
            return true;
        }

        return false;
    }
}
