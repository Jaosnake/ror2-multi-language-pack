using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using RoR2;
using UnityEngine;

namespace R2API;

[BepInPlugin(LanguageAPI.PluginGUID, LanguageAPI.PluginName, LanguageAPI.PluginVersion)]
public sealed class LanguagePlugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger { get; set; }

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
            _harmony = new Harmony("jaosnake.r2api.language");
            _harmony.PatchAll(typeof(LanguagePlugin));
            _harmony.PatchAll(typeof(CustomLanguageRegistration));
            _harmony.PatchAll(typeof(CyrillicFontSupport));
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
        PeleStartupDiagnostics.LogOnce();

        ReloadPeleJsonFiles();
        Logger.LogInfo("PELE JSONs carregados no startup (" + PeleJsonLoader.CountTokens() + " tokens)");

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
        PeleJsonLoader.Reload();
    }

    internal static string[] GetRegisteredCustomLangs() => CustomLanguageRegistration.GetRegisteredCustomLangs();
    internal static string[] GetFallbackLangs() => CustomLanguageRegistration.GetFallbackLangs();
    public static string FixCultureName(string name) => CustomLanguageRegistration.FixCultureName(name);

}
