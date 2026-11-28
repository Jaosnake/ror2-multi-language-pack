using System;
using System.Collections.Generic;
using System.IO;
using Path = System.IO.Path;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using HarmonyLib;
using RoR2;
using RoR2.UI;
using R2API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class LanguagePreview
{
    private void RenderComplexText(string lang)
    {
        foreach (var label in rendered.Keys.ToArray())
            if (label == null || !label.isActiveAndEnabled) RemoveOverlay(label);
        foreach (var label in FindObjectsOfType<TextMeshProUGUI>())
        {
            if (!label.isActiveAndEnabled || string.IsNullOrWhiteSpace(label.text) ||
                label.rectTransform.rect.width <= 0 || label.rectTransform.rect.height <= 0)
            {
                RemoveOverlay(label);
                continue;
            }
            Texture2D texture;
            var horizontal = label.horizontalAlignment;
            // Preserve each control's own alignment: forcing Arabic to the right
            // makes labels collide with badges and checkboxes in the original layout.
            // The renderer still shapes Arabic with right-to-left flow.
            int alignment = horizontal == HorizontalAlignmentOptions.Center ? 1 :
                horizontal == HorizontalAlignmentOptions.Right ? 2 : 0;
            bool requiresRenderer = HasNewScript(label.text, lang);
            if (requiresRenderer && settingsLoaded) StartRenderer();
            if (!requiresRenderer || worker == null ||
                !worker.TryGet(label, lang, alignment, Time.unscaledTime, out texture))
            {
                if (requiresRenderer && renderAttempted && (worker == null || worker.Unavailable))
                    EnableNativeFallback(lang);
                RemoveOverlay(label);
                continue;
            }
            RawImage image;
            if (!rendered.TryGetValue(label, out image))
            {
                var child = new GameObject("PELE language glyphs", typeof(RectTransform), typeof(RawImage));
                image = child.GetComponent<RawImage>();
                image.raycastTarget = false;
                var rect = image.rectTransform;
                rect.SetParent(label.rectTransform, false);
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f,0.5f);
                rect.anchoredPosition = Vector2.zero;
                rendered.Add(label, image);
                originalAlpha.Add(label, label.canvasRenderer.GetAlpha());
            }
            image.texture = texture;
            var anchor = alignment * 0.5f;
            var vertical = label.verticalAlignment == VerticalAlignmentOptions.Top ? 1f :
                label.verticalAlignment == VerticalAlignmentOptions.Bottom ? 0f : 0.5f;
            image.rectTransform.anchorMin = image.rectTransform.anchorMax = new Vector2(anchor, vertical);
            image.rectTransform.pivot = new Vector2(anchor, vertical);
            image.color = Color.white;
            var fontHeight = texture.height * 0.5f;
            var height = Mathf.Min(label.rectTransform.rect.height,
                Mathf.Min(fontHeight, label.rectTransform.rect.width * texture.height / texture.width));
            image.rectTransform.sizeDelta = new Vector2(height * texture.width / texture.height, height);
            label.canvasRenderer.SetAlpha(0);
        }
    }

    private static bool UsesShapeText(string language) => language == "ar" || language == "hi" || language == "th";

    private static bool HasNewScript(string value, string language)
    {
        if (language == "ar") return value.Any(c => c >= '\u0600' && c <= '\u06ff');
        if (language == "hi") return value.Any(c => c >= '\u0900' && c <= '\u097f');
        return value.Any(c => c >= '\u0e00' && c <= '\u0e7f');
    }
}
