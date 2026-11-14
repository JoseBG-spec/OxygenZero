using System.Collections.Generic;
using System.Text;
using MarsOutpost.Core;
using UnityEngine;

namespace MarsOutpost.UI
{
    /// <summary>Shared rich-text formatting for consequence lists.</summary>
    public static class UiTextFormat
    {
        public static string FormatChanges(IReadOnlyList<AppliedResourceChange> changes)
        {
            if (changes == null || changes.Count == 0)
            {
                return "No resource changes.";
            }

            var builder = new StringBuilder();
            foreach (AppliedResourceChange change in changes)
            {
                string color = ColorUtility.ToHtmlStringRGB(UiThemeService.ColorForDelta(change.AppliedDelta));
                string sign = change.AppliedDelta >= 0 ? "+" : "-";
                builder.Append($"<color=#{color}><b>{change.DisplayName} {sign}{Mathf.Abs(change.AppliedDelta)}</b></color>");

                if (change.WasClamped)
                {
                    builder.Append(" <size=80%>(limit reached)</size>");
                }

                if (!string.IsNullOrWhiteSpace(change.Reason))
                {
                    builder.Append($"\n{change.Reason}");
                }

                builder.Append('\n');
            }

            return builder.ToString().TrimEnd();
        }

        public static string FormatChangeSummary(IReadOnlyList<AppliedResourceChange> changes)
        {
            if (changes == null || changes.Count == 0)
            {
                return "no resource changes";
            }

            var builder = new StringBuilder();
            for (int i = 0; i < changes.Count; i++)
            {
                AppliedResourceChange change = changes[i];
                if (i > 0)
                {
                    builder.Append(", ");
                }

                string sign = change.AppliedDelta >= 0 ? "+" : "-";
                builder.Append($"{change.DisplayName} {sign}{Mathf.Abs(change.AppliedDelta)}");
            }

            return builder.ToString();
        }
    }
}
