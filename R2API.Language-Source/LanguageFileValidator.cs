using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace R2API;

public class LanguageFileValidator
{
    private const string TOKEN_PATTERN = @"^[A-Za-z0-9_.]+=";
    private const string COMMENT_PATTERN = "^//.*";

    public ValidationResult ValidateFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return ValidationResult.Error($"Arquivo não encontrado: {filePath}");
        }

        var content = File.ReadAllLines(filePath);
        var errors = new List<string>();
        var warnings = new List<string>();

        for (int i = 0; i < content.Length; i++)
        {
            var line = content[i];
            var lineNumber = i + 1;

            if (string.IsNullOrWhiteSpace(line)) continue;
            if (IsCommentLine(line)) continue;
            if (!IsValidTokenLine(line))
            {
                errors.Add($"Linha {lineNumber}: Sintaxe inválida: '{line}'");
            }
            if (HasPotentialIssues(line))
            {
                warnings.Add($"Linha {lineNumber}: Possível problema: '{line}'");
            }
        }

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Warnings = warnings,
            FilePath = filePath
        };
    }

    public bool ValidateTokenSyntax(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return false;
        var parts = token.Split('=');
        if (parts.Length != 2) return false;
        var tokenName = parts[0].Trim();
        var tokenValue = parts[1].Trim();
        if (string.IsNullOrWhiteSpace(tokenName)) return false;
        if (!IsValidTokenName(tokenName)) return false;
        if (string.IsNullOrWhiteSpace(tokenValue)) return false;
        return true;
    }

    private bool IsCommentLine(string line) => Regex.IsMatch(line, COMMENT_PATTERN);
    private bool IsValidTokenLine(string line) => Regex.IsMatch(line, TOKEN_PATTERN);
    private bool IsValidTokenName(string tokenName) => Regex.IsMatch(tokenName, "^[A-Za-z0-9_.]+$") && !tokenName.StartsWith("//");

    private bool HasPotentialIssues(string line)
    {
        if (line.Contains("  ")) return true;
        if (line.StartsWith(" ") || line.EndsWith(" ")) return true;
        return false;
    }
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
    public List<string> Warnings { get; set; } = new List<string>();
    public string FilePath { get; set; }

    public static ValidationResult Error(string message)
    {
        return new ValidationResult { IsValid = false, Errors = new List<string> { message } };
    }
}
