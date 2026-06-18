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
        var result = new Dictionary<string, Dictionary<string, string>>();
        var json = JSON.Parse(content);
        if (json == null) return result;

        foreach (var langKey in json.Keys)
        {
            var langObj = json[langKey];
            if (langObj == null) continue;

            var langName = langKey == "strings" ? "generic" : langKey;
            var tokens = new Dictionary<string, string>();

            foreach (var tokenKey in langObj.Keys)
                tokens[tokenKey] = langObj[tokenKey].Value;

            result[langName] = tokens;
        }

        return result;
    }

    private static Dictionary<string, Dictionary<string, string>> ParseSimpleTokens(string content, string language)
    {
        var tokens = new Dictionary<string, string>();
        var lines = content.Split('\n');

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("//") || trimmed.StartsWith("@"))
                continue;

            var eqIdx = trimmed.IndexOf('=');
            if (eqIdx < 1) continue;

            var key = trimmed.Substring(0, eqIdx).Trim();
            var val = trimmed.Substring(eqIdx + 1).Trim();

            if (val.StartsWith("\"") && val.EndsWith("\""))
                val = val.Substring(1, val.Length - 2);

            if (!string.IsNullOrEmpty(key))
                tokens[key] = val;
        }

        return new Dictionary<string, Dictionary<string, string>> { { language, tokens } };
    }

    internal static string DetectLanguageFromFileName(string filePath)
    {
        var name = Path.GetFileNameWithoutExtension(filePath);
        if (name == null) return "generic";

        if (name.EndsWith("_eo")) return "eo";
        if (name.EndsWith("_la")) return "la";
        if (name.EndsWith("_uk")) return "uk";

        if (name.EndsWith("_pt-BR")) return "pt-BR";
        if (name.EndsWith("_en")) return "en";
        if (name.EndsWith("_de")) return "de";
        if (name.EndsWith("_fr")) return "fr";
        if (name.EndsWith("_es")) return "es-ES";

        return "generic";
    }

    public static string[] GetCodeFiles(string directory)
    {
        if (!Directory.Exists(directory))
            return Array.Empty<string>();

        return Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories);
    }
}
