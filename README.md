# Mars Outpost: Junior Astronaut Training

A mobile-first Unity learning game for ages 7–12. Players run a fictional training mission on Mars: choose a landing site, set up a plant-light experiment, and answer a space-weather alert while managing **Power**, **Crew Safety**, and **Science Progress**.

> This is a learning simulation, not an official NASA product. Resource values, landing sites, and alerts are invented for play. They are not NASA measurements, procedures, or live data.

## Current status: Phase 4

Phase 4 adds the space-weather alert, the third real mission decision:

- **Space Weather** is playable: read a fictional training alert, compare the four DONKI event types (FLR, SEP, CME, GST), choose one cautious crew plan, and log the outcome.
- The screen separates the alert from the plan: the left panel shows a generated Sun–Mars scene, the event type, the alert code, a plain-language summary, the fictional-scenario note, and an expandable event-type decoder. The right panel shows the headline and three numbered plans with visible **TAP** / **CHOSEN** / **LOCKED** labels, so color is never the only cue.
- A correct plan earns Crew Safety and Science Progress and costs a little Power. The two weaker plans explain their trade-off in kid-readable feedback.
- The screen is visibly labelled as a training simulation. The alert says it is fictional, the bottom note says the costs and scores are not a forecast and not operational safety advice, and Earth geomagnetic-storm (GST) records are never presented as direct Mars-surface danger.
- Large, high-contrast touch targets show the selected plan in gold and enable the **Send crew plan** button; the result panel is reused from the landing and greenhouse activities.
- Text and high-contrast settings remain available; the alert headline, source note, simulation note, and plan cards are checked at 130% text.
- EditMode and PlayMode tests cover the alert data, the local-file source and fallback, the DONKI decoder, plan selection, the full result flow, and the 130%-text screen layout.
- Phase 1–3 captures remain in `Logs/`; Phase 4 captures are listed under **Preview build**.

## Run it in the Unity Editor

1. Open the project folder with **Unity 6000.6.5f1** (Unity 6.6, 2D URP template).
2. Open `Assets/Scenes/00_MainMenu.unity`.
3. Press **Play**.

The other scenes can be opened and played directly too. Each scene creates the persistent `MissionDirector` if it is not already present.

If the scenes or data assets are missing after a fresh clone:

1. Wait for the first import to finish.
2. Run **Tools > Mars Outpost > Build Phase 1 Content**.
3. Confirm the rebuild prompt if prompted.
4. Open `00_MainMenu` and press Play.

TextMeshPro Essential Resources are included under `Assets/TextMesh Pro`. If they are missing, run **Window > TextMeshPro > Import TMP Essential Resources** before rebuilding.

## Controls

- Touch or mouse for all buttons.
- Android system **Back** and desktop **Esc** are handled by the on-screen screens; activity **Back** returns to the hub.
- Every activity has an on-screen **Back** button; an open result panel can be closed or used to return to the hub, and the hub and activity screens do not rely on gestures.
- **Text** cycles 100% → 115% → 130%.
- **High contrast** switches to the high-contrast palette.

The layout is built for **landscape phones and tablets**, with a 1920 × 1080 reference canvas, a 0.5 match-width-or-height CanvasScaler, and safe-area padding. Auto-rotation is limited to landscape left/right; portrait is disabled because the three-column hub and bottom control rows are designed for landscape. Tested at 16:9 (1664 × 936) and 20:9 (2400 × 1080); the report and hub list scroll when the available height is short.

## Landing-site game

1. In the hub, open **Landing Site**.
2. Tap a numbered marker on the fictional training map. The details panel shows the site's slope, dust, and distance-to-outpost tags with a text label and a color dot.
3. Compare the sites. There is no perfect choice: Aurora Flats is gentle but far; Rust Valley is close and low-dust but uneven; Pioneer Mesa is a strong science target but steep.
4. Read the site description, then tap **Confirm landing site**. The result card explains the trade-off and lists the Power, Crew Safety, and Science Progress changes.
5. **Back** returns to the hub, including while a result card is open. A confirmed site is locked for the rest of the mission unless replay is enabled in `MissionConfig`.

All three sites, their tags, and their consequences are fictional game content. The map is generated art, not Mars Trek imagery.

## Greenhouse experiment

1. In the hub, open **Plant Lab**.
2. Choose a treatment for each tray: **Red** or **Dark**. The fair test is one red-light tray and one dark control, with the same seedlings and Mars-like gravity.
3. Read the live status line and the **Uses 8 Power** cost, then tap **Run light trial**. The two button rows lock and the notebook switches to simulated observations.
4. Compare **Cell growth** and **Stress response** for each tray. The values and colors are invented game signals, not NASA measurements.
5. Choose the sentence the study supports: *Red light helped cell-growth signals recover.* Then tap **Log finding**.
6. Read the result card: the careful claim earns **+18 Science**; food-yield and darkness claims are rejected with a kid-readable explanation. **Back** returns to the hub.

The experiment is inspired by NASA OSDR study **OSD-314** (Villacampa et al., 2021). The game does not claim a lamp distance, food yield, oxygen output, or an exact growth percentage. The observations, Power cost, and Science points are labeled in-game as invented simulation values.

### Editing the greenhouse without code

- `Assets/Data/Greenhouse/GreenhouseConfig.asset` holds every kid-facing string: step titles, prompts, tray labels, observation text, claim text, feedback, headlines, learning notes, the fair-test hints, and the simulation note.
- Claim definitions are the `claims` list. Each has an `id`, `headline`, `statement`, `feedback`, `learningNote`, `matchesStudy` flag, and optional resource `changes`. Keep exactly one claim supported by the study and give only that claim a Science reward.
- The two tray cards are `Assets/Prefabs/UI/GreenhouseTray.prefab`; each claim row is `Assets/Prefabs/UI/GreenhouseClaim.prefab`.
- If you change the prefab layout or add/remove authored content, update the matching builder in `Assets/Scripts/Editor/MissionDataBuilder.cs` and `MissionSceneBuilder.cs` before regenerating; otherwise the next rebuild will overwrite hand edits.

## Space-weather alert

1. In the hub, open **Space Weather**.
2. Read the fictional **TRAINING ALERT FLR-01**. The left panel shows a solar flare (FLR) in plain language, a source note that says the alert is fictional, and a **Compare alert types** button.
3. Tap **Compare alert types** to see the four DONKI event types used in the game: **FLR** (solar flare), **SEP** (solar energetic particles), **CME** (coronal mass ejection), and **GST** (geomagnetic storm). They are different hazards, not one hazard. The GST card says it is an Earth forecast and **not a direct Mars-surface alarm**.
4. Read the headline and the three numbered crew plans. Tap one: it turns gold and is labelled **CHOSEN**; the others show **LOCKED**. **Send crew plan** becomes available.
5. Confirm the plan. The result card shows the resource changes (Crew Safety, Power, Science Progress), the plan feedback, and a learning note that separates a flare from a CME and from an Earth geomagnetic storm.
6. **Back** returns to the hub, including while the result card is open.

The alert, plans, costs, scores, and resource changes are invented game content. The event-type names come from NASA DONKI, but this screen is **not** a forecast, does not use live data, and is **not** operational safety advice. No NASA API key, network permission, or live DONKI request is used; alert content is a local training file.

### Editing the space-weather content without code

- `Assets/Data/Weather/SpaceWeatherConfig.asset` holds all kid-facing copy: the screen title and tagline, control-room labels, decoder title and hint, decision prompt, plan hint, confirm/completed labels, the simulation note, the fallback alert, and the four event-type cards.
- `Assets/Data/Weather/CuratedAlerts.json` is the default local training alert. It is a `TextAsset`; the game reads it through `ISpaceWeatherAlertSource`. Each alert has an `id`, `eventType`, `eventName`, `alertCode`, `headline`, `summary`, `sourceNote`, and a `choices` list.
- Each choice has an `id`, `title`, `description`, `headline`, `feedback`, `learningNote`, a `correct` flag, and a list of `changes` (`resourceId`, `delta`, `reason`). Keep **exactly one** `correct` choice. A good plan should raise Crew Safety and Science Progress, cost a little Power, and explain the trade-off; the weaker plans should explain what they miss.
- The three plans are rendered from `Assets/Prefabs/UI/WeatherPlan.prefab` and built by `Assets/Scripts/Editor/MissionUiBuilder.cs` / `MissionSceneBuilder.cs`. Edit the prefab for visual changes; edit the JSON for content.
- If the curated JSON is missing or invalid, the game shows the built-in fallback alert and labels it **BUILT-IN FALLBACK** with a console warning. This is intentional so a classroom demo still works offline.
- The validator checks the decoder covers FLR, SEP, CME, and GST, that the GST text mentions Earth and the “not direct Mars” caveat, that every choice has a non-empty resource change, and that exactly one choice is correct. Run **Tools > Mars Outpost > Validate Mission Content** after editing data.
- If you add another event type, add it to the decoder and to the validator’s known types; do not silently treat every event as the same hazard. The game intentionally keeps one curated alert for now.

## Radiation drill (optional)

1. In the hub, open **Radiation Drill**. It is an optional fourth card; the three required steps still gate the Mission Report.
2. Tap **Start Drill**. A simulated alert begins after a short delay in one of four approved rooms: Plant Lab, Rocket Bay, Biodome, or Space Lab.
3. Drag the large on-screen joystick to move. When a contextual **Talk** or **Activate Shielding** button appears, tap it. Keyboard/gamepad fallback: WASD or arrows to move, Space/Enter to advance dialogue.
4. Read the detector: **SAFE**, **WARNING**, or **HIGH**, with a 0–100 **SIMULATED READING** and a **SIMULATED EXPOSURE** bar. The red area is a game visualization of a simulated warning zone, not visible radiation.
5. Talk to the **Safety Officer** for a calm explanation of radiation, monitoring, and shielding. Reach the **Radiation Control Room** in the top reactor room and tap **Activate Shielding**. Shielding contains the simulated hazard and the warning area shrinks.
6. If the simulated exposure meter fills, the drill pauses, explains what happened, and returns you to the crew hub. Nothing is a real dose or a real safety measurement.
7. The result card records the optional drill. **Back**, **Pause**, and **Return to Ship** are touch buttons; Android Back and Esc use the same flow.

### Editing the radiation drill without code

- `Assets/Data/Radiation/RadiationConfig.asset` holds all kid-facing text and tuning: instructions, detector labels, Safe/Warning/High labels and thresholds, Safety Officer pages, control-room prompt, gentle-reset message, debrief, colors, alert delay, spread speed, maximum radius, containment speed, and exposure gain/recovery.
- Approved spawn points are the four `Spawn_*` objects in `Assets/Scenes/07_Radiation.unity`. Move them inside the illustrated rooms if you change the arena. The `WalkableArea` object has the authored room rectangles; the validator checks every spawn against them.
- The scene is generated by **Tools > Mars Outpost > Build Radiation Drill Scene**. The builder keeps the existing `RadiationConfig` and dialogue, regenerates only the `07_Radiation` scene, and appends it to Build Settings; it does not touch the other six scenes.
- Replace `Assets/Art/Environment/C028FE22-C39D-4996-BEFD-12C2C401FA5A.png` only if the room layout still matches the walkable rectangles. Character frames come from `Assets/Art/Characters/Astronaut.png`; music uses the existing `Assets/Art/UI/main_gameMusic.mp3`.
- Do **not** add real radiation units or real readings. The detector is deliberately a simulated 0–100 game value. Keep the warning area described as a visualization, and keep shielding framed as reducing simulated exposure, not switching radiation off.
- Tuning values in the config are game values, not NASA procedures. The drill is not real radiation-safety training.

## Android target

The project is configured Android-first in `Assets/Scripts/Editor/MissionProjectSettings.cs`:

- Package id: `com.marsoutpost.juniorastronaut`
- Minimum SDK: 26
- Target SDK: automatic
- Architecture: ARM64
- Scripting backend: IL2CPP
- Internet permission: not requested
- Auto-rotation between landscape left and landscape right (portrait disabled; see Controls)

**This machine does not have the Unity Android Build Support module installed.** The project therefore contains no APK/AAB. To produce one:

1. In Unity Hub, install **Android Build Support** with **Android SDK & NDK Tools** and **OpenJDK** for Unity 6000.6.5f1.
2. Open **File > Build Profiles**, select **Android**, and switch the platform.
3. Run **Build** or **Build And Run** with a USB-debugging device.
4. For a store build, create your own keystore and configure it in **Player Settings > Publishing Settings**. No keystore or secrets are included.

## Project layout

| Path | Purpose |
| --- | --- |
| `Assets/Scenes/` | `00_MainMenu`, `01_MissionHub`, the three activity scenes, and `05_Debrief` |
| `Assets/Scripts/Core/` | Mission config, session state, validation, navigation, scene bootstrap |
| `Assets/Scripts/Landing/` | Landing-site data, validator, map marker view, and landing screen controller |
| `Assets/Scripts/Greenhouse/` | Greenhouse data, validator, tray/claim views, screen controller, and the dev-only capture harness |
| `Assets/Scripts/Weather/` | Space-weather data, validator, alert source, plan-card view, and screen controller |
| `Assets/Scripts/UI/` | Shared UI components, theme service, screen controllers |
| `Assets/Scripts/Editor/` | One-time asset/prefab/scene builders and validation menu |
| `Assets/Data/Configs/` | Authored ScriptableObject content: mission, activities, resources, themes, debrief |
| `Assets/Data/Landing/` | `LandingConfig.asset` and the three fictional `Site_*.asset` definitions |
| `Assets/Data/Greenhouse/` | `GreenhouseConfig.asset`: all greenhouse copy, observations, claims, and costs |
| `Assets/Data/Weather/` | `SpaceWeatherConfig.asset` and the local `CuratedAlerts.json` training alert |
| `Assets/Prefabs/UI/` | Resource pill, activity card, result panel, confirmation dialog, landing marker, greenhouse tray/claim row, weather plan card |
| `Assets/Art/Generated/` | Procedural placeholder sprites generated by the project, including the fictional landing map |
| `Assets/Tests/` | EditMode and PlayMode tests |

### Editing content without code

Most kid-facing text and values live in ScriptableObjects:

- `MissionConfig.asset`: title, intro, How to Play, disclaimer, credits, scene names, science target.
- `Resource_*.asset`: display names, starting values, ranges, colors, icon, kid-friendly explanation.
- `Activity_*.asset`: names, taglines, instructions, scene names, simulation notes.
- `LandingConfig.asset`: landing screen copy, map caption, confirm label, and the ordered list of sites.
- `Site_*.asset`: a fictional site's name, region, normalized map position, slope/dust/distance tags, description, result summary, learning note, and resource changes. To add a site, duplicate a `Site_*.asset`, edit it, and add it to the `sites` list on `LandingConfig.asset`.
- `GreenhouseConfig.asset`: greenhouse step titles, tray labels, observations, claims, feedback, learning notes, costs, and simulation note. Keep exactly one `matchesStudy` claim; only that claim should award Science.
- `SpaceWeatherConfig.asset`: weather screen copy, decoder title/hint, plan prompt and labels, simulation note, fallback alert, and the four DONKI event-type cards. The default alert lives in `Assets/Data/Weather/CuratedAlerts.json`; keep exactly one correct plan.
- `DebriefConfig.asset`: report title, intro, source text, and science bands.
- `UiTheme.asset` / `UiTheme_HighContrast.asset`: colors and font sizes.

Hazard tags use three tones (0 gentle/low/near, 1 moderate, 2 steep/high/far). Tone 0 is green, tone 1 amber, and tone 2 red in the details panel; the text label is always shown too, so color is never the only cue. There is intentionally no "perfect" site: each choice trades power, safety, and science progress.

After content generation, the scenes and prefabs are the source of truth. If you edit a scene or prefab by hand, do not run the rebuild command unless you intend to replace it; the builder is a one-time generator, not a live sync tool. Make a backup or commit first.

## Science and reference notes

The game uses real NASA open-science topics but fictional game values:

- **OSD-314**: *Adaptive response of Arabidopsis seedlings in microgravity and Mars reduced gravity environment is enhanced by red light photostimulation* (Villacampa et al., *International Journal of Molecular Sciences*, 2021; doi:10.3390/ijms22020899). The study used 6-day-old Arabidopsis thaliana Col-0 seedlings, spaceflight microgravity and Mars partial gravity, 1 g ground controls, red-light versus dark conditions, RNA sequencing, and cell-growth/proliferation measurements. The game does **not** claim an optimal lamp distance, food yield, oxygen output, or a specific growth percentage.
- **DONKI**: the event taxonomy (FLR, SEP, CME, IPS, MPC, GST, RBE, HSS) is used for a fictional training alert. DONKI is NASA’s database of notifications and forecasts; NOAA SWPC is the operational forecast source. The game uses the names **FLR** (solar flare), **SEP** (solar energetic particles), **CME** (coronal mass ejection), and **GST** (geomagnetic storm) only. A GST is an Earth geomagnetic-storm forecast; it is **not** presented as a direct Mars-surface danger, and the four event types are explicitly described as different hazards. The alert itself is invented, is not live DONKI data, and is not operational safety advice. No NASA API, API key, or network permission is used.
- **Mars Trek**: referenced as the future source for real map layers. The current landing map is generated fictional art and the three sites are invented; the on-screen caption says this explicitly.

The source list appears in the debrief and on the main menu’s **Sources** dialog. The invented costs and scores are labelled in-game as simulation values.

## Preview build

A standalone Windows preview is generated at `Builds/Windows/MarsOutpost.exe`. It is only a review aid for this Windows machine; the Android build is the product target. The preview starts on `00_MainMenu` and keeps the same landscape layout as the phone build.

Screenshots used during review are in `Logs/`. Phase 4 weather captures are `phase4_preview.png` (alert), `phase4_preview_decoder.png` (DONKI event-type decoder), `phase4_preview_selected.png` (chosen plan), and `phase4_preview_result.png` (result card). Phase 3 greenhouse captures are `phase3_greenhouse_01_setup.png`, `phase3_greenhouse_02_treatments.png`, `phase3_greenhouse_03_compare.png`, `phase3_greenhouse_04_claim.png`, `phase3_greenhouse_05_result.png`, plus the 130%-text/high-contrast checks `*_a11y.png`. The Phase 3 captures predate the final claim-fit and selected-button changes; the PlayMode test `GreenhouseClaimFitsAtLargestTextScale` is the current layout check. Phase 2 landing captures are `phase2_landing_01_initial.png`, `phase2_landing_02_selected.png`, `phase2_landing_03_result.png`, and the 130%-text checks `phase2_landing_04_large_initial.png`, `phase2_landing_05_large_selected.png`, `phase2_landing_06_large_result.png`. Phase 1 captures remain as `preview_01_mainmenu.png` through `preview_11_debrief.png`.

## Tests

In the Unity Editor, open **Window > General > Test Runner** and run:

- **EditMode** (23 tests): session clamping, idempotent outcomes, report gating, reset, validator checks, landing-site content, hazard profiles, landing change references, greenhouse config/claim rules, and space-weather config/curated-alert/fallback/decoder rules.
- **PlayMode** (12 tests): scene bootstrap creates a persistent director; missing scenes return a friendly error; the activity **Back** button returns to the hub; selecting a landing site enables confirm, logs exactly one outcome, and opens the result card; greenhouse treatment selection is visible on the buttons; the claim row fits at 130% text; the full greenhouse fair-test flow logs one outcome and reaches the result; the weather alert shows its headline, fictional source note, and simulation disclaimer; selecting a weather plan enables confirm and shows a **CHOSEN** label; the decoder toggles without hiding the plans; the full weather flow logs one outcome and opens the result; weather screen labels and plan cards fit at 130% text.

Last verified result: **EditMode 23/23 passed**, **PlayMode 12/12 passed** (Unity 6000.6.5f1, November 2026).

Command-line examples:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Unity.exe" `
  -batchmode -projectPath "C:\Users\Surface-7\My project" `
  -runTests -testPlatform EditMode `
  -testResults "C:\Users\Surface-7\My project\Logs\editmode.xml"
```

Use `-testPlatform PlayMode` for the play-mode suite. If a run is interrupted, wait for `Temp/UnityLockfile` to disappear before starting another batchmode Unity process; only one Unity instance can open the project.

## Known limitations

- The landing, greenhouse, and space-weather activities are playable; Phase 5 integration, debrief polish, and the final pass are still ahead.
- No audio, animation, save file, leaderboard, or network access yet.
- Session state resets when the app closes. There is no mid-mission save.
- The Android APK has not been built or device-tested on this machine because the Android module is absent.
- The landing map, icons, Mars illustration, and space-weather control-room art are generated locally; they are not NASA imagery or Mars Trek data.
- The greenhouse tray art, lamp art, observation values, Power cost, and Science reward are invented game content. They do not describe a real lamp setup, growth rate, food yield, or oxygen output.
- The greenhouse claim feedback and learning notes are game translations of the OSD-314 abstract, not a substitute for the paper. Check the linked paper before changing scientific wording.
- The greenhouse claim sentences are capped at 24 pt inside the fixed claim rows so the whole sentence stays visible at 130% text; all other greenhouse text still scales. The PlayMode layout test guards this.
- The space-weather alert is a local `TextAsset`, not a live DONKI request. It is one curated FLR scenario with invented plans, costs, and scores; it is not a forecast and not operational safety advice. The built-in fallback exists for offline demos.
- The weather plan cards cap their index/title/description text to keep the fixed rows intact at 130% text; the PlayMode layout test guards this. The alert and decoder copy still scale normally.
- The game uses TMP bitmap fonts and has only been reviewed at desktop and simulated landscape aspect ratios (16:9 and 20:9). Physical-device safe-area, notch, and font-size checks remain.
- On a short/wide phone the hub activity list and the full mission report require a scroll; the visible scrollbar and the report’s “Swipe up or scroll…” hint make this explicit.
- The landing map uses normalized marker positions, so marker labels can crowd if several new sites are added close together; space new sites out and check the screen at 130% text.
