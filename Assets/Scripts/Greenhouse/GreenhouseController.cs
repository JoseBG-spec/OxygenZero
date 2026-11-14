using System;
using System.Collections.Generic;
using MarsOutpost.Core;
using MarsOutpost.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsOutpost.Greenhouse
{
    /// <summary>
    /// Phase 3 greenhouse activity. The child sets up one red-light tray and one dark
    /// control, runs a simulated trial, then chooses a claim that the OSD-314 summary
    /// can support. Signals and rewards are game values, not measurements.
    /// </summary>
    public sealed class GreenhouseController : MissionScreen
    {
        public ActivityDefinition activity;
        public GreenhouseExperimentConfig experiment;

        [Header("Header")]
        public TMP_Text titleLabel;
        public TMP_Text taglineLabel;
        public Button backButton;

        [Header("Growth chamber")]
        public TMP_Text chamberTitleLabel;
        public TMP_Text environmentLabel;
        public GreenhouseTrayView trayA;
        public GreenhouseTrayView trayB;

        [Header("Notebook")]
        public TMP_Text notebookTitleLabel;
        public TMP_Text stepLabel;
        public TMP_Text bodyLabel;
        public GameObject setupGroup;
        public TMP_Text setupStatusLabel;
        public TMP_Text runCostLabel;
        public Button runButton;
        public TMP_Text runLabel;
        public GameObject claimsGroup;
        public TMP_Text claimsHeaderLabel;
        public TMP_Text claimPromptLabel;
        public Transform claimsContainer;
        public GreenhouseClaimOptionView claimPrefab;
        public TMP_Text claimFeedbackLabel;

        [Header("Actions")]
        public Button confirmButton;
        public TMP_Text confirmLabel;
        public ActivityResultPanel resultPanel;
        public ConfirmDialog dialog;

        private readonly List<GreenhouseClaimOptionView> claimViews = new List<GreenhouseClaimOptionView>();
        private GreenhouseTreatment trayATreatment = GreenhouseTreatment.None;
        private GreenhouseTreatment trayBTreatment = GreenhouseTreatment.None;
        private GreenhouseClaimDefinition selectedClaim;
        private bool trialRun;
        private bool locked;

        public bool TrialRun => trialRun;
        public bool IsLocked => locked;
        public IReadOnlyList<GreenhouseClaimOptionView> ClaimViews => claimViews;

        private void Start()
        {
            if (activity == null || experiment == null)
            {
                Debug.LogError("[MarsOutpost] Greenhouse activity is missing its ActivityDefinition or GreenhouseExperimentConfig.");
                return;
            }

            if (titleLabel != null) titleLabel.text = experiment.screenTitle;
            if (taglineLabel != null) taglineLabel.text = experiment.tagline;
            if (chamberTitleLabel != null) chamberTitleLabel.text = experiment.chamberTitle;
            if (environmentLabel != null) environmentLabel.text = experiment.environmentLabel;
            if (notebookTitleLabel != null) notebookTitleLabel.text = experiment.notebookTitle;

            if (trayA != null) trayA.Bind("a", experiment, OnTreatmentChanged);
            if (trayB != null) trayB.Bind("b", experiment, OnTreatmentChanged);

            if (backButton != null) backButton.onClick.AddListener(RequestBack);
            if (runButton != null) runButton.onClick.AddListener(RunTrial);
            if (confirmButton != null) confirmButton.onClick.AddListener(ConfirmFinding);

            foreach (string error in GreenhouseDataValidator.Validate(experiment))
            {
                Debug.LogError($"[MarsOutpost] Greenhouse data: {error}");
            }

            BuildClaims();

            bool alreadyCompleted = Director != null
                                    && Director.Session != null
                                    && Director.Session.HasOutcome(activity.id)
                                    && (Director.Config == null || !Director.Config.allowReplayCompletedActivities);

            if (alreadyCompleted)
            {
                RestoreCompleted(Director.Session.GetOutcome(activity.id));
            }
            else
            {
                ShowSetup();
            }
        }

        private void OnEnable()
        {
            UiThemeService.ThemeChanged += RefreshStatusColor;
        }

        private void OnDisable()
        {
            UiThemeService.ThemeChanged -= RefreshStatusColor;
        }

        private void BuildClaims()
        {
            if (claimsContainer == null || claimPrefab == null)
            {
                Debug.LogError("[MarsOutpost] Greenhouse notebook is missing its claim container or claim prefab.");
                return;
            }

            foreach (GreenhouseClaimDefinition claim in experiment.claims)
            {
                if (claim == null)
                {
                    continue;
                }

                GreenhouseClaimOptionView view = Instantiate(claimPrefab, claimsContainer);
                view.Bind(claim, OnClaimSelected);
                claimViews.Add(view);
            }
        }

        private void ShowSetup()
        {
            trialRun = false;
            locked = false;
            selectedClaim = null;
            trayATreatment = GreenhouseTreatment.None;
            trayBTreatment = GreenhouseTreatment.None;

            if (trayA != null)
            {
                trayA.SetObservation(null);
                trayA.SetInteractable(true);
                trayA.SetTreatment(trayATreatment);
            }

            if (trayB != null)
            {
                trayB.SetObservation(null);
                trayB.SetInteractable(true);
                trayB.SetTreatment(trayBTreatment);
            }

            if (setupGroup != null) setupGroup.SetActive(true);
            if (claimsGroup != null) claimsGroup.SetActive(false);
            if (confirmButton != null) confirmButton.gameObject.SetActive(false);
            if (stepLabel != null) stepLabel.text = experiment.setupHeader;
            if (bodyLabel != null) bodyLabel.text = experiment.setupBody;
            if (runLabel != null) runLabel.text = experiment.runLabel;
            if (runCostLabel != null)
            {
                runCostLabel.text = SafeFormat(experiment.runCostFormat, experiment.runPowerCost);
            }

            ApplyTreatments();
        }

        private void ApplyTreatments()
        {
            if (trayA != null)
            {
                trayA.SetTreatment(trayATreatment);
            }

            if (trayB != null)
            {
                trayB.SetTreatment(trayBTreatment);
            }

            bool valid = HasFairTest();
            if (runButton != null)
            {
                runButton.interactable = valid && !locked && !trialRun;
            }

            if (setupStatusLabel != null)
            {
                setupStatusLabel.text = StatusText(valid);
            }

            RefreshStatusColor();
        }

        private bool HasFairTest()
        {
            return (trayATreatment == GreenhouseTreatment.RedLight && trayBTreatment == GreenhouseTreatment.Dark)
                   || (trayATreatment == GreenhouseTreatment.Dark && trayBTreatment == GreenhouseTreatment.RedLight);
        }

        private string StatusText(bool valid)
        {
            if (valid)
            {
                string redTray = trayATreatment == GreenhouseTreatment.RedLight
                    ? experiment.trayALabel
                    : experiment.trayBLabel;
                string darkTray = trayATreatment == GreenhouseTreatment.Dark
                    ? experiment.trayALabel
                    : experiment.trayBLabel;
                return SafeFormat(experiment.setupReadyFormat, redTray, darkTray);
            }

            if (trayATreatment == GreenhouseTreatment.RedLight && trayBTreatment == GreenhouseTreatment.RedLight)
            {
                return experiment.setupBothRed;
            }

            if (trayATreatment == GreenhouseTreatment.Dark && trayBTreatment == GreenhouseTreatment.Dark)
            {
                return experiment.setupBothDark;
            }

            return experiment.setupIncomplete;
        }

        private void OnTreatmentChanged(string trayId, GreenhouseTreatment treatment)
        {
            if (locked || trialRun)
            {
                return;
            }

            if (trayId == "a")
            {
                trayATreatment = treatment;
            }
            else if (trayId == "b")
            {
                trayBTreatment = treatment;
            }

            ApplyTreatments();
        }

        private void RunTrial()
        {
            if (locked || trialRun || !HasFairTest())
            {
                return;
            }

            trialRun = true;
            selectedClaim = null;

            if (trayA != null) trayA.SetInteractable(false);
            if (trayB != null) trayB.SetInteractable(false);
            if (runButton != null) runButton.interactable = false;

            ApplyObservations();

            if (setupGroup != null) setupGroup.SetActive(false);
            if (claimsGroup != null) claimsGroup.SetActive(true);
            if (stepLabel != null) stepLabel.text = experiment.observationsHeader;
            if (bodyLabel != null) bodyLabel.text = experiment.observationsBody;
            if (claimsHeaderLabel != null) claimsHeaderLabel.text = experiment.claimsHeader;
            if (claimPromptLabel != null) claimPromptLabel.text = experiment.claimPrompt;
            if (claimFeedbackLabel != null) claimFeedbackLabel.text = experiment.claimHint;
            if (confirmButton != null)
            {
                confirmButton.gameObject.SetActive(true);
                confirmButton.interactable = false;
            }

            if (confirmLabel != null) confirmLabel.text = experiment.selectClaimLabel;
            SetClaimSelection(null, true);
        }

        private void ApplyObservations()
        {
            if (trayA != null)
            {
                trayA.SetObservation(trayATreatment == GreenhouseTreatment.RedLight
                    ? experiment.redObservation
                    : experiment.darkObservation);
            }

            if (trayB != null)
            {
                trayB.SetObservation(trayBTreatment == GreenhouseTreatment.RedLight
                    ? experiment.redObservation
                    : experiment.darkObservation);
            }
        }

        private void OnClaimSelected(GreenhouseClaimDefinition claim)
        {
            if (locked || !trialRun || claim == null)
            {
                return;
            }

            selectedClaim = claim;
            SetClaimSelection(claim, true);

            if (claimFeedbackLabel != null)
            {
                claimFeedbackLabel.text = claim.feedback;
            }

            if (confirmButton != null)
            {
                confirmButton.interactable = true;
            }

            if (confirmLabel != null)
            {
                confirmLabel.text = experiment.confirmLabel;
            }
        }

        private void SetClaimSelection(GreenhouseClaimDefinition claim, bool interactable)
        {
            foreach (GreenhouseClaimOptionView view in claimViews)
            {
                view.SetSelected(view.Claim == claim);
                view.SetInteractable(interactable && !locked);
            }
        }

        private void ConfirmFinding()
        {
            if (locked || !trialRun || selectedClaim == null || Director == null || Director.Session == null)
            {
                return;
            }

            locked = true;
            if (confirmButton != null) confirmButton.interactable = false;
            if (confirmLabel != null) confirmLabel.text = experiment.completedLabel;
            SetClaimSelection(selectedClaim, false);
            if (trayA != null) trayA.SetInteractable(false);
            if (trayB != null) trayB.SetInteractable(false);

            var changes = new List<ResourceChange>();
            if (experiment.runPowerCost > 0)
            {
                changes.Add(new ResourceChange(
                    "power",
                    -experiment.runPowerCost,
                    "Red-light trial lamp use."));
            }

            if (selectedClaim.changes != null)
            {
                changes.AddRange(selectedClaim.changes);
            }

            string redTrayId = trayATreatment == GreenhouseTreatment.RedLight ? "a" : "b";
            ActivityOutcome outcome = Director.CompleteActivity(
                activity.id,
                $"{redTrayId}|{selectedClaim.id}",
                selectedClaim.headline,
                selectedClaim.feedback,
                selectedClaim.learningNote,
                changes);

            if (outcome == null)
            {
                locked = false;
                if (confirmButton != null) confirmButton.interactable = true;
                if (confirmLabel != null) confirmLabel.text = experiment.confirmLabel;
                SetClaimSelection(selectedClaim, true);
                Debug.LogError("[MarsOutpost] Greenhouse outcome could not be logged.");
                return;
            }

            if (resultPanel != null)
            {
                resultPanel.Show(outcome, ContinueToHub);
                if (confirmButton != null)
                {
                    confirmButton.gameObject.SetActive(false);
                }
            }
        }

        private void RestoreCompleted(ActivityOutcome prior)
        {
            locked = true;
            trialRun = true;

            ParseChoice(prior != null ? prior.ChoiceId : null, out string redTrayId, out string claimId);
            trayATreatment = redTrayId == "b" ? GreenhouseTreatment.Dark : GreenhouseTreatment.RedLight;
            trayBTreatment = redTrayId == "b" ? GreenhouseTreatment.RedLight : GreenhouseTreatment.Dark;

            if (trayA != null)
            {
                trayA.SetInteractable(false);
                trayA.SetTreatment(trayATreatment);
            }

            if (trayB != null)
            {
                trayB.SetInteractable(false);
                trayB.SetTreatment(trayBTreatment);
            }

            ApplyObservations();

            if (setupGroup != null) setupGroup.SetActive(false);
            if (claimsGroup != null) claimsGroup.SetActive(true);
            if (stepLabel != null) stepLabel.text = experiment.observationsHeader;
            if (bodyLabel != null) bodyLabel.text = experiment.observationsBody;
            if (claimsHeaderLabel != null) claimsHeaderLabel.text = experiment.claimsHeader;
            if (claimPromptLabel != null) claimPromptLabel.text = experiment.claimPrompt;

            GreenhouseClaimDefinition priorClaim = FindClaim(claimId);
            selectedClaim = priorClaim;
            SetClaimSelection(priorClaim, false);

            if (claimFeedbackLabel != null)
            {
                claimFeedbackLabel.text = priorClaim != null
                    ? $"{experiment.completedLabel} {priorClaim.feedback}"
                    : experiment.completedLabel;
            }

            if (confirmButton != null) confirmButton.gameObject.SetActive(false);
        }

        private GreenhouseClaimDefinition FindClaim(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || experiment.claims == null)
            {
                return null;
            }

            foreach (GreenhouseClaimDefinition claim in experiment.claims)
            {
                if (claim != null && claim.id == id)
                {
                    return claim;
                }
            }

            return null;
        }

        private static void ParseChoice(string choiceId, out string redTrayId, out string claimId)
        {
            redTrayId = "a";
            claimId = choiceId;
            if (string.IsNullOrWhiteSpace(choiceId))
            {
                return;
            }

            int separator = choiceId.IndexOf('|');
            if (separator < 0)
            {
                return;
            }

            string prefix = choiceId.Substring(0, separator);
            redTrayId = prefix == "b" ? "b" : "a";
            claimId = choiceId.Substring(separator + 1);
        }

        private void RefreshStatusColor()
        {
            if (setupStatusLabel == null)
            {
                return;
            }

            UiTheme theme = UiThemeService.ActiveTheme;
            if (theme == null)
            {
                return;
            }

            var themed = setupStatusLabel.GetComponent<ThemedText>();
            if (themed != null)
            {
                themed.overrideColor = true;
                themed.colorOverride = HasFairTest() ? theme.success : theme.textSecondary;
                themed.Apply();
            }
        }

        private static string SafeFormat(string format, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                return string.Empty;
            }

            try
            {
                return string.Format(format, args);
            }
            catch (FormatException)
            {
                return format;
            }
        }

        private void ContinueToHub()
        {
            if (!Director.TryLoadHub(out string error))
            {
                dialog?.ShowInfo("Mission Hub unavailable", error);
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
                ContinueToHub();
                return;
            }

            ContinueToHub();
        }
    }
}
