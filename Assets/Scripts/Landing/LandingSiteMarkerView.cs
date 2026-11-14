using System;
using MarsOutpost.Core;
using MarsOutpost.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsOutpost.Landing
{
    /// <summary>
    /// A numbered map marker. Keeps its own selection ring so the map stays readable
    /// at a glance, and refreshes colors when the high-contrast theme changes.
    /// </summary>
    public sealed class LandingSiteMarkerView : MonoBehaviour
    {
        public Button button;
        public Image ring;
        public Image background;
        public TMP_Text numberLabel;
        public TMP_Text nameLabel;

        public LandingSiteDefinition Site { get; private set; }

        private Action<LandingSiteDefinition> onSelected;
        private bool selected;

        private void OnEnable()
        {
            UiThemeService.ThemeChanged += ApplyTheme;
            ApplyTheme();
        }

        private void OnDisable()
        {
            UiThemeService.ThemeChanged -= ApplyTheme;
        }

        public void Bind(LandingSiteDefinition site, int number, Action<LandingSiteDefinition> selectedCallback)
        {
            Site = site;
            onSelected = selectedCallback;

            if (numberLabel != null)
            {
                numberLabel.text = number.ToString("00");
            }

            if (nameLabel != null)
            {
                nameLabel.text = site != null ? site.displayName : string.Empty;
            }

            if (button != null)
            {
                button.onClick.AddListener(HandleClick);
            }

            SetSelected(false);
        }

        public void SetSelected(bool value)
        {
            selected = value;
            if (ring != null)
            {
                ring.enabled = value;
            }

            ApplyTheme();
        }

        public void SetInteractable(bool value)
        {
            if (button != null)
            {
                button.interactable = value;
            }
        }

        private void HandleClick()
        {
            if (Site != null)
            {
                onSelected?.Invoke(Site);
            }
        }

        private void ApplyTheme()
        {
            UiTheme theme = UiThemeService.ActiveTheme;
            if (theme == null)
            {
                return;
            }

            if (background != null)
            {
                background.color = selected ? theme.primary : theme.panelAlt;
            }

            if (ring != null)
            {
                ring.color = theme.textPrimary;
            }

            if (numberLabel != null)
            {
                numberLabel.font = UiThemeService.Font;
                numberLabel.fontSize = Mathf.Round(UiThemeService.Scaled(theme.headingSize * 0.62f));
                numberLabel.color = selected ? theme.primaryText : theme.textPrimary;
            }
        }
    }
}
