using System;
using MarsOutpost.Core;
using UnityEngine;

namespace MarsOutpost.Weather
{
    /// <summary>
    /// One crew plan shown after a training alert. The text stays in the data file so a
    /// non-programmer can edit the scenario without touching the scene.
    /// </summary>
    [Serializable]
    public sealed class SpaceWeatherPlanDefinition
    {
        public string id = "plan";

        [TextArea(1, 2)] public string title = "Crew plan";
        [TextArea(2, 3)] public string description = "";

        [TextArea(1, 2)] public string headline = "Plan sent";
        [TextArea(2, 4)] public string feedback = "";
        [TextArea(2, 5)] public string learningNote = "";

        [Tooltip("True for the cautious choice in this training scenario. This is a game answer, not a NASA procedure.")]
        public bool correct;

        [Header("Game consequences")]
        public ResourceChange[] changes = new ResourceChange[0];
    }

    /// <summary>
    /// One simplified, fictional DONKI-inspired training alert. Event-type names are real
    /// NASA DONKI vocabulary; the alert, plans, and scores are written for the game.
    /// </summary>
    [Serializable]
    public sealed class SpaceWeatherAlertDefinition
    {
        public string id = "flr-training";
        public string eventType = "FLR";
        public string eventName = "Solar flare";
        public string alertCode = "TRAINING ALERT FLR-01";

        [TextArea(1, 2)] public string headline = "A bright burst from the Sun";
        [TextArea(2, 4)] public string summary = "";
        [TextArea(1, 3)] public string sourceNote = "";

        public SpaceWeatherPlanDefinition[] choices = new SpaceWeatherPlanDefinition[0];
    }

    /// <summary>The JSON file shape. Unity's JsonUtility cannot parse a bare top-level array.</summary>
    [Serializable]
    public sealed class SpaceWeatherAlertFile
    {
        public SpaceWeatherAlertDefinition[] alerts = new SpaceWeatherAlertDefinition[0];
    }

    /// <summary>Short, kid-facing explanation for the DONKI decoder.</summary>
    [Serializable]
    public sealed class SpaceWeatherEventTypeInfo
    {
        public string code = "FLR";
        public string name = "Solar flare";

        [TextArea(2, 3)] public string description = "";
    }
}
