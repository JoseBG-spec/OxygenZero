using UnityEngine;

namespace MarsOutpost.Landing
{
    /// <summary>
    /// Authoring container for the landing activity. Add or reorder site assets here
    /// and rebuild the Phase 1 content to update the scene markers.
    /// </summary>
    [CreateAssetMenu(fileName = "LandingConfig", menuName = "Mars Outpost/Landing Config", order = 31)]
    public sealed class LandingMissionConfig : ScriptableObject
    {
        [Header("Screen copy")]
        public string screenTitle = "Landing Site";
        public string tagline = "Compare three fictional sites. There is no single perfect choice.";
        [TextArea(2, 4)] public string prompt = "Tap a numbered marker to compare its hazard tags.";
        public string selectedHeader = "SITE DETAILS";
        public string mapCaption = "FICTIONAL TRAINING MAP - not Mars Trek data";
        public string confirmLabel = "Confirm landing site";

        [Header("Sites")]
        public LandingSiteDefinition[] sites = new LandingSiteDefinition[0];
    }
}
