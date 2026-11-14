using UnityEngine;

namespace MarsOutpost.Core
{
    /// <summary>
    /// Authored placeholder outcome for the Phase 1 activity shells. Phases 2-4 replace
    /// these with real activity results; the shape (reasons + learning note) stays.
    /// </summary>
    [CreateAssetMenu(fileName = "Placeholder_", menuName = "Mars Outpost/Placeholder Activity Outcome", order = 50)]
    public sealed class PlaceholderActivityOutcome : ScriptableObject
    {
        public string activityId = "landing";
        public string headline = "Training step complete";
        [TextArea(2, 4)] public string summary = "";
        [TextArea(2, 4)] public string learningNote = "";
        public ResourceChange[] changes = new ResourceChange[0];
    }
}
