using UnityEngine;

namespace MarsOutpost.Radiation
{
    /// <summary>
    /// All kid-facing radiation-drill copy and tuning in one place. The values here are
    /// game values only: there are deliberately no real-world radiation units in this game.
    /// </summary>
    [CreateAssetMenu(fileName = "RadiationConfig", menuName = "Mars Outpost/Radiation Config", order = 40)]
    public sealed class RadiationConfig : ScriptableObject
    {
        [Header("Screen")]
        public string screenTitle = "Radiation Drill";
        public string tagline = "Monitor a simulated alert and activate shielding.";
        public string instructionTitle = "Radiation Drill";
        [TextArea(4, 8)] public string instructionBody =
            "A simulated radiation alert will begin in one of the ship's rooms. " +
            "Read the detector, talk to the Safety Officer, then reach the Radiation Control Room and activate shielding.\n\n" +
            "The red warning area is a game visualization, not visible radiation. The detector numbers are simulated " +
            "game readings. This drill is not real radiation-safety training.";
        public string startButtonLabel = "Start Drill";
        public string objectiveLabel = "OBJECTIVE";
        public string objectiveTalk = "Talk to the Safety Officer.";
        public string objectiveReach = "Reach the Radiation Control Room and activate shielding.";
        public string objectiveDone = "Shielding active. Simulated exposure reduced.";
        public string controlsHint = "Drag the joystick to move. Tap the action button when it appears.";
        public string legend =
            "Red area = game visualization of a simulated warning zone. It is not visible radiation.";

        [Header("Detector")]
        public string detectorCaption = "SIMULATED READING";
        public string exposureCaption = "SIMULATED EXPOSURE";
        public string detectorSafe = "SAFE";
        public string detectorWarning = "WARNING";
        public string detectorHigh = "HIGH";
        [Range(0f, 100f)] public float warningReading = 25f;
        [Range(0f, 100f)] public float highReading = 60f;

        [Header("Safety Officer dialogue")]
        public string npcName = "Safety Officer";
        public string npcPrompt = "Talk to the Safety Officer";
        [TextArea(2, 8)] public string[] dialoguePages =
        {
            "Hi, Junior Astronaut. A simulated radiation alert is running on the ship. I can explain what radiation is and how our detector works. Ready?",
            "Radiation is energy that travels as waves or particles. We meet some radiation in everyday life. Too much exposure can damage cells and make people sick, so astronauts monitor it carefully.",
            "The red warning area on your display is a game visualization of a simulated hazard. It is not something you can see. The detector shows a simulated reading, not a real measurement.",
            "When the reading rises, put distance between yourself and the warning area, and use shielding. In this drill, the Radiation Control Room can activate shielding to reduce exposure.",
            "Go to the Radiation Control Room and activate shielding. The red area should shrink as the simulated exposure is reduced. This is a practice drill, so take your time."
        };
        public string dialogueContinue = "Continue";
        public string dialogueFinish = "Finish";
        public string dialogueClose = "Skip";
        public string dialogueHint = "Tap Continue, or press Space / Enter.";

        [Header("Control room")]
        public string controlPrompt = "Activate Shielding";
        [TextArea(2, 4)] public string shieldMessage =
            "Shielding active. The simulated warning area is shrinking and exposure is being reduced.";
        public string shieldUnavailable =
            "The shielding control is ready. Wait for the simulated alert before activating it.";

        [Header("Gentle reset")]
        [TextArea(2, 5)] public string exposureLimitMessage =
            "The simulated exposure meter filled, so this practice run returned you to the crew hub. " +
            "Watch the detector and move away from the red warning area sooner. You can try again.";

        [Header("Debrief")]
        public string debriefHeadline = "Radiation Drill complete";
        [TextArea(2, 5)] public string debriefSummary =
            "You talked with the Safety Officer, watched the simulated detector, reached the Radiation Control Room, and activated shielding.";
        [TextArea(4, 8)] public string debriefLearning =
            "<b>What radiation is:</b> energy travelling as waves or particles.\n" +
            "<b>Why we monitor it:</b> a detector tracks exposure so the crew can act before it gets too high.\n" +
            "<b>How shielding helps:</b> shielding and distance reduce simulated exposure. The warning area, readings, and timing in this drill are game values, not real measurements or safety training.";
        public string choiceId = "shielding-active";
        public string activityId = "radiation";

        [Header("Tuning (game values)")]
        [Min(0f)] public float firstAlertDelay = 5f;
        [Min(0.01f)] public float spreadSpeed = 0.35f;
        [Min(0.25f)] public float maxRadius = 4.8f;
        [Min(0.1f)] public float containmentSpeed = 4f;
        [Min(0f)] public float resultDelay = 1.2f;
        [Min(1f)] public float maxExposure = 100f;
        [Min(0f)] public float exposureGainPerSecond = 14f;
        [Min(0f)] public float exposureRecoveryPerSecond = 8f;
        [Min(0f)] public float toastSeconds = 4.5f;

        [Header("Colors")]
        public Color safeColor = new Color(0.29f, 0.87f, 0.50f, 1f);
        public Color warningColor = new Color(0.98f, 0.75f, 0.14f, 1f);
        public Color highColor = new Color(0.97f, 0.44f, 0.44f, 1f);
        public Color hazardColor = new Color(1f, 0.12f, 0.12f, 1f);

        public string LabelFor(RadiationStatus status)
        {
            switch (status)
            {
                case RadiationStatus.High: return detectorHigh;
                case RadiationStatus.Warning: return detectorWarning;
                default: return detectorSafe;
            }
        }

        public Color ColorFor(RadiationStatus status)
        {
            switch (status)
            {
                case RadiationStatus.High: return highColor;
                case RadiationStatus.Warning: return warningColor;
                default: return safeColor;
            }
        }
    }
}
