using System;
using System.Collections.Generic;
using System.IO;
using MarsOutpost.Core;
using MarsOutpost.Radiation;
using MarsOutpost.UI;
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
    /// <summary>
    /// Builds the radiation drill arena and its data. The ship cutaway is used as a fixed
    /// illustrated arena; the player's movement is constrained by authored walkable zones.
    /// </summary>
    public static class RadiationSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/07_Radiation.unity";
        public const string ConfigPath = "Assets/Data/Radiation/RadiationConfig.asset";
        public const string ActivityPath = "Assets/Data/Configs/Activity_Radiation.asset";
        public const string ShipPath = "Assets/Art/Environment/C028FE22-C39D-4996-BEFD-12C2C401FA5A.png";
        public const string AstronautPath = "Assets/Art/Characters/Astronaut.png";
        public const string MusicPath = "Assets/Art/UI/main_gameMusic.mp3";

        private static readonly Color Dim = new Color(0.015f, 0.02f, 0.04f, 0.78f);

        [MenuItem("Tools/Mars Outpost/Build Radiation Drill Scene", priority = 4)]
        public static void BuildFromMenu()
        {
            if (File.Exists(ScenePath) &&
                !EditorUtility.DisplayDialog(
                    "Rebuild Radiation Drill?",
                    "This replaces the generated 07_Radiation scene. Your RadiationConfig asset and dialogue text are kept.",
                    "Rebuild",
                    "Cancel"))
            {
                return;
            }

            Build();
        }

        /// <summary>Batchmode entry point used by the project's content build.</summary>
        public static void BuildRadiationOnly()
        {
            Build();
        }

        public static void Build()
        {
            GeneratedSpriteBuilder.EnsureFolder("Assets/Data");
            GeneratedSpriteBuilder.EnsureFolder("Assets/Data/Radiation");
            GeneratedSpriteBuilder.EnsureFolder("Assets/Data/Configs");
            GeneratedSpriteBuilder.EnsureFolder("Assets/Scenes");

            MissionConfig mission = AssetDatabase.LoadAssetAtPath<MissionConfig>(MissionDataBuilder.MissionConfigPath);
            if (mission == null)
            {
                throw new InvalidOperationException("MissionConfig is missing. Build Phase 1 content first.");
            }

            UiTheme theme = mission.theme;
            if (theme == null)
            {
                throw new InvalidOperationException("MissionConfig has no UiTheme.");
            }

            RadiationConfig config = CreateConfig();
            ActivityDefinition activity = CreateActivity(config);
            AddActivityToMission(mission, activity);

            BuildScene(mission, theme, config);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AddSceneToBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log("[MarsOutpost] Radiation drill scene built at " + ScenePath + ".");
        }

        private static RadiationConfig CreateConfig()
        {
            RadiationConfig config = AssetDatabase.LoadAssetAtPath<RadiationConfig>(ConfigPath);
            if (config != null)
            {
                return config;
            }

            config = ScriptableObject.CreateInstance<RadiationConfig>();
            AssetDatabase.CreateAsset(config, ConfigPath);
            EditorUtility.SetDirty(config);
            return config;
        }

        private static ActivityDefinition CreateActivity(RadiationConfig config)
        {
            ActivityDefinition activity = AssetDatabase.LoadAssetAtPath<ActivityDefinition>(ActivityPath);
            if (activity != null)
            {
                if (string.IsNullOrWhiteSpace(activity.sceneName))
                {
                    activity.sceneName = "07_Radiation";
                    EditorUtility.SetDirty(activity);
                }

                return activity;
            }

            activity = ScriptableObject.CreateInstance<ActivityDefinition>();
            activity.id = "radiation";
            activity.displayName = "Radiation Drill";
            activity.tagline = "Monitor a simulated alert and activate shielding.";
            activity.description =
                "A simulated alert is spreading through the ship. Read the detector, talk to the Safety Officer, then contain it from the Radiation Control Room.";
            activity.hubOrder = 3;
            activity.estimatedMinutes = 4;
            activity.requiredForMission = false;
            activity.sceneName = "07_Radiation";
            activity.instructionTitle = "Radiation Drill";
            activity.instructionBody =
                "The red area is a game visualization of a simulated warning zone. Read the simulated detector, talk to the Safety Officer, then reach the Radiation Control Room and activate shielding.";
            activity.simulationNote =
                "This drill is a game simulation, not real radiation-safety training or a real measurement.";

            AssetDatabase.CreateAsset(activity, ActivityPath);
            EditorUtility.SetDirty(activity);
            return activity;
        }

        private static void AddActivityToMission(MissionConfig mission, ActivityDefinition activity)
        {
            var list = new List<ActivityDefinition>(mission.activities ?? Array.Empty<ActivityDefinition>());
            bool found = false;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null && list[i].id == activity.id)
                {
                    list[i] = activity;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                list.Add(activity);
            }

            mission.activities = list.ToArray();

            if (!string.IsNullOrWhiteSpace(mission.howToPlayBody))
            {
                mission.howToPlayBody = mission.howToPlayBody
                    .Replace("2. Choose one of the three training steps.", "2. Choose a training step. The Radiation Drill is optional.")
                    .Replace("4. Finish all three steps", "4. Finish the three required steps");
            }

            if (!string.IsNullOrWhiteSpace(mission.introBody) &&
                mission.introBody.IndexOf("radiation", StringComparison.OrdinalIgnoreCase) < 0)
            {
                mission.introBody = mission.introBody.TrimEnd() +
                    "\n\nOptional extra training: the Radiation Drill lets you practice reading a simulated detector and activating shielding.";
            }

            EditorUtility.SetDirty(mission);
        }

        private static void AddSceneToBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(scene => scene.path == ScenePath);
            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void BuildScene(MissionConfig mission, UiTheme theme, RadiationConfig config)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject cameraGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener))
            {
                tag = "MainCamera"
            };
            cameraGo.transform.position = new Vector3(0f, 0f, -10f);
            Camera camera = cameraGo.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.8f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = theme.background;
            camera.cullingMask = -1;

            GameObject eventGo = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventGo.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();

            GameObject canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(UiBuild.RefWidth, UiBuild.RefHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 100f;

            GameObject safeGo = UiBuild.NewUi("SafeArea", canvasGo.transform);
            UiBuild.Stretch(UiBuild.Rect(safeGo));
            safeGo.AddComponent<SafeAreaFitter>();
            Transform safe = safeGo.transform;

            GameObject musicGo = new GameObject("Music");
            AudioSource music = musicGo.AddComponent<AudioSource>();
            music.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(MusicPath);
            music.loop = true;
            music.playOnAwake = true;
            music.volume = 0.38f;

            GameObject world = new GameObject("World");
            ShipWalkableArea walkable = BuildWorld(world.transform, config, theme);
            RadiationManager hazard = BuildHazard(config, walkable);
            PlayerController2D player = BuildPlayer(config, walkable);
            NpcGuide npc = BuildNpc(config);
            ControlPanel control = BuildControlPanel(config, hazard, theme);
            Transform respawn = BuildRespawnPoint();
            RadiationSpawnPoint[] spawns = BuildSpawnPoints();

            hazard.spawnPoints = spawns;
            hazard.visual.SetColor(config.hazardColor);

            DialoguePanel dialogue;
            RadiationGameController controller;
            BuildUi(
                safe,
                mission,
                theme,
                config,
                hazard,
                player,
                npc,
                control,
                respawn,
                out dialogue,
                out controller);

            npc.dialogue = dialogue;
            controller.dialogue = dialogue;

            var bootstrapGo = new GameObject("MissionBootstrap");
            var bootstrap = bootstrapGo.AddComponent<MissionSceneBootstrap>();
            bootstrap.AssignConfig(mission);
            EditorUtility.SetDirty(bootstrap);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static ShipWalkableArea BuildWorld(Transform parent, RadiationConfig config, UiTheme theme)
        {
            Sprite ship = LoadFirstSprite(ShipPath);
            if (ship == null)
            {
                throw new InvalidOperationException("Ship cutaway sprite is missing: " + ShipPath);
            }

            GameObject shipGo = new GameObject("ShipCutaway", typeof(SpriteRenderer));
            shipGo.transform.SetParent(parent, false);
            var shipRenderer = shipGo.GetComponent<SpriteRenderer>();
            shipRenderer.sprite = ship;
            shipRenderer.sortingOrder = -20;
            shipRenderer.drawMode = SpriteDrawMode.Simple;

            GameObject walkableGo = new GameObject("WalkableArea");
            walkableGo.transform.SetParent(parent, false);
            ShipWalkableArea walkable = walkableGo.AddComponent<ShipWalkableArea>();
            walkable.SetZones(BuildZones());

            Sprite circle = theme.circle;
            AddWorldCircle(parent, "NpcRing", circle, new Color(0.10f, 0.20f, 0.28f, 0.65f), new Vector3(0.9f, 0.05f, 0f), 1.45f, 18);
            AddWorldCircle(parent, "ConsoleRing", circle, new Color(0.10f, 0.20f, 0.28f, 0.65f), new Vector3(0f, 3.55f, 0f), 1.2f, 18);
            AddWorldLabel(parent, "SHIELD CONTROL", new Vector3(0f, 4.35f, 0f), Color.white, 2.1f);
            AddWorldLabel(parent, "SAFETY OFFICER", new Vector3(0.9f, 1.15f, 0f), Color.white, 1.8f);
            AddWorldLabel(parent, "PLANT LAB", new Vector3(-5.3f, 4.3f, 0f), new Color(0.9f, 0.95f, 1f, 0.9f), 1.7f);
            AddWorldLabel(parent, "ROCKET BAY", new Vector3(5.2f, 4.2f, 0f), new Color(0.9f, 0.95f, 1f, 0.9f), 1.7f);
            AddWorldLabel(parent, "BIODOME", new Vector3(-5.2f, 0.25f, 0f), new Color(0.9f, 0.95f, 1f, 0.9f), 1.7f);
            AddWorldLabel(parent, "SPACE LAB", new Vector3(5.2f, 0.25f, 0f), new Color(0.9f, 0.95f, 1f, 0.9f), 1.7f);
            AddWorldLabel(parent, "CREW HUB", new Vector3(0f, -1.45f, 0f), new Color(0.9f, 0.95f, 1f, 0.8f), 1.5f);

            return walkable;
        }

        private static RadiationManager BuildHazard(RadiationConfig config, ShipWalkableArea walkable)
        {
            GameObject go = new GameObject("SimulatedHazard");
            RadiationManager manager = go.AddComponent<RadiationManager>();
            manager.config = config;
            manager.walkableArea = walkable;

            GameObject visualGo = new GameObject("WarningVisual", typeof(SpriteRenderer), typeof(RadiationVisual));
            visualGo.transform.SetParent(go.transform, false);
            var visual = visualGo.GetComponent<RadiationVisual>();
            visual.SetColor(config.hazardColor);
            var spriteRenderer = visualGo.GetComponent<SpriteRenderer>();
            spriteRenderer.color = config.hazardColor;
            manager.visual = visual;
            return manager;
        }

        private static PlayerController2D BuildPlayer(RadiationConfig config, ShipWalkableArea walkable)
        {
            Sprite[] sprites = LoadAllSprites(AstronautPath);
            Sprite body = FindSprite(sprites, "10588_1");
            if (body == null && sprites.Length > 0)
            {
                body = sprites[0];
            }

            GameObject go = new GameObject("Player", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D));
            go.transform.position = new Vector3(0f, -0.55f, 0f);
            go.transform.localScale = Vector3.one * 0.12f;
            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = body;
            renderer.sortingOrder = 20;
            renderer.color = Color.white;

            var body2D = go.GetComponent<Rigidbody2D>();
            body2D.bodyType = RigidbodyType2D.Dynamic;
            body2D.gravityScale = 0f;
            body2D.freezeRotation = true;
            body2D.interpolation = RigidbodyInterpolation2D.Interpolate;
            body2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var collider = go.GetComponent<CircleCollider2D>();
            collider.radius = 0.2f / 0.12f;

            var controller = go.AddComponent<PlayerController2D>();
            controller.walkableArea = walkable;
            controller.moveSpeed = 2.8f;
            controller.bodyRadius = 0.2f;

            go.AddComponent<PlayerInteractor>();
            go.AddComponent<PlayerRadiationExposure>();
            return controller;
        }

        private static NpcGuide BuildNpc(RadiationConfig config)
        {
            Sprite[] sprites = LoadAllSprites(AstronautPath);
            Sprite body = FindSprite(sprites, "10588_3");
            if (body == null && sprites.Length > 0)
            {
                body = sprites[0];
            }

            GameObject go = new GameObject("SafetyOfficer", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(NpcGuide));
            go.transform.position = new Vector3(0.9f, 0.05f, 0f);
            go.transform.localScale = Vector3.one * 0.12f;
            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = body;
            renderer.sortingOrder = 21;

            var collider = go.GetComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.65f / 0.12f;

            var npc = go.GetComponent<NpcGuide>();
            npc.config = config;
            npc.SetPrompt(config.npcPrompt);
            return npc;
        }

        private static ControlPanel BuildControlPanel(RadiationConfig config, RadiationManager hazard, UiTheme theme)
        {
            GameObject go = new GameObject("ShieldControl", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(ControlPanel));
            go.transform.position = new Vector3(0f, 3.55f, 0f);
            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = theme.roundedPanel;
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.size = new Vector2(1.5f, 0.8f);
            renderer.sortingOrder = 16;
            renderer.color = new Color(0.13f, 0.24f, 0.32f, 1f);

            var collider = go.GetComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(1.7f, 1f);

            var control = go.GetComponent<ControlPanel>();
            control.config = config;
            control.manager = hazard;

            GameObject lampGo = new GameObject("IndicatorLamp", typeof(SpriteRenderer));
            lampGo.transform.SetParent(go.transform, false);
            lampGo.transform.localPosition = new Vector3(0.55f, 0.24f, 0f);
            lampGo.transform.localScale = Vector3.one * 0.22f;
            var lamp = lampGo.GetComponent<SpriteRenderer>();
            lamp.sprite = theme.circle;
            lamp.sortingOrder = 17;
            control.indicatorLamp = lamp;
            return control;
        }

        private static Transform BuildRespawnPoint()
        {
            GameObject go = new GameObject("Respawn_CrewHub");
            go.transform.position = new Vector3(0f, -0.65f, 0f);
            return go.transform;
        }

        private static RadiationSpawnPoint[] BuildSpawnPoints()
        {
            return new[]
            {
                NewSpawn("PlantLab", new Vector3(-5.5f, 3.15f, 0f)),
                NewSpawn("RocketBay", new Vector3(5f, 3f, 0f)),
                NewSpawn("Biodome", new Vector3(-5.4f, -0.8f, 0f)),
                NewSpawn("SpaceLab", new Vector3(5f, -0.8f, 0f))
            };
        }

        private static RadiationSpawnPoint NewSpawn(string id, Vector3 position)
        {
            GameObject go = new GameObject("Spawn_" + id);
            go.transform.position = position;
            var point = go.AddComponent<RadiationSpawnPoint>();
            point.id = id;
            point.clearance = 0.35f;
            return point;
        }

        private static Rect[] BuildZones()
        {
            return new[]
            {
                new Rect(-2.35f, -1.65f, 4.7f, 3.45f),   // crew hub
                new Rect(-0.75f, 1.0f, 1.5f, 2.3f),     // north corridor
                new Rect(-4.6f, 1.0f, 3.0f, 2.6f),      // west upper corridor
                new Rect(1.6f, 1.0f, 3.0f, 2.6f),       // east upper corridor
                new Rect(-6.85f, 1.8f, 3.1f, 2.7f),     // plant lab
                new Rect(-1.75f, 2.6f, 3.5f, 2.3f),     // reactor / control room
                new Rect(3.75f, 1.8f, 3.0f, 2.7f),      // rocket bay
                new Rect(-4.6f, -1.6f, 3.0f, 2.1f),     // west lower corridor
                new Rect(1.6f, -1.6f, 3.0f, 2.1f),      // east lower corridor
                new Rect(-6.95f, -2.0f, 3.1f, 2.5f),    // biodome
                new Rect(3.85f, -2.0f, 3.1f, 2.5f),     // space lab
                new Rect(-0.7f, -3.4f, 1.4f, 2.5f),     // south corridor
                new Rect(-2.3f, -4.5f, 4.6f, 1.8f)      // lockers
            };
        }

        private static void BuildUi(
            Transform safe,
            MissionConfig mission,
            UiTheme theme,
            RadiationConfig config,
            RadiationManager hazard,
            PlayerController2D player,
            NpcGuide npc,
            ControlPanel control,
            Transform respawn,
            out DialoguePanel dialogue,
            out RadiationGameController controller)
        {
            GameObject topBar = UiBuild.NewUi("TopBar", safe);
            RectTransform topRect = UiBuild.Rect(topBar);
            topRect.anchorMin = new Vector2(0f, 1f);
            topRect.anchorMax = new Vector2(1f, 1f);
            topRect.pivot = new Vector2(0.5f, 1f);
            topRect.anchoredPosition = Vector2.zero;
            topRect.sizeDelta = new Vector2(0f, 92f);
            UiBuild.AddPanel(topBar, PanelRole.Panel, false, theme);

            TextMeshProUGUI title = UiBuild.Text(topBar.transform, "Title", config.screenTitle, TextRole.Body,
                TextAlignmentOptions.Left, theme);
            title.fontStyle = FontStyles.Bold;
            UiBuild.TopLeft(title.rectTransform, 30f, 2f, 700f, 48f);

            TextMeshProUGUI tagline = UiBuild.Text(topBar.transform, "Tagline", config.tagline, TextRole.Caption,
                TextAlignmentOptions.Left, theme, true);
            UiBuild.TopLeft(tagline.rectTransform, 32f, 52f, 820f, 34f);

            TextMeshProUGUI returnTopLabel;
            Button returnTop = ButtonAt(topBar.transform, "ReturnTop", "Return to Ship", 30f, 18f, 220f, 54f, false, theme, out returnTopLabel, true);
            TextMeshProUGUI pauseLabel;
            Button pause = ButtonAt(topBar.transform, "Pause", "Pause", 264f, 18f, 128f, 54f, false, theme, out pauseLabel, true);

            GameObject detectorPanel = PanelAt(safe, "DetectorPanel", 24f, 108f, 400f, 176f, PanelRole.Panel, theme);
            Image pill = UiBuild.AddImage(UiBuild.NewUi("StatusPill", detectorPanel.transform), config.safeColor, theme.roundedPill, true, false);
            UiBuild.TopLeft(UiBuild.Rect(pill.gameObject), 46f, 16f, 140f, 38f);
            TextMeshProUGUI status = UiBuild.Text(detectorPanel.transform, "Status", config.detectorSafe, TextRole.Caption,
                TextAlignmentOptions.Center, theme);
            UiBuild.TopLeft(status.rectTransform, 46f, 18f, 140f, 34f);
            status.fontStyle = FontStyles.Bold;

            TextMeshProUGUI readout = UiBuild.Text(detectorPanel.transform, "Readout", "0 / 100", TextRole.Body,
                TextAlignmentOptions.MidlineRight, theme);
            readout.fontStyle = FontStyles.Bold;
            UiBuild.TopLeft(readout.rectTransform, 194f, 8f, 180f, 48f);

            TextMeshProUGUI detectorCaption = UiBuild.Text(detectorPanel.transform, "Caption", config.detectorCaption,
                TextRole.Caption, TextAlignmentOptions.Left, theme, true);
            UiBuild.TopLeft(detectorCaption.rectTransform, 46f, 60f, 320f, 34f);

            TextMeshProUGUI exposureCaption = UiBuild.Text(detectorPanel.transform, "ExposureCaption", config.exposureCaption,
                TextRole.Caption, TextAlignmentOptions.Left, theme, true);
            UiBuild.TopLeft(exposureCaption.rectTransform, 46f, 96f, 240f, 34f);

            TextMeshProUGUI exposureLabel = UiBuild.Text(detectorPanel.transform, "ExposureLabel", "0%", TextRole.Body,
                TextAlignmentOptions.Right, theme);
            UiBuild.TopLeft(exposureLabel.rectTransform, 292f, 92f, 70f, 48f);

            GameObject barGo = UiBuild.NewUi("ExposureBar", detectorPanel.transform);
            UiBuild.TopLeft(UiBuild.Rect(barGo), 46f, 142f, 316f, 14f);
            UiBuild.AddImage(barGo, theme.panelAlt, theme.roundedPill, true);
            GameObject fillGo = UiBuild.NewUi("Fill", barGo.transform);
            UiBuild.Stretch(UiBuild.Rect(fillGo));
            Image fill = UiBuild.AddImage(fillGo, theme.primary, theme.roundedPill, true);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.fillAmount = 0f;

            GameObject objectivePanel = PanelAt(safe, "ObjectivePanel", 940f, 108f, 640f, 156f, PanelRole.Panel, theme);
            TextMeshProUGUI objectiveHeader = UiBuild.Text(objectivePanel.transform, "Header", config.objectiveLabel,
                TextRole.Caption, TextAlignmentOptions.Left, theme, true);
            UiBuild.TopLeft(objectiveHeader.rectTransform, 26f, 10f, 400f, 36f);
            TextMeshProUGUI objective = UiBuild.Text(objectivePanel.transform, "Objective", config.objectiveTalk,
                TextRole.Body, TextAlignmentOptions.TopLeft, theme);
            UiBuild.TopLeft(objective.rectTransform, 26f, 50f, 588f, 96f);
            objective.textWrappingMode = TextWrappingModes.Normal;

            GameObject legendPanel = PanelAt(safe, "LegendPanel", 460f, 12f, 1000f, 84f, PanelRole.Panel, theme);
            UiBuild.BottomLeft(UiBuild.Rect(legendPanel), 460f, 12f, 1000f, 84f);
            TextMeshProUGUI legend = UiBuild.Text(legendPanel.transform, "Legend",
                config.legend, TextRole.Caption, TextAlignmentOptions.Center, theme, true);
            UiBuild.Stretch(legend.rectTransform, 18f, 6f, 18f, 6f);
            legend.textWrappingMode = TextWrappingModes.Normal;

            Image joystickImage = UiBuild.AddImage(UiBuild.NewUi("Joystick", safe), theme.panelAlt, theme.circle, false, true);
            UiBuild.BottomLeft(UiBuild.Rect(joystickImage.gameObject), 40f, 34f, 230f, 230f);
            var joystick = joystickImage.gameObject.AddComponent<TouchJoystick>();
            joystick.background = joystickImage.rectTransform;

            Image knob = UiBuild.AddImage(UiBuild.NewUi("Knob", joystickImage.transform), theme.primary, theme.circle, false, false);
            UiBuild.Center(knob.rectTransform, 0f, 0f, 92f, 92f);
            joystick.knob = knob.rectTransform;

            TextMeshProUGUI moveLabel = UiBuild.Text(safe, "MoveLabel", "MOVE", TextRole.Caption, TextAlignmentOptions.Center, theme, true);
            UiBuild.BottomLeft(moveLabel.rectTransform, 40f, 266f, 230f, 34f);

            Button interact = ButtonAt(safe, "Interact", config.npcPrompt, 40f, 44f, 330f, 86f, true, theme, out TextMeshProUGUI interactLabel, false);
            UiBuild.BottomRight(interact.GetComponent<RectTransform>(), 40f, 44f, 330f, 86f);
            interact.gameObject.SetActive(false);

            GameObject toastRoot = PanelAt(safe, "Toast", 400f, 268f, 1120f, 78f, PanelRole.Warning, theme);
            TextMeshProUGUI toast = UiBuild.Text(toastRoot.transform, "ToastText", "", TextRole.Body,
                TextAlignmentOptions.Center, theme);
            UiBuild.Stretch(toast.rectTransform, 20f, 8f, 20f, 8f);
            toast.textWrappingMode = TextWrappingModes.Normal;
            var toastText = toast.GetComponent<ThemedText>();
            if (toastText != null)
            {
                toastText.overrideColor = true;
                toastText.colorOverride = theme.background;
            }

            toastRoot.SetActive(false);

            dialogue = BuildDialogue(safe, theme, config);
            GameObject pauseRoot = BuildPause(safe, theme, out Button resume, out Button restart, out Button returnButton);
            GameObject introRoot = BuildIntro(safe, theme, config, out Button start, out Button introReturn,
                out TextMeshProUGUI introTitle, out TextMeshProUGUI introBody);

            ActivityResultPanel result = InstantiatePrefab<ActivityResultPanel>("ActivityResultPanel.prefab", canvasRoot(safe));
            ConfirmDialog dialog = InstantiatePrefab<ConfirmDialog>("ConfirmDialog.prefab", canvasRoot(safe));

            GameObject controllerGo = UiBuild.FullScreen(canvasRoot(safe), "RadiationController");
            controller = controllerGo.AddComponent<RadiationGameController>();
            controller.config = config;
            controller.hazard = hazard;
            controller.player = player;
            controller.exposure = player.GetComponent<PlayerRadiationExposure>();
            controller.interactor = player.GetComponent<PlayerInteractor>();
            controller.detector = detectorPanel.AddComponent<Dosimeter>();
            controller.detector.config = config;
            controller.detector.manager = hazard;
            controller.detector.player = player.transform;
            controller.detector.readoutText = readout;
            controller.detector.statusText = status;
            controller.detector.statusPill = pill;
            controller.npc = npc;
            controller.panel = control;
            controller.joystick = joystick;
            controller.respawnPoint = respawn;
            controller.introRoot = introRoot;
            controller.startButton = start;
            controller.introReturnButton = introReturn;
            controller.introTitleLabel = introTitle;
            controller.introInstructionsLabel = introBody;
            controller.taglineLabel = tagline;
            controller.objectiveHeaderLabel = objectiveHeader;
            controller.objectiveLabel = objective;
            controller.legendLabel = legend;
            controller.detectorCaptionLabel = detectorCaption;
            controller.exposureCaptionLabel = exposureCaption;
            controller.pauseRoot = pauseRoot;
            controller.pauseButton = pause;
            controller.resumeButton = resume;
            controller.restartButton = restart;
            controller.returnButton = returnButton;
            controller.returnTopButton = returnTop;
            controller.toastRoot = toastRoot;
            controller.toastLabel = toast;
            controller.interactButton = interact;
            controller.interactLabel = interactLabel;
            controller.resultPanel = result;
            controller.dialog = dialog;

            player.joystick = joystick;
            player.GetComponent<PlayerInteractor>().promptText = null;
            var exposure = player.GetComponent<PlayerRadiationExposure>();
            exposure.config = config;
            exposure.manager = hazard;
            exposure.exposureFill = fill;
            exposure.exposureLabel = exposureLabel;

            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(joystick);
            EditorUtility.SetDirty(exposure);
        }

        private static DialoguePanel BuildDialogue(Transform safe, UiTheme theme, RadiationConfig config)
        {
            GameObject root = UiBuild.NewUi("Dialogue", safe);
            UiBuild.Stretch(UiBuild.Rect(root));
            UiBuild.AddImage(root, Dim, null, false, true);
            var panel = root.AddComponent<DialoguePanel>();
            panel.config = config;

            GameObject card = PanelAt(root.transform, "DialogueCard", 0f, 0f, 1500f, 400f, PanelRole.Panel, theme);
            UiBuild.Center(UiBuild.Rect(card), 0f, -250f, 1500f, 400f);

            TextMeshProUGUI speaker = UiBuild.Text(card.transform, "Speaker", config.npcName, TextRole.Heading,
                TextAlignmentOptions.Left, theme);
            UiBuild.TopLeft(speaker.rectTransform, 28f, 12f, 700f, 76f);

            TextMeshProUGUI page = UiBuild.Text(card.transform, "Page", "1 / 5", TextRole.Caption,
                TextAlignmentOptions.Right, theme, true);
            UiBuild.TopLeft(page.rectTransform, 1120f, 24f, 340f, 34f);

            TextMeshProUGUI body = UiBuild.Text(card.transform, "Body", "", TextRole.Body,
                TextAlignmentOptions.TopLeft, theme);
            UiBuild.TopLeft(body.rectTransform, 28f, 92f, 1444f, 210f);
            body.textWrappingMode = TextWrappingModes.Normal;

            TextMeshProUGUI hint = UiBuild.Text(card.transform, "Hint", config.dialogueHint, TextRole.Caption,
                TextAlignmentOptions.Left, theme, true);
            UiBuild.TopLeft(hint.rectTransform, 28f, 326f, 900f, 34f);

            Button close = ButtonAt(card.transform, "Close", config.dialogueClose, 1040f, 312f, 180f, 64f, false, theme, out TextMeshProUGUI closeLabel, false);
            Button next = ButtonAt(card.transform, "Continue", config.dialogueContinue, 1236f, 312f, 236f, 64f, true, theme, out TextMeshProUGUI nextLabel, false);

            panel.speakerText = speaker;
            panel.bodyText = body;
            panel.hintText = hint;
            panel.pageText = page;
            panel.continueLabel = nextLabel;
            panel.closeLabel = closeLabel;
            panel.continueButton = next;
            panel.closeButton = close;
            root.SetActive(false);
            return panel;
        }

        private static GameObject BuildPause(Transform safe, UiTheme theme, out Button resume, out Button restart, out Button returnButton)
        {
            GameObject root = UiBuild.NewUi("Pause", safe);
            UiBuild.Stretch(UiBuild.Rect(root));
            UiBuild.AddImage(root, Dim, null, false, true);

            GameObject card = PanelAt(root.transform, "PauseCard", 0f, 0f, 760f, 540f, PanelRole.Panel, theme);
            UiBuild.Center(UiBuild.Rect(card), 0f, 0f, 760f, 540f);

            TextMeshProUGUI title = UiBuild.Text(card.transform, "Title", "Drill paused", TextRole.Heading,
                TextAlignmentOptions.Center, theme);
            UiBuild.TopLeft(title.rectTransform, 40f, 18f, 680f, 76f);
            TextMeshProUGUI body = UiBuild.Text(card.transform, "Body",
                "The drill is paused. Take a breath.", TextRole.Body, TextAlignmentOptions.Center, theme, true);
            UiBuild.TopLeft(body.rectTransform, 60f, 102f, 640f, 80f);

            resume = ButtonAt(card.transform, "Resume", "Resume", 190f, 202f, 380f, 70f, true, theme, out _, false);
            restart = ButtonAt(card.transform, "Restart", "Restart Drill", 190f, 294f, 380f, 70f, false, theme, out _, false);
            returnButton = ButtonAt(card.transform, "Return", "Return to Ship", 190f, 386f, 380f, 70f, false, theme, out _, false);

            root.SetActive(false);
            return root;
        }

        private static GameObject BuildIntro(
            Transform safe,
            UiTheme theme,
            RadiationConfig config,
            out Button start,
            out Button introReturn,
            out TextMeshProUGUI titleText,
            out TextMeshProUGUI bodyText)
        {
            GameObject root = UiBuild.NewUi("Intro", safe);
            UiBuild.Stretch(UiBuild.Rect(root));
            UiBuild.AddImage(root, Dim, null, false, true);

            GameObject card = PanelAt(root.transform, "IntroCard", 0f, 0f, 1180f, 720f, PanelRole.Panel, theme);
            UiBuild.Center(UiBuild.Rect(card), 0f, 0f, 1180f, 720f);

            titleText = UiBuild.Text(card.transform, "Title", config.screenTitle, TextRole.Heading,
                TextAlignmentOptions.Left, theme);
            UiBuild.TopLeft(titleText.rectTransform, 60f, 28f, 1060f, 72f);

            TextMeshProUGUI tagline = UiBuild.Text(card.transform, "Tagline", config.tagline, TextRole.Body,
                TextAlignmentOptions.Left, theme, true);
            UiBuild.TopLeft(tagline.rectTransform, 60f, 104f, 1060f, 50f);

            bodyText = UiBuild.Text(card.transform, "Body", config.instructionBody, TextRole.Body,
                TextAlignmentOptions.TopLeft, theme);
            UiBuild.TopLeft(bodyText.rectTransform, 60f, 162f, 1060f, 412f);
            bodyText.textWrappingMode = TextWrappingModes.Normal;

            TextMeshProUGUI note = UiBuild.Text(card.transform, "Note",
                "Practice drill only. No real radiation measurements are shown.", TextRole.Caption,
                TextAlignmentOptions.Left, theme, true);
            UiBuild.TopLeft(note.rectTransform, 60f, 586f, 1060f, 34f);

            start = ButtonAt(card.transform, "Start", config.startButtonLabel, 60f, 628f, 300f, 68f, true, theme, out _, false);
            introReturn = ButtonAt(card.transform, "IntroReturn", "Return to Ship", 384f, 628f, 300f, 68f, false, theme, out _, false);
            root.SetActive(true);
            return root;
        }

        private static Button ButtonAt(
            Transform parent,
            string name,
            string label,
            float x,
            float y,
            float width,
            float height,
            bool primary,
            UiTheme theme,
            out TextMeshProUGUI labelText,
            bool topRight)
        {
            GameObject go = UiBuild.NewUi(name, parent);
            var button = UiBuild.AddButton(go, label, primary, theme, out TextMeshProUGUI tmp);
            labelText = tmp;
            if (topRight)
            {
                UiBuild.TopRight(UiBuild.Rect(go), x, y, width, height);
            }
            else
            {
                UiBuild.TopLeft(UiBuild.Rect(go), x, y, width, height);
            }

            return button;
        }

        private static GameObject PanelAt(
            Transform parent,
            string name,
            float x,
            float y,
            float width,
            float height,
            PanelRole role,
            UiTheme theme)
        {
            GameObject go = UiBuild.NewUi(name, parent);
            UiBuild.TopLeft(UiBuild.Rect(go), x, y, width, height);
            UiBuild.AddPanel(go, role, true, theme);
            return go;
        }

        private static void AddWorldCircle(
            Transform parent,
            string name,
            Sprite sprite,
            Color color,
            Vector3 position,
            float scale,
            int sortingOrder)
        {
            GameObject go = new GameObject(name, typeof(SpriteRenderer));
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = Vector3.one * scale;
            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
        }

        private static void AddWorldLabel(Transform parent, string text, Vector3 position, Color color, float fontSize)
        {
            GameObject go = new GameObject("Label " + text, typeof(TextMeshPro));
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 0.12f;
            var label = go.GetComponent<TextMeshPro>();
            label.text = text;
            label.font = UiThemeService.Font != null ? UiThemeService.Font : TMP_Settings.defaultFontAsset;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            label.sortingOrder = 40;
            label.rectTransform.sizeDelta = new Vector2(8f, 1.5f);
        }

        private static T InstantiatePrefab<T>(string prefabName, Transform parent) where T : Component
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(MissionUiBuilder.UiPrefabRoot + "/" + prefabName);
            if (asset == null)
            {
                throw new InvalidOperationException("Missing UI prefab: " + prefabName);
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
            T component = instance.GetComponent<T>();
            if (component == null)
            {
                throw new InvalidOperationException("Prefab " + prefabName + " does not have " + typeof(T).Name + ".");
            }

            instance.SetActive(false);
            return component;
        }

        private static Transform canvasRoot(Transform safe)
        {
            return safe.parent;
        }

        private static Sprite LoadFirstSprite(string path)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (UnityEngine.Object asset in assets)
            {
                if (asset is Sprite sprite)
                {
                    return sprite;
                }
            }

            return null;
        }

        private static Sprite[] LoadAllSprites(string path)
        {
            var sprites = new List<Sprite>();
            foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is Sprite sprite)
                {
                    sprites.Add(sprite);
                }
            }

            return sprites.ToArray();
        }

        private static Sprite FindSprite(Sprite[] sprites, string name)
        {
            foreach (Sprite sprite in sprites)
            {
                if (sprite.name == name)
                {
                    return sprite;
                }
            }

            return null;
        }
    }
}
