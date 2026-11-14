using System.Collections.Generic;
using UnityEngine;

namespace MarsOutpost.Radiation
{
    /// <summary>
    /// Data checks for the authored radiation config and the generated arena.
    /// The radiation game intentionally uses simulated readings, never real dose units.
    /// </summary>
    public static class RadiationDataValidator
    {
        private static readonly string[] ForbiddenUnitFragments =
        {
            "\u00B5Sv",
            "uSv",
            "microsievert",
            "micro-sievert",
            "mSv/h",
            "mGy"
        };

        public static List<string> Validate(RadiationConfig config)
        {
            var errors = new List<string>();
            if (config == null)
            {
                errors.Add("RadiationConfig is missing.");
                return errors;
            }

            Require(errors, config.screenTitle, "screenTitle");
            Require(errors, config.instructionBody, "instructionBody");
            Require(errors, config.objectiveTalk, "objectiveTalk");
            Require(errors, config.objectiveReach, "objectiveReach");
            Require(errors, config.npcName, "npcName");
            Require(errors, config.controlPrompt, "controlPrompt");
            Require(errors, config.debriefHeadline, "debriefHeadline");
            Require(errors, config.debriefLearning, "debriefLearning");
            Require(errors, config.activityId, "activityId");

            if (config.firstAlertDelay < 0f)
            {
                errors.Add("firstAlertDelay cannot be negative.");
            }

            if (config.spreadSpeed <= 0f)
            {
                errors.Add("spreadSpeed must be greater than zero.");
            }

            if (config.maxRadius <= 0f)
            {
                errors.Add("maxRadius must be greater than zero.");
            }

            if (config.maxExposure <= 0f)
            {
                errors.Add("maxExposure must be greater than zero.");
            }

            if (config.warningReading < 0f || config.warningReading > 100f ||
                config.highReading < 0f || config.highReading > 100f)
            {
                errors.Add("Detector thresholds must be between 0 and 100.");
            }

            if (config.highReading <= config.warningReading)
            {
                errors.Add("highReading must be greater than warningReading.");
            }

            if (config.dialoguePages == null || config.dialoguePages.Length < 3)
            {
                errors.Add("At least three dialogue pages are required.");
            }
            else
            {
                for (int i = 0; i < config.dialoguePages.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(config.dialoguePages[i]))
                    {
                        errors.Add($"Dialogue page {i + 1} is empty.");
                    }
                }
            }

            CheckNoRealUnits(errors, config.instructionBody, "instructionBody");
            CheckNoRealUnits(errors, config.legend, "legend");
            CheckNoRealUnits(errors, config.detectorCaption, "detectorCaption");
            CheckNoRealUnits(errors, config.debriefLearning, "debriefLearning");
            foreach (string page in config.dialoguePages ?? System.Array.Empty<string>())
            {
                CheckNoRealUnits(errors, page, "dialoguePages");
            }

            return errors;
        }

        public static List<string> ValidateArena(
            RadiationManager manager,
            ShipWalkableArea walkableArea,
            RadiationSpawnPoint[] spawnPoints)
        {
            var errors = new List<string>();
            if (manager == null)
            {
                errors.Add("RadiationManager is missing.");
                return errors;
            }

            if (manager.Config == null)
            {
                errors.Add("RadiationManager has no RadiationConfig.");
            }

            if (walkableArea == null)
            {
                errors.Add("ShipWalkableArea is missing.");
            }

            if (spawnPoints == null || spawnPoints.Length < 2)
            {
                errors.Add("At least two approved hazard spawn points are required.");
                return errors;
            }

            var ids = new HashSet<string>();
            foreach (RadiationSpawnPoint point in spawnPoints)
            {
                if (point == null)
                {
                    errors.Add("A radiation spawn point is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(point.id))
                {
                    errors.Add("A radiation spawn point has no id.");
                }
                else if (!ids.Add(point.id))
                {
                    errors.Add($"Duplicate radiation spawn point id '{point.id}'.");
                }

                if (walkableArea != null && !walkableArea.IsInside(point.Position, point.clearance))
                {
                    errors.Add($"Spawn point '{point.id}' is outside the walkable area.");
                }
            }

            return errors;
        }

        private static void Require(List<string> errors, string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add($"{field} is empty.");
            }
        }

        private static void CheckNoRealUnits(List<string> errors, string text, string field)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            string lower = text.ToLowerInvariant();
            foreach (string fragment in ForbiddenUnitFragments)
            {
                if (lower.Contains(fragment.ToLowerInvariant()))
                {
                    errors.Add($"{field} contains a real radiation unit ('{fragment}'). Use a simulated game reading instead.");
                }
            }
        }
    }
}
