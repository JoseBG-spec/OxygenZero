using UnityEngine;

namespace MarsOutpost.Radiation
{
    /// <summary>Something the player can use. The UI action button supplies touch input.</summary>
    public abstract class Interactable : MonoBehaviour
    {
        [SerializeField] private string prompt = "Interact";

        public virtual string Prompt => prompt;
        public virtual bool CanInteract => isActiveAndEnabled;

        public void SetPrompt(string value)
        {
            prompt = value;
        }

        public abstract void Interact();
    }
}
