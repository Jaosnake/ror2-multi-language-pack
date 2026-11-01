using System;
using System.IO;

namespace R2API;

internal static class PelePaths
{
    internal static string LanguageRoot(string plugins)
    {
        var direct = Path.Combine(plugins, "PELE", "Language");
        var managed = Path.Combine(plugins, "Jaosnake-PELE", "PELE", "Language");
        return Directory.Exists(direct) || !Directory.Exists(managed) ? direct : managed;
    }

    internal static string CyrillicFont(string plugins)
    {
        var direct = Path.Combine(plugins, "PELE", "Fonts", "cyrillicfont");
        var managed = Path.Combine(plugins, "Jaosnake-PELE", "PELE", "Fonts", "cyrillicfont");
        return File.Exists(direct) || !File.Exists(managed) ? direct : managed;
    }

    internal static string LanguageAlias(string language)
    {
        if (string.Equals(language, "uk", StringComparison.OrdinalIgnoreCase)) return "UA";
        if (string.Equals(language, "UA", StringComparison.OrdinalIgnoreCase)) return "uk";
        return null;
    }
}
