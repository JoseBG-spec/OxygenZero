using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MarsOutpost.Radiation
{
    /// <summary>
    /// Finds the nearest nearby Interactable and exposes a single action for the touch
    /// button. E remains a keyboard fallback, but it is never the only way to interact.
    /// </summary>
    public sealed class PlayerInteractor : MonoBehaviour
    {
        public TMP_Text promptText;
        public bool InputEnabled { get; private set; } = true;
        public Interactable Nearest { get; private set; }
        public bool HasTarget => Nearest != null && Nearest.CanInteract;

        private readonly List<Interactable> inRange = new List<Interactable>();

        private void Update()
        {
            RefreshNearest();

            if (promptText != null)
            {
                bool show = InputEnabled && HasTarget;
                promptText.gameObject.SetActive(show);
                if (show)
                {
                    promptText.text = Nearest.Prompt;
                }
            }

            if (InputEnabled && HasTarget && GameInput.InteractPressed())
            {
                InteractNearest();
            }
        }

        public void SetInputEnabled(bool value)
        {
            InputEnabled = value;
            if (promptText != null && !value)
            {
                promptText.gameObject.SetActive(false);
            }
        }

        public void InteractNearest()
        {
            RefreshNearest();
            if (HasTarget)
            {
                Nearest.Interact();
            }
        }

        private void RefreshNearest()
        {
            Interactable best = null;
            float bestDistance = float.MaxValue;
            for (int i = inRange.Count - 1; i >= 0; i--)
            {
                Interactable candidate = inRange[i];
                if (candidate == null || !candidate.isActiveAndEnabled)
                {
                    inRange.RemoveAt(i);
                    continue;
                }

                float distance = Vector2.Distance(transform.position, candidate.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = candidate;
                }
            }

            Nearest = best;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Interactable interactable = other.GetComponentInParent<Interactable>();
            if (interactable != null && !inRange.Contains(interactable))
            {
                inRange.Add(interactable);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            Interactable interactable = other.GetComponentInParent<Interactable>();
            if (interactable != null)
            {
                inRange.Remove(interactable);
            }
        }
    }
}
