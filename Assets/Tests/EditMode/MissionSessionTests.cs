using System.Collections.Generic;
using MarsOutpost.Core;
using NUnit.Framework;
using UnityEngine;

namespace MarsOutpost.Tests
{
    public sealed class MissionSessionTests
    {
        private readonly List<Object> created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object item in created)
            {
                if (item != null)
                {
                    Object.DestroyImmediate(item);
                }
            }

            created.Clear();
        }

        [Test]
        public void NewSessionUsesStartingValues()
        {
            MissionConfig config = MakeConfig();
            var session = new MissionSession(config);

            Assert.That(session.GetValue("power"), Is.EqualTo(100));
            Assert.That(session.GetValue("safety"), Is.EqualTo(100));
            Assert.That(session.GetValue("science"), Is.EqualTo(0));
            Assert.That(session.AreRequiredActivitiesComplete(), Is.False);
        }

        [Test]
        public void ApplyingOutcomeClampsAndKeepsReason()
        {
            MissionConfig config = MakeConfig();
            var session = new MissionSession(config);

            ActivityOutcome outcome = session.ApplyOutcome(
                "landing",
                "test",
                "Test headline",
                "Test summary",
                "Test learning",
                new[]
                {
                    new ResourceChange("power", -150, "The test burn uses power."),
                    new ResourceChange("science", 10, "The test adds data.")
                });

            Assert.That(session.GetValue("power"), Is.EqualTo(0));
            Assert.That(session.GetValue("science"), Is.EqualTo(10));
            Assert.That(outcome.Changes.Count, Is.EqualTo(2));

            AppliedResourceChange power = outcome.Changes[0];
            Assert.That(power.RequestedDelta, Is.EqualTo(-150));
            Assert.That(power.AppliedDelta, Is.EqualTo(-100));
            Assert.That(power.WasClamped, Is.True);
            Assert.That(power.Reason, Is.EqualTo("The test burn uses power."));
        }

        [Test]
        public void SameActivityCannotBeAppliedTwice()
        {
            MissionConfig config = MakeConfig();
            var session = new MissionSession(config);

            session.ApplyOutcome("landing", "a", "First", "First", "First", new[]
            {
                new ResourceChange("science", 10, "First")
            });

            session.ApplyOutcome("landing", "b", "Second", "Second", "Second", new[]
            {
                new ResourceChange("science", 50, "Second")
            });

            Assert.That(session.GetValue("science"), Is.EqualTo(10));
            Assert.That(session.GetOutcome("landing").Headline, Is.EqualTo("First"));
        }

        [Test]
        public void UnknownResourceIsSkippedWithoutThrowing()
        {
            MissionConfig config = MakeConfig();
            var session = new MissionSession(config);

            Assert.DoesNotThrow(() => session.ApplyOutcome("landing", "a", "H", "S", "L", new[]
            {
                new ResourceChange("not-a-resource", 10, "Ignored")
            }));

            Assert.That(session.GetOutcome("landing").Changes.Count, Is.EqualTo(0));
        }

        [Test]
        public void RequiredActivitiesGateTheReport()
        {
            MissionConfig config = MakeConfig();

            foreach (ActivityDefinition activity in config.activities)
            {
                // Create a copy with only the first activity required to keep the test explicit.
                activity.requiredForMission = activity.id == "landing";
            }

            var session = new MissionSession(config);
            Assert.That(session.AreRequiredActivitiesComplete(), Is.False);
            Assert.That(session.RequiredTotalCount(), Is.EqualTo(1));

            session.ApplyOutcome("landing", "a", "H", "S", "L", null);
            Assert.That(session.AreRequiredActivitiesComplete(), Is.True);
            Assert.That(session.RequiredCompletedCount(), Is.EqualTo(1));
        }

        [Test]
        public void ResetRestoresStartingValuesAndClearsOutcomes()
        {
            MissionConfig config = MakeConfig();
            var session = new MissionSession(config);

            session.ApplyOutcome("landing", "a", "H", "S", "L", new[]
            {
                new ResourceChange("power", -20, "Test")
            });
            session.BriefingSeen = true;

            session.Reset();

            Assert.That(session.GetValue("power"), Is.EqualTo(100));
            Assert.That(session.HasOutcome("landing"), Is.False);
            Assert.That(session.BriefingSeen, Is.False);
        }

        [Test]
        public void ValidatorFindsDuplicateResourceIds()
        {
            MissionConfig config = MakeConfig();
            config.resources = new[] { config.resources[0], config.resources[0] };

            List<string> errors = MissionDataValidator.Validate(config);

            Assert.That(errors.Exists(error => error.Contains("Duplicate resource id")), Is.True);
        }

        [Test]
        public void ValidatorFindsMissingSceneName()
        {
            MissionConfig config = MakeConfig();
            config.activities[0].sceneName = string.Empty;

            List<string> errors = MissionDataValidator.Validate(config);

            Assert.That(errors.Exists(error => error.Contains("empty sceneName")), Is.True);
        }

        private MissionConfig MakeConfig()
        {
            var config = ScriptableObject.CreateInstance<MissionConfig>();
            created.Add(config);
            config.gameTitle = "Test";
            config.hubScene = "01_MissionHub";
            config.mainMenuScene = "00_MainMenu";
            config.debriefScene = "05_Debrief";

            var power = ScriptableObject.CreateInstance<ResourceDefinition>();
            created.Add(power);
            power.id = "power";
            power.displayName = "Power";
            power.startingValue = 100;
            power.minValue = 0;
            power.maxValue = 100;

            var safety = ScriptableObject.CreateInstance<ResourceDefinition>();
            created.Add(safety);
            safety.id = "safety";
            safety.displayName = "Crew Safety";
            safety.startingValue = 100;
            safety.minValue = 0;
            safety.maxValue = 100;

            var science = ScriptableObject.CreateInstance<ResourceDefinition>();
            created.Add(science);
            science.id = "science";
            science.displayName = "Science Progress";
            science.startingValue = 0;
            science.minValue = 0;
            science.maxValue = 100;

            config.resources = new[] { power, safety, science };

            var landing = ScriptableObject.CreateInstance<ActivityDefinition>();
            created.Add(landing);
            landing.id = "landing";
            landing.displayName = "Landing";
            landing.sceneName = "02_LandingSite";
            landing.requiredForMission = true;

            var greenhouse = ScriptableObject.CreateInstance<ActivityDefinition>();
            created.Add(greenhouse);
            greenhouse.id = "greenhouse";
            greenhouse.displayName = "Greenhouse";
            greenhouse.sceneName = "03_Greenhouse";
            greenhouse.requiredForMission = true;

            config.activities = new[] { landing, greenhouse };

            var debrief = ScriptableObject.CreateInstance<DebriefConfig>();
            created.Add(debrief);
            debrief.bands = new[]
            {
                new DebriefBand { title = "Done", minimumScience = 0 }
            };
            config.debrief = debrief;

            var theme = ScriptableObject.CreateInstance<UiTheme>();
            created.Add(theme);
            theme.font = null;
            config.theme = theme;

            return config;
        }
    }
}
