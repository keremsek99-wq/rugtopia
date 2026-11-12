using System.Collections.Generic;
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
        public static Sprite LinePath { get { return Icon("map-trifold-fill"); } }
        public static Sprite LineGift { get { return Icon("gift-fill"); } }
        public static Sprite LineFlame { get { return Icon("fire-simple-fill"); } }
        public static Sprite PlayMark { get { return Icon("play-fill"); } }
        public static Sprite Gear { get { return Icon("gear-six-fill"); } }
        public static Sprite Heart { get { return Icon("heart-straight-fill"); } }
        public static Sprite Restart { get { return Icon("arrow-clockwise-fill"); } }
        public static Sprite HomeMark { get { return Icon("house-simple-fill"); } }
        public static Sprite BackMark { get { return Icon("caret-left-fill"); } }
        public static Sprite CloseMark { get { return Icon("x-circle-fill"); } }
        public static Sprite CheckMark { get { return Icon("check-fat-fill"); } }
        public static Sprite PackSlot { get { return Icon("cylinder-fill"); } }
        public static Sprite PackShuttle { get { return Icon("boat-fill"); } }

        static readonly Dictionary<string, Sprite> _loaded = new Dictionary<string, Sprite>();

        static Sprite Icon(string name)
        {
            Sprite cached;
            if (_loaded.TryGetValue(name, out cached) && cached != null) return cached;
            var sprite = Resources.Load<Sprite>("Icons/" + name);
            if (sprite == null)
            {
                var tex = Resources.Load<Texture2D>("Icons/" + name);
                if (tex != null)
                    sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.width);
            }
            _loaded[name] = sprite;
            return sprite;
        }

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

        static Sprite MakeRestart()
        {
            const int n = 128;
            var t = Tex(n);
            var px = new Color[n * n];
            float mid = (n - 1) * 0.5f;
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float dx = x - mid, dy = y - mid;
                float rad = Mathf.Sqrt(dx * dx + dy * dy) / mid;
                float ang = Mathf.Atan2(dy, dx);
                bool arc = rad > 0.46f && rad < 0.68f && ang > -2.4f && ang < 2.2f;
                if (arc) px[y * n + x] = Color.white;
            }
            FillTri(px, n, 92f, 78f, 116f, 58f, 86f, 52f);
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        static void FillTri(Color[] px, int n, float ax, float ay, float bx, float by, float cx, float cy)
        {
            float minX = Mathf.Min(ax, Mathf.Min(bx, cx));
            float maxX = Mathf.Max(ax, Mathf.Max(bx, cx));
            float minY = Mathf.Min(ay, Mathf.Min(by, cy));
            float maxY = Mathf.Max(ay, Mathf.Max(by, cy));
            for (int y = Mathf.Max(0, Mathf.FloorToInt(minY)); y <= Mathf.Min(n - 1, Mathf.CeilToInt(maxY)); y++)
            for (int x = Mathf.Max(0, Mathf.FloorToInt(minX)); x <= Mathf.Min(n - 1, Mathf.CeilToInt(maxX)); x++)
            {
                float w0 = (bx - ax) * (y - ay) - (by - ay) * (x - ax);
                float w1 = (cx - bx) * (y - by) - (cy - by) * (x - bx);
                float w2 = (ax - cx) * (y - cy) - (ay - cy) * (x - cx);
                if ((w0 >= 0 && w1 >= 0 && w2 >= 0) || (w0 <= 0 && w1 <= 0 && w2 <= 0)) px[y * n + x] = Color.white;
            }
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

        static Sprite MakeBack()
        {
            const int n = 128;
            var t = Tex(n);
            var px = new Color[n * n];
            StrokeLine(px, n, 34f, 64f, 96f, 104f, 18f);
            StrokeLine(px, n, 34f, 64f, 96f, 24f, 18f);
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        static Sprite MakeClose()
        {
            const int n = 128;
            var t = Tex(n);
            var px = new Color[n * n];
            StrokeLine(px, n, 28f, 28f, 100f, 100f, 16f);
            StrokeLine(px, n, 28f, 100f, 100f, 28f, 16f);
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        static Sprite MakeCheck()
        {
            const int n = 128;
            var t = Tex(n);
            var px = new Color[n * n];
            StrokeLine(px, n, 22f, 62f, 52f, 28f, 16f);
            StrokeLine(px, n, 46f, 32f, 108f, 100f, 18f);
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        static Sprite MakeHome()
        {
            const int n = 128;
            var t = Tex(n);
            var px = new Color[n * n];
            FillRect(px, n, 30f, 16f, 98f, 68f);
            FillTri(px, n, 16f, 60f, 112f, 60f, 64f, 112f);
            ClearRect(px, n, 52f, 16f, 76f, 46f);
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        static Sprite MakePlayMark()
        {
            const int n = 128;
            var t = Tex(n);
            var px = new Color[n * n];
            FillTri(px, n, 6f, 8f, 6f, 120f, 122f, 64f);
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        static Sprite MakeLinePath()
        {
            const int n = 128;
            var t = Tex(n);
            var px = new Color[n * n];
            StrokeLine(px, n, 30f, 98f, 64f, 64f, 16f);
            StrokeLine(px, n, 64f, 64f, 98f, 30f, 16f);
            FillDisc(px, n, 30f, 98f, 16f);
            FillDisc(px, n, 64f, 64f, 16f);
            FillDisc(px, n, 98f, 30f, 16f);
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        static Sprite MakeLineGift()
        {
            const int n = 128;
            var t = Tex(n);
            var px = new Color[n * n];
            FillRect(px, n, 16f, 14f, 112f, 72f);
            FillDisc(px, n, 38f, 90f, 20f);
            FillDisc(px, n, 90f, 90f, 20f);
            FillDisc(px, n, 64f, 74f, 13f);
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        static Sprite MakeLineFlame()
        {
            const int n = 128;
            var t = Tex(n);
            var px = new Color[n * n];
            Tongue(px, n, 32f, 28f, 64f, 30f);
            Tongue(px, n, 96f, 24f, 72f, 30f);
            Tongue(px, n, 64f, 6f, 116f, 34f);
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        static void Tongue(Color[] px, int n, float cx, float baseY, float height, float width)
        {
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float t = (y - baseY) / height;
                if (t < 0f || t > 1f) continue;
                float pinch = Mathf.Pow(1f - t, 0.72f);
                float belly = Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI);
                float half = width * pinch * (0.42f + 0.7f * belly);
                if (Mathf.Abs(x - cx) <= half) px[y * n + x] = Color.white;
            }
        }

        static void ClearRect(Color[] px, int n, float x0, float y0, float x1, float y1)
        {
            int a = Mathf.Clamp(Mathf.RoundToInt(Mathf.Min(x0, x1)), 0, n - 1);
            int b = Mathf.Clamp(Mathf.RoundToInt(Mathf.Max(x0, x1)), 0, n - 1);
            int c = Mathf.Clamp(Mathf.RoundToInt(Mathf.Min(y0, y1)), 0, n - 1);
            int d = Mathf.Clamp(Mathf.RoundToInt(Mathf.Max(y0, y1)), 0, n - 1);
            for (int y = c; y <= d; y++)
            for (int x = a; x <= b; x++)
                px[y * n + x] = Color.clear;
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
