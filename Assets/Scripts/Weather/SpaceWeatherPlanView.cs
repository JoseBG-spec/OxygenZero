using System;
using MarsOutpost.Core;
using MarsOutpost.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsOutpost.Weather
{
    /// <summary>
    /// One tappable crew plan. A filled card, a badge, and a text state label make the
    /// selection clear without relying on color alone.
    /// </summary>
    public sealed class SpaceWeatherPlanView : MonoBehaviour
    {
        public Button button;
        public Image background;
        public Image badge;
        public TMP_Text indexLabel;
        public TMP_Text titleLabel;
        public TMP_Text descriptionLabel;
        public TMP_Text stateLabel;

        public SpaceWeatherPlanDefinition Plan { get; private set; }

        private Action<SpaceWeatherPlanDefinition> onSelected;
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

        public void Bind(SpaceWeatherPlanDefinition plan, Action<SpaceWeatherPlanDefinition> selectedCallback)
        {
            Plan = plan;
            onSelected = selectedCallback;

            if (indexLabel != null)
            {
                indexLabel.text = "?";
            }

            if (titleLabel != null)
            {
                titleLabel.text = plan != null ? plan.title : string.Empty;
            }

            if (descriptionLabel != null)
            {
                descriptionLabel.text = plan != null ? plan.description : string.Empty;
            }

            if (button != null)
            {
                button.onClick.AddListener(HandleClick);
                ColorBlock colors = button.colors;
                colors.disabledColor = Color.white;
                button.colors = colors;
            }

            SetSelected(false);
        }

        public void SetSelected(bool value)
        {
            selected = value;
            ApplyTheme();
        }

        public void SetInteractable(bool value)
        {
            if (button != null)
            {
                button.interactable = value;
            }

            ApplyTheme();
        }

        private void HandleClick()
        {
            if (Plan != null)
            {
                onSelected?.Invoke(Plan);
            }
        }

        private void ApplyTheme()
        {
            UiTheme theme = UiThemeService.ActiveTheme;
            if (theme == null)
            {
                return;
            }

            bool interactable = button == null || button.IsInteractable();
            Color cardColor = selected ? theme.primary : theme.panelAlt;
            if (!interactable)
            {
                cardColor = Color.Lerp(theme.background, cardColor, 0.55f);
            }

            if (background != null)
            {
                background.color = cardColor;
            }

            if (badge != null)
            {
                badge.color = selected ? theme.primaryText : theme.primary;
            }

            Color titleColor = selected ? theme.primaryText : theme.textPrimary;
            Color bodyColor = selected ? theme.primaryText : theme.textSecondary;
            if (!interactable)
            {
                titleColor.a = 0.62f;
                bodyColor.a = 0.62f;
            }

            if (indexLabel != null)
            {
                indexLabel.font = UiThemeService.Font;
                indexLabel.fontSize = Mathf.Min(30f, Mathf.Round(UiThemeService.Scaled(theme.headingSize) * 0.65f));
                indexLabel.color = selected ? theme.primary : theme.primaryText;
            }

            if (titleLabel != null)
            {
                titleLabel.font = UiThemeService.Font;
                titleLabel.enableAutoSizing = true;
                titleLabel.fontSizeMin = 20f;
                // Fixed card height: cap the title so it never pushes into the description.
                titleLabel.fontSizeMax = Mathf.Min(32f, Mathf.Round(UiThemeService.Scaled(theme.bodySize)));
                titleLabel.fontSize = titleLabel.fontSizeMax;
                titleLabel.color = titleColor;
                titleLabel.overflowMode = TextOverflowModes.Ellipsis;
            }

            if (descriptionLabel != null)
            {
                descriptionLabel.font = UiThemeService.Font;
                descriptionLabel.enableAutoSizing = true;
                descriptionLabel.fontSizeMin = 16f;
                // Fixed card height: cap the caption so two lines always fit at 130% text.
                descriptionLabel.fontSizeMax = Mathf.Min(23f, Mathf.Round(UiThemeService.Scaled(theme.captionSize)));
                descriptionLabel.fontSize = descriptionLabel.fontSizeMax;
                descriptionLabel.color = bodyColor;
                descriptionLabel.overflowMode = TextOverflowModes.Ellipsis;
            }

            if (stateLabel != null)
            {
                stateLabel.font = UiThemeService.Font;
                stateLabel.fontSize = Mathf.Min(20f, Mathf.Round(UiThemeService.Scaled(theme.captionSize)));
                stateLabel.color = selected ? theme.primaryText : theme.textSecondary;
                if (!interactable && !selected)
                {
                    stateLabel.color = new Color(stateLabel.color.r, stateLabel.color.g, stateLabel.color.b, 0.62f);
                }

                stateLabel.text = selected ? "CHOSEN" : interactable ? "TAP" : "LOCKED";
            }
        }
    }
}
