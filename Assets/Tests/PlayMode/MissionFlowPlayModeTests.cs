using System.Collections;
using System.Linq;
using System.Reflection;
using MarsOutpost.Core;
using MarsOutpost.Greenhouse;
using MarsOutpost.Landing;
using MarsOutpost.UI;
using MarsOutpost.Weather;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace MarsOutpost.Tests
{
    public sealed class MissionFlowPlayModeTests
    {
        [UnityTest]
        public IEnumerator BootstrapCreatesPersistentDirector()
        {
            MissionConfig config = ScriptableObject.CreateInstance<MissionConfig>();
            config.gameTitle = "Test Mission";
            config.hubScene = "01_MissionHub";
            config.mainMenuScene = "00_MainMenu";
            config.debriefScene = "05_Debrief";

            ResourceDefinition power = ScriptableObject.CreateInstance<ResourceDefinition>();
            power.id = "power";
            power.displayName = "Power";
            power.startingValue = 10;
            power.minValue = 0;
            power.maxValue = 20;
            config.resources = new[] { power };

            ActivityDefinition activity = ScriptableObject.CreateInstance<ActivityDefinition>();
            activity.id = "test";
            activity.displayName = "Test Activity";
            activity.sceneName = "02_LandingSite";
            config.activities = new[] { activity };

            DebriefConfig debrief = ScriptableObject.CreateInstance<DebriefConfig>();
            debrief.bands = new[] { new DebriefBand { title = "Done", message = "Done", minimumScience = 0 } };
            config.debrief = debrief;

            UiTheme theme = ScriptableObject.CreateInstance<UiTheme>();
            theme.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            config.theme = theme;
            config.scienceTarget = 0;

            var host = new GameObject("TestBootstrap");
            host.SetActive(false);
            var bootstrap = host.AddComponent<MissionSceneBootstrap>();

            FieldInfo configField = typeof(MissionSceneBootstrap).GetField(
                "config",
                BindingFlags.Instance | BindingFlags.NonPublic);
            configField?.SetValue(bootstrap, config);

            host.SetActive(true);

            yield return null;

            Assert.That(MissionDirector.Instance, Is.Not.Null);
            Assert.That(MissionDirector.Instance.Session, Is.Not.Null);
            Assert.That(MissionDirector.Instance.Config, Is.SameAs(config));

            if (MissionDirector.Instance != null)
            {
                Object.DestroyImmediate(MissionDirector.Instance.gameObject);
            }

            Object.DestroyImmediate(host);
            Object.DestroyImmediate(config);
            Object.DestroyImmediate(power);
            Object.DestroyImmediate(activity);
            Object.DestroyImmediate(debrief);
            Object.DestroyImmediate(theme);
            yield return null;
        }

        [UnityTest]
        public IEnumerator MissingSceneReturnsFriendlyError()
        {
            yield return null;

            bool loaded = MissionFlow.TryLoad("Scene_That_Does_Not_Exist", out string error);

            Assert.That(loaded, Is.False);
            Assert.That(error, Does.Contain("Build Settings"));
        }

        [UnityTest]
        public IEnumerator ActivityBackButtonReturnsToHub()
        {
            SceneManager.LoadScene("02_LandingSite");
            yield return null;
            yield return null;

            var controller = Object.FindAnyObjectByType<LandingSiteController>();
            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.backButton, Is.Not.Null, "The activity Back button must be wired at runtime.");

            controller.backButton.onClick.Invoke();
            yield return null;
            yield return null;

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("01_MissionHub"));

            SceneManager.LoadScene("00_MainMenu");
            yield return null;
        }

        [UnityTest]
        public IEnumerator LandingSelectionLogsOneOutcomeAndReachesResult()
        {
            SceneManager.LoadScene("02_LandingSite");
            yield return null;
            yield return null;

            var controller = Object.FindAnyObjectByType<LandingSiteController>();
            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.markerContainer, Is.Not.Null);
            Assert.That(controller.markerContainer.childCount, Is.GreaterThanOrEqualTo(3));
            Assert.That(controller.confirmButton.interactable, Is.False,
                "Confirm should stay disabled until the player picks a site.");

            var marker = Object.FindObjectsByType<LandingSiteMarkerView>()
                .FirstOrDefault(m => m.Site != null);
            Assert.That(marker, Is.Not.Null, "The landing map should create a site marker.");

            marker.button.onClick.Invoke();
            yield return null;

            Assert.That(controller.confirmButton.interactable, Is.True);
            Assert.That(controller.siteNameLabel.text, Is.EqualTo(marker.Site.displayName));

            string siteId = marker.Site.id;
            controller.confirmButton.onClick.Invoke();
            yield return null;

            Assert.That(MissionDirector.Instance.Session.HasOutcome("landing"), Is.True);
            Assert.That(MissionDirector.Instance.Session.GetOutcome("landing").ChoiceId, Is.EqualTo(siteId));
            Assert.That(controller.resultPanel.IsOpen, Is.True);

            SceneManager.LoadScene("00_MainMenu");
            yield return null;

            if (MissionDirector.Instance != null)
            {
                MissionDirector.Instance.Session.Reset();
            }
        }

        [UnityTest]
        public IEnumerator GreenhouseTreatmentSelectionIsVisibleOnTheButtons()
        {
            SceneManager.LoadScene("03_Greenhouse");
            yield return null;
            yield return null;

            var controller = Object.FindAnyObjectByType<GreenhouseController>();
            Assert.That(controller, Is.Not.Null);

            UiTheme theme = UiThemeService.ActiveTheme;
            var red = controller.trayA.redButton.GetComponent<ThemedButton>();
            var dark = controller.trayA.darkButton.GetComponent<ThemedButton>();
            Assert.That(red, Is.Not.Null);
            Assert.That(dark, Is.Not.Null);

            controller.trayA.redButton.onClick.Invoke();
            yield return null;

            Assert.That(red.selected, Is.True, "The chosen treatment button should show a selected state.");
            Assert.That(dark.selected, Is.False);
            Assert.That(red.background.color, Is.EqualTo(theme.primary),
                "The selected treatment button should use the primary accent.");
            Assert.That(controller.trayA.redButtonLabel.color, Is.EqualTo(theme.primaryText),
                "The selected treatment label should use the dark on-accent color.");

            controller.trayB.darkButton.onClick.Invoke();
            yield return null;

            Assert.That(controller.runButton.interactable, Is.True);
            Assert.That(controller.runLabel.color, Is.EqualTo(theme.primaryText),
                "The enabled primary action label should use the dark on-accent color.");

            SceneManager.LoadScene("00_MainMenu");
            yield return null;

            if (MissionDirector.Instance != null)
            {
                MissionDirector.Instance.Session.Reset();
            }
        }

        [UnityTest]
        public IEnumerator GreenhouseClaimFitsAtLargestTextScale()
        {
            SceneManager.LoadScene("03_Greenhouse");
            yield return null;
            yield return null;

            try
            {
                UiThemeService.CycleFontScale();
                UiThemeService.CycleFontScale();
                Canvas.ForceUpdateCanvases();
                yield return null;

                var controller = Object.FindAnyObjectByType<GreenhouseController>();
                Assert.That(controller, Is.Not.Null);

                controller.trayA.redButton.onClick.Invoke();
                controller.trayB.darkButton.onClick.Invoke();
                yield return null;
                controller.runButton.onClick.Invoke();
                yield return null;

                GreenhouseClaimOptionView claim = controller.ClaimViews
                    .FirstOrDefault(view => view.Claim != null && view.Claim.matchesStudy);
                Assert.That(claim, Is.Not.Null);

                claim.button.onClick.Invoke();
                yield return null;
                claim.statement.ForceMeshUpdate();

                Assert.That(claim.statement.textInfo.lineCount, Is.EqualTo(1),
                    "The claim sentence should stay on one line at 130% text scale.");
                Assert.That(claim.statement.preferredHeight,
                    Is.LessThanOrEqualTo(claim.statement.rectTransform.rect.height + 1f),
                    "The claim sentence should not clip inside its row at 130% text scale.");
                Assert.That(claim.statement.color, Is.EqualTo(UiThemeService.ActiveTheme.primaryText),
                    "The selected claim should use the dark on-accent text color.");
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
        public IEnumerator GreenhouseFairTestLogsOneOutcomeAndReachesResult()
        {
            SceneManager.LoadScene("03_Greenhouse");
            yield return null;
            yield return null;

            var controller = Object.FindAnyObjectByType<GreenhouseController>();
            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.trayA, Is.Not.Null);
            Assert.That(controller.trayB, Is.Not.Null);
            Assert.That(controller.runButton, Is.Not.Null);
            Assert.That(controller.runButton.interactable, Is.False,
                "Run should stay disabled until one tray is red and one is dark.");

            controller.trayA.redButton.onClick.Invoke();
            yield return null;
            Assert.That(controller.runButton.interactable, Is.False,
                "One treated tray is not a fair test.");

            controller.trayB.darkButton.onClick.Invoke();
            yield return null;
            Assert.That(controller.runButton.interactable, Is.True);

            controller.runButton.onClick.Invoke();
            yield return null;

            Assert.That(controller.TrialRun, Is.True);
            Assert.That(controller.claimsGroup.activeSelf, Is.True);
            Assert.That(controller.ClaimViews.Count, Is.GreaterThanOrEqualTo(2));

            GreenhouseClaimOptionView claim = controller.ClaimViews
                .FirstOrDefault(view => view.Claim != null && view.Claim.matchesStudy);
            Assert.That(claim, Is.Not.Null, "The study-supported claim should be present.");
            Assert.That(controller.confirmButton.gameObject.activeSelf, Is.True);
            Assert.That(controller.confirmButton.interactable, Is.False,
                "Confirm should wait for a claim.");

            claim.button.onClick.Invoke();
            yield return null;

            Assert.That(controller.confirmButton.interactable, Is.True);
            Assert.That(controller.confirmLabel.text, Is.EqualTo(controller.experiment.confirmLabel));

            controller.confirmButton.onClick.Invoke();
            yield return null;

            Assert.That(MissionDirector.Instance.Session.HasOutcome("greenhouse"), Is.True);
            ActivityOutcome outcome = MissionDirector.Instance.Session.GetOutcome("greenhouse");
            Assert.That(outcome.ChoiceId, Does.StartWith("a|"));
            Assert.That(outcome.Headline, Is.EqualTo(claim.Claim.headline));
            Assert.That(controller.resultPanel.IsOpen, Is.True);

            SceneManager.LoadScene("00_MainMenu");
            yield return null;

            if (MissionDirector.Instance != null)
            {
                MissionDirector.Instance.Session.Reset();
            }
        }

        [UnityTest]
        public IEnumerator WeatherScreenShowsCuratedAlertAndLocksConfirm()
        {
            SceneManager.LoadScene("04_SpaceWeather");
            yield return null;
            yield return null;

            var controller = Object.FindAnyObjectByType<SpaceWeatherController>();
            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.IsReady, Is.True);
            Assert.That(controller.Alert, Is.Not.Null);
            Assert.That(controller.Alert.eventType, Is.EqualTo("FLR"));
            Assert.That(controller.Alert.choices, Has.Length.EqualTo(3));
            Assert.That(controller.PlanViews, Has.Count.EqualTo(3));
            Assert.That(
                controller.PlanViews.Select(plan => plan.indexLabel.text),
                Is.EqualTo(new[] { "1", "2", "3" }),
                "Plan cards should be numbered so kids can talk about them.");
            Assert.That(controller.sourceLabel.text, Is.EqualTo("LOCAL TRAINING FILE"));
            Assert.That(controller.headlineLabel.text, Is.EqualTo("A bright burst from the Sun"),
                "The alert headline should be visible above the plan choices.");
            Assert.That(controller.sourceNoteLabel.text, Does.Contain("This alert is fictional"),
                "The alert must say the scenario is fictional.");
            Assert.That(controller.simulationNoteLabel.text, Does.Contain("not a forecast"),
                "The simulation disclaimer must stay on screen.");
            Assert.That(controller.confirmButton.interactable, Is.False,
                "Confirm should wait for a crew plan.");

            SceneManager.LoadScene("00_MainMenu");
            yield return null;

            if (MissionDirector.Instance != null)
            {
                MissionDirector.Instance.Session.Reset();
            }
        }

        [UnityTest]
        public IEnumerator WeatherPlanSelectionIsVisibleAndEnablesConfirm()
        {
            SceneManager.LoadScene("04_SpaceWeather");
            yield return null;
            yield return null;

            var controller = Object.FindAnyObjectByType<SpaceWeatherController>();
            Assert.That(controller, Is.Not.Null);
            UiTheme theme = UiThemeService.ActiveTheme;

            SpaceWeatherPlanView view = controller.PlanViews.First(plan => plan.Plan.correct);
            view.button.onClick.Invoke();
            yield return null;

            Assert.That(controller.SelectedPlan, Is.SameAs(view.Plan));
            Assert.That(view.stateLabel.text, Is.EqualTo("CHOSEN"));
            Assert.That(view.background.color, Is.EqualTo(theme.primary));
            Assert.That(view.titleLabel.color, Is.EqualTo(theme.primaryText));
            Assert.That(controller.confirmButton.interactable, Is.True);
            Assert.That(controller.confirmLabel.text, Is.EqualTo(controller.config.confirmLabel));

            SceneManager.LoadScene("00_MainMenu");
            yield return null;

            if (MissionDirector.Instance != null)
            {
                MissionDirector.Instance.Session.Reset();
            }
        }

        [UnityTest]
        public IEnumerator WeatherCompletionLogsOneOutcomeAndShowsResult()
        {
            SceneManager.LoadScene("04_SpaceWeather");
            yield return null;
            yield return null;

            var controller = Object.FindAnyObjectByType<SpaceWeatherController>();
            Assert.That(controller, Is.Not.Null);

            SpaceWeatherPlanView view = controller.PlanViews.First(plan => plan.Plan.correct);
            view.button.onClick.Invoke();
            yield return null;
            controller.confirmButton.onClick.Invoke();
            yield return null;

            Assert.That(MissionDirector.Instance.Session.HasOutcome("weather"), Is.True);
            ActivityOutcome outcome = MissionDirector.Instance.Session.GetOutcome("weather");
            Assert.That(outcome.ChoiceId, Is.EqualTo(view.Plan.id));
            Assert.That(outcome.Headline, Is.EqualTo(view.Plan.headline));
            Assert.That(controller.resultPanel.IsOpen, Is.True);
            Assert.That(controller.confirmButton.gameObject.activeSelf, Is.False);
            Assert.That(controller.PlanViews.All(plan => !plan.button.interactable), Is.True);

            controller.confirmButton.onClick.Invoke();
            yield return null;
            Assert.That(MissionDirector.Instance.Session.GetOutcome("weather").ChoiceId, Is.EqualTo(view.Plan.id));

            SceneManager.LoadScene("00_MainMenu");
            yield return null;

            if (MissionDirector.Instance != null)
            {
                MissionDirector.Instance.Session.Reset();
            }
        }

        [UnityTest]
        public IEnumerator WeatherDecoderTogglesWithoutHidingThePlans()
        {
            SceneManager.LoadScene("04_SpaceWeather");
            yield return null;
            yield return null;

            var controller = Object.FindAnyObjectByType<SpaceWeatherController>();
            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.DecoderOpen, Is.False);
            Assert.That(controller.alertGroup.activeSelf, Is.True);
            Assert.That(controller.decoderGroup.activeSelf, Is.False);

            controller.compareButton.onClick.Invoke();
            yield return null;

            Assert.That(controller.DecoderOpen, Is.True);
            Assert.That(controller.alertGroup.activeSelf, Is.False);
            Assert.That(controller.decoderGroup.activeSelf, Is.True);
            Assert.That(controller.compareLabel.text, Is.EqualTo(controller.config.backToAlertLabel));

            controller.compareButton.onClick.Invoke();
            yield return null;

            Assert.That(controller.DecoderOpen, Is.False);
            Assert.That(controller.alertGroup.activeSelf, Is.True);
            Assert.That(controller.decoderGroup.activeSelf, Is.False);
            Assert.That(controller.compareLabel.text, Is.EqualTo(controller.config.compareLabel));

            SceneManager.LoadScene("00_MainMenu");
            yield return null;

            if (MissionDirector.Instance != null)
            {
                MissionDirector.Instance.Session.Reset();
            }
        }

        [UnityTest]
        public IEnumerator WeatherPlanCardsFitAtLargestTextScale()
        {
            SceneManager.LoadScene("04_SpaceWeather");
            yield return null;
            yield return null;

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

                var controller = Object.FindAnyObjectByType<SpaceWeatherController>();
                Assert.That(controller, Is.Not.Null);

                controller.eventTypeLabel.ForceMeshUpdate();
                controller.alertSummaryLabel.ForceMeshUpdate();
                Assert.That(
                    controller.eventTypeLabel.preferredWidth,
                    Is.LessThanOrEqualTo(controller.eventTypeLabel.rectTransform.rect.width + 1.5f),
                    "The event-type badge should fit its three-letter code at 130% text.");
                Assert.That(
                    controller.alertSummaryLabel.preferredHeight,
                    Is.LessThanOrEqualTo(controller.alertSummaryLabel.rectTransform.rect.height + 1.5f),
                    "The alert summary should fit its panel at 130% text.");

                controller.headlineLabel.ForceMeshUpdate();
                Assert.That(
                    controller.headlineLabel.preferredHeight,
                    Is.LessThanOrEqualTo(controller.headlineLabel.rectTransform.rect.height + 1.5f),
                    "The alert headline should stay visible at 130% text.");

                controller.sourceNoteLabel.ForceMeshUpdate();
                Assert.That(
                    controller.sourceNoteLabel.preferredHeight,
                    Is.LessThanOrEqualTo(controller.sourceNoteLabel.rectTransform.rect.height + 1.5f),
                    "The fiction source note should stay visible at 130% text.");

                controller.simulationNoteLabel.ForceMeshUpdate();
                Assert.That(
                    controller.simulationNoteLabel.preferredHeight,
                    Is.LessThanOrEqualTo(controller.simulationNoteLabel.rectTransform.rect.height + 1.5f),
                    "The simulation disclaimer should stay visible at 130% text.");

                foreach (SpaceWeatherPlanView view in controller.PlanViews)
                {
                    view.titleLabel.ForceMeshUpdate();
                    view.descriptionLabel.ForceMeshUpdate();
                    Assert.That(
                        view.titleLabel.preferredHeight,
                        Is.LessThanOrEqualTo(view.titleLabel.rectTransform.rect.height + 1.5f),
                        $"{view.Plan.id} title should fit its card at 130% text.");
                    Assert.That(
                        view.descriptionLabel.preferredHeight,
                        Is.LessThanOrEqualTo(view.descriptionLabel.rectTransform.rect.height + 1.5f),
                        $"{view.Plan.id} description should fit its card at 130% text.");
                    Assert.That(
                        view.descriptionLabel.textInfo.lineCount,
                        Is.LessThanOrEqualTo(2),
                        $"{view.Plan.id} description should stay within two lines.");
                }
            }
            finally
            {
                while (UiThemeService.FontScale > 1.01f)
                {
                    UiThemeService.CycleFontScale();
                }
            }

            SceneManager.LoadScene("00_MainMenu");
            yield return null;

            if (MissionDirector.Instance != null)
            {
                MissionDirector.Instance.Session.Reset();
            }
        }
    }
}
