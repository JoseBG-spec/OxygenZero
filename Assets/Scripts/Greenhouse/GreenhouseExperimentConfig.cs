using System;
using MarsOutpost.Core;
using UnityEngine;

namespace MarsOutpost.Greenhouse
{
    /// <summary>
    /// One simplified, qualitative readout after the trial. These are game signals,
    /// not measurements or growth percentages.
    /// </summary>
    [Serializable]
    public sealed class GreenhouseObservation
    {
        public string growthLabel = "CELL GROWTH";
        public string growthValue = "Recovery signal";

        [Tooltip("0 = friendly, 1 = caution, 2 = challenging. This is a game rating, not a measured value.")]
        [Range(0, 2)]
        public int growthTone;

        public string stressLabel = "STRESS RESPONSE";
        public string stressValue = "Active";

        [Tooltip("0 = friendly, 1 = caution, 2 = challenging. This is a game rating, not a measured value.")]
        [Range(0, 2)]
        public int stressTone = 1;
    }

    /// <summary>
    /// One possible sentence a junior astronaut can log. Only the matching claim earns
    /// Science Progress; the other claims are useful mistakes with specific feedback.
    /// </summary>
    [Serializable]
    public sealed class GreenhouseClaimDefinition
    {
        public string id = "claim";
        public string headline = "Claim logged";

        [TextArea(2, 3)] public string statement = "";

        [TextArea(2, 3)] public string feedback = "";

        [TextArea(2, 4)] public string learningNote = "";

        [Tooltip("True for the one claim supported by the OSD-314 summary.")]
        public bool matchesStudy;

        [Header("Game consequences")]
        public ResourceChange[] changes = new ResourceChange[0];
    }

    /// <summary>
    /// Phase 3 greenhouse content. The activity is a controlled red-light versus dark
    /// comparison inspired by NASA OSDR study OSD-314. All costs, signals, and scores
    /// are game values, not NASA measurements or farm predictions.
    /// </summary>
    [CreateAssetMenu(fileName = "GreenhouseConfig", menuName = "Mars Outpost/Greenhouse Experiment", order = 32)]
    public sealed class GreenhouseExperimentConfig : ScriptableObject
    {
        [Header("Header")]
        public string screenTitle = "Plant Lab";
        public string tagline = "Run a fair red-light trial with Mars-like seedlings.";

        [Header("Growth chamber")]
        public string chamberTitle = "GROWTH CHAMBER";
        public string environmentLabel = "MARS-LIKE GRAVITY";
        public string trayALabel = "TRAY A";
        public string trayBLabel = "TRAY B";
        public string chooseTreatmentLabel = "Choose a treatment";
        public string redTreatmentLabel = "RED LIGHT";
        public string darkTreatmentLabel = "DARK CONTROL";
        public string redButtonLabel = "Red";
        public string darkButtonLabel = "Dark";

        [Header("Notebook")]
        public string notebookTitle = "LAB NOTEBOOK";
        public string setupHeader = "STEP 1 - SET THE TREATMENTS";
        [TextArea(2, 4)] public string setupBody =
            "Give one tray red light. Keep the other dark as the control. Change only the light.";
        public string setupIncomplete = "A fair test uses one red-light tray and one dark control.";
        public string setupBothRed = "Both trays have red light. One must stay dark as the control.";
        public string setupBothDark = "Both trays are dark. One needs red light for the treatment.";

        [Tooltip("Use {0} for the red-light tray name and {1} for the dark-control tray name.")]
        public string setupReadyFormat = "Fair test ready: {0} red light, {1} dark control.";

        [Header("Run")]
        public string runLabel = "Run light trial";
        [Tooltip("Use {0} for the Power cost.")]
        public string runCostFormat = "Uses {0} Power";
        [Min(0)] public int runPowerCost = 8;

        [Header("Observations")]
        public string observationsHeader = "STEP 2 - COMPARE THE SIGNALS";
        [TextArea(2, 4)] public string observationsBody =
            "Both trays had the same seedlings and Mars-like gravity. Only the light changed.";
        public GreenhouseObservation redObservation = new GreenhouseObservation
        {
            growthLabel = "CELL GROWTH",
            growthValue = "Recovery signal",
            growthTone = 0,
            stressLabel = "STRESS RESPONSE",
            stressValue = "Active",
            stressTone = 1
        };
        public GreenhouseObservation darkObservation = new GreenhouseObservation
        {
            growthLabel = "CELL GROWTH",
            growthValue = "Reduced signal",
            growthTone = 2,
            stressLabel = "STRESS RESPONSE",
            stressValue = "Active",
            stressTone = 1
        };

        [Header("Claim check")]
        public string claimsHeader = "STEP 3 - CHOOSE A CAREFUL CLAIM";
        public string claimPrompt = "Which sentence is supported by the study?";
        public string claimHint = "Tap one claim. Then log your finding.";
        public string confirmLabel = "Log finding";
        public string selectClaimLabel = "Choose a claim first";
        public string completedLabel = "Trial complete";
        public GreenhouseClaimDefinition[] claims = new GreenhouseClaimDefinition[0];

        [Header("Simulation note")]
        [TextArea(2, 4)] public string simulationNote =
            "Inspired by NASA OSDR study OSD-314. Trial signals, power cost, and science points are invented for the game - not NASA measurements or farm predictions.";
    }
}
