using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using SimpleJSON;

namespace R2API;

internal static class PeleStartupDiagnostics
{
    private static readonly string[] RequiredCustomLanguages = { "la", "eo", "uk" };
    private static bool _logged;

    internal static void LogOnce()
    {
        if (_logged) return;
        _logged = true;

        try
        {
            LogPeleFolders();
            LogCyrillicFontBundle();
            LogDuplicateLanguageDlls();
        }
        catch (Exception ex)
        {
            LanguagePlugin.Logger?.LogWarning("PELE startup diagnostics falhou: " + ex.Message);
        }
    }

    private static void LogPeleFolders()
    {
        var languageRoot = Path.Combine(Paths.PluginPath, "PELE", "Language");
        if (!Directory.Exists(languageRoot))
        {
            LanguagePlugin.Logger?.LogWarning("PELE/Language nao encontrado: " + languageRoot);
            return;
        }

        LanguagePlugin.Logger?.LogInfo("PELE/Language encontrado: " + languageRoot);

        var tokenCounts = new List<string>();
        foreach (var lang in RequiredCustomLanguages)
        {
            var langDir = Path.Combine(languageRoot, lang);
            if (!Directory.Exists(langDir))
            {
                LanguagePlugin.Logger?.LogWarning("Idioma PELE ausente: " + langDir);
                continue;
            }

            var languageJson = Path.Combine(langDir, "language.json");
            if (!File.Exists(languageJson))
                LanguagePlugin.Logger?.LogWarning("language.json ausente para '" + lang + "': " + languageJson);

            var iconPath = Path.Combine(langDir, "icon.png");
            if (!File.Exists(iconPath))
                LanguagePlugin.Logger?.LogWarning("icon.png ausente para '" + lang + "': " + iconPath);

            tokenCounts.Add(lang + "=" + CountJsonTokens(langDir));
        }

        if (tokenCounts.Count > 0)
            LanguagePlugin.Logger?.LogInfo("Tokens PELE por idioma: " + string.Join(", ", tokenCounts));
    }

    private static void LogCyrillicFontBundle()
    {
        var fontPath = Path.Combine(Paths.PluginPath, "PELE", "Fonts", "cyrillicfont");
        if (File.Exists(fontPath))
            LanguagePlugin.Logger?.LogInfo("PELE/Fonts/cyrillicfont encontrado.");
        else
            LanguagePlugin.Logger?.LogWarning("PELE/Fonts/cyrillicfont ausente; ucraniano pode depender de fallback.");
    }

    private static void LogDuplicateLanguageDlls()
    {
        var dlls = Directory.GetFiles(Paths.PluginPath, "R2API.Language.dll", SearchOption.AllDirectories);
        if (dlls.Length == 1)
        {
            LanguagePlugin.Logger?.LogInfo("DLL R2API.Language unica detectada.");
            return;
        }

        LanguagePlugin.Logger?.LogWarning("Detectadas " + dlls.Length + " copias de R2API.Language.dll no perfil.");
        foreach (var dll in dlls.OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            LanguagePlugin.Logger?.LogWarning("  R2API.Language.dll: " + dll);
    }

    private static int CountJsonTokens(string langDir)
    {
        var total = 0;
        foreach (var jsonFile in Directory.GetFiles(langDir, "*.json"))
        {
            try
            {
                var json = JSON.Parse(File.ReadAllText(jsonFile));
                if (json == null) continue;

                var strings = json["strings"];
                total += strings != null && strings.Count > 0
                    ? CountTokenObject(strings)
                    : CountTokenObject(json, skipManifestKeys: true);
            }
            catch (Exception ex)
            {
                LanguagePlugin.Logger?.LogWarning("Falha ao contar tokens em " + jsonFile + ": " + ex.Message);
            }
        }

        return total;
    }

    private static int CountTokenObject(JSONNode node, bool skipManifestKeys = false)
    {
        var count = 0;
        foreach (var key in node.Keys)
        {
            if (skipManifestKeys && string.Equals(key, "language", StringComparison.OrdinalIgnoreCase))
                continue;
            if (skipManifestKeys && string.Equals(key, "strings", StringComparison.OrdinalIgnoreCase))
                continue;
            if (key.StartsWith("_", StringComparison.Ordinal))
                continue;
            if (node[key] == null || node[key].Count > 0)
                continue;
            count++;
        }

        return count;
    }
}
