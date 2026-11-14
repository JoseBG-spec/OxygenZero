using System.Collections.Generic;
using UnityEngine;

namespace MarsOutpost.Core
{
    /// <summary>
    /// Small data checks that can run in the Editor and in tests. Scene existence in
    /// Build Settings is intentionally an editor-only check (see MissionDataTests).
    /// </summary>
    public static class MissionDataValidator
    {
        public static List<string> Validate(MissionConfig config)
        {
            var errors = new List<string>();

            if (config == null)
            {
                errors.Add("MissionConfig is missing.");
                return errors;
            }

            if (string.IsNullOrWhiteSpace(config.gameTitle))
            {
                errors.Add("gameTitle is empty.");
            }

            if (string.IsNullOrWhiteSpace(config.hubScene))
            {
                errors.Add("hubScene is empty.");
            }

            if (string.IsNullOrWhiteSpace(config.mainMenuScene))
            {
                errors.Add("mainMenuScene is empty.");
            }

            if (string.IsNullOrWhiteSpace(config.debriefScene))
            {
                errors.Add("debriefScene is empty.");
            }

            if (config.resources == null || config.resources.Length == 0)
            {
                errors.Add("At least one ResourceDefinition is required.");
            }
            else
            {
                var resourceIds = new HashSet<string>();
                foreach (ResourceDefinition resource in config.resources)
                {
                    if (resource == null)
                    {
                        errors.Add("A resource entry is null.");
                        continue;
                    }

                    string id = resource.IdOrFallback;
                    if (!resourceIds.Add(id))
                    {
                        errors.Add($"Duplicate resource id '{id}'.");
                    }

                    if (resource.maxValue < resource.minValue)
                    {
                        errors.Add($"Resource '{id}' has maxValue below minValue.");
                    }

                    if (resource.startingValue < resource.minValue || resource.startingValue > resource.maxValue)
                    {
                        errors.Add($"Resource '{id}' startingValue is outside its range.");
                    }
                }
            }

            if (config.activities == null || config.activities.Length == 0)
            {
                errors.Add("At least one ActivityDefinition is required.");
            }
            else
            {
                var activityIds = new HashSet<string>();
                foreach (ActivityDefinition activity in config.activities)
                {
                    if (activity == null)
                    {
                        errors.Add("An activity entry is null.");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(activity.id))
                    {
                        errors.Add($"Activity '{activity.name}' has an empty id.");
                    }
                    else if (!activityIds.Add(activity.id))
                    {
                        errors.Add($"Duplicate activity id '{activity.id}'.");
                    }

                    if (string.IsNullOrWhiteSpace(activity.displayName))
                    {
                        errors.Add($"Activity '{activity.name}' has an empty displayName.");
                    }

                    if (string.IsNullOrWhiteSpace(activity.sceneName))
                    {
                        errors.Add($"Activity '{activity.name}' has an empty sceneName.");
                    }
                }
            }

            if (config.debrief == null)
            {
                errors.Add("DebriefConfig is missing.");
            }
            else if (config.debrief.bands == null || config.debrief.bands.Length == 0)
            {
                errors.Add("DebriefConfig needs at least one band.");
            }

            if (config.theme == null)
            {
                errors.Add("UiTheme is missing.");
            }
            else if (config.theme.font == null)
            {
                errors.Add("UiTheme.font is missing. Import TMP Essential Resources first.");
            }

            if (config.scienceTarget < 0)
            {
                errors.Add("scienceTarget cannot be negative.");
            }

            return errors;
        }
    }
}
