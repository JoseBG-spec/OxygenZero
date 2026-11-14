using UnityEngine;

namespace MarsOutpost.Core
{
    /// <summary>
    /// An activity shown on the mission hub. Scene names are data, so a designer can
    /// swap an activity scene without editing navigation code.
    /// </summary>
    [CreateAssetMenu(fileName = "Activity_", menuName = "Mars Outpost/Activity Definition", order = 20)]
    public sealed class ActivityDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string id = "landing";
        public string displayName = "Landing Site";
        public string tagline = "Choose where the crew touches down.";
        [TextArea(2, 4)] public string description = "";
        [Tooltip("Optional icon. A numbered circle is used when this is empty.")]
        public Sprite icon;

        [Header("Activity card")]
        [Min(0)] public int hubOrder = 0;
        [Min(1)] public int estimatedMinutes = 2;
        public bool requiredForMission = true;
        [TextArea(2, 4)] public string simulationNote = "This activity is a game simulation, not a NASA procedure or measurement.";

        [Header("Scene")]
        [Tooltip("Scene name exactly as it appears in Build Settings.")]
        public string sceneName = "02_LandingSite";

        [Header("Instruction card")]
        public string instructionTitle = "How this works";
        [TextArea(3, 6)] public string instructionBody = "";
    }
}
