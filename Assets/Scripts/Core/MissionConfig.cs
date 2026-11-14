using UnityEngine;

namespace MarsOutpost.Core
{
    /// <summary>
    /// Edit this asset to change the mission title, intro copy, resources, activities,
    /// scene names, and the shared simulation disclaimer. No code changes needed.
    /// </summary>
    [CreateAssetMenu(fileName = "MissionConfig", menuName = "Mars Outpost/Mission Config", order = 0)]
    public sealed class MissionConfig : ScriptableObject
    {
        [Header("Title and framing")]
        public string gameTitle = "Mars Outpost";
        public string subtitle = "Junior Astronaut Training";
        public string missionCode = "TRAINING MISSION 01";

        [Header("Main menu / briefing")]
        public string howToPlayTitle = "How to Play";
        [TextArea(4, 8)] public string howToPlayBody =
            "You are a junior astronaut on a practice mission to Mars.\n\n" +
            "1. Pick a landing site.\n" +
            "2. Run a plant experiment in the greenhouse.\n" +
            "3. Answer a space-weather alert.\n\n" +
            "Every choice changes Power, Crew Safety, and Science Progress. " +
            "There is no single perfect mission - learn from the report at the end!";

        [TextArea(2, 4)] public string introHeadline = "Welcome, Junior Astronaut!";
        [TextArea(4, 8)] public string introBody =
            "Your crew is setting up a practice outpost on Mars. " +
            "Pick a landing site, grow a plant, and answer a space-weather alert. " +
            "Every choice changes your Power, Crew Safety, and Science Progress.";

        [TextArea(2, 4)] public string simulationDisclaimer =
            "Training simulation. Resources, costs, and results are invented for the game - " +
            "not NASA measurements, procedures, or live data.";

        [TextArea(3, 6)] public string credits =
            "Inspired by NASA open science resources: OSDR study OSD-314, the DONKI space-weather " +
            "notification system, and Mars Trek. The landing map is fictional placeholder art, not " +
            "Mars Trek data. This game is not an official NASA product and is not endorsed by NASA.";

        [Header("Content")]
        public ResourceDefinition[] resources = new ResourceDefinition[0];
        public ActivityDefinition[] activities = new ActivityDefinition[0];
        public DebriefConfig debrief;
        public UiTheme theme;
        [Tooltip("Optional. Used when the player turns on high contrast.")]
        public UiTheme highContrastTheme;

        [Header("Scene names (must be in Build Settings)")]
        public string mainMenuScene = "00_MainMenu";
        public string hubScene = "01_MissionHub";
        public string debriefScene = "05_Debrief";

        [Header("Rules")]
        [Tooltip("Science Progress at or above this value earns the top debrief band.")]
        public int scienceTarget = 45;
        public bool allowReplayCompletedActivities = false;
    }
}
