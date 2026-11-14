using System;
using System.Collections;
using System.IO;
using System.Linq;
using MarsOutpost.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MarsOutpost.Greenhouse
{
    /// <summary>
    /// Temporary developer harness for capturing the greenhouse states in a Windows
    /// preview build. It is only active when the player is launched with -phase3capture.
    /// </summary>
    public sealed class GreenhouseCaptureHarness : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Application.isEditor)
            {
                return;
            }

            if (!Environment.GetCommandLineArgs().Contains("-phase3capture"))
            {
                return;
            }

            var go = new GameObject("GreenhouseCaptureHarness");
            DontDestroyOnLoad(go);
            go.AddComponent<GreenhouseCaptureHarness>();
        }

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(0.6f);
            SceneManager.LoadScene("03_Greenhouse");
            yield return new WaitForSeconds(0.3f);

            bool accessibilityPass = Environment.GetCommandLineArgs().Contains("-phase3a11y");
            if (accessibilityPass)
            {
                UiThemeService.CycleFontScale();
                UiThemeService.CycleFontScale();
                UiThemeService.ToggleHighContrast();
                yield return new WaitForSeconds(0.25f);
            }

            var controller = FindAnyObjectByType<GreenhouseController>();
            if (controller == null)
            {
                Debug.LogError("[Capture] GreenhouseController was not found.");
                Application.Quit(2);
                yield break;
            }

            yield return Capture("phase3_greenhouse_01_setup");
            LogRunState(controller, "setup");

            controller.trayA.redButton.onClick.Invoke();
            controller.trayB.darkButton.onClick.Invoke();
            yield return new WaitForSeconds(0.2f);
            yield return Capture("phase3_greenhouse_02_fair_test");
            LogRunState(controller, "fair-test");

            controller.runButton.onClick.Invoke();
            yield return new WaitForSeconds(0.2f);
            yield return Capture("phase3_greenhouse_03_compare");

            GreenhouseClaimOptionView claim = controller.ClaimViews
                .FirstOrDefault(view => view.Claim != null && view.Claim.matchesStudy);
            if (claim != null)
            {
                claim.button.onClick.Invoke();
                yield return new WaitForSeconds(0.2f);
                yield return Capture("phase3_greenhouse_04_claim");
            }

            controller.confirmButton.onClick.Invoke();
            yield return new WaitForSeconds(0.2f);
            yield return Capture("phase3_greenhouse_05_result");

            if (accessibilityPass)
            {
                while (UiThemeService.FontScale > 1.01f)
                {
                    UiThemeService.CycleFontScale();
                }

                if (UiThemeService.HighContrast)
                {
                    UiThemeService.ToggleHighContrast();
                }
            }

            yield return new WaitForSeconds(0.4f);
            Application.Quit(0);
        }

        private static void LogRunState(GreenhouseController controller, string phase)
        {
            Button button = controller.runButton;
            var image = button.targetGraphic as UnityEngine.UI.Image;
            Debug.Log($"[Capture] {phase}: interactable={button.IsInteractable()} locked={controller.IsLocked} " +
                      $"image={(image != null ? image.color.ToString() : "null")} " +
                      $"normal={button.colors.normalColor} disabled={button.colors.disabledColor}");
        }

        private static IEnumerator Capture(string name)
        {
            yield return new WaitForEndOfFrame();
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", ".."));
            string directory = Path.Combine(root, "Logs");
            Directory.CreateDirectory(directory);
            string suffix = Environment.GetCommandLineArgs().Contains("-phase3a11y") ? "_a11y" : string.Empty;
            string path = Path.Combine(directory, name + suffix + ".png");
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log($"[Capture] {path}");
            yield return null;
            yield return null;
        }
    }
}
