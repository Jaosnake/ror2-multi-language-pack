using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;

internal sealed class ExtendedScriptFont : IDisposable
{
    private const int AtlasSize = 2048;
    private const int Padding = 8;
    private readonly string path;
    private readonly ManualLogSource log;
    private readonly (int First, int Last)[] ranges;
    private TMP_FontAsset asset;
    private Texture2D atlas;
    private Material material;
    private bool attempted;

    internal ExtendedScriptFont(string path, ManualLogSource log, params (int First, int Last)[] ranges)
    {
        this.path = path;
        this.log = log;
        this.ranges = ranges.Length > 0 ? ranges :
            new[] { (0x00C0, 0x01B0), (0x0300, 0x036F), (0x0370, 0x03FF), (0x1EA0, 0x1EFF) };
    }

    internal void EnsureRegistered()
    {
        var fallbacks = TMP_Settings.fallbackFontAssets;
        if (fallbacks == null) return;

        if (!attempted)
        {
            attempted = true;
            try { CreateAtlas(); }
            catch (Exception error)
            {
                Dispose();
                log.LogWarning("Fonte alternativa indisponível: " + Path.GetFileName(path) + ". " + error.Message);
            }
        }
        if (asset == null || fallbacks.Contains(asset)) return;
        fallbacks.Add(asset);
        foreach (var text in UnityEngine.Object.FindObjectsOfType<TMP_Text>())
            text.SetVerticesDirty();
    }

    private void CreateAtlas()
    {
        if (!File.Exists(path)) throw new FileNotFoundException("Noto Sans", path);
        FontEngine.InitializeFontEngine();
        var result = FontEngine.LoadFontFace(path, 40);
        if (result != FontEngineError.Success) throw new InvalidOperationException(result.ToString());

        atlas = new Texture2D(AtlasSize, AtlasSize, TextureFormat.Alpha8, false);
        atlas.SetPixels32(new Color32[AtlasSize * AtlasSize]);
        atlas.Apply();
        var free = new List<GlyphRect> { new GlyphRect(0, 0, AtlasSize - 1, AtlasSize - 1) };
        var used = new List<GlyphRect>();
        var glyphs = new Dictionary<uint, Glyph>();
        var characters = new List<TMP_Character>();
        var flags = BindingFlags.Static | BindingFlags.NonPublic;
        var getIndex = typeof(FontEngine).GetMethod("GetGlyphIndex", flags);
        var addGlyph = typeof(FontEngine).GetMethod("TryAddGlyphToTexture", flags);

        foreach (var range in ranges)
        {
            for (var code = (uint)range.First; code <= range.Last; code++)
            {
                var index = (uint)getIndex.Invoke(null, new object[] { code });
                if (index == 0) continue;
                if (!glyphs.TryGetValue(index, out var glyph))
                {
                    object[] args = { index, Padding, GlyphPackingMode.BestShortSideFit, free, used,
                        GlyphRenderMode.SDFAA, atlas, null };
                    if (!(bool)addGlyph.Invoke(null, args))
                        throw new InvalidOperationException("Font atlas is full at U+" + code.ToString("X4"));
                    glyph = (Glyph)args[7];
                    glyphs.Add(index, glyph);
                }
                characters.Add(new TMP_Character(code, glyph));
            }
        }
        atlas.Apply();
        asset = ScriptableObject.CreateInstance<TMP_FontAsset>();
        asset.name = "P.E.L.E. Noto Sans " + Path.GetFileNameWithoutExtension(path);
        asset.faceInfo = FontEngine.GetFaceInfo();
        asset.atlasPopulationMode = AtlasPopulationMode.Static;
        asset.atlasTextures = new[] { atlas };
        SetFontField("m_Version", "1.1.0");
        SetFontField("m_AtlasWidth", AtlasSize);
        SetFontField("m_AtlasHeight", AtlasSize);
        SetFontField("m_AtlasPadding", Padding);
        SetFontField("m_AtlasRenderMode", GlyphRenderMode.SDFAA);
        SetFontField("m_GlyphTable", new List<Glyph>(glyphs.Values));
        SetFontField("m_CharacterTable", characters);
        material = new Material(TMP_Settings.defaultFontAsset.material);
        material.mainTexture = atlas;
        material.SetFloat("_TextureWidth", AtlasSize);
        material.SetFloat("_TextureHeight", AtlasSize);
        material.SetFloat("_GradientScale", Padding + 1);
        asset.material = material;
        asset.ReadFontAssetDefinition();
        log.LogInfo($"Fonte alternativa {asset.name}: {characters.Count} caracteres.");
    }

    private void SetFontField(string name, object value) =>
        typeof(TMP_FontAsset).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(asset, value);

    public void Dispose()
    {
        if (asset != null)
        {
            TMP_Settings.fallbackFontAssets?.Remove(asset);
            UnityEngine.Object.Destroy(asset);
        }
        if (material != null) UnityEngine.Object.Destroy(material);
        if (atlas != null) UnityEngine.Object.Destroy(atlas);
        asset = null;
        material = null;
        atlas = null;
    }
}
