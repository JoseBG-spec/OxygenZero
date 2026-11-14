using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsOutpost.Radiation
{
    /// <summary>
    /// Tracks the player's simulated exposure. When the meter fills, the controller gives
    /// a calm explanation and returns the player to the crew hub. No real dose is tracked.
    /// </summary>
    public sealed class PlayerRadiationExposure : MonoBehaviour
    {
        public RadiationConfig config;
        public RadiationManager manager;
        public Image exposureFill;
        public TMP_Text exposureLabel;

        public bool IsPaused { get; private set; }
        public float Exposure { get; private set; }
        public float Exposure01 => Mathf.Clamp01(Exposure / Mathf.Max(1f, MaxExposure));
        public float CurrentLevel { get; private set; }

        public event Action ExposureLimitReached;

        private bool limitNotified;
        private float MaxExposure => config != null ? config.maxExposure : 100f;

        private void Start()
        {
            ResetExposure();
        }

        private void Update()
        {
            if (IsPaused || manager == null)
            {
                return;
            }

            CurrentLevel = manager.GetLevelAt(transform.position);
            float gain = config != null ? config.exposureGainPerSecond : 14f;
            float recovery = config != null ? config.exposureRecoveryPerSecond : 8f;

            if (CurrentLevel > 0f)
            {
                Exposure += gain * CurrentLevel * Time.deltaTime;
            }
            else
            {
                Exposure -= recovery * Time.deltaTime;
            }

            Exposure = Mathf.Clamp(Exposure, 0f, MaxExposure);
            RefreshHud();

            if (Exposure >= MaxExposure && !limitNotified)
            {
                limitNotified = true;
                ExposureLimitReached?.Invoke();
            }
        }

        public void SetPaused(bool paused)
        {
            IsPaused = paused;
        }

        public void ResetExposure()
        {
            Exposure = 0f;
            CurrentLevel = 0f;
            limitNotified = false;
            RefreshHud();
        }

        private void RefreshHud()
        {
            if (exposureFill != null)
            {
                exposureFill.fillAmount = Exposure01;
            }

            if (exposureLabel != null)
            {
                exposureLabel.text = $"{Mathf.RoundToInt(Exposure01 * 100f)}%";
            }
        }
    }
}
