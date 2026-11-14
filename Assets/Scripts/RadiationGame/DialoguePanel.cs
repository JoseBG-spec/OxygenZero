using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsOutpost.Radiation
{
    /// <summary>
    /// Touch-first dialogue. Continue and Skip are real buttons; Space/Enter is a
    /// keyboard fallback. The controller pauses the drill while this panel is open.
    /// </summary>
    public sealed class DialoguePanel : MonoBehaviour
    {
        public RadiationConfig config;
        public TMP_Text speakerText;
        public TMP_Text bodyText;
        public TMP_Text hintText;
        public TMP_Text pageText;
        public TMP_Text continueLabel;
        public TMP_Text closeLabel;
        public Button continueButton;
        public Button closeButton;

        public bool IsOpen { get; private set; }

        public event Action Opened;
        public event Action Closed;

        private string[] pages;
        private int index;
        private int openedFrame = -1;
        private bool wired;

        public void Open(string speaker, string[] newPages)
        {
            if (newPages == null || newPages.Length == 0)
            {
                return;
            }

            pages = newPages;
            index = 0;
            openedFrame = Time.frameCount;
            IsOpen = true;
            gameObject.SetActive(true);
            EnsureWired();

            if (speakerText != null)
            {
                speakerText.text = speaker;
            }

            ShowPage();
            Opened?.Invoke();
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            IsOpen = false;
            gameObject.SetActive(false);
            Closed?.Invoke();
        }

        public void Advance()
        {
            if (!IsOpen || pages == null)
            {
                return;
            }

            index++;
            if (index >= pages.Length)
            {
                Close();
                return;
            }

            ShowPage();
        }

        private void Update()
        {
            if (!IsOpen || Time.frameCount == openedFrame)
            {
                return;
            }

            if (GameInput.DialogueAdvancePressed())
            {
                Advance();
            }
        }

        private void ShowPage()
        {
            if (bodyText != null)
            {
                bodyText.text = pages[index];
            }

            if (pageText != null)
            {
                pageText.text = $"{index + 1} / {pages.Length}";
            }

            if (hintText != null)
            {
                hintText.text = config != null
                    ? config.dialogueHint
                    : "Tap Continue, or press Space / Enter.";
            }

            bool last = index >= pages.Length - 1;
            if (continueLabel != null)
            {
                continueLabel.text = last
                    ? (config != null ? config.dialogueFinish : "Finish")
                    : (config != null ? config.dialogueContinue : "Continue");
            }

            if (closeLabel != null)
            {
                closeLabel.text = config != null ? config.dialogueClose : "Skip";
            }
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
                continueButton.onClick.AddListener(Advance);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }
        }
    }
}
