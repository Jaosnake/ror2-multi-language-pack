using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace R2API;

[BepInPlugin(LanguageAPI.PluginGUID, LanguageAPI.PluginName, LanguageAPI.PluginVersion)]
public sealed class LanguagePlugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger { get; set; }

    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo("R2API.Language inicializado!");
    }

    private void OnEnable()
    {
        LanguageAPI.SetHooks();
        
        // Habilita Hot-Reload automaticamente para o diretório de plugins
        var pluginPath = BepInEx.Paths.PluginPath;
        Logger.LogInfo($"Habilitando Hot-Reload para: {pluginPath}");
        LanguageAPI.EnableHotReload(pluginPath);
        
        Logger.LogInfo("Hot-Reload habilitado! Arquivos .language serão recarregados automaticamente.");
        Logger.LogInfo("[DICA] Pressione F5 para recarregar todas as linguagens manualmente.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Logger.LogInfo("F5 pressionado! Recarregando todas as linguagens...");
            LanguageAPI.ReloadAllLanguageFiles(BepInEx.Paths.PluginPath);
            Logger.LogInfo("Recarregamento concluído!");
        }
    }

    private void OnDisable()
    {
        LanguageAPI.UnsetHooks();
        LanguageAPI.DisableHotReload();
    }

    private void OnDestroy()
    {
        LanguageAPI.DisableHotReload();
    }
}
