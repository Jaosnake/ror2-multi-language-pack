using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.Options;
using RoR2;
using System;
using System.Globalization;
using System.Reflection;

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
                "Language code used by the Ukrainian R2API.Language packs.");

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
                "Forces Risk of Rain 2 to use the Ukrainian R2API.Language code. The game does not show Ukrainian in the vanilla language menu.");

            ModSettingsManager.AddOption(new CheckBoxOption(enablePlugin));
            ModSettingsManager.AddOption(new StringInputFieldOption(languageCode));
            ModSettingsManager.AddOption(new CheckBoxOption(onlyWhenSystemLocaleIsUkrainian));
            ModSettingsManager.AddOption(new StringInputFieldOption(fallbackLanguageCode));
            ModSettingsManager.AddOption(new GenericButtonOption(
                "Apply now",
                "General",
                "Applies the current UAForce settings immediately. Restart the game if a mod only loads language files during startup.",
                "Apply",
                () => ApplyCurrentConfig("Risk Of Options button")));
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

            var targetLanguage = languageCode.Value.Trim();
            if (string.IsNullOrEmpty(targetLanguage))
            {
                targetLanguage = "UA";
            }

            try
            {
                if (Language.FindLanguageByName(targetLanguage) == null)
                {
                    Logger.LogWarning("Language '" + targetLanguage + "' was not found. Make sure Ukrainian language packs are installed and loaded.");
                    return;
                }

                SetLanguage(targetLanguage);
                Logger.LogInfo("Current language set to " + targetLanguage + " (" + reason + ").");
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
