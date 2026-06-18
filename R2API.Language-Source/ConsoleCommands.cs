using System;
using System.Collections;
using System.Reflection;
using BepInEx;
using RoR2;
using RoR2.ConVar;
using UnityEngine;

namespace R2API;

internal static class ConsoleCommands
{
    private static bool _registered;

    internal static void Init()
    {
        On.RoR2.Console.InitConVarsCoroutine += OnConsoleInit;
    }

    internal static void Cleanup()
    {
        On.RoR2.Console.InitConVarsCoroutine -= OnConsoleInit;
    }

    private static System.Collections.IEnumerator OnConsoleInit(On.RoR2.Console.orig_InitConVarsCoroutine orig, RoR2.Console self)
    {
        yield return orig(self);
        RegisterCommands();
    }

    private static void RegisterCommands()
    {
        if (_registered) return;
        _registered = true;

        var console = RoR2.Console.instance;
        if (console == null) return;

        var catalogField = typeof(RoR2.Console).GetField("concommandCatalog", BindingFlags.Instance | BindingFlags.NonPublic);
        if (catalogField == null)
        {
            LanguagePlugin.Logger.LogWarning("concommandCatalog field not found via reflection");
            return;
        }

        var conCommandType = typeof(RoR2.Console).GetNestedType("ConCommand", BindingFlags.NonPublic);
        if (conCommandType == null)
        {
            LanguagePlugin.Logger.LogWarning("ConCommand nested type not found");
            return;
        }

        var catalog = catalogField.GetValue(console) as System.Collections.IDictionary;
        if (catalog == null)
        {
            LanguagePlugin.Logger.LogWarning("concommandCatalog is null");
            return;
        }

        var flagsField = conCommandType.GetField("flags");
        var helpTextField = conCommandType.GetField("helpText");
        var actionField = conCommandType.GetField("action", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (actionField == null)
        {
            LanguagePlugin.Logger.LogWarning("ConCommand.action field not found");
            return;
        }
        var delegateType = actionField.FieldType;

        var asm = Assembly.GetExecutingAssembly();
        foreach (var type in asm.GetTypes())
        {
            foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var attr = method.GetCustomAttribute<ConCommandAttribute>();
                if (attr == null) continue;

                var cmd = Activator.CreateInstance(conCommandType);
                flagsField.SetValue(cmd, attr.flags);
                helpTextField.SetValue(cmd, attr.helpText);
                actionField.SetValue(cmd, Delegate.CreateDelegate(delegateType, method));

                catalog[attr.commandName.ToLower()] = cmd;
            }
        }

        LanguagePlugin.Logger.LogInfo("Console commands registered: /reloadlang, /langdebug, /langstatus");
    }

    [ConCommand(commandName = "reloadlang", flags = ConVarFlags.None, helpText = "Recarrega todos os arquivos de idioma (.language + PELE/Language JSONs)")]
    private static void CC_ReloadLang(ConCommandArgs args)
    {
        LanguageAPI.InvalidateDiskCache();
        LanguageAPI.ReloadAllLanguageFiles(Paths.PluginPath);
        LanguagePlugin.ReloadPeleJsonFiles();
        Debug.Log("[R2API.Language] /reloadlang: todos os arquivos recarregados.");
    }

    [ConCommand(commandName = "langdebug", flags = ConVarFlags.None, helpText = "Alterna a janela de debug do R2API.Language")]
    private static void CC_LangDebug(ConCommandArgs args)
    {
        LanguageDebugUI.Toggle();
        Debug.Log("[R2API.Language] /langdebug: janela de debug alternada.");
    }

    [ConCommand(commandName = "langstatus", flags = ConVarFlags.None, helpText = "Exibe o status atual do R2API.Language no console")]
    private static void CC_LangStatus(ConCommandArgs args)
    {
        var langs = string.Join(", ", LanguagePlugin.GetRegisteredCustomLangs());
        var fallback = string.Join(", ", LanguagePlugin.GetFallbackLangs());
        var current = Language.currentLanguageName;
        var apiTokens = LanguageAPI.GetTotalCustomTokens();
        var langFiles = LanguageAPI.GetLanguageFileCount();
        var peleFiles = LanguageAPI.GetPeleJsonFileCount();
        var peleLangs = string.Join(", ", LanguageAPI.GetPeleLanguageFolders());
        var reload = LanguageAPI.IsHotReloadEnabled ? "ativo" : "inativo";

        Debug.Log($"[R2API.Language] === Status ===");
        Debug.Log($"  Current language: {current}");
        Debug.Log($"  Custom langs: {langs}");
        Debug.Log($"  Fallback (en): {fallback}");
        Debug.Log($"  Hot-Reload: {reload}");
        Debug.Log($"  .language files: {langFiles}");
        Debug.Log($"  PELE JSON files: {peleFiles}");
        Debug.Log($"  PELE language folders: {peleLangs}");
        Debug.Log($"  API tokens: {apiTokens}");
        Debug.Log($"[R2API.Language] ===============");
    }
}
