using UnityEngine;

namespace MarsOutpost.Core
{
    /// <summary>
    /// One per scene. Ensures the persistent director exists, which makes every scene
    /// playable directly with the Play button during development.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class MissionSceneBootstrap : MonoBehaviour
    {
        [SerializeField] private MissionConfig config;

        public MissionConfig Config => config;

        public void AssignConfig(MissionConfig value)
        {
            config = value;
        }

        private void Awake()
        {
            if (config == null)
            {
                Debug.LogError($"[MarsOutpost] '{name}' has no MissionConfig assigned. Assign Assets/Data/Configs/MissionConfig.asset.");
                return;
            }

            MissionDirector.Ensure(config);
        }
    }
}
