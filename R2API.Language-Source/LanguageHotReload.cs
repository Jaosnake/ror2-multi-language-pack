using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Timer = System.Timers.Timer;
using UnityEngine;

namespace R2API;

public class LanguageHotReload : IDisposable
{
    private static LanguageHotReload _instance;
    public static LanguageHotReload Instance => _instance ??= new LanguageHotReload();

    public event Action<string, string> OnTokenReloaded;
    public event Action<string> OnFileReloaded;
    public event Action<IEnumerable<string>> OnBatchReloaded;
    public event Action OnBeforeReload;
    public event Action OnAfterReload;

    public bool IsEnabled { get; private set; }
    public string WatchDirectory { get; private set; }

    private FileSystemWatcher _languageWatcher;
    private FileSystemWatcher _jsonWatcher;
    private readonly Dictionary<string, DateTime> _lastModified = new();
    private readonly Queue<string> _pending = new();
    private readonly object _pendingLock = new();
    private Timer _debounceTimer;

    // Captured lazily on first use from the main Unity thread.
    private SynchronizationContext _unityContext;

    public void Enable(string directory)
    {
        if (IsEnabled) Disable();
        if (!Directory.Exists(directory))
        {
            Debug.LogError($"[LanguageAPI] Diretorio nao encontrado: {directory}");
            return;
        }

        // Capture Unity sync context here — called from OnEnable which runs on the main thread.
        _unityContext = SynchronizationContext.Current;

        WatchDirectory = directory;

        try
        {
            _languageWatcher = new FileSystemWatcher(directory)
            {
                Filter = "*.language",
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
            };
            _languageWatcher.Changed += OnChanged;
            _languageWatcher.Created += OnChanged;
            _languageWatcher.Deleted += OnChanged;
            _languageWatcher.EnableRaisingEvents = true;

            var peleJsonDir = Path.Combine(directory, "PELE", "Language");
            if (Directory.Exists(peleJsonDir))
            {
                _jsonWatcher = new FileSystemWatcher(peleJsonDir)
                {
                    Filter = "*.json",
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
                };
                _jsonWatcher.Changed += OnChanged;
                _jsonWatcher.Created += OnChanged;
                _jsonWatcher.Deleted += OnChanged;
                _jsonWatcher.EnableRaisingEvents = true;
            }

            _debounceTimer = new Timer(500) { AutoReset = false };
            _debounceTimer.Elapsed += (_, _) => ProcessPending();

            IsEnabled = true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LanguageAPI] Erro ao iniciar FileSystemWatcher: {ex.Message}");
        }
    }

    public void Disable()
    {
        IsEnabled = false; // Set first so ProcessPending bails out quickly if racing.

        if (_languageWatcher != null)
        {
            _languageWatcher.EnableRaisingEvents = false;
            _languageWatcher.Changed -= OnChanged;
            _languageWatcher.Created -= OnChanged;
            _languageWatcher.Deleted -= OnChanged;
            _languageWatcher.Dispose();
            _languageWatcher = null;
        }

        if (_jsonWatcher != null)
        {
            _jsonWatcher.EnableRaisingEvents = false;
            _jsonWatcher.Changed -= OnChanged;
            _jsonWatcher.Created -= OnChanged;
            _jsonWatcher.Deleted -= OnChanged;
            _jsonWatcher.Dispose();
            _jsonWatcher = null;
        }

        _debounceTimer?.Stop();
        _debounceTimer?.Dispose();
        _debounceTimer = null;

        lock (_pendingLock)
        {
            _pending.Clear();
            _lastModified.Clear();
        }
    }

    public void Dispose() => Disable();

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        lock (_pendingLock)
        {
            try
            {
                var lastWrite = File.GetLastWriteTime(e.FullPath);
                if (_lastModified.TryGetValue(e.FullPath, out var prev) && prev == lastWrite)
                    return;
                _lastModified[e.FullPath] = lastWrite;
            }
            catch
            {
                // File may be locked/deleted mid-event; skip it.
                return;
            }

            if (!_pending.Contains(e.FullPath))
                _pending.Enqueue(e.FullPath);

            _debounceTimer?.Stop();
            _debounceTimer?.Start();
        }
    }

    private void ProcessPending()
    {
        if (!IsEnabled) return;

        List<string> files;
        lock (_pendingLock)
        {
            files = new List<string>(_pending);
            _pending.Clear();
        }

        if (files.Count == 0) return;

        // Fire OnBeforeReload BEFORE modifying any token state.
        void FireEvents()
        {
            try { OnBeforeReload?.Invoke(); } catch (Exception ex) { Debug.LogError("[LanguageAPI] OnBeforeReload: " + ex.Message); }

            LanguageAPI.ClearAllTokens();

            var allFiles = LanguageFileHelper.GetLanguageFiles(WatchDirectory);
            foreach (var file in allFiles)
            {
                try
                {
                    var langTokens = LanguageFileHelper.ParseTokensFromFile(file);
                    foreach (var kvp in langTokens)
                        foreach (var token in kvp.Value)
                            LanguageAPI.AddOrUpdateToken(token.Key, token.Value, kvp.Key);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[LanguageAPI] Erro ao recarregar {file}: {ex.Message}");
                }
            }

            try { OnAfterReload?.Invoke(); } catch (Exception ex) { Debug.LogError("[LanguageAPI] OnAfterReload: " + ex.Message); }
            try { OnBatchReloaded?.Invoke(files); } catch (Exception ex) { Debug.LogError("[LanguageAPI] OnBatchReloaded: " + ex.Message); }
        }

        var ctx = _unityContext;
        if (ctx != null)
            ctx.Post(_ => FireEvents(), null);
        else
            FireEvents();
    }
}
