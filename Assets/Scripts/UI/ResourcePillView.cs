using MarsOutpost.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsOutpost.UI
{
    /// <summary>Compact mobile HUD pill: color-coded icon, short label, current value.</summary>
    public sealed class ResourcePillView : MonoBehaviour
    {
        public Image background;
        public Image icon;
        public TMP_Text shortLabel;
        public TMP_Text valueLabel;

        private ResourceDefinition definition;

        public void Bind(ResourceDefinition resourceDefinition, int currentValue)
        {
            definition = resourceDefinition;
            if (definition == null)
            {
                return;
            }

            if (shortLabel != null)
            {
                shortLabel.text = definition.shortLabel;
            }

            if (icon != null)
            {
                UiTheme theme = UiThemeService.ActiveTheme;
                icon.sprite = definition.icon != null
                    ? definition.icon
                    : theme != null ? theme.circle : null;
                icon.color = definition.color;
            }

            SetValue(currentValue);
        }

        public void SetValue(int currentValue)
        {
            if (valueLabel != null)
            {
                valueLabel.text = currentValue.ToString();
            }
        }

        public void RefreshTheme()
        {
            if (definition != null && icon != null)
            {
                UiTheme theme = UiThemeService.ActiveTheme;
                icon.sprite = definition.icon != null
                    ? definition.icon
                    : theme != null ? theme.circle : null;
                icon.color = definition.color;
            }
        }
    }
}
