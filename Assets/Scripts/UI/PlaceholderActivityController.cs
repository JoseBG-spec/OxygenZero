using MarsOutpost.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsOutpost.UI
{
    /// <summary>
    /// Phase 1 activity shell. It explains the upcoming activity and records one authored
    /// practice outcome so the hub, resource bar, and debrief can be tested end to end.
    /// Phases 2-4 replace each shell with the real minigame.
    /// </summary>
    public sealed class PlaceholderActivityController : MissionScreen
    {
        public ActivityDefinition activity;
        public PlaceholderActivityOutcome placeholderOutcome;

        public TMP_Text titleLabel;
        public TMP_Text taglineLabel;
        public TMP_Text instructionTitleLabel;
        public TMP_Text instructionBodyLabel;
        public TMP_Text simulationNoteLabel;
        public TMP_Text statusLabel;
        public Button backButton;
        public Button completeButton;
        public TMP_Text completeLabel;

        public ActivityResultPanel resultPanel;
        public ConfirmDialog dialog;

        private void Start()
        {
            if (activity == null || placeholderOutcome == null)
            {
                Debug.LogError("[MarsOutpost] Activity shell is missing its ActivityDefinition or placeholder outcome.");
                return;
            }

            if (titleLabel != null) titleLabel.text = activity.displayName;
            if (taglineLabel != null) taglineLabel.text = activity.tagline;
            if (instructionTitleLabel != null) instructionTitleLabel.text = activity.instructionTitle;
            if (instructionBodyLabel != null) instructionBodyLabel.text = activity.instructionBody;
            if (simulationNoteLabel != null) simulationNoteLabel.text = activity.simulationNote;
            if (backButton != null) backButton.onClick.AddListener(RequestBack);
            if (completeButton != null) completeButton.onClick.AddListener(CompleteActivity);

            bool alreadyCompleted = Director != null
                                    && Director.Session != null
                                    && Director.Session.HasOutcome(activity.id)
                                    && (Director.Config == null || !Director.Config.allowReplayCompletedActivities);

            if (alreadyCompleted)
            {
                if (statusLabel != null) statusLabel.text = "This training step is already on your mission report.";
                if (completeButton != null) completeButton.interactable = false;
                if (completeLabel != null) completeLabel.text = "Already completed";
            }
            else
            {
                if (statusLabel != null) statusLabel.text = "Ready for your decision.";
                if (completeLabel != null) completeLabel.text = "Complete practice step";
            }
        }

        private void CompleteActivity()
        {
            if (Director == null || Director.Session == null)
            {
                return;
            }

            if (Director.Session.HasOutcome(activity.id) && (Director.Config == null || !Director.Config.allowReplayCompletedActivities))
            {
                return;
            }

            if (completeButton != null) completeButton.interactable = false;
            if (completeLabel != null) completeLabel.text = "Completed";

            ActivityOutcome outcome = Director.CompleteActivity(
                activity.id,
                "phase1-placeholder",
                placeholderOutcome.headline,
                placeholderOutcome.summary,
                placeholderOutcome.learningNote,
                placeholderOutcome.changes);

            resultPanel?.Show(outcome, ContinueToHub);
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
