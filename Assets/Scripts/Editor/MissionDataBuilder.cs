using System;
using System.IO;
using MarsOutpost.Core;
using MarsOutpost.Greenhouse;
using MarsOutpost.Landing;
using MarsOutpost.Weather;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace MarsOutpost.EditorTools
{
    /// <summary>Creates the first-run data assets. Safe to run only once; it never overwrites.</summary>
    public static class MissionDataBuilder
    {
        public const string ConfigDir = "Assets/Data/Configs";
        public const string DataRoot = "Assets/Data";
        public const string MissionConfigPath = ConfigDir + "/MissionConfig.asset";

        public sealed class Content
        {
            public MissionConfig Config;
            public UiTheme Theme;
            public UiTheme HighContrastTheme;
            public ResourceDefinition Power;
            public ResourceDefinition Safety;
            public ResourceDefinition Science;
            public ActivityDefinition Landing;
            public ActivityDefinition Greenhouse;
            public ActivityDefinition Weather;
            public PlaceholderActivityOutcome WeatherOutcome;
            public LandingMissionConfig LandingConfig;
            public GreenhouseExperimentConfig GreenhouseConfig;
            public SpaceWeatherConfig WeatherConfig;
            public DebriefConfig Debrief;
            public GeneratedSpriteBuilder.Sprites Sprites;
        }

        public static Content Build(GeneratedSpriteBuilder.Sprites sprites)
        {
            GeneratedSpriteBuilder.EnsureFolder(DataRoot);
            GeneratedSpriteBuilder.EnsureFolder(ConfigDir);
            GeneratedSpriteBuilder.EnsureFolder("Assets/Data/Events");
            GeneratedSpriteBuilder.EnsureFolder("Assets/Data/Studies");
            GeneratedSpriteBuilder.EnsureFolder("Assets/Data/Landing");
            GeneratedSpriteBuilder.EnsureFolder("Assets/Data/Greenhouse");
            GeneratedSpriteBuilder.EnsureFolder("Assets/Data/Weather");

            TMP_FontAsset font = TmpEssentials.GetDefaultFont();
            if (font == null)
            {
                throw new InvalidOperationException(
                    "TMP Essential Resources are not imported. Run Tools > Mars Outpost > Import TMP Essentials first.");
            }

            UiTheme theme = CreateOrLoad<UiTheme>(ConfigDir + "/UiTheme.asset");
            ApplyBaseTheme(theme, font, sprites);

            UiTheme highContrast = CreateOrLoad<UiTheme>(ConfigDir + "/UiTheme_HighContrast.asset");
            ApplyHighContrastTheme(highContrast, font, sprites);

            ResourceDefinition power = CreateOrLoad<ResourceDefinition>(ConfigDir + "/Resource_Power.asset");
            ApplyResource(power, "power", "Power", "PWR", "Power runs the outpost. Activities use it.",
                new Color(1f, 0.72f, 0.16f), 100, 100);

            ResourceDefinition safety = CreateOrLoad<ResourceDefinition>(ConfigDir + "/Resource_Safety.asset");
            ApplyResource(safety, "safety", "Crew Safety", "CREW", "Crew Safety shows how protected the crew is.",
                new Color(0.29f, 0.79f, 0.94f), 100, 100);

            ResourceDefinition science = CreateOrLoad<ResourceDefinition>(ConfigDir + "/Resource_Science.asset");
            ApplyResource(science, "science", "Science Progress", "SCI", "Science Progress grows when you collect data.",
                new Color(0.45f, 0.89f, 0.53f), 0, 100);

            ActivityDefinition landing = CreateOrLoad<ActivityDefinition>(ConfigDir + "/Activity_Landing.asset");
            ApplyLanding(landing);

            ActivityDefinition greenhouse = CreateOrLoad<ActivityDefinition>(ConfigDir + "/Activity_Greenhouse.asset");
            ApplyGreenhouse(greenhouse);

            ActivityDefinition weather = CreateOrLoad<ActivityDefinition>(ConfigDir + "/Activity_Weather.asset");
            ApplyWeather(weather);

            LandingMissionConfig landingConfig = CreateOrLoad<LandingMissionConfig>(
                "Assets/Data/Landing/LandingConfig.asset");
            LandingSiteDefinition auroraFlats = CreateOrLoad<LandingSiteDefinition>(
                "Assets/Data/Landing/Site_AuroraFlats.asset");
            LandingSiteDefinition rustValley = CreateOrLoad<LandingSiteDefinition>(
                "Assets/Data/Landing/Site_RustValley.asset");
            LandingSiteDefinition pioneerMesa = CreateOrLoad<LandingSiteDefinition>(
                "Assets/Data/Landing/Site_PioneerMesa.asset");
            ApplyAuroraFlats(auroraFlats);
            ApplyRustValley(rustValley);
            ApplyPioneerMesa(pioneerMesa);
            ApplyLandingConfig(landingConfig, auroraFlats, rustValley, pioneerMesa);

            DebriefConfig debrief = CreateOrLoad<DebriefConfig>(ConfigDir + "/DebriefConfig.asset");
            ApplyDebrief(debrief);

            GreenhouseExperimentConfig greenhouseConfig = CreateOrLoad<GreenhouseExperimentConfig>(
                "Assets/Data/Greenhouse/GreenhouseConfig.asset");
            ApplyGreenhouseConfig(greenhouseConfig);

            SpaceWeatherConfig weatherConfig = CreateOrLoad<SpaceWeatherConfig>(
                "Assets/Data/Weather/SpaceWeatherConfig.asset");
            ApplySpaceWeatherConfig(weatherConfig);

            PlaceholderActivityOutcome weatherOutcome = CreateOrLoad<PlaceholderActivityOutcome>(
                ConfigDir + "/Placeholder_Weather.asset");
            ApplyWeatherOutcome(weatherOutcome);

            MissionConfig config = CreateOrLoad<MissionConfig>(MissionConfigPath);
            ApplyMissionConfig(config, power, safety, science, landing, greenhouse, weather, debrief, theme, highContrast);

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();

            foreach (string error in MissionDataValidator.Validate(config))
            {
                Debug.LogError($"[MarsOutpost] Mission data: {error}");
            }

            foreach (string error in LandingDataValidator.Validate(landingConfig))
            {
                Debug.LogError($"[MarsOutpost] Landing data: {error}");
            }

            foreach (string error in GreenhouseDataValidator.Validate(greenhouseConfig))
            {
                Debug.LogError($"[MarsOutpost] Greenhouse data: {error}");
            }

            foreach (string error in SpaceWeatherDataValidator.Validate(weatherConfig))
            {
                Debug.LogError($"[MarsOutpost] Space weather data: {error}");
            }

            return new Content
            {
                Config = config,
                Theme = theme,
                HighContrastTheme = highContrast,
                Power = power,
                Safety = safety,
                Science = science,
                Landing = landing,
                Greenhouse = greenhouse,
                Weather = weather,
                WeatherOutcome = weatherOutcome,
                LandingConfig = landingConfig,
                GreenhouseConfig = greenhouseConfig,
                WeatherConfig = weatherConfig,
                Debrief = debrief,
                Sprites = sprites
            };
        }

        public static Content BuildSpaceWeather(GeneratedSpriteBuilder.Sprites sprites)
        {
            GeneratedSpriteBuilder.EnsureFolder(DataRoot);
            GeneratedSpriteBuilder.EnsureFolder(ConfigDir);
            GeneratedSpriteBuilder.EnsureFolder("Assets/Data/Weather");

            MissionConfig config = AssetDatabase.LoadAssetAtPath<MissionConfig>(MissionConfigPath);
            if (config == null)
            {
                throw new InvalidOperationException(
                    "MissionConfig is missing. Run the full Phase 1 build once before the targeted weather build.");
            }

            UiTheme theme = config.theme;
            if (theme == null)
            {
                theme = AssetDatabase.LoadAssetAtPath<UiTheme>(ConfigDir + "/UiTheme.asset");
            }

            if (theme == null)
            {
                throw new InvalidOperationException("The UI theme asset is missing.");
            }

            ActivityDefinition weather = CreateOrLoad<ActivityDefinition>(ConfigDir + "/Activity_Weather.asset");
            ApplyWeather(weather);

            SpaceWeatherConfig weatherConfig = CreateOrLoad<SpaceWeatherConfig>(
                "Assets/Data/Weather/SpaceWeatherConfig.asset");
            ApplySpaceWeatherConfig(weatherConfig);

            EditorUtility.SetDirty(weather);
            EditorUtility.SetDirty(weatherConfig);
            AssetDatabase.SaveAssets();

            foreach (string error in SpaceWeatherDataValidator.Validate(weatherConfig))
            {
                Debug.LogError($"[MarsOutpost] Space weather data: {error}");
            }

            return new Content
            {
                Config = config,
                Theme = theme,
                Weather = weather,
                WeatherConfig = weatherConfig,
                Sprites = sprites
            };
        }

        private static T CreateOrLoad<T>(string path) where T : ScriptableObject
        {
            T existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                return existing;
            }

            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void ApplyBaseTheme(UiTheme theme, TMP_FontAsset font, GeneratedSpriteBuilder.Sprites sprites)
        {
            theme.background = new Color(0.043f, 0.063f, 0.149f, 1f);
            theme.backgroundAccent = new Color(0.09f, 0.12f, 0.26f, 1f);
            theme.panel = new Color(0.082f, 0.11f, 0.23f, 1f);
            theme.panelAlt = new Color(0.118f, 0.153f, 0.314f, 1f);
            theme.primary = new Color(1f, 0.718f, 0.012f, 1f);
            theme.primaryText = new Color(0.09f, 0.07f, 0.02f, 1f);
            theme.textPrimary = new Color(0.96f, 0.972f, 1f, 1f);
            theme.textSecondary = new Color(0.663f, 0.702f, 0.839f, 1f);
            theme.success = new Color(0.29f, 0.871f, 0.502f, 1f);
            theme.warning = new Color(0.984f, 0.749f, 0.141f, 1f);
            theme.danger = new Color(0.973f, 0.443f, 0.443f, 1f);
            theme.focusRing = new Color(0.56f, 0.4f, 1f, 1f);
            theme.font = font;
            theme.titleSize = 96f;
            theme.headingSize = 46f;
            theme.bodySize = 28f;
            theme.buttonSize = 32f;
            theme.captionSize = 21f;
            theme.roundedPanel = sprites.RoundedPanel;
            theme.roundedButton = sprites.RoundedButton;
            theme.roundedPill = sprites.RoundedPill;
            theme.circle = sprites.Circle;
            EditorUtility.SetDirty(theme);
        }

        private static void ApplyHighContrastTheme(UiTheme theme, TMP_FontAsset font, GeneratedSpriteBuilder.Sprites sprites)
        {
            theme.background = Color.black;
            theme.backgroundAccent = new Color(0.06f, 0.06f, 0.06f, 1f);
            theme.panel = new Color(0.1f, 0.1f, 0.1f, 1f);
            theme.panelAlt = new Color(0.16f, 0.16f, 0.16f, 1f);
            theme.primary = new Color(1f, 0.9f, 0.1f, 1f);
            theme.primaryText = Color.black;
            theme.textPrimary = Color.white;
            theme.textSecondary = new Color(0.92f, 0.92f, 0.92f, 1f);
            theme.success = new Color(0.3f, 1f, 0.4f, 1f);
            theme.warning = new Color(1f, 0.85f, 0.2f, 1f);
            theme.danger = new Color(1f, 0.35f, 0.35f, 1f);
            theme.focusRing = Color.white;
            theme.font = font;
            theme.titleSize = 96f;
            theme.headingSize = 46f;
            theme.bodySize = 28f;
            theme.buttonSize = 32f;
            theme.captionSize = 21f;
            theme.roundedPanel = sprites.RoundedPanel;
            theme.roundedButton = sprites.RoundedButton;
            theme.roundedPill = sprites.RoundedPill;
            theme.circle = sprites.Circle;
            EditorUtility.SetDirty(theme);
        }

        private static void ApplyResource(
            ResourceDefinition resource,
            string id,
            string displayName,
            string shortLabel,
            string explanation,
            Color color,
            int startingValue,
            int maxValue)
        {
            resource.id = id;
            resource.displayName = displayName;
            resource.shortLabel = shortLabel;
            resource.kidExplanation = explanation;
            resource.color = color;
            resource.startingValue = startingValue;
            resource.minValue = 0;
            resource.maxValue = maxValue;
            EditorUtility.SetDirty(resource);
        }

        private static void ApplyLanding(ActivityDefinition activity)
        {
            activity.id = "landing";
            activity.displayName = "Landing Site";
            activity.tagline = "Choose where the crew touches down.";
            activity.description = "Compare hazard tags on a fictional training map and pick a landing site.";
            activity.hubOrder = 0;
            activity.estimatedMinutes = 2;
            activity.requiredForMission = true;
            activity.sceneName = "02_LandingSite";
            activity.instructionTitle = "Pick a landing site";
            activity.instructionBody =
                "Study the fictional training map. Tap a numbered marker to compare three hazard tags: slope, dust, and distance from the outpost.\n\n" +
                "There is no single perfect site. A short drive saves power, a gentle slope protects the crew, and a site with interesting geology can add science progress.\n\n" +
                "Pick the trade-off your crew can live with, then confirm one site.";
            activity.simulationNote =
                "Landing hazard tags and resource changes are game labels. The training map is fictional art - not NASA or Mars Trek data.";
            EditorUtility.SetDirty(activity);
        }

        private static void ApplyGreenhouse(ActivityDefinition activity)
        {
            activity.id = "greenhouse";
            activity.displayName = "Plant Lab";
            activity.tagline = "Run a fair red-light trial with seedlings.";
            activity.description = "Compare red light with a dark control and choose a careful claim.";
            activity.hubOrder = 1;
            activity.estimatedMinutes = 2;
            activity.requiredForMission = true;
            activity.sceneName = "03_Greenhouse";
            activity.instructionTitle = "Set up a fair plant-light trial";
            activity.instructionBody =
                "The greenhouse has young Arabidopsis seedlings in Mars-like gravity. Give one tray red light and keep the other dark as the control.\n\n" +
                "NASA OSDR study OSD-314 compared 6-day-old seedlings in microgravity and Mars-like gravity with red light or darkness. " +
                "At Mars-like gravity, red light accompanied recovery of cell-growth parameters. The study measured cell growth, cell division, and gene activity - not food yield or oxygen output.\n\n" +
                "Set up the trial, compare the simulated signals, then choose the sentence the study can support.";
            activity.simulationNote =
                "Inspired by NASA OSDR study OSD-314. Trial signals, lamp power, and science points are invented game values - not NASA measurements or farm predictions.";
            EditorUtility.SetDirty(activity);
        }

        private static void ApplyGreenhouseConfig(GreenhouseExperimentConfig config)
        {
            config.screenTitle = "Plant Lab";
            config.tagline = "Run a fair red-light trial with Mars-like seedlings.";
            config.chamberTitle = "GROWTH CHAMBER";
            config.environmentLabel = "MARS-LIKE GRAVITY";
            config.trayALabel = "TRAY A";
            config.trayBLabel = "TRAY B";
            config.chooseTreatmentLabel = "Choose a treatment";
            config.redTreatmentLabel = "RED LIGHT";
            config.darkTreatmentLabel = "DARK CONTROL";
            config.redButtonLabel = "Red";
            config.darkButtonLabel = "Dark";

            config.notebookTitle = "LAB NOTEBOOK";
            config.setupHeader = "STEP 1 - SET THE TREATMENTS";
            config.setupBody =
                "Give one tray red light. Keep the other dark as the control. Change only the light.";
            config.setupIncomplete = "A fair test uses one red-light tray and one dark control.";
            config.setupBothRed = "Both trays have red light. One must stay dark as the control.";
            config.setupBothDark = "Both trays are dark. One needs red light for the treatment.";
            config.setupReadyFormat = "Fair test ready: {0} red light, {1} dark control.";

            config.runLabel = "Run light trial";
            config.runCostFormat = "Uses {0} Power";
            config.runPowerCost = 8;

            config.observationsHeader = "STEP 2 - COMPARE THE SIGNALS";
            config.observationsBody =
                "Both trays had the same seedlings and Mars-like gravity. Only the light changed.";
            config.redObservation = new GreenhouseObservation
            {
                growthLabel = "CELL GROWTH",
                growthValue = "Recovery signal",
                growthTone = 0,
                stressLabel = "STRESS RESPONSE",
                stressValue = "Active",
                stressTone = 1
            };
            config.darkObservation = new GreenhouseObservation
            {
                growthLabel = "CELL GROWTH",
                growthValue = "Reduced signal",
                growthTone = 2,
                stressLabel = "STRESS RESPONSE",
                stressValue = "Active",
                stressTone = 1
            };

            config.claimsHeader = "STEP 3 - CHOOSE A CAREFUL CLAIM";
            config.claimPrompt = "Which sentence is supported by the study?";
            config.claimHint = "Tap one claim. Then log your finding.";
            config.confirmLabel = "Log finding";
            config.selectClaimLabel = "Choose a claim first";
            config.completedLabel = "Trial complete";
            config.simulationNote =
                "Inspired by NASA OSDR study OSD-314. Trial signals, power cost, and science points are invented for the game - not NASA measurements or farm predictions.";

            config.claims = new[]
            {
                new GreenhouseClaimDefinition
                {
                    id = "red-light-recovery",
                    headline = "Careful claim logged",
                    statement = "Red light helped cell-growth signals recover.",
                    feedback = "Red light went with cell-growth recovery.",
                    learningNote =
                        "Careful claim: OSD-314 measured cell growth, cell division, and gene activity in young Arabidopsis seedlings - not food yield, oxygen output, or a farm-ready recipe.",
                    matchesStudy = true,
                    changes = new[]
                    {
                        new ResourceChange("science", 18, "A careful claim earned science progress.")
                    }
                },
                new GreenhouseClaimDefinition
                {
                    id = "food-doubling",
                    headline = "Claim goes too far",
                    statement = "Red light will double Mars food.",
                    feedback = "The study did not measure food yield.",
                    learningNote =
                        "The study did not measure food production. A careful claim stays inside what the study actually measured.",
                    matchesStudy = false,
                    changes = new ResourceChange[0]
                },
                new GreenhouseClaimDefinition
                {
                    id = "darkness-wins",
                    headline = "Claim checked",
                    statement = "Darkness helped the seedlings more.",
                    feedback = "The dark group did not show that recovery.",
                    learningNote =
                        "Check the direction: in OSD-314, the red-light group showed recovery at Mars-like gravity. The dark group did not.",
                    matchesStudy = false,
                    changes = new ResourceChange[0]
                }
            };

            EditorUtility.SetDirty(config);
        }

        private static void ApplyWeather(ActivityDefinition activity)
        {
            activity.id = "weather";
            activity.displayName = "Space Weather";
            activity.tagline = "Read the alert. Pick a cautious crew plan.";
            activity.description = "Read a fictional DONKI-style training alert and choose a cautious crew response.";
            activity.hubOrder = 2;
            activity.estimatedMinutes = 2;
            activity.requiredForMission = true;
            activity.sceneName = "04_SpaceWeather";
            activity.instructionTitle = "Read the space-weather alert";
            activity.instructionBody =
                "A fictional training alert arrives from the space-weather station. Read the event type and the plain-language note, then choose the crew plan that fits the hazard.\n\n" +
                "NASA DONKI uses event-type names such as solar flares (FLR), solar energetic particles (SEP), coronal mass ejections (CME), " +
                "and geomagnetic storms (GST). They are not all the same hazard. This activity uses one FLR training alert; compare the others in the decoder.\n\n" +
                "Choose one plan and send it to the crew. The result explains what changed and why.";
            activity.simulationNote =
                "Training scenario using real DONKI event-type names. Alert, plans, costs, and scores are invented for the game - not a forecast, live data, or operational safety advice.";
            EditorUtility.SetDirty(activity);
        }

        private static void ApplySpaceWeatherConfig(SpaceWeatherConfig config)
        {
            config.screenTitle = "Space Weather";
            config.tagline = "Read the alert. Pick a cautious crew plan.";
            config.controlRoomTitle = "SPACE WEATHER STATION";
            config.alertTag = "TRAINING ALERT";
            config.decoderTitle = "DONKI EVENT TYPES";
            config.decoderHint = "These names are different events, not one hazard.";
            config.compareLabel = "Compare alert types";
            config.backToAlertLabel = "Back to alert";
            config.decisionTitle = "CREW PLAN";
            config.decisionPrompt = "Which crew plan fits this training alert?";
            config.planHint = "Tap one plan. Then send it to the crew.";
            config.selectedPlanFormat = "Selected: {0}. Ready to send.";
            config.choosePlanLabel = "Choose a crew plan";
            config.confirmLabel = "Send crew plan";
            config.completedLabel = "Crew plan sent";
            config.completedPlanFormat = "Training complete: {0}.";
            config.simulationNote =
                "Training scenario using real DONKI event-type names. Alert, plans, costs, and scores are invented for the game - not a forecast, live data, or operational safety advice.";

            config.curatedAlertsJson = AssetDatabase.LoadAssetAtPath<TextAsset>(
                "Assets/Data/Weather/CuratedAlerts.json");
            config.fallbackAlert = BuildWeatherFallback();
            config.eventTypes = new[]            {
                new SpaceWeatherEventTypeInfo
                {
                    code = "FLR",
                    name = "Solar flare",
                    description = "A sudden bright burst from the Sun. This alert is a flare."
                },
                new SpaceWeatherEventTypeInfo
                {
                    code = "SEP",
                    name = "Solar particles",
                    description = "Fast solar particles that can raise radiation risk in space."
                },
                new SpaceWeatherEventTypeInfo
                {
                    code = "CME",
                    name = "Sun eruption",
                    description = "A huge cloud of solar plasma. It can arrive hours or days later."
                },
                new SpaceWeatherEventTypeInfo
                {
                    code = "GST",
                    name = "Earth storm",
                    description = "An Earth geomagnetic-storm forecast. Not a direct Mars-surface alarm."
                }
            };

            EditorUtility.SetDirty(config);
        }

        private static SpaceWeatherAlertDefinition BuildWeatherFallback()
        {
            const string correctNote =
                "A solar flare (FLR) is not a coronal mass ejection (CME). An Earth geomagnetic storm (GST) is a different hazard, not a direct Mars-surface alarm. This is fictional training, not operational safety advice.";

            return new SpaceWeatherAlertDefinition
            {
                id = "flr-fallback",
                eventType = "FLR",
                eventName = "Solar flare",
                alertCode = "TRAINING ALERT FLR-00",
                headline = "A bright burst from the Sun",
                summary = "A solar flare is the training alert. The crew is inside. Choose a cautious plan.",
                sourceNote = "Built-in fallback alert. This is fictional training, not live data.",
                choices = new[]
                {
                    new SpaceWeatherPlanDefinition
                    {
                        id = "shelter",
                        title = "Shelter and watch",
                        description = "Pause outdoor work. Keep the crew inside and check the radiation meter.",
                        headline = "Crew sheltered",
                        feedback = "Good call. The crew limits exposure and watches the meter while the flare is active.",
                        learningNote = correctNote,
                        correct = true,
                        changes = new[]
                        {
                            new ResourceChange("safety", 12, "The crew stays inside and monitors the radiation meter."),
                            new ResourceChange("power", -6, "Safe mode uses a little extra power."),
                            new ResourceChange("science", 10, "Recording the flare adds to the training log.")
                        }
                    },
                    new SpaceWeatherPlanDefinition
                    {
                        id = "outside",
                        title = "Send a crew outside",
                        description = "Take photos from the rover deck during the flare.",
                        headline = "Risky plan",
                        feedback = "Not this time. Sending the crew outside adds exposure while the flare is active.",
                        learningNote = "A cautious training plan limits exposure. This game does not give real radiation procedures.",
                        correct = false,
                        changes = new[]
                        {
                            new ResourceChange("safety", -14, "The crew goes outside during the flare."),
                            new ResourceChange("power", -2, "The rover drive uses a little power.")
                        }
                    },
                    new SpaceWeatherPlanDefinition
                    {
                        id = "lights-off",
                        title = "Switch off the greenhouse",
                        description = "Save power by turning off the plant lights.",
                        headline = "Plan misses the hazard",
                        feedback = "This does not address the flare. Greenhouse lights are not the source of the radiation risk.",
                        learningNote = "A plan should match the hazard. Turning off plant lights saves power but does not shelter the crew from a solar flare.",
                        correct = false,
                        changes = new[]
                        {
                            new ResourceChange("power", 6, "Turning off the plant lights saves power."),
                            new ResourceChange("safety", -3, "The plan leaves the crew exposed to the flare.")
                        }
                    }
                }
            };
        }

        private static void ApplyLandingConfig(
            LandingMissionConfig config,
            LandingSiteDefinition auroraFlats,
            LandingSiteDefinition rustValley,
            LandingSiteDefinition pioneerMesa)
        {
            config.screenTitle = "Landing Site";
            config.tagline = "Compare three fictional sites. There is no single perfect choice.";
            config.prompt = "Tap a numbered marker to compare its slope, dust, and distance tags.";
            config.selectedHeader = "SITE DETAILS";
            config.mapCaption = "FICTIONAL TRAINING MAP - not Mars Trek data";
            config.confirmLabel = "Confirm landing site";
            config.sites = new[] { auroraFlats, rustValley, pioneerMesa };
            EditorUtility.SetDirty(config);
        }

        private static void ApplyAuroraFlats(LandingSiteDefinition site)
        {
            ApplyLandingSite(
                site,
                "aurora-flats",
                "Aurora Flats",
                "Northern basin",
                new Vector2(0.22f, 0.68f),
                "Gentle", 0,
                "Medium", 1,
                "Far", 2,
                "A wide, gentle shelf. The rover has a long drive back to the outpost.",
                "You chose Aurora Flats: a gentle approach and a wide landing area, but a long rover route and moderate dust.",
                "Real Mars site selection weighs many engineering constraints. This training map uses only three simplified game tags - it is not a real landing assessment.",
                new[]
                {
                    new ResourceChange("power", -12, "The long rover route uses extra power."),
                    new ResourceChange("safety", 5, "The gentle slope gives the lander a wide margin."),
                    new ResourceChange("science", 5, "The basin survey adds a little data.")
                });
        }

        private static void ApplyRustValley(LandingSiteDefinition site)
        {
            ApplyLandingSite(
                site,
                "rust-valley",
                "Rust Valley",
                "Equatorial valley",
                new Vector2(0.50f, 0.42f),
                "Moderate", 1,
                "Low", 0,
                "Near", 0,
                "A short, low-dust route close to the outpost. The ground is uneven.",
                "You chose Rust Valley: a short, low-dust route close to the outpost, with a moderate slope to watch.",
                "Real site selection would use detailed terrain and engineering data. Here, the hazard tags are simplified game labels, not measurements.",
                new[]
                {
                    new ResourceChange("power", -8, "The short rover route saves power."),
                    new ResourceChange("safety", 2, "The site is close to the outpost, though the ground is uneven."),
                    new ResourceChange("science", 10, "The valley survey adds a useful data set.")
                });
        }

        private static void ApplyPioneerMesa(LandingSiteDefinition site)
        {
            ApplyLandingSite(
                site,
                "pioneer-mesa",
                "Pioneer Mesa",
                "Southern highland",
                new Vector2(0.78f, 0.65f),
                "Steep", 2,
                "Low", 0,
                "Mid", 1,
                "Layered cliffs and a strong science target. The descent is steep.",
                "You chose Pioneer Mesa: layered cliffs and a strong science target, balanced against a steep descent.",
                "The science value here is a game reward, not a NASA measurement. Real missions must prove a site is safe before choosing it.",
                new[]
                {
                    new ResourceChange("power", -15, "The descent and climb use extra power."),
                    new ResourceChange("safety", -5, "Steep ground narrows the landing margin."),
                    new ResourceChange("science", 20, "The layered cliffs are a rich sample target.")
                });
        }

        private static void ApplyLandingSite(
            LandingSiteDefinition site,
            string id,
            string displayName,
            string region,
            Vector2 mapPosition,
            string slopeLabel,
            int slopeTone,
            string dustLabel,
            int dustTone,
            string distanceLabel,
            int distanceTone,
            string description,
            string resultSummary,
            string learningNote,
            ResourceChange[] changes)
        {
            site.id = id;
            site.displayName = displayName;
            site.region = region;
            site.mapPosition = mapPosition;
            site.slope = new LandingHazardTag { label = slopeLabel, tone = slopeTone };
            site.dust = new LandingHazardTag { label = dustLabel, tone = dustTone };
            site.distance = new LandingHazardTag { label = distanceLabel, tone = distanceTone };
            site.description = description;
            site.resultSummary = resultSummary;
            site.learningNote = learningNote;
            site.changes = changes;
            EditorUtility.SetDirty(site);
        }

        private static void ApplyDebrief(DebriefConfig debrief)
        {
            debrief.reportTitle = "Mission Report";
            debrief.intro = "Here is how your junior astronaut training mission went.";
            debrief.sources =
                "NASA OSDR study OSD-314: 'Adaptive response of Arabidopsis seedlings in microgravity and Mars reduced gravity " +
                "environment is enhanced by red light photostimulation' (Villacampa et al., International Journal of Molecular " +
                "Sciences, 2021; doi:10.3390/ijms22020899).\n\n" +
                "NASA CCMC DONKI documentation for space-weather event types and API terms. DONKI data is research and " +
                "prototyping quality; NOAA SWPC is the official forecast source.\n\n" +
                "NASA Mars Trek (trek.nasa.gov/mars) is the reference for future real map layers. The current landing map is " +
                "a fictional training map, not Mars Trek data.";
            debrief.notNasaNote =
                "This game is a learning simulation, not an official NASA product. Game values, landing sites, and space-weather " +
                "alerts are invented for play.";
            debrief.bands = new[]
            {
                new DebriefBand
                {
                    title = "Mission Complete",
                    message = "Your crew finished the training mission and brought home a report. Try again to compare choices!",
                    minimumScience = 0,
                    color = new Color(0.29f, 0.79f, 0.94f, 1f)
                },
                new DebriefBand
                {
                    title = "Science Started",
                    message = "You collected useful practice data. Every careful choice adds to the crew's science story.",
                    minimumScience = 10,
                    color = new Color(1f, 0.72f, 0.16f, 1f)
                },
                new DebriefBand
                {
                    title = "Junior Researcher",
                    message = "Excellent work! You compared options, explained your choices, and filled the science log.",
                    minimumScience = 20,
                    color = new Color(0.29f, 0.87f, 0.5f, 1f)
                }
            };
            debrief.replayLabel = "Play Again";
            debrief.menuLabel = "Main Menu";
            EditorUtility.SetDirty(debrief);
        }

        private static void ApplyWeatherOutcome(PlaceholderActivityOutcome outcome)
        {
            outcome.activityId = "weather";
            outcome.headline = "Training alert answered";
            outcome.summary =
                "You chose a cautious response to the fictional training alert: the crew pauses outdoor work and records the event. " +
                "Safety improves, and the log adds science progress.";
            outcome.learningNote =
                "DONKI event types are real, but this alert is invented for practice. Different event types are not the same hazard. Check NOAA SWPC for official forecasts.";
            outcome.changes = new[]
            {
                new ResourceChange("safety", 10, "A cautious response protects the crew."),
                new ResourceChange("power", -5, "Safe mode uses a little power."),
                new ResourceChange("science", 5, "Recording the event adds data to the science log.")
            };
            EditorUtility.SetDirty(outcome);
        }

        private static void ApplyMissionConfig(
            MissionConfig config,
            ResourceDefinition power,
            ResourceDefinition safety,
            ResourceDefinition science,
            ActivityDefinition landing,
            ActivityDefinition greenhouse,
            ActivityDefinition weather,
            DebriefConfig debrief,
            UiTheme theme,
            UiTheme highContrastTheme)
        {
            config.gameTitle = "Mars Outpost";
            config.subtitle = "Junior Astronaut Training";
            config.missionCode = "TRAINING MISSION 01";
            config.introHeadline = "Welcome, Junior Astronaut!";
            config.introBody =
                "Your crew is setting up a practice outpost on Mars. Pick a landing site, run a plant experiment, " +
                "and answer a space-weather alert. Every choice changes Power, Crew Safety, and Science Progress.";
            config.howToPlayTitle = "How to Play";
            config.howToPlayBody =
                "1. Tap Start Mission.\n" +
                "2. Choose one of the three training steps.\n" +
                "3. Read the situation, make a choice, and check the result card.\n" +
                "4. Finish all three steps, then open the Mission Report.\n\n" +
                "The numbers are game values, not NASA measurements. There is no single perfect mission - " +
                "the report explains what each choice changed.";
            config.simulationDisclaimer =
                "Training simulation. Resources, costs, and results are invented for the game - not NASA measurements, " +
                "procedures, or live data.";
            config.credits =
                "Inspired by NASA open science resources: OSDR study OSD-314, the DONKI space-weather notification system, " +
                "and Mars Trek. The landing map is fictional placeholder art, not Mars Trek data. This game is not an official " +
                "NASA product and is not endorsed by NASA.";
            config.resources = new[] { power, safety, science };
            config.activities = new[] { landing, greenhouse, weather };
            config.debrief = debrief;
            config.theme = theme;
            config.highContrastTheme = highContrastTheme;
            config.mainMenuScene = "00_MainMenu";
            config.hubScene = "01_MissionHub";
            config.debriefScene = "05_Debrief";
            config.scienceTarget = 20;
            config.allowReplayCompletedActivities = false;
            EditorUtility.SetDirty(config);
        }
    }
}
