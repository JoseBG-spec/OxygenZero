using UnityEngine;

namespace MarsOutpost.Core
{
    /// <summary>
    /// One of the small, invented game resources (Power, Crew Safety, Science Progress).
    /// Values are game balance only - they are not NASA measurements.
    /// </summary>
    [CreateAssetMenu(fileName = "Resource_", menuName = "Mars Outpost/Resource Definition", order = 10)]
    public sealed class ResourceDefinition : ScriptableObject
    {
        [Header("Identity (safe to rename)")]
        [Tooltip("Stable id used by data. Lowercase, no spaces.")]
        public string id = "power";
        public string displayName = "Power";
        [Tooltip("Very short label for the compact mobile HUD.")]
        public string shortLabel = "PWR";
        [TextArea(2, 3)] public string kidExplanation = "Power runs the outpost. Activities use it.";
        public Color color = new Color(1f, 0.72f, 0.16f, 1f);
        [Tooltip("Optional icon. A colored circle is used when this is empty.")]
        public Sprite icon;

        [Header("Game values (invented for the simulation)")]
        [Min(0)] public int startingValue = 100;
        public int minValue = 0;
        [Min(1)] public int maxValue = 100;

        public string IdOrFallback => string.IsNullOrWhiteSpace(id) ? name : id;

        public int Clamp(int value) => Mathf.Clamp(value, minValue, maxValue);

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                id = name.ToLowerInvariant();
            }

            if (maxValue < minValue)
            {
                maxValue = minValue;
            }

            startingValue = Mathf.Clamp(startingValue, minValue, maxValue);
        }
    }
}
