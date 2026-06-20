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

internal static class LanguagePauseMenu
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
            RegisterUiTokens();

            var panel = self.mainPanel;
            if (panel == null || panel.childCount == 0) return;

            var firstButton = panel.GetChild(0).GetChild(1);
            if (firstButton == null) return;

            var newButton = Object.Instantiate(firstButton, panel.GetChild(0));
            newButton.name = "LanguageSwitcherButton";

            var ctrl = newButton.GetComponentInChildren<LanguageTextMeshController>();
            if (ctrl != null)
                ctrl.token = "PELE_LANGUAGE_BUTTON";

            var oldText = newButton.GetComponentInChildren<HGTextMeshProUGUI>();
            if (oldText != null)
                oldText.text = Language.GetString("PELE_LANGUAGE_BUTTON");

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

    private static void RegisterUiTokens()
    {
        LanguageAPI.AddOrUpdateToken("PELE_LANGUAGE_BUTTON", "Language");
        LanguageAPI.AddOrUpdateToken("PELE_LANGUAGE_BUTTON", "Idioma", "pt-BR");
        LanguageAPI.AddOrUpdateToken("PELE_LANGUAGE_BUTTON", "Lingua", "la");
        LanguageAPI.AddOrUpdateToken("PELE_LANGUAGE_BUTTON", "Lingvo", "eo");
        LanguageAPI.AddOrUpdateToken("PELE_LANGUAGE_BUTTON", "Мова", "uk");
        LanguageAPI.AddOrUpdateToken("PELE_LANGUAGE_BUTTON", "Sprache", "de");
        LanguageAPI.AddOrUpdateToken("PELE_LANGUAGE_BUTTON", "Idioma", "es-ES");
        LanguageAPI.AddOrUpdateToken("PELE_LANGUAGE_BUTTON", "Idioma", "es-419");
        LanguageAPI.AddOrUpdateToken("PELE_LANGUAGE_BUTTON", "Langue", "fr");
        LanguageAPI.AddOrUpdateToken("PELE_LANGUAGE_BUTTON", "Lingua", "it");
        LanguageAPI.AddOrUpdateToken("PELE_LANGUAGE_BUTTON", "言語", "ja");
        LanguageAPI.AddOrUpdateToken("PELE_LANGUAGE_BUTTON", "언어", "ko");
        LanguageAPI.AddOrUpdateToken("PELE_LANGUAGE_BUTTON", "Язык", "ru");
        LanguageAPI.AddOrUpdateToken("PELE_LANGUAGE_BUTTON", "Dil", "tr");
        LanguageAPI.AddOrUpdateToken("PELE_LANGUAGE_BUTTON", "语言", "zh-CN");

        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_TITLE", "Select Language");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_TITLE", "Selecionar idioma", "pt-BR");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_TITLE", "Elige linguam", "la");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_TITLE", "Elekti lingvon", "eo");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_TITLE", "Вибрати мову", "uk");

        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_DESC", "Choose a language below. The change applies immediately.");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_DESC", "Escolha um idioma abaixo. A troca e aplicada imediatamente.", "pt-BR");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_DESC", "Elige linguam infra. Mutatio statim fit.", "la");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_DESC", "Elektu lingvon sube. La sxangxo validas tuj.", "eo");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_DESC", "Оберіть мову нижче. Зміна застосовується одразу.", "uk");

        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_CANCEL", "Cancel");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_CANCEL", "Cancelar", "pt-BR");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_CANCEL", "Abrogare", "la");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_CANCEL", "Nuligi", "eo");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_CANCEL", "Скасувати", "uk");

        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_APPLY_HINT", "Apply");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_APPLY_HINT", "Aplicar", "pt-BR");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_APPLY_HINT", "Applicare", "la");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_APPLY_HINT", "Apliki", "eo");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_APPLY_HINT", "Застосувати", "uk");

        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_CANCEL_HINT", "Cancel");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_CANCEL_HINT", "Cancelar", "pt-BR");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_CANCEL_HINT", "Abrogare", "la");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_CANCEL_HINT", "Nuligi", "eo");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_CANCEL_HINT", "Скасувати", "uk");

        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_MOUSE_APPLY_HINT", "Left click");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_MOUSE_APPLY_HINT", "Clique esquerdo", "pt-BR");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_MOUSE_APPLY_HINT", "Clic sinistro", "la");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_MOUSE_APPLY_HINT", "Maldekstra klako", "eo");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_MOUSE_APPLY_HINT", "Ліва кнопка", "uk");

        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_ESC_HINT", "Esc");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_ESC_HINT", "Esc", "pt-BR");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_ESC_HINT", "Esc", "la");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_ESC_HINT", "Esc", "eo");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_ESC_HINT", "Esc", "uk");
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
        RegisterUiTokens();

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

            AddInputLegend(dialog);
            LayoutRebuilder.ForceRebuildLayoutImmediate(container);
            WrapGridNavigation(container);

            LanguagePlugin.Logger?.LogInfo($"ApplyGridLayout: grid aplicado com {container.childCount} botoes");
        }
        catch (Exception ex)
        {
            LanguagePlugin.Logger?.LogError("Falha ao aplicar grid layout: " + ex.Message);
        }
    }

    private static void WrapGridNavigation(RectTransform container)
    {
        var buttons = new List<MPButton>();
        foreach (var button in container.GetComponentsInChildren<MPButton>())
        {
            if (button.GetComponentInParent<LanguageInputLegendController>() != null) continue;
            buttons.Add(button);
        }
        if (buttons.Count == 0) return;

        int cols = Mathf.Min(3, buttons.Count);
        int rows = (buttons.Count + cols - 1) / cols;

        for (int i = 0; i < buttons.Count; i++)
        {
            var nav = new Navigation { mode = Navigation.Mode.Explicit };

            int col = i % cols;
            int row = i / cols;

            int left  = col > 0 ? i - 1 : i + cols - 1;
            int right = col < cols - 1 ? i + 1 : i - col;
            int up    = row > 0
                ? i - cols
                : (rows - 1) * cols + Mathf.Min(col, buttons.Count - (rows - 1) * cols - 1);
            int down  = row < rows - 1
                ? Mathf.Min(i + cols, buttons.Count - 1)
                : col;

            if (left  >= 0 && left  < buttons.Count) nav.selectOnLeft  = buttons[left];
            if (right >= 0 && right < buttons.Count) nav.selectOnRight = buttons[right];
            if (up    >= 0 && up    < buttons.Count) nav.selectOnUp    = buttons[up];
            if (down  >= 0 && down  < buttons.Count) nav.selectOnDown  = buttons[down];

            buttons[i].navigation = nav;
        }
    }

    private static void AddInputLegend(SimpleDialogBox dialog)
    {
        var container = dialog?.buttonContainer;
        if (container == null) return;
        if (container.Find("PELELanguageInputLegend") != null) return;

        var legend = new GameObject("PELELanguageInputLegend", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LanguageInputLegendController));
        var legendRect = legend.GetComponent<RectTransform>();
        legendRect.SetParent(container, false);
        legendRect.sizeDelta = new Vector2(616f, 42f);

        var legendLayout = legend.AddComponent<LayoutElement>();
        legendLayout.minWidth = 616f;
        legendLayout.preferredWidth = 616f;
        legendLayout.minHeight = 42f;
        legendLayout.preferredHeight = 42f;
        legendLayout.flexibleWidth = 0f;
        legendLayout.flexibleHeight = 0f;

        var layout = legend.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 58f;
        layout.padding = new RectOffset(8, 0, 8, 0);
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;

        var controller = legend.GetComponent<LanguageInputLegendController>();
        controller.eventSystem = EventSystem.current as MPEventSystem;
        controller.applyGlyph = AddLegendItem(legendRect, "UISubmit", "SWITCH_LANGUAGE_MOUSE_APPLY_HINT", "SWITCH_LANGUAGE_APPLY_HINT");
        controller.cancelGlyph = AddLegendItem(legendRect, "UICancel", "SWITCH_LANGUAGE_ESC_HINT", "SWITCH_LANGUAGE_CANCEL_HINT");
    }

    private static InputBindingDisplayController AddLegendItem(RectTransform parent, string actionName, string keyToken, string textToken)
    {
        var item = new GameObject(actionName + "Legend", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        var itemRect = item.GetComponent<RectTransform>();
        itemRect.SetParent(parent, false);
        itemRect.sizeDelta = new Vector2(270f, 30f);

        var layout = item.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 7f;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;

        var itemLayout = item.AddComponent<LayoutElement>();
        itemLayout.minWidth = 270f;
        itemLayout.preferredWidth = 270f;
        itemLayout.minHeight = 30f;
        itemLayout.preferredHeight = 30f;

        var glyphObject = new GameObject("Glyph", typeof(RectTransform));
        glyphObject.SetActive(false);
        var glyphRect = glyphObject.GetComponent<RectTransform>();
        glyphRect.SetParent(itemRect, false);
        glyphRect.sizeDelta = new Vector2(28f, 28f);

        var glyphText = glyphObject.AddComponent<HGTextMeshProUGUI>();
        glyphText.fontSize = 18f;
        glyphText.alignment = TextAlignmentOptions.Center;

        var glyph = glyphObject.AddComponent<InputBindingDisplayController>();
        glyph.actionName = actionName;
        glyph.useExplicitInputSource = true;
        glyph.explicitInputSource = MPEventSystem.InputSource.Gamepad;
        glyphObject.SetActive(true);

        var keyObject = new GameObject("KeyFallback", typeof(RectTransform));
        keyObject.SetActive(false);
        var keyRect = keyObject.GetComponent<RectTransform>();
        keyRect.SetParent(itemRect, false);
        keyRect.sizeDelta = new Vector2(158f, 28f);

        var keyText = keyObject.AddComponent<HGTextMeshProUGUI>();
        keyText.fontSize = 14f;
        keyText.alignment = TextAlignmentOptions.MidlineLeft;
        keyText.enableWordWrapping = false;

        var keyLang = keyObject.AddComponent<LanguageTextMeshController>();
        keyLang.token = keyToken;
        keyObject.SetActive(true);

        var labelObject = new GameObject("Label", typeof(RectTransform));
        labelObject.SetActive(false);
        var labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.SetParent(itemRect, false);
        labelRect.sizeDelta = new Vector2(76f, 28f);

        var labelText = labelObject.AddComponent<HGTextMeshProUGUI>();
        labelText.fontSize = 14f;
        labelText.alignment = TextAlignmentOptions.MidlineLeft;
        labelText.enableWordWrapping = false;

        var lang = labelObject.AddComponent<LanguageTextMeshController>();
        lang.token = textToken;
        labelObject.SetActive(true);

        return glyph;
    }

    private class LanguageInputLegendController : MonoBehaviour
    {
        public MPEventSystem eventSystem;
        public InputBindingDisplayController applyGlyph;
        public InputBindingDisplayController cancelGlyph;

        private void Update()
        {
            var gamepad = eventSystem != null && eventSystem.currentInputSource == MPEventSystem.InputSource.Gamepad;
            SetGlyphVisible(applyGlyph, gamepad);
            SetGlyphVisible(cancelGlyph, gamepad);
        }

        private static void SetGlyphVisible(InputBindingDisplayController glyph, bool visible)
        {
            if (glyph == null) return;

            var glyphText = glyph.GetComponent<HGTextMeshProUGUI>();
            if (glyphText != null)
                glyphText.enabled = visible;

            var fallback = glyph.transform.parent.Find("KeyFallback");
            if (fallback != null)
                fallback.gameObject.SetActive(!visible);
        }
    }
}
