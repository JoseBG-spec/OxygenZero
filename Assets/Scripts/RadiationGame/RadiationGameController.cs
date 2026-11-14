using System;
using System.Collections;
using MarsOutpost.Core;
using MarsOutpost.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MarsOutpost.Radiation
{
    /// <summary>
    /// Flow for the radiation drill: intro, calm alert, touch movement, detector,
    /// guide dialogue, control-room shielding, gentle reset, debrief, and hub return.
    /// </summary>
    public sealed class RadiationGameController : MissionScreen
    {
        [Header("Data")]
        public RadiationConfig config;

        [Header("World")]
        public RadiationManager hazard;
        public PlayerController2D player;
        public PlayerRadiationExposure exposure;
        public PlayerInteractor interactor;
        public Dosimeter detector;
        public NpcGuide npc;
        public ControlPanel panel;
        public TouchJoystick joystick;
        public Transform respawnPoint;

        [Header("UI")]
        public GameObject introRoot;
        public Button startButton;
        public Button introReturnButton;
        public TMP_Text introTitleLabel;
        public TMP_Text introInstructionsLabel;
        public TMP_Text taglineLabel;
        public TMP_Text objectiveHeaderLabel;
        public TMP_Text objectiveLabel;
        public TMP_Text legendLabel;
        public TMP_Text detectorCaptionLabel;
        public TMP_Text exposureCaptionLabel;
        public GameObject pauseRoot;
        public Button pauseButton;
        public Button resumeButton;
        public Button restartButton;
        public Button returnButton;
        public Button returnTopButton;
        public GameObject toastRoot;
        public TMP_Text toastLabel;
        public Button interactButton;
        public TMP_Text interactLabel;
        public DialoguePanel dialogue;
        public ActivityResultPanel resultPanel;
        public ConfirmDialog dialog;

        private bool started;
        private bool paused;
        private bool completed;
        private bool talking;
        private bool exposureDialogOpen;
        private Coroutine toastRoutine;

        private void Start()
        {
            ApplyConfig();

            if (config == null || hazard == null || player == null || resultPanel == null)
            {
                Debug.LogError("[MarsOutpost] Radiation drill is missing its config or scene references.");
                return;
            }

            if (startButton != null) startButton.onClick.AddListener(StartDrill);
            if (introReturnButton != null) introReturnButton.onClick.AddListener(ReturnToHub);
            if (pauseButton != null) pauseButton.onClick.AddListener(OpenPause);
            if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
            if (restartButton != null) restartButton.onClick.AddListener(ConfirmRestart);
            if (returnButton != null) returnButton.onClick.AddListener(ConfirmReturn);
            if (returnTopButton != null) returnTopButton.onClick.AddListener(ConfirmReturn);
            if (interactButton != null) interactButton.onClick.AddListener(OnInteractPressed);

            if (dialogue != null)
            {
                dialogue.Opened += OnDialogueOpened;
                dialogue.Closed += OnDialogueClosed;
            }

            hazard.AlertStarted += OnAlertStarted;
            hazard.ShieldingActivated += OnShieldingActivated;
            exposure.ExposureLimitReached += OnExposureLimitReached;

            if (introRoot != null) introRoot.SetActive(true);
            if (pauseRoot != null) pauseRoot.SetActive(false);
            if (toastRoot != null) toastRoot.SetActive(false);
            if (resultPanel != null) resultPanel.Hide();
            if (interactButton != null) interactButton.gameObject.SetActive(false);

            hazard.ResetDrill();
            exposure.ResetExposure();
            SetObjective(config.objectiveTalk);
            SetPaused(true);
        }

        protected override void Update()
        {
            base.Update();

            if (interactor == null || interactButton == null)
            {
                return;
            }

            bool show = started && !paused && !completed && interactor.HasTarget;
            interactButton.gameObject.SetActive(show);
            if (show && interactLabel != null)
            {
                interactLabel.text = interactor.Nearest.Prompt;
            }
        }

        private void ApplyConfig()
        {
            if (config == null)
            {
                return;
            }

            if (introTitleLabel != null) introTitleLabel.text = config.screenTitle;
            if (introInstructionsLabel != null) introInstructionsLabel.text = config.instructionBody;
            if (taglineLabel != null) taglineLabel.text = config.tagline;
            if (objectiveHeaderLabel != null) objectiveHeaderLabel.text = config.objectiveLabel;
            if (legendLabel != null) legendLabel.text = config.legend + "\n" + config.controlsHint;
            if (detectorCaptionLabel != null) detectorCaptionLabel.text = config.detectorCaption;
            if (exposureCaptionLabel != null) exposureCaptionLabel.text = config.exposureCaption;

            if (hazard != null) hazard.config = config;
            if (exposure != null) exposure.config = config;
            if (detector != null) detector.config = config;
            if (npc != null) npc.config = config;
            if (panel != null) panel.config = config;
            if (dialogue != null) dialogue.config = config;
        }

        private void StartDrill()
        {
            if (started)
            {
                return;
            }

            started = true;
            if (introRoot != null) introRoot.SetActive(false);
            SetPaused(false);
            SetObjective(config.objectiveTalk);
            ShowToast("Simulated alert starting. Watch the detector and talk to the Safety Officer.");
            StartCoroutine(BeginAlertAfterDelay());
        }

        private IEnumerator BeginAlertAfterDelay()
        {
            float delay = Mathf.Max(0f, config.firstAlertDelay);
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            while (paused && !completed)
            {
                yield return null;
            }

            if (!completed && hazard != null)
            {
                hazard.BeginAlert();
            }
        }

        private void OnAlertStarted()
        {
            SetObjective(config.objectiveTalk);
            ShowToast("A simulated radiation alert has begun. Read the detector, then find the Safety Officer.");
        }

        private void OnDialogueOpened()
        {
            talking = true;
            SetPaused(true);
            if (interactButton != null) interactButton.gameObject.SetActive(false);
        }

        private void OnDialogueClosed()
        {
            if (!talking)
            {
                return;
            }

            talking = false;
            SetObjective(config.objectiveReach);
            SetPaused(false);
            ShowToast("Safety Officer: go to the Radiation Control Room and activate shielding.");
        }

        private void OnShieldingActivated()
        {
            if (completed)
            {
                return;
            }

            SetObjective(config.objectiveDone);
            ShowToast(config.shieldMessage);
            StartCoroutine(CompleteAfterDelay());
        }

        private IEnumerator CompleteAfterDelay()
        {
            float delay = Mathf.Max(0f, config.resultDelay);
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            CompleteDrill();
        }

        private void CompleteDrill()
        {
            if (completed)
            {
                return;
            }

            completed = true;
            SetPaused(true);
            if (interactButton != null) interactButton.gameObject.SetActive(false);

            if (Director == null)
            {
                Debug.LogError("[MarsOutpost] Radiation drill cannot complete without MissionDirector.");
                if (dialog != null)
                {
                    dialog.ShowInfo("Mission data missing", "The radiation drill could not record a result. Return to the Mission Hub and try again.");
                }

                return;
            }

            ActivityOutcome outcome = Director.CompleteActivity(
                config.activityId,
                config.choiceId,
                config.debriefHeadline,
                config.debriefSummary,
                config.debriefLearning,
                Array.Empty<ResourceChange>());

            if (outcome == null)
            {
                Debug.LogError("[MarsOutpost] Radiation drill did not receive an activity outcome.");
                return;
            }

            if (resultPanel != null)
            {
                resultPanel.Show(outcome, ReturnToHub);
            }
        }

        private void OpenPause()
        {
            if (completed || (introRoot != null && introRoot.activeSelf))
            {
                return;
            }

            if (pauseRoot != null) pauseRoot.SetActive(true);
            SetPaused(true);
        }

        private void Resume()
        {
            if (pauseRoot != null) pauseRoot.SetActive(false);
            if (dialogue != null && dialogue.IsOpen)
            {
                return;
            }

            SetPaused(false);
        }

        private void ConfirmRestart()
        {
            if (dialog == null)
            {
                return;
            }

            SetPaused(true);
            dialog.Show(
                "Restart the drill?",
                "The simulated alert will reset and you will start from the crew hub again.",
                "Restart",
                "Stay",
                RestartScene,
                ResumeIfClear);
        }

        private void ConfirmReturn()
        {
            if (completed)
            {
                ReturnToHub();
                return;
            }

            if (dialog == null)
            {
                return;
            }

            SetPaused(true);
            dialog.Show(
                "Return to the ship?",
                "This drill will stop. You can try it again from the Mission Hub.",
                "Return",
                "Stay",
                ReturnToHub,
                ResumeIfClear);
        }

        private void RestartScene()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (!MissionFlow.TryLoad(sceneName, out string error))
            {
                dialog.ShowInfo("Could not restart", error);
            }
        }

        private void ReturnToHub()
        {
            if (Director == null)
            {
                return;
            }

            if (!Director.TryLoadHub(out string error))
            {
                dialog.ShowInfo("Could not return to the hub", error);
            }
        }

        private void OnInteractPressed()
        {
            if (!paused && interactor != null)
            {
                interactor.InteractNearest();
            }
        }

        private void OnExposureLimitReached()
        {
            if (completed || exposureDialogOpen || dialog == null)
            {
                return;
            }

            exposureDialogOpen = true;
            SetPaused(true);
            dialog.ShowInfo(
                "Exposure meter filled",
                config.exposureLimitMessage,
                HandleExposureReset);
        }

        private void HandleExposureReset()
        {
            exposureDialogOpen = false;
            if (respawnPoint != null)
            {
                player.Teleport(respawnPoint.position);
            }

            exposure.ResetExposure();
            ShowToast("Back at the crew hub. Move away from the red warning area sooner.");
            ResumeIfClear();
        }

        private void ResumeIfClear()
        {
            if (pauseRoot != null && pauseRoot.activeSelf)
            {
                return;
            }

            if (introRoot != null && introRoot.activeSelf)
            {
                return;
            }

            if (dialogue != null && dialogue.IsOpen)
            {
                return;
            }

            if (resultPanel != null && resultPanel.IsOpen)
            {
                return;
            }

            if (dialog != null && dialog.IsOpen)
            {
                return;
            }

            SetPaused(false);
        }

        private void SetPaused(bool value)
        {
            paused = value;
            if (hazard != null) hazard.SetPaused(value);
            if (player != null) player.SetInputEnabled(!value);
            if (exposure != null) exposure.SetPaused(value);
            if (interactor != null) interactor.SetInputEnabled(!value);
            if (joystick != null) joystick.SetInteractable(!value);
            if (value && interactButton != null)
            {
                interactButton.gameObject.SetActive(false);
            }
        }

        private void SetObjective(string text)
        {
            if (objectiveLabel != null)
            {
                objectiveLabel.text = text;
            }
        }

        private void ShowToast(string message)
        {
            if (toastRoot == null || toastLabel == null)
            {
                return;
            }

            if (toastRoutine != null)
            {
                StopCoroutine(toastRoutine);
            }

            toastLabel.text = message;
            toastRoot.SetActive(true);
            toastRoutine = StartCoroutine(HideToastAfter(Mathf.Max(0.5f, config.toastSeconds)));
        }

        private IEnumerator HideToastAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (toastRoot != null)
            {
                toastRoot.SetActive(false);
            }

            toastRoutine = null;
        }

        protected override void HandleBack()
        {
            if (dialog != null && dialog.IsOpen)
            {
                dialog.Hide();
                ResumeIfClear();
                return;
            }

            if (resultPanel != null && resultPanel.IsOpen)
            {
                ReturnToHub();
                return;
            }

            if (dialogue != null && dialogue.IsOpen)
            {
                dialogue.Close();
                return;
            }

            if (pauseRoot != null && pauseRoot.activeSelf)
            {
                Resume();
                return;
            }

            if (introRoot != null && introRoot.activeSelf)
            {
                ReturnToHub();
                return;
            }

            OpenPause();
        }
    }
}
