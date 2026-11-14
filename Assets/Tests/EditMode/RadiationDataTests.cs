using System.Linq;
using MarsOutpost.Radiation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class RadiationDataTests
{
    private const string ConfigPath = "Assets/Data/Radiation/RadiationConfig.asset";

    [Test]
    public void RadiationConfig_IsValid()
    {
        RadiationConfig config = AssetDatabase.LoadAssetAtPath<RadiationConfig>(ConfigPath);
        Assert.That(config, Is.Not.Null, "RadiationConfig asset is missing.");
        var errors = RadiationDataValidator.Validate(config);
        Assert.That(errors, Is.Empty, string.Join("\n", errors));
    }

    [Test]
    public void RadiationConfig_UsesOnlySimulatedReadings()
    {
        RadiationConfig config = AssetDatabase.LoadAssetAtPath<RadiationConfig>(ConfigPath);
        Assert.That(config, Is.Not.Null);

        string[] text =
        {
            config.instructionBody,
            config.legend,
            config.detectorCaption,
            config.exposureCaption,
            config.debriefLearning
        };

        foreach (string value in text.Concat(config.dialoguePages ?? System.Array.Empty<string>()))
        {
            Assert.That(value, Is.Not.Null);
            Assert.That(value.ToLowerInvariant(), Does.Not.Contain("\u00b5sv"));
            Assert.That(value.ToLowerInvariant(), Does.Not.Contain("usv"));
            Assert.That(value.ToLowerInvariant(), Does.Not.Contain("microsievert"));
        }

        Assert.That(config.detectorCaption, Is.EqualTo("SIMULATED READING"));
    }

    [Test]
    public void DetectorStatus_FollowsConfiguredThresholds()
    {
        Assert.That(RadiationReadings.Classify(0f, 25f, 60f), Is.EqualTo(RadiationStatus.Safe));
        Assert.That(RadiationReadings.Classify(24.9f, 25f, 60f), Is.EqualTo(RadiationStatus.Safe));
        Assert.That(RadiationReadings.Classify(25f, 25f, 60f), Is.EqualTo(RadiationStatus.Warning));
        Assert.That(RadiationReadings.Classify(59.9f, 25f, 60f), Is.EqualTo(RadiationStatus.Warning));
        Assert.That(RadiationReadings.Classify(60f, 25f, 60f), Is.EqualTo(RadiationStatus.High));
        Assert.That(RadiationReadings.Classify(100f, 25f, 60f), Is.EqualTo(RadiationStatus.High));
    }

    [Test]
    public void WalkableArea_ResolveNeverLeavesApprovedZone()
    {
        var go = new GameObject("WalkableTest");
        try
        {
            ShipWalkableArea area = go.AddComponent<ShipWalkableArea>();
            area.SetZones(new[] { new Rect(-2f, -2f, 4f, 4f) });
            Assert.That(area.IsInside(new Vector2(0f, 0f), 0.2f), Is.True);
            Assert.That(area.IsInside(new Vector2(2.1f, 0f), 0.2f), Is.False);

            Vector2 resolved = area.Resolve(new Vector2(0f, 0f), new Vector2(8f, 8f), 0.2f);
            Assert.That(resolved, Is.EqualTo(new Vector2(0f, 0f)));
        }
        finally
        {
            Object.DestroyImmediate(go);
        }
    }
}
