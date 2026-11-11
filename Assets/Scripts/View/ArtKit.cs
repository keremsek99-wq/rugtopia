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
                float along = (x + y) % 18f;
                Color c = indigo;
                if (edge < 8f) c = indigo;
                else if (edge < 13f) c = cream;
                else if (edge < 34f)
                {
                    c = madder;
                    float dx = Mathf.Abs(along - 9f);
                    float dy = Mathf.Abs(edge - 23f);
                    if (dx + dy < 3.2f) c = saffron;
                    else if (dx + dy < 4.4f) c = cream;
                }
                else if (edge < 42f) c = indigo;
                else if (edge < 48f) c = saffron;
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

        static bool InEllipse(float x, float y, float cx, float cy, float rx, float ry)
        {
            float dx = (x - cx) / rx, dy = (y - cy) / ry;
            return dx * dx + dy * dy <= 1f;
        }

        static Sprite MakeHand()
        {
            const int w = 96, h = 140;
            var mask = new bool[w * h];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float u = x + 0.5f, v = y + 0.5f;
                bool index = Mathf.Abs(u - 46f) < 11f && v > 6f && v < 92f;
                bool tip = InEllipse(u, v, 46f, 16f, 13f, 16f);
                bool palm = InEllipse(u, v, 52f, 102f, 30f, 26f);
                bool thumb = InEllipse(u, v, 22f, 96f, 16f, 12f);
                bool knuckle = InEllipse(u, v, 70f, 78f, 12f, 16f) || InEllipse(u, v, 78f, 96f, 10f, 14f);
                mask[y * w + x] = index || tip || palm || thumb || knuckle;
            }
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var px = new Color[w * h];
            var skin = ViewKit.Hex("#F3C892");
            var ink = ViewKit.Hex("#2B1A14");
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                if (!mask[y * w + x]) { px[y * w + x] = Color.clear; continue; }
                bool edge = false;
                for (int oy = -2; oy <= 2 && !edge; oy++)
                for (int ox = -2; ox <= 2 && !edge; ox++)
                {
                    int xx = x + ox, yy = y + oy;
                    if (xx < 0 || yy < 0 || xx >= w || yy >= h || !mask[yy * w + xx]) edge = true;
                }
                px[y * w + x] = edge ? ink : skin;
            }
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, w, h), new Vector2(0.5f, 0.08f), 96, 0, SpriteMeshType.FullRect, Vector4.zero);
        }
    }
}
