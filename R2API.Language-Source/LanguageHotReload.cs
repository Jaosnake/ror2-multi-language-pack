using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using UnityEngine;

namespace R2API;

public class LanguageHotReload : IDisposable
{
    private static LanguageHotReload _instance;
    public static LanguageHotReload Instance => _instance ??= new LanguageHotReload();

    public event Action<string, string> OnTokenReloaded;
    public event Action<string> OnFileReloaded;
    public event Action<IEnumerable<string>> OnBatchReloaded;

    public bool IsEnabled { get; private set; }
    public string WatchDirectory { get; private set; }

    private FileSystemWatcher _watcher;
    private readonly Dictionary<string, DateTime> _lastModified = new();
    private readonly Queue<string> _pending = new();
    private readonly object _lock = new();
    private Timer _debounceTimer;

    public void Enable(string directory)
    {
        if (IsEnabled) Disable();
        if (!Directory.Exists(directory))
        {
            Debug.LogError($"[LanguageAPI] Diretório não encontrado: {directory}");
            return;
        }

        WatchDirectory = directory;

        try
        {
            _watcher = new FileSystemWatcher(directory)
            {
                Filter = "*.language",
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
            };

            _watcher.Changed += OnChanged;
            _watcher.Created += OnChanged;
            _watcher.Deleted += OnChanged;
            _watcher.EnableRaisingEvents = true;

            _debounceTimer = new Timer(500) { AutoReset = false };
            _debounceTimer.Elapsed += (_, _) => ProcessPending();

            IsEnabled = true;
            Debug.Log($"[LanguageAPI] Hot-Reload habilitado: {directory}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LanguageAPI] Erro ao iniciar FileSystemWatcher: {ex.Message}");
        }
    }

    public void Disable()
    {
        if (_watcher != null)
        {
            _watcher.Changed -= OnChanged;
            _watcher.Created -= OnChanged;
            _watcher.Deleted -= OnChanged;
            _watcher.Dispose();
            _watcher = null;
        }

        _debounceTimer?.Stop();
        _debounceTimer?.Dispose();
        _debounceTimer = null;

        lock (_lock) _pending.Clear();
        IsEnabled = false;
    }

    public void Dispose() => Disable();

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        lock (_lock)
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
        List<string> files;
        lock (_lock)
        {
            files = new List<string>(_pending);
            _pending.Clear();
        }

        LanguageAPI.ClearAllTokens();

        var count = 0;
        var allFiles = LanguageFileHelper.GetLanguageFiles(WatchDirectory);
        foreach (var file in allFiles)
        {
            try
            {
                var langTokens = LanguageFileHelper.ParseTokensFromFile(file);
                foreach (var kvp in langTokens)
                {
                    var language = kvp.Key;
                    foreach (var token in kvp.Value)
                    {
                        LanguageAPI.AddOrUpdateToken(token.Key, token.Value, language);
                        OnTokenReloaded?.Invoke(token.Key, token.Value);
                        count++;
                    }
                }
                OnFileReloaded?.Invoke(file);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LanguageAPI] Erro ao recarregar {file}: {ex.Message}");
            }
        }

        Debug.Log($"[LanguageAPI] CLEAN completo: {count} tokens recarregados de {allFiles.Length} arquivos");
        OnBatchReloaded?.Invoke(allFiles);
    }
}
