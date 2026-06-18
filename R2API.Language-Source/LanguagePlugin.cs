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

                        foreach (var key in json.Keys)
                        {
                            var val = json[key].Value;
                            if (val != null)
                                LanguageAPI.AddOrUpdateToken(key, val, langCode);
                        }
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
            if (ctor == null) { Logger.LogError("Construtor Language(string) nao encontrado"); return null; }

            var langObj = (Language)ctor.Invoke(new object[] { lang });
            var setFolders = typeof(Language).GetMethod("SetFolders", BindingFlags.Instance | BindingFlags.NonPublic);

            var peleDir = System.IO.Path.Combine(BepInEx.Paths.PluginPath, "PELE", "Language");
            var subPath = System.IO.Path.Combine(peleDir, lang);
            if (Directory.Exists(subPath))
                setFolders?.Invoke(langObj, new object[] { new[] { subPath } });

            return langObj;
        }
        catch (Exception ex)
        {
            Logger.LogError("CreateLanguage falhou para " + lang + ": " + ex.Message);
            return null;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Language), "SetCurrentLanguage")]
    private static void SetCurrentLanguagePrefix(ref string __0)
    {
        if (Array.IndexOf(CultureFallbackLangs, __0) >= 0)
            __0 = "en";
    }

    internal static string[] GetRegisteredCustomLangs() => (string[])CustomLangs.Clone();
    internal static string[] GetFallbackLangs() => (string[])CultureFallbackLangs.Clone();

    public static string FixCultureName(string name)
    {
        if (Array.IndexOf(CultureFallbackLangs, name) >= 0) return "en";
        return name;
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

                    foreach (var key in json.Keys)
                    {
                        var val = json[key].Value;
                        if (val != null)
                            total++;
                    }
                }
            }
            return total;
        }
        catch { return 0; }
    }
}
