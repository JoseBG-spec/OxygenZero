using System;
using System.Collections.Generic;
using UnityEngine;

namespace MarsOutpost.Core
{
    /// <summary>
    /// All mutable state for one play session. Resources are clamped on apply and every
    /// change keeps its authored reason so the debrief can explain the number.
    /// </summary>
    public sealed class MissionSession
    {
        private readonly MissionConfig config;
        private readonly Dictionary<string, ResourceDefinition> definitions = new Dictionary<string, ResourceDefinition>();
        private readonly Dictionary<string, int> values = new Dictionary<string, int>();
        private readonly Dictionary<string, ActivityOutcome> outcomes = new Dictionary<string, ActivityOutcome>();

        public bool BriefingSeen { get; set; }
        public IReadOnlyDictionary<string, int> Values => values;
        public IReadOnlyDictionary<string, ActivityOutcome> Outcomes => outcomes;

        public MissionSession(MissionConfig config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));

            if (config.resources == null)
            {
                return;
            }

            foreach (var definition in config.resources)
            {
                if (definition == null)
                {
                    continue;
                }

                string id = definition.IdOrFallback;
                definitions[id] = definition;
                values[id] = definition.Clamp(definition.startingValue);
            }
        }

        public int GetValue(string resourceId)
        {
            if (string.IsNullOrWhiteSpace(resourceId))
            {
                return 0;
            }

            return values.TryGetValue(resourceId, out int value) ? value : 0;
        }

        public bool TryGetDefinition(string resourceId, out ResourceDefinition definition)
        {
            return definitions.TryGetValue(resourceId ?? string.Empty, out definition);
        }

        public bool HasOutcome(string activityId)
        {
            return !string.IsNullOrWhiteSpace(activityId) && outcomes.ContainsKey(activityId);
        }

        public ActivityOutcome GetOutcome(string activityId)
        {
            if (string.IsNullOrWhiteSpace(activityId))
            {
                return null;
            }

            return outcomes.TryGetValue(activityId, out ActivityOutcome outcome) ? outcome : null;
        }

        public int RequiredCompletedCount()
        {
            int count = 0;
            if (config.activities == null)
            {
                return count;
            }

            foreach (var activity in config.activities)
            {
                if (activity != null && activity.requiredForMission && outcomes.ContainsKey(activity.id))
                {
                    count++;
                }
            }

            return count;
        }

        public int RequiredTotalCount()
        {
            int count = 0;
            if (config.activities == null)
            {
                return count;
            }

            foreach (var activity in config.activities)
            {
                if (activity != null && activity.requiredForMission)
                {
                    count++;
                }
            }

            return count;
        }

        public bool AreRequiredActivitiesComplete()
        {
            if (config.activities == null)
            {
                return true;
            }

            foreach (var activity in config.activities)
            {
                if (activity != null && activity.requiredForMission && !outcomes.ContainsKey(activity.id))
                {
                    return false;
                }
            }

            return true;
        }

        public int Science => GetValue("science");

        /// <summary>
        /// Applies an activity once. Calling again returns the stored outcome without
        /// double-applying resources (safe for repeated taps / accidental reloads).
        /// Unknown resource ids are logged and skipped rather than throwing.
        /// </summary>
        public ActivityOutcome ApplyOutcome(
            string activityId,
            string choiceId,
            string headline,
            string summary,
            string learningNote,
            IEnumerable<ResourceChange> changes)
        {
            if (string.IsNullOrWhiteSpace(activityId))
            {
                throw new ArgumentException("activityId is required.", nameof(activityId));
            }

            if (outcomes.TryGetValue(activityId, out ActivityOutcome existing))
            {
                return existing;
            }

            var applied = new List<AppliedResourceChange>();

            if (changes != null)
            {
                foreach (ResourceChange change in changes)
                {
                    if (string.IsNullOrWhiteSpace(change.resourceId))
                    {
                        continue;
                    }

                    if (!values.TryGetValue(change.resourceId, out int before))
                    {
                        Debug.LogWarning($"[MarsOutpost] Resource '{change.resourceId}' is not defined in MissionConfig. Change skipped.");
                        continue;
                    }

                    ResourceDefinition definition = definitions[change.resourceId];
                    int after = definition.Clamp(before + change.delta);
                    values[change.resourceId] = after;

                    applied.Add(new AppliedResourceChange(
                        change.resourceId,
                        definition.displayName,
                        change.delta,
                        after - before,
                        before,
                        after,
                        change.reason));
                }
            }

            var outcome = new ActivityOutcome(activityId, choiceId, headline, summary, learningNote, applied);
            outcomes[activityId] = outcome;
            return outcome;
        }

        public void Reset()
        {
            values.Clear();
            foreach (ResourceDefinition definition in definitions.Values)
            {
                values[definition.IdOrFallback] = definition.Clamp(definition.startingValue);
            }

            outcomes.Clear();
            BriefingSeen = false;
        }
    }
}
