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
        var tokenFiles = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            var parsed = LanguageFileHelper.ParseTokensFromFile(file);
            var fileName = Path.GetFileName(file);

            foreach (var langKvp in parsed)
            {
                foreach (var tokenName in langKvp.Value.Keys)
                {
                    if (!tokenFiles.TryGetValue(tokenName, out var list))
                    {
                        list = new List<string>();
                        tokenFiles[tokenName] = list;
                    }
                    if (!list.Contains(fileName))
                        list.Add(fileName);
                }
            }
        }

        return tokenFiles
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
        return FindDuplicateTokens(directory).Any(d =>
            d.TokenName.Equals(tokenName, StringComparison.OrdinalIgnoreCase));
    }
}

public class DuplicateEntry
{
    public string TokenName { get; set; }
    public List<string> Files { get; set; }
    public int Count { get; set; }
}
