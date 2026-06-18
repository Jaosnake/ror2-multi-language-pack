using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace R2API;

public class LanguageFileCompiler
{
    public void CompileLanguageFiles(string sourceDir, string outputDir)
    {
        if (!Directory.Exists(sourceDir))
            throw new DirectoryNotFoundException($"Origem não encontrada: {sourceDir}");

        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        var files = LanguageFileHelper.GetLanguageFiles(sourceDir);
        var errors = new List<string>();

        foreach (var file in files)
        {
            try
            {
                var compiled = CompileFile(file);
                var relPath = Path.GetRelativePath(sourceDir, file);
                var outPath = Path.Combine(outputDir, relPath + ".compiled");
                var outDir = Path.GetDirectoryName(outPath);

                if (!Directory.Exists(outDir))
                    Directory.CreateDirectory(outDir);

                File.WriteAllText(outPath, compiled, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                errors.Add($"{Path.GetFileName(file)}: {ex.Message}");
            }
        }

        if (errors.Any())
            throw new AggregateException("Erros na compilação:", errors.Select(e => new Exception(e)));
    }

    private static string CompileFile(string filePath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// COMPILED");
        sb.AppendLine($"// {Path.GetFileName(filePath)}");
        sb.AppendLine();

        var tokens = LanguageFileHelper.ParseTokensFromFile(filePath);
        foreach (var kvp in tokens)
            sb.AppendLine($"{kvp.Key}={kvp.Value}");

        return sb.ToString().TrimEnd();
    }
}
