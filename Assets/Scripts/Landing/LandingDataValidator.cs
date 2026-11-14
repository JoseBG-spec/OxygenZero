using System.Collections.Generic;
using MarsOutpost.Core;
using UnityEngine;

namespace MarsOutpost.Landing
{
    /// <summary>
    /// Catches authoring mistakes in the landing content before a child reaches a
    /// broken map. Returns human-readable messages for the Console.
    /// </summary>
    public static class LandingDataValidator
    {
        public static List<string> Validate(LandingMissionConfig config)
        {
            var errors = new List<string>();
            if (config == null)
            {
                errors.Add("Landing config is missing.");
                return errors;
            }

            if (config.sites == null || config.sites.Length < 2)
            {
                errors.Add("Landing config needs at least two sites.");
                return errors;
            }

            var ids = new HashSet<string>();
            for (int i = 0; i < config.sites.Length; i++)
            {
                LandingSiteDefinition site = config.sites[i];
                if (site == null)
                {
                    errors.Add($"Landing site slot {i + 1} is empty.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(site.id))
                {
                    errors.Add($"Landing site slot {i + 1} has an empty id.");
                }
                else if (!ids.Add(site.id))
                {
                    errors.Add($"Duplicate landing site id '{site.id}'.");
                }

                if (string.IsNullOrWhiteSpace(site.displayName))
                {
                    errors.Add($"Landing site '{site.id}' has no display name.");
                }

                if (site.mapPosition.x < 0f || site.mapPosition.x > 1f ||
                    site.mapPosition.y < 0f || site.mapPosition.y > 1f)
                {
                    errors.Add($"Landing site '{site.id}' has a map position outside 0..1.");
                }

                ValidateTag(errors, site.id, "slope", site.slope);
                ValidateTag(errors, site.id, "dust", site.dust);
                ValidateTag(errors, site.id, "distance", site.distance);

                if (string.IsNullOrWhiteSpace(site.resultSummary))
                {
                    errors.Add($"Landing site '{site.id}' has no result summary.");
                }

                if (site.changes == null || site.changes.Length == 0)
                {
                    errors.Add($"Landing site '{site.id}' has no resource changes.");
                    continue;
                }

                foreach (ResourceChange change in site.changes)
                {
                    if (string.IsNullOrWhiteSpace(change.resourceId))
                    {
                        errors.Add($"Landing site '{site.id}' has a resource change with no resource id.");
                    }

                    if (string.IsNullOrWhiteSpace(change.reason))
                    {
                        errors.Add($"Landing site '{site.id}' has a resource change with no reason.");
                    }
                }
            }

            return errors;
        }

        private static void ValidateTag(List<string> errors, string siteId, string name, LandingHazardTag tag)
        {
            if (tag == null)
            {
                errors.Add($"Landing site '{siteId}' is missing its {name} tag.");
                return;
            }

            if (string.IsNullOrWhiteSpace(tag.label))
            {
                errors.Add($"Landing site '{siteId}' has an empty {name} label.");
            }

            if (tag.tone < 0 || tag.tone > 2)
            {
                errors.Add($"Landing site '{siteId}' has a {name} tone outside 0..2.");
            }
        }
    }
}
