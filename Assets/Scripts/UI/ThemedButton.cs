using MarsOutpost.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsOutpost.UI
{
    /// <summary>
    /// Applies theme colors, sprite, and text size to a uGUI Button. Uses ColorTint
    /// (no animation) so it is cheap on mobile and works with touch and keyboard focus.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class ThemedButton : MonoBehaviour
    {
        public bool primary;
        public bool panelBackground;
        public bool selected;
        public Image background;
        public TMP_Text label;

        private Button button;
        private bool lastInteractable = true;

        private void Awake()
        {
            EnsureReferences();
        }

        private void OnEnable()
        {
            UiThemeService.ThemeChanged += Apply;
            Apply();
        }

        private void Update()
        {
            if (button == null)
            {
                return;
            }

            if (button.IsInteractable() != lastInteractable)
            {
                Apply();
            }
        }

        private void OnDisable()
        {
            UiThemeService.ThemeChanged -= Apply;
        }

        private void EnsureReferences()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (background == null)
            {
                background = GetComponent<Image>();
            }

            if (label == null)
            {
                label = GetComponentInChildren<TMP_Text>(true);
            }
        }

        public void Apply()
        {
            EnsureReferences();

            UiTheme theme = UiThemeService.ActiveTheme;
            if (theme == null || button == null)
            {
                return;
            }

            bool interactable = button.IsInteractable();
            lastInteractable = interactable;

            if (background != null)
            {
                background.sprite = theme.roundedButton;
                background.type = Image.Type.Sliced;

                Color baseColor = selected
                    ? theme.primary
                    : primary ? theme.primary
                    : panelBackground ? theme.panel : theme.panelAlt;
                if (!interactable)
                {
                    baseColor = Color.Lerp(theme.background, baseColor, 0.55f);
                }

                background.color = baseColor;
            }

            if (label != null)
            {
                label.font = UiThemeService.Font;
                float targetSize = Mathf.Max(14f, Mathf.Round(UiThemeService.Scaled(theme.buttonSize)));
                label.enableAutoSizing = true;
                label.fontSizeMin = 12f;
                label.fontSizeMax = targetSize;
                label.fontSize = targetSize;
                label.textWrappingMode = TextWrappingModes.NoWrap;

                Color labelColor = primary || selected ? theme.primaryText : theme.textPrimary;
                if (!interactable)
                {
                    labelColor.a = 0.62f;
                }

                label.color = labelColor;
            }

            button.transition = Selectable.Transition.ColorTint;
            button.targetGraphic = background;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
            colors.pressedColor = new Color(0.82f, 0.86f, 0.94f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = Color.white;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
        }
    }
}
