using System;
using UnityEngine;

namespace MarsOutpost.Weather
{
    /// <summary>
    /// Replaceable source for one training alert. A future implementation can read a
    /// cached/network service; the controller only needs a success or a readable failure
    /// so it can fall back to the authored alert.
    /// </summary>
    public interface ISpaceWeatherAlertSource
    {
        string SourceLabel { get; }

        bool TryLoad(out SpaceWeatherAlertDefinition alert, out string error);
    }

    /// <summary>
    /// Reads the curated local JSON file. No network access, API key, or live NASA feed is
    /// used by this build.
    /// </summary>
    public sealed class LocalJsonSpaceWeatherAlertSource : ISpaceWeatherAlertSource
    {
        private readonly TextAsset file;

        public LocalJsonSpaceWeatherAlertSource(TextAsset file)
        {
            this.file = file;
        }

        public string SourceLabel => "LOCAL TRAINING FILE";

        public bool TryLoad(out SpaceWeatherAlertDefinition alert, out string error)
        {
            alert = null;
            error = null;

            if (file == null)
            {
                error = "The curated training file is not assigned.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(file.text))
            {
                error = "The curated training file is empty.";
                return false;
            }

            try
            {
                SpaceWeatherAlertFile parsed = JsonUtility.FromJson<SpaceWeatherAlertFile>(file.text);
                if (parsed == null || parsed.alerts == null || parsed.alerts.Length == 0 || parsed.alerts[0] == null)
                {
                    error = "The curated training file does not contain an alert.";
                    return false;
                }

                alert = parsed.alerts[0];
                return true;
            }
            catch (Exception exception)
            {
                error = $"The curated training file could not be read: {exception.Message}";
                return false;
            }
        }
    }

    /// <summary>
    /// Shared fallback path. Any alert source can fail without leaving the child on a
    /// broken screen: the authored fallback is used and the UI shows a quiet source label.
    /// </summary>
    public static class SpaceWeatherAlertLibrary
    {
        public const string FallbackSourceLabel = "BUILT-IN FALLBACK";

        public static bool TryLoad(
            ISpaceWeatherAlertSource source,
            SpaceWeatherAlertDefinition fallback,
            out SpaceWeatherAlertDefinition alert,
            out string sourceLabel,
            out string error)
        {
            alert = null;
            sourceLabel = string.Empty;
            error = null;

            if (source != null && source.TryLoad(out alert, out error) && alert != null)
            {
                sourceLabel = source.SourceLabel;
                return true;
            }

            if (fallback != null)
            {
                alert = fallback;
                sourceLabel = FallbackSourceLabel;
                return true;
            }

            return false;
        }
    }
}
