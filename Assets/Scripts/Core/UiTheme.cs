using TMPro;
using UnityEngine;

namespace MarsOutpost.Core
{
    /// <summary>
    /// Colors, type sizes, and 9-sliced sprites for all screens. Swap sprites here to
    /// re-skin the game without touching UI code.
    /// </summary>
    [CreateAssetMenu(fileName = "UiTheme_", menuName = "Mars Outpost/UI Theme", order = 30)]
    public sealed class UiTheme : ScriptableObject
    {
        [Header("Colors")]
        public Color background = new Color(0.043f, 0.063f, 0.149f, 1f);
        public Color backgroundAccent = new Color(0.09f, 0.12f, 0.26f, 1f);
        public Color panel = new Color(0.082f, 0.11f, 0.23f, 1f);
        public Color panelAlt = new Color(0.118f, 0.153f, 0.314f, 1f);
        public Color primary = new Color(1f, 0.718f, 0.012f, 1f);
        public Color primaryText = new Color(0.09f, 0.07f, 0.02f, 1f);
        public Color textPrimary = new Color(0.96f, 0.972f, 1f, 1f);
        public Color textSecondary = new Color(0.663f, 0.702f, 0.839f, 1f);
        public Color success = new Color(0.29f, 0.871f, 0.502f, 1f);
        public Color warning = new Color(0.984f, 0.749f, 0.141f, 1f);
        public Color danger = new Color(0.973f, 0.443f, 0.443f, 1f);
        public Color focusRing = new Color(0.56f, 0.4f, 1f, 1f);

        [Header("Typography (pixels at 1920x1080 reference)")]
        public TMP_FontAsset font;
        public float titleSize = 96f;
        public float headingSize = 48f;
        public float bodySize = 30f;
        public float buttonSize = 34f;
        public float captionSize = 22f;

        [Header("9-sliced sprites (generated placeholders are fine)")]
        public Sprite roundedPanel;
        public Sprite roundedButton;
        public Sprite roundedPill;
        public Sprite circle;

        public Color ColorForRole(PanelRole role)
        {
            switch (role)
            {
                case PanelRole.Background: return background;
                case PanelRole.Panel: return panel;
                case PanelRole.PanelAlt: return panelAlt;
                case PanelRole.Primary: return primary;
                case PanelRole.Success: return success;
                case PanelRole.Warning: return warning;
                case PanelRole.Danger: return danger;
                default: return panel;
            }
        }
    }

    public enum PanelRole
    {
        Background,
        Panel,
        PanelAlt,
        Primary,
        Success,
        Warning,
        Danger
    }
}
