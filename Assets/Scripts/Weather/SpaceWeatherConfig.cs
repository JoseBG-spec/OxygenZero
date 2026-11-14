using UnityEngine;

namespace MarsOutpost.Weather
{
    /// <summary>
    /// Phase 4 space-weather content. One curated, fictional training alert is loaded from
    /// a local JSON file through a replaceable source. The authored fallback keeps the
    /// screen playable if the file is missing or cannot be parsed.
    /// </summary>
    [CreateAssetMenu(fileName = "SpaceWeatherConfig", menuName = "Mars Outpost/Space Weather", order = 34)]
    public sealed class SpaceWeatherConfig : ScriptableObject
    {
        [Header("Header")]
        public string screenTitle = "Space Weather";
        public string tagline = "Read the alert. Pick a cautious crew plan.";

        [Header("Control room")]
        public string controlRoomTitle = "SPACE WEATHER STATION";
        public string alertTag = "TRAINING ALERT";
        public string decoderTitle = "DONKI EVENT TYPES";
        public string decoderHint = "These labels name different events. They are not the same hazard.";
        public string compareLabel = "Compare alert types";
        public string backToAlertLabel = "Back to alert";

        [Header("Crew plan")]
        public string decisionTitle = "CREW PLAN";
        public string decisionPrompt = "A training alert is on the screen. Which plan fits this event?";
        public string planHint = "Tap one plan. Then send it to the crew.";
        [Tooltip("Use {0} for the selected plan title.")]
        public string selectedPlanFormat = "Selected: {0}. Ready to send.";
        public string choosePlanLabel = "Choose a crew plan";
        public string confirmLabel = "Send crew plan";
        public string completedLabel = "Crew plan sent";
        [Tooltip("Use {0} for the selected plan title.")]
        public string completedPlanFormat = "Training complete: {0}.";

        [Header("Local curated data")]
        [Tooltip("Optional local JSON file. If this is missing or invalid, the fallback alert below is used.")]
        public TextAsset curatedAlertsJson;
        public SpaceWeatherAlertDefinition fallbackAlert = new SpaceWeatherAlertDefinition();
        public SpaceWeatherEventTypeInfo[] eventTypes = new SpaceWeatherEventTypeInfo[0];

        [Header("Simulation note")]
        [TextArea(2, 4)] public string simulationNote =
            "Training scenario using real DONKI event-type names. Alert, plans, costs, and scores are invented for the game - not a forecast, live data, or operational safety advice.";
    }
}
