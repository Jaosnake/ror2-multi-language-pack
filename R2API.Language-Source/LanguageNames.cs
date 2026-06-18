using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RoR2;

namespace R2API;

internal static class LanguageNames
{
    private static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        { "en", "English" },
        { "pt-BR", "Portuguese (Brazil)" },
        { "es-ES", "Spanish (Spain)" },
        { "de", "German" },
        { "fr", "French" },
        { "it", "Italian" },
        { "ja", "Japanese" },
        { "ko", "Korean" },
        { "ru", "Russian" },
        { "tr", "Turkish" },
        { "uk", "Ukrainian" },
        { "zh-CN", "Chinese (Simplified)" },
        { "la", "Latin" },
        { "eo", "Esperanto" },

    };

    internal static string GetFriendlyName(string langCode)
    {
        return Map.TryGetValue(langCode, out var name) ? name : langCode;
    }

    internal static List<string> GetAvailableLanguages(bool includeCustom = true)
    {
        try
        {
            var field = typeof(Language).GetField("languagesByName", BindingFlags.Static | BindingFlags.NonPublic);
            if (field?.GetValue(null) is Dictionary<string, Language> dict)
            {
                var result = new HashSet<string>(dict.Keys, StringComparer.OrdinalIgnoreCase);
                if (includeCustom)
                {
                    foreach (var c in new[] { "uk", "la", "eo" })
                        if (!result.Contains(c))
                        {
                            Language.FindLanguageByName(c);
                            result.Add(c);
                        }
                }
                return result.OrderBy(x => x).ToList();
            }
        }
        catch { }
        return new List<string>();
    }
}
