using System.Collections.Generic;
using MarsOutpost.Core;
using UnityEngine;

namespace MarsOutpost.Greenhouse
{
    /// <summary>
    /// Catches authoring mistakes in the greenhouse content before a child reaches a
    /// broken trial. Returns readable messages for the Console and tests.
    /// </summary>
    public static class GreenhouseDataValidator
    {
        public static List<string> Validate(GreenhouseExperimentConfig config)
        {
            var errors = new List<string>();
            if (config == null)
            {
                errors.Add("Greenhouse config is missing.");
                return errors;
            }

            Require(errors, config.screenTitle, "screenTitle");
            Require(errors, config.tagline, "tagline");
            Require(errors, config.chamberTitle, "chamberTitle");
            Require(errors, config.environmentLabel, "environmentLabel");
            Require(errors, config.trayALabel, "trayALabel");
            Require(errors, config.trayBLabel, "trayBLabel");
            Require(errors, config.chooseTreatmentLabel, "chooseTreatmentLabel");
            Require(errors, config.redTreatmentLabel, "redTreatmentLabel");
            Require(errors, config.darkTreatmentLabel, "darkTreatmentLabel");
            Require(errors, config.redButtonLabel, "redButtonLabel");
            Require(errors, config.darkButtonLabel, "darkButtonLabel");
            Require(errors, config.notebookTitle, "notebookTitle");
            Require(errors, config.setupHeader, "setupHeader");
            Require(errors, config.setupBody, "setupBody");
            Require(errors, config.setupIncomplete, "setupIncomplete");
            Require(errors, config.setupBothRed, "setupBothRed");
            Require(errors, config.setupBothDark, "setupBothDark");
            Require(errors, config.setupReadyFormat, "setupReadyFormat");
            Require(errors, config.runLabel, "runLabel");
            Require(errors, config.runCostFormat, "runCostFormat");
            Require(errors, config.observationsHeader, "observationsHeader");
            Require(errors, config.observationsBody, "observationsBody");
            Require(errors, config.claimsHeader, "claimsHeader");
            Require(errors, config.claimPrompt, "claimPrompt");
            Require(errors, config.confirmLabel, "confirmLabel");
            Require(errors, config.selectClaimLabel, "selectClaimLabel");
            Require(errors, config.completedLabel, "completedLabel");
            Require(errors, config.simulationNote, "simulationNote");

            if (config.runPowerCost < 0)
            {
                errors.Add("Greenhouse runPowerCost cannot be negative.");
            }

            ValidateObservation(errors, "redObservation", config.redObservation);
            ValidateObservation(errors, "darkObservation", config.darkObservation);

            if (config.redObservation != null && config.darkObservation != null &&
                config.redObservation.growthValue == config.darkObservation.growthValue)
            {
                errors.Add("Red-light and dark-control growth values should differ so the comparison teaches something.");
            }

            if (config.claims == null || config.claims.Length < 2)
            {
                errors.Add("Greenhouse config needs at least two claim options.");
                return errors;
            }

            var ids = new HashSet<string>();
            int matchingClaims = 0;
            for (int i = 0; i < config.claims.Length; i++)
            {
                GreenhouseClaimDefinition claim = config.claims[i];
                if (claim == null)
                {
                    errors.Add($"Greenhouse claim slot {i + 1} is empty.");
                    continue;
                }

                string label = string.IsNullOrWhiteSpace(claim.id) ? $"slot {i + 1}" : claim.id;
                Require(errors, claim.id, $"claim {label} id");
                Require(errors, claim.headline, $"claim {label} headline");
                Require(errors, claim.statement, $"claim {label} statement");
                Require(errors, claim.feedback, $"claim {label} feedback");
                Require(errors, claim.learningNote, $"claim {label} learningNote");

                if (!string.IsNullOrWhiteSpace(claim.id) && !ids.Add(claim.id))
                {
                    errors.Add($"Duplicate greenhouse claim id '{claim.id}'.");
                }

                if (claim.matchesStudy)
                {
                    matchingClaims++;
                }

                if (claim.changes == null)
                {
                    errors.Add($"Greenhouse claim '{label}' has a null changes array.");
                    continue;
                }

                if (claim.matchesStudy && claim.changes.Length == 0)
                {
                    errors.Add($"Greenhouse claim '{label}' matches the study but has no reward.");
                }

                foreach (ResourceChange change in claim.changes)
                {
                    if (string.IsNullOrWhiteSpace(change.resourceId))
                    {
                        errors.Add($"Greenhouse claim '{label}' has a resource change with no resource id.");
                    }

                    if (string.IsNullOrWhiteSpace(change.reason))
                    {
                        errors.Add($"Greenhouse claim '{label}' has a resource change with no reason.");
                    }
                }
            }

            if (matchingClaims != 1)
            {
                errors.Add($"Greenhouse config needs exactly one claim marked matchesStudy (found {matchingClaims}).");
            }

            return errors;
        }

        private static void ValidateObservation(
            List<string> errors,
            string name,
            GreenhouseObservation observation)
        {
            if (observation == null)
            {
                errors.Add($"Greenhouse {name} is missing.");
                return;
            }

            Require(errors, observation.growthLabel, $"{name}.growthLabel");
            Require(errors, observation.growthValue, $"{name}.growthValue");
            Require(errors, observation.stressLabel, $"{name}.stressLabel");
            Require(errors, observation.stressValue, $"{name}.stressValue");

            if (observation.growthTone < 0 || observation.growthTone > 2)
            {
                errors.Add($"Greenhouse {name}.growthTone is outside 0..2.");
            }

            if (observation.stressTone < 0 || observation.stressTone > 2)
            {
                errors.Add($"Greenhouse {name}.stressTone is outside 0..2.");
            }
        }

        private static void Require(List<string> errors, string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add($"Greenhouse config field '{field}' is empty.");
            }
        }
    }
}
