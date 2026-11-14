using MarsOutpost.Core;
using MarsOutpost.Greenhouse;
using MarsOutpost.Landing;
using MarsOutpost.UI;
using MarsOutpost.Weather;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MarsOutpost.EditorTools
{
    /// <summary>Builds the six Phase 1 scenes from code so layout is reproducible.</summary>
    public static class MissionSceneBuilder
    {
        public const string SceneRoot = "Assets/Scenes";
        public const string MainMenuPath = SceneRoot + "/00_MainMenu.unity";
        public const string HubPath = SceneRoot + "/01_MissionHub.unity";
        public const string LandingPath = SceneRoot + "/02_LandingSite.unity";
        public const string GreenhousePath = SceneRoot + "/03_Greenhouse.unity";
        public const string WeatherPath = SceneRoot + "/04_SpaceWeather.unity";
        public const string DebriefPath = SceneRoot + "/05_Debrief.unity";

        public static void BuildAll(MissionDataBuilder.Content content, MissionUiBuilder.Prefabs prefabs)
        {
            GeneratedSpriteBuilder.EnsureFolder(SceneRoot);

            BuildMainMenu(content);
            BuildHub(content, prefabs);
            BuildLanding(content, prefabs);
            BuildGreenhouse(content, prefabs);
            BuildWeather(content, prefabs);
            BuildDebrief(content, prefabs);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MainMenuPath, true),
                new EditorBuildSettingsScene(HubPath, true),
                new EditorBuildSettingsScene(LandingPath, true),
                new EditorBuildSettingsScene(GreenhousePath, true),
                new EditorBuildSettingsScene(WeatherPath, true),
                new EditorBuildSettingsScene(DebriefPath, true),
                new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", false)
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private sealed class SceneContext
        {
            public Scene Scene;
            public Canvas Canvas;
            public Transform SafeArea;
        }

        private static SceneContext CreateScreen(MissionDataBuilder.Content content, string sceneName)
        {
            UiTheme theme = content.Theme;
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener))
            {
                tag = "MainCamera"
            };
            cameraGo.transform.position = new Vector3(0f, 0f, -10f);
            var camera = cameraGo.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = theme.background;
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.cullingMask = 0;

            var eventGo = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            var inputModule = eventGo.GetComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();

            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(UiBuild.RefWidth, UiBuild.RefHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 100f;

            GameObject background = UiBuild.NewUi("Background", canvasGo.transform);
            UiBuild.Stretch(UiBuild.Rect(background));
            UiBuild.AddPanel(background, PanelRole.Background, false, theme);

            GameObject safeArea = UiBuild.NewUi("SafeArea", canvasGo.transform);
            UiBuild.Stretch(UiBuild.Rect(safeArea));
            safeArea.AddComponent<SafeAreaFitter>();

            var bootstrapGo = new GameObject("MissionBootstrap");
            var bootstrap = bootstrapGo.AddComponent<MissionSceneBootstrap>();
            bootstrap.AssignConfig(content.Config);
            EditorUtility.SetDirty(bootstrap);

            return new SceneContext
            {
                Scene = scene,
                Canvas = canvas,
                SafeArea = safeArea.transform
            };
        }

        private static void Save(SceneContext context, string path)
        {
            EditorSceneManager.MarkSceneDirty(context.Scene);
            EditorSceneManager.SaveScene(context.Scene, path);
        }

        private static T AddController<T>(SceneContext context, string name) where T : Component
        {
            GameObject go = UiBuild.FullScreen(context.Canvas.transform, name);
            go.transform.SetAsLastSibling();
            return go.AddComponent<T>();
        }

        private static T InstantiatePrefab<T>(string prefabName, Transform parent) where T : Component
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(MissionUiBuilder.UiPrefabRoot + "/" + prefabName);
            if (asset == null)
            {
                throw new System.InvalidOperationException("Missing UI prefab: " + prefabName);
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
            T component = instance.GetComponent<T>();
            if (component == null)
            {
                throw new System.InvalidOperationException("Prefab " + prefabName + " does not have " + typeof(T).Name + ".");
            }

            instance.SetActive(false);
            return component;
        }

        private static GameObject TopBar(Transform parent, string name, float height, UiTheme theme)
        {
            GameObject go = UiBuild.NewUi(name, parent);
            var rect = UiBuild.Rect(go);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, height);
            UiBuild.AddPanel(go, PanelRole.Panel, false, theme);
            return go;
        }

        private static GameObject BottomBar(Transform parent, string name, float height, UiTheme theme)
        {
            GameObject go = UiBuild.NewUi(name, parent);
            var rect = UiBuild.Rect(go);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, height);
            UiBuild.AddPanel(go, PanelRole.Panel, false, theme);
            return go;
        }

        private static void BuildMainMenu(MissionDataBuilder.Content content)
        {
            MissionConfig config = content.Config;
            UiTheme theme = content.Theme;
            SceneContext context = CreateScreen(content, "00_MainMenu");
            Transform safe = context.SafeArea;

            GameObject glow = UiBuild.NewUi("MarsGlow", safe);
            UiBuild.Center(UiBuild.Rect(glow), 470f, 40f, 1040f, 1040f);
            UiBuild.AddImage(glow, new Color(theme.primary.r, theme.primary.g, theme.primary.b, 0.08f), content.Sprites.Circle);

            GameObject hero = UiBuild.NewUi("Mars", safe);
            UiBuild.Center(UiBuild.Rect(hero), 520f, 40f, 820f, 820f);
            UiBuild.AddImage(hero, Color.white, content.Sprites.Mars);

            Transform root = UiBuild.FullScreen(safe, "MenuContent").transform;

            TextMeshProUGUI missionCode = UiBuild.Text(
                root, "MissionCode", config.missionCode, TextRole.Caption, TextAlignmentOptions.Left, theme);
            UiBuild.TopLeft(UiBuild.Rect(missionCode.gameObject), 64f, 34f, 600f, 32f);
            missionCode.text = config.missionCode;

            TextMeshProUGUI title = UiBuild.Text(
                root, "Title", config.gameTitle, TextRole.Title, TextAlignmentOptions.Left, theme);
            UiBuild.TopLeft(UiBuild.Rect(title.gameObject), 60f, 76f, 980f, 140f);

            TextMeshProUGUI subtitle = UiBuild.Text(
                root, "Subtitle", config.subtitle, TextRole.Heading, TextAlignmentOptions.Left, theme);
            UiBuild.TopLeft(UiBuild.Rect(subtitle.gameObject), 64f, 218f, 900f, 64f);
            var subtitleTheme = subtitle.GetComponent<ThemedText>();
            subtitleTheme.overrideColor = true;
            subtitleTheme.colorOverride = theme.primary;

            TextMeshProUGUI intro = UiBuild.Text(
                root, "Intro", config.introBody, TextRole.Body, TextAlignmentOptions.TopLeft, theme);
            UiBuild.TopLeft(UiBuild.Rect(intro.gameObject), 64f, 300f, 830f, 240f);

            Button start = UiBuild.Button(root, "Start", "Start Mission", true, theme, out TextMeshProUGUI startLabel);
            UiBuild.TopLeft(UiBuild.Rect(start.gameObject), 64f, 570f, 440f, 108f);

            Button howTo = UiBuild.Button(root, "HowTo", "How to Play", false, theme, out TextMeshProUGUI howLabel);
            UiBuild.TopLeft(UiBuild.Rect(howTo.gameObject), 528f, 570f, 300f, 108f);

            Button sources = UiBuild.Button(root, "Sources", "Sources", false, theme, out TextMeshProUGUI sourcesLabel);
            UiBuild.BottomLeft(UiBuild.Rect(sources.gameObject), 64f, 92f, 230f, 76f);

            Button textSize = UiBuild.Button(root, "TextSize", "Text 100%", false, theme, out TextMeshProUGUI textSizeLabel);
            UiBuild.BottomLeft(UiBuild.Rect(textSize.gameObject), 310f, 92f, 220f, 76f);

            Button contrast = UiBuild.Button(root, "Contrast", "High contrast: OFF", false, theme, out TextMeshProUGUI contrastLabel);
            UiBuild.BottomLeft(UiBuild.Rect(contrast.gameObject), 546f, 92f, 340f, 76f);

            Button quit = UiBuild.Button(root, "Quit", "Quit", false, theme, out TextMeshProUGUI quitLabel);
            UiBuild.BottomLeft(UiBuild.Rect(quit.gameObject), 902f, 92f, 170f, 76f);

            TextMeshProUGUI footer = UiBuild.Text(
                root, "Footer", config.simulationDisclaimer, TextRole.Caption, TextAlignmentOptions.Left, theme, secondary: true);
            UiBuild.BottomLeft(UiBuild.Rect(footer.gameObject), 64f, 30f, 1500f, 50f);

            var controller = AddController<MainMenuController>(context, "MainMenuScreen");
            controller.titleLabel = title;
            controller.subtitleLabel = subtitle;
            controller.missionCodeLabel = missionCode;
            controller.introLabel = intro;
            controller.footerLabel = footer;
            controller.startButton = start;
            controller.howToPlayButton = howTo;
            controller.sourcesButton = sources;
            controller.quitButton = quit;
            controller.textSizeButton = textSize;
            controller.contrastButton = contrast;
            controller.textSizeLabel = textSizeLabel;
            controller.contrastLabel = contrastLabel;

            ConfirmDialog dialog = InstantiatePrefab<ConfirmDialog>("ConfirmDialog.prefab", context.Canvas.transform);
            controller.dialog = dialog;

            Save(context, MainMenuPath);
        }

        private static void BuildHub(MissionDataBuilder.Content content, MissionUiBuilder.Prefabs prefabs)
        {
            MissionConfig config = content.Config;
            UiTheme theme = content.Theme;
            SceneContext context = CreateScreen(content, "01_MissionHub");
            Transform safe = context.SafeArea;

            GameObject top = TopBar(safe, "TopBar", 200f, theme);
            TextMeshProUGUI missionCode = UiBuild.Text(
                top.transform, "MissionCode", config.missionCode, TextRole.Caption, TextAlignmentOptions.Left, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(missionCode.gameObject), 40f, 24f, 520f, 30f);

            TextMeshProUGUI progress = UiBuild.Text(
                top.transform, "Progress", "0 of 3 training steps complete", TextRole.Body, TextAlignmentOptions.Left, theme);
            UiBuild.TopLeft(UiBuild.Rect(progress.gameObject), 40f, 62f, 760f, 44f);

            GameObject resourceRow = UiBuild.NewUi("Resources", top.transform);
            UiBuild.TopRight(UiBuild.Rect(resourceRow), 40f, 22f, 760f, 84f);
            var resourceLayout = resourceRow.AddComponent<HorizontalLayoutGroup>();
            resourceLayout.spacing = 12f;
            resourceLayout.padding = new RectOffset(0, 0, 0, 0);
            resourceLayout.childControlWidth = true;
            resourceLayout.childControlHeight = true;
            resourceLayout.childForceExpandWidth = false;
            resourceLayout.childForceExpandHeight = false;
            resourceLayout.childAlignment = TextAnchor.MiddleRight;

            ScrollRect scroll = UiBuild.ScrollView(safe, "ActivityScroll", out RectTransform scrollContent);
            UiBuild.Stretch(UiBuild.Rect(scroll.gameObject), 40f, 220f, 40f, 220f);

            GameObject bottom = BottomBar(safe, "BottomBar", 210f, theme);
            TextMeshProUGUI lastResult = UiBuild.Text(
                bottom.transform, "LastResult", "Your crew is ready. Choose a training step.",
                TextRole.Body, TextAlignmentOptions.TopLeft, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(lastResult.gameObject), 40f, 18f, 980f, 54f);

            Button restart = UiBuild.Button(bottom.transform, "Restart", "Restart", false, theme, out TextMeshProUGUI restartLabel);
            UiBuild.BottomLeft(UiBuild.Rect(restart.gameObject), 40f, 20f, 190f, 76f);

            Button textSize = UiBuild.Button(bottom.transform, "TextSize", "Text 100%", false, theme, out TextMeshProUGUI textSizeLabel);
            UiBuild.BottomLeft(UiBuild.Rect(textSize.gameObject), 246f, 20f, 190f, 76f);

            Button contrast = UiBuild.Button(bottom.transform, "Contrast", "High contrast: OFF", false, theme, out TextMeshProUGUI contrastLabel);
            UiBuild.BottomLeft(UiBuild.Rect(contrast.gameObject), 452f, 20f, 340f, 76f);

            Button sendReport = UiBuild.Button(bottom.transform, "SendReport", "Send Mission Report", true, theme, out TextMeshProUGUI sendReportLabel);
            UiBuild.BottomRight(UiBuild.Rect(sendReport.gameObject), 40f, 20f, 430f, 92f);

            GameObject briefing = UiBuild.FullScreen(context.Canvas.transform, "Briefing");
            UiBuild.AddImage(briefing, new Color(0f, 0f, 0f, 0.8f), null, false, true);

            GameObject briefingPanel = UiBuild.NewUi("Panel", briefing.transform);
            UiBuild.Center(UiBuild.Rect(briefingPanel), 0f, 0f, 1280f, 760f);
            UiBuild.AddPanel(briefingPanel, PanelRole.Panel, true, theme);

            TextMeshProUGUI briefingTitle = UiBuild.Text(
                briefingPanel.transform, "Title", config.introHeadline, TextRole.Heading, TextAlignmentOptions.Left, theme);
            UiBuild.TopLeft(UiBuild.Rect(briefingTitle.gameObject), 50f, 40f, 1180f, 82f);

            TextMeshProUGUI briefingBody = UiBuild.Text(
                briefingPanel.transform, "Body", config.introBody, TextRole.Body, TextAlignmentOptions.TopLeft, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(briefingBody.gameObject), 50f, 138f, 1180f, 470f);

            Button briefingStart = UiBuild.Button(briefingPanel.transform, "StartTraining", "Start Training", true, theme, out TextMeshProUGUI briefingStartLabel);
            UiBuild.BottomRight(UiBuild.Rect(briefingStart.gameObject), 50f, 40f, 400f, 96f);

            ConfirmDialog dialog = InstantiatePrefab<ConfirmDialog>("ConfirmDialog.prefab", context.Canvas.transform);

            var controller = AddController<MissionHubController>(context, "HubScreen");
            controller.resourceContainer = resourceRow.transform;
            controller.activityContainer = scrollContent;
            controller.resourcePillPrefab = prefabs.ResourcePill;
            controller.activityCardPrefab = prefabs.ActivityCard;
            controller.briefingRoot = briefing;
            controller.briefingTitleLabel = briefingTitle;
            controller.briefingBodyLabel = briefingBody;
            controller.briefingStartButton = briefingStart;
            controller.missionCodeLabel = missionCode;
            controller.progressLabel = progress;
            controller.lastResultLabel = lastResult;
            controller.sendReportButton = sendReport;
            controller.restartButton = restart;
            controller.textSizeButton = textSize;
            controller.contrastButton = contrast;
            controller.sendReportLabel = sendReportLabel;
            controller.textSizeLabel = textSizeLabel;
            controller.contrastLabel = contrastLabel;
            controller.dialog = dialog;

            Save(context, HubPath);
        }

        private static void BuildLanding(MissionDataBuilder.Content content, MissionUiBuilder.Prefabs prefabs)
        {
            UiTheme theme = content.Theme;
            LandingMissionConfig landing = content.LandingConfig;
            SceneContext context = CreateScreen(content, "02_LandingSite");
            Transform safe = context.SafeArea;

            GameObject top = TopBar(safe, "TopBar", 160f, theme);

            Button back = UiBuild.Button(top.transform, "Back", "Back", false, theme, out TextMeshProUGUI backLabel);
            UiBuild.TopLeft(UiBuild.Rect(back.gameObject), 30f, 26f, 130f, 76f);

            TextMeshProUGUI title = UiBuild.Text(
                top.transform, "Title", landing != null ? landing.screenTitle : "Landing Site",
                TextRole.Heading, TextAlignmentOptions.Left, theme);
            UiBuild.TopLeft(UiBuild.Rect(title.gameObject), 190f, 22f, 1200f, 56f);

            TextMeshProUGUI tagline = UiBuild.Text(
                top.transform, "Tagline", landing != null ? landing.tagline : string.Empty,
                TextRole.Body, TextAlignmentOptions.Left, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(tagline.gameObject), 190f, 82f, 1200f, 38f);

            GameObject mapPanel = UiBuild.NewUi("MapPanel", safe);
            UiBuild.TopLeft(UiBuild.Rect(mapPanel), 50f, 180f, 900f, 650f);
            UiBuild.AddPanel(mapPanel, PanelRole.Panel, true, theme);

            GameObject mapGo = UiBuild.NewUi("Map", mapPanel.transform);
            UiBuild.Stretch(UiBuild.Rect(mapGo), 14f, 14f, 14f, 14f);
            Image mapImage = UiBuild.AddImage(mapGo, Color.white, content.Sprites.LandingMap);
            mapImage.preserveAspect = false;

            GameObject outpost = UiBuild.NewUi("Outpost", mapGo.transform);
            var outpostRect = UiBuild.Rect(outpost);
            outpostRect.anchorMin = new Vector2(0.47f, 0.12f);
            outpostRect.anchorMax = new Vector2(0.47f, 0.12f);
            outpostRect.pivot = new Vector2(0.5f, 0.5f);
            outpostRect.anchoredPosition = Vector2.zero;
            outpostRect.sizeDelta = new Vector2(34f, 34f);
            Image outpostDot = UiBuild.AddImage(outpost, theme.textPrimary, theme.circle);
            outpostDot.raycastTarget = false;

            GameObject outpostLabelGo = UiBuild.NewUi("OutpostLabel", mapGo.transform);
            var outpostLabelRect = UiBuild.Rect(outpostLabelGo);
            outpostLabelRect.anchorMin = new Vector2(0.47f, 0.12f);
            outpostLabelRect.anchorMax = new Vector2(0.47f, 0.12f);
            outpostLabelRect.pivot = new Vector2(0.5f, 0f);
            outpostLabelRect.anchoredPosition = new Vector2(0f, 24f);
            outpostLabelRect.sizeDelta = new Vector2(180f, 30f);
            TextMeshProUGUI outpostLabel = UiBuild.AddText(
                outpostLabelGo, "OUTPOST", TextRole.Caption, TextAlignmentOptions.Center, theme);
            outpostLabel.color = theme.textPrimary;

            GameObject captionBackdrop = UiBuild.NewUi("CaptionBackdrop", mapPanel.transform);
            var captionBackdropRect = UiBuild.Rect(captionBackdrop);
            captionBackdropRect.anchorMin = new Vector2(0f, 0f);
            captionBackdropRect.anchorMax = new Vector2(1f, 0f);
            captionBackdropRect.pivot = new Vector2(0.5f, 0f);
            captionBackdropRect.anchoredPosition = new Vector2(0f, 14f);
            captionBackdropRect.sizeDelta = new Vector2(0f, 44f);
            Image captionBackground = UiBuild.AddImage(
                captionBackdrop,
                new Color(theme.background.r, theme.background.g, theme.background.b, 0.62f),
                null,
                false,
                false);
            captionBackground.raycastTarget = false;

            TextMeshProUGUI mapCaption = UiBuild.Text(
                mapPanel.transform, "MapCaption", landing != null ? landing.mapCaption : string.Empty,
                TextRole.Caption, TextAlignmentOptions.Left, theme, secondary: true);
            UiBuild.BottomLeft(UiBuild.Rect(mapCaption.gameObject), 32f, 20f, 700f, 32f);

            GameObject details = UiBuild.NewUi("DetailsPanel", safe);
            UiBuild.TopLeft(UiBuild.Rect(details), 980f, 180f, 620f, 650f);
            UiBuild.AddPanel(details, PanelRole.PanelAlt, true, theme);

            TextMeshProUGUI detailsHeader = UiBuild.Text(
                details.transform, "DetailsHeader", landing != null ? landing.selectedHeader : "SITE DETAILS",
                TextRole.Caption, TextAlignmentOptions.Left, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(detailsHeader.gameObject), 34f, 26f, 552f, 30f);

            TextMeshProUGUI siteName = UiBuild.Text(
                details.transform, "SiteName", "Choose a site", TextRole.Heading, TextAlignmentOptions.Left, theme);
            UiBuild.TopLeft(UiBuild.Rect(siteName.gameObject), 34f, 58f, 552f, 58f);

            TextMeshProUGUI siteRegion = UiBuild.Text(
                details.transform, "SiteRegion", "", TextRole.Body, TextAlignmentOptions.Left, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(siteRegion.gameObject), 34f, 118f, 552f, 38f);

            GameObject divider = UiBuild.NewUi("Divider", details.transform);
            UiBuild.TopLeft(UiBuild.Rect(divider), 34f, 168f, 552f, 2f);
            UiBuild.AddImage(divider, new Color(theme.textSecondary.r, theme.textSecondary.g, theme.textSecondary.b, 0.28f));

            TextMeshProUGUI slopeCaption = UiBuild.Text(
                details.transform, "SlopeCaption", "SLOPE", TextRole.Caption, TextAlignmentOptions.Left, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(slopeCaption.gameObject), 34f, 190f, 300f, 30f);

            GameObject slopeDotGo = UiBuild.NewUi("SlopeDot", details.transform);
            UiBuild.TopLeft(UiBuild.Rect(slopeDotGo), 34f, 232f, 18f, 18f);
            Image slopeDot = UiBuild.AddImage(slopeDotGo, theme.success, theme.circle);

            TextMeshProUGUI slopeValue = UiBuild.Text(
                details.transform, "SlopeValue", "-", TextRole.Body, TextAlignmentOptions.Left, theme);
            UiBuild.TopLeft(UiBuild.Rect(slopeValue.gameObject), 64f, 222f, 480f, 44f);

            TextMeshProUGUI dustCaption = UiBuild.Text(
                details.transform, "DustCaption", "DUST", TextRole.Caption, TextAlignmentOptions.Left, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(dustCaption.gameObject), 34f, 290f, 300f, 30f);

            GameObject dustDotGo = UiBuild.NewUi("DustDot", details.transform);
            UiBuild.TopLeft(UiBuild.Rect(dustDotGo), 34f, 332f, 18f, 18f);
            Image dustDot = UiBuild.AddImage(dustDotGo, theme.success, theme.circle);

            TextMeshProUGUI dustValue = UiBuild.Text(
                details.transform, "DustValue", "-", TextRole.Body, TextAlignmentOptions.Left, theme);
            UiBuild.TopLeft(UiBuild.Rect(dustValue.gameObject), 64f, 322f, 480f, 44f);

            TextMeshProUGUI distanceCaption = UiBuild.Text(
                details.transform, "DistanceCaption", "DISTANCE TO OUTPOST", TextRole.Caption, TextAlignmentOptions.Left, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(distanceCaption.gameObject), 34f, 390f, 420f, 30f);

            GameObject distanceDotGo = UiBuild.NewUi("DistanceDot", details.transform);
            UiBuild.TopLeft(UiBuild.Rect(distanceDotGo), 34f, 432f, 18f, 18f);
            Image distanceDot = UiBuild.AddImage(distanceDotGo, theme.success, theme.circle);

            TextMeshProUGUI distanceValue = UiBuild.Text(
                details.transform, "DistanceValue", "-", TextRole.Body, TextAlignmentOptions.Left, theme);
            UiBuild.TopLeft(UiBuild.Rect(distanceValue.gameObject), 64f, 422f, 480f, 44f);

            string emptyDescription = landing != null && !string.IsNullOrWhiteSpace(landing.prompt)
                ? landing.prompt
                : "Tap a marker to inspect a site.";
            TextMeshProUGUI description = UiBuild.Text(
                details.transform, "Description", emptyDescription,
                TextRole.Body, TextAlignmentOptions.TopLeft, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(description.gameObject), 34f, 486f, 552f, 140f);

            TextMeshProUGUI note = UiBuild.Text(
                safe, "SimulationNote", content.Landing.simulationNote, TextRole.Caption,
                TextAlignmentOptions.TopLeft, theme, secondary: true);
            UiBuild.BottomLeft(UiBuild.Rect(note.gameObject), 50f, 28f, 900f, 82f);

            Button confirm = UiBuild.Button(
                safe, "Confirm", landing != null ? landing.confirmLabel : "Confirm landing site",
                true, theme, out TextMeshProUGUI confirmLabel);
            UiBuild.BottomRight(UiBuild.Rect(confirm.gameObject), 50f, 28f, 380f, 96f);

            ActivityResultPanel resultPanel =
                InstantiatePrefab<ActivityResultPanel>("ActivityResultPanel.prefab", context.Canvas.transform);

            ConfirmDialog dialog = InstantiatePrefab<ConfirmDialog>("ConfirmDialog.prefab", context.Canvas.transform);

            var controller = AddController<LandingSiteController>(context, "LandingScreen");
            controller.activity = content.Landing;
            controller.landing = landing;
            controller.titleLabel = title;
            controller.taglineLabel = tagline;
            controller.backButton = back;
            controller.markerContainer = mapGo.transform;
            controller.markerPrefab = prefabs.LandingSiteMarker;
            controller.mapCaptionLabel = mapCaption;
            controller.detailsHeaderLabel = detailsHeader;
            controller.siteNameLabel = siteName;
            controller.siteRegionLabel = siteRegion;
            controller.siteDescriptionLabel = description;
            controller.slopeLabel = slopeValue;
            controller.dustLabel = dustValue;
            controller.distanceLabel = distanceValue;
            controller.slopeDot = slopeDot;
            controller.dustDot = dustDot;
            controller.distanceDot = distanceDot;
            controller.confirmButton = confirm;
            controller.confirmLabel = confirmLabel;
            controller.resultPanel = resultPanel;
            controller.dialog = dialog;

            Save(context, LandingPath);
        }

        private static void BuildGreenhouse(MissionDataBuilder.Content content, MissionUiBuilder.Prefabs prefabs)
        {
            UiTheme theme = content.Theme;
            GreenhouseExperimentConfig experiment = content.GreenhouseConfig;
            SceneContext context = CreateScreen(content, "03_Greenhouse");
            Transform safe = context.SafeArea;

            GameObject top = TopBar(safe, "TopBar", 160f, theme);

            Button back = UiBuild.Button(top.transform, "Back", "Back", false, theme, out TextMeshProUGUI backLabel);
            UiBuild.TopLeft(UiBuild.Rect(back.gameObject), 30f, 26f, 130f, 76f);

            TextMeshProUGUI title = UiBuild.Text(
                top.transform, "Title", experiment != null ? experiment.screenTitle : "Plant Lab",
                TextRole.Heading, TextAlignmentOptions.Left, theme);
            UiBuild.TopLeft(UiBuild.Rect(title.gameObject), 190f, 22f, 1200f, 56f);

            TextMeshProUGUI tagline = UiBuild.Text(
                top.transform, "Tagline", experiment != null ? experiment.tagline : string.Empty,
                TextRole.Body, TextAlignmentOptions.Left, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(tagline.gameObject), 190f, 82f, 1200f, 38f);

            GameObject chamber = UiBuild.NewUi("ChamberPanel", safe);
            UiBuild.TopLeft(UiBuild.Rect(chamber), 50f, 180f, 940f, 650f);
            UiBuild.AddPanel(chamber, PanelRole.Panel, true, theme);

            TextMeshProUGUI chamberTitle = UiBuild.Text(
                chamber.transform, "ChamberTitle", experiment != null ? experiment.chamberTitle : "GROWTH CHAMBER",
                TextRole.Caption, TextAlignmentOptions.Left, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(chamberTitle.gameObject), 30f, 22f, 540f, 32f);

            GameObject environment = UiBuild.NewUi("Environment", chamber.transform);
            UiBuild.TopRight(UiBuild.Rect(environment), 30f, 18f, 290f, 48f);
            UiBuild.AddPanel(environment, PanelRole.PanelAlt, true, theme);

            TextMeshProUGUI environmentLabel = UiBuild.Text(
                environment.transform, "Label", experiment != null ? experiment.environmentLabel : "MARS-LIKE GRAVITY",
                TextRole.Caption, TextAlignmentOptions.Center, theme, secondary: true);
            UiBuild.Stretch(UiBuild.Rect(environmentLabel.gameObject), 10f, 4f, 10f, 4f);

            GreenhouseTrayView trayA = InstantiatePrefab<GreenhouseTrayView>("GreenhouseTray.prefab", chamber.transform);
            UiBuild.TopLeft(UiBuild.Rect(trayA.gameObject), 24f, 92f, 430f, 550f);
            trayA.gameObject.SetActive(true);

            GreenhouseTrayView trayB = InstantiatePrefab<GreenhouseTrayView>("GreenhouseTray.prefab", chamber.transform);
            UiBuild.TopLeft(UiBuild.Rect(trayB.gameObject), 486f, 92f, 430f, 550f);
            trayB.gameObject.SetActive(true);

            GameObject notebook = UiBuild.NewUi("Notebook", safe);
            UiBuild.TopLeft(UiBuild.Rect(notebook), 1030f, 180f, 800f, 650f);
            UiBuild.AddPanel(notebook, PanelRole.Panel, true, theme);

            TextMeshProUGUI notebookTitle = UiBuild.Text(
                notebook.transform, "NotebookTitle", experiment != null ? experiment.notebookTitle : "LAB NOTEBOOK",
                TextRole.Caption, TextAlignmentOptions.Left, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(notebookTitle.gameObject), 26f, 20f, 748f, 32f);

            TextMeshProUGUI step = UiBuild.Text(
                notebook.transform, "Step", "STEP 1 - SET THE TREATMENTS",
                TextRole.Body, TextAlignmentOptions.Left, theme);
            UiBuild.TopLeft(UiBuild.Rect(step.gameObject), 26f, 58f, 748f, 42f);

            TextMeshProUGUI body = UiBuild.Text(
                notebook.transform, "Body", "Give one tray red light. Keep the other dark as the control.",
                TextRole.Body, TextAlignmentOptions.TopLeft, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(body.gameObject), 26f, 104f, 748f, 88f);

            GameObject setupGroup = UiBuild.NewUi("SetupGroup", notebook.transform);
            UiBuild.Stretch(UiBuild.Rect(setupGroup));

            TextMeshProUGUI setupStatus = UiBuild.Text(
                setupGroup.transform, "SetupStatus", "A fair test uses one red-light tray and one dark control.",
                TextRole.Body, TextAlignmentOptions.TopLeft, theme);
            UiBuild.TopLeft(UiBuild.Rect(setupStatus.gameObject), 26f, 204f, 748f, 80f);

            TextMeshProUGUI runCost = UiBuild.Text(
                setupGroup.transform, "RunCost", "Uses 8 Power",
                TextRole.Caption, TextAlignmentOptions.Left, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(runCost.gameObject), 26f, 300f, 360f, 34f);

            Button run = UiBuild.Button(
                setupGroup.transform, "Run", experiment != null ? experiment.runLabel : "Run light trial",
                true, theme, out TextMeshProUGUI runLabel);
            UiBuild.TopLeft(UiBuild.Rect(run.gameObject), 26f, 350f, 748f, 84f);

            GameObject claimsGroup = UiBuild.NewUi("ClaimsGroup", notebook.transform);
            UiBuild.Stretch(UiBuild.Rect(claimsGroup));
            claimsGroup.SetActive(false);

            TextMeshProUGUI claimsHeader = UiBuild.Text(
                claimsGroup.transform, "ClaimsHeader", experiment != null ? experiment.claimsHeader : "STEP 3 - CHOOSE A CAREFUL CLAIM",
                TextRole.Caption, TextAlignmentOptions.Left, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(claimsHeader.gameObject), 26f, 204f, 748f, 32f);

            TextMeshProUGUI claimPrompt = UiBuild.Text(
                claimsGroup.transform, "ClaimPrompt", experiment != null ? experiment.claimPrompt : "Which sentence is supported by the study?",
                TextRole.Body, TextAlignmentOptions.Left, theme);
            UiBuild.TopLeft(UiBuild.Rect(claimPrompt.gameObject), 26f, 238f, 748f, 38f);

            GameObject claimsRoot = UiBuild.NewUi("Claims", claimsGroup.transform);
            UiBuild.TopLeft(UiBuild.Rect(claimsRoot), 26f, 286f, 748f, 260f);
            var claimsLayout = claimsRoot.AddComponent<VerticalLayoutGroup>();
            claimsLayout.spacing = 4f;
            claimsLayout.childControlWidth = true;
            claimsLayout.childControlHeight = true;
            claimsLayout.childForceExpandWidth = true;
            claimsLayout.childForceExpandHeight = false;
            claimsLayout.childAlignment = TextAnchor.UpperLeft;
            claimsLayout.padding = new RectOffset(0, 0, 0, 0);

            TextMeshProUGUI claimFeedback = UiBuild.Text(
                claimsGroup.transform, "ClaimFeedback", string.Empty,
                TextRole.Caption, TextAlignmentOptions.TopLeft, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(claimFeedback.gameObject), 26f, 558f, 748f, 72f);

            TextMeshProUGUI note = UiBuild.Text(
                safe, "SimulationNote", content.Greenhouse.simulationNote, TextRole.Caption,
                TextAlignmentOptions.TopLeft, theme, secondary: true);
            UiBuild.BottomLeft(UiBuild.Rect(note.gameObject), 50f, 28f, 900f, 82f);

            Button confirm = UiBuild.Button(
                safe, "Confirm", experiment != null ? experiment.confirmLabel : "Log finding",
                true, theme, out TextMeshProUGUI confirmLabel);
            UiBuild.BottomRight(UiBuild.Rect(confirm.gameObject), 50f, 28f, 430f, 96f);
            confirm.gameObject.SetActive(false);

            ActivityResultPanel resultPanel =
                InstantiatePrefab<ActivityResultPanel>("ActivityResultPanel.prefab", context.Canvas.transform);

            ConfirmDialog dialog = InstantiatePrefab<ConfirmDialog>("ConfirmDialog.prefab", context.Canvas.transform);

            var controller = AddController<GreenhouseController>(context, "GreenhouseScreen");
            controller.activity = content.Greenhouse;
            controller.experiment = experiment;
            controller.titleLabel = title;
            controller.taglineLabel = tagline;
            controller.backButton = back;
            controller.chamberTitleLabel = chamberTitle;
            controller.environmentLabel = environmentLabel;
            controller.trayA = trayA;
            controller.trayB = trayB;
            controller.notebookTitleLabel = notebookTitle;
            controller.stepLabel = step;
            controller.bodyLabel = body;
            controller.setupGroup = setupGroup;
            controller.setupStatusLabel = setupStatus;
            controller.runCostLabel = runCost;
            controller.runButton = run;
            controller.runLabel = runLabel;
            controller.claimsGroup = claimsGroup;
            controller.claimsHeaderLabel = claimsHeader;
            controller.claimPromptLabel = claimPrompt;
            controller.claimsContainer = claimsRoot.transform;
            controller.claimPrefab = prefabs.GreenhouseClaim;
            controller.claimFeedbackLabel = claimFeedback;
            controller.confirmButton = confirm;
            controller.confirmLabel = confirmLabel;
            controller.resultPanel = resultPanel;
            controller.dialog = dialog;

            Save(context, GreenhousePath);
        }

        public static void BuildWeather(MissionDataBuilder.Content content, MissionUiBuilder.Prefabs prefabs)
        {
            UiTheme theme = content.Theme;
            SceneContext context = CreateScreen(content, "04_SpaceWeather");
            SpaceWeatherConfig config = AssetDatabase.LoadAssetAtPath<SpaceWeatherConfig>(
                "Assets/Data/Weather/SpaceWeatherConfig.asset");
            if (config == null)
            {
                config = content.WeatherConfig;
            }

            Transform safe = context.SafeArea;

            GameObject top = TopBar(safe, "TopBar", 160f, theme);

            Button back = UiBuild.Button(top.transform, "Back", "Back", false, theme, out TextMeshProUGUI backLabel);
            UiBuild.TopLeft(UiBuild.Rect(back.gameObject), 30f, 26f, 130f, 76f);

            TextMeshProUGUI title = UiBuild.Text(
                top.transform, "Title", config != null ? config.screenTitle : "Space Weather",
                TextRole.Heading, TextAlignmentOptions.Left, theme);
            UiBuild.TopLeft(UiBuild.Rect(title.gameObject), 190f, 16f, 1200f, 64f);

            TextMeshProUGUI tagline = UiBuild.Text(
                top.transform, "Tagline", config != null ? config.tagline : string.Empty,
                TextRole.Body, TextAlignmentOptions.Left, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(tagline.gameObject), 190f, 86f, 1200f, 38f);

            GameObject station = UiBuild.NewUi("ControlRoomPanel", safe);
            UiBuild.TopLeft(UiBuild.Rect(station), 50f, 180f, 900f, 650f);
            UiBuild.AddPanel(station, PanelRole.Panel, true, theme);

            TextMeshProUGUI controlTitle = UiBuild.Text(
                station.transform, "ControlRoomTitle", config != null ? config.controlRoomTitle : "SPACE WEATHER STATION",
                TextRole.Caption, TextAlignmentOptions.Left, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(controlTitle.gameObject), 26f, 16f, 560f, 32f);

            TextMeshProUGUI sourceLabel = UiBuild.Text(
                station.transform, "SourceLabel", string.Empty,
                TextRole.Caption, TextAlignmentOptions.Right, theme, secondary: true);
            UiBuild.TopRight(UiBuild.Rect(sourceLabel.gameObject), 26f, 16f, 260f, 32f);

            GameObject alertGroup = UiBuild.NewUi("AlertGroup", station.transform);
            UiBuild.Stretch(UiBuild.Rect(alertGroup));

            GameObject artGo = UiBuild.NewUi("ControlRoomArt", alertGroup.transform);
            UiBuild.TopLeft(UiBuild.Rect(artGo), 26f, 50f, 848f, 238f);
            if (content.Sprites != null && content.Sprites.WeatherControlRoom != null)
            {
                UiBuild.AddImage(artGo, Color.white, content.Sprites.WeatherControlRoom);
            }
            else
            {
                UiBuild.AddPanel(artGo, PanelRole.PanelAlt, true, theme);
            }

            GameObject badge = UiBuild.NewUi("EventBadge", alertGroup.transform);
            UiBuild.TopLeft(UiBuild.Rect(badge), 26f, 302f, 84f, 84f);
            UiBuild.AddPanel(badge, PanelRole.Primary, true, theme);
            TextMeshProUGUI eventTypeLabel = UiBuild.Text(
                badge.transform, "EventType", "FLR",
                TextRole.Body, TextAlignmentOptions.Center, theme);
            UiBuild.Center(UiBuild.Rect(eventTypeLabel.gameObject), 0f, 0f, 84f, 84f);
            eventTypeLabel.fontStyle = FontStyles.Bold;
            var eventTypeTheme = eventTypeLabel.GetComponent<ThemedText>();
            eventTypeTheme.overrideColor = true;
            eventTypeTheme.colorOverride = theme.primaryText;

            TextMeshProUGUI eventNameLabel = UiBuild.Text(
                alertGroup.transform, "EventName", "Solar flare",
                TextRole.Body, TextAlignmentOptions.Left, theme);
            UiBuild.TopLeft(UiBuild.Rect(eventNameLabel.gameObject), 122f, 300f, 520f, 44f);
            eventNameLabel.fontStyle = FontStyles.Bold;

            TextMeshProUGUI alertCodeLabel = UiBuild.Text(
                alertGroup.transform, "AlertCode", "TRAINING ALERT FLR-01",
                TextRole.Caption, TextAlignmentOptions.Left, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(alertCodeLabel.gameObject), 122f, 344f, 520f, 32f);

            GameObject alertTagGo = UiBuild.NewUi("AlertTag", alertGroup.transform);
            UiBuild.TopRight(UiBuild.Rect(alertTagGo), 26f, 306f, 224f, 40f);
            UiBuild.AddPanel(alertTagGo, PanelRole.PanelAlt, true, theme);
            TextMeshProUGUI alertTagLabel = UiBuild.Text(
                alertTagGo.transform, "Label", config != null ? config.alertTag : "TRAINING ALERT",
                TextRole.Caption, TextAlignmentOptions.Center, theme, secondary: true);
            UiBuild.Stretch(UiBuild.Rect(alertTagLabel.gameObject), 10f, 4f, 10f, 4f);

            TextMeshProUGUI alertSummaryLabel = UiBuild.Text(
                alertGroup.transform, "AlertSummary", string.Empty,
                TextRole.Body, TextAlignmentOptions.TopLeft, theme);
            UiBuild.TopLeft(UiBuild.Rect(alertSummaryLabel.gameObject), 26f, 398f, 848f, 140f);

            TextMeshProUGUI sourceNoteLabel = UiBuild.Text(
                alertGroup.transform, "SourceNote",
                config != null && config.fallbackAlert != null ? config.fallbackAlert.sourceNote : string.Empty,
                TextRole.Caption, TextAlignmentOptions.Left, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(sourceNoteLabel.gameObject), 26f, 542f, 848f, 38f);

            GameObject decoderGroup = UiBuild.NewUi("DecoderGroup", station.transform);
            UiBuild.Stretch(UiBuild.Rect(decoderGroup));
            decoderGroup.SetActive(false);

            TextMeshProUGUI decoderTitle = UiBuild.Text(
                decoderGroup.transform, "DecoderTitle", config != null ? config.decoderTitle : "DONKI EVENT TYPES",
                TextRole.Caption, TextAlignmentOptions.Left, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(decoderTitle.gameObject), 26f, 48f, 848f, 32f);

            TextMeshProUGUI decoderHint = UiBuild.Text(
                decoderGroup.transform, "DecoderHint", config != null ? config.decoderHint : string.Empty,
                TextRole.Caption, TextAlignmentOptions.Left, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(decoderHint.gameObject), 26f, 82f, 848f, 32f);

            SpaceWeatherEventTypeInfo[] types = config != null ? config.eventTypes : null;
            BuildWeatherDecoderCell(decoderGroup.transform, types, 0, theme);
            BuildWeatherDecoderCell(decoderGroup.transform, types, 1, theme);
            BuildWeatherDecoderCell(decoderGroup.transform, types, 2, theme);
            BuildWeatherDecoderCell(decoderGroup.transform, types, 3, theme);

            Button compare = UiBuild.Button(
                station.transform, "Compare", config != null ? config.compareLabel : "Compare alert types",
                false, theme, out TextMeshProUGUI compareLabel);
            UiBuild.TopLeft(UiBuild.Rect(compare.gameObject), 26f, 538f, 848f, 64f);

            GameObject decision = UiBuild.NewUi("DecisionPanel", safe);
            UiBuild.TopLeft(UiBuild.Rect(decision), 980f, 180f, 890f, 650f);
            UiBuild.AddPanel(decision, PanelRole.Panel, true, theme);

            TextMeshProUGUI decisionTitle = UiBuild.Text(
                decision.transform, "DecisionTitle", config != null ? config.decisionTitle : "CREW PLAN",
                TextRole.Caption, TextAlignmentOptions.Left, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(decisionTitle.gameObject), 26f, 16f, 500f, 32f);

            TextMeshProUGUI headlineLabel = UiBuild.Text(
                decision.transform, "Headline", "A training alert is on the screen",
                TextRole.Body, TextAlignmentOptions.Left, theme);
            UiBuild.TopLeft(UiBuild.Rect(headlineLabel.gameObject), 26f, 50f, 838f, 44f);
            headlineLabel.fontStyle = FontStyles.Bold;

            TextMeshProUGUI decisionPromptLabel = UiBuild.Text(
                decision.transform, "DecisionPrompt", config != null ? config.decisionPrompt : string.Empty,
                TextRole.Body, TextAlignmentOptions.TopLeft, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(decisionPromptLabel.gameObject), 26f, 100f, 838f, 62f);

            GameObject planList = UiBuild.NewUi("Plans", decision.transform);
            UiBuild.TopLeft(UiBuild.Rect(planList), 26f, 170f, 838f, 372f);
            var plansLayout = planList.AddComponent<VerticalLayoutGroup>();
            plansLayout.spacing = 6f;
            plansLayout.childControlWidth = true;
            plansLayout.childControlHeight = true;
            plansLayout.childForceExpandWidth = true;
            plansLayout.childForceExpandHeight = false;
            plansLayout.childAlignment = TextAnchor.UpperLeft;
            plansLayout.padding = new RectOffset(0, 0, 0, 0);

            TextMeshProUGUI planHintLabel = UiBuild.Text(
                decision.transform, "PlanHint", config != null ? config.planHint : string.Empty,
                TextRole.Caption, TextAlignmentOptions.TopLeft, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(planHintLabel.gameObject), 26f, 556f, 838f, 60f);

            TextMeshProUGUI note = UiBuild.Text(
                safe, "SimulationNote", config != null ? config.simulationNote : string.Empty,
                TextRole.Caption, TextAlignmentOptions.TopLeft, theme, secondary: true);
            UiBuild.BottomLeft(UiBuild.Rect(note.gameObject), 50f, 24f, 900f, 104f);

            Button confirm = UiBuild.Button(
                safe, "Confirm", config != null ? config.choosePlanLabel : "Choose a crew plan",
                true, theme, out TextMeshProUGUI confirmLabel);
            UiBuild.BottomRight(UiBuild.Rect(confirm.gameObject), 50f, 28f, 430f, 96f);
            confirm.interactable = false;

            ActivityResultPanel resultPanel =
                InstantiatePrefab<ActivityResultPanel>("ActivityResultPanel.prefab", context.Canvas.transform);

            ConfirmDialog dialog = InstantiatePrefab<ConfirmDialog>("ConfirmDialog.prefab", context.Canvas.transform);

            var controller = AddController<SpaceWeatherController>(context, "SpaceWeatherScreen");
            controller.activity = content.Weather;
            controller.config = config;
            controller.screenTitleLabel = title;
            controller.taglineLabel = tagline;
            controller.alertGroup = alertGroup;
            controller.decoderGroup = decoderGroup;
            controller.controlRoomArt = artGo.GetComponent<Image>();
            controller.controlRoomTitleLabel = controlTitle;
            controller.sourceLabel = sourceLabel;
            controller.eventTypeLabel = eventTypeLabel;
            controller.eventNameLabel = eventNameLabel;
            controller.alertCodeLabel = alertCodeLabel;
            controller.alertTagLabel = alertTagLabel;
            controller.alertSummaryLabel = alertSummaryLabel;
            controller.sourceNoteLabel = sourceNoteLabel;
            controller.compareButton = compare;
            controller.compareLabel = compareLabel;
            controller.decisionTitleLabel = decisionTitle;
            controller.headlineLabel = headlineLabel;
            controller.decisionPromptLabel = decisionPromptLabel;
            controller.planContainer = planList.transform;
            controller.planPrefab = prefabs.WeatherPlan;
            controller.planHintLabel = planHintLabel;
            controller.confirmButton = confirm;
            controller.confirmLabel = confirmLabel;
            controller.simulationNoteLabel = note;
            controller.resultPanel = resultPanel;
            controller.dialog = dialog;

            Save(context, WeatherPath);
        }

        private static void BuildWeatherDecoderCell(
            Transform parent,
            SpaceWeatherEventTypeInfo[] types,
            int index,
            UiTheme theme)
        {
            SpaceWeatherEventTypeInfo info = null;
            if (types != null && index < types.Length)
            {
                info = types[index];
            }

            if (info == null)
            {
                info = new SpaceWeatherEventTypeInfo
                {
                    code = index == 0 ? "FLR" : index == 1 ? "SEP" : index == 2 ? "CME" : "GST",
                    name = "Event type",
                    description = "Compare this event with the alert."
                };
            }

            GameObject cell = UiBuild.NewUi($"Decoder_{info.code}", parent);
            int column = index % 2;
            int row = index / 2;
            UiBuild.TopLeft(UiBuild.Rect(cell), 26f + column * 426f, 136f + row * 188f, 410f, 180f);
            UiBuild.AddPanel(cell, PanelRole.PanelAlt, true, theme);

            GameObject badge = UiBuild.NewUi("CodeBadge", cell.transform);
            UiBuild.TopLeft(UiBuild.Rect(badge), 16f, 16f, 66f, 48f);
            UiBuild.AddPanel(badge, PanelRole.Primary, true, theme);
            TextMeshProUGUI code = UiBuild.Text(
                badge.transform, "Code", info.code,
                TextRole.Caption, TextAlignmentOptions.Center, theme);
            UiBuild.Stretch(UiBuild.Rect(code.gameObject), 4f, 2f, 4f, 2f);
            var codeTheme = code.GetComponent<ThemedText>();
            codeTheme.overrideColor = true;
            codeTheme.colorOverride = theme.primaryText;
            code.fontStyle = FontStyles.Bold;

            TextMeshProUGUI name = UiBuild.Text(
                cell.transform, "Name", info.name,
                TextRole.Caption, TextAlignmentOptions.Left, theme);
            UiBuild.TopLeft(UiBuild.Rect(name.gameObject), 94f, 14f, 300f, 32f);
            name.fontStyle = FontStyles.Bold;

            TextMeshProUGUI description = UiBuild.Text(
                cell.transform, "Description", info.description,
                TextRole.Caption, TextAlignmentOptions.TopLeft, theme, secondary: true);
            description.textWrappingMode = TextWrappingModes.Normal;
            UiBuild.TopLeft(UiBuild.Rect(description.gameObject), 16f, 66f, 378f, 100f);
        }

        private static void BuildActivity(
            MissionDataBuilder.Content content,
            MissionUiBuilder.Prefabs prefabs,
            ActivityDefinition activity,
            PlaceholderActivityOutcome outcome,
            string path,
            Sprite icon)
        {
            UiTheme theme = content.Theme;
            SceneContext context = CreateScreen(content, activity.sceneName);
            Transform safe = context.SafeArea;

            GameObject top = TopBar(safe, "TopBar", 160f, theme);
            var controller = AddController<PlaceholderActivityController>(context, "ActivityScreen");

            Button back = UiBuild.Button(top.transform, "Back", "Back", false, theme, out TextMeshProUGUI backLabel);
            UiBuild.TopLeft(UiBuild.Rect(back.gameObject), 30f, 26f, 130f, 76f);

            TextMeshProUGUI title = UiBuild.Text(
                top.transform, "Title", activity.displayName, TextRole.Heading, TextAlignmentOptions.Left, theme);
            UiBuild.TopLeft(UiBuild.Rect(title.gameObject), 190f, 22f, 1200f, 56f);

            TextMeshProUGUI tagline = UiBuild.Text(
                top.transform, "Tagline", activity.tagline, TextRole.Body, TextAlignmentOptions.Left, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(tagline.gameObject), 190f, 82f, 1300f, 38f);

            GameObject panel = UiBuild.NewUi("ActivityPanel", safe);
            UiBuild.Stretch(UiBuild.Rect(panel), 60f, 180f, 60f, 240f);
            UiBuild.AddPanel(panel, PanelRole.Panel, true, theme);

            TextMeshProUGUI instructionTitle = UiBuild.Text(
                panel.transform, "InstructionTitle", activity.instructionTitle, TextRole.Heading, TextAlignmentOptions.Left, theme);
            UiBuild.TopLeft(UiBuild.Rect(instructionTitle.gameObject), 44f, 32f, 1180f, 54f);

            TextMeshProUGUI instructionBody = UiBuild.Text(
                panel.transform, "InstructionBody", activity.instructionBody, TextRole.Body, TextAlignmentOptions.TopLeft, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(instructionBody.gameObject), 44f, 98f, 1120f, 360f);

            GameObject art = UiBuild.NewUi("Art", panel.transform);
            UiBuild.TopRight(UiBuild.Rect(art), 70f, 70f, 300f, 300f);
            UiBuild.AddImage(art, new Color(theme.primary.r, theme.primary.g, theme.primary.b, 0.14f), theme.circle);

            GameObject artIcon = UiBuild.NewUi("Icon", art.transform);
            UiBuild.Center(UiBuild.Rect(artIcon), 0f, 0f, 180f, 180f);
            UiBuild.AddImage(artIcon, theme.primary, icon);

            TextMeshProUGUI status = UiBuild.Text(
                panel.transform, "Status", "Ready for your decision.", TextRole.Body, TextAlignmentOptions.Left, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(status.gameObject), 44f, 520f, 1120f, 60f);

            TextMeshProUGUI note = UiBuild.Text(
                safe, "SimulationNote", activity.simulationNote, TextRole.Caption, TextAlignmentOptions.TopLeft, theme, secondary: true);
            UiBuild.BottomLeft(UiBuild.Rect(note.gameObject), 64f, 32f, 1180f, 84f);

            Button complete = UiBuild.Button(safe, "Complete", "Complete practice step", true, theme, out TextMeshProUGUI completeLabel);
            UiBuild.BottomRight(UiBuild.Rect(complete.gameObject), 64f, 30f, 420f, 100f);

            ActivityResultPanel resultPanel =
                InstantiatePrefab<ActivityResultPanel>("ActivityResultPanel.prefab", context.Canvas.transform);

            ConfirmDialog dialog = InstantiatePrefab<ConfirmDialog>("ConfirmDialog.prefab", context.Canvas.transform);

            controller.activity = activity;
            controller.placeholderOutcome = outcome;
            controller.titleLabel = title;
            controller.taglineLabel = tagline;
            controller.instructionTitleLabel = instructionTitle;
            controller.instructionBodyLabel = instructionBody;
            controller.simulationNoteLabel = note;
            controller.statusLabel = status;
            controller.backButton = back;
            controller.completeButton = complete;
            controller.completeLabel = completeLabel;
            controller.resultPanel = resultPanel;
            controller.dialog = dialog;

            Save(context, path);
        }

        private static void BuildDebrief(MissionDataBuilder.Content content, MissionUiBuilder.Prefabs prefabs)
        {
            MissionConfig config = content.Config;
            DebriefConfig debrief = config.debrief;
            UiTheme theme = content.Theme;
            SceneContext context = CreateScreen(content, "05_Debrief");
            Transform safe = context.SafeArea;

            TextMeshProUGUI header = UiBuild.Text(
                safe, "Header", debrief != null ? debrief.reportTitle : "Mission Report",
                TextRole.Title, TextAlignmentOptions.Left, theme);
            UiBuild.TopLeft(UiBuild.Rect(header.gameObject), 80f, 40f, 900f, 96f);

            GameObject badge = UiBuild.NewUi("Badge", safe);
            UiBuild.TopLeft(UiBuild.Rect(badge), 80f, 150f, 1080f, 170f);
            UiBuild.AddPanel(badge, PanelRole.PanelAlt, true, theme);

            GameObject stripe = UiBuild.NewUi("Stripe", badge.transform);
            var stripeRect = UiBuild.Rect(stripe);
            stripeRect.anchorMin = new Vector2(0f, 0f);
            stripeRect.anchorMax = new Vector2(0f, 1f);
            stripeRect.pivot = new Vector2(0f, 0.5f);
            stripeRect.offsetMin = Vector2.zero;
            stripeRect.offsetMax = new Vector2(14f, 0f);
            Image stripeImage = UiBuild.AddImage(stripe, theme.primary, null);

            TextMeshProUGUI badgeTitle = UiBuild.Text(
                badge.transform, "Title", "Mission Complete", TextRole.Heading, TextAlignmentOptions.Left, theme);
            UiBuild.TopLeft(UiBuild.Rect(badgeTitle.gameObject), 42f, 24f, 1000f, 58f);

            TextMeshProUGUI badgeBody = UiBuild.Text(
                badge.transform, "Body", "Your crew finished the training mission.", TextRole.Body,
                TextAlignmentOptions.TopLeft, theme, secondary: true);
            UiBuild.TopLeft(UiBuild.Rect(badgeBody.gameObject), 42f, 88f, 1000f, 62f);

            GameObject scienceCard = UiBuild.NewUi("Science", safe);
            UiBuild.TopRight(UiBuild.Rect(scienceCard), 80f, 150f, 330f, 170f);
            UiBuild.AddPanel(scienceCard, PanelRole.Primary, true, theme);

            TextMeshProUGUI scienceCaption = UiBuild.Text(
                scienceCard.transform, "Caption", "SCIENCE PROGRESS", TextRole.Caption, TextAlignmentOptions.Center, theme);
            UiBuild.TopLeft(UiBuild.Rect(scienceCaption.gameObject), 16f, 18f, 298f, 30f);
            var scienceCaptionTheme = scienceCaption.GetComponent<ThemedText>();
            scienceCaptionTheme.overrideColor = true;
            scienceCaptionTheme.colorOverride = theme.primaryText;

            TextMeshProUGUI scienceLabel = UiBuild.Text(
                scienceCard.transform, "Value", "0", TextRole.Title, TextAlignmentOptions.Center, theme);
            UiBuild.TopLeft(UiBuild.Rect(scienceLabel.gameObject), 16f, 52f, 298f, 100f);
            var scienceTheme = scienceLabel.GetComponent<ThemedText>();
            scienceTheme.overrideColor = true;
            scienceTheme.colorOverride = theme.primaryText;

            ScrollRect scroll = UiBuild.ScrollView(safe, "ReportScroll", out RectTransform scrollContent);
            UiBuild.Stretch(UiBuild.Rect(scroll.gameObject), 80f, 340f, 80f, 210f);

            TextMeshProUGUI intro = AddTextBlock(
                scrollContent, "Intro", debrief != null ? debrief.intro : string.Empty,
                TextRole.Body, theme, false);
            TextMeshProUGUI outcomes = AddTextBlock(scrollContent, "Outcomes", string.Empty, TextRole.Body, theme, false);
            TextMeshProUGUI sources = AddTextBlock(scrollContent, "Sources", string.Empty, TextRole.Caption, theme, true);

            Button replay = UiBuild.Button(safe, "Replay", "Play Again", true, theme, out TextMeshProUGUI replayLabel);
            UiBuild.BottomLeft(UiBuild.Rect(replay.gameObject), 80f, 42f, 310f, 90f);

            Button menu = UiBuild.Button(safe, "Menu", "Main Menu", false, theme, out TextMeshProUGUI menuLabel);
            UiBuild.BottomLeft(UiBuild.Rect(menu.gameObject), 410f, 42f, 300f, 90f);

            Button textSize = UiBuild.Button(safe, "TextSize", "Text 100%", false, theme, out TextMeshProUGUI textSizeLabel);
            UiBuild.BottomLeft(UiBuild.Rect(textSize.gameObject), 730f, 42f, 190f, 76f);

            Button contrast = UiBuild.Button(safe, "Contrast", "High contrast: OFF", false, theme, out TextMeshProUGUI contrastLabel);
            UiBuild.BottomLeft(UiBuild.Rect(contrast.gameObject), 936f, 42f, 340f, 76f);

            TextMeshProUGUI footer = UiBuild.Text(
                safe, "Footer", config.simulationDisclaimer, TextRole.Caption, TextAlignmentOptions.Left, theme, secondary: true);
            UiBuild.BottomLeft(UiBuild.Rect(footer.gameObject), 80f, 12f, 1500f, 26f);

            ConfirmDialog dialog = InstantiatePrefab<ConfirmDialog>("ConfirmDialog.prefab", context.Canvas.transform);

            var controller = AddController<DebriefController>(context, "DebriefScreen");
            controller.badgeTitleLabel = badgeTitle;
            controller.badgeBodyLabel = badgeBody;
            controller.badgeStripe = stripeImage;
            controller.scienceLabel = scienceLabel;
            controller.introLabel = intro;
            controller.outcomesLabel = outcomes;
            controller.sourcesLabel = sources;
            controller.footerLabel = footer;
            controller.replayButton = replay;
            controller.menuButton = menu;
            controller.textSizeButton = textSize;
            controller.contrastButton = contrast;
            controller.replayLabel = replayLabel;
            controller.menuLabel = menuLabel;
            controller.textSizeLabel = textSizeLabel;
            controller.contrastLabel = contrastLabel;
            controller.dialog = dialog;

            Save(context, DebriefPath);
        }

        private static TextMeshProUGUI AddTextBlock(
            Transform parent,
            string name,
            string text,
            TextRole role,
            UiTheme theme,
            bool secondary)
        {
            GameObject go = UiBuild.NewUi(name, parent);
            TextMeshProUGUI label = UiBuild.AddText(go, text, role, TextAlignmentOptions.TopLeft, theme, secondary);
            var fitter = go.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return label;
        }
    }
}
