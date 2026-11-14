using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MarsOutpost.EditorTools
{
    /// <summary>Builds a desktop preview for layout review. Android packaging is documented in the README.</summary>
    public static class PreviewBuilder
    {
        [MenuItem("Tools/Mars Outpost/Build Windows Preview", priority = 40)]
        public static void BuildWindowsPreview()
        {
            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                throw new System.InvalidOperationException("No enabled scenes in Build Settings. Build Phase 1 content first.");
            }

            string directory = Path.Combine(Directory.GetCurrentDirectory(), "Builds", "Windows");
            Directory.CreateDirectory(directory);
            string executable = Path.Combine(directory, "MarsOutpost.exe");

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = executable,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            Debug.Log(
                $"[MarsOutpost] Windows preview: {report.summary.result}, " +
                $"{report.summary.totalSize / (1024 * 1024)} MB, {executable}");

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new System.InvalidOperationException("Windows preview build failed: " + report.summary.result);
            }
        }
    }
}
