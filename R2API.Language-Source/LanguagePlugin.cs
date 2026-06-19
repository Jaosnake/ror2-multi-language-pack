using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using RoR2;
using RoR2.UI;
using SimpleJSON;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace R2API;

[BepInPlugin(LanguageAPI.PluginGUID, LanguageAPI.PluginName, LanguageAPI.PluginVersion)]
public sealed class LanguagePlugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger { get; set; }

    private static readonly string[] CustomLangs = { "la", "eo", "uk" };
    private static readonly string[] CultureFallbackLangs = { "la", "eo" };
    private static TMP_FontAsset _cachedFont;

    private Harmony _harmony;
    private Action<List<string>> _collectLanguageRootFoldersHandler;

    private void Awake()
    {
        Logger = base.Logger;
        LanguageAPI.Log = Logger;
        Logger.LogInfo("R2API.Language (Jaosnake fork) inicializado!");

        LanguageDebugUI.Create();
        LanguagePauseButton.Init();
        ConsoleCommands.Init();

        _collectLanguageRootFoldersHandler = folders =>
        {
            var pelePath = System.IO.Path.Combine(BepInEx.Paths.PluginPath, "PELE", "Language");
            if (Directory.Exists(pelePath))
            {
                folders.Add(pelePath);
                Logger.LogInfo("Pasta PELE/Language registrada: " + pelePath);
            }
        };
        Language.collectLanguageRootFolders += _collectLanguageRootFoldersHandler;

        try
        {
            _harmony = Harmony.CreateAndPatchAll(typeof(LanguagePlugin), "jaosnake.r2api.language");
            Logger.LogInfo("Harmony patches aplicados");
        }
        catch (Exception ex)
        {
            Logger.LogError("Falha ao aplicar Harmony patches: " + ex.Message);
        }
    }

    private void OnEnable()
    {
        LanguageAPI.SetHooks();

        ReloadPeleJsonFiles();
        Logger.LogInfo("PELE JSONs carregados no startup (" + GetPeleTokenCount() + " tokens)");

        var pluginPath = BepInEx.Paths.PluginPath;
        LanguageAPI.EnableHotReload(pluginPath);

        LanguageHotReload.Instance.OnAfterReload += OnHotReloadCompleted;

        Logger.LogInfo("Hot-Reload habilitado! Pressione F5 para recarregar manualmente.");
    }

    private void OnDisable()
    {
        LanguageAPI.UnsetHooks();
        LanguageAPI.DisableHotReload();
        Language.collectLanguageRootFolders -= _collectLanguageRootFoldersHandler;
        LanguageHotReload.Instance.OnAfterReload -= OnHotReloadCompleted;
    }

    private void OnDestroy()
    {
        LanguageAPI.DisableHotReload();
        LanguageAPI.DisposeLock();
        Language.collectLanguageRootFolders -= _collectLanguageRootFoldersHandler;
        LanguageHotReload.Instance.OnAfterReload -= OnHotReloadCompleted;
        LanguageDebugUI.DestroyInstance();
        LanguagePauseButton.Cleanup();
        ConsoleCommands.Cleanup();
        _harmony?.UnpatchSelf();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Logger.LogInfo("F5 pressionado! Recarregando todas as linguagens...");
            LanguageAPI.ReloadAllLanguageFiles(BepInEx.Paths.PluginPath);
            ReloadPeleJsonFiles();
            Logger.LogInfo("Recarregamento concluido!");
        }
    }

    private void OnHotReloadCompleted()
    {
        ReloadPeleJsonFiles();
    }

    public static void ReloadPeleJsonFiles()
    {
        LanguageAPI.InvalidateDiskCache();
        try
        {
            var peleDir = System.IO.Path.Combine(BepInEx.Paths.PluginPath, "PELE", "Language");
            if (!Directory.Exists(peleDir)) return;

            foreach (var langDir in Directory.GetDirectories(peleDir))
            {
                var langCode = System.IO.Path.GetFileName(langDir);
                if (string.IsNullOrEmpty(langCode)) continue;

                foreach (var jsonFile in Directory.GetFiles(langDir, "*.json"))
                {
                    try
                    {
                        var content = File.ReadAllText(jsonFile);
                        var json = JSON.Parse(content);
                        if (json == null) continue;

                        AddJsonTokens(json, langCode);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("Erro ao recarregar " + jsonFile + ": " + ex.Message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Erro em ReloadPeleJsonFiles: " + ex.Message);
        }
    }

    private static int AddJsonTokens(JSONNode json, string langCode)
    {
        if (json == null) return 0;

        var strings = json["strings"];
        if (strings != null && strings.Count > 0)
            return AddTokenObject(strings, langCode);

        return AddTokenObject(json, langCode, skipManifestKeys: true);
    }

    private static int AddTokenObject(JSONNode node, string langCode, bool skipManifestKeys = false)
    {
        int count = 0;
        foreach (var key in node.Keys)
        {
            if (skipManifestKeys && string.Equals(key, "language", StringComparison.OrdinalIgnoreCase))
                continue;
            if (skipManifestKeys && string.Equals(key, "strings", StringComparison.OrdinalIgnoreCase))
                continue;
            if (key.StartsWith("_", StringComparison.Ordinal))
                continue;

            var valueNode = node[key];
            if (valueNode == null || valueNode.Count > 0)
                continue;

            var val = valueNode.Value;
            if (val == null)
                continue;

            LanguageAPI.AddOrUpdateToken(key, val, langCode);
            count++;
        }
        return count;
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
        Logger.LogInfo("Lazy-registrado: " + __0);
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
            if (ctor == null) { Logger.LogError("CreateLanguage: Construtor Language(string) nao encontrado para " + lang); return null; }

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
            var peleDir = System.IO.Path.Combine(BepInEx.Paths.PluginPath, "PELE", "Language");
            var subPath = System.IO.Path.Combine(peleDir, lang);
            if (Directory.Exists(subPath))
                setFolders?.Invoke(langObj, new object[] { new[] { subPath } });

            var finalName = nameField != null ? (string)nameField.GetValue(langObj) : "(field not found)";
            Logger.LogInfo("CreateLanguage: '" + lang + "' ok (nameField='" + finalName + "')");
            return langObj;
        }
        catch (Exception ex)
        {
            Logger.LogError("CreateLanguage falhou para " + lang + ": " + (ex.InnerException?.Message ?? ex.Message));
            Logger.LogError("  Stack: " + (ex.InnerException?.StackTrace ?? ex.StackTrace));
            return null;
        }
    }



    internal static string[] GetRegisteredCustomLangs() => (string[])CustomLangs.Clone();
    internal static string[] GetFallbackLangs() => (string[])CultureFallbackLangs.Clone();

    public static string FixCultureName(string name)
    {
        if (Array.IndexOf(CultureFallbackLangs, name) >= 0) return "en";
        return name;
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

            Logger.LogInfo("SetCurrentLanguage interceptado: '" + newCurrentLanguageName + "' (CultureInfo='" + CultureInfo.CurrentCulture.Name + "')");
        }
        catch (Exception ex)
        {
            Logger.LogError("SetCurrentLanguagePrefix falhou: " + (ex.InnerException?.Message ?? ex.Message));
            return true;
        }

        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HGTextMeshProUGUI), "OnCurrentLanguageChanged")]
    private static void OnLanguageChanged()
    {
        var customFont = GetCustomFontForCurrentLanguage();
        if (customFont != null)
            HGTextMeshProUGUI.defaultLanguageFont = customFont;
    }

    private static TMP_FontAsset GetCustomFontForCurrentLanguage()
    {
        var current = Language.currentLanguageName;
        if (current == null) return null;
        if (!"uk".Equals(current, StringComparison.OrdinalIgnoreCase)) return null;
        return TryLoadFont("ukrainianfont");
    }

    private static TMP_FontAsset TryLoadFont(string resourceName)
    {
        if (_cachedFont != null) return _cachedFont;

        try
        {
            var stream = TryGetResourceStream(resourceName);
            if (stream == null) return null;

            using (stream)
            {
                var bundle = AssetBundle.LoadFromStream(stream);
                if (bundle == null) return null;
                var font = bundle.LoadAsset<TMP_FontAsset>("Assets/PELE-Font.asset");
                if (font != null) _cachedFont = font;
                return font;
            }
        }
        catch
        {
            return null;
        }
    }

    private static Stream TryGetResourceStream(string resourceName)
    {
        var asm = Assembly.GetExecutingAssembly();
        return asm.GetManifestResourceStream("R2API.Language." + resourceName)
            ?? asm.GetManifestResourceStream("PELE." + resourceName);
    }

    private static int GetPeleTokenCount()
    {
        try
        {
            var peleDir = System.IO.Path.Combine(BepInEx.Paths.PluginPath, "PELE", "Language");
            if (!Directory.Exists(peleDir)) return 0;

            int total = 0;
            foreach (var langDir in Directory.GetDirectories(peleDir))
            {
                var langCode = System.IO.Path.GetFileName(langDir);
                if (string.IsNullOrEmpty(langCode)) continue;

                foreach (var jsonFile in Directory.GetFiles(langDir, "*.json"))
                {
                    var content = File.ReadAllText(jsonFile);
                    var json = SimpleJSON.JSON.Parse(content);
                    if (json == null) continue;

                    total += CountJsonTokens(json);
                }
            }
            return total;
        }
        catch { return 0; }
    }

    private static int CountJsonTokens(JSONNode json)
    {
        if (json == null) return 0;

        var strings = json["strings"];
        if (strings != null && strings.Count > 0)
            return CountTokenObject(strings);

        return CountTokenObject(json, skipManifestKeys: true);
    }

    private static int CountTokenObject(JSONNode node, bool skipManifestKeys = false)
    {
        int total = 0;
        foreach (var key in node.Keys)
        {
            if (skipManifestKeys && string.Equals(key, "language", StringComparison.OrdinalIgnoreCase))
                continue;
            if (skipManifestKeys && string.Equals(key, "strings", StringComparison.OrdinalIgnoreCase))
                continue;
            if (key.StartsWith("_", StringComparison.Ordinal))
                continue;
            if (node[key] != null && node[key].Count == 0)
                total++;
        }
        return total;
    }
}
