using System;
using System.Collections.Generic;

namespace MarsOutpost.Core
{
    /// <summary>
    /// Plain runtime record of one completed activity. This is session state only:
    /// it is never written back into a ScriptableObject asset.
    /// </summary>
    public sealed class ActivityOutcome
    {
        public string ActivityId { get; }
        public string ChoiceId { get; }
        public string Headline { get; }
        public string Summary { get; }
        public string LearningNote { get; }
        public IReadOnlyList<AppliedResourceChange> Changes { get; }
        public DateTime CompletedUtc { get; }

        public ActivityOutcome(
            string activityId,
            string choiceId,
            string headline,
            string summary,
            string learningNote,
            IReadOnlyList<AppliedResourceChange> changes)
        {
            ActivityId = activityId;
            ChoiceId = choiceId;
            Headline = headline;
            Summary = summary;
            LearningNote = learningNote;
            Changes = changes ?? Array.Empty<AppliedResourceChange>();
            CompletedUtc = DateTime.UtcNow;
        }
    }
}
