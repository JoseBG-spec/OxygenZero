using System.Collections.Generic;
using MarsOutpost.Core;
using MarsOutpost.Landing;
using MarsOutpost.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsOutpost.Landing
{
    /// <summary>
    /// Phase 2 landing-site activity. The child compares three fictional sites by
    /// tapping numbered markers, reads game hazard tags, and confirms a trade-off.
    /// No real map layers or landing measurements are used.
    /// </summary>
    public sealed class LandingSiteController : MissionScreen
    {
        public ActivityDefinition activity;
        public LandingMissionConfig landing;

        [Header("Header")]
        public TMP_Text titleLabel;
        public TMP_Text taglineLabel;
        public Button backButton;

        [Header("Map")]
        public Transform markerContainer;
        public LandingSiteMarkerView markerPrefab;
        public TMP_Text mapCaptionLabel;

        [Header("Details")]
        public TMP_Text detailsHeaderLabel;
        public TMP_Text siteNameLabel;
        public TMP_Text siteRegionLabel;
        public TMP_Text siteDescriptionLabel;
        public TMP_Text slopeLabel;
        public TMP_Text dustLabel;
        public TMP_Text distanceLabel;
        public Image slopeDot;
        public Image dustDot;
        public Image distanceDot;

        [Header("Actions")]
        public Button confirmButton;
        public TMP_Text confirmLabel;
        public ActivityResultPanel resultPanel;
        public ConfirmDialog dialog;

        private readonly List<LandingSiteMarkerView> markers = new List<LandingSiteMarkerView>();
        private LandingSiteDefinition selectedSite;
        private bool locked;

        private void Start()
        {
            if (activity == null || landing == null)
            {
                Debug.LogError("[MarsOutpost] Landing activity is missing its ActivityDefinition or LandingMissionConfig.");
                return;
            }

            if (titleLabel != null) titleLabel.text = landing.screenTitle;
            if (taglineLabel != null) taglineLabel.text = landing.tagline;
            if (mapCaptionLabel != null) mapCaptionLabel.text = landing.mapCaption;
            if (backButton != null) backButton.onClick.AddListener(RequestBack);
            if (confirmButton != null) confirmButton.onClick.AddListener(ConfirmLanding);

            foreach (string error in LandingDataValidator.Validate(landing))
            {
                Debug.LogError($"[MarsOutpost] Landing data: {error}");
            }

            BuildMarkers();

            bool alreadyCompleted = Director != null
                                    && Director.Session != null
                                    && Director.Session.HasOutcome(activity.id)
                                    && (Director.Config == null || !Director.Config.allowReplayCompletedActivities);

            if (alreadyCompleted)
            {
                locked = true;
                ActivityOutcome prior = Director.Session.GetOutcome(activity.id);
                LandingSiteDefinition priorSite = FindSite(prior != null ? prior.ChoiceId : null);
                if (priorSite != null)
                {
                    SelectSite(priorSite);
                }

                foreach (LandingSiteMarkerView marker in markers)
                {
                    marker.SetInteractable(false);
                }

                if (confirmButton != null) confirmButton.interactable = false;
                if (confirmLabel != null) confirmLabel.text = "Already completed";
            }
            else
            {
                ClearSelection();
            }
        }

        private void OnEnable()
        {
            UiThemeService.ThemeChanged += ApplyHazardColors;
        }

        private void OnDisable()
        {
            UiThemeService.ThemeChanged -= ApplyHazardColors;
        }

        private void BuildMarkers()
        {
            if (markerPrefab == null || markerContainer == null)
            {
                Debug.LogError("[MarsOutpost] Landing map is missing its marker prefab or container.");
                return;
            }

            int number = 1;
            foreach (LandingSiteDefinition site in landing.sites)
            {
                if (site == null)
                {
                    continue;
                }

                LandingSiteMarkerView marker = Instantiate(markerPrefab, markerContainer);
                var rect = (RectTransform)marker.transform;
                rect.anchorMin = site.mapPosition;
                rect.anchorMax = site.mapPosition;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(88f, 88f);

                marker.Bind(site, number, OnSiteTapped);
                markers.Add(marker);
                number++;
            }
        }

        private LandingSiteDefinition FindSite(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || landing.sites == null)
            {
                return null;
            }

            foreach (LandingSiteDefinition site in landing.sites)
            {
                if (site != null && site.id == id)
                {
                    return site;
                }
            }

            return null;
        }

        private void OnSiteTapped(LandingSiteDefinition site)
        {
            if (locked)
            {
                return;
            }

            SelectSite(site);
        }

        private void SelectSite(LandingSiteDefinition site)
        {
            selectedSite = site;

            foreach (LandingSiteMarkerView marker in markers)
            {
                marker.SetSelected(marker.Site == site);
            }

            if (site == null)
            {
                ClearSelection();
                return;
            }

            if (detailsHeaderLabel != null) detailsHeaderLabel.text = landing.selectedHeader;
            if (siteNameLabel != null) siteNameLabel.text = site.displayName;
            if (siteRegionLabel != null) siteRegionLabel.text = site.region;
            if (siteDescriptionLabel != null) siteDescriptionLabel.text = site.description;
            if (slopeLabel != null) slopeLabel.text = site.slope != null ? site.slope.label : "-";
            if (dustLabel != null) dustLabel.text = site.dust != null ? site.dust.label : "-";
            if (distanceLabel != null) distanceLabel.text = site.distance != null ? site.distance.label : "-";
            if (confirmButton != null) confirmButton.interactable = !locked;
            if (confirmLabel != null && !locked) confirmLabel.text = landing.confirmLabel;

            ApplyHazardColors();
        }

        private void ClearSelection()
        {
            selectedSite = null;

            foreach (LandingSiteMarkerView marker in markers)
            {
                marker.SetSelected(false);
            }

            if (detailsHeaderLabel != null) detailsHeaderLabel.text = landing.selectedHeader;
            if (siteNameLabel != null) siteNameLabel.text = "Choose a site";
            if (siteRegionLabel != null) siteRegionLabel.text = "";
            if (siteDescriptionLabel != null)
            {
                siteDescriptionLabel.text = string.IsNullOrWhiteSpace(landing.prompt)
                    ? "Tap a marker to inspect a site."
                    : landing.prompt;
            }
            if (slopeLabel != null) slopeLabel.text = "-";
            if (dustLabel != null) dustLabel.text = "-";
            if (distanceLabel != null) distanceLabel.text = "-";
            if (confirmButton != null) confirmButton.interactable = false;
            if (confirmLabel != null) confirmLabel.text = "Select a site first";

            ApplyHazardColors();
        }

        private void ApplyHazardColors()
        {
            UiTheme theme = UiThemeService.ActiveTheme;
            if (selectedSite == null || theme == null)
            {
                SetDot(slopeDot, theme != null ? theme.textSecondary : Color.gray, false);
                SetDot(dustDot, theme != null ? theme.textSecondary : Color.gray, false);
                SetDot(distanceDot, theme != null ? theme.textSecondary : Color.gray, false);
                return;
            }

            SetDot(slopeDot, ToneColor(theme, selectedSite.slope), true);
            SetDot(dustDot, ToneColor(theme, selectedSite.dust), true);
            SetDot(distanceDot, ToneColor(theme, selectedSite.distance), true);
        }

        private static Color ToneColor(UiTheme theme, LandingHazardTag tag)
        {
            int tone = tag != null ? tag.tone : 0;
            if (tone <= 0)
            {
                return theme.success;
            }

            return tone == 1 ? theme.warning : theme.danger;
        }

        private static void SetDot(Image dot, Color color, bool visible)
        {
            if (dot == null)
            {
                return;
            }

            dot.color = color;
            dot.enabled = visible;
        }

        private void ConfirmLanding()
        {
            if (locked || selectedSite == null || Director == null || Director.Session == null)
            {
                return;
            }

            locked = true;
            if (confirmButton != null) confirmButton.interactable = false;
            if (confirmLabel != null) confirmLabel.text = "Landing site logged";

            foreach (LandingSiteMarkerView marker in markers)
            {
                marker.SetInteractable(false);
            }

            ActivityOutcome outcome = Director.CompleteActivity(
                activity.id,
                selectedSite.id,
                $"Landing site: {selectedSite.displayName}",
                selectedSite.resultSummary,
                selectedSite.learningNote,
                selectedSite.changes);

            if (outcome == null)
            {
                locked = false;
                if (confirmButton != null) confirmButton.interactable = true;
                if (confirmLabel != null) confirmLabel.text = landing.confirmLabel;
                Debug.LogError("[MarsOutpost] Landing outcome could not be logged.");
                return;
            }

            resultPanel?.Show(outcome, ContinueToHub);
        }

        private void ContinueToHub()
        {
            if (!Director.TryLoadHub(out string error))
            {
                dialog?.ShowInfo("Mission Hub unavailable", error);
            }
        }

        protected override void HandleBack()
        {
            if (dialog != null && dialog.IsOpen)
            {
                dialog.Hide();
                return;
            }

            if (resultPanel != null && resultPanel.IsOpen)
            {
                ContinueToHub();
                return;
            }

            ContinueToHub();
        }
    }
}
