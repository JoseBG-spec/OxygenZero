using System;
using MarsOutpost.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsOutpost.UI
{
    /// <summary>
    /// Shared end-of-activity card. Every minigame uses this so consequences are always
    /// explained the same way: headline, what happened, resource changes with reasons.
    /// </summary>
    public sealed class ActivityResultPanel : MonoBehaviour
    {
        public TMP_Text titleLabel;
        public TMP_Text summaryLabel;
        public TMP_Text changesLabel;
        public TMP_Text learningLabel;
        public Button continueButton;
        public TMP_Text continueLabel;

        private Action onContinue;
        private bool wired;

        public bool IsOpen => gameObject.activeSelf;

        public void Show(ActivityOutcome outcome, Action onDone)
        {
            if (outcome == null)
            {
                return;
            }

            gameObject.SetActive(true);
            EnsureWired();

            onContinue = onDone;

            if (titleLabel != null)
            {
                titleLabel.text = outcome.Headline;
            }

            if (summaryLabel != null)
            {
                summaryLabel.text = outcome.Summary;
            }

            if (changesLabel != null)
            {
                changesLabel.text = UiTextFormat.FormatChanges(outcome.Changes);
            }

            if (learningLabel != null)
            {
                learningLabel.text = outcome.LearningNote;
                learningLabel.gameObject.SetActive(!string.IsNullOrWhiteSpace(outcome.LearningNote));
            }

            if (continueLabel != null)
            {
                continueLabel.text = "Back to Mission Hub";
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void EnsureWired()
        {
            if (wired)
            {
                return;
            }

            wired = true;
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(HandleContinue);
            }
        }

        private void HandleContinue()
        {
            Action callback = onContinue;
            onContinue = null;
            Hide();
            callback?.Invoke();
        }
    }
}
