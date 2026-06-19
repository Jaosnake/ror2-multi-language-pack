using System;
using System.IO;
using UnityEngine;

namespace R2API;

public class ModInstallationWizard
{
    public void CreateModStructure(string modPath)
    {
        if (string.IsNullOrWhiteSpace(modPath))
            throw new ArgumentException("Caminho invalido", nameof(modPath));

        Directory.CreateDirectory(modPath);

        foreach (var dir in new[] { "Plugins", "Language", "Assets" })
            Directory.CreateDirectory(Path.Combine(modPath, dir));

        Debug.Log($"[LanguageAPI] Estrutura criada em: {modPath}");
    }

    public void SetupLanguageFiles(string modPath)
    {
        var langDir = Path.Combine(modPath, "Language");
        Directory.CreateDirectory(langDir);

        WriteIfAbsent(Path.Combine(langDir, "en.language"),
            "EXAMPLE_TOKEN = \"Example text\"\n");

        WriteIfAbsent(Path.Combine(langDir, "pt-BR.language"),
            "EXAMPLE_TOKEN = \"Texto de exemplo\"\n");

        Debug.Log($"[LanguageAPI] Arquivos de idioma criados em: {langDir}");
    }

    public void GenerateBasicLanguageFile(string modName, string modPath)
    {
        if (string.IsNullOrWhiteSpace(modName))
            throw new ArgumentException("Nome do mod invalido", nameof(modName));

        var langDir  = Path.Combine(modPath, "Language");
        Directory.CreateDirectory(langDir);

        var safeName = modName.Replace(" ", "_").ToUpperInvariant();
        var filePath = Path.Combine(langDir, $"{modName.Replace(" ", "_")}.language");

        File.WriteAllText(filePath,
$@"// {modName}
MOD_{safeName}_NAME = ""{modName}""
MOD_{safeName}_DESCRIPTION = ""Descricao do {modName}""
");

        Debug.Log($"[LanguageAPI] Arquivo gerado: {filePath}");
    }

    private static void WriteIfAbsent(string path, string content)
    {
        if (!File.Exists(path))
            File.WriteAllText(path, content);
    }
}
