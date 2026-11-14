using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace MarsOutpost.EditorTools
{
    /// <summary>
    /// Imports the TextMeshPro essential resources that ship inside the uGUI package.
    /// Unity does not auto-import these into a fresh project.
    /// </summary>
    public static class TmpEssentials
    {
        public const string SettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
        public const string FallbackFontPath =
            "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

        public static bool Ensure()
        {
            if (HasSettings())
            {
                return true;
            }

            string[] packages = Directory.GetFiles(
                "Library/PackageCache",
                "TMP Essential Resources.unitypackage",
                SearchOption.AllDirectories);

            if (packages.Length == 0)
            {
                Debug.LogError(
                    "[MarsOutpost] Could not find 'TMP Essential Resources.unitypackage' in Library/PackageCache. " +
                    "Open Window > TextMeshPro > Import TMP Essential Resources, then run the builder again.");
                return false;
            }

            Debug.Log($"[MarsOutpost] Importing TMP essentials from {packages[0]}");
            AssetDatabase.ImportPackage(packages[0], false);
            AssetDatabase.Refresh();

            if (!HasSettings())
            {
                Debug.LogError(
                    "[MarsOutpost] TMP import finished but no TMP Settings asset was found. " +
                    "Import TMP Essential Resources manually, then try again.");
                return false;
            }

            return true;
        }

        public static TMP_FontAsset GetDefaultFont()
        {
            TMP_Settings settings = AssetDatabase.LoadAssetAtPath<TMP_Settings>(SettingsPath);
            if (settings != null)
            {
                var serialized = new SerializedObject(settings);
                SerializedProperty font = serialized.FindProperty("m_defaultFontAsset");
                if (font != null && font.objectReferenceValue is TMP_FontAsset configured)
                {
                    return configured;
                }
            }

            return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FallbackFontPath);
        }

        private static bool HasSettings()
        {
            if (AssetDatabase.LoadAssetAtPath<TMP_Settings>(SettingsPath) != null)
            {
                return true;
            }

            // Package versions differ slightly in folder naming; accept any TMP Settings asset.
            string[] guids = AssetDatabase.FindAssets("t:TMP_Settings");
            return guids.Length > 0;
        }
    }
}
