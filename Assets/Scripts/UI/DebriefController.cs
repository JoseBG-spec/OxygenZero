using System.Collections.Generic;
using System.Text;
using MarsOutpost.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsOutpost.UI
{
    /// <summary>
    /// End-of-mission report. Uses the authored debrief bands so the tone stays
    /// encouraging even when a resource ended low.
    /// </summary>
    public sealed class DebriefController : MissionScreen
    {
        public TMP_Text badgeTitleLabel;
        public TMP_Text badgeBodyLabel;
        public Image badgeStripe;
        public TMP_Text scienceLabel;
        public TMP_Text introLabel;
        public TMP_Text outcomesLabel;
        public TMP_Text sourcesLabel;
        public TMP_Text footerLabel;

        public Button replayButton;
        public Button menuButton;
        public Button textSizeButton;
        public Button contrastButton;
        public TMP_Text replayLabel;
        public TMP_Text menuLabel;
        public TMP_Text textSizeLabel;
        public TMP_Text contrastLabel;
        public ConfirmDialog dialog;

        private void Start()
        {
            if (Director == null || Director.Config == null || Director.Session == null)
            {
                Debug.LogError("[MarsOutpost] Debrief has no mission data.");
                return;
            }

            MissionConfig config = Director.Config;
            DebriefConfig debrief = config.debrief;

            if (introLabel != null)
            {
                string intro = debrief != null ? debrief.intro : string.Empty;
                introLabel.text = intro +
                    "\n\n<size=85%><i>Swipe up or scroll to read every training step.</i></size>";
            }
            if (footerLabel != null) footerLabel.text = config.simulationDisclaimer;
            if (scienceLabel != null) scienceLabel.text = Director.Session.Science.ToString();

            DebriefBand band = ChooseBand(debrief, Director.Session.Science);
            if (band != null)
            {
                if (badgeTitleLabel != null) badgeTitleLabel.text = band.title;
                if (badgeBodyLabel != null) badgeBodyLabel.text = band.message;
                if (badgeStripe != null) badgeStripe.color = band.color;
            }

            if (outcomesLabel != null)
            {
                outcomesLabel.text = BuildOutcomes(config, Director.Session);
            }

            if (sourcesLabel != null && debrief != null)
            {
                sourcesLabel.text = debrief.sources + "\n\n" + debrief.notNasaNote;
            }

            if (replayLabel != null) replayLabel.text = debrief != null ? debrief.replayLabel : "Play Again";
            if (menuLabel != null) menuLabel.text = debrief != null ? debrief.menuLabel : "Main Menu";
            if (replayButton != null) replayButton.onClick.AddListener(OnReplay);
            if (menuButton != null) menuButton.onClick.AddListener(OnMenu);
            if (textSizeButton != null) textSizeButton.onClick.AddListener(OnTextSize);
            if (contrastButton != null) contrastButton.onClick.AddListener(OnContrast);

            UpdateAccessibilityLabels();
        }

        private static DebriefBand ChooseBand(DebriefConfig debrief, int science)
        {
            if (debrief?.bands == null)
            {
                return null;
            }

            DebriefBand chosen = null;
            DebriefBand lowest = null;

            foreach (DebriefBand band in debrief.bands)
            {
                if (band == null)
                {
                    continue;
                }

                if (lowest == null || band.minimumScience < lowest.minimumScience)
                {
                    lowest = band;
                }

                if (science >= band.minimumScience && (chosen == null || band.minimumScience > chosen.minimumScience))
                {
                    chosen = band;
                }
            }

            return chosen ?? lowest;
        }

        private static string BuildOutcomes(MissionConfig config, MissionSession session)
        {
            var builder = new StringBuilder();
            bool any = false;

            if (config.activities != null)
            {
                foreach (ActivityDefinition activity in config.activities)
                {
                    if (activity == null)
                    {
                        continue;
                    }

                    ActivityOutcome outcome = session.GetOutcome(activity.id);
                    if (outcome == null)
                    {
                        continue;
                    }

                    any = true;
                    builder.Append($"<b>{activity.displayName.ToUpperInvariant()}</b>\n");
                    builder.Append($"<color=#FFE08A>{outcome.Headline}</color>\n");
                    builder.Append(outcome.Summary).Append("\n\n");
                    builder.Append(UiTextFormat.FormatChanges(outcome.Changes)).Append("\n");

                    if (!string.IsNullOrWhiteSpace(outcome.LearningNote))
                    {
                        builder.Append($"<size=88%><i>Learning note: {outcome.LearningNote}</i></size>\n");
                    }

                    builder.Append('\n');
                }
            }

            return any
                ? builder.ToString().TrimEnd()
                : "No training steps were completed. Return to the hub to start the mission.";
        }

        private void OnReplay()
        {
            if (!Director.TryRestartMission(out string error))
            {
                dialog?.ShowInfo("Mission can't restart yet", error);
            }
        }

        private void OnMenu()
        {
            if (!Director.TryLoadMainMenu(out string error))
            {
                dialog?.ShowInfo("Menu unavailable", error);
            }
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

        protected override void HandleBack()
        {
            if (dialog != null && dialog.IsOpen)
            {
                dialog.Hide();
                return;
            }

            OnMenu();
        }
    }
}
