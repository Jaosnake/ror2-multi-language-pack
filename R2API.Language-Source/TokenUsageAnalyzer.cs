using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace R2API;

public class TokenUsageAnalyzer
{
    // Matches Language.GetLocalizedStringByToken("TOKEN") and GetLocalizedStringByToken("TOKEN").
    private static readonly Regex TokenRefRegex = new(
        @"(?:Language\.)?GetLocalizedStringByToken\(""([^""]+)""\)",
        RegexOptions.Compiled);

    public List<string> FindUnusedTokens(string directory)
    {
        var defined  = GetAllTokenNames(directory);
        var used     = GetTokensReferencedInCode(directory);
        return defined.Except(used, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public List<string> FindBrokenReferences(string directory)
    {
        var defined = GetAllTokenNames(directory);
        var used    = GetTokensReferencedInCode(directory);
        return used.Except(defined, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public Dictionary<string, int> GetTokenUsageStats(string directory)
    {
        var defined = GetAllTokenNames(directory);
        var stats   = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var t in defined) stats[t] = 0;

        foreach (var file in LanguageFileHelper.GetCodeFiles(directory))
        {
            try
            {
                var content = File.ReadAllText(file);
                foreach (var token in defined)
                {
                    var count = Regex.Matches(content, Regex.Escape(token)).Count;
                    if (count > 0) stats[token] += count;
                }
            }
            catch { }
        }

        return stats;
    }

    private HashSet<string> GetAllTokenNames(string directory)
    {
        var tokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string[] files;
        try { files = LanguageFileHelper.GetLanguageFiles(directory); }
        catch { return tokens; }

        foreach (var file in files)
        {
            try
            {
                var parsed = LanguageFileHelper.ParseTokensFromFile(file);
                foreach (var langKvp in parsed)
                    foreach (var tokenName in langKvp.Value.Keys)
                        tokens.Add(tokenName);
            }
            catch { }
        }

        return tokens;
    }

    private HashSet<string> GetTokensReferencedInCode(string directory)
    {
        var referenced = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in LanguageFileHelper.GetCodeFiles(directory))
        {
            try
            {
                var content = File.ReadAllText(file);
                foreach (Match m in TokenRefRegex.Matches(content))
                    if (m.Groups.Count > 1)
                        referenced.Add(m.Groups[1].Value);
            }
            catch { }
        }

        return referenced;
    }
}
