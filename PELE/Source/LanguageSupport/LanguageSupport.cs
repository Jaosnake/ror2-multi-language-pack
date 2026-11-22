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
using R2API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[BepInPlugin("jaosnake.pele.language.support", "PELE Language Support", "1.0.0")]
[BepInDependency("com.bepis.r2api.language", "2.0.0")]
public sealed class LanguagePreview : BaseUnityPlugin
{
    private static readonly string[] Codes = { "th", "hi", "ar" };
    private static readonly string[] DisplayNames = { "Thai", "Hindi", "Arabic" };
    private const BindingFlags PrivateStatic = BindingFlags.Static | BindingFlags.NonPublic;
    private static MethodInfo create;
    private static MethodInfo getLanguages;
    private static FieldInfo selfName;
    private static FieldInfo iconSprite;
    private static LanguagePreview instance;
    private bool ready;
    private bool settingsLoaded;
    private float next;
    private string lastLanguage;
    private string renderer;
    private bool renderAttempted;
    private LanguageRenderWorker worker;
    private readonly Dictionary<string, Dictionary<string, Texture2D>> pictures =
        new Dictionary<string, Dictionary<string, Texture2D>>();
    private readonly Dictionary<TextMeshProUGUI, RawImage> rendered =
        new Dictionary<TextMeshProUGUI, RawImage>();
    private readonly Dictionary<TextMeshProUGUI, float> originalAlpha =
        new Dictionary<TextMeshProUGUI, float>();
    private readonly Dictionary<string, Sprite> flags = new Dictionary<string, Sprite>();

    private void Awake()
    {
        try
        {
            var api = typeof(LanguageAPI).Assembly;
            var registration = api.GetType("R2API.CustomLanguageRegistration", true);
            var names = api.GetType("R2API.LanguageNames", true);
            var map = (Dictionary<string,string>)names.GetField("Map", PrivateStatic).GetValue(null);
            for (int i = 0; i < Codes.Length; i++)
                map[Codes[i]] = DisplayNames[i];
            selfName = typeof(Language).GetField("<selfName>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            if (selfName == null) Logger.LogWarning("Campo de nome do seletor principal indisponivel.");
            iconSprite = typeof(Language).GetField("<iconSprite>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            if (iconSprite == null) Logger.LogWarning("Campo de bandeira do seletor principal indisponivel.");
            create = registration.GetMethod("CreateLanguage", PrivateStatic);
            getLanguages = registration.GetMethod("GetLanguagesByName", PrivateStatic);
            if (create == null || getLanguages == null)
                throw new MissingMethodException("PELE language registration");
            var choices = names.GetMethod("GetAvailableLanguages", PrivateStatic);
            if (choices == null) throw new MissingMethodException("PELE language list");
            instance = this;
            var harmony = new Harmony("jaosnake.pele.language.preview");
            harmony.Patch(choices, prefix: new HarmonyMethod(typeof(LanguagePreview), "EnsureRegistered"));
            // The game's runtime can optimize the P.E.L.E. readonly array. Register through
            // the language dictionary when the native callers need the entries.
            harmony.Patch(AccessTools.Method(typeof(Language), "GetAllLanguages"),
                prefix: new HarmonyMethod(typeof(LanguagePreview), "EnsureRegistered"));
            harmony.Patch(AccessTools.Method(typeof(Language), "FindLanguageByName", new[] { typeof(string) }),
                prefix: new HarmonyMethod(typeof(LanguagePreview), "BeforeSwitch"));
            var setter = typeof(Language).GetMethods(PrivateStatic | BindingFlags.Public).FirstOrDefault(m => m.Name == "SetCurrentLanguage" && m.GetParameters().Length > 0 && m.GetParameters()[0].ParameterType == typeof(string));
            if (setter != null)
                harmony.Patch(setter, prefix: new HarmonyMethod(typeof(LanguagePreview), "BeforeSwitch") {
                    priority = Priority.First
                });
            var assets = Path.Combine(Path.GetDirectoryName(Info.Location), "assets");
            renderer = Path.Combine(Path.GetDirectoryName(Info.Location), "renderer", "ShapeText.exe");
            bool extended = File.Exists(renderer);
            foreach (var lang in Codes)
            {
                pictures.Add(lang, new Dictionary<string, Texture2D>());
                LoadFlag(assets, lang);
            }
            RoR2Application.onLoadFinished += OnGameLoaded;
            ready = true;
            Logger.LogInfo("Idiomas registrados: Thai, Hindi, Arabic.");
        }
        catch (Exception error) { Logger.LogError(error); }
    }

    private void StartRenderer()
    {
        if (renderAttempted) return;
        renderAttempted = true;
        if (!File.Exists(renderer)) return;
        try
        {
            worker = new LanguageRenderWorker(renderer, message => Logger.LogInfo(message),
                message => Logger.LogWarning(message));
        }
        catch (Exception error)
        {
            Logger.LogError("Falha ao iniciar o renderizador: " + error.Message);
        }
    }

    private void OnGameLoaded()
    {
        settingsLoaded = true;
    }

    private void OnDestroy()
    {
        RoR2Application.onLoadFinished -= OnGameLoaded;
        foreach (var label in rendered.Keys.ToArray())
            RemoveOverlay(label);
        if (worker != null) worker.Dispose();
    }

    private void RemoveOverlay(TextMeshProUGUI label)
    {
        RawImage image;
        if (!rendered.TryGetValue(label, out image)) return;
        rendered.Remove(label);
        if (image != null) Destroy(image.gameObject);
        if (label != null) label.canvasRenderer.SetAlpha(originalAlpha[label]);
        originalAlpha.Remove(label);
    }

    private void LoadFlag(string assets, string lang)
    {
        var file = Path.Combine(assets, "flag-" + lang + ".png");
        if (!File.Exists(file))
        {
            Logger.LogWarning("Bandeira ausente: " + lang);
            return;
        }

        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!ImageConversion.LoadImage(texture, File.ReadAllBytes(file), true))
        {
            Logger.LogWarning("Bandeira invalida: " + lang);
            Destroy(texture);
            return;
        }

        texture.wrapMode = TextureWrapMode.Clamp;
        flags.Add(lang, Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect));
    }

    public static void BeforeSwitch(string __0)
    {
        if (Array.IndexOf(Codes, __0) >= 0)
            EnsureRegistered();
    }

    public static void EnsureRegistered()
    {
        try
        {
            var dict = getLanguages.Invoke(null, null) as Dictionary<string, Language>;
            if (dict == null) return;
            for (int i = 0; i < Codes.Length; i++)
            {
                var lang = Codes[i];
                Language language;
                if (!dict.TryGetValue(lang, out language))
                {
                    language = create.Invoke(null, new object[] { lang }) as Language;
                    if (language == null) throw new InvalidOperationException(lang);
                    dict[lang] = language;
                    instance.Logger.LogInfo("Idioma registrado no seletor: " + lang);
                }
                if (selfName != null && string.IsNullOrEmpty(language.selfName))
                    selfName.SetValue(language, DisplayNames[i]);
                Sprite flag;
                if (iconSprite != null && language.iconSprite == null && instance.flags.TryGetValue(lang, out flag))
                {
                    iconSprite.SetValue(language, flag);
                    instance.Logger.LogInfo("Bandeira registrada no seletor: " + lang);
                }
            }
        }
        catch (Exception error) { instance.Logger.LogError(error); }
    }

    private void LateUpdate()
    {
        if (!ready || Time.unscaledTime < next || Language.currentLanguage == null) return;
        next = Time.unscaledTime + 0.25f;
        var lang = Language.currentLanguage.name;
        if (worker != null) worker.Update(Time.unscaledTime);
        if (lastLanguage != lang)
        {
            Logger.LogInfo("Idioma ativo: " + lang);
            lastLanguage = lang;
            EnsureRegistered();
            foreach (var label in rendered.Keys.ToArray()) RemoveOverlay(label);
        }
        Dictionary<string,Texture2D> dictionary;
        if (!pictures.TryGetValue(lang, out dictionary)) return;
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

    private static bool HasNewScript(string value, string language)
    {
        if (language == "ar") return value.Any(c => c >= '\u0600' && c <= '\u06ff');
        if (language == "hi") return value.Any(c => c >= '\u0900' && c <= '\u097f');
        return value.Any(c => c >= '\u0e00' && c <= '\u0e7f');
    }
}
