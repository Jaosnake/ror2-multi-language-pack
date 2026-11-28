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
[BepInDependency("com.bepis.r2api.language", "1.1.0")]
public sealed class LanguagePlugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger { get; set; }
    internal static bool EnableDebugMenu => _enableDebugMenu?.Value ?? false;
    internal static bool EnableVerboseLogging => _enableVerboseLogging?.Value ?? false;
    internal static bool EnableHotReload => _enableHotReload?.Value ?? true;

    private Harmony _harmony;
    private bool _applicationQuitting;
    private Action<List<string>> _collectLanguageRootFoldersHandler;
    private static ConfigEntry<bool> _enableDebugMenu;
    private static ConfigEntry<bool> _enableVerboseLogging;
    private static ConfigEntry<bool> _enableHotReload;

    private void Awake()
    {
        Logger = base.Logger;
        LanguageAPI.Log = Logger;
        BindConfig();
        Application.quitting += MarkApplicationQuitting;
        Logger.LogInfo("P.E.L.E. Language Core inicializado.");

        if (EnableDebugMenu)
            LanguageDebugUI.Create();
        LanguagePauseMenu.Init();
        ConsoleCommands.Init();

        _collectLanguageRootFoldersHandler = folders =>
        {
            var pelePath = PelePaths.LanguageRoot(BepInEx.Paths.PluginPath);
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


            Logger.LogInfo("Hot-Reload habilitado! Pressione F5 para recarregar manualmente.");
        }
        else
        {
            Logger.LogInfo("Hot-Reload desabilitado por config.");
        }
    }

    private void MarkApplicationQuitting() => _applicationQuitting = true;

    private void OnDisable()
    {
        // The language integration is process-wide. Keep the official overlay alive through teardown.
        LanguageAPI.DisableHotReload();
        Language.collectLanguageRootFolders -= _collectLanguageRootFoldersHandler;

    }

    private void OnDestroy()
    {
        LanguageAPI.DisableHotReload();
        Application.quitting -= MarkApplicationQuitting;
        Language.collectLanguageRootFolders -= _collectLanguageRootFoldersHandler;

        LanguageDebugUI.DestroyInstance();
        LanguagePauseMenu.Cleanup();
        ConsoleCommands.Cleanup();
        if (!_applicationQuitting)
            _harmony?.UnpatchSelf();
    }

    private void Update()
    {
        if (EnableHotReload && Input.GetKeyDown(KeyCode.F5))
        {
            Logger.LogInfo("F5 pressionado! Recarregando traducoes do P.E.L.E...");
            LanguageAPI.ReloadAllLanguageFiles(BepInEx.Paths.PluginPath);
            Logger.LogInfo("Recarregamento concluido!");
        }
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

