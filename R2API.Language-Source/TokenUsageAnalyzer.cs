using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace R2API;

public class TokenUsageAnalyzer
{
    public List<string> FindUnusedTokens(string directory)
    {
        var defined = GetAllTokenNames(directory);
        var used = GetTokensReferencedInCode(directory);
        return defined.Except(used).ToList();
    }

    public List<string> FindBrokenReferences(string directory)
    {
        var defined = GetAllTokenNames(directory);
        var used = GetTokensReferencedInCode(directory);
        return used.Except(defined).ToList();
    }

    public Dictionary<string, int> GetTokenUsageStats(string directory)
    {
        var defined = GetAllTokenNames(directory);
        var stats = new Dictionary<string, int>();
        foreach (var t in defined) stats[t] = 0;

        var codeFiles = LanguageFileHelper.GetCodeFiles(directory);
        foreach (var file in codeFiles)
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
        var tokens = new HashSet<string>();
        var files = LanguageFileHelper.GetLanguageFiles(directory);

        foreach (var file in files)
        {
            var parsed = LanguageFileHelper.ParseTokensFromFile(file);
            foreach (var key in parsed.Keys)
                tokens.Add(key);
        }

        return tokens;
    }

    private HashSet<string> GetTokensReferencedInCode(string directory)
    {
        var referenced = new HashSet<string>();
        var codeFiles = LanguageFileHelper.GetCodeFiles(directory);

        foreach (var file in codeFiles)
        {
            try
            {
                var content = File.ReadAllText(file);
                var matches = Regex.Matches(content, @"Language\.GetLocalizedStringByToken\(""([^""]+)""\)");
                foreach (Match m in matches)
                    if (m.Groups.Count > 1)
                        referenced.Add(m.Groups[1].Value);
            }
            catch { }
        }

        return referenced;
    }
}
