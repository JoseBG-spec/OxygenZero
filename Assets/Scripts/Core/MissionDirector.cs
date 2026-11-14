using System;
using System.Collections.Generic;
using MarsOutpost.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MarsOutpost.Core
{
    /// <summary>
    /// Persistent owner of mission configuration, session state, and navigation.
    /// Created by the first scene's MissionSceneBootstrap, so any scene can be played
    /// directly in the Editor without a separate boot scene.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class MissionDirector : MonoBehaviour
    {
        public static MissionDirector Instance { get; private set; }

        public MissionConfig Config { get; private set; }
        public MissionSession Session { get; private set; }
        public ActivityOutcome LastOutcome { get; private set; }

        public event Action<MissionSession> SessionChanged;

        public static MissionDirector Ensure(MissionConfig config)
        {
            if (Instance != null)
            {
                if (Instance.Config == null)
                {
                    Instance.Initialize(config);
                }
                else if (config != null && Instance.Config != config)
                {
                    Debug.LogWarning("[MarsOutpost] A different MissionConfig was supplied. Reinitializing the mission session.");
                    Instance.Initialize(config);
                }

                return Instance;
            }

            if (config == null)
            {
                Debug.LogError("[MarsOutpost] MissionSceneBootstrap has no MissionConfig assigned.");
                return null;
            }

            var host = new GameObject("MissionDirector");
            var director = host.AddComponent<MissionDirector>();
            director.Initialize(config);
            DontDestroyOnLoad(host);
            Instance = director;
            return director;
        }

        private void Initialize(MissionConfig config)
        {
            if (config == null)
            {
                Debug.LogError("[MarsOutpost] Cannot initialize without a MissionConfig.");
                return;
            }

            Config = config;
            Session = new MissionSession(config);
            LastOutcome = null;
            UiThemeService.Initialize(config);

            List<string> errors = MissionDataValidator.Validate(config);
            foreach (string error in errors)
            {
                Debug.LogError($"[MarsOutpost] Mission data: {error}");
            }
        }

        private void Awake()
        {
            // Mobile-first: a steady 60 fps target with simple UI.
            Application.targetFrameRate = 60;
        }

        public bool TryStartNewMission(out string error)
        {
            if (Config == null || Session == null)
            {
                error = "Mission data is missing.";
                return false;
            }

            Session.Reset();
            LastOutcome = null;
            SessionChanged?.Invoke(Session);
            return MissionFlow.TryLoad(Config.hubScene, out error);
        }

        public bool TryRestartMission(out string error)
        {
            return TryStartNewMission(out error);
        }

        public ActivityOutcome CompleteActivity(
            string activityId,
            string choiceId,
            string headline,
            string summary,
            string learningNote,
            IEnumerable<ResourceChange> changes)
        {
            if (Session == null)
            {
                Debug.LogError("[MarsOutpost] CompleteActivity called before the session was initialized.");
                return null;
            }

            ActivityOutcome outcome = Session.ApplyOutcome(activityId, choiceId, headline, summary, learningNote, changes);
            LastOutcome = outcome;
            SessionChanged?.Invoke(Session);
            return outcome;
        }

        public bool TryLoadScene(string sceneName, out string error)
        {
            return MissionFlow.TryLoad(sceneName, out error);
        }

        public bool TryLoadHub(out string error)
        {
            return MissionFlow.TryLoad(Config != null ? Config.hubScene : null, out error);
        }

        public bool TryLoadDebrief(out string error)
        {
            return MissionFlow.TryLoad(Config != null ? Config.debriefScene : null, out error);
        }

        public bool TryLoadMainMenu(out string error)
        {
            return MissionFlow.TryLoad(Config != null ? Config.mainMenuScene : null, out error);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Instance = null;
        }
    }
}
