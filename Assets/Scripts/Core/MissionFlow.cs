using UnityEngine;
using UnityEngine.SceneManagement;

namespace MarsOutpost.Core
{
    /// <summary>
    /// Scene loading with a friendly failure path. A missing Build Settings entry
    /// should never leave a child on a blank screen.
    /// </summary>
    public static class MissionFlow
    {
        public static bool TryLoad(string sceneName, out string error)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                error = "No scene is configured for that button.";
                return false;
            }

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                error = $"Scene '{sceneName}' is not in Build Settings. Add it under File > Build Settings.";
                return false;
            }

            SceneManager.LoadScene(sceneName);
            error = null;
            return true;
        }
    }
}
