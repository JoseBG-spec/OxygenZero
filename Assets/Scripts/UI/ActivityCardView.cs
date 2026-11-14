using System;
using MarsOutpost.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsOutpost.UI
{
    /// <summary>One activity card on the hub. Data-driven, with touch-sized hit area.</summary>
    public sealed class ActivityCardView : MonoBehaviour
    {
        public Button button;
        public Image iconBackground;
        public TMP_Text indexLabel;
        public TMP_Text titleLabel;
        public TMP_Text taglineLabel;
        public TMP_Text statusLabel;
        public TMP_Text arrowLabel;

        public void Bind(
            ActivityDefinition activity,
            int order,
            bool completed,
            bool available,
            bool allowReplay,
            Action onSelected)
        {
            if (activity == null)
            {
                return;
            }

            if (titleLabel != null)
            {
                titleLabel.text = activity.displayName;
            }

            if (taglineLabel != null)
            {
                taglineLabel.text = string.IsNullOrWhiteSpace(activity.tagline)
                    ? activity.description
                    : activity.tagline;
            }

            if (indexLabel != null)
            {
                indexLabel.text = (order + 1).ToString("00");
            }

            if (statusLabel != null)
            {
                if (completed)
                {
                    statusLabel.text = "COMPLETE";
                }
                else if (!available)
                {
                    statusLabel.text = "UNAVAILABLE";
                }
                else
                {
                    statusLabel.text = $"{activity.estimatedMinutes} MIN";
                }
            }

            UiTheme theme = UiThemeService.ActiveTheme;
            if (iconBackground != null && theme != null)
            {
                iconBackground.sprite = theme.circle;
                iconBackground.color = theme.primary;
            }

            if (arrowLabel != null)
            {
                arrowLabel.text = available && (!completed || allowReplay) ? ">" : string.Empty;
            }

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                bool canStart = available && (!completed || allowReplay);
                button.interactable = canStart;

                if (canStart && onSelected != null)
                {
                    button.onClick.AddListener(() => onSelected());
                }
            }
        }
    }
}
