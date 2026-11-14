using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsOutpost.UI
{
    /// <summary>
    /// Accessible mobile modal with a full-screen dimmer, large confirm/cancel targets,
    /// and a single-button info mode for help and sources.
    /// </summary>
    public sealed class ConfirmDialog : MonoBehaviour
    {
        public TMP_Text titleLabel;
        public TMP_Text bodyLabel;
        public Button confirmButton;
        public Button cancelButton;
        public TMP_Text confirmLabel;
        public TMP_Text cancelLabel;

        private Action onConfirm;
        private Action onCancel;
        private bool wired;

        public bool IsOpen => gameObject.activeSelf;

        public void Show(
            string title,
            string body,
            string confirmText,
            string cancelText,
            Action confirmAction = null,
            Action cancelAction = null)
        {
            gameObject.SetActive(true);
            EnsureWired();

            if (titleLabel != null)
            {
                titleLabel.text = title;
            }

            if (bodyLabel != null)
            {
                bodyLabel.text = body;
            }

            if (confirmLabel != null)
            {
                confirmLabel.text = string.IsNullOrWhiteSpace(confirmText) ? "OK" : confirmText;
            }

            bool hasCancel = !string.IsNullOrWhiteSpace(cancelText);
            if (cancelLabel != null)
            {
                cancelLabel.text = hasCancel ? cancelText : string.Empty;
            }

            if (cancelButton != null)
            {
                cancelButton.gameObject.SetActive(hasCancel);
            }

            onConfirm = confirmAction;
            onCancel = cancelAction;
        }

        public void ShowInfo(string title, string body, Action onClose = null)
        {
            Show(title, body, "Got it", null, onClose);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            onConfirm = null;
            onCancel = null;
        }

        private void EnsureWired()
        {
            if (wired)
            {
                return;
            }

            wired = true;

            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(HandleConfirm);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(HandleCancel);
            }
        }

        private void HandleConfirm()
        {
            Action callback = onConfirm;
            Hide();
            callback?.Invoke();
        }

        private void HandleCancel()
        {
            Action callback = onCancel;
            Hide();
            callback?.Invoke();
        }
    }
}
