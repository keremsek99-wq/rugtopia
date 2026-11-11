using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Procedural textures: workshop wall, wood floor, smooth kilim frame, flat UI shapes.</summary>
    public static class ArtKit
    {
        static Sprite _wall, _floor, _vignette, _shadow, _wood, _wound, _frame, _ui, _card, _coin, _hand;

        public static Sprite Wall { get { if (_wall == null) _wall = MakeWall(); return _wall; } }
        public static Sprite Floor { get { if (_floor == null) _floor = MakeFloor(); return _floor; } }
        public static Sprite Vignette { get { if (_vignette == null) _vignette = MakeVignette(); return _vignette; } }
        public static Sprite Shadow { get { if (_shadow == null) _shadow = MakeShadow(); return _shadow; } }
        public static Sprite Wood { get { if (_wood == null) _wood = MakeWood(); return _wood; } }
        public static Sprite Wound { get { if (_wound == null) _wound = MakeWound(); return _wound; } }
        public static Sprite KilimFrame { get { if (_frame == null) _frame = MakeFrame(); return _frame; } }
        public static Sprite UiRound { get { if (_ui == null) _ui = MakeUi(); return _ui; } }
        public static Sprite Card { get { if (_card == null) _card = MakeCard(); return _card; } }
        public static Sprite Coin { get { if (_coin == null) _coin = MakeCoin(); return _coin; } }
        public static Sprite Hand { get { if (_hand == null) _hand = MakeHand(); return _hand; } }

        static Sprite SpriteOf(Texture2D t, float ppu, Vector4 border)
        {
            return Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), ppu, 0, SpriteMeshType.FullRect, border);
        }

        static Sprite MakeWall()
        {
            const int n = 256;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Repeat };
            var px = new Color[n * n];
            var plaster = ViewKit.Hex("#C4785A");
            var plasterDeep = ViewKit.Hex("#A85E45");
            var indigo = ViewKit.Hex("#2E4A9E");
            var saffron = ViewKit.Hex("#F5B700");
            var cream = ViewKit.Hex("#F6ECD9");
            var madder = ViewKit.Hex("#C9343F");
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                int cx = x % 64, cy = y % 64;
                int man = Mathf.Abs(cx - 32) + Mathf.Abs(cy - 32);
                float grain = 0.94f + 0.06f * Mathf.Sin((x * 0.17f) + Mathf.Sin(y * 0.08f));
                Color c = Color.Lerp(plasterDeep, plaster, grain);
                if (man < 7) c = indigo;
                else if (man < 11) c = saffron;
                else if (man < 14) c = cream;
                else if (Mathf.Abs(cx - 32) < 2 || Mathf.Abs(cy - 32) < 2) c = Color.Lerp(c, madder, 0.85f);
                px[y * n + x] = c;
            }
            t.SetPixels(px); t.Apply();
            return SpriteOf(t, 64f, Vector4.zero);
        }

        static Sprite MakeFloor()
        {
            const int w = 128, h = 64;
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Repeat };
            var px = new Color[w * h];
            var a = ViewKit.Hex("#7A4A2C");
            var b = ViewKit.Hex("#5C341F");
            var seam = ViewKit.Hex("#3E2416");
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int plank = y / 16;
                Color c = (plank % 2 == 0) ? a : b;
                float g = 0.9f + 0.1f * Mathf.Sin(x * 0.35f + plank);
                c *= g;
                if (y % 16 == 0 || (x + plank * 20) % 64 == 0) c = seam;
                px[y * w + x] = c;
            }
            t.SetPixels(px); t.Apply();
            return SpriteOf(t, 32f, Vector4.zero);
        }

        static Sprite MakeVignette()
        {
            const int n = 128;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float u = (x + 0.5f) / n * 2f - 1f, v = (y + 0.5f) / n * 2f - 1f;
                float e = Mathf.Max(Mathf.Abs(u), Mathf.Abs(v) * 0.92f);
                float a = Mathf.SmoothStep(0.62f, 1.05f, e);
                px[y * n + x] = new Color(0.08f, 0.04f, 0.06f, a * 0.8f);
            }
            t.SetPixels(px); t.Apply();
            return SpriteOf(t, n, Vector4.zero);
        }

        static Sprite MakeShadow()
        {
            const int n = 128;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float u = (x + 0.5f) / n * 2f - 1f, v = (y + 0.5f) / n * 2f - 1f;
                float a = Mathf.Clamp01(1f - (u * u * 0.7f + v * v * 2.4f));
                px[y * n + x] = new Color(0.12f, 0.06f, 0.04f, a * a * 0.6f);
            }
            t.SetPixels(px); t.Apply();
            return SpriteOf(t, n, Vector4.zero);
        }

        static Sprite MakeWood()
        {
            const int n = 64;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Repeat };
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float g = 0.78f + 0.22f * Mathf.Sin(y * 0.85f + Mathf.Sin(x * 0.2f));
                px[y * n + x] = new Color(g, g * 0.86f, g * 0.62f, 1f);
            }
            t.SetPixels(px); t.Apply();
            return SpriteOf(t, n, Vector4.zero);
        }

        static Sprite MakeWound()
        {
            const int n = 64;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Repeat };
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float thread = Mathf.Abs(Mathf.Sin(y * 0.85f));
                float g = Mathf.Lerp(0.72f, 1f, thread);
                px[y * n + x] = new Color(g, g, g, 1f);
            }
            t.SetPixels(px); t.Apply();
            return SpriteOf(t, n, Vector4.zero);
        }

        static Sprite MakeFrame()
        {
            const int n = 512;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var px = new Color[n * n];
            var indigo = ViewKit.Hex("#2E4A9E");
            var madder = ViewKit.Hex("#C9343F");
            var saffron = ViewKit.Hex("#F5B700");
            var cream = ViewKit.Hex("#F6ECD9");
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                Color acc = Color.clear;
                for (int sy = 0; sy < 2; sy++)
                for (int sx = 0; sx < 2; sx++)
                {
                    float pxX = x + (sx + 0.5f) * 0.5f;
                    float pxY = y + (sy + 0.5f) * 0.5f;
                    acc += FramePixel(pxX, pxY, n, indigo, madder, saffron, cream);
                }
                px[y * n + x] = acc * 0.25f;
            }
            t.SetPixels(px); t.Apply();
            return SpriteOf(t, n, Vector4.zero);
        }

        static Color FramePixel(float x, float y, int n, Color indigo, Color madder, Color saffron, Color cream)
        {
            float edge = Mathf.Min(Mathf.Min(x, y), Mathf.Min(n - x, n - y));
            const float outer = 56f;
            if (edge >= outer + 1.2f) return Color.clear;
            float a = Mathf.Clamp01(outer + 0.8f - edge);
            float along = (x + y) % 28f;
            Color c = indigo;
            if (edge < 8f) c = indigo;
            else if (edge < 14f) c = cream;
            else if (edge < 36f)
            {
                c = madder;
                float dx = Mathf.Abs(along - 14f);
                float dy = Mathf.Abs(edge - 24f);
                if (dx + dy < 7f) c = saffron;
                else if (dx + dy < 9f) c = cream;
            }
            else if (edge < 44f) c = indigo;
            else c = cream;
            // Soften the band boundaries.
            c.a = a;
            return c;
        }

        static Sprite MakeUi()
        {
            const int n = 96;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var px = new Color[n * n];
            float r = 28f;
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float dx = Mathf.Abs(x + 0.5f - n * 0.5f), dy = Mathf.Abs(y + 0.5f - n * 0.5f);
                float ax = Mathf.Max(dx - (n * 0.5f - r), 0f), ay = Mathf.Max(dy - (n * 0.5f - r), 0f);
                float a = Mathf.Clamp01(r - Mathf.Sqrt(ax * ax + ay * ay) + 0.6f);
                px[y * n + x] = new Color(1f, 1f, 1f, a);
            }
            t.SetPixels(px); t.Apply();
            return SpriteOf(t, n, new Vector4(32, 32, 32, 32));
        }

        static Sprite MakeCard()
        {
            const int n = 96;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var px = new Color[n * n];
            float r = 18f;
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float dx = Mathf.Abs(x + 0.5f - n * 0.5f), dy = Mathf.Abs(y + 0.5f - n * 0.5f);
                float ax = Mathf.Max(dx - (n * 0.5f - r), 0f), ay = Mathf.Max(dy - (n * 0.5f - r), 0f);
                float a = Mathf.Clamp01(r - Mathf.Sqrt(ax * ax + ay * ay) + 0.6f);
                px[y * n + x] = new Color(1f, 1f, 1f, a);
            }
            t.SetPixels(px); t.Apply();
            return SpriteOf(t, n, new Vector4(22, 22, 22, 22));
        }

        static Sprite MakeCoin()
        {
            const int n = 64;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var px = new Color[n * n];
            var gold = ViewKit.Hex("#F5B700");
            var deep = ViewKit.Hex("#C48A10");
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float u = (x + 0.5f) / n * 2f - 1f, v = (y + 0.5f) / n * 2f - 1f;
                float d = Mathf.Sqrt(u * u + v * v);
                float a = Mathf.Clamp01((0.92f - d) * 8f);
                Color c = Color.Lerp(deep, gold, Mathf.Clamp01(0.5f - v));
                if (d > 0.62f && d < 0.78f) c = Color.Lerp(c, Color.white, 0.55f);
                c.a = a;
                px[y * n + x] = c;
            }
            t.SetPixels(px); t.Apply();
            return SpriteOf(t, n, Vector4.zero);
        }

        static Sprite MakeHand()
        {
            const int w = 64, h = 96;
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var px = new Color[w * h];
            var skin = ViewKit.Hex("#F6D7B0");
            var ink = ViewKit.Hex("#2B1A14");
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float u = (x + 0.5f) / w, v = (y + 0.5f) / h;
                bool finger = Mathf.Abs(u - 0.5f) < 0.12f && v < 0.62f;
                bool palm = (u - 0.5f) * (u - 0.5f) / 0.12f + (v - 0.72f) * (v - 0.72f) / 0.06f < 1f;
                float d = finger || palm ? 0f : 1f;
                // Soft outline by sampling the shape distance roughly.
                float du = Mathf.Abs(u - 0.5f);
                float edge = finger ? (0.12f - du) * 8f : (palm ? 1f : 0f);
                if (finger) edge = Mathf.Clamp01((0.12f - du) * 14f) * Mathf.Clamp01(v < 0.58f ? 1f : (0.64f - v) * 16f);
                if (palm)
                {
                    float e = 1f - ((u - 0.5f) * (u - 0.5f) / 0.12f + (v - 0.72f) * (v - 0.72f) / 0.06f);
                    edge = Mathf.Max(edge, Mathf.Clamp01(e * 6f));
                }
                Color c = Color.Lerp(ink, skin, Mathf.Clamp01((edge - 0.35f) * 3f));
                c.a = Mathf.Clamp01(edge);
                px[y * w + x] = c;
            }
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, w, h), new Vector2(0.5f, 0.08f), 64, 0, SpriteMeshType.FullRect, Vector4.zero);
        }
    }
}
