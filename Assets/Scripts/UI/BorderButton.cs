using UnityEngine;
using UnityEngine.UI;

namespace MarsOutpost
{
    public class BorderButton : MonoBehaviour
    {
        public Color effectC;
        public Vector2 distance;
        void Start()
        {
            // Get the Button's Image component
            Image img = GetComponent<Image>();
            if (img == null)
            {
                Debug.LogError("No Image component found on this GameObject.");
                return;
            }

            // Add Outline component if not already present
            Outline outline = gameObject.GetComponent<Outline>();
            if (outline == null)
                outline = gameObject.AddComponent<Outline>();

            // Configure outline
            outline.effectColor = effectC; // Border color
            outline.effectDistance = distance; // Thickness
            outline.useGraphicAlpha = false; // Keep solid color
        }
    }
}
