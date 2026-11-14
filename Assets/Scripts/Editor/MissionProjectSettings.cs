using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace MarsOutpost.EditorTools
{
    /// <summary>
    /// Android-first player settings for a landscape-friendly mobile build. This does not
    /// install the Android module; it only records the intended settings in the project.
    /// </summary>
    public static class MissionProjectSettings
    {
        public static void Apply()
        {
            PlayerSettings.companyName = "Mars Outpost Learning Lab";
            PlayerSettings.productName = "Mars Outpost: Junior Astronaut Training";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.colorSpace = ColorSpace.Linear;

            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;

            // Landscape is the primary layout, but keep portrait available for phones/tablets.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            PlayerSettings.SetApplicationIdentifier(
                NamedBuildTarget.Android,
                "com.marsoutpost.juniorastronaut");

            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.forceInternetPermission = false;
            PlayerSettings.Android.minifyRelease = false;
            PlayerSettings.Android.useCustomKeystore = false;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);

            AssetDatabase.SaveAssets();
            Debug.Log("[MarsOutpost] Applied Android-first player settings.");
        }
    }
}
