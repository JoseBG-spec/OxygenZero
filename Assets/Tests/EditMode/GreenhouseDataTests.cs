using System.Collections.Generic;
using System.Linq;
using MarsOutpost.Greenhouse;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace MarsOutpost.Tests.EditMode
{
    public sealed class GreenhouseDataTests
    {
        private const string ConfigPath = "Assets/Data/Greenhouse/GreenhouseConfig.asset";

        private static GreenhouseExperimentConfig LoadConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<GreenhouseExperimentConfig>(ConfigPath);
            Assert.That(config, Is.Not.Null, "Run Tools > Mars Outpost > Build Phase 1 Content first.");
            return config;
        }

        [Test]
        public void AuthoredGreenhouseConfigIsValid()
        {
            GreenhouseExperimentConfig config = LoadConfig();
            List<string> errors = GreenhouseDataValidator.Validate(config);
            Assert.That(errors, Is.Empty, string.Join("\n", errors));
        }

        [Test]
        public void ExactlyOneClaimMatchesTheStudyAndItRewardsScience()
        {
            GreenhouseExperimentConfig config = LoadConfig();
            var matching = config.claims.Where(claim => claim != null && claim.matchesStudy).ToArray();

            Assert.That(matching, Has.Length.EqualTo(1));
            Assert.That(matching[0].changes, Is.Not.Empty);
            Assert.That(matching[0].changes.Any(change => change.resourceId == "science" && change.delta > 0), Is.True);
        }

        [Test]
        public void IncorrectClaimsDoNotRewardScience()
        {
            GreenhouseExperimentConfig config = LoadConfig();
            var incorrect = config.claims.Where(claim => claim != null && !claim.matchesStudy).ToArray();

            Assert.That(incorrect, Is.Not.Empty);
            foreach (GreenhouseClaimDefinition claim in incorrect)
            {
                Assert.That(
                    claim.changes.Any(change => change.resourceId == "science" && change.delta > 0),
                    Is.False,
                    $"Claim '{claim.id}' is not supported by the study and must not award science.");
            }
        }

        [Test]
        public void TrialHasAPowerCostAndNonEmptyGuidance()
        {
            GreenhouseExperimentConfig config = LoadConfig();

            Assert.That(config.runPowerCost, Is.GreaterThan(0));
            Assert.That(config.runCostFormat, Does.Contain("{0}"));
            Assert.That(config.setupReadyFormat, Does.Contain("{0}"));
            Assert.That(config.setupReadyFormat, Does.Contain("{1}"));
            Assert.That(config.observationsBody, Is.Not.Empty);
            Assert.That(config.simulationNote, Does.Contain("not NASA measurements"));
        }

        [Test]
        public void EmptyConfigReportsReadableErrors()
        {
            var config = ScriptableObject.CreateInstance<GreenhouseExperimentConfig>();
            List<string> errors = GreenhouseDataValidator.Validate(config);

            Assert.That(errors, Is.Not.Empty);
            Assert.That(errors.Any(error => error.Contains("claim options")), Is.True);

            Object.DestroyImmediate(config);
        }
    }
}
