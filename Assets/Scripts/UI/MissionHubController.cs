using System.Collections.Generic;
using MarsOutpost.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsOutpost.UI
{
    /// <summary>
    /// Mission hub: shared resources, three data-driven activity cards, first-visit
    /// briefing, and the mission report gate.
    /// </summary>
    public sealed class MissionHubController : MissionScreen
    {
        public Transform resourceContainer;
        public Transform activityContainer;
        public ResourcePillView resourcePillPrefab;
        public ActivityCardView activityCardPrefab;

        public GameObject briefingRoot;
        public TMP_Text briefingTitleLabel;
        public TMP_Text briefingBodyLabel;
        public Button briefingStartButton;

        public TMP_Text missionCodeLabel;
        public TMP_Text progressLabel;
        public TMP_Text lastResultLabel;
        public Button sendReportButton;
        public Button restartButton;
        public Button textSizeButton;
        public Button contrastButton;
        public TMP_Text sendReportLabel;
        public TMP_Text textSizeLabel;
        public TMP_Text contrastLabel;
        public ConfirmDialog dialog;

        private readonly List<ResourcePillView> resourcePills = new List<ResourcePillView>();
        private bool started;

        private void OnEnable()
        {
            if (Director != null)
            {
                Director.SessionChanged += OnSessionChanged;
            }
        }

        private void OnDisable()
        {
            if (Director != null)
            {
                Director.SessionChanged -= OnSessionChanged;
            }
        }

        private void Start()
        {
            if (Director == null || Director.Config == null || Director.Session == null)
            {
                Debug.LogError("[MarsOutpost] MissionHub has no mission data.");
                return;
            }

            MissionConfig config = Director.Config;

            if (missionCodeLabel != null) missionCodeLabel.text = config.missionCode;
            if (briefingTitleLabel != null) briefingTitleLabel.text = config.introHeadline;
            if (briefingBodyLabel != null) briefingBodyLabel.text = config.introBody;
            if (briefingStartButton != null) briefingStartButton.onClick.AddListener(StartTraining);
            if (sendReportButton != null) sendReportButton.onClick.AddListener(OnSendReport);
            if (restartButton != null) restartButton.onClick.AddListener(OnRestart);
            if (textSizeButton != null) textSizeButton.onClick.AddListener(OnTextSize);
            if (contrastButton != null) contrastButton.onClick.AddListener(OnContrast);

            BuildResourcePills(config);
            BuildActivityCards(config);

            started = true;
            Refresh();

            if (briefingRoot != null)
            {
                briefingRoot.SetActive(!Director.Session.BriefingSeen);
            }

            UpdateAccessibilityLabels();
        }

        private void StartTraining()
        {
            if (Director?.Session == null)
            {
                return;
            }

            Director.Session.BriefingSeen = true;
            if (briefingRoot != null)
            {
                briefingRoot.SetActive(false);
            }

            Refresh();
        }

        private void BuildResourcePills(MissionConfig config)
        {
            resourcePills.Clear();
            if (resourceContainer == null || resourcePillPrefab == null || config.resources == null)
            {
                return;
            }

            foreach (Transform child in resourceContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (ResourceDefinition resource in config.resources)
            {
                if (resource == null)
                {
                    continue;
                }

                ResourcePillView pill = Instantiate(resourcePillPrefab, resourceContainer);
                pill.Bind(resource, Director.Session.GetValue(resource.IdOrFallback));
                resourcePills.Add(pill);
            }
        }

        private void BuildActivityCards(MissionConfig config)
        {
            if (activityContainer == null || activityCardPrefab == null || config.activities == null)
            {
                return;
            }

            foreach (Transform child in activityContainer)
            {
                Destroy(child.gameObject);
            }

            var ordered = new List<ActivityDefinition>();
            foreach (ActivityDefinition activity in config.activities)
            {
                if (activity != null)
                {
                    ordered.Add(activity);
                }
            }

            ordered.Sort((left, right) => left.hubOrder.CompareTo(right.hubOrder));

            for (int i = 0; i < ordered.Count; i++)
            {
                ActivityDefinition activity = ordered[i];
                ActivityCardView card = Instantiate(activityCardPrefab, activityContainer);
                bool completed = Director.Session.HasOutcome(activity.id);
                bool available = Application.CanStreamedLevelBeLoaded(activity.sceneName);
                card.Bind(
                    activity,
                    i,
                    completed,
                    available,
                    config.allowReplayCompletedActivities,
                    () => OpenActivity(activity));
            }
        }

        private void OpenActivity(ActivityDefinition activity)
        {
            if (activity == null)
            {
                return;
            }

            if (!Director.TryLoadScene(activity.sceneName, out string error))
            {
                dialog?.ShowInfo("Activity unavailable", error);
            }
        }

        private void Refresh()
        {
            if (!started || Director?.Session == null)
            {
                return;
            }

            MissionSession session = Director.Session;
            MissionConfig config = Director.Config;

            foreach (ResourcePillView pill in resourcePills)
            {
                pill.RefreshTheme();
            }

            if (config.resources != null)
            {
                for (int i = 0; i < resourcePills.Count && i < config.resources.Length; i++)
                {
                    ResourceDefinition resource = config.resources[i];
                    if (resource != null)
                    {
                        resourcePills[i].SetValue(session.GetValue(resource.IdOrFallback));
                    }
                }
            }

            if (progressLabel != null)
            {
                progressLabel.text = $"{session.RequiredCompletedCount()} of {session.RequiredTotalCount()} training steps complete";
            }

            if (sendReportButton != null)
            {
                sendReportButton.interactable = session.AreRequiredActivitiesComplete();
            }

            if (lastResultLabel != null)
            {
                ActivityOutcome last = Director.LastOutcome;
                lastResultLabel.text = last == null
                    ? "Your crew is ready. Choose a training step."
                    : $"<b>Last report:</b> {last.Headline} - {UiTextFormat.FormatChangeSummary(last.Changes)}";
            }
        }

        private void OnSessionChanged(MissionSession session)
        {
            Refresh();
        }

        private void OnSendReport()
        {
            if (!Director.TryLoadDebrief(out string error))
            {
                dialog?.ShowInfo("Report unavailable", error);
            }
        }

        private void OnRestart()
        {
            dialog?.Show(
                "Restart the mission?",
                "This resets Power, Crew Safety, Science Progress, and every completed training step.",
                "Restart",
                "Cancel",
                () =>
                {
                    if (!Director.TryRestartMission(out string error))
                    {
                        dialog?.ShowInfo("Mission can't restart yet", error);
                    }
                });
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

            dialog?.Show(
                "Leave the mission?",
                "Your current training progress will be lost. You can start a new mission from the main menu.",
                "Main Menu",
                "Stay",
                () =>
                {
                    if (!Director.TryLoadMainMenu(out string error))
                    {
                        dialog?.ShowInfo("Menu unavailable", error);
                    }
                });
        }
    }
}
