using System.Collections;
using MarsOutpost.Core;
using MarsOutpost.Radiation;
using MarsOutpost.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class RadiationPlayModeTests
{
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        MissionDirector.Instance?.Session?.Reset();
        SceneManager.LoadScene("07_Radiation");
        yield return null;
        yield return null;
    }

    [TearDown]
    public void TearDown()
    {
        MissionDirector.Instance?.Session?.Reset();
    }

    [UnityTest]
    public IEnumerator SceneHasValidApprovedSpawns()
    {
        RadiationManager manager = Object.FindFirstObjectByType<RadiationManager>();
        ShipWalkableArea walkable = Object.FindFirstObjectByType<ShipWalkableArea>();
        RadiationSpawnPoint[] spawns = Object.FindObjectsByType<RadiationSpawnPoint>(FindObjectsSortMode.None);

        Assert.That(manager, Is.Not.Null);
        Assert.That(walkable, Is.Not.Null);
        Assert.That(spawns.Length, Is.GreaterThanOrEqualTo(2));

        var errors = RadiationDataValidator.ValidateArena(manager, walkable, spawns);
        Assert.That(errors, Is.Empty, string.Join("\n", errors));
        yield return null;
    }

    [UnityTest]
    public IEnumerator AlertCanStartAtApprovedPointAndShieldingContainsIt()
    {
        RadiationManager manager = Object.FindFirstObjectByType<RadiationManager>();
        Assert.That(manager, Is.Not.Null);
        Assert.That(manager.config, Is.Not.Null);

        Assert.That(manager.BeginAlert(), Is.True);
        Assert.That(manager.IsAlertActive, Is.True);
        Assert.That(manager.ActiveSpawn, Is.Not.Null);
        Assert.That(manager.ActiveSpawn.id, Is.Not.Empty);

        yield return new WaitForSeconds(0.15f);
        Assert.That(manager.Radius, Is.GreaterThan(0f));

        Assert.That(manager.ActivateShielding(), Is.True);
        Assert.That(manager.IsContained, Is.True);

        manager.ResetDrill();
        Assert.That(manager.IsAlertActive, Is.False);
        Assert.That(manager.Radius, Is.EqualTo(0f));
    }

    [UnityTest]
    public IEnumerator RadiationHudFitsAtLargestTextScale()
    {
        try
        {
            while (UiThemeService.FontScale > 1.01f)
            {
                UiThemeService.CycleFontScale();
            }

            UiThemeService.CycleFontScale();
            UiThemeService.CycleFontScale();
            Canvas.ForceUpdateCanvases();
            yield return null;

            RadiationGameController controller = Object.FindFirstObjectByType<RadiationGameController>();
            Assert.That(controller, Is.Not.Null);

            foreach (TMPro.TMP_Text label in controller.GetComponentsInChildren<TMPro.TMP_Text>(true))
            {
                if (!label.gameObject.activeInHierarchy)
                {
                    continue;
                }

                label.ForceMeshUpdate();
                RectTransform rect = label.rectTransform;
                Assert.That(
                    label.preferredHeight,
                    Is.LessThanOrEqualTo(rect.rect.height + 1.5f),
                    $"{label.name} text is taller than its box at 130%: {label.text}");
            }
        }
        finally
        {
            while (UiThemeService.FontScale > 1.01f)
            {
                UiThemeService.CycleFontScale();
            }
        }
    }

    [UnityTest]
    public IEnumerator FullDrillFlowLogsTheOptionalOutcome()
    {
        RadiationGameController controller = Object.FindFirstObjectByType<RadiationGameController>();
        RadiationManager manager = Object.FindFirstObjectByType<RadiationManager>();
        Assert.That(controller, Is.Not.Null);
        Assert.That(manager, Is.Not.Null);

        controller.startButton.onClick.Invoke();
        yield return null;
        Assert.That(controller.introRoot.activeSelf, Is.False);

        manager.BeginAlert();
        yield return null;
        Assert.That(controller.objectiveLabel.text, Does.Contain("Safety Officer"));
        Assert.That(controller.panel.CanInteract, Is.True);

        controller.npc.Interact();
        yield return null;
        for (int i = 0; i < controller.config.dialoguePages.Length; i++)
        {
            controller.dialogue.continueButton.onClick.Invoke();
            yield return null;
        }

        Assert.That(controller.objectiveLabel.text, Does.Contain("Control Room"));
        controller.panel.Interact();
        yield return new WaitForSeconds(controller.config.resultDelay + 0.6f);

        Assert.That(controller.resultPanel.IsOpen, Is.True);
        Assert.That(MissionDirector.Instance.Session.HasOutcome(controller.config.activityId), Is.True);
    }

    [UnityTest]
    public IEnumerator DialogueAdvancesWithTouchButtonsAndUnpauses()
    {
        RadiationGameController controller = Object.FindFirstObjectByType<RadiationGameController>();
        Assert.That(controller, Is.Not.Null);
        Assert.That(controller.dialogue, Is.Not.Null);
        Assert.That(controller.npc, Is.Not.Null);

        controller.startButton.onClick.Invoke();
        yield return null;

        controller.npc.Interact();
        yield return null;
        Assert.That(controller.dialogue.IsOpen, Is.True);
        Assert.That(controller.pauseRoot.activeSelf, Is.False);

        int pages = controller.config.dialoguePages.Length;
        for (int i = 0; i < pages; i++)
        {
            controller.dialogue.continueButton.onClick.Invoke();
            yield return null;
        }

        Assert.That(controller.dialogue.IsOpen, Is.False);
        Assert.That(controller.objectiveLabel.text, Does.Contain("Control Room"));
    }

    [UnityTest]
    public IEnumerator DetectorAndTouchControlsArePresent()
    {
        Dosimeter detector = Object.FindFirstObjectByType<Dosimeter>();
        TouchJoystick joystick = Object.FindFirstObjectByType<TouchJoystick>();
        RadiationGameController controller = Object.FindFirstObjectByType<RadiationGameController>();

        Assert.That(detector, Is.Not.Null);
        Assert.That(joystick, Is.Not.Null);
        Assert.That(controller, Is.Not.Null);
        Assert.That(controller.config, Is.Not.Null);
        Assert.That(controller.hazard, Is.Not.Null);
        Assert.That(controller.player, Is.Not.Null);
        Assert.That(controller.exposure, Is.Not.Null);
        Assert.That(controller.interactor, Is.Not.Null);
        Assert.That(controller.npc, Is.Not.Null);
        Assert.That(controller.panel, Is.Not.Null);
        Assert.That(controller.respawnPoint, Is.Not.Null);
        Assert.That(controller.startButton, Is.Not.Null);
        Assert.That(controller.introReturnButton, Is.Not.Null);
        Assert.That(controller.interactButton, Is.Not.Null);
        Assert.That(controller.pauseButton, Is.Not.Null);
        Assert.That(controller.resumeButton, Is.Not.Null);
        Assert.That(controller.restartButton, Is.Not.Null);
        Assert.That(controller.returnButton, Is.Not.Null);
        Assert.That(controller.returnTopButton, Is.Not.Null);
        Assert.That(controller.introRoot, Is.Not.Null);
        Assert.That(controller.pauseRoot, Is.Not.Null);
        Assert.That(controller.toastRoot, Is.Not.Null);
        Assert.That(controller.dialogue, Is.Not.Null);
        Assert.That(controller.resultPanel, Is.Not.Null);
        Assert.That(controller.dialog, Is.Not.Null);
        Assert.That(controller.introTitleLabel, Is.Not.Null);
        Assert.That(controller.introInstructionsLabel, Is.Not.Null);
        Assert.That(controller.taglineLabel, Is.Not.Null);
        Assert.That(controller.objectiveHeaderLabel, Is.Not.Null);
        Assert.That(controller.objectiveLabel, Is.Not.Null);
        Assert.That(controller.legendLabel, Is.Not.Null);
        Assert.That(controller.detectorCaptionLabel, Is.Not.Null);
        Assert.That(controller.exposureCaptionLabel, Is.Not.Null);
        Assert.That(controller.toastLabel, Is.Not.Null);
        Assert.That(controller.interactLabel, Is.Not.Null);
        Assert.That(joystick.Value, Is.EqualTo(Vector2.zero));

        RadiationManager manager = Object.FindFirstObjectByType<RadiationManager>();
        manager.BeginAlert();
        yield return new WaitForSeconds(0.1f);

        Assert.That(detector.readoutText, Is.Not.Null);
        Assert.That(detector.readoutText.text, Does.Match(@"^\d+ / 100$"));
        Assert.That(detector.readoutText.text, Does.Not.Contain("\u00b5"));
        string status = detector.statusText.text;
        Assert.That(status == "SAFE" || status == "WARNING" || status == "HIGH", Is.True, status);
    }
}
