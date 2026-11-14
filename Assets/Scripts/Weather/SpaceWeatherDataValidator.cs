using System.Collections.Generic;
using MarsOutpost.Core;
using UnityEngine;

namespace MarsOutpost.Weather
{
    /// <summary>
    /// Catches authoring mistakes before a child reaches an alert with no choices or no
    /// cautious answer. Also guards the DONKI-decoder caveat about Earth storms.
    /// </summary>
    public static class SpaceWeatherDataValidator
    {
        private static readonly string[] RequiredEventTypes = { "FLR", "SEP", "CME", "GST" };
        private static readonly HashSet<string> KnownEventTypes = new HashSet<string>
        {
            "FLR", "SEP", "CME", "IPS", "MPC", "GST", "RBE", "HSS"
        };

        public static List<string> Validate(SpaceWeatherConfig config)
        {
            var errors = new List<string>();
            if (config == null)
            {
                errors.Add("Space weather config is missing.");
                return errors;
            }

            Require(errors, config.screenTitle, "screenTitle");
            Require(errors, config.tagline, "tagline");
            Require(errors, config.controlRoomTitle, "controlRoomTitle");
            Require(errors, config.alertTag, "alertTag");
            Require(errors, config.decoderTitle, "decoderTitle");
            Require(errors, config.decoderHint, "decoderHint");
            Require(errors, config.compareLabel, "compareLabel");
            Require(errors, config.backToAlertLabel, "backToAlertLabel");
            Require(errors, config.decisionTitle, "decisionTitle");
            Require(errors, config.decisionPrompt, "decisionPrompt");
            Require(errors, config.planHint, "planHint");
            Require(errors, config.selectedPlanFormat, "selectedPlanFormat");
            Require(errors, config.choosePlanLabel, "choosePlanLabel");
            Require(errors, config.confirmLabel, "confirmLabel");
            Require(errors, config.completedLabel, "completedLabel");
            Require(errors, config.completedPlanFormat, "completedPlanFormat");
            Require(errors, config.simulationNote, "simulationNote");

            ValidateEventTypes(errors, config.eventTypes);
            ValidateAlert(errors, config.fallbackAlert, "fallbackAlert");
            ValidateCuratedJson(errors, config.curatedAlertsJson);

            return errors;
        }

        public static List<string> ValidateAlert(SpaceWeatherAlertDefinition alert, string context = "alert")
        {
            var errors = new List<string>();
            ValidateAlert(errors, alert, context);
            return errors;
        }

        private static void ValidateCuratedJson(List<string> errors, TextAsset json)
        {
            if (json == null)
            {
                errors.Add("Space weather curatedAlertsJson is not assigned.");
                return;
            }

            var source = new LocalJsonSpaceWeatherAlertSource(json);
            if (!source.TryLoad(out SpaceWeatherAlertDefinition alert, out string error))
            {
                errors.Add($"Space weather curated JSON: {error}");
                return;
            }

            ValidateAlert(errors, alert, "curated alert");
        }

        private static void ValidateAlert(
            List<string> errors,
            SpaceWeatherAlertDefinition alert,
            string context)
        {
            if (alert == null)
            {
                errors.Add($"Space weather {context} is missing.");
                return;
            }

            Require(errors, alert.id, $"{context} id");
            Require(errors, alert.eventType, $"{context} eventType");
            Require(errors, alert.eventName, $"{context} eventName");
            Require(errors, alert.alertCode, $"{context} alertCode");
            Require(errors, alert.headline, $"{context} headline");
            Require(errors, alert.summary, $"{context} summary");
            Require(errors, alert.sourceNote, $"{context} sourceNote");

            if (!string.IsNullOrWhiteSpace(alert.eventType) && !KnownEventTypes.Contains(alert.eventType))
            {
                errors.Add($"Space weather {context} uses unknown DONKI event type '{alert.eventType}'.");
            }

            if (alert.choices == null || alert.choices.Length < 2)
            {
                errors.Add($"Space weather {context} needs at least two crew plans.");
                return;
            }

            var ids = new HashSet<string>();
            int correct = 0;
            for (int i = 0; i < alert.choices.Length; i++)
            {
                SpaceWeatherPlanDefinition choice = alert.choices[i];
                if (choice == null)
                {
                    errors.Add($"Space weather {context} plan slot {i + 1} is empty.");
                    continue;
                }

                string label = string.IsNullOrWhiteSpace(choice.id) ? $"slot {i + 1}" : choice.id;
                Require(errors, choice.id, $"{context} plan {label} id");
                Require(errors, choice.title, $"{context} plan {label} title");
                Require(errors, choice.description, $"{context} plan {label} description");
                Require(errors, choice.headline, $"{context} plan {label} headline");
                Require(errors, choice.feedback, $"{context} plan {label} feedback");
                Require(errors, choice.learningNote, $"{context} plan {label} learningNote");

                if (!string.IsNullOrWhiteSpace(choice.id) && !ids.Add(choice.id))
                {
                    errors.Add($"Duplicate space weather plan id '{choice.id}' in {context}.");
                }

                if (choice.correct)
                {
                    correct++;
                }

                ValidateChanges(errors, choice.changes, $"{context} plan '{label}'");
            }

            if (correct != 1)
            {
                errors.Add($"Space weather {context} needs exactly one cautious plan (found {correct}).");
            }
        }

        private static void ValidateChanges(
            List<string> errors,
            ResourceChange[] changes,
            string context)
        {
            if (changes == null)
            {
                errors.Add($"{context} has a null changes array.");
                return;
            }

            if (changes.Length == 0)
            {
                errors.Add($"{context} has no resource changes.");
                return;
            }

            foreach (ResourceChange change in changes)
            {
                if (string.IsNullOrWhiteSpace(change.resourceId))
                {
                    errors.Add($"{context} has a resource change with no resource id.");
                }

                if (string.IsNullOrWhiteSpace(change.reason))
                {
                    errors.Add($"{context} has a resource change with no reason.");
                }
            }
        }

        private static void ValidateEventTypes(List<string> errors, SpaceWeatherEventTypeInfo[] types)
        {
            if (types == null || types.Length < RequiredEventTypes.Length)
            {
                errors.Add($"Space weather needs at least {RequiredEventTypes.Length} DONKI decoder entries.");
                return;
            }

            var codes = new HashSet<string>();
            foreach (SpaceWeatherEventTypeInfo info in types)
            {
                if (info == null)
                {
                    errors.Add("Space weather decoder has an empty entry.");
                    continue;
                }

                Require(errors, info.code, "decoder code");
                Require(errors, info.name, $"decoder {info.code} name");
                Require(errors, info.description, $"decoder {info.code} description");

                if (!string.IsNullOrWhiteSpace(info.code) && !codes.Add(info.code))
                {
                    errors.Add($"Duplicate space weather decoder code '{info.code}'.");
                }
            }

            foreach (string required in RequiredEventTypes)
            {
                if (!codes.Contains(required))
                {
                    errors.Add($"Space weather decoder is missing {required}.");
                }
            }

            SpaceWeatherEventTypeInfo gst = null;
            foreach (SpaceWeatherEventTypeInfo info in types)
            {
                if (info != null && info.code == "GST")
                {
                    gst = info;
                    break;
                }
            }

            if (gst != null && !string.IsNullOrWhiteSpace(gst.description))
            {
                string text = gst.description.ToLowerInvariant();
                if (!text.Contains("earth") || !text.Contains("mars") || !text.Contains("not"))
                {
                    errors.Add("Space weather GST decoder must say it is an Earth forecast, not a direct Mars-surface alarm.");
                }
            }
        }

        private static void Require(List<string> errors, string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add($"Space weather field '{field}' is empty.");
            }
        }
    }
}
