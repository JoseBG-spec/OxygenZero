using System;
using MarsOutpost.Core;
using UnityEngine;

namespace MarsOutpost.Landing
{
    /// <summary>
    /// A game label for one hazard category. The tone drives the marker dot color;
    /// the label is always shown so color is never the only cue.
    /// </summary>
    [Serializable]
    public sealed class LandingHazardTag
    {
        public string label = "Gentle";

        [Tooltip("0 = friendly, 1 = caution, 2 = challenging. This is a game rating, not a measured value.")]
        [Range(0, 2)]
        public int tone;
    }

    /// <summary>
    /// One fictional landing site on the training map. The tags and consequences are
    /// authored game values, not NASA measurements or Mars Trek layers.
    /// </summary>
    [CreateAssetMenu(fileName = "LandingSite_", menuName = "Mars Outpost/Landing Site", order = 30)]
    public sealed class LandingSiteDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string id = "site";
        public string displayName = "Training Site";
        public string region = "Training region";

        [Header("Map position")]
        [Tooltip("Normalized position on the map: (0,0) is bottom-left, (1,1) is top-right.")]
        public Vector2 mapPosition = new Vector2(0.5f, 0.5f);

        [Header("Hazard tags (game labels)")]
        public LandingHazardTag slope = new LandingHazardTag { label = "Gentle", tone = 0 };
        public LandingHazardTag dust = new LandingHazardTag { label = "Low", tone = 0 };
        public LandingHazardTag distance = new LandingHazardTag { label = "Near", tone = 0 };

        [Header("Site notes")]
        [TextArea(3, 6)] public string description = "";
        [TextArea(2, 4)] public string resultSummary = "";
        [TextArea(2, 4)] public string learningNote = "";

        [Header("Game consequences")]
        public ResourceChange[] changes = new ResourceChange[0];
    }
}
