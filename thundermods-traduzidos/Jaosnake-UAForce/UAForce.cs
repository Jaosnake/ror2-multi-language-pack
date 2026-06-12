using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.Options;
using R2API;
using RoR2;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ForceUkrainianLanguage
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency("com.bepis.r2api.language", "1.1.0")]
    [BepInDependency("com.rune580.riskofoptions", "2.8.5")]
    public class ForceUkrainianLanguagePlugin : BaseUnityPlugin
    {
        public const string PluginGUID = "jaosnake.uaforce";
        public const string PluginName = "UAForce";
        public const string PluginVersion = "1.0.0";

        private ConfigEntry<bool> enablePlugin;
        private ConfigEntry<string> languageCode;
        private ConfigEntry<bool> onlyWhenSystemLocaleIsUkrainian;
        private ConfigEntry<string> fallbackLanguageCode;
        private string previousLanguageName;
        private static readonly FieldInfo FallbackLanguageField = typeof(Language).GetField("fallbackLanguage", BindingFlags.NonPublic | BindingFlags.Instance);

        private void Awake()
        {
            enablePlugin = Config.Bind(
                "General",
                "Enable",
                true,
                "Turn Ukrainian language forcing on or off.");

            languageCode = Config.Bind(
                "General",
                "LanguageCode",
                "UA",
                "Language code used by the Ukrainian R2API.Language packs. UA is automatically applied through the valid game culture uk-UA.");

            onlyWhenSystemLocaleIsUkrainian = Config.Bind(
                "General",
                "OnlyWhenSystemLocaleIsUkrainian",
                false,
                "Only force Ukrainian when the operating system UI culture starts with 'uk'.");

            fallbackLanguageCode = Config.Bind(
                "General",
                "FallbackLanguageCode",
                "en",
                "Language code used when UAForce is turned off and the previous language is unknown.");

            RegisterRiskOfOptions();
            enablePlugin.SettingChanged += OnConfigChanged;
            languageCode.SettingChanged += OnConfigChanged;
            onlyWhenSystemLocaleIsUkrainian.SettingChanged += OnConfigChanged;
            fallbackLanguageCode.SettingChanged += OnConfigChanged;

            RoR2Application.onLoad += TrySetConfiguredLanguage;
            RoR2Application.onLoadFinished += TrySetConfiguredLanguage;
        }

        private void OnDestroy()
        {
            enablePlugin.SettingChanged -= OnConfigChanged;
            languageCode.SettingChanged -= OnConfigChanged;
            onlyWhenSystemLocaleIsUkrainian.SettingChanged -= OnConfigChanged;
            fallbackLanguageCode.SettingChanged -= OnConfigChanged;

            RoR2Application.onLoad -= TrySetConfiguredLanguage;
            RoR2Application.onLoadFinished -= TrySetConfiguredLanguage;
        }

        private void OnConfigChanged(object sender, EventArgs args)
        {
            ApplyCurrentConfig("config changed");
        }

        private void RegisterRiskOfOptions()
        {
            ModSettingsManager.SetModDescription(
                "Forces mod translations to Ukrainian while keeping untranslated base-game text in English. The game does not show Ukrainian in the vanilla language menu.");
            TryRegisterRiskOfOptionsIcon();

            ModSettingsManager.AddOption(new CheckBoxOption(enablePlugin));
            ModSettingsManager.AddOption(new StringInputFieldOption(languageCode));
            ModSettingsManager.AddOption(new CheckBoxOption(onlyWhenSystemLocaleIsUkrainian));
            ModSettingsManager.AddOption(new StringInputFieldOption(fallbackLanguageCode));
            ModSettingsManager.AddOption(new GenericButtonOption(
                "Apply now",
                "General",
                "Applies the current UAForce settings immediately. It uses Ukrainian mod tokens and English for missing base-game text.",
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

        private void TrySetConfiguredLanguage()
        {
            ApplyCurrentConfig("game load");
        }

        private void ApplyCurrentConfig(string reason)
        {
            if (!enablePlugin.Value)
            {
                RestoreLanguage(reason);
                return;
            }

            if (onlyWhenSystemLocaleIsUkrainian.Value && !CultureInfo.CurrentUICulture.Name.StartsWith("uk", StringComparison.OrdinalIgnoreCase))
            {
                Logger.LogInfo("System locale is not Ukrainian: " + CultureInfo.CurrentUICulture.Name);
                return;
            }

            var sourceLanguage = languageCode.Value.Trim();
            if (string.IsNullOrEmpty(sourceLanguage))
            {
                sourceLanguage = "UA";
            }

            try
            {
                var runtimeLanguage = GetRuntimeLanguageCode(sourceLanguage);
                EnsureLanguageBridge(sourceLanguage, runtimeLanguage);
                SetLanguage(runtimeLanguage);
                Logger.LogInfo("Current language set to " + runtimeLanguage + " using " + sourceLanguage + " mod tokens (" + reason + ").");
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to set Ukrainian language: " + ex);
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
                Logger.LogInfo("UAForce is disabled. Current language is already " + restoreLanguage + ".");
                return;
            }

            if (Language.FindLanguageByName(restoreLanguage) == null)
            {
                Logger.LogWarning("UAForce is disabled, but fallback language '" + restoreLanguage + "' was not found. Restart the game or change FallbackLanguageCode.");
                return;
            }

            SetLanguage(restoreLanguage);
            Logger.LogInfo("UAForce is disabled. Restored language to " + restoreLanguage + " (" + reason + ").");
        }

        private static string GetRuntimeLanguageCode(string sourceLanguage)
        {
            if (string.Equals(sourceLanguage, "UA", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(sourceLanguage, "ua", StringComparison.OrdinalIgnoreCase))
            {
                return "uk-UA";
            }

            return sourceLanguage;
        }

        private void EnsureLanguageBridge(string sourceLanguage, string runtimeLanguage)
        {
            var english = Language.FindLanguageByName("en") ?? Language.english;
            var source = EnsureLanguageExists(sourceLanguage);
            var runtime = EnsureLanguageExists(runtimeLanguage);

            if (!string.Equals(sourceLanguage, runtimeLanguage, StringComparison.OrdinalIgnoreCase))
            {
                SetFallbackLanguage(runtime, source);
                SetFallbackLanguage(source, english);
                LanguageAPI.Add("UAFORCE_LANGUAGE_MARKER", string.Empty, runtimeLanguage);
                Logger.LogInfo("Linked " + runtimeLanguage + " -> " + sourceLanguage + " -> en.");
            }
            else
            {
                SetFallbackLanguage(runtime, english);
            }
        }

        private Language EnsureLanguageExists(string targetLanguage)
        {
            var language = Language.FindLanguageByName(targetLanguage);
            if (language != null)
            {
                return language;
            }

            // R2API.Language can store custom language tokens, but RoR2 still needs
            // an internal Language object before SetCurrentLanguage can switch to it.
            LanguageAPI.Add("UAFORCE_LANGUAGE_MARKER", string.Empty, targetLanguage);

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
