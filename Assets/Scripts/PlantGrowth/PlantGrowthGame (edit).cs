using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// GAME 1: PLANT GROWTH  (Unity 6, uGUI, top-down view)
///
/// Drop this script on an empty GameObject in an empty scene and press Play.
/// Everything is built at runtime in a blue sci-fi pixel-art style:
/// space background, a lab room, 4 planting trays (2 x 2), 3 light bulbs on the right,
/// an info panel bottom-left and a star character that pops up at the end.
///
/// All art is generated in code (pixel sprites). Every sprite slot below is optional:
/// assign your own art and it replaces the generated one.
/// </summary>
public class PlantGrowthGame : MonoBehaviour
{
    public enum BulbType { Street, Household, Fluorescent }

    // ------------------------------------------------------------------ Inspector
    [Header("Optional art overrides (leave empty to use the generated pixel art)")]
    [SerializeField] Sprite backgroundSprite;
    [SerializeField] Sprite traySprite;
    [SerializeField] Sprite saplingSprite;
    [SerializeField] Sprite lettuceSprite;
    [SerializeField] Sprite streetBulbSprite;
    [SerializeField] Sprite householdBulbSprite;
    [SerializeField] Sprite fluorescentBulbSprite;
    [SerializeField] Sprite starSprite;
    [Tooltip("Optional pixel font. Leave empty for Unity's built-in font.")]
    [SerializeField] Font customFont;

    [Header("Layout")]
    [SerializeField] int trayCount = 4;
    [SerializeField] int trayColumns = 2;
    [SerializeField] Vector2 traySize = new Vector2(320f, 250f);
    [SerializeField] float traySpacing = 40f;

    [Header("Timing")]
    [SerializeField] float overlayFadeTime = 0.35f;
    [SerializeField] float growDuration = 1.2f;
    [SerializeField] float growStagger = 0.12f;
    [SerializeField] float starDelayAfterAllBulbs = 2.5f;
    [SerializeField] float starCharsPerSecond = 45f;
    [SerializeField] bool wiggleSaplingsOnWrongLight = true;

    [Header("Light overlay colours (alpha = strength of the shine)")]
    [SerializeField] Color fluorescentOverlay = new Color(1f, 1f, 1f, 0.28f);          // light white
    [SerializeField] Color householdOverlay = new Color(1f, 0.93f, 0.40f, 0.28f);      // light yellow
    [SerializeField] Color streetOverlay = new Color(1f, 0.55f, 0.10f, 0.30f);         // light orange

    [Header("Text")]
    [SerializeField] string gameTitle = "Plant Growth Lab";

    [SerializeField, TextArea(5, 10)] string whyFluorescentText =
        "Fluorescent lights grow plants better because they stay safely cool and " +
        "deliver the exact \"colors\" plants need to make food. While regular bulbs " +
        "get burning hot and lack the right light, cool fluorescent bulbs provide plenty of " +
        "blue light for strong leaves and red light for healthy growth. This lets you " +
        "place the light right next to the plant so it can safely soak up all the energy it " +
        "needs to grow big and strong.";

    [SerializeField, TextArea(3, 6)] string starExplanation =
        "Great job! Fluorescent lights stay cool, so plants can sit close without burning. " +
        "They give the blue light for strong leaves and the red light for healthy growth!";

    [Header("Events")]
    public UnityEvent onGameCompleted;   // hook up "go to next game" here

    // ------------------------------------------------------------------ Palette (matches the blue sci-fi reference)
    static readonly Color SpaceNavy = Hex("#070B24");
    static readonly Color OutlineNavy = Hex("#0A0F33");
    static readonly Color FrameBlue = Hex("#2146C7");
    static readonly Color Neon = Hex("#5AE6FF");
    static readonly Color FloorLight = Hex("#A9B2E0");
    static readonly Color FloorLine = new Color(0.45f, 0.50f, 0.80f, 0.45f);
    static readonly Color PanelDark = Hex("#101A52");
    static readonly Color TrayWhite = Hex("#EEF1FF");
    static readonly Color Soil = Hex("#3A2418");
    static readonly Color TextLight = Hex("#DFF6FF");
    static readonly Color TextDark = Hex("#0E1450");
    static readonly Color BubbleFill = Hex("#EAF0FF");
    static readonly Color Planet = Hex("#2133A0");
    static readonly Color SparkYellow = Hex("#FFE66B");

    static Color Hex(string h)
    {
        Color c;
        ColorUtility.TryParseHtmlString(h, out c);
        return c;
    }

    // ------------------------------------------------------------------ Internals
    class PlantSlot { public RectTransform sapling; public RectTransform lettuce; }

    class BulbUI
    {
        public BulbType type;
        public Image bulb;
        public Image glow;
        public Color baseColor;
    }

    readonly List<PlantSlot> plants = new List<PlantSlot>();
    readonly List<BulbUI> bulbs = new List<BulbUI>();
    readonly HashSet<BulbType> tried = new HashSet<BulbType>();

    Font font;
    RectTransform canvasRoot;
    Image overlay;
    Text statusLabel;

    RectTransform starGroup;
    RectTransform starPivot;
    RectTransform bubbleRT;
    Text bubbleText;
    Button continueButton;
    Vector2 starBasePos;

    Coroutine overlayRoutine;
    bool grown;
    bool finished;

    // ------------------------------------------------------------------ Lifecycle
    void Start()
    {
        Build();
    }

    /// <summary>Restart the game from the beginning.</summary>
    public void ResetGame()
    {
        StopAllCoroutines();
        if (canvasRoot != null) Destroy(canvasRoot.gameObject);
        plants.Clear();
        bulbs.Clear();
        tried.Clear();
        grown = false;
        finished = false;
        overlayRoutine = null;
        Build();
    }

    // ------------------------------------------------------------------ Build the screen
    void Build()
    {
        font = customFont != null ? customFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        EnsureEventSystem();

        var canvasGO = new GameObject("PlantGrowthCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.transform.SetParent(transform, false);
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasRoot = (RectTransform)canvasGO.transform;

        BuildSpace(canvasRoot);

        // The lab room (pale floor inside a glowing blue frame)
        var room = MakePanel("Room", canvasRoot, new Vector2(1760f, 980f), Vector2.zero, FloorLight);
        for (int x = -800; x <= 800; x += 200)
            MakeImage("TileLineV", room, White, FloorLine, new Vector2(2f, 940f), new Vector2(x, 0f));
        for (int y = -400; y <= 400; y += 200)
            MakeImage("TileLineH", room, White, FloorLine, new Vector2(1720f, 2f), new Vector2(0f, y));

        BuildTrays(canvasRoot);

        // Light overlay: over the trays, under the UI, never blocks clicks
        overlay = MakeImage("LightOverlay", canvasRoot, White, new Color(1f, 1f, 1f, 0f), Vector2.zero, Vector2.zero);
        Stretch(overlay.rectTransform);

        BuildTitle(canvasRoot);
        BuildBulbs(canvasRoot);
        BuildInfoPanel(canvasRoot);
        BuildStar(canvasRoot);
        UpdateStatus();
    }

    void BuildSpace(RectTransform parent)
    {
        if (backgroundSprite != null)
        {
            var art = MakeImage("Space", parent, backgroundSprite, Color.white, Vector2.zero, Vector2.zero);
            Stretch(art.rectTransform);
            return;
        }

        var bg = MakeImage("Space", parent, White, SpaceNavy, Vector2.zero, Vector2.zero);
        Stretch(bg.rectTransform);

        // planet peeking in from the top-left corner
        MakeImage("Planet", parent, Circle, Planet, new Vector2(460f, 460f), new Vector2(-960f, 540f));
        MakeImage("PlanetShade", parent, Circle, new Color(0.08f, 0.12f, 0.45f, 1f), new Vector2(330f, 330f), new Vector2(-1000f, 580f));

        // tiny stars
        var rng = new System.Random(7);
        for (int i = 0; i < 80; i++)
        {
            float x = (float)(rng.NextDouble() * 1920.0 - 960.0);
            float y = (float)(rng.NextDouble() * 1080.0 - 540.0);
            float s = rng.Next(0, 4) == 0 ? 6f : 4f;
            float a = 0.35f + (float)rng.NextDouble() * 0.65f;
            MakeImage("Star", parent, White, new Color(1f, 1f, 1f, a), new Vector2(s, s), new Vector2(x, y));
        }
    }

    void BuildTrays(RectTransform parent)
    {
        int cols = Mathf.Max(1, trayColumns);
        int rows = Mathf.CeilToInt(trayCount / (float)cols);
        Vector2 gridCenter = new Vector2(-400f, 80f);
        float stepX = traySize.x + traySpacing;
        float stepY = traySize.y + traySpacing;

        // plant spots (the dots from the sketch): 2 x 2 in every tray
        Vector2[] spots =
        {
            new Vector2(-traySize.x * 0.25f,  traySize.y * 0.20f),
            new Vector2( traySize.x * 0.25f,  traySize.y * 0.20f),
            new Vector2(-traySize.x * 0.25f, -traySize.y * 0.20f),
            new Vector2( traySize.x * 0.25f, -traySize.y * 0.20f),
        };

        for (int i = 0; i < trayCount; i++)
        {
            int col = i % cols;
            int row = i / cols;
            float x = gridCenter.x + (col - (cols - 1) / 2f) * stepX;
            float y = gridCenter.y - (row - (rows - 1) / 2f) * stepY;

            var tray = NewRect("Tray " + (i + 1), parent, traySize, new Vector2(x, y));
            if (traySprite != null)
            {
                MakeImage("TrayArt", tray, traySprite, Color.white, traySize, Vector2.zero);
            }
            else
            {
                MakeImage("TrayOutline", tray, White, OutlineNavy, traySize, Vector2.zero);
                MakeImage("TrayWhite", tray, White, TrayWhite, traySize - new Vector2(8f, 8f), Vector2.zero);
                MakeImage("SoilRim", tray, White, OutlineNavy, traySize - new Vector2(32f, 32f), Vector2.zero);
                MakeImage("Soil", tray, White, Soil, traySize - new Vector2(38f, 38f), Vector2.zero);
            }

            foreach (var spot in spots)
                plants.Add(BuildPlant(tray, spot));
        }
    }

    PlantSlot BuildPlant(RectTransform parent, Vector2 pos)
    {
        var slot = new PlantSlot();
        var root = NewRect("Plant", parent, new Vector2(100f, 100f), pos);

        // Sapling
        slot.sapling = NewRect("Sapling", root, new Vector2(60f, 60f), Vector2.zero);
        MakeImage("Art", slot.sapling, saplingSprite != null ? saplingSprite : SaplingArt,
            Color.white, new Vector2(60f, 60f), Vector2.zero, true);

        // Lettuce (hidden until the fluorescent light is clicked)
        slot.lettuce = NewRect("Lettuce", root, new Vector2(100f, 100f), Vector2.zero);
        MakeImage("Art", slot.lettuce, lettuceSprite != null ? lettuceSprite : LettuceArt,
            Color.white, new Vector2(96f, 96f), Vector2.zero, true);
        slot.lettuce.localScale = Vector3.zero;

        return slot;
    }

    void BuildTitle(RectTransform parent)
    {
        var panel = MakePanel("TitlePanel", parent, new Vector2(820f, 100f), new Vector2(0f, 420f), PanelDark);
        MakeText("Title", panel, gameTitle, 44, TextAnchor.MiddleCenter, Neon,
            new Vector2(780f, 56f), new Vector2(0f, 14f), FontStyle.Bold);
        statusLabel = MakeText("Status", panel, "", 24, TextAnchor.MiddleCenter, TextLight,
            new Vector2(780f, 32f), new Vector2(0f, -26f));

        MakeText("Hint", parent, "Click each bulb!", 28, TextAnchor.MiddleCenter, TextDark,
            new Vector2(340f, 44f), new Vector2(700f, 420f), FontStyle.Bold);
    }

    void BuildBulbs(RectTransform parent)
    {
        MakeBulb(parent, BulbType.Street, "Street Light", streetBulbSprite != null ? streetBulbSprite : BulbArt,
            new Vector2(120f, 150f), new Color(1f, 0.60f, 0.15f, 1f), new Vector2(700f, 270f));
        MakeBulb(parent, BulbType.Household, "Standard Household Light", householdBulbSprite != null ? householdBulbSprite : BulbArt,
            new Vector2(120f, 150f), new Color(1f, 0.92f, 0.45f, 1f), new Vector2(700f, 60f));
        MakeBulb(parent, BulbType.Fluorescent, "Fluorescent Light", fluorescentBulbSprite != null ? fluorescentBulbSprite : TubeArt,
            new Vector2(210f, 70f), new Color(0.80f, 0.95f, 1f, 1f), new Vector2(700f, -150f));
    }

    void MakeBulb(RectTransform parent, BulbType type, string label, Sprite art, Vector2 artSize, Color baseColor, Vector2 pos)
    {
        var root = NewRect(label + " Bulb", parent, new Vector2(340f, 230f), pos);

        var glow = MakeImage("Glow", root, Circle, new Color(baseColor.r, baseColor.g, baseColor.b, 0f),
            new Vector2(210f, 210f), new Vector2(0f, 25f));

        var bulb = MakeImage("Bulb", root, art, OffColor(baseColor), artSize, new Vector2(0f, 25f), true, true);

        var button = bulb.gameObject.AddComponent<Button>();
        button.targetGraphic = bulb;
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(() => OnBulbClicked(type));

        MakeText("Label", root, label, 26, TextAnchor.MiddleCenter, TextDark,
            new Vector2(340f, 44f), new Vector2(0f, -85f), FontStyle.Bold);

        bulbs.Add(new BulbUI { type = type, bulb = bulb, glow = glow, baseColor = baseColor });
    }

    void BuildInfoPanel(RectTransform parent)
    {
        // Bottom-left info box (always visible)
        var panel = MakePanel("InfoPanel", parent, new Vector2(840f, 240f), new Vector2(-420f, -350f), PanelDark);
        var t = MakeText("InfoText", panel, whyFluorescentText, 24, TextAnchor.UpperLeft, TextLight,
            new Vector2(780f, 196f), Vector2.zero);
        t.resizeTextForBestFit = true;
        t.resizeTextMinSize = 14;
        t.resizeTextMaxSize = 24;
    }

    void BuildStar(RectTransform parent)
    {
        starGroup = NewRect("StarGroup", parent, Vector2.zero, Vector2.zero);

        // star + soft glow
        starBasePos = new Vector2(330f, -395f);
        starPivot = NewRect("StarPivot", starGroup, new Vector2(200f, 200f), starBasePos);
        MakeImage("Glow", starPivot, Circle, new Color(1f, 0.9f, 0.4f, 0.28f), new Vector2(190f, 190f), Vector2.zero);
        MakeImage("Star", starPivot, starSprite != null ? starSprite : StarArt, Color.white,
            new Vector2(140f, 140f), Vector2.zero, true);

        // speech bubble
        bubbleRT = MakePanel("StarBubble", starGroup, new Vector2(440f, 220f), new Vector2(330f, -190f), BubbleFill);
        var tailOutline = MakeImage("TailOutline", bubbleRT, White, OutlineNavy, new Vector2(30f, 30f), new Vector2(0f, -112f));
        tailOutline.rectTransform.localEulerAngles = new Vector3(0f, 0f, 45f);
        var tailFill = MakeImage("TailFill", bubbleRT, White, BubbleFill, new Vector2(22f, 22f), new Vector2(0f, -108f));
        tailFill.rectTransform.localEulerAngles = new Vector3(0f, 0f, 45f);
        MakeImage("TailCover", bubbleRT, White, BubbleFill, new Vector2(44f, 14f), new Vector2(0f, -102f));

        bubbleText = MakeText("BubbleText", bubbleRT, "", 24, TextAnchor.MiddleLeft, TextDark,
            new Vector2(400f, 176f), Vector2.zero);

        // continue button
        var btn = MakePanel("ContinueButton", starGroup, new Vector2(200f, 60f), new Vector2(590f, -410f), FrameBlue, true);
        MakeText("Label", btn, "Continue", 28, TextAnchor.MiddleCenter, Color.white,
            new Vector2(200f, 60f), Vector2.zero, FontStyle.Bold);
        continueButton = btn.gameObject.AddComponent<Button>();
        continueButton.targetGraphic = btn.GetComponent<Image>();
        continueButton.transition = Selectable.Transition.None;
        continueButton.onClick.AddListener(OnContinueClicked);
        btn.gameObject.SetActive(false);

        starGroup.gameObject.SetActive(false);
    }

    // ------------------------------------------------------------------ Game logic
    void OnBulbClicked(BulbType type)
    {
        if (finished && starGroup != null && starGroup.gameObject.activeSelf) return;

        tried.Add(type);
        HighlightBulb(type);

        Color target = type == BulbType.Fluorescent ? fluorescentOverlay
                     : type == BulbType.Household ? householdOverlay
                     : streetOverlay;
        if (overlayRoutine != null) StopCoroutine(overlayRoutine);
        overlayRoutine = StartCoroutine(FadeOverlay(target));

        if (type == BulbType.Fluorescent)
        {
            if (!grown)
            {
                grown = true;
                for (int i = 0; i < plants.Count; i++)
                    StartCoroutine(GrowPlant(plants[i], i * growStagger));
            }
        }
        else if (!grown && wiggleSaplingsOnWrongLight)
        {
            foreach (var p in plants) StartCoroutine(WiggleSapling(p));
        }

        UpdateStatus();

        // All three bulbs tried (and the plants grew) -> the star pops up
        if (!finished && tried.Count == 3 && grown)
        {
            finished = true;
            StartCoroutine(StarRoutine());
        }
    }

    void HighlightBulb(BulbType active)
    {
        foreach (var b in bulbs)
        {
            bool lit = b.type == active;
            b.bulb.color = lit ? b.baseColor : OffColor(b.baseColor);
            b.glow.color = new Color(b.baseColor.r, b.baseColor.g, b.baseColor.b, lit ? 0.55f : 0f);
        }
    }

    void UpdateStatus()
    {
        if (statusLabel == null) return;
        statusLabel.text = (grown ? "After fluorescent light" : "Before fluorescent light")
                         + "   |   Bulbs tried: " + tried.Count + "/3";
    }

    void OnContinueClicked()
    {
        starGroup.gameObject.SetActive(false);
        onGameCompleted?.Invoke();
    }

    // ------------------------------------------------------------------ Animations
    IEnumerator FadeOverlay(Color target)
    {
        Color start = overlay.color;
        float t = 0f;
        while (t < overlayFadeTime)
        {
            t += Time.deltaTime;
            overlay.color = Color.Lerp(start, target, t / overlayFadeTime);
            yield return null;
        }
        overlay.color = target;
        overlayRoutine = null;
    }

    IEnumerator GrowPlant(PlantSlot slot, float delay)
    {
        yield return new WaitForSeconds(delay);
        slot.sapling.localEulerAngles = Vector3.zero;
        float t = 0f;
        while (t < growDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / growDuration);
            slot.lettuce.localScale = Vector3.one * EaseOutBack(k);
            slot.sapling.localScale = Vector3.one * (1f - k);
            yield return null;
        }
        slot.lettuce.localScale = Vector3.one;
        slot.sapling.gameObject.SetActive(false);
    }

    IEnumerator WiggleSapling(PlantSlot slot)
    {
        float t = 0f;
        const float duration = 0.6f;
        while (t < duration && !grown)
        {
            t += Time.deltaTime;
            float z = Mathf.Sin(t * 30f) * 8f * (1f - t / duration);
            slot.sapling.localEulerAngles = new Vector3(0f, 0f, z);
            yield return null;
        }
        slot.sapling.localEulerAngles = Vector3.zero;
    }

    IEnumerator StarRoutine()
    {
        yield return new WaitForSeconds(starDelayAfterAllBulbs);

        starGroup.gameObject.SetActive(true);
        starPivot.localScale = Vector3.zero;
        bubbleRT.localScale = Vector3.zero;
        bubbleText.text = "";
        continueButton.gameObject.SetActive(false);

        StartCoroutine(SparkBurst());

        // star pops up with a spin
        float t = 0f;
        const float popTime = 0.6f;
        while (t < popTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / popTime);
            starPivot.localScale = Vector3.one * EaseOutBack(k);
            starPivot.localEulerAngles = new Vector3(0f, 0f, (1f - k) * 360f);
            yield return null;
        }
        starPivot.localScale = Vector3.one;
        starPivot.localEulerAngles = Vector3.zero;
        StartCoroutine(StarIdle());

        // speech bubble pops in
        t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            bubbleRT.localScale = Vector3.one * EaseOutBack(Mathf.Clamp01(t / 0.3f));
            yield return null;
        }
        bubbleRT.localScale = Vector3.one;

        // typewriter text
        float wait = 1f / Mathf.Max(1f, starCharsPerSecond);
        for (int i = 1; i <= starExplanation.Length; i++)
        {
            bubbleText.text = starExplanation.Substring(0, i);
            yield return new WaitForSeconds(wait);
        }

        continueButton.gameObject.SetActive(true);
    }

    IEnumerator StarIdle()
    {
        float time = 0f;
        while (starGroup != null && starGroup.gameObject.activeInHierarchy)
        {
            time += Time.deltaTime;
            starPivot.anchoredPosition = starBasePos + new Vector2(0f, Mathf.Sin(time * 3f) * 8f);
            starPivot.localEulerAngles = new Vector3(0f, 0f, Mathf.Sin(time * 2f) * 5f);
            yield return null;
        }
    }

    IEnumerator SparkBurst()
    {
        const int n = 10;
        var bits = new Image[n];
        var dirs = new Vector2[n];
        for (int i = 0; i < n; i++)
        {
            float a = (i / (float)n) * Mathf.PI * 2f;
            dirs[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            bits[i] = MakeImage("Spark", starGroup, White, SparkYellow, new Vector2(12f, 12f), starBasePos);
        }

        float t = 0f;
        const float duration = 0.7f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            for (int i = 0; i < n; i++)
            {
                bits[i].rectTransform.anchoredPosition = starBasePos + dirs[i] * Mathf.Lerp(40f, 170f, k);
                bits[i].color = new Color(SparkYellow.r, SparkYellow.g, SparkYellow.b, 1f - k);
            }
            yield return null;
        }
        for (int i = 0; i < n; i++) Destroy(bits[i].gameObject);
    }

    static float EaseOutBack(float k)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(k - 1f, 3f) + c1 * Mathf.Pow(k - 1f, 2f);
    }

    // ------------------------------------------------------------------ Generated pixel art
    static Sprite whiteSprite, circleSprite, lettuceArt, saplingArt, bulbArt, tubeArt, starArt;

    static Sprite White
    {
        get
        {
            if (whiteSprite == null)
                whiteSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
            return whiteSprite;
        }
    }

    static Sprite Circle
    {
        get
        {
            if (circleSprite != null) return circleSprite;
            const int s = 128;
            float r = s / 2f;
            var px = new Color[s * s];
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                    px[y * s + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(r - d));
                }
            circleSprite = SpriteFromPixels(px, s, s, FilterMode.Bilinear);
            return circleSprite;
        }
    }

    static Sprite SpriteFromPixels(Color[] px, int w, int h, FilterMode filter)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = filter
        };
        tex.SetPixels(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    }

    // Round-leaved lettuce head: dark outline, pale head with darker leaves around it, veins and a highlight.
    static Sprite LettuceArt
    {
        get
        {
            if (lettuceArt != null) return lettuceArt;
            const int s = 48;
            var px = new Color[s * s];
            Color outline = Hex("#12501C");
            Color leafDark = Hex("#2F8A2E");
            Color leafMid = Hex("#4FB04A");
            Color head = Hex("#9BD768");
            Color headLight = Hex("#BEEA84");
            Color headEdge = Hex("#3C9A35");
            Color vein = Hex("#5FA83A");
            const float cx = 24f, cy = 22f, R = 22f;

            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    float dx = (x + 0.5f - cx) / R;
                    float dy = (y + 0.5f - cy) / R;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float th = Mathf.Atan2(dy, dx);
                    float ro = 0.93f + 0.06f * Mathf.Sin(5f * th + 0.6f) + 0.035f * Mathf.Sin(11f * th);

                    if (r > ro || dy < -0.80f) continue;

                    Color col;
                    if (r > ro - 0.10f)
                    {
                        col = outline;
                    }
                    else
                    {
                        float hx = dx / 0.60f;
                        float hy = (dy - 0.08f) / 0.78f;
                        float hd = Mathf.Sqrt(hx * hx + hy * hy);
                        if (hd < 1f)
                        {
                            col = hd > 0.9f ? headEdge : head;

                            // veins fan out from the base of the head
                            float bx = dx, by = dy + 0.62f;
                            float d = Mathf.Sqrt(bx * bx + by * by);
                            float phi = Mathf.Atan2(by, bx);
                            float a = Mathf.Repeat(phi + 0.19f, 0.38f) - 0.19f;
                            if (d > 0.18f && Mathf.Abs(a) * d < 0.032f && hd < 0.88f) col = vein;

                            // highlight
                            float hl = (dx + 0.22f) * (dx + 0.22f) + (dy - 0.28f) * (dy - 0.28f);
                            if (hl < 0.018f) col = headLight;
                        }
                        else
                        {
                            col = r > 0.72f ? leafDark : leafMid;
                        }
                    }
                    px[y * s + x] = col;
                }
            }
            lettuceArt = SpriteFromPixels(px, s, s, FilterMode.Point);
            return lettuceArt;
        }
    }

    static Sprite SaplingArt
    {
        get
        {
            if (saplingArt != null) return saplingArt;
            const int w = 24, h = 24;
            var px = new Color[w * h];
            Color ol = Hex("#12501C"), green = Hex("#4CC24A"), light = Hex("#8BE06A");
            Color soilOl = Hex("#2A180F"), soilC = Hex("#5A3A22");

            Ellipse(px, w, h, 12f, 4f, 10f, 4.5f, 0f, soilOl);
            Ellipse(px, w, h, 12f, 4f, 9f, 3.5f, 0f, soilC);

            // stem
            for (int y = 5; y <= 12; y++) { px[y * w + 11] = ol; px[y * w + 12] = green; px[y * w + 13] = ol; }

            // leaf outlines then fills
            Ellipse(px, w, h, 7f, 14f, 6f, 3.6f, 30f, ol);
            Ellipse(px, w, h, 17f, 14f, 6f, 3.6f, -30f, ol);
            Ellipse(px, w, h, 12f, 18.5f, 3.6f, 5.4f, 0f, ol);
            Ellipse(px, w, h, 7f, 14f, 5f, 2.6f, 30f, green);
            Ellipse(px, w, h, 17f, 14f, 5f, 2.6f, -30f, green);
            Ellipse(px, w, h, 12f, 18.5f, 2.6f, 4.4f, 0f, green);
            px[19 * w + 11] = light; px[20 * w + 11] = light;
            px[15 * w + 5] = light; px[15 * w + 6] = light;
            px[15 * w + 17] = light;

            saplingArt = SpriteFromPixels(px, w, h, FilterMode.Point);
            return saplingArt;
        }
    }

    // Light bulb. Glass is near-white so it can be tinted by the bulb colour.
    static Sprite BulbArt
    {
        get
        {
            if (bulbArt != null) return bulbArt;
            const int w = 32, h = 40;
            var px = new Color[w * h];
            Color ol = Hex("#0A0F33");
            Color glass = new Color(0.92f, 0.92f, 0.92f, 1f);
            Color baseA = new Color(0.62f, 0.64f, 0.75f, 1f);
            Color baseB = new Color(0.48f, 0.50f, 0.62f, 1f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(16f, 25f));
                    bool inGlass = d <= 12f || (x >= 11 && x <= 20 && y >= 12 && y <= 17);
                    bool inGlassCore = d <= 10f || (x >= 13 && x <= 18 && y >= 12 && y <= 17);
                    if (inGlass) px[y * w + x] = inGlassCore ? glass : ol;

                    if (x >= 11 && x <= 20 && y >= 2 && y <= 11)
                    {
                        bool edge = x == 11 || x == 20 || y == 2 || y == 11;
                        px[y * w + x] = edge ? ol : (y % 3 == 0 ? baseB : baseA);
                    }
                }
            }
            // highlight
            for (int y = 28; y <= 31; y++) { px[y * w + 9] = Color.white; px[y * w + 10] = Color.white; }
            px[32 * w + 10] = Color.white; px[32 * w + 11] = Color.white;

            bulbArt = SpriteFromPixels(px, w, h, FilterMode.Point);
            return bulbArt;
        }
    }

    // Fluorescent tube.
    static Sprite TubeArt
    {
        get
        {
            if (tubeArt != null) return tubeArt;
            const int w = 48, h = 16;
            var px = new Color[w * h];
            Color ol = Hex("#0A0F33");
            Color glass = new Color(0.92f, 0.92f, 0.92f, 1f);
            Color cap = new Color(0.62f, 0.64f, 0.75f, 1f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool isCap = x < 5 || x >= 43;
                    bool edge = y == 0 || y == h - 1 || x == 0 || x == w - 1 || x == 5 || x == 42;
                    Color c = edge ? ol : (isCap ? cap : glass);
                    if (!isCap && !edge && y == h - 4 && x >= 8 && x <= 40) c = Color.white;
                    px[y * w + x] = c;
                }
            }
            tubeArt = SpriteFromPixels(px, w, h, FilterMode.Point);
            return tubeArt;
        }
    }

    // Cute star character (pixel art with a face).
    static Sprite StarArt
    {
        get
        {
            if (starArt != null) return starArt;
            const int s = 40;
            var px = new Color[s * s];
            Color ol = Hex("#6B3B05");
            Color body = Hex("#FFD93D");
            Color light = Hex("#FFF08A");
            Color dark = Hex("#3A1F02");
            Color blush = Hex("#FF8A5C");

            Vector2 c = new Vector2(20f, 20.5f);
            Vector2[] outer = StarPoly(c, 19f);
            Vector2[] inner = StarPoly(c, 15f);

            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    var p = new Vector2(x + 0.5f, y + 0.5f);
                    if (!InPoly(outer, p)) continue;
                    Color col = ol;
                    if (InPoly(inner, p))
                    {
                        col = body;
                        if (p.x < c.x - 3f && p.y > c.y + 2f) col = light;
                    }
                    px[y * s + x] = col;
                }
            }

            // face
            for (int y = 19; y <= 21; y++) { px[y * s + 15] = dark; px[y * s + 16] = dark; px[y * s + 24] = dark; px[y * s + 25] = dark; }
            px[22 * s + 15] = Color.white; px[22 * s + 24] = Color.white;
            for (int x = 18; x <= 21; x++) px[14 * s + x] = dark;
            px[15 * s + 17] = dark; px[15 * s + 22] = dark;
            for (int x = 12; x <= 13; x++) { px[16 * s + x] = blush; px[17 * s + x] = blush; }
            for (int x = 27; x <= 28; x++) { px[16 * s + x] = blush; px[17 * s + x] = blush; }

            starArt = SpriteFromPixels(px, s, s, FilterMode.Point);
            return starArt;
        }
    }

    static Vector2[] StarPoly(Vector2 c, float outerR)
    {
        var v = new Vector2[10];
        for (int i = 0; i < 10; i++)
        {
            float a = (90f + i * 36f) * Mathf.Deg2Rad;
            float r = (i % 2 == 0) ? outerR : outerR * 0.45f;
            v[i] = c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r;
        }
        return v;
    }

    static bool InPoly(Vector2[] v, Vector2 p)
    {
        bool inside = false;
        for (int i = 0, j = v.Length - 1; i < v.Length; j = i++)
        {
            if (((v[i].y > p.y) != (v[j].y > p.y)) &&
                (p.x < (v[j].x - v[i].x) * (p.y - v[i].y) / (v[j].y - v[i].y) + v[i].x))
                inside = !inside;
        }
        return inside;
    }

    static void Ellipse(Color[] pixels, int w, int h, float cx, float cy, float rx, float ry, float angleDeg, Color c)
    {
        float a = angleDeg * Mathf.Deg2Rad;
        float ca = Mathf.Cos(a), sa = Mathf.Sin(a);
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = x + 0.5f - cx;
                float dy = y + 0.5f - cy;
                float u = dx * ca + dy * sa;
                float v = -dx * sa + dy * ca;
                if ((u * u) / (rx * rx) + (v * v) / (ry * ry) <= 1f) pixels[y * w + x] = c;
            }
        }
    }

    // ------------------------------------------------------------------ UI helpers
    static Color OffColor(Color c)
    {
        return Color.Lerp(c, new Color(0.35f, 0.35f, 0.40f, 1f), 0.65f);
    }

    static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null) return;
        var es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
        es.AddComponent<InputSystemUIInputModule>();
#else
        es.AddComponent<StandaloneInputModule>();
#endif
    }

    static RectTransform NewRect(string name, Transform parent, Vector2 size, Vector2 pos)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        return rt;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static Image MakeImage(string name, Transform parent, Sprite sprite, Color color,
        Vector2 size, Vector2 pos, bool keepAspect = false, bool raycast = false)
    {
        var rt = NewRect(name, parent, size, pos);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.preserveAspect = keepAspect;
        img.raycastTarget = raycast;
        return img;
    }

    /// <summary>Blue sci-fi panel: dark outline, blue frame, neon rim, then the fill colour.</summary>
    static RectTransform MakePanel(string name, Transform parent, Vector2 size, Vector2 pos, Color fill, bool clickable = false)
    {
        var root = NewRect(name, parent, size, pos);
        MakeImage("Outline", root, White, OutlineNavy, size, Vector2.zero);
        MakeImage("Frame", root, White, FrameBlue, size - new Vector2(8f, 8f), Vector2.zero);
        MakeImage("Neon", root, White, Neon, size - new Vector2(18f, 18f), Vector2.zero);
        MakeImage("Fill", root, White, fill, size - new Vector2(24f, 24f), Vector2.zero);

        if (clickable)
        {
            var hit = root.gameObject.AddComponent<Image>();
            hit.color = new Color(0f, 0f, 0f, 0f);
            hit.raycastTarget = true;
        }
        return root;
    }

    Text MakeText(string name, Transform parent, string content, int fontSize, TextAnchor anchor,
        Color color, Vector2 size, Vector2 pos, FontStyle style = FontStyle.Normal)
    {
        var rt = NewRect(name, parent, size, pos);
        var t = rt.gameObject.AddComponent<Text>();
        t.font = font;
        t.text = content;
        t.fontSize = fontSize;
        t.alignment = anchor;
        t.color = color;
        t.fontStyle = style;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }
}
