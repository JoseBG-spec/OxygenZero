namespace MarsOutpost.Core
{
    /// <summary>
    /// The result of applying one change, including clamping and the player-facing reason.
    /// Kept separate from the authored change so the UI can explain what happened.
    /// </summary>
    public sealed class AppliedResourceChange
    {
        public string ResourceId { get; }
        public string DisplayName { get; }
        public int RequestedDelta { get; }
        public int AppliedDelta { get; }
        public int ValueBefore { get; }
        public int ValueAfter { get; }
        public string Reason { get; }
        public bool WasClamped => RequestedDelta != AppliedDelta;

        public AppliedResourceChange(
            string resourceId,
            string displayName,
            int requestedDelta,
            int appliedDelta,
            int valueBefore,
            int valueAfter,
            string reason)
        {
            ResourceId = resourceId;
            DisplayName = displayName;
            RequestedDelta = requestedDelta;
            AppliedDelta = appliedDelta;
            ValueBefore = valueBefore;
            ValueAfter = valueAfter;
            Reason = reason;
        }
    }
}
