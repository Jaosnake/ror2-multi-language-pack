using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace R2API;

public class DuplicateTokenDetector
{
    public List<DuplicateEntry> FindDuplicateTokens(string directory)
    {
        string[] files;
        try
        {
            files = LanguageFileHelper.GetLanguageFiles(directory);
        }
        catch (DirectoryNotFoundException)
        {
            return new List<DuplicateEntry>();
        }

        var tokenMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            Dictionary<string, Dictionary<string, string>> parsed;
            try { parsed = LanguageFileHelper.ParseTokensFromFile(file); }
            catch { continue; }

            var fileName = Path.GetFileName(file);

            foreach (var langKvp in parsed)
            {
                var lang = langKvp.Key;
                foreach (var tokenName in langKvp.Value.Keys)
                {
                    var key = $"{lang}:{tokenName}";
                    if (!tokenMap.TryGetValue(key, out var fileList))
                    {
                        fileList = new List<string>();
                        tokenMap[key] = fileList;
                    }
                    if (!fileList.Contains(fileName, StringComparer.OrdinalIgnoreCase))
                        fileList.Add(fileName);
                }
            }
        }

        return tokenMap
            .Where(kvp => kvp.Value.Count > 1)
            .Select(kvp =>
            {
                var parts = kvp.Key.Split(new[] { ':' }, 2);
                return new DuplicateEntry
                {
                    Language  = parts.Length > 1 ? parts[0] : "",
                    TokenName = parts.Length > 1 ? parts[1] : kvp.Key,
                    Files     = kvp.Value,
                    Count     = kvp.Value.Count
                };
            })
            .OrderByDescending(d => d.Count)
            .ToList();
    }

    public bool HasDuplicates(string tokenName, string directory)
    {
        return FindDuplicateTokens(directory)
            .Any(d => d.TokenName.Equals(tokenName, StringComparison.OrdinalIgnoreCase));
    }
}

public class DuplicateEntry
{
    public string TokenName { get; set; }
    public string Language  { get; set; }
    public List<string> Files { get; set; }
    public int Count { get; set; }
}
