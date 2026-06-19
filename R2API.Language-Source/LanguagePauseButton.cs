using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace R2API;

internal static class LanguagePauseButton
{
    private static bool _hooksActive;

    // Cache SetCurrentLanguage reflection once.
    private static MethodInfo _setCurrentLanguageMethod;

    internal static void Init()
    {
        if (_hooksActive) return;
        _hooksActive = true;

        _setCurrentLanguageMethod = typeof(Language).GetMethod("SetCurrentLanguage",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

        On.RoR2.UI.PauseScreenController.Awake += OnPauseAwake;
    }

    internal static void Cleanup()
    {
        if (!_hooksActive) return;
        _hooksActive = false;
        On.RoR2.UI.PauseScreenController.Awake -= OnPauseAwake;
    }

    private static void OnPauseAwake(On.RoR2.UI.PauseScreenController.orig_Awake orig, PauseScreenController self)
    {
        orig(self);
        try
        {
            var panel = self.mainPanel;
            if (panel == null || panel.childCount == 0) return;

            var firstButton = panel.GetChild(0).GetChild(1);
            if (firstButton == null) return;

            var newButton = Object.Instantiate(firstButton, panel.GetChild(0));
            newButton.name = "LanguageSwitcherButton";

            var ctrl = newButton.GetComponentInChildren<LanguageTextMeshController>();
            if (ctrl != null) Object.DestroyImmediate(ctrl);

            var oldText = newButton.GetComponentInChildren<HGTextMeshProUGUI>();
            if (oldText != null)
                oldText.text = "Language";

            var hgButton = newButton.GetComponent<HGButton>();
            hgButton.onClick = new Button.ButtonClickedEvent();
            hgButton.onClick.AddListener(() => ShowLanguageDialog(self));

            newButton.transform.SetAsLastSibling();
        }
        catch (Exception ex)
        {
            LanguagePlugin.Logger?.LogError("Falha no botao de idiomas: " + ex.Message);
        }
    }

    private static void SetLanguage(string lang)
    {
        if (_setCurrentLanguageMethod == null)
        {
            LanguagePlugin.Logger?.LogError("SetCurrentLanguage method not found via reflection");
            return;
        }
        try
        {
            if (Language.LanguageConVar.instance != null)
            {
                Language.LanguageConVar.instance.SetString(lang);
                return;
            }

            _setCurrentLanguageMethod.Invoke(null, new object[] { lang });
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            var stack = ex.InnerException?.StackTrace ?? ex.StackTrace;
            LanguagePlugin.Logger?.LogError("Falha ao trocar lingua para '" + lang + "': " + msg);
            LanguagePlugin.Logger?.LogError("  Stack: " + stack);
        }
    }

    private static void ShowLanguageDialog(PauseScreenController pause)
    {
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_TITLE", "Select Language");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_DESC",
            "Choose a language below. The change applies immediately.");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_CANCEL", "Cancel");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_APPLY_HINT", "Apply");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_CANCEL_HINT", "Cancel");

        var languages = LanguageNames.GetAvailableLanguages();
        foreach (var lang in languages)
            LanguageAPI.AddOrUpdateToken($"LANG_SEL_{lang}", LanguageNames.GetFriendlyName(lang));

        var dialog = SimpleDialogBox.Create(EventSystem.current as MPEventSystem);
        if (dialog == null) return;

        dialog.headerToken      = new SimpleDialogBox.TokenParamsPair { token = "SWITCH_LANGUAGE_TITLE" };
        dialog.descriptionToken = new SimpleDialogBox.TokenParamsPair { token = "SWITCH_LANGUAGE_DESC" };

        foreach (var lang in languages)
        {
            var captured = lang;
            dialog.AddActionButton(() =>
            {
                SetLanguage(captured);
            }, $"LANG_SEL_{lang}", destroyDialog: true);
        }

        dialog.AddCancelButton("SWITCH_LANGUAGE_CANCEL");

        var watcherHost = dialog.rootObject != null ? dialog.rootObject : dialog.gameObject;
        var watcher = watcherHost.AddComponent<LanguageDialogCloseWatcher>();
        watcher.dialog = dialog;

        var mb = pause.GetComponent<MonoBehaviour>();
        if (mb != null)
            mb.StartCoroutine(DelayedGridLayout(dialog));
        else
            ApplyGridLayout(dialog);
    }

    private class LanguageDialogCloseWatcher : MonoBehaviour
    {
        private float _delay = 0.4f;
        public SimpleDialogBox dialog;
        private bool _closing;

        void Update()
        {
            if (_closing) return;
            _delay -= Time.unscaledDeltaTime;
            if (_delay > 0f) return;

            if (Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetKeyDown(KeyCode.JoystickButton1))
            {
                _closing = true;
                CloseDialog(dialog);
            }
        }
    }

    private static void CloseDialog(SimpleDialogBox dialog)
    {
        if (dialog == null) return;
        var target = dialog.rootObject != null ? dialog.rootObject : dialog.gameObject;
        Object.Destroy(target);
    }

    private static IEnumerator DelayedGridLayout(SimpleDialogBox dialog)
    {
        yield return null;
        ApplyGridLayout(dialog);
    }

    private static void ApplyGridLayout(SimpleDialogBox dialog)
    {
        try
        {
            var container = dialog.buttonContainer;
            if (container == null)
            {
                LanguagePlugin.Logger?.LogWarning("ApplyGridLayout: buttonContainer e null");
                return;
            }

            foreach (var lg in container.GetComponents<LayoutGroup>())
            {
                lg.enabled = false;
                Object.DestroyImmediate(lg, true);
            }

            var grid = container.gameObject.AddComponent<GridLayoutGroup>();
            grid.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.cellSize        = new Vector2(200, 40);
            grid.spacing         = new Vector2(8, 6);
            grid.startAxis       = GridLayoutGroup.Axis.Horizontal;

            var csf = container.GetComponent<ContentSizeFitter>()
                   ?? container.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            LayoutRebuilder.ForceRebuildLayoutImmediate(container);
            WrapGridNavigation(container);
            AddInputLegend(dialog);

            LanguagePlugin.Logger?.LogInfo($"ApplyGridLayout: grid aplicado com {container.childCount} botoes");
        }
        catch (Exception ex)
        {
            LanguagePlugin.Logger?.LogError("Falha ao aplicar grid layout: " + ex.Message);
        }
    }

    private static void WrapGridNavigation(RectTransform container)
    {
        var buttons = container.GetComponentsInChildren<MPButton>();
        if (buttons.Length == 0) return;

        int cols = Mathf.Min(3, buttons.Length);
        int rows = (buttons.Length + cols - 1) / cols;

        for (int i = 0; i < buttons.Length; i++)
        {
            var nav = new Navigation { mode = Navigation.Mode.Explicit };

            int col = i % cols;
            int row = i / cols;

            int left  = col > 0 ? i - 1 : i + cols - 1;
            int right = col < cols - 1 ? i + 1 : i - col;
            int up    = row > 0
                ? i - cols
                : (rows - 1) * cols + Mathf.Min(col, buttons.Length - (rows - 1) * cols - 1);
            int down  = row < rows - 1
                ? Mathf.Min(i + cols, buttons.Length - 1)
                : col;

            if (left  >= 0 && left  < buttons.Length) nav.selectOnLeft  = buttons[left];
            if (right >= 0 && right < buttons.Length) nav.selectOnRight = buttons[right];
            if (up    >= 0 && up    < buttons.Length) nav.selectOnUp    = buttons[up];
            if (down  >= 0 && down  < buttons.Length) nav.selectOnDown  = buttons[down];

            buttons[i].navigation = nav;
        }
    }

    private static void AddInputLegend(SimpleDialogBox dialog)
    {
        if (dialog?.buttonContainer == null) return;

        var parent = dialog.buttonContainer.parent as RectTransform;
        if (parent == null) return;
        if (parent.Find("PELELanguageInputLegend") != null) return;

        var legend = new GameObject("PELELanguageInputLegend", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        var legendRect = legend.GetComponent<RectTransform>();
        legendRect.SetParent(parent, false);
        legendRect.anchorMin = new Vector2(1f, 0f);
        legendRect.anchorMax = new Vector2(1f, 0f);
        legendRect.pivot = new Vector2(1f, 0f);
        legendRect.anchoredPosition = new Vector2(-8f, 8f);
        legendRect.sizeDelta = new Vector2(260f, 34f);

        var layout = legend.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 12f;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;

        AddLegendItem(legendRect, "UISubmit", "SWITCH_LANGUAGE_APPLY_HINT");
        AddLegendItem(legendRect, "UICancel", "SWITCH_LANGUAGE_CANCEL_HINT");
    }

    private static void AddLegendItem(RectTransform parent, string actionName, string textToken)
    {
        var item = new GameObject(actionName + "Legend", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        var itemRect = item.GetComponent<RectTransform>();
        itemRect.SetParent(parent, false);
        itemRect.sizeDelta = new Vector2(120f, 30f);

        var layout = item.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 5f;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;

        var glyphObject = new GameObject("Glyph", typeof(RectTransform), typeof(HGTextMeshProUGUI));
        var glyphRect = glyphObject.GetComponent<RectTransform>();
        glyphRect.SetParent(itemRect, false);
        glyphRect.sizeDelta = new Vector2(28f, 28f);

        var glyphText = glyphObject.GetComponent<HGTextMeshProUGUI>();
        glyphText.fontSize = 18f;
        glyphText.alignment = TextAlignmentOptions.Center;

        var glyph = glyphObject.AddComponent<InputBindingDisplayController>();
        glyph.actionName = actionName;
        glyph.useExplicitInputSource = false;

        var labelObject = new GameObject("Label", typeof(RectTransform), typeof(HGTextMeshProUGUI), typeof(LanguageTextMeshController));
        var labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.SetParent(itemRect, false);
        labelRect.sizeDelta = new Vector2(78f, 28f);

        var labelText = labelObject.GetComponent<HGTextMeshProUGUI>();
        labelText.fontSize = 14f;
        labelText.alignment = TextAlignmentOptions.MidlineLeft;
        labelText.enableWordWrapping = false;

        var lang = labelObject.GetComponent<LanguageTextMeshController>();
        lang.token = textToken;
    }
}
