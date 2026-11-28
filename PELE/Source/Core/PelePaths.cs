using System;
using System.IO;

namespace R2API;

internal static class PelePaths
{
    internal static string LanguageRoot(string plugins)
    {
        var packaged = Path.Combine(PackageContentRoot(), "Language");
        if (Directory.Exists(packaged)) return packaged;

        var direct = Path.Combine(plugins, "PELE", "Language");
        var managed = Path.Combine(plugins, "Jaosnake-PELE", "PELE", "Language");
        return Directory.Exists(direct) || !Directory.Exists(managed) ? direct : managed;
    }

    internal static string CyrillicFont(string plugins)
    {
        var packaged = Path.Combine(PackageContentRoot(), "Fonts", "cyrillicfont");
        if (File.Exists(packaged)) return packaged;

        var direct = Path.Combine(plugins, "PELE", "Fonts", "cyrillicfont");
        var managed = Path.Combine(plugins, "Jaosnake-PELE", "PELE", "Fonts", "cyrillicfont");
        return File.Exists(direct) || !File.Exists(managed) ? direct : managed;
    }

    private static string PackageContentRoot()
    {
        // The core ships at <package>/RiskofThunder-R2API_Language/R2API.Language.
        // The manager may choose a different name for <package> during local import.
        var assemblyFolder = new FileInfo(typeof(PelePaths).Assembly.Location).Directory;
        return Path.Combine(assemblyFolder.Parent.Parent.FullName, "PELE");
    }

    internal static string LanguageAlias(string language)
    {
        if (string.Equals(language, "uk", StringComparison.OrdinalIgnoreCase)) return "UA";
        if (string.Equals(language, "UA", StringComparison.OrdinalIgnoreCase)) return "uk";
        return null;
    }
}
