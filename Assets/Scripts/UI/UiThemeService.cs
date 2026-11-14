using System;
using MarsOutpost.Core;
using TMPro;
using UnityEngine;

namespace MarsOutpost.UI
{
    /// <summary>
    /// Runtime theme selection plus two accessibility settings that persist on the device.
    /// Text scale is capped so mobile layouts keep fitting.
    /// </summary>
    public static class UiThemeService
    {
        private const string FontScaleKey = "marsoutpost.fontScale";
        private const string HighContrastKey = "marsoutpost.highContrast";

        public static UiTheme BaseTheme { get; private set; }
        public static UiTheme ContrastTheme { get; private set; }
        public static float FontScale { get; private set; } = 1f;
        public static bool HighContrast { get; private set; }

        public static event Action ThemeChanged;

        public static UiTheme ActiveTheme =>
            HighContrast && ContrastTheme != null ? ContrastTheme : BaseTheme;

        public static TMP_FontAsset Font
        {
            get
            {
                if (ActiveTheme != null && ActiveTheme.font != null)
                {
                    return ActiveTheme.font;
                }

                try
                {
                    return TMP_Settings.defaultFontAsset;
                }
                catch (NullReferenceException)
                {
                    return null;
                }
            }
        }

        public static void Initialize(MissionConfig config)
        {
            BaseTheme = config != null ? config.theme : null;
            ContrastTheme = config != null ? config.highContrastTheme : null;
            FontScale = Mathf.Clamp(PlayerPrefs.GetFloat(FontScaleKey, 1f), 1f, 1.35f);
            HighContrast = PlayerPrefs.GetInt(HighContrastKey, 0) == 1;
            ThemeChanged?.Invoke();
        }

        public static void CycleFontScale()
        {
            if (FontScale < 1.12f)
            {
                FontScale = 1.15f;
            }
            else if (FontScale < 1.22f)
            {
                FontScale = 1.3f;
            }
            else
            {
                FontScale = 1f;
            }

            PlayerPrefs.SetFloat(FontScaleKey, FontScale);
            PlayerPrefs.Save();
            ThemeChanged?.Invoke();
        }

        public static void ToggleHighContrast()
        {
            HighContrast = !HighContrast;
            PlayerPrefs.SetInt(HighContrastKey, HighContrast ? 1 : 0);
            PlayerPrefs.Save();
            ThemeChanged?.Invoke();
        }

        public static string FontScaleLabel => $"{Mathf.RoundToInt(FontScale * 100f)}%";

        public static float Scaled(float baseSize) => baseSize * FontScale;

        public static Color ColorForDelta(int delta)
        {
            UiTheme theme = ActiveTheme;
            if (theme == null)
            {
                return delta >= 0 ? Color.green : Color.red;
            }

            if (delta > 0)
            {
                return theme.success;
            }

            return delta < 0 ? theme.danger : theme.textSecondary;
        }
    }
}
