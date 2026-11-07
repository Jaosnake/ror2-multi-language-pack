using System;
using System.Text.RegularExpressions;

namespace PELE.KingDBZFix
{
    internal static class TutorialMatcher
    {
        internal static string FindToken(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            string visible = Regex.Replace(Regex.Replace(text, @"<[^>]*>", ""), @"\s+", " ").Trim();
            if (visible.Contains("Enable/Disable Tutorials")) return "KINGMOD_TUTORIAL_F1";
            if (visible.Contains("With Form Mastery")) return "KINGMOD_TUTORIAL_FORM_MASTERY";
            if (visible.Contains("Once your mastery level is high enough")) return "KINGMOD_TUTORIAL_TRANSFORM_TRIGGER";
            if (visible.Contains("While transformed") && visible.Contains("Ultimate Skill")) return "KINGMOD_TUTORIAL_ULTIMATE_SKILL";
            if (visible.Contains("Ki is a resource that your skills use")) return "KINGMOD_TUTORIAL_KI_RESOURCE";
            if (visible.Contains("Gain Zenkai")) return "KINGMOD_TUTORIAL_ZENKAI";
            if (visible.Contains("Gain Light Charge")) return "KINGMOD_TUTORIAL_LIGHT_CHARGE";
            if (visible.Contains("Unlock Kaioken forms") || visible.Contains("Kaioken Meter")) return "KINGMOD_TUTORIAL_KAIOKEN";
            return null;
        }
    }
}
