using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using RoR2;

[BepInPlugin("jaosnake.starstorm2.languagepilot", "Starstorm 2 Pilot Formatting", "1.0.0")]
[BepInDependency("com.TeamMoonstorm.MSU")]
public sealed class StarstormPilotFormatting : BaseUnityPlugin
{
    private static readonly string[] Descriptions = {
        "SS2_ITEM_COFFEEBAG_DESC",
        "SS2_ITEM_GUARDINGAMULET_DESC",
        "SS2_ITEM_RELICOFMASS_DESC"
    };
    private static Dictionary<string, Dictionary<string, string>> templates;
    private static FieldInfo attributes;
    private static MethodInfo format;
    private static MethodInfo setString;
    private static BepInEx.Logging.ManualLogSource log;

    private void Awake()
    {
        log = Logger;
        try
        {
            var msu = BepInEx.Bootstrap.Chainloader.PluginInfos["com.TeamMoonstorm.MSU"].Instance.GetType().Assembly;
            var loader = msu.GetType("MSU.LanguageFileLoader", true);
            var manager = msu.GetType("MSU.FormatTokenManager", true);
            templates = AccessTools.Field(loader, "_languageNameToRawTokenData").GetValue(null)
                as Dictionary<string, Dictionary<string, string>>;
            attributes = AccessTools.Field(manager, "_cachedFormattingArray");
            format = AccessTools.Method(manager, "FormatString");
            setString = AccessTools.Method(typeof(Language), "SetStringByToken");
            var register = AccessTools.Method(manager, "AddTokensFromLanguageFileLoaderAndFormatThem");
            if (templates == null || attributes == null || format == null || setString == null || register == null)
                throw new MissingMemberException("MSU formatting integration");
            new Harmony(Info.Metadata.GUID).Patch(register,
                postfix: new HarmonyMethod(typeof(StarstormPilotFormatting), nameof(EnsurePilotDescriptions)));
        }
        catch (Exception exception)
        {
            Logger.LogWarning("Pilot descriptions could not be integrated with MSU: " + exception.Message);
        }
    }

    private static void EnsurePilotDescriptions(Language __0)
    {
        var code = __0.name;
        if ((code != "vi" && code != "el") || !templates.TryGetValue(code, out var strings)
            || !(attributes.GetValue(null) is IDictionary formatting))
            return;

        // Use the current configuration values from MSU even if its queued formatting
        // pass has not run. Only the three translated pilot descriptions are affected.
        foreach (var token in Descriptions)
        {
            if (!strings.TryGetValue(token, out var template) || !formatting.Contains(token))
                continue;

            try
            {
                var localized = (string)format.Invoke(null, new[] { token, template, formatting[token] });
                setString.Invoke(__0, new object[] { token, localized });
            }
            catch (Exception exception)
            {
                log.LogWarning($"Could not format {code}/{token}: {exception.GetBaseException().Message}");
            }
        }
    }
}
