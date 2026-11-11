using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Procedural textures for the workshop, kilim frame, wood and wound thread.</summary>
    public static class ArtKit
    {
        static Sprite _wall, _vignette, _shadow, _wood, _wound, _frame, _ui;

        public static Sprite Wall { get { if (_wall == null) _wall = MakeWall(); return _wall; } }
        public static Sprite Vignette { get { if (_vignette == null) _vignette = MakeVignette(); return _vignette; } }
        public static Sprite Shadow { get { if (_shadow == null) _shadow = MakeShadow(); return _shadow; } }
        public static Sprite Wood { get { if (_wood == null) _wood = MakeWood(); return _wood; } }
        public static Sprite Wound { get { if (_wound == null) _wound = MakeWound(); return _wound; } }
        public static Sprite KilimFrame { get { if (_frame == null) _frame = MakeFrame(); return _frame; } }
        public static Sprite UiRound { get { if (_ui == null) _ui = MakeUi(); return _ui; } }

        static Sprite MakeWall()
        {
            const int n = 128;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Repeat };
            var px = new Color[n * n];
            var cream = ViewKit.Hex("#F3E2C8");
            var madder = ViewKit.Hex("#C4554A");
            var walnut = ViewKit.Hex("#6B422C");
            var saffron = ViewKit.Hex("#E2B15A");
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                int cx = x % 32, cy = y % 32;
                int d = Mathf.Abs(cx - 16) + Mathf.Abs(cy - 16);
                Color c = cream;
                if (d < 4) c = madder;
                else if (d < 7) c = saffron;
                else if ((cx + cy) % 16 == 0) c = walnut;
                px[y * n + x] = c;
            }
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 32);
        }

        static Sprite MakeVignette()
        {
            const int n = 64;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float u = (x + 0.5f) / n * 2f - 1f, v = (y + 0.5f) / n * 2f - 1f;
                float e = Mathf.Clamp01((u * u + v * v - 0.35f) / 0.9f);
                px[y * n + x] = new Color(0.12f, 0.07f, 0.05f, e * 0.55f);
            }
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        static Sprite MakeShadow()
        {
            const int n = 64;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float u = (x + 0.5f) / n * 2f - 1f, v = (y + 0.5f) / n * 2f - 1f;
                float a = Mathf.Clamp01(1f - (u * u * 0.65f + v * v * 2.2f));
                px[y * n + x] = new Color(0.15f, 0.08f, 0.05f, a * a * 0.55f);
            }
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        static Sprite MakeWood()
        {
            const int n = 32;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Repeat };
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float g = 0.82f + 0.18f * Mathf.Sin(y * 1.7f + Mathf.Sin(x * 0.4f));
                px[y * n + x] = new Color(g, g * 0.92f, g * 0.75f, 1f);
            }
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        static Sprite MakeWound()
        {
            const int n = 32;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Repeat };
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float g = (y % 3 == 0) ? 0.78f : 1f;
                px[y * n + x] = new Color(g, g, g, 1f);
            }
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        static Sprite MakeFrame()
        {
            const int n = 128;
            const int b = 18;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var px = new Color[n * n];
            var cream = ViewKit.Hex("#F6ECD9");
            var madder = ViewKit.Hex("#C9343F");
            var walnut = ViewKit.Hex("#4B2C20");
            var saffron = ViewKit.Hex("#F5B700");
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                int edge = Mathf.Min(Mathf.Min(x, y), Mathf.Min(n - 1 - x, n - 1 - y));
                Color c = new Color(0, 0, 0, 0);
                if (edge < b)
                {
                    int band = edge / 4;
                    c = band == 0 ? walnut : band == 1 ? saffron : band == 2 ? madder : cream;
                    if ((x + y) % 8 < 2 && edge > 3 && edge < b - 2) c = walnut;
                }
                px[y * n + x] = c;
            }
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n, 0, SpriteMeshType.FullRect, new Vector4(b, b, b, b));
        }

        static Sprite MakeUi()
        {
            const int n = 64;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var px = new Color[n * n];
            float r = 22f;
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float dx = Mathf.Abs(x + 0.5f - n * 0.5f), dy = Mathf.Abs(y + 0.5f - n * 0.5f);
                float ax = Mathf.Max(dx - (n * 0.5f - r), 0f), ay = Mathf.Max(dy - (n * 0.5f - r), 0f);
                float a = Mathf.Clamp01(r - Mathf.Sqrt(ax * ax + ay * ay) + 0.5f);
                float shade = y > n * 0.55f ? 1.06f : 0.96f;
                px[y * n + x] = new Color(shade, shade, shade, a);
            }
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n, 0, SpriteMeshType.FullRect, new Vector4(24, 24, 24, 24));
        }
    }
}
