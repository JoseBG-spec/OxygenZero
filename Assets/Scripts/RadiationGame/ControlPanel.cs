using System;
using UnityEngine;

namespace MarsOutpost.Radiation
{
    /// <summary>
    /// The Radiation Control Room console. It activates shielding, which contains the
    /// simulated hazard and reduces exposure. It does not claim to switch radiation off.
    /// </summary>
    public sealed class ControlPanel : Interactable
    {
        public RadiationConfig config;
        public RadiationManager manager;
        public SpriteRenderer indicatorLamp;
        public Color idleColor = new Color(0.30f, 0.88f, 0.55f, 1f);
        public Color alertColor = new Color(0.98f, 0.35f, 0.30f, 1f);
        public Color activeColor = new Color(0.35f, 0.72f, 1f, 1f);

        public event Action ShieldingActivated;

        public override string Prompt =>
            config != null && !string.IsNullOrWhiteSpace(config.controlPrompt)
                ? config.controlPrompt
                : "Activate Shielding";

        public override bool CanInteract => manager != null && manager.IsAlertActive;

        public override void Interact()
        {
            if (manager == null)
            {
                return;
            }

            if (manager.ActivateShielding())
            {
                SetLamp(activeColor);
                ShieldingActivated?.Invoke();
            }
        }

        private void Awake()
        {
            SetLamp(idleColor);
        }

        private void Update()
        {
            if (manager == null)
            {
                return;
            }

            if (manager.IsContained)
            {
                SetLamp(activeColor);
            }
            else if (manager.IsAlertActive)
            {
                SetLamp(alertColor);
            }
            else
            {
                SetLamp(idleColor);
            }
        }

        private void SetLamp(Color color)
        {
            if (indicatorLamp != null)
            {
                indicatorLamp.color = color;
            }
        }
    }
}
