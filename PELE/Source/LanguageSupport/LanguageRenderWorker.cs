using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using TMPro;
using UnityEngine;

internal sealed class LanguageRenderWorker : IDisposable
{
    private const int MaxPending = 64;
    private static readonly HashSet<string> measured = new HashSet<string>();
    private const int MaxTextureBytes = 48 * 1024 * 1024;
    private static readonly Regex StyleTags = new Regex(@"<style=([^>]+)>|</style>", RegexOptions.IgnoreCase);
    private static readonly Regex SpriteTags = new Regex(@"<sprite(?:\s|=)[^>]*>", RegexOptions.IgnoreCase);
    private readonly Dictionary<string, string> spriteImages = new Dictionary<string, string>();

    private sealed class Cached
    {
        internal Texture2D Texture;
        internal int Bytes;
        internal float LastUsed;
    }

    private readonly Dictionary<string, Cached> cache = new Dictionary<string, Cached>();
    private readonly Dictionary<string, string> pending = new Dictionary<string, string>();
    private readonly HashSet<string> queued = new HashSet<string>();
    private readonly BlockingCollection<string> requests = new BlockingCollection<string>(MaxPending);
    private readonly ConcurrentQueue<string> results = new ConcurrentQueue<string>();
    private readonly Action<string> log;
    private readonly Action<string> warn;
    private Process process;
    private int nextId;
    private int textureBytes;
    private volatile bool closed;
    private volatile bool failed;

    internal LanguageRenderWorker(string path, Action<string> log, Action<string> warn)
    {
        this.log = log;
        this.warn = warn;
        process = new Process { StartInfo = new ProcessStartInfo(path, "--worker") {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        }};
        process.OutputDataReceived += (_, e) => {
            if (e.Data == null) failed = true;
            else results.Enqueue(e.Data);
        };
        process.ErrorDataReceived += (_, e) => {
            if (e.Data != null && !closed) warn(e.Data);
        };
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        new Thread(WriteRequests) { IsBackground = true, Name = "PELE text rendering" }.Start();
    }

    private void WriteRequests()
    {
        try
        {
            using (var input = process.StandardInput)
            {
                input.AutoFlush = true;
                foreach (var request in requests.GetConsumingEnumerable())
                    input.WriteLine(request);
            }
        }
        catch (IOException) { failed = true; }
        catch (ObjectDisposedException) { failed = true; }
    }

    internal void Update(float now)
    {
        string response;
        int count = 0;
        while (count++ < 12 && results.TryDequeue(out response))
        {
            var tab = response.IndexOf('\t');
            if (tab < 0) continue;
            var id = response.Substring(0, tab);
            string key;
            if (!pending.TryGetValue(id, out key)) continue;
            pending.Remove(id);
            queued.Remove(key);
            Texture2D texture = null;
            if (response.Substring(tab + 1) != "-")
            {
                try
                {
                    texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (!ImageConversion.LoadImage(texture, Convert.FromBase64String(response.Substring(tab + 1)), true))
                    {
                        UnityEngine.Object.Destroy(texture);
                        texture = null;
                    }
                    if (texture != null) texture.wrapMode = TextureWrapMode.Clamp;
                }
                catch (FormatException)
                {
                    if (texture != null) UnityEngine.Object.Destroy(texture);
                    texture = null;
                }
            }
            var item = new Cached { Texture = texture, LastUsed = now,
                Bytes = texture == null ? 0 : texture.width * texture.height * 4 };
            cache.Add(key, item);
            textureBytes += item.Bytes;
        }
        if (cache.Count > 512 || textureBytes > MaxTextureBytes)
        {
            foreach (var entry in cache.Where(p => now - p.Value.LastUsed > 2f).OrderBy(p => p.Value.LastUsed).ToArray())
            {
                if (cache.Count <= 384 && textureBytes <= MaxTextureBytes) break;
                if (entry.Value.Texture != null) UnityEngine.Object.Destroy(entry.Value.Texture);
                textureBytes -= entry.Value.Bytes;
                cache.Remove(entry.Key);
            }
        }
        if (failed && !closed)
        {
            warn("O renderizador dos novos idiomas parou.");
            Dispose();
        }
    }

    internal bool TryGet(TextMeshProUGUI label, string lang, int alignment, float now, out Texture2D texture)
    {
        texture = null;
        if (closed) return false;
        var width = Mathf.Clamp(Mathf.RoundToInt(label.rectTransform.rect.width / 2) * 4, 16, 2048);
        var height = Mathf.Clamp(Mathf.RoundToInt(label.rectTransform.rect.height / 2) * 4, 8, Mathf.Min(8192, SystemInfo.maxTextureSize));
        var size = Mathf.Clamp(Mathf.RoundToInt(label.fontSize * 2), 8, 220);
        var markup = label.richText ? ExpandStyles(label) : label.text;
        if (lang == "ar" && (label.text.Contains("الصاعقة الغلفانية") || label.text.Contains("Commando مقاتل")) && measured.Add(label.text))
            log("PELE SIZE DIAGNOSTIC: text=" + label.text.Substring(0, Math.Min(60, label.text.Length)) + " rect=" + label.rectTransform.rect + " font=" + label.fontSize + " min=" + label.fontSizeMin + " max=" + label.fontSizeMax + " auto=" + label.enableAutoSizing + " wrap=" + label.enableWordWrapping + " request=" + width + "x" + height + "/" + size + " alignment=" + alignment + " markup=" + markup.Replace("\r", "[CR]").Replace("\n", "[LF]"));
        if (markup.Length > 32768) return false;
        if (SpriteTags.IsMatch(markup))
        {
            string withSprites;
            if (!TryEmbedSprites(label, markup, out withSprites)) return false;
            markup = withSprites;
        }
        if ((label.fontStyle & FontStyles.Bold) != 0) markup = "<b>" + markup + "</b>";
        if ((label.fontStyle & FontStyles.Italic) != 0) markup = "<i>" + markup + "</i>";
        var key = string.Join("\t", lang, width.ToString(CultureInfo.InvariantCulture),
            height.ToString(CultureInfo.InvariantCulture), size.ToString(CultureInfo.InvariantCulture),
            label.enableWordWrapping ? "1" : "0", alignment.ToString(CultureInfo.InvariantCulture),
            "#" + ColorUtility.ToHtmlStringRGBA(label.color), markup);
        Cached existing;
        if (cache.TryGetValue(key, out existing))
        {
            existing.LastUsed = now;
            texture = existing.Texture;
            return texture != null;
        }
        if (pending.Count >= MaxPending || queued.Contains(key) || markup.Length > 262144) return false;
        var id = (++nextId).ToString(CultureInfo.InvariantCulture);
        var payload = id + "\t" + key.Substring(0, key.Length - markup.Length) +
            Convert.ToBase64String(Encoding.UTF8.GetBytes(markup));
        if (!requests.TryAdd(payload)) return false;
        if (nextId == 1) log("Renderização das traduções ativada.");
        queued.Add(key);
        pending.Add(id, key);
        return false;
    }

    private bool TryEmbedSprites(TextMeshProUGUI label, string markup, out string result)
    {
        result = null;
        try
        {
            label.ForceMeshUpdate();
            var sprites = new List<TMP_CharacterInfo>();
            for (int i = 0; i < label.textInfo.characterCount; ++i)
                if (label.textInfo.characterInfo[i].elementType == TMP_TextElementType.Sprite)
                    sprites.Add(label.textInfo.characterInfo[i]);
            int index = 0;
            result = SpriteTags.Replace(markup, match => {
                if (index >= sprites.Count) return "◆";
                var info = sprites[index++];
                if (info.spriteAsset == null || info.spriteIndex < 0 || info.spriteIndex >= info.spriteAsset.spriteCharacterTable.Count)
                { return "◆"; }
                var glyph = info.spriteAsset.spriteCharacterTable[info.spriteIndex].glyph as TMP_SpriteGlyph;
                var sprite = glyph == null ? null : glyph.sprite;
                if (glyph == null) { return "◆"; }
                var source = sprite != null ? (Texture)sprite.texture : info.spriteAsset.spriteSheet;
                var glyphRect = glyph.glyphRect;
                var rect = sprite != null ? sprite.textureRect : new Rect(glyphRect.x, glyphRect.y, glyphRect.width, glyphRect.height);
                string encoded = EncodeSprite(source, rect, match.Value.IndexOf("tint=1", StringComparison.OrdinalIgnoreCase) >= 0 ? info.color : (Color32)Color.white);
                if (encoded == null) { return "◆"; }
                return "<pele-sprite=" + encoded + ">";
            });
            return index == sprites.Count;
        }
        catch (Exception error)
        {
            warn("Falha ao preparar ícone de texto: " + error.Message);
            return false;
        }
    }

    private string EncodeSprite(Texture source, Rect rect, Color32 tint)
    {
        if (source == null) return null;
        var cacheKey = source.GetInstanceID().ToString(CultureInfo.InvariantCulture) + ":" + rect.ToString() + ":" + ColorUtility.ToHtmlStringRGBA(tint);
        string cached;
        if (spriteImages.TryGetValue(cacheKey, out cached)) return cached;
        if (rect.width < 1 || rect.height < 1 || rect.width > 512 || rect.height > 512 || rect.x < 0 || rect.y < 0 || rect.xMax > source.width || rect.yMax > source.height) return null;
        var previous = RenderTexture.active;
        var target = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
        Texture2D image = null;
        try
        {
            Graphics.Blit(source, target);
            RenderTexture.active = target;
            image = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
            image.ReadPixels(rect, 0, 0);
            if (tint.r != 255 || tint.g != 255 || tint.b != 255 || tint.a != 255)
            {
                var pixels = image.GetPixels32();
                for (int i = 0; i < pixels.Length; i++)
                {
                    var c = pixels[i];
                    c.r = (byte)(c.r * tint.r / 255); c.g = (byte)(c.g * tint.g / 255);
                    c.b = (byte)(c.b * tint.b / 255); c.a = (byte)(c.a * tint.a / 255);
                    pixels[i] = c;
                }
                image.SetPixels32(pixels);
            }
            image.Apply();
            cached = Convert.ToBase64String(ImageConversion.EncodeToPNG(image));
            if (cached.Length > 65536) return null;
            spriteImages[cacheKey] = cached;
            log("Ícone de texto extraído para renderização: " + (int)rect.width + "x" + (int)rect.height + ".");
            return cached;
        }
        finally
        {
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(target);
            if (image != null) UnityEngine.Object.Destroy(image);
        }
    }

    private static string ExpandStyles(TextMeshProUGUI label)
    {
        var sheet = label.styleSheet;
        var defaultSheet = TMP_Settings.defaultStyleSheet;
        if (sheet == null && defaultSheet == null) return label.text;
        var definitions = new Stack<TMP_Style>();
        return StyleTags.Replace(label.text, tag => {
            if (!tag.Groups[1].Success)
            {
                var openedStyle = definitions.Count == 0 ? null : definitions.Pop();
                return openedStyle == null ? tag.Value : openedStyle.styleClosingDefinition;
            }
            var name = tag.Groups[1].Value.Trim('"', '\'');
            var style = sheet == null ? null : sheet.GetStyle(name);
            if (style == null && defaultSheet != null) style = defaultSheet.GetStyle(name);
            definitions.Push(style);
            return style == null ? tag.Value : style.styleOpeningDefinition;
        });
    }

    public void Dispose()
    {
        if (closed) return;
        closed = true;
        requests.CompleteAdding();
        foreach (var item in cache.Values)
            if (item.Texture != null) UnityEngine.Object.Destroy(item.Texture);
        cache.Clear();
    }
}
