using MarsOutpost.Core;
using MarsOutpost.Greenhouse;
using MarsOutpost.Landing;
using MarsOutpost.UI;
using MarsOutpost.Weather;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MarsOutpost.EditorTools
{
    /// <summary>Builds the reusable UI prefabs used by every screen.</summary>
    public static class MissionUiBuilder
    {
        public const string PrefabRoot = "Assets/Prefabs";
        public const string UiPrefabRoot = "Assets/Prefabs/UI";

        public sealed class Prefabs
        {
            public ResourcePillView ResourcePill;
            public ActivityCardView ActivityCard;
            public ActivityResultPanel ResultPanel;
            public ConfirmDialog Dialog;
            public LandingSiteMarkerView LandingSiteMarker;
            public GreenhouseTrayView GreenhouseTray;
            public GreenhouseClaimOptionView GreenhouseClaim;
            public SpaceWeatherPlanView WeatherPlan;
        }

        public static Prefabs Build(MissionDataBuilder.Content content)
        {
            GeneratedSpriteBuilder.EnsureFolder(PrefabRoot);
            GeneratedSpriteBuilder.EnsureFolder(UiPrefabRoot);

            return new Prefabs
            {
                ResourcePill = BuildResourcePill(content),
                ActivityCard = BuildActivityCard(content),
                ResultPanel = BuildResultPanel(content),
                Dialog = BuildDialog(content),
                LandingSiteMarker = BuildLandingSiteMarker(content),
                GreenhouseTray = BuildGreenhouseTray(content),
                GreenhouseClaim = BuildGreenhouseClaim(content),
                WeatherPlan = BuildWeatherPlan(content)
            };
        }

        public static Prefabs BuildSpaceWeather(MissionDataBuilder.Content content)
        {
            GeneratedSpriteBuilder.EnsureFolder(PrefabRoot);
            GeneratedSpriteBuilder.EnsureFolder(UiPrefabRoot);

            return new Prefabs
            {
                WeatherPlan = BuildWeatherPlan(content)
            };
        }

        private static SpaceWeatherPlanView BuildWeatherPlan(MissionDataBuilder.Content content)
        {
            UiTheme theme = content.Theme;
            GameObject root = UiBuild.NewUi("WeatherPlan", null);
            UiBuild.Rect(root).sizeDelta = new Vector2(838f, 120f);

            Image background = UiBuild.AddImage(root, theme.panelAlt, theme.roundedPanel, true, true);
            var button = root.AddComponent<Button>();
            button.targetGraphic = background;
            button.navigation = new Navigation { mode = Navigation.Mode.Automatic };
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.04f, 1.04f, 1.04f, 1f);
            colors.pressedColor = new Color(0.84f, 0.88f, 0.95f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = Color.white;
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            GameObject badgeGo = UiBuild.NewUi("Badge", root.transform);
            UiBuild.TopLeft(UiBuild.Rect(badgeGo), 16f, 33f, 54f, 54f);
            Image badge = UiBuild.AddImage(badgeGo, theme.primary, theme.circle);

            GameObject indexGo = UiBuild.NewUi("Index", badgeGo.transform);
            UiBuild.Center(UiBuild.Rect(indexGo), 0f, 0f, 54f, 54f);
            TextMeshProUGUI indexLabel = UiBuild.AddText(
                indexGo, "1", TextRole.Body, TextAlignmentOptions.Center, theme);
            DisableTheme(indexGo);

            GameObject titleGo = UiBuild.NewUi("Title", root.transform);
            UiBuild.TopLeft(UiBuild.Rect(titleGo), 86f, 10f, 610f, 38f);
            TextMeshProUGUI titleLabel = UiBuild.AddText(
                titleGo, "Crew plan", TextRole.Body, TextAlignmentOptions.Left, theme);
            DisableTheme(titleGo);

            GameObject descriptionGo = UiBuild.NewUi("Description", root.transform);
            UiBuild.TopLeft(UiBuild.Rect(descriptionGo), 86f, 50f, 632f, 58f);
            TextMeshProUGUI descriptionLabel = UiBuild.AddText(
                descriptionGo, "Description", TextRole.Caption, TextAlignmentOptions.TopLeft, theme, secondary: true);
            DisableTheme(descriptionGo);

            GameObject stateGo = UiBuild.NewUi("State", root.transform);
            UiBuild.TopRight(UiBuild.Rect(stateGo), 26f, 12f, 108f, 24f);
            TextMeshProUGUI stateLabel = UiBuild.AddText(
                stateGo, "TAP", TextRole.Caption, TextAlignmentOptions.Right, theme, secondary: true);
            DisableTheme(stateGo);

            var view = root.AddComponent<SpaceWeatherPlanView>();
            view.button = button;
            view.background = background;
            view.badge = badge;
            view.indexLabel = indexLabel;
            view.titleLabel = titleLabel;
            view.descriptionLabel = descriptionLabel;
            view.stateLabel = stateLabel;

            var layout = root.AddComponent<LayoutElement>();
            layout.minHeight = 120f;
            layout.preferredHeight = 120f;
            layout.flexibleWidth = 1f;
            layout.flexibleHeight = 0f;

            return Save<SpaceWeatherPlanView>(root, $"{UiPrefabRoot}/WeatherPlan.prefab");
        }

        private static void DisableTheme(GameObject go)
        {
            ThemedText themed = go.GetComponent<ThemedText>();
            if (themed != null)
            {
                themed.enabled = false;
            }
        }

        private static ResourcePillView BuildResourcePill(MissionDataBuilder.Content content)
        {
            UiTheme theme = content.Theme;
            GameObject root = UiBuild.NewUi("ResourcePill", null);
            var rootRect = UiBuild.Rect(root);
            rootRect.sizeDelta = new Vector2(220f, 80f);

            Image background = UiBuild.AddPanel(root, PanelRole.PanelAlt, true, theme);

            GameObject iconGo = UiBuild.NewUi("Icon", root.transform);
            UiBuild.TopLeft(UiBuild.Rect(iconGo), 14f, 14f, 52f, 52f);
            Image icon = UiBuild.AddImage(iconGo, Color.white, theme.circle);

            GameObject shortGo = UiBuild.NewUi("ShortLabel", root.transform);
            UiBuild.TopLeft(UiBuild.Rect(shortGo), 78f, 10f, 130f, 26f);
            TextMeshProUGUI shortLabel = UiBuild.AddText(
                shortGo, "PWR", TextRole.Caption, TextAlignmentOptions.Left, theme, secondary: true);

            GameObject valueGo = UiBuild.NewUi("Value", root.transform);
            UiBuild.TopLeft(UiBuild.Rect(valueGo), 78f, 36f, 130f, 38f);
            TextMeshProUGUI valueLabel = UiBuild.AddText(
                valueGo, "100", TextRole.Heading, TextAlignmentOptions.Left, theme);

            var view = root.AddComponent<ResourcePillView>();
            view.background = background;
            view.icon = icon;
            view.shortLabel = shortLabel;
            view.valueLabel = valueLabel;

            var layout = root.AddComponent<LayoutElement>();
            layout.preferredWidth = 220f;
            layout.minWidth = 190f;
            layout.preferredHeight = 80f;
            layout.flexibleWidth = 0f;

            return Save<ResourcePillView>(root, $"{UiPrefabRoot}/ResourcePill.prefab");
        }

        private static ActivityCardView BuildActivityCard(MissionDataBuilder.Content content)
        {
            UiTheme theme = content.Theme;
            GameObject root = UiBuild.NewUi("ActivityCard", null);
            var rootRect = UiBuild.Rect(root);
            rootRect.sizeDelta = new Vector2(1600f, 190f);

            Image background = UiBuild.AddPanel(root, PanelRole.PanelAlt, true, theme);
            background.raycastTarget = true;

            var button = root.AddComponent<Button>();
            button.targetGraphic = background;
            button.navigation = new Navigation { mode = Navigation.Mode.Automatic };
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.82f, 0.86f, 0.94f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.6f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            GameObject iconGo = UiBuild.NewUi("Icon", root.transform);
            UiBuild.TopLeft(UiBuild.Rect(iconGo), 26f, 51f, 88f, 88f);
            Image icon = UiBuild.AddImage(iconGo, theme.primary, theme.circle);

            GameObject indexGo = UiBuild.NewUi("Index", iconGo.transform);
            UiBuild.Center(UiBuild.Rect(indexGo), 0f, 0f, 80f, 80f);
            TextMeshProUGUI indexLabel = UiBuild.AddText(
                indexGo, "01", TextRole.Heading, TextAlignmentOptions.Center, theme);
            var indexTheme = indexGo.GetComponent<ThemedText>();
            indexTheme.overrideColor = true;
            indexTheme.colorOverride = theme.primaryText;

            GameObject titleGo = UiBuild.NewUi("Title", root.transform);
            UiBuild.TopLeft(UiBuild.Rect(titleGo), 142f, 26f, 900f, 56f);
            TextMeshProUGUI titleLabel = UiBuild.AddText(
                titleGo, "Landing Site", TextRole.Heading, TextAlignmentOptions.Left, theme);

            GameObject taglineGo = UiBuild.NewUi("Tagline", root.transform);
            UiBuild.TopLeft(UiBuild.Rect(taglineGo), 142f, 86f, 1000f, 42f);
            TextMeshProUGUI taglineLabel = UiBuild.AddText(
                taglineGo, "Choose where the crew touches down.", TextRole.Body, TextAlignmentOptions.Left, theme, secondary: true);

            GameObject statusGo = UiBuild.NewUi("Status", root.transform);
            UiBuild.TopRight(UiBuild.Rect(statusGo), 38f, 28f, 220f, 34f);
            TextMeshProUGUI statusLabel = UiBuild.AddText(
                statusGo, "2 MIN", TextRole.Caption, TextAlignmentOptions.Right, theme, secondary: true);

            GameObject arrowGo = UiBuild.NewUi("Arrow", root.transform);
            UiBuild.TopRight(UiBuild.Rect(arrowGo), 38f, 78f, 60f, 60f);
            TextMeshProUGUI arrowLabel = UiBuild.AddText(
                arrowGo, ">", TextRole.Heading, TextAlignmentOptions.Right, theme);

            var view = root.AddComponent<ActivityCardView>();
            view.button = button;
            view.iconBackground = icon;
            view.indexLabel = indexLabel;
            view.titleLabel = titleLabel;
            view.taglineLabel = taglineLabel;
            view.statusLabel = statusLabel;
            view.arrowLabel = arrowLabel;

            var layout = root.AddComponent<LayoutElement>();
            layout.minHeight = 170f;
            layout.preferredHeight = 190f;
            layout.flexibleWidth = 1f;
            layout.flexibleHeight = 0f;

            return Save<ActivityCardView>(root, $"{UiPrefabRoot}/ActivityCard.prefab");
        }

        private static ActivityResultPanel BuildResultPanel(MissionDataBuilder.Content content)
        {
            UiTheme theme = content.Theme;
            GameObject root = UiBuild.NewUi("ActivityResultPanel", null);
            UiBuild.Stretch(UiBuild.Rect(root));
            UiBuild.AddImage(root, new Color(0f, 0f, 0f, 0.74f), null, false, true);

            GameObject panel = UiBuild.NewUi("Panel", root.transform);
            UiBuild.Center(UiBuild.Rect(panel), 0f, 0f, 1300f, 820f);
            UiBuild.AddPanel(panel, PanelRole.Panel, true, theme);

            GameObject titleGo = UiBuild.NewUi("Title", panel.transform);
            UiBuild.TopLeft(UiBuild.Rect(titleGo), 44f, 36f, 1212f, 90f);
            TextMeshProUGUI titleLabel = UiBuild.AddText(
                titleGo, "Activity complete", TextRole.Heading, TextAlignmentOptions.Left, theme);

            GameObject summaryGo = UiBuild.NewUi("Summary", panel.transform);
            UiBuild.TopLeft(UiBuild.Rect(summaryGo), 44f, 136f, 1212f, 108f);
            TextMeshProUGUI summaryLabel = UiBuild.AddText(
                summaryGo, "Summary", TextRole.Body, TextAlignmentOptions.TopLeft, theme, secondary: true);

            GameObject dividerGo = UiBuild.NewUi("Divider", panel.transform);
            UiBuild.TopLeft(UiBuild.Rect(dividerGo), 44f, 258f, 1212f, 2f);
            UiBuild.AddImage(dividerGo, theme.panelAlt, null, false, false);

            GameObject changesGo = UiBuild.NewUi("Changes", panel.transform);
            UiBuild.TopLeft(UiBuild.Rect(changesGo), 44f, 280f, 1212f, 282f);
            TextMeshProUGUI changesLabel = UiBuild.AddText(
                changesGo, "Resource changes", TextRole.Body, TextAlignmentOptions.TopLeft, theme);

            GameObject learningGo = UiBuild.NewUi("Learning", panel.transform);
            UiBuild.TopLeft(UiBuild.Rect(learningGo), 44f, 574f, 1212f, 104f);
            TextMeshProUGUI learningLabel = UiBuild.AddText(
                learningGo, "Learning note", TextRole.Caption, TextAlignmentOptions.TopLeft, theme, secondary: true, italic: true);

            GameObject buttonGo = UiBuild.NewUi("Continue", panel.transform);
            UiBuild.BottomRight(UiBuild.Rect(buttonGo), 44f, 32f, 420f, 96f);
            Button continueButton = UiBuild.AddButton(buttonGo, "Back to Mission Hub", true, theme, out TextMeshProUGUI continueLabel);

            var view = root.AddComponent<ActivityResultPanel>();
            view.titleLabel = titleLabel;
            view.summaryLabel = summaryLabel;
            view.changesLabel = changesLabel;
            view.learningLabel = learningLabel;
            view.continueButton = continueButton;
            view.continueLabel = continueLabel;

            root.SetActive(false);
            return Save<ActivityResultPanel>(root, $"{UiPrefabRoot}/ActivityResultPanel.prefab");
        }

        private static ConfirmDialog BuildDialog(MissionDataBuilder.Content content)
        {
            UiTheme theme = content.Theme;
            GameObject root = UiBuild.NewUi("ConfirmDialog", null);
            UiBuild.Stretch(UiBuild.Rect(root));
            UiBuild.AddImage(root, new Color(0f, 0f, 0f, 0.78f), null, false, true);

            GameObject panel = UiBuild.NewUi("Panel", root.transform);
            UiBuild.Center(UiBuild.Rect(panel), 0f, 0f, 1100f, 680f);
            UiBuild.AddPanel(panel, PanelRole.Panel, true, theme);

            GameObject titleGo = UiBuild.NewUi("Title", panel.transform);
            UiBuild.TopLeft(UiBuild.Rect(titleGo), 44f, 40f, 1012f, 76f);
            TextMeshProUGUI titleLabel = UiBuild.AddText(
                titleGo, "Are you sure?", TextRole.Heading, TextAlignmentOptions.Left, theme);

            GameObject bodyGo = UiBuild.NewUi("Body", panel.transform);
            UiBuild.TopLeft(UiBuild.Rect(bodyGo), 44f, 130f, 1012f, 340f);
            TextMeshProUGUI bodyLabel = UiBuild.AddText(
                bodyGo, "Body", TextRole.Body, TextAlignmentOptions.TopLeft, theme, secondary: true);

            GameObject rowGo = UiBuild.NewUi("ButtonRow", panel.transform);
            UiBuild.BottomLeft(UiBuild.Rect(rowGo), 44f, 40f, 1012f, 104f);
            var row = rowGo.AddComponent<HorizontalLayoutGroup>();
            row.spacing = 20f;
            row.padding = new RectOffset(0, 0, 0, 0);
            row.childControlWidth = true;
            row.childControlHeight = true;
            row.childForceExpandWidth = true;
            row.childForceExpandHeight = true;
            row.childAlignment = TextAnchor.MiddleCenter;

            GameObject cancelGo = UiBuild.NewUi("Cancel", rowGo.transform);
            Button cancelButton = UiBuild.AddButton(cancelGo, "Cancel", false, theme, out TextMeshProUGUI cancelLabel);
            UiBuild.Layout(cancelGo, 92f, 100f);

            GameObject confirmGo = UiBuild.NewUi("Confirm", rowGo.transform);
            Button confirmButton = UiBuild.AddButton(confirmGo, "OK", true, theme, out TextMeshProUGUI confirmLabel);
            UiBuild.Layout(confirmGo, 92f, 100f);

            var dialog = root.AddComponent<ConfirmDialog>();
            dialog.titleLabel = titleLabel;
            dialog.bodyLabel = bodyLabel;
            dialog.confirmButton = confirmButton;
            dialog.cancelButton = cancelButton;
            dialog.confirmLabel = confirmLabel;
            dialog.cancelLabel = cancelLabel;

            root.SetActive(false);
            return Save<ConfirmDialog>(root, $"{UiPrefabRoot}/ConfirmDialog.prefab");
        }

        private static LandingSiteMarkerView BuildLandingSiteMarker(MissionDataBuilder.Content content)
        {
            UiTheme theme = content.Theme;
            GameObject root = UiBuild.NewUi("LandingSiteMarker", null);
            UiBuild.Center(UiBuild.Rect(root), 0f, 0f, 88f, 88f);

            GameObject ringGo = UiBuild.NewUi("Ring", root.transform);
            UiBuild.Center(UiBuild.Rect(ringGo), 0f, 0f, 116f, 116f);
            Image ring = UiBuild.AddImage(ringGo, theme.textPrimary, theme.circle);
            ring.raycastTarget = false;
            ring.enabled = false;

            Image background = UiBuild.AddImage(root, theme.primary, theme.circle, false, true);
            var button = root.AddComponent<Button>();
            button.targetGraphic = background;
            button.navigation = new Navigation { mode = Navigation.Mode.Automatic };
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
            colors.pressedColor = new Color(0.8f, 0.84f, 0.9f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.6f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            GameObject numberGo = UiBuild.NewUi("Number", root.transform);
            UiBuild.Center(UiBuild.Rect(numberGo), 0f, 2f, 74f, 54f);
            TextMeshProUGUI numberLabel = UiBuild.AddText(
                numberGo, "01", TextRole.Heading, TextAlignmentOptions.Center, theme);
            numberLabel.fontSize = 34f;
            numberLabel.fontStyle = FontStyles.Bold;

            GameObject nameGo = UiBuild.NewUi("Name", root.transform);
            var nameRect = UiBuild.Rect(nameGo);
            nameRect.anchorMin = new Vector2(0.5f, 0f);
            nameRect.anchorMax = new Vector2(0.5f, 0f);
            nameRect.pivot = new Vector2(0.5f, 1f);
            nameRect.anchoredPosition = new Vector2(0f, -8f);
            nameRect.sizeDelta = new Vector2(250f, 44f);
            TextMeshProUGUI nameLabel = UiBuild.AddText(
                nameGo, "Site", TextRole.Caption, TextAlignmentOptions.Center, theme);

            var view = root.AddComponent<LandingSiteMarkerView>();
            view.button = button;
            view.ring = ring;
            view.background = background;
            view.numberLabel = numberLabel;
            view.nameLabel = nameLabel;

            return Save<LandingSiteMarkerView>(root, $"{UiPrefabRoot}/LandingSiteMarker.prefab");
        }

        private static GreenhouseTrayView BuildGreenhouseTray(MissionDataBuilder.Content content)
        {
            UiTheme theme = content.Theme;
            GameObject root = UiBuild.NewUi("GreenhouseTray", null);
            UiBuild.Rect(root).sizeDelta = new Vector2(420f, 550f);
            UiBuild.AddPanel(root, PanelRole.PanelAlt, true, theme);

            TextMeshProUGUI title = UiBuild.Text(
                root.transform, "Title", "TRAY A", TextRole.Caption, TextAlignmentOptions.Left, theme, secondary: true);
            title.fontStyle = FontStyles.Bold;
            UiBuild.TopLeft(UiBuild.Rect(title.gameObject), 20f, 16f, 380f, 30f);

            TextMeshProUGUI treatment = UiBuild.Text(
                root.transform, "Treatment", "Choose a treatment", TextRole.Body, TextAlignmentOptions.Left, theme, secondary: true);
            treatment.fontStyle = FontStyles.Bold;
            UiBuild.TopLeft(UiBuild.Rect(treatment.gameObject), 20f, 50f, 380f, 42f);

            GameObject glowGo = UiBuild.NewUi("LampGlow", root.transform);
            UiBuild.TopLeft(UiBuild.Rect(glowGo), 150f, 70f, 120f, 120f);
            Image lampGlow = UiBuild.AddImage(glowGo, new Color(1f, 0.3f, 0.25f, 0.42f), theme.circle);

            GameObject coreGo = UiBuild.NewUi("LampCore", root.transform);
            UiBuild.TopLeft(UiBuild.Rect(coreGo), 180f, 100f, 60f, 60f);
            Image lampCore = UiBuild.AddImage(coreGo, theme.danger, theme.circle);

            GameObject plantGo = UiBuild.NewUi("Plant", root.transform);
            UiBuild.TopLeft(UiBuild.Rect(plantGo), 95f, 108f, 240f, 240f);
            Image plant = UiBuild.AddImage(plantGo, Color.white, content.Sprites.GreenhouseTray);

            GameObject observationGo = UiBuild.NewUi("Observation", root.transform);
            UiBuild.Stretch(UiBuild.Rect(observationGo));
            observationGo.SetActive(false);

            TextMeshProUGUI growthLabel = UiBuild.Text(
                observationGo.transform, "GrowthLabel", "CELL GROWTH",
                TextRole.Caption, TextAlignmentOptions.Left, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(growthLabel.gameObject), 22f, 342f, 376f, 24f);

            GameObject growthDotGo = UiBuild.NewUi("GrowthDot", observationGo.transform);
            UiBuild.TopLeft(UiBuild.Rect(growthDotGo), 22f, 372f, 16f, 16f);
            Image growthDot = UiBuild.AddImage(growthDotGo, theme.success, theme.circle);

            TextMeshProUGUI growthValue = UiBuild.Text(
                observationGo.transform, "GrowthValue", "Recovery signal",
                TextRole.Body, TextAlignmentOptions.Left, theme);
            UiBuild.TopLeft(UiBuild.Rect(growthValue.gameObject), 48f, 360f, 350f, 38f);

            TextMeshProUGUI stressLabel = UiBuild.Text(
                observationGo.transform, "StressLabel", "STRESS RESPONSE",
                TextRole.Caption, TextAlignmentOptions.Left, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(stressLabel.gameObject), 22f, 398f, 376f, 24f);

            GameObject stressDotGo = UiBuild.NewUi("StressDot", observationGo.transform);
            UiBuild.TopLeft(UiBuild.Rect(stressDotGo), 22f, 428f, 16f, 16f);
            Image stressDot = UiBuild.AddImage(stressDotGo, theme.warning, theme.circle);

            TextMeshProUGUI stressValue = UiBuild.Text(
                observationGo.transform, "StressValue", "Active",
                TextRole.Body, TextAlignmentOptions.Left, theme);
            UiBuild.TopLeft(UiBuild.Rect(stressValue.gameObject), 48f, 416f, 350f, 38f);

            Button red = UiBuild.Button(root.transform, "Red", "Red", false, theme, out TextMeshProUGUI redLabel);
            UiBuild.BottomLeft(UiBuild.Rect(red.gameObject), 20f, 30f, 184f, 64f);
            red.GetComponent<ThemedButton>().panelBackground = true;

            Button dark = UiBuild.Button(root.transform, "Dark", "Dark", false, theme, out TextMeshProUGUI darkLabel);
            UiBuild.BottomLeft(UiBuild.Rect(dark.gameObject), 216f, 30f, 184f, 64f);
            dark.GetComponent<ThemedButton>().panelBackground = true;

            var view = root.AddComponent<GreenhouseTrayView>();
            view.titleLabel = title;
            view.treatmentLabel = treatment;
            view.lampGlow = lampGlow;
            view.lampCore = lampCore;
            view.plantImage = plant;
            view.redButton = red;
            view.darkButton = dark;
            view.redButtonLabel = redLabel;
            view.darkButtonLabel = darkLabel;
            view.observationGroup = observationGo;
            view.growthLabel = growthLabel;
            view.growthValue = growthValue;
            view.growthDot = growthDot;
            view.stressLabel = stressLabel;
            view.stressValue = stressValue;
            view.stressDot = stressDot;

            return Save<GreenhouseTrayView>(root, $"{UiPrefabRoot}/GreenhouseTray.prefab");
        }

        private static GreenhouseClaimOptionView BuildGreenhouseClaim(MissionDataBuilder.Content content)
        {
            UiTheme theme = content.Theme;
            GameObject root = UiBuild.NewUi("GreenhouseClaim", null);
            UiBuild.Rect(root).sizeDelta = new Vector2(790f, 62f);
            Image background = UiBuild.AddImage(root, theme.panelAlt, theme.roundedPanel, true, true);

            var button = root.AddComponent<Button>();
            button.targetGraphic = background;
            button.navigation = new Navigation { mode = Navigation.Mode.Automatic };
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.06f, 1.06f, 1.06f, 1f);
            colors.pressedColor = new Color(0.88f, 0.92f, 1f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = Color.white;
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            GameObject radioGo = UiBuild.NewUi("Radio", root.transform);
            UiBuild.TopLeft(UiBuild.Rect(radioGo), 24f, 17f, 28f, 28f);
            Image radioOuter = UiBuild.AddImage(radioGo, theme.textSecondary, theme.circle);
            radioOuter.raycastTarget = false;

            GameObject dotGo = UiBuild.NewUi("Dot", radioGo.transform);
            UiBuild.Center(UiBuild.Rect(dotGo), 0f, 0f, 12f, 12f);
            Image radioDot = UiBuild.AddImage(dotGo, theme.primaryText, theme.circle);
            radioDot.raycastTarget = false;

            TextMeshProUGUI statement = UiBuild.Text(
                root.transform, "Statement", "Claim", TextRole.Body, TextAlignmentOptions.TopLeft, theme);
            statement.enableAutoSizing = false;
            statement.lineSpacing = -4f;
            UiBuild.TopLeft(UiBuild.Rect(statement.gameObject), 70f, 3f, 690f, 56f);

            var layout = root.AddComponent<LayoutElement>();
            layout.preferredHeight = 62f;
            layout.minHeight = 62f;
            layout.flexibleHeight = 0f;
            layout.flexibleWidth = 1f;

            var view = root.AddComponent<GreenhouseClaimOptionView>();
            view.button = button;
            view.background = background;
            view.radioOuter = radioOuter;
            view.radioDot = radioDot;
            view.statement = statement;

            return Save<GreenhouseClaimOptionView>(root, $"{UiPrefabRoot}/GreenhouseClaim.prefab");
        }

        private static T Save<T>(GameObject root, string path) where T : Component
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            if (prefab == null)
            {
                Debug.LogError($"[MarsOutpost] Could not save prefab at {path}.");
                return null;
            }

            return prefab.GetComponent<T>();
        }
    }
}
