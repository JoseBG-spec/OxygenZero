using System;
using MarsOutpost.Core;
using MarsOutpost.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsOutpost.Greenhouse
{
    public enum GreenhouseTreatment
    {
        None,
        RedLight,
        Dark
    }

    /// <summary>
    /// One seedling tray card. The treatment badge, lamp, and qualitative signal rows
    /// always pair color with a text label, so color is never the only cue.
    /// </summary>
    public sealed class GreenhouseTrayView : MonoBehaviour
    {
        public TMP_Text titleLabel;
        public TMP_Text treatmentLabel;
        public Image lampGlow;
        public Image lampCore;
        public Image plantImage;

        [Header("Treatment buttons")]
        public Button redButton;
        public Button darkButton;
        public TMP_Text redButtonLabel;
        public TMP_Text darkButtonLabel;

        [Header("Simulated observations")]
        public GameObject observationGroup;
        public TMP_Text growthLabel;
        public TMP_Text growthValue;
        public Image growthDot;
        public TMP_Text stressLabel;
        public TMP_Text stressValue;
        public Image stressDot;

        public string TrayId { get; private set; }
        public GreenhouseTreatment Treatment { get; private set; }

        private Action<string, GreenhouseTreatment> onTreatmentChanged;
        private GreenhouseObservation observation;
        private GreenhouseExperimentConfig config;

        private void OnEnable()
        {
            UiThemeService.ThemeChanged += ApplyTheme;
            ApplyTheme();
        }

        private void OnDisable()
        {
            UiThemeService.ThemeChanged -= ApplyTheme;
        }

        public void Bind(
            string trayId,
            GreenhouseExperimentConfig experiment,
            Action<string, GreenhouseTreatment> treatmentChanged)
        {
            TrayId = trayId;
            config = experiment;
            onTreatmentChanged = treatmentChanged;

            if (redButton != null)
            {
                redButton.onClick.AddListener(HandleRed);
            }

            if (darkButton != null)
            {
                darkButton.onClick.AddListener(HandleDark);
            }

            SetTreatment(GreenhouseTreatment.None);
            SetObservation(null);
        }

        public void SetTreatment(GreenhouseTreatment treatment)
        {
            Treatment = treatment;
            ApplyTheme();
        }

        public void SetObservation(GreenhouseObservation value)
        {
            observation = value;
            ApplyTheme();
        }

        public void SetInteractable(bool value)
        {
            if (redButton != null)
            {
                redButton.interactable = value;
            }

            if (darkButton != null)
            {
                darkButton.interactable = value;
            }
        }

        private void HandleRed()
        {
            onTreatmentChanged?.Invoke(TrayId, GreenhouseTreatment.RedLight);
        }

        private void HandleDark()
        {
            onTreatmentChanged?.Invoke(TrayId, GreenhouseTreatment.Dark);
        }

        private void ApplyTheme()
        {
            UiTheme theme = UiThemeService.ActiveTheme;
            if (theme == null)
            {
                return;
            }

            if (titleLabel != null)
            {
                titleLabel.text = TrayId == "a"
                    ? config != null ? config.trayALabel : "TRAY A"
                    : config != null ? config.trayBLabel : "TRAY B";
            }

            if (redButtonLabel != null && config != null)
            {
                redButtonLabel.text = config.redButtonLabel;
            }

            if (darkButtonLabel != null && config != null)
            {
                darkButtonLabel.text = config.darkButtonLabel;
            }

            SetButtonSelected(redButton, Treatment == GreenhouseTreatment.RedLight);
            SetButtonSelected(darkButton, Treatment == GreenhouseTreatment.Dark);

            Color treatmentColor = theme.textSecondary;
            string treatmentText = config != null ? config.chooseTreatmentLabel : "Choose a treatment";
            Color glowColor = new Color(theme.textSecondary.r, theme.textSecondary.g, theme.textSecondary.b, 0.10f);
            Color coreColor = theme.panelAlt;

            if (Treatment == GreenhouseTreatment.RedLight)
            {
                treatmentText = config != null ? config.redTreatmentLabel : "RED LIGHT";
                treatmentColor = theme.danger;
                glowColor = new Color(theme.danger.r, theme.danger.g, theme.danger.b, 0.42f);
                coreColor = theme.danger;
            }
            else if (Treatment == GreenhouseTreatment.Dark)
            {
                treatmentText = config != null ? config.darkTreatmentLabel : "DARK CONTROL";
                treatmentColor = theme.textPrimary;
                glowColor = new Color(0f, 0f, 0f, 0.26f);
                coreColor = theme.background;
            }

            if (treatmentLabel != null)
            {
                treatmentLabel.text = treatmentText;
                var themed = treatmentLabel.GetComponent<ThemedText>();
                if (themed != null)
                {
                    themed.overrideColor = true;
                    themed.colorOverride = treatmentColor;
                    themed.Apply();
                }
                else
                {
                    treatmentLabel.color = treatmentColor;
                }
            }

            if (lampGlow != null)
            {
                lampGlow.color = glowColor;
            }

            if (lampCore != null)
            {
                lampCore.color = coreColor;
            }

            if (plantImage != null)
            {
                plantImage.color = Color.white;
            }

            ApplyObservation(theme);
        }

        private void ApplyObservation(UiTheme theme)
        {
            bool visible = observation != null;
            if (observationGroup != null)
            {
                observationGroup.SetActive(visible);
            }

            if (!visible)
            {
                return;
            }

            if (growthLabel != null) growthLabel.text = observation.growthLabel;
            if (growthValue != null)
            {
                growthValue.text = observation.growthValue;
                SetToneText(growthValue, theme, observation.growthTone);
            }

            if (stressLabel != null) stressLabel.text = observation.stressLabel;
            if (stressValue != null)
            {
                stressValue.text = observation.stressValue;
                SetToneText(stressValue, theme, observation.stressTone);
            }

            SetDot(growthDot, ToneColor(theme, observation.growthTone));
            SetDot(stressDot, ToneColor(theme, observation.stressTone));
        }

        private static void SetButtonSelected(Button button, bool value)
        {
            if (button == null)
            {
                return;
            }

            var themed = button.GetComponent<ThemedButton>();
            if (themed != null)
            {
                themed.selected = value;
                themed.Apply();
            }
        }

        private static Color ToneColor(UiTheme theme, int tone)
        {
            if (tone <= 0)
            {
                return theme.success;
            }

            return tone == 1 ? theme.warning : theme.danger;
        }

        private static void SetToneText(TMP_Text label, UiTheme theme, int tone)
        {
            if (label == null)
            {
                return;
            }

            Color color = ToneColor(theme, tone);
            var themed = label.GetComponent<ThemedText>();
            if (themed != null)
            {
                themed.overrideColor = true;
                themed.colorOverride = color;
                themed.Apply();
            }
            else
            {
                label.color = color;
            }
        }

        private static void SetDot(Image dot, Color color)
        {
            if (dot == null)
            {
                return;
            }

            dot.color = color;
            dot.enabled = true;
        }
    }
}
