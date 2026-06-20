using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using RoR2;
using UnityEngine;

namespace R2API;

[BepInPlugin(LanguageAPI.PluginGUID, LanguageAPI.PluginName, LanguageAPI.PluginVersion)]
public sealed class LanguagePlugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger { get; set; }
    internal static bool EnableDebugMenu => _enableDebugMenu?.Value ?? false;
    internal static bool EnableVerboseLogging => _enableVerboseLogging?.Value ?? false;
    internal static bool EnableHotReload => _enableHotReload?.Value ?? true;

    private Harmony _harmony;
    private Action<List<string>> _collectLanguageRootFoldersHandler;
    private static ConfigEntry<bool> _enableDebugMenu;
    private static ConfigEntry<bool> _enableVerboseLogging;
    private static ConfigEntry<bool> _enableHotReload;

    private void Awake()
    {
        Logger = base.Logger;
        LanguageAPI.Log = Logger;
        BindConfig();
        Logger.LogInfo("R2API.Language (Jaosnake fork) inicializado!");

        if (EnableDebugMenu)
            LanguageDebugUI.Create();
        LanguagePauseMenu.Init();
        ConsoleCommands.Init();

        _collectLanguageRootFoldersHandler = folders =>
        {
            var pelePath = System.IO.Path.Combine(BepInEx.Paths.PluginPath, "PELE", "Language");
            if (Directory.Exists(pelePath))
            {
                folders.Add(pelePath);
                LogVerbose("Pasta PELE/Language registrada: " + pelePath);
            }
        };
        Language.collectLanguageRootFolders += _collectLanguageRootFoldersHandler;

        try
        {
            _harmony = new Harmony("jaosnake.r2api.language");
            _harmony.PatchAll(typeof(LanguagePlugin));
            _harmony.PatchAll(typeof(CustomLanguageRegistration));
            _harmony.PatchAll(typeof(CyrillicFontSupport));
            LogVerbose("Harmony patches aplicados");
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

        if (EnableHotReload)
        {
            var pluginPath = BepInEx.Paths.PluginPath;
            LanguageAPI.EnableHotReload(pluginPath);
            LanguageHotReload.Instance.OnAfterReload += OnHotReloadCompleted;

            Logger.LogInfo("Hot-Reload habilitado! Pressione F5 para recarregar manualmente.");
        }
        else
        {
            Logger.LogInfo("Hot-Reload desabilitado por config.");
        }
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
        LanguagePauseMenu.Cleanup();
        ConsoleCommands.Cleanup();
        _harmony?.UnpatchSelf();
    }

    private void Update()
    {
        if (EnableHotReload && Input.GetKeyDown(KeyCode.F5))
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

    internal static void LogVerbose(string message)
    {
        if (EnableVerboseLogging)
            Logger?.LogInfo(message);
    }

    private void BindConfig()
    {
        _enableHotReload = Config.Bind(
            "PELE",
            "EnableHotReload",
            true,
            "Enable F5/manual and file watcher language reload.");

        _enableDebugMenu = Config.Bind(
            "PELE",
            "EnableDebugMenu",
            false,
            "Enable the F6 PELE debug window.");

        _enableVerboseLogging = Config.Bind(
            "PELE",
            "EnableVerboseLogging",
            false,
            "Enable extra diagnostic logs for PELE hooks and UI layout.");
    }
}
