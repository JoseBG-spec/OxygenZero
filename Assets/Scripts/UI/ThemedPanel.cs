using MarsOutpost.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MarsOutpost.UI
{
    /// <summary>Applies a theme color and rounded 9-sliced sprite to a panel image.</summary>
    [RequireComponent(typeof(Image))]
    public sealed class ThemedPanel : MonoBehaviour
    {
        public PanelRole role = PanelRole.Panel;
        [Tooltip("Leave on for rounded cards. Turn off for full-screen backgrounds.")]
        public bool useRoundedSprite = true;

        private Image image;

        private void Awake()
        {
            image = GetComponent<Image>();
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
            if (image == null)
            {
                image = GetComponent<Image>();
            }

            UiTheme theme = UiThemeService.ActiveTheme;
            if (image == null || theme == null)
            {
                return;
            }

            image.color = theme.ColorForRole(role);

            if (!useRoundedSprite)
            {
                image.sprite = null;
                image.type = Image.Type.Simple;
                return;
            }

            Sprite sprite = role == PanelRole.Primary ? theme.roundedButton : theme.roundedPanel;
            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = Image.Type.Sliced;
            }
        }
    }
}
