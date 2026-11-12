using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Flat motif sprites used on rug cells, NEXT chips and spools. Same set as the HTML prototype.</summary>
    public static class Glyphs
    {
        static Sprite _diamond, _cross, _circle, _square, _star, _chevron, _box, _ring, _dot, _slot, _comb, _shuttle, _solidCircle, _solidStar;

        public static Sprite Of(char c)
        {
            switch (c)
            {
                case 'R': return Diamond;
                case 'Y': return Star;
                case 'N': return Circle;
                case 'C': return Cross;
                case 'K': return Square;
                case 'T': return Circle;
                default: return Dot;
            }
        }

        public static Sprite Diamond { get { if (_diamond == null) _diamond = Poly(4, 0.78f, true); return _diamond; } }
        public static Sprite Cross { get { if (_cross == null) _cross = Plus(); return _cross; } }
        public static Sprite Circle { get { if (_circle == null) _circle = Disc(false); return _circle; } }
        public static Sprite Square { get { if (_square == null) _square = Frame(true); return _square; } }
        public static Sprite Star { get { if (_star == null) _star = Poly(5, 0.45f, true); return _star; } }
        public static Sprite Chevron { get { if (_chevron == null) _chevron = Chevrons(); return _chevron; } }
        public static Sprite Box { get { if (_box == null) _box = Frame(false); return _box; } }
        public static Sprite Ring { get { if (_ring == null) _ring = Disc(true); return _ring; } }
        public static Sprite Dot { get { if (_dot == null) _dot = Disc(false); return _dot; } }
        public static Sprite SlotIcon { get { if (_slot == null) _slot = Pocket(); return _slot; } }
        public static Sprite CombIcon { get { if (_comb == null) _comb = Teeth(); return _comb; } }
        public static Sprite ShuttleIcon { get { if (_shuttle == null) _shuttle = Boat(); return _shuttle; } }
        public static Sprite SolidCircle { get { if (_solidCircle == null) _solidCircle = HardDisc(); return _solidCircle; } }
        public static Sprite SolidStar { get { if (_solidStar == null) _solidStar = HardStar(); return _solidStar; } }

        static Sprite Disc(bool ring)
        {
            const int n = 64;
            var t = Tex(n);
            var px = new Color[n * n];
            float mid = (n - 1) * 0.5f;
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float d = Mathf.Sqrt((x - mid) * (x - mid) + (y - mid) * (y - mid)) / mid;
                float a = ring ? Mathf.Clamp01(1f - Mathf.Abs(d - 0.78f) / 0.12f) : Mathf.Clamp01((0.92f - d) * 8f);
                px[y * n + x] = new Color(1f, 1f, 1f, a);
            }
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        static Sprite Frame(bool fill)
        {
            const int n = 64;
            var t = Tex(n);
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                int e = Mathf.Min(Mathf.Min(x, y), Mathf.Min(n - 1 - x, n - 1 - y));
                bool on = fill ? (x > 8 && y > 8 && x < n - 9 && y < n - 9) : (e < 5);
                px[y * n + x] = on ? Color.white : Color.clear;
            }
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        static Sprite Plus()
        {
            const int n = 64;
            var t = Tex(n);
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                bool on = (Mathf.Abs(x - 32) < 8 && y > 8 && y < 56) || (Mathf.Abs(y - 32) < 8 && x > 8 && x < 56);
                px[y * n + x] = on ? Color.white : Color.clear;
            }
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        static Sprite Poly(int sides, float inner, bool diamond)
        {
            const int n = 64;
            var t = Tex(n);
            var px = new Color[n * n];
            float mid = (n - 1) * 0.5f;
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float dx = (x - mid) / mid, dy = (y - mid) / mid;
                float ang = Mathf.Atan2(dy, dx);
                float rad = Mathf.Sqrt(dx * dx + dy * dy);
                bool on;
                if (diamond && sides == 4)
                    on = Mathf.Abs(dx) + Mathf.Abs(dy) < 0.82f;
                else
                {
                    float slice = Mathf.PI * 2f / (sides * 2f);
                    float m = (ang / slice) % 2f;
                    if (m < 0) m += 2f;
                    float lim = Mathf.Lerp(1f, inner, Mathf.Abs(m - 1f));
                    on = rad < lim * 0.9f;
                }
                px[y * n + x] = on ? Color.white : Color.clear;
            }
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        static Sprite Chevrons()
        {
            const int n = 64;
            var t = Tex(n);
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float u = x / (float)n, v = y / (float)n;
                bool on = NearV(u, v, 0.28f) || NearV(u, v, 0.62f);
                px[y * n + x] = on ? Color.white : Color.clear;
            }
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        static bool NearV(float u, float v, float cy)
        {
            float d = Mathf.Abs(v - (cy + Mathf.Abs(u - 0.5f) * 0.55f));
            return d < 0.045f && u > 0.12f && u < 0.88f;
        }

        static Sprite Pocket()
        {
            const int n = 64;
            var t = Tex(n);
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float u = (x + 0.5f) / n, v = (y + 0.5f) / n;
                float dx = Mathf.Abs(u - 0.5f) / 0.38f, dy = Mathf.Abs(v - 0.5f) / 0.22f;
                bool on = dx * dx + dy * dy <= 1f;
                px[y * n + x] = on ? Color.white : Color.clear;
            }
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        static Sprite Teeth()
        {
            const int n = 64;
            var t = Tex(n);
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                bool bar = y > 40 && y < 50 && x > 8 && x < 56;
                int tooth = (x - 10) / 12;
                bool tine = tooth >= 0 && tooth < 4 && x % 12 > 2 && x % 12 < 9 && y > 14 && y < 46;
                px[y * n + x] = (bar || tine) ? Color.white : Color.clear;
            }
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        static Sprite Boat()
        {
            const int n = 64;
            var t = Tex(n);
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float u = (x + 0.5f) / n, v = (y + 0.5f) / n;
                float body = Mathf.Abs(v - 0.55f) / 0.16f + Mathf.Abs(u - 0.42f) / 0.28f;
                bool nose = u > 0.55f && u < 0.88f && Mathf.Abs(v - 0.55f) < (0.88f - u) * 0.55f;
                px[y * n + x] = (body < 1f || nose) ? Color.white : Color.clear;
            }
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        static Sprite HardDisc()
        {
            const int n = 256;
            var t = Tex(n);
            var px = new Color[n * n];
            float mid = (n - 1) * 0.5f;
            float aa = 1.25f / mid;
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float d = Mathf.Sqrt((x - mid) * (x - mid) + (y - mid) * (y - mid)) / mid;
                float a = Mathf.Clamp01((0.96f - d) / aa);
                px[y * n + x] = new Color(1f, 1f, 1f, a);
            }
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        static Sprite HardStar()
        {
            const int n = 256;
            var t = Tex(n);
            var px = new Color[n * n];
            float mid = (n - 1) * 0.5f;
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float dx = (x - mid) / mid, dy = (y - mid) / mid;
                float ang = Mathf.Atan2(dy, dx);
                float rad = Mathf.Sqrt(dx * dx + dy * dy);
                float slice = Mathf.PI * 2f / 10f;
                float m = (ang / slice) % 2f;
                if (m < 0) m += 2f;
                float lim = Mathf.Lerp(1f, 0.42f, Mathf.Abs(m - 1f));
                px[y * n + x] = rad < lim * 0.92f ? Color.white : Color.clear;
            }
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        static Texture2D Tex(int n)
        {
            return new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
        }
    }
}
