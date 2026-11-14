using System.IO;
using System.Linq;
using MarsOutpost.Core;
using MarsOutpost.Greenhouse;
using MarsOutpost.Landing;
using MarsOutpost.Radiation;
using MarsOutpost.Weather;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MarsOutpost.EditorTools
{
    /// <summary>
    /// One-time content generator and validation menu. Existing scenes and prefabs are the
    /// source of truth after generation; rebuilding is explicit and asks for confirmation.
    /// </summary>
    public static class Phase1Builder
    {
        [MenuItem("Tools/Mars Outpost/Import TMP Essentials", priority = 0)]
        public static void ImportTmpEssentials()
        {
            bool ok = TmpEssentials.Ensure();
            EditorUtility.DisplayDialog(
                "Mars Outpost",
                ok ? "TMP Essential Resources are imported." : "TMP import did not complete. Check the Console for details.",
                "OK");
        }

        [MenuItem("Tools/Mars Outpost/Build Phase 1 Content", priority = 1)]
        public static void BuildFromMenu()
        {
            if (File.Exists(MissionDataBuilder.MissionConfigPath))
            {
                bool rebuild = EditorUtility.DisplayDialog(
                    "Rebuild Phase 1 content?",
                    "This replaces the generated scenes, prefabs, and data assets. " +
                    "If you have edited them, make a backup or use version control first.",
                    "Rebuild",
                    "Cancel");

                if (!rebuild)
                {
                    return;
                }
            }

            BuildAll();
        }

        /// <summary>Batchmode entry point. Run with -executeMethod MarsOutpost.EditorTools.Phase1Builder.BuildAll.</summary>
        public static void BuildAll()
        {
            if (!TmpEssentials.Ensure())
            {
                throw new System.InvalidOperationException("TMP essentials are required before building content.");
            }

            GeneratedSpriteBuilder.Sprites sprites = GeneratedSpriteBuilder.BuildAll();
            MissionDataBuilder.Content content = MissionDataBuilder.Build(sprites);
            MissionUiBuilder.Prefabs prefabs = MissionUiBuilder.Build(content);
            MissionSceneBuilder.BuildAll(content, prefabs);
            MissionProjectSettings.Apply();

            // The radiation drill is optional, but a full content rebuild must keep its
            // activity card and generated scene in sync with the other activities.
            RadiationSceneBuilder.BuildRadiationOnly();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[MarsOutpost] Phase 1 content built: 7 scenes, 7 UI prefabs, data assets.");
        }

        /// <summary>
        /// Targeted Phase 4 build: regenerates only the weather art, config, plan prefab,
        /// and scene. Does not touch the other activities or the project settings.
        /// </summary>
        [MenuItem("Tools/Mars Outpost/Build Space Weather Scene", priority = 3)]
        public static void BuildSpaceWeatherFromMenu()
        {
            bool rebuild = EditorUtility.DisplayDialog(
                "Rebuild the space-weather screen?",
                "This replaces the generated weather scene, plan prefab, art, and weather config. " +
                "Other screens and data are left alone.",
                "Rebuild",
                "Cancel");
            if (!rebuild)
            {
                return;
            }

            BuildSpaceWeatherOnly();
        }

        /// <summary>Batchmode entry point for the Phase 4 screen.</summary>
        public static void BuildSpaceWeatherOnly()
        {
            if (!TmpEssentials.Ensure())
            {
                throw new System.InvalidOperationException("TMP essentials are required before building content.");
            }

            Sprite art = GeneratedSpriteBuilder.BuildWeatherControlRoom();
            var sprites = new GeneratedSpriteBuilder.Sprites
            {
                WeatherControlRoom = art
            };

            MissionDataBuilder.Content content = MissionDataBuilder.BuildSpaceWeather(sprites);
            if (content.WeatherConfig == null)
            {
                throw new System.InvalidOperationException("The space-weather config was not created.");
            }

            var errors = SpaceWeatherDataValidator.Validate(content.WeatherConfig);
            if (errors.Count > 0)
            {
                foreach (string error in errors)
                {
                    Debug.LogError($"[MarsOutpost] Space weather data: {error}");
                }

                throw new System.InvalidOperationException(
                    "Space weather validation failed; the scene was not saved. Fix the logged data errors and rerun.");
            }

            MissionUiBuilder.Prefabs prefabs = MissionUiBuilder.BuildSpaceWeather(content);
            MissionSceneBuilder.BuildWeather(content, prefabs);

            var scenes = EditorBuildSettings.scenes.ToList();
            int existing = scenes.FindIndex(entry => entry.path == MissionSceneBuilder.WeatherPath);
            if (existing < 0)
            {
                scenes.Add(new EditorBuildSettingsScene(MissionSceneBuilder.WeatherPath, true));
            }
            else
            {
                scenes[existing] = new EditorBuildSettingsScene(MissionSceneBuilder.WeatherPath, true);
            }

            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[MarsOutpost] Phase 4 space-weather screen built.");
        }

        [MenuItem("Tools/Mars Outpost/Validate Mission Content", priority = 2)]
        public static void ValidateFromMenu()
        {
            MissionConfig config = AssetDatabase.LoadAssetAtPath<MissionConfig>(MissionDataBuilder.MissionConfigPath);
            if (config == null)
            {
                Debug.LogError("[MarsOutpost] MissionConfig asset is missing. Build Phase 1 content first.");
                return;
            }

            var errors = MissionDataValidator.Validate(config);
            foreach (string error in errors)
            {
                Debug.LogError($"[MarsOutpost] Mission data: {error}");
            }

            LandingMissionConfig landing = AssetDatabase.LoadAssetAtPath<LandingMissionConfig>(
                "Assets/Data/Landing/LandingConfig.asset");
            if (landing == null)
            {
                Debug.LogError("[MarsOutpost] LandingConfig asset is missing. Build Phase 1 content first.");
            }
            else
            {
                foreach (string error in LandingDataValidator.Validate(landing))
                {
                    Debug.LogError($"[MarsOutpost] Landing data: {error}");
                    errors.Add(error);
                }
            }

            GreenhouseExperimentConfig greenhouse = AssetDatabase.LoadAssetAtPath<GreenhouseExperimentConfig>(
                "Assets/Data/Greenhouse/GreenhouseConfig.asset");
            if (greenhouse == null)
            {
                Debug.LogError("[MarsOutpost] GreenhouseConfig asset is missing. Build Phase 1 content first.");
            }
            else
            {
                foreach (string error in GreenhouseDataValidator.Validate(greenhouse))
                {
                    Debug.LogError($"[MarsOutpost] Greenhouse data: {error}");
                    errors.Add(error);
                }
            }

            RadiationConfig radiation = AssetDatabase.LoadAssetAtPath<RadiationConfig>(
                RadiationSceneBuilder.ConfigPath);
            if (radiation == null)
            {
                Debug.LogError("[MarsOutpost] RadiationConfig asset is missing. Build the radiation drill scene.");
            }
            else
            {
                foreach (string error in RadiationDataValidator.Validate(radiation))
                {
                    Debug.LogError($"[MarsOutpost] Radiation data: {error}");
                    errors.Add(error);
                }
            }

            string[] scenes =
            {
                MissionSceneBuilder.MainMenuPath,
                MissionSceneBuilder.HubPath,
                MissionSceneBuilder.LandingPath,
                MissionSceneBuilder.GreenhousePath,
                MissionSceneBuilder.WeatherPath,
                MissionSceneBuilder.DebriefPath,
                RadiationSceneBuilder.ScenePath
            };

            foreach (string scene in scenes)
            {
                bool inBuild = EditorBuildSettings.scenes.Any(entry => entry.path == scene && entry.enabled);
                if (!inBuild)
                {
                    Debug.LogError($"[MarsOutpost] Scene is not enabled in Build Settings: {scene}");
                }
            }

            if (errors.Count == 0)
            {
                Debug.Log("[MarsOutpost] Mission content validation passed.");
            }
        }

        [MenuItem("Tools/Mars Outpost/Open Main Menu Scene", priority = 20)]
        public static void OpenMainMenu()
        {
            if (File.Exists(MissionSceneBuilder.MainMenuPath))
            {
                EditorSceneManager.OpenScene(MissionSceneBuilder.MainMenuPath);
            }
            else
            {
                EditorUtility.DisplayDialog("Mars Outpost", "Build Phase 1 content first.", "OK");
            }
        }
    }
}
