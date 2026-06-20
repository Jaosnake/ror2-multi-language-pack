using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using RoR2;
using UnityEngine;

namespace R2API;

internal class LanguageDebugUI : MonoBehaviour
{
    private static LanguageDebugUI _instance;
    private bool _visible;
    private Rect _windowRect;
    private int _activeTab;
    private Vector2 _scrollPos;
    private readonly string[] _tabs = { "Linguas", "PELE", "Mods", "Status", "Duplicatas" };

    private const string PrefsKeyX = "R2APILang_DebugX";
    private const string PrefsKeyY = "R2APILang_DebugY";
    private const string PrefsKeyW = "R2APILang_DebugW";
    private const string PrefsKeyH = "R2APILang_DebugH";

    private List<DuplicateEntry> _duplicates;
    private bool _duplicatesDirty = true;
    private bool _scanning;
    private readonly Queue<Action> _mainThreadQueue = new();

    // Error count is re-computed only when refreshed explicitly, not on every frame.
    private int _errorCount = -1;

    // Cache SetCurrentLanguage so the Languages tab doesn't reflect every frame.
    private static MethodInfo _setCurrentLanguageMethod;

    internal static void Create()
    {
        if (_instance != null) return;
        var go = new GameObject("LanguageDebugUI", typeof(LanguageDebugUI));
        DontDestroyOnLoad(go);
        _instance = go.GetComponent<LanguageDebugUI>();

        _setCurrentLanguageMethod = typeof(Language).GetMethod("SetCurrentLanguage",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
    }

    internal static void DestroyInstance()
    {
        if (_instance != null)
        {
            Destroy(_instance.gameObject);
            _instance = null;
        }
    }

    internal static void Toggle()
    {
        if (_instance != null)
            _instance._visible = !_instance._visible;
    }

    private void Awake()
    {
        _windowRect = new Rect(
            PlayerPrefs.GetFloat(PrefsKeyX, 50f),
            PlayerPrefs.GetFloat(PrefsKeyY, 50f),
            PlayerPrefs.GetFloat(PrefsKeyW, 600f),
            PlayerPrefs.GetFloat(PrefsKeyH, 400f)
        );
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F6))
            _visible = !_visible;

        ProcessMainThreadQueue();
    }

    private void OnGUI()
    {
        if (!_visible) return;
        _windowRect = GUILayout.Window(999, _windowRect, DrawWindow, "R2API.Language Debug",
            GUILayout.MinWidth(400), GUILayout.MinHeight(200));
    }

    private void DrawWindow(int _)
    {
        _activeTab = GUILayout.Toolbar(_activeTab, _tabs);
        _scrollPos = GUILayout.BeginScrollView(_scrollPos);
        switch (_activeTab)
        {
            case 0: DrawLanguagesTab(); break;
            case 1: DrawPeleTab();      break;
            case 2: DrawModsTab();      break;
            case 3: DrawStatusTab();    break;
            case 4: DrawDuplicatesTab(); break;
        }
        GUILayout.EndScrollView();

        if (Event.current.type == EventType.MouseUp)
            SaveWindowRect();
        GUI.DragWindow();
    }

    private void SaveWindowRect()
    {
        PlayerPrefs.SetFloat(PrefsKeyX, _windowRect.x);
        PlayerPrefs.SetFloat(PrefsKeyY, _windowRect.y);
        PlayerPrefs.SetFloat(PrefsKeyW, _windowRect.width);
        PlayerPrefs.SetFloat(PrefsKeyH, _windowRect.height);
        PlayerPrefs.Save();
    }

    private void InvokeSetCurrentLanguage(string lang)
    {
        if (_setCurrentLanguageMethod == null)
        {
            LanguagePlugin.Logger?.LogError("SetCurrentLanguage method not found");
            return;
        }
        try { _setCurrentLanguageMethod.Invoke(null, new object[] { lang }); }
        catch (Exception ex) { LanguagePlugin.Logger?.LogError("Falha ao trocar lingua: " + (ex.InnerException?.Message ?? ex.Message)); }
    }

    private void DrawLanguagesTab()
    {
        GUILayout.Label("Linguas registradas no jogo — clique Ativar para trocar:", GUI.skin.label);
        var current = Language.currentLanguageName;
        var gameLangs = LanguageNames.GetAvailableLanguages();
        if (gameLangs.Count == 0)
        {
            GUILayout.Label("  (nenhuma)");
        }
        else
        {
            foreach (var lang in gameLangs)
            {
                var name = LanguageNames.GetFriendlyName(lang);
                GUILayout.BeginHorizontal();
                GUILayout.Label($"  {lang} - {name}", GUILayout.Width(250));
                if (lang == current)
                    GUILayout.Label("[ATIVO]", GUILayout.Width(60));
                else if (GUILayout.Button("Ativar", GUILayout.Width(60)))
                    InvokeSetCurrentLanguage(lang);
                GUILayout.EndHorizontal();
            }
        }

        GUILayout.Space(10);
        GUILayout.Label("Tokens customizados por lingua (API):", GUI.skin.label);
        var counts = LanguageAPI.GetCustomTokenCounts();
        if (counts.Count == 0)
            GUILayout.Label("  (nenhum)");
        else
            foreach (var kvp in counts.OrderBy(x => x.Key))
                GUILayout.Label($"  {kvp.Key}: {kvp.Value} tokens");

        GUILayout.Space(5);
        GUILayout.Label("Total de tokens via API: " + LanguageAPI.GetTotalCustomTokens());
    }

    private void DrawPeleTab()
    {
        var folders = LanguageAPI.GetPeleLanguageFolders();
        if (folders.Length == 0)
        {
            GUILayout.Label("Pasta PELE/Language/ nao encontrada.");
            return;
        }

        GUILayout.Label("Pastas de idioma encontradas:", GUI.skin.label);
        foreach (var f in folders)
            GUILayout.Label("  " + f);

        GUILayout.Space(5);
        GUILayout.Label("Arquivos JSON: " + LanguageAPI.GetPeleJsonFileCount());

        GUILayout.Space(5);
        GUILayout.Label("Tokens por lingua (PELE JSONs):", GUI.skin.label);
        var peleCounts = LanguageAPI.GetPeleTokenCounts();
        if (peleCounts.Count == 0)
            GUILayout.Label("  (nenhum token)");
        else
            foreach (var kvp in peleCounts.OrderBy(x => x.Key))
                GUILayout.Label($"  {kvp.Key}: {kvp.Value} tokens");
    }

    private void DrawModsTab()
    {
        GUILayout.Label("Mods com arquivos .language:", GUI.skin.label);
        var modFiles = LanguageAPI.GetModFileGroups();
        if (modFiles.Count == 0)
        {
            GUILayout.Label("  (nenhum encontrado)");
            return;
        }

        foreach (var kvp in modFiles.OrderBy(x => x.Key))
        {
            GUILayout.Label($"  {kvp.Key}: {kvp.Value.Count} arquivo(s)");
            foreach (var f in kvp.Value.Take(5))
                GUILayout.Label("      " + System.IO.Path.GetFileName(f));
            if (kvp.Value.Count > 5)
                GUILayout.Label($"      ... e mais {kvp.Value.Count - 5}");
        }
    }

    private void DrawStatusTab()
    {
        GUILayout.Label("Status do Plugin", GUI.skin.label);
        GUILayout.Label("  DLL: " + LanguageAPI.PluginGUID + " v" + LanguageAPI.PluginVersion);
        GUILayout.Label("  Hot-Reload: " + (LanguageAPI.IsHotReloadEnabled ? "ATIVO" : "INATIVO"));

        GUILayout.Space(5);
        GUILayout.Label("Diretorios monitorados:", GUI.skin.label);
        GUILayout.Label("  *.language: " + BepInEx.Paths.PluginPath);
        GUILayout.Label("  *.json:     " + System.IO.Path.Combine(BepInEx.Paths.PluginPath, "PELE", "Language"));

        GUILayout.Space(5);
        GUILayout.Label("Arquivos carregados:", GUI.skin.label);
        GUILayout.Label($"  .language: {LanguageAPI.GetLanguageFileCount()}");
        GUILayout.Label($"  .json:     {LanguageAPI.GetPeleJsonFileCount()}");
        GUILayout.Label($"  Tokens:    {LanguageAPI.GetTotalCustomTokens()}");
        GUILayout.Label($"  Erros de sintaxe: {(_errorCount >= 0 ? _errorCount.ToString() : "?")}");

        GUILayout.Space(5);
        GUILayout.Label("Custom langs:", GUI.skin.label);
        GUILayout.Label("  " + string.Join(", ", LanguagePlugin.GetRegisteredCustomLangs()));
        GUILayout.Label("  Com fallback: " + string.Join(", ", LanguagePlugin.GetFallbackLangs()));

        GUILayout.Space(10);
        if (GUILayout.Button("Refresh cache"))
        {
            LanguageAPI.InvalidateDiskCache();
            LanguageAPI.RefreshDiskCache();
            _errorCount = CountFileErrors();
            _duplicatesDirty = true;
        }

        if (GUILayout.Button("Recarregar tudo (F5)"))
        {
            LanguageAPI.InvalidateDiskCache();
            LanguageAPI.ReloadAllLanguageFiles(BepInEx.Paths.PluginPath);
            LanguagePlugin.ReloadPeleJsonFiles();
            _errorCount = CountFileErrors();
            _duplicatesDirty = true;
        }

        GUILayout.Space(5);
        GUILayout.Label("F6 - Alternar esta janela", GUI.skin.label);
    }

    private void DrawDuplicatesTab()
    {
        if (_duplicatesDirty && !_scanning)
        {
            _scanning = true;
            _duplicates = null;
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    var result = new DuplicateTokenDetector().FindDuplicateTokens(BepInEx.Paths.PluginPath);
                    _mainThreadQueue.Enqueue(() =>
                    {
                        _duplicates = result;
                        _duplicatesDirty = false;
                        _scanning = false;
                    });
                }
                catch (Exception ex)
                {
                    _mainThreadQueue.Enqueue(() =>
                    {
                        _duplicates = new List<DuplicateEntry>();
                        _duplicatesDirty = false;
                        _scanning = false;
                        Debug.LogError("Erro ao escanear duplicatas: " + ex.Message);
                    });
                }
            });
        }

        if (_scanning)
        {
            GUILayout.Label("Escaneando arquivos .language...");
            return;
        }

        if (_duplicates == null || _duplicates.Count == 0)
        {
            GUILayout.Label("Nenhum token duplicado encontrado.");
            return;
        }

        GUILayout.Label($"{_duplicates.Count} token(s) duplicado(s):", GUI.skin.label);
        GUILayout.Space(5);

        int maxShow = Math.Min(_duplicates.Count, 50);
        foreach (var dup in _duplicates.OrderByDescending(d => d.Count).Take(maxShow))
        {
            var langTag = string.IsNullOrEmpty(dup.Language) ? "" : $"[{dup.Language}] ";
            GUILayout.Label($"  {langTag}{dup.TokenName} ({dup.Count}x)");
            foreach (var f in dup.Files)
                GUILayout.Label("      " + f);
            GUILayout.Space(3);
        }
        if (_duplicates.Count > 50)
            GUILayout.Label($"  ... e mais {_duplicates.Count - 50} token(s)");
    }

    private void ProcessMainThreadQueue()
    {
        while (_mainThreadQueue.Count > 0)
        {
            try { _mainThreadQueue.Dequeue()?.Invoke(); }
            catch (Exception ex) { Debug.LogError("Error processing main thread queue: " + ex.Message); }
        }
    }

    private static int CountFileErrors()
    {
        int total = 0;
        try
        {
            var validator = new LanguageFileValidator();
            foreach (var file in Directory.GetFiles(BepInEx.Paths.PluginPath, "*.language", SearchOption.AllDirectories))
            {
                var result = validator.ValidateFile(file);
                if (!result.IsValid)
                    total += result.Errors.Count;
            }
        }
        catch { }
        return total;
    }
}
