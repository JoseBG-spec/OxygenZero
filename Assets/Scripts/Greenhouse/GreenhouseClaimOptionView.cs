using System;
using MarsOutpost.Core;
using MarsOutpost.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsOutpost.Greenhouse
{
    /// <summary>
    /// One tappable claim in the notebook. A filled radio and a filled card show the
    /// choice, and the statement text is always visible.
    /// </summary>
    public sealed class GreenhouseClaimOptionView : MonoBehaviour
    {
        public Button button;
        public Image background;
        public Image radioOuter;
        public Image radioDot;
        public TMP_Text statement;

        public GreenhouseClaimDefinition Claim { get; private set; }

        private Action<GreenhouseClaimDefinition> onSelected;
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

        public void Bind(GreenhouseClaimDefinition claim, Action<GreenhouseClaimDefinition> selected)
        {
            Claim = claim;
            onSelected = selected;

            if (statement != null)
            {
                statement.text = claim != null ? claim.statement : string.Empty;
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
        }

        private void HandleClick()
        {
            if (Claim != null)
            {
                onSelected?.Invoke(Claim);
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

            if (radioOuter != null)
            {
                radioOuter.color = selected ? theme.primaryText : theme.textSecondary;
            }

            if (radioDot != null)
            {
                radioDot.color = theme.primaryText;
                radioDot.gameObject.SetActive(selected);
            }

            if (statement != null)
            {
                statement.enableAutoSizing = true;
                statement.fontSizeMin = 20f;
                statement.overflowMode = TextOverflowModes.Ellipsis;

                Color statementColor = selected ? theme.primaryText : theme.textPrimary;
                var themed = statement.GetComponent<ThemedText>();
                if (themed != null)
                {
                    themed.overrideColor = true;
                    themed.colorOverride = statementColor;
                    themed.Apply();
                }

                // Fixed-height row: cap the claim size so the full sentence stays visible at 130% text.
                statement.fontSizeMax = 24f;
                statement.fontSize = Mathf.Min(24f, Mathf.Max(20f, statement.fontSize));
                statement.color = statementColor;
            }
        }
    }
}
