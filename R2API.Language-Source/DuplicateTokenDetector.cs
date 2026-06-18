using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace R2API;

public class DuplicateTokenDetector
{
    public List<DuplicateEntry> FindDuplicateTokens(string directory)
    {
        var files = LanguageFileHelper.GetLanguageFiles(directory);
        var tokenCounts = new Dictionary<string, List<string>>();

        foreach (var file in files)
        {
            var tokens = LanguageFileHelper.ParseTokensFromFile(file);
            var fileName = Path.GetFileName(file);

            foreach (var key in tokens.Keys)
            {
                if (!tokenCounts.ContainsKey(key))
                    tokenCounts[key] = new List<string>();
                tokenCounts[key].Add(fileName);
            }
        }

        return tokenCounts
            .Where(kvp => kvp.Value.Count > 1)
            .Select(kvp => new DuplicateEntry
            {
                TokenName = kvp.Key,
                Files = kvp.Value,
                Count = kvp.Value.Count
            })
            .ToList();
    }

    public bool HasDuplicates(string tokenName, string directory)
    {
        return FindDuplicateTokens(directory).Any(d => d.TokenName == tokenName);
    }
}

public class DuplicateEntry
{
    public string TokenName { get; set; }
    public List<string> Files { get; set; }
    public int Count { get; set; }
}
