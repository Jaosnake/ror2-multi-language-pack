using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using Newtonsoft.Json.Linq;
using SimpleJSON;

namespace R2API;

internal static class PeleJsonLoader
{
    private static Dictionary<string, Dictionary<string, string>> _lastValidFiles =
        new(StringComparer.OrdinalIgnoreCase);
    private static Dictionary<string, string[]> _languageFiles = new(StringComparer.OrdinalIgnoreCase);

    internal static IEnumerable<string> FindSources(string language, string token)
    {
        if (!_languageFiles.TryGetValue(language, out var files)) yield break;
        foreach (var file in files.Reverse())
            if (_lastValidFiles.TryGetValue(file, out var tokens) && tokens.ContainsKey(token))
                yield return Path.GetRelativePath(Paths.PluginPath, file);
    }

    internal static void Reload()
    {
        LanguageAPI.InvalidateDiskCache();
        try
        {
            var peleDir = PelePaths.LanguageRoot(Paths.PluginPath);
            if (!Directory.Exists(peleDir)) return;

            var languages = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            var validFiles = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            var languageFiles = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            var reloadedFiles = new List<string>();
            foreach (var langDir in Directory.GetDirectories(peleDir))
            {
                var langCode = Path.GetFileName(langDir);
                if (string.IsNullOrEmpty(langCode)) continue;

                var tokens = new Dictionary<string, string>();
                languages[langCode] = tokens;
                var files = Directory.GetFiles(langDir, "*.json").OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray();
                languageFiles[langCode] = files;
                foreach (var jsonFile in files)
                {
                    Dictionary<string, string> fileTokens;
                    try
                    {
                        var content = File.ReadAllText(jsonFile);
                        var document = JObject.Parse(content, new JsonLoadSettings
                        {
                            DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error
                        });
                        if (document.TryGetValue("strings", out var strings) && strings is not JObject)
                            throw new InvalidDataException("A propriedade 'strings' deve ser um objeto.");
                        fileTokens = new Dictionary<string, string>();
                        AddJsonTokens(JSON.Parse(content), fileTokens);
                        reloadedFiles.Add(jsonFile);
                    }
                    catch (Exception ex)
                    {
                        bool recovered = _lastValidFiles.TryGetValue(jsonFile, out fileTokens);
                        LanguagePlugin.Logger?.LogWarning($"JSON inválido ou indisponível: {jsonFile}. " +
                            (recovered ? "Última versão válida mantida. " : "Arquivo ignorado. ") + ex.Message);
                        if (!recovered) continue;
                    }
                    validFiles[jsonFile] = fileTokens;
                    foreach (var pair in fileTokens) tokens[pair.Key] = pair.Value;
                }
            }
            LanguageAPI.RegisterPeleJsonTokens(languages);
            _lastValidFiles = validFiles;
            _languageFiles = languageFiles;
            foreach (var file in reloadedFiles)
                LanguageHotReload.Instance.NotifyFileReloaded(file);
            foreach (var language in languages)
                foreach (var token in language.Value.Keys)
                    LanguageHotReload.Instance.NotifyTokenReloaded(language.Key, token);
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
            var peleDir = PelePaths.LanguageRoot(Paths.PluginPath);
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

    private static int AddJsonTokens(JSONNode json, Dictionary<string, string> tokens)
    {
        if (json == null) return 0;

        var strings = json["strings"];
        if (strings != null && strings.Count > 0)
            return AddTokenObject(strings, tokens);

        return AddTokenObject(json, tokens, skipManifestKeys: true);
    }

    private static int AddTokenObject(JSONNode node, Dictionary<string, string> tokens, bool skipManifestKeys = false)
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

            tokens[key] = val;
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
