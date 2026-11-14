using System.Collections.Generic;
using MarsOutpost.Core;
using MarsOutpost.Landing;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace MarsOutpost.Tests
{
    public sealed class LandingDataTests
    {
        private const string LandingConfigPath = "Assets/Data/Landing/LandingConfig.asset";
        private const string MissionConfigPath = "Assets/Data/Configs/MissionConfig.asset";

        [Test]
        public void AuthoredLandingContentHasThreeDistinctSites()
        {
            LandingMissionConfig landing = AssetDatabase.LoadAssetAtPath<LandingMissionConfig>(LandingConfigPath);
            Assert.That(landing, Is.Not.Null, "Run Tools > Mars Outpost > Build Phase 1 Content first.");
            Assert.That(landing.sites, Has.Length.EqualTo(3));

            var ids = new HashSet<string>();
            foreach (LandingSiteDefinition site in landing.sites)
            {
                Assert.That(site, Is.Not.Null);
                Assert.That(site.id, Is.Not.Empty);
                Assert.That(ids.Add(site.id), Is.True, $"Duplicate landing site id: {site.id}");
                Assert.That(site.displayName, Is.Not.Empty);
                Assert.That(site.mapPosition.x, Is.InRange(0f, 1f));
                Assert.That(site.mapPosition.y, Is.InRange(0f, 1f));
                Assert.That(site.slope.label, Is.Not.Empty);
                Assert.That(site.dust.label, Is.Not.Empty);
                Assert.That(site.distance.label, Is.Not.Empty);
                Assert.That(site.resultSummary, Is.Not.Empty);
                Assert.That(site.learningNote, Is.Not.Empty);
            }
        }

        [Test]
        public void LandingChangesReferenceAuthoredResources()
        {
            LandingMissionConfig landing = AssetDatabase.LoadAssetAtPath<LandingMissionConfig>(LandingConfigPath);
            MissionConfig mission = AssetDatabase.LoadAssetAtPath<MissionConfig>(MissionConfigPath);
            Assert.That(landing, Is.Not.Null);
            Assert.That(mission, Is.Not.Null);

            var resourceIds = new HashSet<string>();
            foreach (ResourceDefinition resource in mission.resources)
            {
                resourceIds.Add(resource.id);
            }

            foreach (LandingSiteDefinition site in landing.sites)
            {
                Assert.That(site, Is.Not.Null);
                Assert.That(site.changes, Is.Not.Empty, $"{site.id} has no changes.");
                foreach (ResourceChange change in site.changes)
                {
                    Assert.That(resourceIds.Contains(change.resourceId), Is.True,
                        $"{site.id} changes unknown resource '{change.resourceId}'.");
                    Assert.That(string.IsNullOrWhiteSpace(change.reason), Is.False,
                        $"{site.id} has a change with no reason.");
                }
            }
        }

        [Test]
        public void ValidatorReportsAnEmptySiteList()
        {
            LandingMissionConfig landing = ScriptableObject.CreateInstance<LandingMissionConfig>();
            landing.sites = new LandingSiteDefinition[0];

            List<string> errors = LandingDataValidator.Validate(landing);

            Assert.That(errors, Has.Count.GreaterThan(0));
            Object.DestroyImmediate(landing);
        }

        [Test]
        public void EachSiteHasADifferentTradeoffProfile()
        {
            LandingMissionConfig landing = AssetDatabase.LoadAssetAtPath<LandingMissionConfig>(LandingConfigPath);
            var profiles = new HashSet<string>();
            foreach (LandingSiteDefinition site in landing.sites)
            {
                string profile = $"{site.slope.label}|{site.dust.label}|{site.distance.label}";
                Assert.That(profiles.Add(profile), Is.True, $"Duplicate hazard profile: {profile}");
            }
        }
    }
}
