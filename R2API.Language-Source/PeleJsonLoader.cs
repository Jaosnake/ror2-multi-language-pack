using System;
using System.IO;
using BepInEx;
using SimpleJSON;

namespace R2API;

internal static class PeleJsonLoader
{
    internal static void Reload()
    {
        LanguageAPI.InvalidateDiskCache();
        try
        {
            var peleDir = Path.Combine(Paths.PluginPath, "PELE", "Language");
            if (!Directory.Exists(peleDir)) return;

            foreach (var langDir in Directory.GetDirectories(peleDir))
            {
                var langCode = Path.GetFileName(langDir);
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
                        LanguagePlugin.Logger?.LogError("Erro ao recarregar " + jsonFile + ": " + ex.Message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LanguagePlugin.Logger?.LogError("Erro em PeleJsonLoader.Reload: " + ex.Message);
        }
    }

    internal static int CountTokens()
    {
        try
        {
            var peleDir = Path.Combine(Paths.PluginPath, "PELE", "Language");
            if (!Directory.Exists(peleDir)) return 0;

            int total = 0;
            foreach (var langDir in Directory.GetDirectories(peleDir))
            {
                var langCode = Path.GetFileName(langDir);
                if (string.IsNullOrEmpty(langCode)) continue;

                foreach (var jsonFile in Directory.GetFiles(langDir, "*.json"))
                {
                    try
                    {
                        var json = JSON.Parse(File.ReadAllText(jsonFile));
                        if (json != null) total += CountJsonTokens(json);
                    }
                    catch
                    {
                        // Detailed read errors are logged by Reload(); this count is only startup summary.
                    }
                }
            }

            return total;
        }
        catch
        {
            return 0;
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

    private static int CountJsonTokens(JSONNode json)
    {
        var strings = json["strings"];
        if (strings != null && strings.Count > 0)
            return CountTokenObject(strings);

        return CountTokenObject(json, skipManifestKeys: true);
    }

    private static int CountTokenObject(JSONNode node, bool skipManifestKeys = false)
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
            if (node[key] == null || node[key].Count > 0)
                continue;
            count++;
        }
        return count;
    }
}
