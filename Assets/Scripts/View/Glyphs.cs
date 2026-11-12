using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Flat motif sprites used on rug cells, NEXT chips and spools. Same set as the HTML prototype.</summary>
    public static class Glyphs
    {
        static Sprite _diamond, _cross, _circle, _square, _star, _chevron, _box, _ring, _dot, _slot, _comb, _shuttle, _solidCircle, _solidStar, _linePath, _lineGift, _lineFlame, _gear, _heart;

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
        public static Sprite LinePath { get { if (_linePath == null) _linePath = MakeLinePath(); return _linePath; } }
        public static Sprite LineGift { get { if (_lineGift == null) _lineGift = MakeLineGift(); return _lineGift; } }
        public static Sprite LineFlame { get { if (_lineFlame == null) _lineFlame = MakeLineFlame(); return _lineFlame; } }
        public static Sprite Gear { get { if (_gear == null) _gear = MakeGear(); return _gear; } }
        public static Sprite Heart { get { if (_heart == null) _heart = MakeHeart(); return _heart; } }

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

        static Sprite MakeGear()
        {
            const int n = 128;
            var t = Tex(n);
            var px = new Color[n * n];
            float mid = (n - 1) * 0.5f;
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float dx = (x - mid) / mid, dy = (y - mid) / mid;
                float rad = Mathf.Sqrt(dx * dx + dy * dy);
                float ang = Mathf.Atan2(dy, dx);
                float tooth = Mathf.Abs(Mathf.Repeat(ang / (Mathf.PI * 2f) * 8f, 1f) - 0.5f);
                bool teeth = rad > 0.62f && rad < 0.94f && tooth < 0.28f;
                bool ring = rad > 0.38f && rad < 0.62f;
                bool hub = rad < 0.2f;
                if (teeth || ring || hub) px[y * n + x] = Color.white;
            }
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        static Sprite MakeHeart()
        {
            const int n = 128;
            var t = Tex(n);
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float nx = (x - n * 0.5f) / (n * 0.36f);
                float ny = (y - n * 0.42f) / (n * 0.36f);
                float a = nx * nx + ny * ny - 1f;
                if (a * a * a - nx * nx * ny * ny * ny < 0f) px[y * n + x] = Color.white;
            }
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        static Sprite MakeLinePath()
        {
            const int n = 128;
            var t = Tex(n);
            var px = new Color[n * n];
            StrokeLine(px, n, 34f, 96f, 64f, 64f, 5.5f);
            StrokeLine(px, n, 64f, 64f, 96f, 34f, 5.5f);
            StrokeDisc(px, n, 34f, 96f, 11f, 5f);
            StrokeDisc(px, n, 64f, 64f, 11f, 5f);
            StrokeDisc(px, n, 96f, 34f, 11f, 5f);
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        static Sprite MakeLineGift()
        {
            const int n = 128;
            var t = Tex(n);
            var px = new Color[n * n];
            StrokeRect(px, n, 34f, 22f, 94f, 74f, 6f);
            FillRect(px, n, 58f, 22f, 70f, 74f);
            FillRect(px, n, 34f, 56f, 94f, 68f);
            StrokeDisc(px, n, 50f, 90f, 14f, 5.5f);
            StrokeDisc(px, n, 78f, 90f, 14f, 5.5f);
            FillDisc(px, n, 64f, 78f, 7f);
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        static Sprite MakeLineFlame()
        {
            const int n = 128;
            var t = Tex(n);
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float u = (x + 0.5f) / n;
                float v = (y + 0.5f) / n;
                float taper = Mathf.Lerp(0.34f, 0.04f, Mathf.Clamp01((v - 0.18f) / 0.72f));
                float dx = Mathf.Abs(u - 0.5f);
                bool outer = dx < taper && v > 0.14f && v < 0.92f;
                float innerT = taper * 0.55f;
                bool inner = dx < innerT && v > 0.24f && v < 0.78f;
                if (outer && !inner) px[y * n + x] = Color.white;
            }
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        static void FillRect(Color[] px, int n, float x0, float y0, float x1, float y1)
        {
            int a = Mathf.Clamp(Mathf.RoundToInt(x0), 0, n - 1);
            int b = Mathf.Clamp(Mathf.RoundToInt(x1), 0, n - 1);
            int c = Mathf.Clamp(Mathf.RoundToInt(y0), 0, n - 1);
            int d = Mathf.Clamp(Mathf.RoundToInt(y1), 0, n - 1);
            for (int y = Mathf.Min(c, d); y <= Mathf.Max(c, d); y++)
            for (int x = Mathf.Min(a, b); x <= Mathf.Max(a, b); x++)
                px[y * n + x] = Color.white;
        }

        static void StrokeRect(Color[] px, int n, float x0, float y0, float x1, float y1, float w)
        {
            FillRect(px, n, x0, y0, x1, y0 + w);
            FillRect(px, n, x0, y1 - w, x1, y1);
            FillRect(px, n, x0, y0, x0 + w, y1);
            FillRect(px, n, x1 - w, y0, x1, y1);
        }

        static void FillDisc(Color[] px, int n, float cx, float cy, float r)
        {
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float dx = x - cx, dy = y - cy;
                if (dx * dx + dy * dy <= r * r) px[y * n + x] = Color.white;
            }
        }

        static void StrokeDisc(Color[] px, int n, float cx, float cy, float r, float w)
        {
            float inner = Mathf.Max(0f, r - w);
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                if (d <= r && d >= inner) px[y * n + x] = Color.white;
            }
        }

        static void StrokeLine(Color[] px, int n, float x0, float y0, float x1, float y1, float w)
        {
            float half = w * 0.5f;
            float len2 = (x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0);
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float t = len2 < 0.01f ? 0f : Mathf.Clamp01(((x - x0) * (x1 - x0) + (y - y0) * (y1 - y0)) / len2);
                float px0 = x0 + (x1 - x0) * t;
                float py0 = y0 + (y1 - y0) * t;
                float dx = x - px0, dy = y - py0;
                if (dx * dx + dy * dy <= half * half) px[y * n + x] = Color.white;
            }
        }

        static Texture2D Tex(int n)
        {
            return new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
        }
    }
}
