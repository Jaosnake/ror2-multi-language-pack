using System;
using System.Collections.Generic;
using System.IO;
using Path = System.IO.Path;
using System.Linq;
using System.Text;
using BepInEx;
using Newtonsoft.Json.Linq;
using RoR2;

namespace R2API;

public static partial class LanguageAPI
{
    private static HarmonyLib.Harmony _officialHook;
    private static System.Reflection.MethodInfo _addOfficialOverlay;
    private static object _peleJsonOverlay;
    private static Dictionary<string, Dictionary<string, string>> _officialTokens;
    private static Dictionary<string, Dictionary<string, string>> _officialOverlays;

    internal static void SetHooks()
    {
        if (_officialHook != null) return;
        var plugin = BepInEx.Bootstrap.Chainloader.PluginInfos["com.bepis.r2api.language"];
        var api = plugin.Instance.GetType().Assembly.GetType("R2API.LanguageAPI", true);
        var lookup = HarmonyLib.AccessTools.Method(api, "Language_GetLocalizedStringByToken")
            ?? throw new MissingMethodException(api.FullName, "Language_GetLocalizedStringByToken");
        var registered = HarmonyLib.AccessTools.Method(api, "Language_TokenIsRegistered")
            ?? throw new MissingMethodException(api.FullName, "Language_TokenIsRegistered");
        _addOfficialOverlay = HarmonyLib.AccessTools.Method(api, "AddOverlay",
            new[] { typeof(Dictionary<string, Dictionary<string, string>>) })
            ?? throw new MissingMethodException(api.FullName, "AddOverlay");
        _officialTokens = ReadOfficialTokens(api, "CustomLanguage");
        _officialOverlays = ReadOfficialTokens(api, "OverlayLanguage");

        // Patch inside the official handler so outer mod hooks can format the returned value.
        var bridge = new HarmonyLib.Harmony("jaosnake.pele.officiallanguage");
        try
        {
            bridge.Patch(lookup, prefix: new HarmonyLib.HarmonyMethod(typeof(LanguageAPI), nameof(ResolvePeleToken)));
            bridge.Patch(registered, prefix: new HarmonyLib.HarmonyMethod(typeof(LanguageAPI), nameof(IsPeleTokenRegistered)));
            _officialHook = bridge;
            Log?.LogInfo("P.E.L.E. tokens integrated inside the official R2API.Language handlers.");
        }
        catch
        {
            bridge.UnpatchSelf();
            throw;
        }
    }

    private static Dictionary<string, Dictionary<string, string>> ReadOfficialTokens(Type api, string fieldName)
    {
        return (Dictionary<string, Dictionary<string, string>>)
            (HarmonyLib.AccessTools.Field(api, fieldName)?.GetValue(null)
                ?? throw new MissingFieldException(api.FullName, fieldName));
    }

    internal static void RegisterPeleJsonTokens(Dictionary<string, Dictionary<string, string>> tokens)
    {
        SetHooks();
        var replacement = _addOfficialOverlay.Invoke(null, new object[] { tokens });
        RemovePeleJsonOverlay();
        _peleJsonOverlay = replacement;
    }

    private static void RemovePeleJsonOverlay()
    {
        if (_peleJsonOverlay == null) return;
        _peleJsonOverlay.GetType().GetMethod("Remove").Invoke(_peleJsonOverlay, null);
        _peleJsonOverlay = null;
    }

    private static bool TryGetToken(Dictionary<string, Dictionary<string, string>> dict, string lang, string token, out string value)
    {
        _rwLock.EnterReadLock();
        try
        {
            if (dict.TryGetValue(lang, out var langDict) && langDict.TryGetValue(token, out value))
                return true;
            var alias = PelePaths.LanguageAlias(lang);
            if (alias != null && dict.TryGetValue(alias, out var aliasDict) && aliasDict.TryGetValue(token, out value))
                return true;
            if (dict.TryGetValue(GenericLanguage, out var genericDict) && genericDict.TryGetValue(token, out value))
                return true;
            value = null;
            return false;
        }
        finally
        {
            _rwLock.ExitReadLock();
        }
    }

    private static bool ResolvePeleToken(Language __1, string __2, ref string __result)
    {
        if (!TryResolvePeleToken(__1.name, __2, out var value)) return true;
        __result = value;
        return false;
    }

    private static bool IsPeleTokenRegistered(Language __1, string __2, ref bool __result)
    {
        if (!TryResolvePeleToken(__1.name, __2, out _)) return true;
        __result = true;
        return false;
    }

    private static bool TryResolvePeleToken(string language, string token, out string value)
    {
        if (TryGetToken(OverlayLanguage, language, token, out value)) return true;
        var alias = PelePaths.LanguageAlias(language);
        if (alias != null && TryGetOfficialAlias(_officialOverlays, language, alias, token, out value))
            return true;
        if (TryGetExactOrGenericOfficial(_officialOverlays, language, token)) return false;
        if (alias != null && TryGetOfficialAlias(_officialTokens, language, alias, token, out value))
            return true;
        return TryGetToken(CustomLanguage, language, token, out value);
    }

    private static bool TryGetOfficialAlias(Dictionary<string, Dictionary<string, string>> source,
        string language, string alias, string token, out string value)
    {
        value = null;
        if (source.TryGetValue(language, out var direct) && direct.ContainsKey(token)) return false;
        return source.TryGetValue(alias, out var fallback) && fallback.TryGetValue(token, out value);
    }

    private static bool TryGetExactOrGenericOfficial(Dictionary<string, Dictionary<string, string>> source,
        string language, string token)
    {
        return (source.TryGetValue(language, out var direct) && direct.ContainsKey(token))
            || (source.TryGetValue(GenericLanguage, out var generic) && generic.ContainsKey(token));
    }

    internal static string DescribeToken(string token)
    {
        var language = Language.currentLanguageName;
        var resolved = Language.GetString(token);
        var description = new StringBuilder($"[P.E.L.E.] {token} | selecionado: {language}\nTexto em jogo: {resolved}");
        var codes = new[] { language, PelePaths.LanguageAlias(language), GenericLanguage, "en" }
            .Where(code => code != null).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        if (TryResolvePeleToken(language, token, out var bridgeValue))
            description.AppendLine().Append("Texto retornado pela integração do P.E.L.E.: ").Append(bridgeValue);

        foreach (var code in codes)
        {
            string[] files = PeleJsonLoader.FindSources(code, token).ToArray();
            for (int i = 0; i < files.Length; i++)
                description.AppendLine().Append($"JSON {code} {(i == 0 ? "(vencedor entre os JSONs desse idioma)" : "(duplicado)")}: {files[i]}");
            if (_officialOverlays.TryGetValue(code, out var overlay) && overlay.TryGetValue(token, out var overlayValue))
                description.AppendLine().Append($"Overlay R2API {code}: {overlayValue}");
            if (_officialTokens.TryGetValue(code, out var tokens) && tokens.TryGetValue(token, out var officialValue))
                description.AppendLine().Append($"Token registrado no R2API {code}: {officialValue}");
        }

        // The official dictionaries do not retain filenames. Report disk candidates as such.
        try
        {
            foreach (var path in Directory.EnumerateFiles(Paths.PluginPath, "*.language", SearchOption.AllDirectories))
            {
                try
                {
                    var file = JObject.Parse(File.ReadAllText(path));
                    foreach (var entry in file.Properties())
                    {
                        var code = entry.Name == "strings" ? GenericLanguage : entry.Name;
                        if (codes.Contains(code, StringComparer.OrdinalIgnoreCase) &&
                            entry.Value is JObject entries && entries.TryGetValue(token, out var value))
                            description.AppendLine().Append($"Candidato .language {code}: {Path.GetRelativePath(Paths.PluginPath, path)} → {value}");
                    }
                }
                catch (Exception error) when (error is IOException or UnauthorizedAccessException or Newtonsoft.Json.JsonException)
                {
                    // Other mods can use a non-JSON format; the live value above still applies.
                }
            }
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            description.AppendLine().Append("Leitura de candidatos no disco incompleta.");
        }
        return description.ToString();
    }

}
