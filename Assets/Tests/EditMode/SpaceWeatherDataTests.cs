using System.Linq;
using MarsOutpost.Core;
using MarsOutpost.Weather;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace MarsOutpost.Tests
{
    public sealed class SpaceWeatherDataTests
    {
        private const string ConfigPath = "Assets/Data/Weather/SpaceWeatherConfig.asset";
        private const string JsonPath = "Assets/Data/Weather/CuratedAlerts.json";

        [Test]
        public void AuthoredWeatherConfigPassesValidation()
        {
            SpaceWeatherConfig config = AssetDatabase.LoadAssetAtPath<SpaceWeatherConfig>(ConfigPath);

            Assert.That(config, Is.Not.Null, "SpaceWeatherConfig.asset is missing.");
            var errors = SpaceWeatherDataValidator.Validate(config);
            Assert.That(errors, Is.Empty, string.Join("\n", errors));
            Assert.That(config.curatedAlertsJson, Is.Not.Null);
            Assert.That(config.fallbackAlert, Is.Not.Null);
        }

        [Test]
        public void CuratedAlertHasOneCautiousPlanAndExplainableChanges()
        {
            SpaceWeatherConfig config = AssetDatabase.LoadAssetAtPath<SpaceWeatherConfig>(ConfigPath);
            Assert.That(config, Is.Not.Null);

            var source = new LocalJsonSpaceWeatherAlertSource(config.curatedAlertsJson);
            Assert.That(source.TryLoad(out SpaceWeatherAlertDefinition alert, out string error), Is.True, error);
            Assert.That(alert.eventType, Is.EqualTo("FLR"));
            Assert.That(alert.choices, Has.Length.EqualTo(3));
            Assert.That(alert.choices.Count(choice => choice.correct), Is.EqualTo(1));

            foreach (SpaceWeatherPlanDefinition choice in alert.choices)
            {
                Assert.That(choice.changes, Is.Not.Empty, choice.id);
                Assert.That(choice.changes.All(change => !string.IsNullOrWhiteSpace(change.reason)), Is.True, choice.id);
                Assert.That(choice.learningNote, Is.Not.Empty, choice.id);
            }
        }

        [Test]
        public void InvalidJsonFallsBackToAuthoredAlert()
        {
            SpaceWeatherConfig config = AssetDatabase.LoadAssetAtPath<SpaceWeatherConfig>(ConfigPath);
            Assert.That(config, Is.Not.Null);

            var source = new LocalJsonSpaceWeatherAlertSource(new TextAsset("{ this is not json"));
            bool loaded = SpaceWeatherAlertLibrary.TryLoad(
                source,
                config.fallbackAlert,
                out SpaceWeatherAlertDefinition alert,
                out string sourceLabel,
                out string error);

            Assert.That(loaded, Is.True);
            Assert.That(alert, Is.SameAs(config.fallbackAlert));
            Assert.That(sourceLabel, Is.EqualTo(SpaceWeatherAlertLibrary.FallbackSourceLabel));
            Assert.That(error, Is.Not.Empty);
        }

        [Test]
        public void AlertWithoutEnoughPlansIsRejected()
        {
            var alert = new SpaceWeatherAlertDefinition
            {
                id = "test-alert",
                eventType = "FLR",
                eventName = "Solar flare",
                alertCode = "TRAINING ALERT FLR-99",
                headline = "Test",
                summary = "Test",
                sourceNote = "Test",
                choices = new[]
                {
                    new SpaceWeatherPlanDefinition
                    {
                        id = "only",
                        title = "Only plan",
                        description = "Test",
                        headline = "Test",
                        feedback = "Test",
                        learningNote = "Test",
                        correct = true,
                        changes = new[] { new ResourceChange("safety", 1, "Test.") }
                    }
                }
            };

            var errors = SpaceWeatherDataValidator.ValidateAlert(alert);
            Assert.That(errors.Any(error => error.Contains("at least two")), Is.True, string.Join("\n", errors));
        }

        [Test]
        public void DecoderCoversTheFourEventsAndTheEarthStormCaveat()
        {
            SpaceWeatherConfig config = AssetDatabase.LoadAssetAtPath<SpaceWeatherConfig>(ConfigPath);
            Assert.That(config, Is.Not.Null);
            Assert.That(config.eventTypes.Select(info => info.code), Is.EquivalentTo(new[] { "FLR", "SEP", "CME", "GST" }));

            SpaceWeatherEventTypeInfo gst = config.eventTypes.Single(info => info.code == "GST");
            StringAssert.Contains("Earth", gst.description);
            StringAssert.Contains("Mars", gst.description);
            StringAssert.Contains("not", gst.description.ToLowerInvariant());
        }

        [Test]
        public void CuratedJsonAssetIsTheExpectedLocalFile()
        {
            var json = AssetDatabase.LoadAssetAtPath<TextAsset>(JsonPath);
            Assert.That(json, Is.Not.Null);
            StringAssert.Contains("\"eventType\": \"FLR\"", json.text);
            StringAssert.Contains("\"correct\": true", json.text);
        }
    }
}
