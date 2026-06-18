using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Threading;
using BepInEx;
using BepInEx.Logging;
using RoR2;
using SimpleJSON;
using UnityEngine;

namespace R2API;

public static partial class LanguageAPI
{
    public const string PluginGUID = "jaosnake.r2api.language";
    public const string PluginName = "R2API.Language (Jaosnake fork)";
    public const string PluginVersion = "2.0.0";

    public static bool Loaded => true;

    internal static ManualLogSource Log { get; set; }

    private static readonly Dictionary<string, Dictionary<string, string>> CustomLanguage = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, Dictionary<string, string>> OverlayLanguage = new(StringComparer.OrdinalIgnoreCase);
    private static readonly List<LanguageOverlay> temporaryOverlays = new();
    private static readonly ReaderWriterLockSlim _rwLock = new();
    private static LanguageHotReload _hotReload;

    private static Dictionary<string, int> _peleTokenCountCache;
    private static DateTime _peleTokenCacheTime = DateTime.MinValue;
    private static int _cachedLanguageFileCount;
    private static int _cachedPeleJsonFileCount;
    private static string[] _cachedPeleFolders;
    private static Dictionary<string, List<string>> _cachedModFiles;
    private static DateTime _diskCacheTime = DateTime.MinValue;

    private const string GenericLanguage = "generic";
    private const string GenericLegacyName = "strings";

    private static bool _hooksEnabled = false;

    internal static void SetHooks()
    {
        if (_hooksEnabled) return;

        try
        {
            _hooksEnabled = true;
            LoadLanguageFilesFromPluginFolder();
        }
        finally
        {
            _hooksEnabled = false;
        }

        On.RoR2.Language.GetLocalizedStringByToken += Language_GetLocalizedStringByToken;
        On.RoR2.Language.TokenIsRegistered += Language_TokenIsRegistered;

        _hooksEnabled = true;
    }

    private static void LoadLanguageFilesFromPluginFolder()
    {
        foreach (var path in Directory.GetFiles(Paths.PluginPath, "*.language", SearchOption.AllDirectories))
            AddPath(path);
    }

    internal static void UnsetHooks()
    {
        On.RoR2.Language.GetLocalizedStringByToken -= Language_GetLocalizedStringByToken;
        On.RoR2.Language.TokenIsRegistered -= Language_TokenIsRegistered;
        _hooksEnabled = false;
    }

    internal static void DisposeLock()
    {
        if (_rwLock != null)
        {
            _rwLock.Dispose();
        }
    }

    private static bool TryGetToken(Dictionary<string, Dictionary<string, string>> dict, string lang, string token, out string value)
    {
        _rwLock.EnterReadLock();
        try
        {
            if (dict.TryGetValue(lang, out var langDict) && langDict.TryGetValue(token, out value))
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

    private static bool TokenExists(Dictionary<string, Dictionary<string, string>> dict, string lang, string token)
    {
        _rwLock.EnterReadLock();
        try
        {
            return (dict.TryGetValue(lang, out var ld) && ld.ContainsKey(token))
                || (dict.TryGetValue(GenericLanguage, out var gd) && gd.ContainsKey(token));
        }
        finally
        {
            _rwLock.ExitReadLock();
        }
    }

    private static bool Language_TokenIsRegistered(On.RoR2.Language.orig_TokenIsRegistered orig, Language self, string token)
    {
        var lang = self.name;
        if (TokenExists(OverlayLanguage, lang, token)) return true;
        if (TokenExists(CustomLanguage, lang, token)) return true;
        return orig(self, token);
    }

    private static string Language_GetLocalizedStringByToken(On.RoR2.Language.orig_GetLocalizedStringByToken orig, Language self, string token)
    {
        var lang = self.name;
        if (TryGetToken(OverlayLanguage, lang, token, out var v)) return v;
        if (TryGetToken(CustomLanguage, lang, token, out v)) return v;
        return orig(self, token);
    }

    private static Dictionary<string, Dictionary<string, string>> LoadFile(string fileContent)
    {
        var result = new Dictionary<string, Dictionary<string, string>>();

        try
        {
            var json = JSON.Parse(fileContent);
            if (json == null) return null;

            foreach (var language in json.Keys)
            {
                var langObj = json[language];
                if (langObj == null) continue;

                var langName = language == GenericLegacyName ? GenericLanguage : language;

                if (!result.TryGetValue(langName, out var langDict))
                {
                    langDict = new Dictionary<string, string>();
                    result[langName] = langDict;
                }

                foreach (var key in langObj.Keys)
                    langDict[key] = langObj[key].Value;
            }
        }
        catch (Exception ex)
        {
            Debug.LogFormat("Parsing error in language file, Error: {0}", ex);
            return null;
        }

        return result.Count > 0 ? result : null;
    }

    public static void Add(string key, string value)
    {
        SetHooks();
        Add(key, value, GenericLanguage);
    }

    public static void Add(string key, string value, string language)
    {
        SetHooks();

        if (key == null) throw new ArgumentNullException(nameof(key));
        if (value == null) throw new ArgumentNullException(nameof(value));
        if (language == null) throw new ArgumentNullException(nameof(language));

        _rwLock.EnterWriteLock();
        try
        {
            if (!CustomLanguage.TryGetValue(language, out var langDict))
            {
                langDict = new Dictionary<string, string>();
                CustomLanguage[language] = langDict;
            }

            if (!langDict.ContainsKey(key))
                langDict[key] = value;
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    public static void AddPath(string path)
    {
        SetHooks();

        if (path == null) throw new ArgumentNullException(nameof(path));
        Add(File.ReadAllText(path));
    }

    public static void Add(string file)
    {
        SetHooks();

        if (file == null) throw new ArgumentNullException(nameof(file));
        var dict = LoadFile(file);
        if (dict != null) Add(dict);
    }

    public static void Add(Dictionary<string, string> tokenDictionary)
    {
        SetHooks();
        Add(tokenDictionary, GenericLanguage);
    }

    public static void Add(Dictionary<string, string> tokenDictionary, string language)
    {
        SetHooks();

        if (tokenDictionary == null) throw new ArgumentNullException(nameof(tokenDictionary));

        foreach (var item in tokenDictionary)
        {
            if (item.Value != null)
                Add(item.Key, item.Value, language);
        }
    }

    public static void Add(Dictionary<string, Dictionary<string, string>> languageDictionary)
    {
        SetHooks();

        if (languageDictionary == null) throw new ArgumentNullException(nameof(languageDictionary));

        foreach (var kvp in languageDictionary)
        {
            if (kvp.Value != null)
                Add(kvp.Value, kvp.Key);
        }
    }

    #region New Features

    public static void AddOrUpdateToken(string key, string value)
    {
        SetHooks();
        AddOrUpdateToken(key, value, GenericLanguage);
    }

    public static void AddOrUpdateToken(string key, string value, string language)
    {
        SetHooks();

        if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
        if (value == null) throw new ArgumentNullException(nameof(value));
        if (string.IsNullOrEmpty(language)) language = GenericLanguage;

        _rwLock.EnterWriteLock();
        try
        {
            if (!CustomLanguage.TryGetValue(language, out var langDict))
            {
                langDict = new Dictionary<string, string>();
                CustomLanguage[language] = langDict;
            }
            langDict[key] = value;
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    public static void RemoveToken(string key, string language = null)
    {
        SetHooks();

        if (string.IsNullOrEmpty(key)) return;
        var lang = language ?? GenericLanguage;

        _rwLock.EnterWriteLock();
        try
        {
            if (CustomLanguage.TryGetValue(lang, out var dict))
                dict.Remove(key);
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    public static void ClearAllTokens()
    {
        SetHooks();

        _rwLock.EnterWriteLock();
        try
        {
            CustomLanguage.Clear();
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    public static void EnableHotReload(string directory)
    {
        SetHooks();
        _hotReload ??= LanguageHotReload.Instance;
        _hotReload.Enable(directory);
    }

    public static void DisableHotReload()
    {
        _hotReload?.Disable();
    }

    public static void ReloadAllLanguageFiles(string directory)
    {
        SetHooks();

        ClearAllTokens();

        foreach (var file in LanguageFileHelper.GetLanguageFiles(directory))
        {
            try
            {
                var langTokens = LanguageFileHelper.ParseTokensFromFile(file);
                foreach (var kvp in langTokens)
                    foreach (var token in kvp.Value)
                        AddOrUpdateToken(token.Key, token.Value, kvp.Key);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LanguageAPI] Erro ao recarregar {file}: {ex.Message}");
            }
        }
    }

    public static ValidationResult ValidateLanguageFile(string filePath)
    {
        SetHooks();
        return new LanguageFileValidator().ValidateFile(filePath);
    }

    public static List<DuplicateEntry> FindDuplicateTokens(string directory)
    {
        SetHooks();
        return new DuplicateTokenDetector().FindDuplicateTokens(directory);
    }

    public static List<string> FindUnusedTokens(string directory)
    {
        SetHooks();
        return new TokenUsageAnalyzer().FindUnusedTokens(directory);
    }

    public static void CompileLanguageFiles(string sourceDirectory, string outputDirectory)
    {
        SetHooks();
        new LanguageFileCompiler().CompileLanguageFiles(sourceDirectory, outputDirectory);
    }

    #endregion

    #region Language Overlay

    public class LanguageOverlay
    {
        private readonly List<OverlayTokenData> _overlayTokenDatas;
        public readonly ReadOnlyCollection<OverlayTokenData> readOnlyOverlays;

        internal LanguageOverlay(List<OverlayTokenData> data)
        {
            _overlayTokenDatas = data;
            readOnlyOverlays = data.AsReadOnly();
            Apply();
            temporaryOverlays.Add(this);
        }

        private void Apply()
        {
            _rwLock.EnterWriteLock();
            try
            {
                foreach (var item in readOnlyOverlays)
                {
                    if (!OverlayLanguage.TryGetValue(item.lang, out var langDict))
                    {
                        langDict = new Dictionary<string, string>();
                        OverlayLanguage[item.lang] = langDict;
                    }
                    langDict[item.key] = item.value;
                }
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public void Remove()
        {
            SetHooks();
            temporaryOverlays.Remove(this);

            _rwLock.EnterWriteLock();
            try
            {
                OverlayLanguage.Clear();
                foreach (var item in temporaryOverlays)
                    item.Apply();
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }
    }

    public static LanguageOverlay AddOverlay(string key, string value)
    {
        SetHooks();
        return AddOverlay(key, value, GenericLanguage);
    }

    public static LanguageOverlay AddOverlay(string key, string value, string lang)
    {
        SetHooks();

        if (key == null) throw new ArgumentNullException(nameof(key));
        if (value == null) throw new ArgumentNullException(nameof(value));
        if (lang == null) throw new ArgumentNullException(nameof(lang));

        return new LanguageOverlay(new List<OverlayTokenData>(1)
        {
            new OverlayTokenData(key, value, lang)
        });
    }

    public static LanguageOverlay AddOverlayPath(string path)
    {
        SetHooks();

        if (path == null) throw new ArgumentNullException(nameof(path));
        return AddOverlay(File.ReadAllText(path));
    }

    public static LanguageOverlay AddOverlay(string file)
    {
        SetHooks();

        if (file == null) throw new ArgumentNullException(nameof(file));
        var dict = LoadFile(file);
        return dict != null ? AddOverlay(dict) : null;
    }

    public static LanguageOverlay AddOverlay(Dictionary<string, string> tokenDictionary)
    {
        SetHooks();
        return AddOverlay(tokenDictionary, GenericLanguage);
    }

    public static LanguageOverlay AddOverlay(Dictionary<string, string> tokenDictionary, string language)
    {
        SetHooks();

        if (tokenDictionary == null) throw new ArgumentNullException(nameof(tokenDictionary));
        if (language == null) throw new ArgumentNullException(nameof(language));

        var list = new List<OverlayTokenData>(tokenDictionary.Count);
        foreach (var kvp in tokenDictionary)
        {
            if (kvp.Value != null)
                list.Add(new OverlayTokenData(kvp.Key, kvp.Value, language));
        }

        return new LanguageOverlay(list);
    }

    public static LanguageOverlay AddOverlay(Dictionary<string, Dictionary<string, string>> languageDictionary)
    {
        SetHooks();

        if (languageDictionary == null) throw new ArgumentNullException(nameof(languageDictionary));

        var list = new List<OverlayTokenData>();
        foreach (var (langName, tokens) in languageDictionary)
        {
            if (tokens == null) continue;
            foreach (var (key, value) in tokens)
            {
                if (value != null)
                    list.Add(new OverlayTokenData(key, value, langName));
            }
        }

        return new LanguageOverlay(list);
    }

    #endregion

    #region Debug API

    internal static Dictionary<string, int> GetCustomTokenCounts()
    {
        _rwLock.EnterReadLock();
        try
        {
            var result = new Dictionary<string, int>();
            foreach (var kvp in CustomLanguage)
                result[kvp.Key] = kvp.Value.Count;
            foreach (var kvp in OverlayLanguage)
                result[kvp.Key] = result.GetValueOrDefault(kvp.Key) + kvp.Value.Count;
            return result;
        }
        finally { _rwLock.ExitReadLock(); }
    }

    internal static int GetTotalCustomTokens()
    {
        _rwLock.EnterReadLock();
        try
        {
            return CustomLanguage.Sum(kvp => kvp.Value.Count)
                 + OverlayLanguage.Sum(kvp => kvp.Value.Count);
        }
        finally { _rwLock.ExitReadLock(); }
    }

    internal static IEnumerable<string> GetCustomLanguages()
    {
        _rwLock.EnterReadLock();
        try
        {
            return CustomLanguage.Keys.Union(OverlayLanguage.Keys)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();
        }
        finally { _rwLock.ExitReadLock(); }
    }

    internal static void InvalidateDiskCache()
    {
        _diskCacheTime = DateTime.MinValue;
        _peleTokenCacheTime = DateTime.MinValue;
    }

    internal static void RefreshDiskCache()
    {
        _diskCacheTime = DateTime.UtcNow;

        try { _cachedLanguageFileCount = Directory.GetFiles(Paths.PluginPath, "*.language", SearchOption.AllDirectories).Length; }
        catch (Exception ex) { Log?.LogWarning("GetLanguageFileCount: " + ex.Message); _cachedLanguageFileCount = 0; }

        var peleDir = System.IO.Path.Combine(Paths.PluginPath, "PELE", "Language");
        try
        {
            _cachedPeleJsonFileCount = Directory.Exists(peleDir) ? Directory.GetFiles(peleDir, "*.json", SearchOption.AllDirectories).Length : 0;
        }
        catch (Exception ex) { Log?.LogWarning("GetPeleJsonFileCount: " + ex.Message); _cachedPeleJsonFileCount = 0; }

        try
        {
            _cachedPeleFolders = Directory.Exists(peleDir) ? Directory.GetDirectories(peleDir).Select(System.IO.Path.GetFileName).ToArray() : Array.Empty<string>();
        }
        catch (Exception ex) { Log?.LogWarning("GetPeleLanguageFolders: " + ex.Message); _cachedPeleFolders = Array.Empty<string>(); }

        try
        {
            _cachedModFiles = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var file in Directory.GetFiles(Paths.PluginPath, "*.language", SearchOption.AllDirectories))
            {
                var dir = new DirectoryInfo(System.IO.Path.GetDirectoryName(file));
                var modName = dir.Parent?.Name ?? dir.Name;
                if (!_cachedModFiles.TryGetValue(modName, out var list))
                {
                    list = new List<string>();
                    _cachedModFiles[modName] = list;
                }
                list.Add(file);
            }
        }
        catch (Exception ex) { Log?.LogWarning("GetModFiles: " + ex.Message); _cachedModFiles ??= new Dictionary<string, List<string>>(); }
    }

    internal static int GetLanguageFileCount()
    {
        if (_diskCacheTime < DateTime.UtcNow.AddSeconds(-5)) RefreshDiskCache();
        return _cachedLanguageFileCount;
    }

    internal static int GetPeleJsonFileCount()
    {
        if (_diskCacheTime < DateTime.UtcNow.AddSeconds(-5)) RefreshDiskCache();
        return _cachedPeleJsonFileCount;
    }

    internal static string[] GetPeleLanguageFolders()
    {
        if (_diskCacheTime < DateTime.UtcNow.AddSeconds(-5)) RefreshDiskCache();
        return _cachedPeleFolders ?? Array.Empty<string>();
    }

    internal static Dictionary<string, List<string>> GetModFileGroups()
    {
        if (_diskCacheTime < DateTime.UtcNow.AddSeconds(-5)) RefreshDiskCache();
        return _cachedModFiles ?? new Dictionary<string, List<string>>();
    }

    internal static Dictionary<string, int> GetPeleTokenCounts()
    {
        if (_peleTokenCacheTime > DateTime.UtcNow.AddSeconds(-5))
            return _peleTokenCountCache ?? new Dictionary<string, int>();

        var result = new Dictionary<string, int>();
        try
        {
            var peleDir = System.IO.Path.Combine(Paths.PluginPath, "PELE", "Language");
            if (!Directory.Exists(peleDir)) return result;

            foreach (var langDir in Directory.GetDirectories(peleDir))
            {
                var langCode = System.IO.Path.GetFileName(langDir);
                int count = 0;
                foreach (var jsonFile in Directory.GetFiles(langDir, "*.json"))
                {
                    try
                    {
                        var json = SimpleJSON.JSON.Parse(File.ReadAllText(jsonFile));
                        if (json != null) count += json.Keys.Count();
                    }
                    catch (Exception ex)
                    {
                        Log?.LogWarning("Erro ao ler " + jsonFile + ": " + ex.Message);
                    }
                }
                if (count > 0) result[langCode] = count;
            }
        }
        catch (Exception ex)
        {
            Log?.LogWarning("GetPeleTokenCounts: " + ex.Message);
        }

        _peleTokenCountCache = result;
        _peleTokenCacheTime = DateTime.UtcNow;
        return result;
    }

    internal static bool IsHotReloadEnabled => _hotReload?.IsEnabled ?? false;

    #endregion

    public struct OverlayTokenData
    {
        public string key;
        public string value;
        public string lang;

        internal OverlayTokenData(string _key, string _value, string _lang)
        {
            key = _key;
            value = _value;
            lang = _lang;
        }

        internal OverlayTokenData(string _key, string _value)
        {
            key = _key;
            value = _value;
            lang = GenericLanguage;
        }
    }
}
