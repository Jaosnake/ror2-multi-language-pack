using System;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;

namespace R2API;

internal static class LanguageFileHelper
{
    public static string[] GetLanguageFiles(string directory)
    {
        if (!Directory.Exists(directory))
            throw new DirectoryNotFoundException($"Diretorio nao encontrado: {directory}");

        return Directory.GetFiles(directory, "*.language", SearchOption.AllDirectories);
    }

    public static Dictionary<string, Dictionary<string, string>> ParseTokensFromFile(string filePath)
    {
        var content = File.ReadAllText(filePath).TrimStart();

        if (content.StartsWith("{"))
            return ParseJsonTokens(content);

        return ParseSimpleTokens(content, DetectLanguageFromFileName(filePath));
    }

    private static Dictionary<string, Dictionary<string, string>> ParseJsonTokens(string content)
    {
        var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        var json = JSON.Parse(content);
        if (json == null) return result;

        foreach (var langKey in json.Keys)
        {
            var langObj = json[langKey];
            if (langObj == null) continue;

            var langName = langKey.Equals("strings", StringComparison.OrdinalIgnoreCase) ? "generic" : langKey;
            var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var tokenKey in langObj.Keys)
                tokens[tokenKey] = langObj[tokenKey].Value;

            result[langName] = tokens;
        }

        return result;
    }

    private static Dictionary<string, Dictionary<string, string>> ParseSimpleTokens(string content, string language)
    {
        var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var lines = content.Split('\n');

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)
                || trimmed.StartsWith("//")
                || trimmed.StartsWith("@"))
                continue;

            var eqIdx = trimmed.IndexOf('=');
            if (eqIdx < 1) continue;

            var key = trimmed.Substring(0, eqIdx).Trim();
            var val = trimmed.Substring(eqIdx + 1).Trim();

            if (val.StartsWith("\"") && val.EndsWith("\"") && val.Length >= 2)
                val = val.Substring(1, val.Length - 2);

            if (!string.IsNullOrEmpty(key))
                tokens[key] = val;
        }

        return new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            { language, tokens }
        };
    }

    internal static string DetectLanguageFromFileName(string filePath)
    {
        var name = Path.GetFileNameWithoutExtension(filePath);
        if (name == null) return "generic";

        if (name.EndsWith("_eo",    StringComparison.OrdinalIgnoreCase)) return "eo";
        if (name.EndsWith("_la",    StringComparison.OrdinalIgnoreCase)) return "la";
        if (name.EndsWith("_uk",    StringComparison.OrdinalIgnoreCase)) return "uk";
        if (name.EndsWith("_pt-BR", StringComparison.OrdinalIgnoreCase)) return "pt-BR";
        if (name.EndsWith("_en",    StringComparison.OrdinalIgnoreCase)) return "en";
        if (name.EndsWith("_de",    StringComparison.OrdinalIgnoreCase)) return "de";
        if (name.EndsWith("_fr",    StringComparison.OrdinalIgnoreCase)) return "fr";
        if (name.EndsWith("_es",    StringComparison.OrdinalIgnoreCase)) return "es-ES";
        if (name.EndsWith("_ru",    StringComparison.OrdinalIgnoreCase)) return "ru";
        if (name.EndsWith("_ja",    StringComparison.OrdinalIgnoreCase)) return "ja";
        if (name.EndsWith("_ko",    StringComparison.OrdinalIgnoreCase)) return "ko";
        if (name.EndsWith("_zh",    StringComparison.OrdinalIgnoreCase)) return "zh-CN";
        if (name.EndsWith("_tr",    StringComparison.OrdinalIgnoreCase)) return "tr";
        if (name.EndsWith("_it",    StringComparison.OrdinalIgnoreCase)) return "it";

        return "generic";
    }

    public static string[] GetCodeFiles(string directory)
    {
        if (!Directory.Exists(directory))
            return Array.Empty<string>();

        return Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories);
    }
}
