using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace R2API;

public class LanguageFileValidator
{
    private static readonly Regex TokenPattern   = new(@"^[A-Za-z0-9_.]+\s*=", RegexOptions.Compiled);
    private static readonly Regex CommentPattern = new(@"^\s*//",               RegexOptions.Compiled);
    private static readonly Regex TokenNameRegex = new(@"^[A-Za-z0-9_.]+$",    RegexOptions.Compiled);

    public ValidationResult ValidateFile(string filePath)
    {
        if (!File.Exists(filePath))
            return ValidationResult.Error($"Arquivo nao encontrado: {filePath}");

        var content = File.ReadAllLines(filePath);
        var errors   = new List<string>();
        var warnings = new List<string>();

        for (int i = 0; i < content.Length; i++)
        {
            var line       = content[i];
            var lineNumber = i + 1;

            if (string.IsNullOrWhiteSpace(line)) continue;
            if (CommentPattern.IsMatch(line))    continue;

            // Allow JSON opening/closing braces for multi-language JSON files.
            var trimmed = line.Trim();
            if (trimmed == "{" || trimmed == "}") continue;

            if (!TokenPattern.IsMatch(trimmed))
                errors.Add($"Linha {lineNumber}: Sintaxe invalida: '{line}'");

            if (HasPotentialIssues(line))
                warnings.Add($"Linha {lineNumber}: Possivel problema de espacos: '{line}'");
        }

        return new ValidationResult
        {
            IsValid  = errors.Count == 0,
            Errors   = errors,
            Warnings = warnings,
            FilePath = filePath
        };
    }

    public bool ValidateTokenSyntax(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return false;
        var eqIdx = token.IndexOf('=');
        if (eqIdx < 1) return false;
        var tokenName  = token.Substring(0, eqIdx).Trim();
        var tokenValue = token.Substring(eqIdx + 1).Trim();
        return !string.IsNullOrWhiteSpace(tokenName)
            && !string.IsNullOrWhiteSpace(tokenValue)
            && IsValidTokenName(tokenName);
    }

    private static bool IsValidTokenName(string tokenName)
        => TokenNameRegex.IsMatch(tokenName) && !tokenName.StartsWith("//");

    private static bool HasPotentialIssues(string line)
        => line.Contains("  ") || line.StartsWith(" ") || line.EndsWith(" ");
}

public class ValidationResult
{
    public bool IsValid    { get; set; }
    public List<string> Errors   { get; set; } = new List<string>();
    public List<string> Warnings { get; set; } = new List<string>();
    public string FilePath { get; set; }

    public static ValidationResult Error(string message)
        => new ValidationResult { IsValid = false, Errors = new List<string> { message } };
}
