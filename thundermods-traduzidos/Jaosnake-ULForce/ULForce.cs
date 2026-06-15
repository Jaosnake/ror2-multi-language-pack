using BepInEx;
using BepInEx.Configuration;
using R2API;
using RiskOfOptions;
using RiskOfOptions.Options;
using RoR2;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ULForce
{
    public enum LanguageChoice
    {
        None = 0,
        UA = 1,
        pl = 2,
        es_MX = 3,
        mx = 4,
        es_latam = 5,
        Esperanto = 6
    }

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency("com.bepis.r2api.language", "1.1.0")]
    [BepInDependency("com.rune580.riskofoptions", "2.8.5")]
    public class ULForcePlugin : BaseUnityPlugin
    {
        public const string PluginGUID = "jaosnake.ulforce";
        public const string PluginName = "ULForce";
        public const string PluginVersion = "1.0.0";

        private static readonly FieldInfo FallbackLanguageField = typeof(Language).GetField("fallbackLanguage", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo CustomLanguageField = typeof(LanguageAPI).GetField("CustomLanguage", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly Regex LanguageObjectRegex = new Regex("\"([A-Za-z]{2}(?:-[A-Za-z0-9]{2,8})?)\"\\s*:", RegexOptions.Compiled);

        private static readonly HashSet<string> OfficialLanguages = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "en",
            "pt-BR",
            "pt-br",
            "es",
            "es-ES",
            "es-419",
            "de",
            "fr",
            "it",
            "ja",
            "jp",
            "ko",
            "ru",
            "tr",
            "zh-CN",
            "zh-cn",
            "zh-TW",
            "zh-tw"
        };

        private ConfigEntry<bool> enablePlugin;
        private ConfigEntry<LanguageChoice> selectedLanguage;
        private ConfigEntry<string> fallbackLanguageCode;
        private string previousLanguageName;
        private HashSet<string> installedCustomLanguages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private void Awake()
        {
            enablePlugin = Config.Bind(
                "General",
                "Enable",
                true,
                "Turn ULForce on or off.");

            selectedLanguage = Config.Bind(
                "General",
                "SelectedLanguage",
                LanguageChoice.UA,
                "Custom language pack to force. Official game languages are intentionally not listed here.");

            fallbackLanguageCode = Config.Bind(
                "General",
                "FallbackLanguageCode",
                "en",
                "Language used when ULForce is turned off or a custom token is missing.");

            RefreshInstalledLanguages();
            RegisterRiskOfOptions();

            RoR2Application.onLoad += TryApplyConfiguredLanguage;
            RoR2Application.onLoadFinished += TryApplyConfiguredLanguage;
        }

        private void OnDestroy()
        {
            RoR2Application.onLoad -= TryApplyConfiguredLanguage;
            RoR2Application.onLoadFinished -= TryApplyConfiguredLanguage;
        }

        private void RegisterRiskOfOptions()
        {
            ModSettingsManager.SetModDescription(
                "Forces installed Jaosnake custom R2API.Language packs while keeping untranslated base-game text in English. Official Risk of Rain 2 languages should still be selected from the normal game language menu.");
            TryRegisterRiskOfOptionsIcon();

            ModSettingsManager.AddOption(new CheckBoxOption(enablePlugin));
            ModSettingsManager.AddOption(new ChoiceOption(selectedLanguage));
            ModSettingsManager.AddOption(new StringInputFieldOption(fallbackLanguageCode));
            ModSettingsManager.AddOption(new GenericButtonOption(
                "Refresh languages",
                "General",
                "Scans installed Jaosnake language packs again and logs which custom language codes were found.",
                "Refresh",
                RefreshInstalledLanguages));
            ModSettingsManager.AddOption(new GenericButtonOption(
                "Apply now",
                "General",
                "Applies the selected custom language immediately. Restart the game if a mod only loads language files during startup.",
                "Apply",
                () => ApplyCurrentConfig("Risk Of Options button")));
        }

        private void TryRegisterRiskOfOptionsIcon()
        {
            try
            {
                var pluginDirectory = System.IO.Path.GetDirectoryName(Info.Location);
                if (string.IsNullOrEmpty(pluginDirectory))
                {
                    return;
                }

                var iconPath = System.IO.Path.Combine(pluginDirectory, "icon.png");
                if (!File.Exists(iconPath))
                {
                    Logger.LogInfo("Risk Of Options icon not found at " + iconPath + ".");
                    return;
                }

                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!ImageConversion.LoadImage(texture, File.ReadAllBytes(iconPath)))
                {
                    Logger.LogWarning("Could not load Risk Of Options icon from " + iconPath + ".");
                    return;
                }

                texture.wrapMode = TextureWrapMode.Clamp;
                var sprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f);

                ModSettingsManager.SetModIcon(sprite);
            }
            catch (Exception ex)
            {
                Logger.LogWarning("Failed to register Risk Of Options icon: " + ex.Message);
            }
        }

        private void TryApplyConfiguredLanguage()
        {
            ApplyCurrentConfig("game load");
        }

        private void ApplyCurrentConfig(string reason)
        {
            RefreshInstalledLanguages();

            if (!enablePlugin.Value)
            {
                RestoreLanguage(reason);
                return;
            }

            var sourceLanguage = GetLanguageCode(selectedLanguage.Value);
            if (string.IsNullOrWhiteSpace(sourceLanguage))
            {
                Logger.LogWarning("No custom language selected. Install a Jaosnake custom language pack, choose it in ULForce, then press Apply.");
                return;
            }

            if (!IsInstalledCustomLanguage(sourceLanguage))
            {
                Logger.LogWarning("Selected language '" + sourceLanguage + "' was not found in installed Jaosnake language packs. Press Refresh or install a matching pack.");
                return;
            }

            try
            {
                ApplyLanguageBridge(sourceLanguage);
                Logger.LogInfo("Current language set using custom language '" + sourceLanguage + "' with English fallback (" + reason + ").");
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to set custom language '" + sourceLanguage + "': " + ex);
            }
        }

        private void RestoreLanguage(string reason)
        {
            var restoreLanguage = string.IsNullOrWhiteSpace(previousLanguageName)
                ? fallbackLanguageCode.Value.Trim()
                : previousLanguageName;

            if (string.IsNullOrWhiteSpace(restoreLanguage))
            {
                restoreLanguage = "en";
            }

            if (string.Equals(Language.currentLanguageName, restoreLanguage, StringComparison.OrdinalIgnoreCase))
            {
                Logger.LogInfo("ULForce is disabled. Current language is already " + restoreLanguage + ".");
                return;
            }

            if (Language.FindLanguageByName(restoreLanguage) == null)
            {
                Logger.LogWarning("ULForce is disabled, but fallback language '" + restoreLanguage + "' was not found. Restart the game or change FallbackLanguageCode.");
                return;
            }

            SetLanguage(restoreLanguage);
            Logger.LogInfo("ULForce is disabled. Restored language to " + restoreLanguage + " (" + reason + ").");
        }

        private void RefreshInstalledLanguages()
        {
            installedCustomLanguages = DiscoverInstalledCustomLanguages();
            if (installedCustomLanguages.Count == 0)
            {
                Logger.LogWarning("No installed Jaosnake custom languages found for ULForce.");
                return;
            }

            Logger.LogInfo("ULForce detected custom languages: " + string.Join(", ", installedCustomLanguages));
        }

        private HashSet<string> DiscoverInstalledCustomLanguages()
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var pluginDirectory = System.IO.Path.GetDirectoryName(Info.Location);
            var pluginsRoot = Directory.GetParent(pluginDirectory ?? string.Empty);
            if (pluginsRoot == null || !pluginsRoot.Exists)
            {
                return result;
            }

            foreach (var directory in pluginsRoot.GetDirectories("Jaosnake-*LanguagePack"))
            {
                foreach (var file in directory.GetFiles("*.*", SearchOption.AllDirectories))
                {
                    if (!IsLanguageFile(file))
                    {
                        continue;
                    }

                    foreach (var code in ReadLanguageCodes(file))
                    {
                        if (!OfficialLanguages.Contains(code))
                        {
                            result.Add(NormalizeLanguageCode(code));
                        }
                    }
                }
            }

            return result;
        }

        private static bool IsLanguageFile(FileInfo file)
        {
            var extension = file.Extension.ToLowerInvariant();
            return extension == ".language" || extension == ".json" || extension == ".txt";
        }

        private static IEnumerable<string> ReadLanguageCodes(FileInfo file)
        {
            string text;
            try
            {
                text = File.ReadAllText(file.FullName);
            }
            catch
            {
                yield break;
            }

            foreach (Match match in LanguageObjectRegex.Matches(text))
            {
                var code = match.Groups[1].Value;
                if (!string.IsNullOrWhiteSpace(code))
                {
                    yield return code;
                }
            }
        }

        private static string NormalizeLanguageCode(string code)
        {
            if (string.Equals(code, "ua", StringComparison.OrdinalIgnoreCase))
            {
                return "UA";
            }

            return code;
        }

        private bool IsInstalledCustomLanguage(string sourceLanguage)
        {
            if (installedCustomLanguages.Contains(sourceLanguage))
            {
                return true;
            }

            if (string.Equals(sourceLanguage, "UA", StringComparison.OrdinalIgnoreCase))
            {
                return installedCustomLanguages.Contains("ua");
            }

            return false;
        }

        private static string GetLanguageCode(LanguageChoice choice)
        {
            switch (choice)
            {
                case LanguageChoice.UA:
                    return "UA";
                case LanguageChoice.pl:
                    return "pl";
                case LanguageChoice.es_MX:
                    return "es-MX";
                case LanguageChoice.mx:
                    return "mx";
                case LanguageChoice.es_latam:
                    return "es-latam";
                case LanguageChoice.Esperanto:
                    return "eo";
                default:
                    return string.Empty;
            }
        }

        private void ApplyLanguageBridge(string sourceLanguage)
        {
            var english = Language.FindLanguageByName("en") ?? Language.english;
            var source = EnsureLanguageExists(sourceLanguage);

            SetFallbackLanguage(source, english);

            if (string.Equals(sourceLanguage, "UA", StringComparison.OrdinalIgnoreCase))
            {
                var lowerUa = Language.FindLanguageByName("ua");
                if (lowerUa != null)
                {
                    SetFallbackLanguage(source, lowerUa);
                    SetFallbackLanguage(lowerUa, english);
                }

                BridgeAndSet("uk-UA", sourceLanguage);
                return;
            }

            var runtimeLanguage = GetRuntimeLanguageCode(sourceLanguage);
            if (string.Equals(runtimeLanguage, sourceLanguage, StringComparison.OrdinalIgnoreCase))
            {
                SetLanguage(runtimeLanguage);
                return;
            }

            var runtime = EnsureLanguageExists(runtimeLanguage);
            SetFallbackLanguage(runtime, source);
            MirrorCustomLanguageTokens(sourceLanguage, runtimeLanguage);
            SetLanguage(runtimeLanguage);
            Logger.LogInfo("Linked " + runtimeLanguage + " -> " + sourceLanguage + " -> en.");
        }

        private void BridgeAndSet(string runtimeLanguage, string sourceLanguage)
        {
            var source = EnsureLanguageExists(sourceLanguage);
            var runtime = EnsureLanguageExists(runtimeLanguage);
            SetFallbackLanguage(runtime, source);
            MirrorCustomLanguageTokens(sourceLanguage, runtimeLanguage);
            SetLanguage(runtimeLanguage);
            Logger.LogInfo("Linked " + runtimeLanguage + " -> " + sourceLanguage + " -> en.");
        }

        private void MirrorCustomLanguageTokens(string sourceLanguage, string runtimeLanguage)
        {
            if (string.Equals(sourceLanguage, runtimeLanguage, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var tokens = CollectCustomLanguageTokens(sourceLanguage);
            if (tokens.Count == 0)
            {
                Logger.LogWarning("No R2API custom tokens found for '" + sourceLanguage + "' to mirror into '" + runtimeLanguage + "'.");
                return;
            }

            LanguageAPI.Add(tokens, runtimeLanguage);
            Logger.LogInfo("Mirrored " + tokens.Count + " custom token(s) from " + sourceLanguage + " to runtime language " + runtimeLanguage + ".");
        }

        private Dictionary<string, string> CollectCustomLanguageTokens(string sourceLanguage)
        {
            var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            AddTokensFromR2ApiCustomLanguage(tokens, sourceLanguage);
            AddTokensFromInstalledLanguageFiles(tokens, sourceLanguage);
            return tokens;
        }

        private void AddTokensFromR2ApiCustomLanguage(Dictionary<string, string> tokens, string sourceLanguage)
        {
            if (CustomLanguageField == null)
            {
                return;
            }

            var customLanguage = CustomLanguageField.GetValue(null) as Dictionary<string, Dictionary<string, string>>;
            if (customLanguage == null)
            {
                return;
            }

            foreach (var languageKey in GetEquivalentLanguageKeys(sourceLanguage))
            {
                Dictionary<string, string> languageTokens;
                if (!customLanguage.TryGetValue(languageKey, out languageTokens) || languageTokens == null)
                {
                    continue;
                }

                foreach (var token in languageTokens)
                {
                    tokens[token.Key] = token.Value;
                }
            }
        }

        private void AddTokensFromInstalledLanguageFiles(Dictionary<string, string> tokens, string sourceLanguage)
        {
            var pluginDirectory = System.IO.Path.GetDirectoryName(Info.Location);
            var pluginsRoot = Directory.GetParent(pluginDirectory ?? string.Empty);
            if (pluginsRoot == null || !pluginsRoot.Exists)
            {
                return;
            }

            foreach (var directory in pluginsRoot.GetDirectories("Jaosnake-*LanguagePack"))
            {
                foreach (var file in directory.GetFiles("*.*", SearchOption.AllDirectories))
                {
                    if (!IsLanguageFile(file))
                    {
                        continue;
                    }

                    foreach (var languageKey in GetEquivalentLanguageKeys(sourceLanguage))
                    {
                        AddTokensFromLanguageFile(tokens, file, languageKey);
                    }
                }
            }
        }

        private static void AddTokensFromLanguageFile(Dictionary<string, string> tokens, FileInfo file, string sourceLanguage)
        {
            string text;
            try
            {
                text = File.ReadAllText(file.FullName);
            }
            catch
            {
                return;
            }

            var languagePattern = new Regex("\"" + Regex.Escape(sourceLanguage) + "\"\\s*:\\s*\\{", RegexOptions.IgnoreCase);
            var match = languagePattern.Match(text);
            if (!match.Success)
            {
                return;
            }

            var objectStart = text.IndexOf('{', match.Index + match.Length - 1);
            if (objectStart < 0)
            {
                return;
            }

            var objectEnd = FindMatchingBrace(text, objectStart);
            if (objectEnd <= objectStart)
            {
                return;
            }

            var body = text.Substring(objectStart + 1, objectEnd - objectStart - 1);
            var tokenRegex = new Regex("\"((?:\\\\.|[^\"\\\\])+)\"\\s*:\\s*\"((?:\\\\.|[^\"\\\\])*)\"", RegexOptions.Singleline);
            foreach (Match tokenMatch in tokenRegex.Matches(body))
            {
                var key = Regex.Unescape(tokenMatch.Groups[1].Value);
                var value = Regex.Unescape(tokenMatch.Groups[2].Value);
                if (!string.IsNullOrWhiteSpace(key))
                {
                    tokens[key] = value;
                }
            }
        }

        private static int FindMatchingBrace(string text, int objectStart)
        {
            var depth = 0;
            var inString = false;
            var escaped = false;

            for (var i = objectStart; i < text.Length; i++)
            {
                var character = text[i];
                if (inString)
                {
                    if (escaped)
                    {
                        escaped = false;
                    }
                    else if (character == '\\')
                    {
                        escaped = true;
                    }
                    else if (character == '"')
                    {
                        inString = false;
                    }

                    continue;
                }

                if (character == '"')
                {
                    inString = true;
                    continue;
                }

                if (character == '{')
                {
                    depth++;
                }
                else if (character == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        private static IEnumerable<string> GetEquivalentLanguageKeys(string sourceLanguage)
        {
            yield return sourceLanguage;

            if (string.Equals(sourceLanguage, "UA", StringComparison.OrdinalIgnoreCase))
            {
                yield return "ua";
            }
            else if (string.Equals(sourceLanguage, "ua", StringComparison.OrdinalIgnoreCase))
            {
                yield return "UA";
            }
        }

        private static string GetRuntimeLanguageCode(string sourceLanguage)
        {
            if (IsValidCulture(sourceLanguage))
            {
                return sourceLanguage;
            }

            if (string.Equals(sourceLanguage, "es-latam", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(sourceLanguage, "mx", StringComparison.OrdinalIgnoreCase))
            {
                return "es-MX";
            }

            return "uk-UA";
        }

        private static bool IsValidCulture(string languageCode)
        {
            try
            {
                var culture = new CultureInfo(languageCode);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private Language EnsureLanguageExists(string targetLanguage)
        {
            var language = Language.FindLanguageByName(targetLanguage);
            if (language != null)
            {
                return language;
            }

            LanguageAPI.Add("ULFORCE_LANGUAGE_MARKER", string.Empty, targetLanguage);

            var getOrCreateLanguage = typeof(Language).GetMethod("GetOrCreateLanguage", BindingFlags.NonPublic | BindingFlags.Static);
            if (getOrCreateLanguage == null)
            {
                throw new MissingMethodException("Could not find RoR2.Language.GetOrCreateLanguage(string).");
            }

            language = (Language)getOrCreateLanguage.Invoke(null, new object[] { targetLanguage });
            Logger.LogInfo("Created custom language entry " + targetLanguage + ".");
            return language;
        }

        private void SetFallbackLanguage(Language language, Language fallback)
        {
            if (language == null || fallback == null || FallbackLanguageField == null)
            {
                return;
            }

            FallbackLanguageField.SetValue(language, fallback);
        }

        private void SetLanguage(string targetLanguage)
        {
            if (!string.Equals(Language.currentLanguageName, targetLanguage, StringComparison.OrdinalIgnoreCase))
            {
                previousLanguageName = Language.currentLanguageName;
            }

            var setCurrentLanguage = typeof(Language).GetMethod("SetCurrentLanguage", BindingFlags.NonPublic | BindingFlags.Static);
            if (setCurrentLanguage == null)
            {
                Logger.LogWarning("Could not find RoR2.Language.SetCurrentLanguage(string).");
                return;
            }

            setCurrentLanguage.Invoke(null, new object[] { targetLanguage });
        }
    }
}
