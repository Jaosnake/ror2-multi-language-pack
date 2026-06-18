using System;
using System.IO;
using UnityEngine;

namespace R2API;

public class ModInstallationWizard
{
    public void CreateModStructure(string modPath)
    {
        if (string.IsNullOrWhiteSpace(modPath))
            throw new ArgumentException("Caminho inválido");

        Directory.CreateDirectory(modPath);

        foreach (var dir in new[] { "Plugins", "Language", "Assets" })
            Directory.CreateDirectory(Path.Combine(modPath, dir));

        Debug.Log($"[LanguageAPI] Estrutura criada em: {modPath}");
    }

    public void SetupLanguageFiles(string modPath)
    {
        var langDir = Path.Combine(modPath, "Language");
        Directory.CreateDirectory(langDir);

        var enPath = Path.Combine(langDir, "en-US.language");
        if (!File.Exists(enPath))
            File.WriteAllText(enPath, "EXAMPLE_TOKEN = \"Example text\"\n");

        var ptPath = Path.Combine(langDir, "pt-BR.language");
        if (!File.Exists(ptPath))
            File.WriteAllText(ptPath, "EXAMPLE_TOKEN = \"Texto de exemplo\"\n");

        Debug.Log($"[LanguageAPI] Arquivos de idioma criados em: {langDir}");
    }

    public void GenerateBasicLanguageFile(string modName, string modPath)
    {
        var langDir = Path.Combine(modPath, "Language");
        Directory.CreateDirectory(langDir);

        var safeName = modName.Replace(" ", "_").ToUpperInvariant();
        var filePath = Path.Combine(langDir, $"{modName.Replace(" ", "_")}.language");

        File.WriteAllText(filePath,
$@"// {modName}
MOD_{safeName}_NAME = ""{modName}""
MOD_{safeName}_DESCRIPTION = ""Descrição do {modName}""
");

        Debug.Log($"[LanguageAPI] Arquivo gerado: {filePath}");
    }
}
