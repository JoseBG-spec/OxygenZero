using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsOutpost.Radiation
{
    /// <summary>
    /// The detector HUD. It shows a simulated 0-100 reading and a text status, never
    /// a real radiation unit. The red world overlay is a game visualization.
    /// </summary>
    public sealed class Dosimeter : MonoBehaviour
    {
        public RadiationConfig config;
        public RadiationManager manager;
        public Transform player;
        public TMP_Text readoutText;
        public TMP_Text statusText;
        public Image statusPill;

        public float CurrentReading { get; private set; }
        public RadiationStatus CurrentStatus { get; private set; } = RadiationStatus.Safe;

        private void Update()
        {
            if (manager == null || player == null)
            {
                return;
            }

            CurrentReading = manager.GetReadingAt(player.position);
            CurrentStatus = RadiationReadings.Classify(
                CurrentReading,
                config != null ? config.warningReading : 25f,
                config != null ? config.highReading : 60f);

            if (readoutText != null)
            {
                readoutText.text = $"{Mathf.RoundToInt(CurrentReading)} / 100";
            }

            Color color = config != null ? config.ColorFor(CurrentStatus) : Color.white;
            if (statusText != null)
            {
                statusText.text = config != null ? config.LabelFor(CurrentStatus) : CurrentStatus.ToString().ToUpperInvariant();
                statusText.color = color;
            }

            if (statusPill != null)
            {
                statusPill.color = new Color(color.r, color.g, color.b, 0.9f);
            }
        }
    }
}
