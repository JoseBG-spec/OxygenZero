using MarsOutpost.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsOutpost.UI
{
    /// <summary>Main menu: start, help, sources, accessibility, and a safe quit path.</summary>
    public sealed class MainMenuController : MissionScreen
    {
        public TMP_Text titleLabel;
        public TMP_Text subtitleLabel;
        public TMP_Text missionCodeLabel;
        public TMP_Text introLabel;
        public TMP_Text footerLabel;
        public Button startButton;
        public Button howToPlayButton;
        public Button sourcesButton;
        public Button quitButton;
        public Button textSizeButton;
        public Button contrastButton;
        public TMP_Text textSizeLabel;
        public TMP_Text contrastLabel;
        public ConfirmDialog dialog;

        private void Start()
        {
            if (Director == null || Director.Config == null)
            {
                Debug.LogError("[MarsOutpost] MainMenu has no MissionConfig. Check the MissionSceneBootstrap object.");
                return;
            }

            MissionConfig config = Director.Config;

            if (titleLabel != null) titleLabel.text = config.gameTitle;
            if (subtitleLabel != null) subtitleLabel.text = config.subtitle;
            if (missionCodeLabel != null) missionCodeLabel.text = config.missionCode;
            if (introLabel != null) introLabel.text = config.introBody;
            if (footerLabel != null) footerLabel.text = config.simulationDisclaimer;

            if (startButton != null) startButton.onClick.AddListener(OnStartMission);
            if (howToPlayButton != null) howToPlayButton.onClick.AddListener(OnHowToPlay);
            if (sourcesButton != null) sourcesButton.onClick.AddListener(OnSources);
            if (quitButton != null) quitButton.onClick.AddListener(ShowQuitDialog);
            if (textSizeButton != null) textSizeButton.onClick.AddListener(OnTextSize);
            if (contrastButton != null) contrastButton.onClick.AddListener(OnContrast);

            UpdateAccessibilityLabels();
        }

        private void OnStartMission()
        {
            if (!Director.TryStartNewMission(out string error))
            {
                dialog?.ShowInfo("Mission can't start yet", error);
            }
        }

        private void OnHowToPlay()
        {
            MissionConfig config = Director.Config;
            dialog?.ShowInfo(config.howToPlayTitle, config.howToPlayBody);
        }

        private void OnSources()
        {
            MissionConfig config = Director.Config;
            string body = config.credits;

            if (config.debrief != null)
            {
                body += "\n\n" + config.debrief.sources;
                body += "\n\n" + config.debrief.notNasaNote;
            }

            dialog?.ShowInfo("Sources & Credits", body);
        }

        private void OnTextSize()
        {
            UiThemeService.CycleFontScale();
            UpdateAccessibilityLabels();
        }

        private void OnContrast()
        {
            UiThemeService.ToggleHighContrast();
            UpdateAccessibilityLabels();
        }

        private void UpdateAccessibilityLabels()
        {
            if (textSizeLabel != null)
            {
                textSizeLabel.text = $"Text {UiThemeService.FontScaleLabel}";
            }

            if (contrastLabel != null)
            {
                contrastLabel.text = UiThemeService.HighContrast ? "High contrast: ON" : "High contrast: OFF";
            }
        }

        private void ShowQuitDialog()
        {
            dialog?.Show(
                "Leave the outpost?",
                "Your training progress will be lost. You can start a new mission any time.",
                "Quit",
                "Keep playing",
                () => Director?.QuitGame());
        }

        protected override void HandleBack()
        {
            if (dialog != null && dialog.IsOpen)
            {
                dialog.Hide();
                return;
            }

            ShowQuitDialog();
        }
    }
}
