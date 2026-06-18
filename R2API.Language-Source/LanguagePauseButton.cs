using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace R2API;

internal static class LanguagePauseButton
{
    private static bool _hooksActive;

    internal static void Init()
    {
        if (_hooksActive) return;
        _hooksActive = true;
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

            var ctrl = newButton.GetComponentInChildren<RoR2.UI.LanguageTextMeshController>();
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

    private static void ShowLanguageDialog(PauseScreenController pause)
    {
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_TITLE", "Select Language");
        LanguageAPI.AddOrUpdateToken("SWITCH_LANGUAGE_DESC",
            "Choose a language below. The change applies immediately.\n\n" +
            "[A / Enter] Select    [B / Esc] Cancel");

        var languages = LanguageNames.GetAvailableLanguages();
        foreach (var lang in languages)
            LanguageAPI.AddOrUpdateToken($"LANG_SEL_{lang}", LanguageNames.GetFriendlyName(lang));

        var dialog = SimpleDialogBox.Create();
        if (dialog == null) return;

        dialog.headerToken = new SimpleDialogBox.TokenParamsPair { token = "SWITCH_LANGUAGE_TITLE" };
        dialog.descriptionToken = new SimpleDialogBox.TokenParamsPair { token = "SWITCH_LANGUAGE_DESC" };

        foreach (var lang in languages)
        {
            var captured = lang;
            dialog.AddActionButton(() =>
            {
                try
                {
                    var method = typeof(Language).GetMethod("SetCurrentLanguage", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                    method?.Invoke(null, new object[] { captured });
                }
                catch (Exception ex)
                {
                    LanguagePlugin.Logger?.LogError("Falha ao trocar lingua: " + ex.Message);
                }
                Object.Destroy(dialog.gameObject);
            }, $"LANG_SEL_{lang}");
        }

        dialog.gameObject.AddComponent<LanguageDialogCloseWatcher>();

        var mb = pause.GetComponent<MonoBehaviour>();
        if (mb != null)
            mb.StartCoroutine(DelayedGridLayout(dialog));
        else
            ApplyGridLayout(dialog);
    }

    private class LanguageDialogCloseWatcher : MonoBehaviour
    {
        private float _delay = 0.4f;
        private SimpleDialogBox _dialog;
        private bool _closing;

        void Start()
        {
            _dialog = GetComponent<SimpleDialogBox>();
        }

        void Update()
        {
            if (_closing) return;
            _delay -= Time.unscaledDeltaTime;
            if (_delay > 0f) return;

            if (Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetKeyDown(KeyCode.JoystickButton1))
            {
                _closing = true;
                if (_dialog != null)
                {
                    var es = UnityEngine.EventSystems.EventSystem.current;
                    if (es != null)
                        es.SetSelectedGameObject(null);
                    Object.Destroy(_dialog.gameObject);
                }
            }
        }
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
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.cellSize = new Vector2(200, 40);
            grid.spacing = new Vector2(8, 6);
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;

            var csf = container.GetComponent<ContentSizeFitter>();
            if (csf == null)
                csf = container.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

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
        var buttons = container.GetComponentsInChildren<MPButton>();
        if (buttons.Length == 0) return;

        int cols = Mathf.Min(3, buttons.Length);
        int rows = (buttons.Length + cols - 1) / cols;

        for (int i = 0; i < buttons.Length; i++)
        {
            var nav = new Navigation();
            nav.mode = Navigation.Mode.Explicit;

            int col = i % cols;
            int row = i / cols;

            int left = col > 0 ? i - 1 : i + cols - 1;
            int right = col < cols - 1 ? i + 1 : i - col;
            int up = row > 0 ? i - cols : (rows - 1) * cols + Mathf.Min(col, buttons.Length - (rows - 1) * cols - 1);
            int down = row < rows - 1 ? Mathf.Min(i + cols, buttons.Length - 1) : col;

            if (left >= 0 && left < buttons.Length) nav.selectOnLeft = buttons[left];
            if (right >= 0 && right < buttons.Length) nav.selectOnRight = buttons[right];
            if (up >= 0 && up < buttons.Length) nav.selectOnUp = buttons[up];
            if (down >= 0 && down < buttons.Length) nav.selectOnDown = buttons[down];

            buttons[i].navigation = nav;
        }
    }
}
