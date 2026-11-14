using UnityEngine;

namespace MarsOutpost.Radiation
{
    /// <summary>
    /// The red warning area. This is a game visualization of a simulated hazard,
    /// not visible radiation. It is deliberately translucent so the ship stays readable.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class RadiationVisual : MonoBehaviour
    {
        [SerializeField] private Color color = new Color(1f, 0.12f, 0.12f, 1f);
        [SerializeField, Range(0f, 1f)] private float startAlpha = 0.34f;
        [SerializeField, Range(0f, 1f)] private float endAlpha = 0.12f;
        [SerializeField] private int sortingOrder = 10;

        private SpriteRenderer spriteRenderer;
        private Sprite generatedSprite;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = sortingOrder;
            if (spriteRenderer.sprite == null)
            {
                generatedSprite = CreateGradientSprite(256);
                spriteRenderer.sprite = generatedSprite;
            }

            Hide();
        }

        private void OnDestroy()
        {
            if (generatedSprite != null)
            {
                Destroy(generatedSprite.texture);
                Destroy(generatedSprite);
            }
        }

        public void SetColor(Color value)
        {
            color = value;
        }

        public void Show(Vector2 origin)
        {
            transform.position = new Vector3(origin.x, origin.y, 0f);
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }
        }

        public void Hide()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }
        }

        public void SetState(float radius, float maxRadius)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            transform.localScale = Vector3.one * (Mathf.Max(0f, radius) * 2f);
            float t = Mathf.Clamp01(radius / Mathf.Max(0.01f, maxRadius));
            Color tint = color;
            tint.a = Mathf.Lerp(startAlpha, endAlpha, t);
            spriteRenderer.color = tint;
        }

        private static Sprite CreateGradientSprite(int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                name = "SimulatedWarningArea"
            };

            float half = size * 0.5f;
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(half, half)) / half;
                    float alpha = Mathf.Pow(Mathf.Clamp01(1f - distance), 0.65f);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
