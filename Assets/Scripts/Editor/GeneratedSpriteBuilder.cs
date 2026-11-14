using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MarsOutpost.EditorTools
{
    /// <summary>
    /// Generates simple, original placeholder sprites so the project renders without
    /// external art. Replace the PNGs in Assets/Art/Generated to re-skin the game.
    /// </summary>
    public static class GeneratedSpriteBuilder
    {
        public const string ArtRoot = "Assets/Art/Generated";

        public sealed class Sprites
        {
            public Sprite RoundedPanel;
            public Sprite RoundedButton;
            public Sprite RoundedPill;
            public Sprite Circle;
            public Sprite Mars;
            public Sprite LandingMap;
            public Sprite LandingIcon;
            public Sprite GreenhouseIcon;
            public Sprite GreenhouseTray;
            public Sprite WeatherIcon;
            public Sprite WeatherControlRoom;
        }

        public static Sprites BuildAll()
        {
            EnsureFolder(ArtRoot);

            var sprites = new Sprites
            {
                RoundedPanel = BuildRounded("UI_RoundedPanel", 64, 64, 20, new Vector4(20, 20, 20, 20)),
                RoundedButton = BuildRounded("UI_RoundedButton", 64, 64, 24, new Vector4(24, 24, 24, 24)),
                RoundedPill = BuildRounded("UI_RoundedPill", 64, 64, 28, new Vector4(28, 28, 28, 28)),
                Circle = BuildCircle("UI_Circle", 128),
                Mars = BuildMars("Hero_Mars", 512),
                LandingMap = BuildLandingMap("Landing_Map", 1024, 640),
                LandingIcon = BuildIcon("Icon_Landing", 128, DrawLanding),
                GreenhouseIcon = BuildIcon("Icon_Greenhouse", 128, DrawGreenhouse),
                GreenhouseTray = BuildIcon("Greenhouse_Tray", 512, DrawGreenhouseTray),
                WeatherIcon = BuildIcon("Icon_Weather", 128, DrawWeather),
                WeatherControlRoom = BuildWeatherControlRoom()
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return sprites;
        }

        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent))
            {
                EnsureFolder(parent);
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        private static Sprite BuildRounded(string name, int width, int height, float radius, Vector4 border)
        {
            Texture2D texture = NewTexture(width, height);
            var pixels = new Color32[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float alpha = RoundedRectAlpha(x + 0.5f, y + 0.5f, width, height, radius);
                    pixels[y * width + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            return SaveSprite(texture, name, border);
        }

        private static Sprite BuildCircle(string name, int size)
        {
            Texture2D texture = NewTexture(size, size);
            var pixels = new Color32[size * size];
            float radius = size * 0.5f - 1f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - size * 0.5f;
                    float dy = y + 0.5f - size * 0.5f;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01(radius + 0.5f - distance);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            return SaveSprite(texture, name, Vector4.zero);
        }

        private static Sprite BuildLandingMap(string name, int width, int height)
        {
            Texture2D texture = NewTexture(width, height);
            var pixels = new Color32[width * height];
            Color32 low = new Color32(82, 39, 27, 255);
            Color32 mid = new Color32(151, 74, 43, 255);
            Color32 high = new Color32(211, 124, 76, 255);
            Color32 dust = new Color32(199, 153, 108, 255);
            Color32 shadow = new Color32(54, 27, 22, 255);
            Color32 grid = new Color32(255, 235, 200, 255);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float nx = x / (float)(width - 1);
                    float ny = y / (float)(height - 1);

                    // Broad, low-frequency terrain bands.
                    float terrain = 0.48f
                                    + 0.16f * Mathf.Sin(nx * 9.4f + ny * 2.7f)
                                    + 0.10f * Mathf.Sin(ny * 15.0f - nx * 3.1f)
                                    + 0.06f * Mathf.Sin((nx + ny) * 31.0f);

                    Color32 color = Blend(low, mid, Mathf.Clamp01(terrain));
                    float dustAmount = Mathf.Clamp01(0.28f + 0.32f * Mathf.Sin(nx * 5.2f - ny * 8.0f));
                    color = Blend(color, dust, dustAmount * 0.20f);

                    // A dark canyon cuts across the map.
                    float canyonX = 0.34f + 0.09f * Mathf.Sin(ny * 5.4f) + 0.03f * Mathf.Sin(ny * 15f);
                    float canyonDistance = Mathf.Abs(nx - canyonX);
                    if (canyonDistance < 0.012f)
                    {
                        color = Blend(color, shadow, 0.75f);
                    }
                    else if (canyonDistance < 0.024f)
                    {
                        color = Blend(color, high, 0.38f);
                    }

                    // A shallow ridge line helps the map read as a place, not a flat color.
                    float ridge = 0.72f + 0.035f * Mathf.Sin(nx * 16f + 1.2f);
                    if (ny > ridge && ny < ridge + 0.012f)
                    {
                        color = Blend(color, high, 0.42f);
                    }

                    // Crater rings and floors. Positions are normalized to the map rectangle.
                    AddMapCrater(ref color, x, y, width, height, 0.18f, 0.74f, 0.055f, shadow);
                    AddMapCrater(ref color, x, y, width, height, 0.63f, 0.22f, 0.085f, shadow);
                    AddMapCrater(ref color, x, y, width, height, 0.82f, 0.77f, 0.040f, shadow);
                    AddMapCrater(ref color, x, y, width, height, 0.44f, 0.61f, 0.030f, high);
                    AddMapCrater(ref color, x, y, width, height, 0.31f, 0.38f, 0.022f, high);

                    // A faint survey grid gives the map a playful "training board" look.
                    bool vertical = x % 128 <= 1;
                    bool horizontal = y % 128 <= 1;
                    if (vertical || horizontal)
                    {
                        color = Blend(color, grid, 0.12f);
                    }

                    pixels[y * width + x] = color;
                }
            }

            texture.SetPixels32(pixels);
            return SaveSprite(texture, name, Vector4.zero);
        }

        private static Sprite BuildMars(string name, int size)
        {
            Texture2D texture = NewTexture(size, size);
            var pixels = new Color32[size * size];
            float radius = size * 0.5f - 2f;
            Color32 baseColor = new Color32(196, 82, 42, 255);
            Color32 dark = new Color32(142, 56, 30, 255);
            Color32 light = new Color32(222, 116, 70, 255);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - size * 0.5f;
                    float dy = y + 0.5f - size * 0.5f;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01(radius + 0.5f - distance);
                    if (alpha <= 0f)
                    {
                        pixels[y * size + x] = new Color32(0, 0, 0, 0);
                        continue;
                    }

                    Color32 color = baseColor;

                    // A soft terminator so the planet reads as a sphere.
                    float shade = Mathf.Clamp01((dx * -0.45f + dy * 0.45f) / (size * 0.55f) + 0.55f);
                    color = Blend(baseColor, dark, Mathf.Clamp01(1f - shade) * 0.55f);

                    // Fictional craters for a playful, recognizable Mars.
                    AddCrater(ref color, x, y, size, 0.30f, 0.34f, 0.075f, dark);
                    AddCrater(ref color, x, y, size, 0.62f, 0.30f, 0.055f, dark);
                    AddCrater(ref color, x, y, size, 0.48f, 0.66f, 0.09f, dark);
                    AddCrater(ref color, x, y, size, 0.71f, 0.58f, 0.04f, dark);
                    AddCrater(ref color, x, y, size, 0.26f, 0.62f, 0.035f, light);

                    color.a = (byte)Mathf.RoundToInt(alpha * 255f);
                    pixels[y * size + x] = color;
                }
            }

            texture.SetPixels32(pixels);
            return SaveSprite(texture, name, Vector4.zero);
        }

        private enum IconKind
        {
            Landing,
            Greenhouse,
            Weather
        }

        private static Sprite BuildIcon(string name, int size, Action<Color32[], int> draw)
        {
            Texture2D texture = NewTexture(size, size);
            var pixels = new Color32[size * size];
            draw(pixels, size);
            texture.SetPixels32(pixels);
            return SaveSprite(texture, name, Vector4.zero);
        }

        private static void DrawLanding(Color32[] pixels, int size)
        {
            Color32 white = new Color32(255, 255, 255, 255);
            FillCircle(pixels, size, size * 0.5f, size * 0.5f, size * 0.30f, new Color32(255, 255, 255, 40));
            DrawRing(pixels, size, size * 0.5f, size * 0.5f, size * 0.30f, 5f, white);
            FillCircle(pixels, size, size * 0.5f, size * 0.55f, size * 0.09f, white);
            DrawLine(pixels, size, size * 0.38f, size * 0.38f, size * 0.62f, size * 0.38f, 5f, white);
            DrawLine(pixels, size, size * 0.5f, size * 0.55f, size * 0.5f, size * 0.30f, 4f, white);
        }

        private static void DrawGreenhouse(Color32[] pixels, int size)
        {
            Color32 white = new Color32(255, 255, 255, 255);
            DrawLine(pixels, size, size * 0.5f, size * 0.20f, size * 0.5f, size * 0.62f, 5f, white);
            FillCircle(pixels, size, size * 0.35f, size * 0.62f, size * 0.16f, white);
            FillCircle(pixels, size, size * 0.65f, size * 0.70f, size * 0.14f, white);
            DrawLine(pixels, size, size * 0.50f, size * 0.48f, size * 0.34f, size * 0.62f, 4f, white);
            DrawLine(pixels, size, size * 0.50f, size * 0.55f, size * 0.66f, size * 0.70f, 4f, white);
            DrawLine(pixels, size, size * 0.26f, size * 0.18f, size * 0.74f, size * 0.18f, 5f, white);
        }

        private static void DrawGreenhouseTray(Color32[] pixels, int size)
        {
            Color32 trayRim = new Color32(205, 124, 74, 255);
            Color32 trayDark = new Color32(132, 74, 45, 255);
            Color32 soil = new Color32(87, 48, 32, 255);
            Color32 stem = new Color32(92, 201, 119, 255);
            Color32 leaf = new Color32(125, 226, 145, 255);
            Color32 leafLight = new Color32(171, 244, 176, 255);

            FillRoundedRect(pixels, size, 58f, 30f, 454f, 148f, 22f, trayRim);
            FillRoundedRect(pixels, size, 72f, 44f, 440f, 138f, 16f, trayDark);
            FillRoundedRect(pixels, size, 84f, 54f, 428f, 132f, 12f, soil);

            // Three seedlings of different heights: a simple, friendly tray illustration.
            DrawSeedling(pixels, size, 170f, 126f, 300f, stem, leaf, leafLight);
            DrawSeedling(pixels, size, 256f, 126f, 250f, stem, leaf, leafLight);
            DrawSeedling(pixels, size, 342f, 126f, 280f, stem, leaf, leafLight);

            FillRoundedRect(pixels, size, 96f, 66f, 416f, 78f, 4f, new Color32(63, 34, 24, 255));
            DrawLine(pixels, size, 140f, 148f, 140f, 164f, 5f, new Color32(232, 177, 126, 255));
            DrawLine(pixels, size, 372f, 148f, 372f, 164f, 5f, new Color32(232, 177, 126, 255));
        }

        private static void DrawSeedling(
            Color32[] pixels,
            int size,
            float x,
            float baseY,
            float topY,
            Color32 stem,
            Color32 leaf,
            Color32 leafLight)
        {
            DrawLine(pixels, size, x, baseY, x, topY, 10f, stem);
            FillCircle(pixels, size, x - size * 0.055f, topY - 14f, size * 0.045f, leaf);
            FillCircle(pixels, size, x + size * 0.060f, topY - 34f, size * 0.052f, leaf);
            FillCircle(pixels, size, x + size * 0.012f, topY + 8f, size * 0.032f, leafLight);
        }

        private static void FillRoundedRect(
            Color32[] pixels,
            int size,
            float x0,
            float y0,
            float x1,
            float y1,
            float radius,
            Color32 color)
        {
            int startX = Mathf.Max(0, Mathf.FloorToInt(x0));
            int endX = Mathf.Min(size - 1, Mathf.CeilToInt(x1));
            int startY = Mathf.Max(0, Mathf.FloorToInt(y0));
            int endY = Mathf.Min(size - 1, Mathf.CeilToInt(y1));
            float width = x1 - x0;
            float height = y1 - y0;

            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    float alpha = RoundedRectAlpha(x + 0.5f - x0, y + 0.5f - y0, width, height, radius);
                    if (alpha <= 0f)
                    {
                        continue;
                    }

                    int index = y * size + x;
                    pixels[index] = Blend(pixels[index], color, alpha);
                }
            }
        }

        private static void DrawWeather(Color32[] pixels, int size)
        {
            Color32 white = new Color32(255, 255, 255, 255);
            FillCircle(pixels, size, size * 0.5f, size * 0.58f, size * 0.18f, white);
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI * 0.25f;
                float x1 = size * 0.5f + Mathf.Cos(angle) * size * 0.28f;
                float y1 = size * 0.58f + Mathf.Sin(angle) * size * 0.28f;
                float x2 = size * 0.5f + Mathf.Cos(angle) * size * 0.36f;
                float y2 = size * 0.58f + Mathf.Sin(angle) * size * 0.36f;
                DrawLine(pixels, size, x1, y1, x2, y2, 5f, white);
            }

            // A lightning bolt for the "solar storm" feeling.
            DrawLine(pixels, size, size * 0.56f, size * 0.44f, size * 0.44f, size * 0.24f, 6f, white);
            DrawLine(pixels, size, size * 0.44f, size * 0.24f, size * 0.58f, size * 0.26f, 6f, white);
            DrawLine(pixels, size, size * 0.58f, size * 0.26f, size * 0.46f, size * 0.10f, 6f, white);
        }

        public static Sprite BuildWeatherControlRoom()
        {
            const int width = 1024;
            const int height = 288;
            Texture2D texture = NewTexture(width, height);
            var pixels = new Color32[width * height];

            Color32 top = new Color32(7, 13, 31, 255);
            Color32 bottom = new Color32(20, 29, 61, 255);
            for (int y = 0; y < height; y++)
            {
                float v = y / (float)(height - 1);
                Color32 sky = Blend(top, bottom, v * 0.75f);
                for (int x = 0; x < width; x++)
                {
                    int hash = unchecked(x * 73856093 ^ y * 19349663);
                    Color32 color = sky;
                    if ((hash & 1023) < 3)
                    {
                        color = Blend(color, new Color32(255, 247, 213, 255), 0.85f);
                    }
                    else if ((hash & 1023) < 6)
                    {
                        color = Blend(color, new Color32(145, 185, 255, 255), 0.55f);
                    }

                    pixels[y * width + x] = color;
                }
            }

            // A warm sun with a soft corona on the left.
            for (int i = 7; i >= 1; i--)
            {
                float radius = 52f + i * 14f;
                Color32 glow = new Color32(255, 171, 72, 255);
                BlendCircle(pixels, width, height, 206f, 156f, radius, glow, 0.035f + (7 - i) * 0.007f);
            }

            FillCircleRect(pixels, width, height, 206f, 156f, 58f, new Color32(255, 188, 73, 255));
            FillCircleRect(pixels, width, height, 198f, 164f, 38f, new Color32(255, 229, 137, 255));

            Color32 ray = new Color32(255, 208, 116, 255);
            for (int i = 0; i < 12; i++)
            {
                float angle = i * Mathf.PI / 6f + 0.12f;
                float x1 = 206f + Mathf.Cos(angle) * 72f;
                float y1 = 156f + Mathf.Sin(angle) * 72f;
                float x2 = 206f + Mathf.Cos(angle) * 92f;
                float y2 = 156f + Mathf.Sin(angle) * 92f;
                DrawLineRect(pixels, width, height, x1, y1, x2, y2, 4f, ray, 0.55f);
            }

            // Mars on the right: a friendly, simplified red planet.
            FillCircleRect(pixels, width, height, 816f, 132f, 76f, new Color32(154, 63, 43, 255));
            FillCircleRect(pixels, width, height, 804f, 144f, 56f, new Color32(196, 91, 57, 255));
            AddCraterRect(pixels, width, height, 796f, 128f, 17f, new Color32(128, 49, 38, 255), 0.65f);
            AddCraterRect(pixels, width, height, 842f, 152f, 11f, new Color32(136, 53, 39, 255), 0.6f);
            AddCraterRect(pixels, width, height, 824f, 104f, 8f, new Color32(109, 40, 31, 255), 0.7f);

            // A tiny outpost dome and antenna, drawn on Mars.
            FillCircleRect(pixels, width, height, 850f, 166f, 20f, new Color32(222, 239, 244, 255));
            FillRect(pixels, width, height, 838f, 150f, 24f, 10f, new Color32(168, 204, 216, 255));
            DrawLineRect(pixels, width, height, 850f, 184f, 850f, 204f, 3f, new Color32(232, 245, 248, 255), 0.9f);
            FillCircleRect(pixels, width, height, 850f, 206f, 4f, new Color32(255, 213, 105, 255));

            // Dashed training-signal arcs between the Sun and Mars.
            for (int arc = 0; arc < 3; arc++)
            {
                float offset = arc * 14f;
                for (int i = 0; i <= 64; i++)
                {
                    if (i % 10 > 4)
                    {
                        continue;
                    }

                    float t = i / 64f;
                    float x = Mathf.Lerp(286f, 730f, t);
                    float y = Mathf.Lerp(174f, 146f, t) + Mathf.Sin(t * Mathf.PI) * (58f + offset);
                    FillCircleRect(pixels, width, height, x, y, 3.2f, new Color32(133, 226, 236, 255));
                }
            }

            texture.SetPixels32(pixels);
            return SaveSprite(texture, "Weather_ControlRoom", Vector4.zero);
        }

        private static void FillRect(
            Color32[] pixels,
            int width,
            int height,
            float x0,
            float y0,
            float rectWidth,
            float rectHeight,
            Color32 color)
        {
            int startX = Mathf.Max(0, Mathf.FloorToInt(x0));
            int endX = Mathf.Min(width - 1, Mathf.CeilToInt(x0 + rectWidth));
            int startY = Mathf.Max(0, Mathf.FloorToInt(y0));
            int endY = Mathf.Min(height - 1, Mathf.CeilToInt(y0 + rectHeight));
            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    pixels[y * width + x] = color;
                }
            }
        }

        private static void FillCircleRect(
            Color32[] pixels,
            int width,
            int height,
            float cx,
            float cy,
            float radius,
            Color32 color)
        {
            int startX = Mathf.Max(0, Mathf.FloorToInt(cx - radius));
            int endX = Mathf.Min(width - 1, Mathf.CeilToInt(cx + radius));
            int startY = Mathf.Max(0, Mathf.FloorToInt(cy - radius));
            int endY = Mathf.Min(height - 1, Mathf.CeilToInt(cy + radius));
            float radiusSquared = radius * radius;
            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    float dx = x + 0.5f - cx;
                    float dy = y + 0.5f - cy;
                    if (dx * dx + dy * dy <= radiusSquared)
                    {
                        pixels[y * width + x] = color;
                    }
                }
            }
        }

        private static void BlendCircle(
            Color32[] pixels,
            int width,
            int height,
            float cx,
            float cy,
            float radius,
            Color32 color,
            float alpha)
        {
            int startX = Mathf.Max(0, Mathf.FloorToInt(cx - radius));
            int endX = Mathf.Min(width - 1, Mathf.CeilToInt(cx + radius));
            int startY = Mathf.Max(0, Mathf.FloorToInt(cy - radius));
            int endY = Mathf.Min(height - 1, Mathf.CeilToInt(cy + radius));
            float radiusSquared = radius * radius;
            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    float dx = x + 0.5f - cx;
                    float dy = y + 0.5f - cy;
                    if (dx * dx + dy * dy <= radiusSquared)
                    {
                        int index = y * width + x;
                        pixels[index] = Blend(pixels[index], color, alpha);
                    }
                }
            }
        }

        private static void AddCraterRect(
            Color32[] pixels,
            int width,
            int height,
            float cx,
            float cy,
            float radius,
            Color32 color,
            float strength)
        {
            for (int y = Mathf.Max(0, Mathf.FloorToInt(cy - radius)); y <= Mathf.Min(height - 1, Mathf.CeilToInt(cy + radius)); y++)
            {
                for (int x = Mathf.Max(0, Mathf.FloorToInt(cx - radius)); x <= Mathf.Min(width - 1, Mathf.CeilToInt(cx + radius)); x++)
                {
                    float dx = x + 0.5f - cx;
                    float dy = y + 0.5f - cy;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    if (distance <= radius)
                    {
                        int index = y * width + x;
                        pixels[index] = Blend(pixels[index], color, strength * (1f - distance / radius));
                    }
                }
            }
        }

        private static void DrawLineRect(
            Color32[] pixels,
            int width,
            int height,
            float x1,
            float y1,
            float x2,
            float y2,
            float thickness,
            Color32 color,
            float alpha)
        {
            float dx = x2 - x1;
            float dy = y2 - y1;
            float lengthSquared = dx * dx + dy * dy;
            if (lengthSquared <= 0.0001f)
            {
                return;
            }

            int startX = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(x1, x2) - thickness));
            int endX = Mathf.Min(width - 1, Mathf.CeilToInt(Mathf.Max(x1, x2) + thickness));
            int startY = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(y1, y2) - thickness));
            int endY = Mathf.Min(height - 1, Mathf.CeilToInt(Mathf.Max(y1, y2) + thickness));

            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    float t = Mathf.Clamp01(((x + 0.5f - x1) * dx + (y + 0.5f - y1) * dy) / lengthSquared);
                    float px = x1 + t * dx;
                    float py = y1 + t * dy;
                    float distance = Mathf.Sqrt((x + 0.5f - px) * (x + 0.5f - px) + (y + 0.5f - py) * (y + 0.5f - py));
                    if (distance <= thickness * 0.5f)
                    {
                        int index = y * width + x;
                        pixels[index] = Blend(pixels[index], color, alpha);
                    }
                }
            }
        }

        private static Texture2D NewTexture(int width, int height)
        {
            return new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
        }

        private static Sprite SaveSprite(Texture2D texture, string name, Vector4 border)
        {
            texture.Apply(false, false);

            string path = $"{ArtRoot}/{name}.png";
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteBorder = border;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static float RoundedRectAlpha(float x, float y, float width, float height, float radius)
        {
            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;
            float qx = Mathf.Abs(x - halfWidth) - (halfWidth - radius);
            float qy = Mathf.Abs(y - halfHeight) - (halfHeight - radius);
            float outside = Mathf.Sqrt(Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) + Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f));
            float distance = outside + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
            return Mathf.Clamp01(0.5f - distance);
        }

        private static void AddMapCrater(
            ref Color32 color,
            int x,
            int y,
            int width,
            int height,
            float cx,
            float cy,
            float radius,
            Color32 crater)
        {
            float dx = x - cx * width;
            float dy = y - cy * height;
            float distance = Mathf.Sqrt(dx * dx + dy * dy);
            float blend = Mathf.Clamp01(1f - distance / (radius * width));
            if (blend > 0f)
            {
                color = Blend(color, crater, blend * 0.7f);
            }
        }

        private static void AddCrater(ref Color32 color, int x, int y, int size, float cx, float cy, float radius, Color32 crater)
        {
            float dx = x - cx * size;
            float dy = y - cy * size;
            float distance = Mathf.Sqrt(dx * dx + dy * dy);
            float blend = Mathf.Clamp01(1f - distance / (radius * size));
            if (blend > 0f)
            {
                color = Blend(color, crater, blend * 0.75f);
            }
        }

        private static Color32 Blend(Color32 from, Color32 to, float t)
        {
            t = Mathf.Clamp01(t);
            return new Color32(
                (byte)Mathf.RoundToInt(Mathf.Lerp(from.r, to.r, t)),
                (byte)Mathf.RoundToInt(Mathf.Lerp(from.g, to.g, t)),
                (byte)Mathf.RoundToInt(Mathf.Lerp(from.b, to.b, t)),
                (byte)Mathf.RoundToInt(Mathf.Lerp(from.a, to.a, t)));
        }

        private static void FillCircle(Color32[] pixels, int size, float cx, float cy, float radius, Color32 color)
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - cx;
                    float dy = y + 0.5f - cy;
                    if (dx * dx + dy * dy <= radius * radius)
                    {
                        pixels[y * size + x] = color;
                    }
                }
            }
        }

        private static void DrawRing(Color32[] pixels, int size, float cx, float cy, float radius, float thickness, Color32 color)
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - cx;
                    float dy = y + 0.5f - cy;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    if (Mathf.Abs(distance - radius) <= thickness * 0.5f)
                    {
                        pixels[y * size + x] = color;
                    }
                }
            }
        }

        private static void DrawLine(Color32[] pixels, int size, float x1, float y1, float x2, float y2, float thickness, Color32 color)
        {
            float minX = Mathf.Min(x1, x2) - thickness;
            float maxX = Mathf.Max(x1, x2) + thickness;
            float minY = Mathf.Min(y1, y2) - thickness;
            float maxY = Mathf.Max(y1, y2) + thickness;

            int startX = Mathf.Max(0, Mathf.FloorToInt(minX));
            int endX = Mathf.Min(size - 1, Mathf.CeilToInt(maxX));
            int startY = Mathf.Max(0, Mathf.FloorToInt(minY));
            int endY = Mathf.Min(size - 1, Mathf.CeilToInt(maxY));

            float dx = x2 - x1;
            float dy = y2 - y1;
            float lengthSquared = dx * dx + dy * dy;
            if (lengthSquared <= 0.0001f)
            {
                return;
            }

            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    float t = Mathf.Clamp01(((x + 0.5f - x1) * dx + (y + 0.5f - y1) * dy) / lengthSquared);
                    float px = x1 + t * dx;
                    float py = y1 + t * dy;
                    float distance = Mathf.Sqrt((x + 0.5f - px) * (x + 0.5f - px) + (y + 0.5f - py) * (y + 0.5f - py));
                    if (distance <= thickness * 0.5f)
                    {
                        pixels[y * size + x] = color;
                    }
                }
            }
        }
    }
}
