using MarsOutpost.Core;
using TMPro;
using UnityEngine;

namespace MarsOutpost.UI
{
    public enum TextRole
    {
        Title,
        Heading,
        Body,
        Button,
        Caption
    }

    /// <summary>
    /// Applies theme font, color and scaled size to a TMP label. Add this to any text
    /// you want to respond to the "Aa" and high-contrast controls.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public sealed class ThemedText : MonoBehaviour
    {
        public TextRole role = TextRole.Body;
        public bool useSecondaryColor;
        public bool overrideColor;
        public Color colorOverride = Color.white;

        private TMP_Text text;

        private void Awake()
        {
            text = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            UiThemeService.ThemeChanged += Apply;
            Apply();
        }

        private void OnDisable()
        {
            UiThemeService.ThemeChanged -= Apply;
        }

        public void Apply()
        {
            if (text == null)
            {
                text = GetComponent<TMP_Text>();
            }

            if (text == null)
            {
                return;
            }

            TMP_FontAsset font = UiThemeService.Font;
            if (font != null)
            {
                text.font = font;
            }

            UiTheme theme = UiThemeService.ActiveTheme;
            if (theme == null)
            {
                return;
            }

            float baseSize;
            switch (role)
            {
                case TextRole.Title: baseSize = theme.titleSize; break;
                case TextRole.Heading: baseSize = theme.headingSize; break;
                case TextRole.Button: baseSize = theme.buttonSize; break;
                case TextRole.Caption: baseSize = theme.captionSize; break;
                default: baseSize = theme.bodySize; break;
            }

            text.fontSize = Mathf.Max(14f, Mathf.Round(UiThemeService.Scaled(baseSize)));
            text.color = overrideColor
                ? colorOverride
                : useSecondaryColor ? theme.textSecondary : theme.textPrimary;
        }
    }
}
