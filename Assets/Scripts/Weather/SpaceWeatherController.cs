using System.Collections.Generic;
using MarsOutpost.Core;
using MarsOutpost.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsOutpost.Weather
{
    /// <summary>
    /// Phase 4 screen: one fictional DONKI-style training alert, a compare view for event
    /// types, and three crew plans. The local JSON source can be replaced, and the authored
    /// fallback keeps the activity usable if that source fails.
    /// </summary>
    public sealed class SpaceWeatherController : MissionScreen
    {
        [Header("Activity")]
        public ActivityDefinition activity;
        public SpaceWeatherConfig config;

        [Header("Header")]
        public TMP_Text screenTitleLabel;
        public TMP_Text taglineLabel;

        [Header("Control room")]
        public GameObject alertGroup;
        public GameObject decoderGroup;
        public Image controlRoomArt;
        public TMP_Text controlRoomTitleLabel;
        public TMP_Text sourceLabel;
        public TMP_Text eventTypeLabel;
        public TMP_Text eventNameLabel;
        public TMP_Text alertCodeLabel;
        public TMP_Text alertTagLabel;
        public TMP_Text alertSummaryLabel;
        public TMP_Text sourceNoteLabel;

        [Header("Decoder")]
        public Button compareButton;
        public TMP_Text compareLabel;

        [Header("Crew plan")]
        public TMP_Text decisionTitleLabel;
        public TMP_Text headlineLabel;
        public TMP_Text decisionPromptLabel;
        public Transform planContainer;
        public SpaceWeatherPlanView planPrefab;
        public TMP_Text planHintLabel;
        public Button confirmButton;
        public TMP_Text confirmLabel;
        public TMP_Text simulationNoteLabel;

        [Header("Shared panels")]
        public ActivityResultPanel resultPanel;
        public ConfirmDialog dialog;

        private readonly List<SpaceWeatherPlanView> planViews = new List<SpaceWeatherPlanView>();
        private SpaceWeatherAlertDefinition alert;
        private SpaceWeatherPlanDefinition selectedPlan;
        private bool locked;
        private bool decoderOpen;

        public bool IsReady { get; private set; }
        public SpaceWeatherAlertDefinition Alert => alert;
        public SpaceWeatherPlanDefinition SelectedPlan => selectedPlan;
        public IReadOnlyList<SpaceWeatherPlanView> PlanViews => planViews;
        public bool DecoderOpen => decoderOpen;

        private void Start()
        {
            if (activity == null || config == null)
            {
                Debug.LogError("[MarsOutpost] Space weather screen is missing its activity or config.");
                return;
            }

            if (screenTitleLabel != null)
            {
                screenTitleLabel.text = config.screenTitle;
            }

            if (taglineLabel != null)
            {
                taglineLabel.text = config.tagline;
            }

            if (controlRoomTitleLabel != null)
            {
                controlRoomTitleLabel.text = config.controlRoomTitle;
            }

            if (decisionTitleLabel != null)
            {
                decisionTitleLabel.text = config.decisionTitle;
            }

            if (alertTagLabel != null)
            {
                alertTagLabel.text = config.alertTag;
            }

            if (simulationNoteLabel != null)
            {
                simulationNoteLabel.text = config.simulationNote;
            }

            WireButtons();
            LoadAlert();
        }

        private void WireButtons()
        {
            if (compareButton != null)
            {
                compareButton.onClick.AddListener(ToggleDecoder);
            }

            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(SendCrewPlan);
            }
        }

        private void LoadAlert()
        {
            var source = new LocalJsonSpaceWeatherAlertSource(config.curatedAlertsJson);
            if (!SpaceWeatherAlertLibrary.TryLoad(
                    source,
                    config.fallbackAlert,
                    out alert,
                    out string sourceLabelText,
                    out string loadError))
            {
                ShowUnavailable("The station could not load a training alert. This screen cannot run yet.");
                return;
            }

            if (!string.IsNullOrWhiteSpace(loadError))
            {
                Debug.LogWarning($"[MarsOutpost] Space weather is using the fallback alert. {loadError}");
            }

            List<string> errors = SpaceWeatherDataValidator.ValidateAlert(alert);
            foreach (string error in errors)
            {
                Debug.LogWarning($"[MarsOutpost] Space weather alert: {error}");
            }

            if (sourceLabel != null)
            {
                sourceLabel.text = sourceLabelText;
            }

            if (alertSummaryLabel != null)
            {
                alertSummaryLabel.text = alert.summary;
            }

            if (sourceNoteLabel != null)
            {
                sourceNoteLabel.text = alert.sourceNote;
            }

            if (headlineLabel != null)
            {
                headlineLabel.text = alert.headline;
            }

            if (eventTypeLabel != null)
            {
                eventTypeLabel.text = alert.eventType;
            }

            if (eventNameLabel != null)
            {
                eventNameLabel.text = alert.eventName;
            }

            if (alertCodeLabel != null)
            {
                alertCodeLabel.text = alert.alertCode;
            }

            if (decisionPromptLabel != null)
            {
                decisionPromptLabel.text = config.decisionPrompt;
            }

            if (!BuildPlans())
            {
                return;
            }

            if (MissionDirector.Instance != null &&
                MissionDirector.Instance.Session != null &&
                MissionDirector.Instance.Session.HasOutcome(activity.id))
            {
                RestoreCompleted(MissionDirector.Instance.Session.GetOutcome(activity.id));
            }
            else
            {
                ShowDecision();
            }

            IsReady = true;
        }

        private bool BuildPlans()
        {
            ClearPlans();

            if (planPrefab == null || planContainer == null)
            {
                ShowUnavailable("The crew-plan cards are missing from this screen.");
                return false;
            }

            if (alert == null || alert.choices == null || alert.choices.Length == 0)
            {
                ShowUnavailable("This training alert has no crew plans.");
                return false;
            }

            foreach (SpaceWeatherPlanDefinition plan in alert.choices)
            {
                if (plan == null)
                {
                    continue;
                }

                SpaceWeatherPlanView view = Instantiate(planPrefab, planContainer);
                view.Bind(plan, SelectPlan);
                if (view.indexLabel != null)
                {
                    view.indexLabel.text = (planViews.Count + 1).ToString();
                }

                view.SetInteractable(true);
                view.SetSelected(false);
                view.gameObject.SetActive(true);
                planViews.Add(view);
            }

            if (planViews.Count == 0)
            {
                ShowUnavailable("This training alert has no usable crew plans.");
                return false;
            }

            return true;
        }

        private void ClearPlans()
        {
            foreach (SpaceWeatherPlanView view in planViews)
            {
                if (view != null)
                {
                    Destroy(view.gameObject);
                }
            }

            planViews.Clear();
        }

        private void ShowDecision()
        {
            if (confirmButton != null)
            {
                confirmButton.gameObject.SetActive(true);
                confirmButton.interactable = false;
            }

            if (confirmLabel != null)
            {
                confirmLabel.text = config.choosePlanLabel;
            }

            if (planHintLabel != null)
            {
                planHintLabel.text = config.planHint;
            }

            if (alertGroup != null)
            {
                alertGroup.SetActive(true);
            }

            if (decoderGroup != null)
            {
                decoderGroup.SetActive(false);
            }

            decoderOpen = false;
            UpdateCompareLabel();
        }

        private void SelectPlan(SpaceWeatherPlanDefinition plan)
        {
            if (locked || plan == null)
            {
                return;
            }

            selectedPlan = plan;
            foreach (SpaceWeatherPlanView view in planViews)
            {
                view.SetSelected(view.Plan == plan);
            }

            if (confirmButton != null)
            {
                confirmButton.interactable = true;
            }

            if (confirmLabel != null)
            {
                confirmLabel.text = config.confirmLabel;
            }

            if (planHintLabel != null)
            {
                planHintLabel.text = SafeFormat(config.selectedPlanFormat, plan.title);
            }
        }

        private void SendCrewPlan()
        {
            if (locked || selectedPlan == null)
            {
                return;
            }

            MissionDirector director = MissionDirector.Instance;
            if (director == null || director.Session == null)
            {
                ShowUnavailable("The mission session is not ready.");
                return;
            }

            locked = true;
            SetPlanInteraction(false);

            if (confirmButton != null)
            {
                confirmButton.interactable = false;
            }

            if (confirmLabel != null)
            {
                confirmLabel.text = config.completedLabel;
            }

            if (planHintLabel != null)
            {
                planHintLabel.text = SafeFormat(config.completedPlanFormat, selectedPlan.title);
            }

            ActivityOutcome outcome = director.CompleteActivity(
                activity.id,
                selectedPlan.id,
                selectedPlan.headline,
                selectedPlan.feedback,
                selectedPlan.learningNote,
                selectedPlan.changes);

            if (outcome == null)
            {
                locked = false;
                SetPlanInteraction(true);
                if (confirmButton != null)
                {
                    confirmButton.interactable = true;
                }

                if (confirmLabel != null)
                {
                    confirmLabel.text = config.confirmLabel;
                }

                return;
            }

            if (resultPanel != null)
            {
                resultPanel.Show(outcome, ContinueToHub);
            }

            if (confirmButton != null)
            {
                confirmButton.gameObject.SetActive(false);
            }
        }

        private void RestoreCompleted(ActivityOutcome outcome)
        {
            locked = true;
            selectedPlan = FindPlan(outcome != null ? outcome.ChoiceId : null);

            foreach (SpaceWeatherPlanView view in planViews)
            {
                view.SetSelected(view.Plan == selectedPlan);
                view.SetInteractable(false);
            }

            if (planHintLabel != null)
            {
                planHintLabel.text = selectedPlan != null
                    ? SafeFormat(config.completedPlanFormat, selectedPlan.title)
                    : config.completedLabel;
            }

            if (confirmButton != null)
            {
                confirmButton.gameObject.SetActive(false);
            }
        }

        private SpaceWeatherPlanDefinition FindPlan(string planId)
        {
            if (alert == null || alert.choices == null || string.IsNullOrWhiteSpace(planId))
            {
                return null;
            }

            foreach (SpaceWeatherPlanDefinition plan in alert.choices)
            {
                if (plan != null && plan.id == planId)
                {
                    return plan;
                }
            }

            return null;
        }

        private void SetPlanInteraction(bool interactable)
        {
            foreach (SpaceWeatherPlanView view in planViews)
            {
                view.SetInteractable(interactable);
            }
        }

        private void ToggleDecoder()
        {
            if (alertGroup == null || decoderGroup == null)
            {
                return;
            }

            decoderOpen = !decoderOpen;
            alertGroup.SetActive(!decoderOpen);
            decoderGroup.SetActive(decoderOpen);
            UpdateCompareLabel();
        }

        private void UpdateCompareLabel()
        {
            if (compareLabel != null)
            {
                compareLabel.text = decoderOpen ? config.backToAlertLabel : config.compareLabel;
            }
        }

        private void ShowUnavailable(string message)
        {
            IsReady = false;
            if (confirmButton != null)
            {
                confirmButton.interactable = false;
            }

            if (planHintLabel != null)
            {
                planHintLabel.text = message;
            }

            if (dialog != null)
            {
                dialog.ShowInfo("Training alert unavailable", message);
            }
        }

        private void ContinueToHub()
        {
            if (MissionDirector.Instance != null && MissionDirector.Instance.TryLoadHub(out string error))
            {
                return;
            }

            if (dialog != null)
            {
                dialog.ShowInfo("Hub unavailable", "The mission hub could not be loaded. Return to the main menu.");
            }
        }

        protected override void HandleBack()
        {
            if (dialog != null && dialog.IsOpen)
            {
                dialog.Hide();
                return;
            }

            if (resultPanel != null && resultPanel.IsOpen)
            {
                resultPanel.Hide();
                ContinueToHub();
                return;
            }

            ContinueToHub();
        }

        private static string SafeFormat(string format, string value)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                return value ?? string.Empty;
            }

            try
            {
                return string.Format(format, value);
            }
            catch (System.FormatException)
            {
                return format;
            }
        }
    }
}
