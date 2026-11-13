using System.Collections.Generic;
using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Placeholder art helpers: a 1x1-unit white sprite tinted per colour, and world-space labels.
    /// Real art (rug textures, spool models) replaces these in week 3.</summary>
    public static class ViewKit
    {
        static Sprite _square;
        static Font _font;

        public static readonly Dictionary<char, Color> Palette = new Dictionary<char, Color>
        {
            {'R', Hex("#C9343F")}, {'Y', Hex("#F5B700")}, {'N', Hex("#2E4A9E")}, {'C', Hex("#F6ECD9")}, {'K', Hex("#4B2C20")},
            {'T', Hex("#62C4F0")}, {'O', Hex("#F08A3C")}, {'G', Hex("#12A08F")}, {'M', Hex("#F07C9A")}, {'L', Hex("#7BB661")},
            {'W', Hex("#FFFFFF")}, {'B', Hex("#22222A")}, {'I', Hex("#1E2F66")}, {'U', Hex("#9FD8F5")}, {'S', Hex("#F7E15A")},
        };
        public static readonly Color Background = Hex("#2B1A14");
        public static readonly Color Letterbox = Hex("#22140F");
        public static readonly Color Cream = Hex("#F6E7CF");
        public static readonly Color Gold = Hex("#E0A526");
        public static readonly Color Green = Hex("#3E7C59");
        public static readonly Color Unwoven = Hex("#E4D2B8");
        public static readonly Color Veil = Hex("#B7A48C");
        public static readonly Color Ink = Hex("#2B1A14");
        public static readonly Color Wool = Hex("#8C5A3C");

        public static Color Hex(string h) { Color c; ColorUtility.TryParseHtmlString(h, out c); return c; }
        public static Color Of(char c) { Color v; return Palette.TryGetValue(c, out v) ? v : Color.magenta; }
        public static Color InkFor(char c) { var k = Of(c); return (0.299f * k.r + 0.587f * k.g + 0.114f * k.b) > 0.59f ? Ink : Color.white; }

        public static Sprite Square
        {
            get
            {
                if (_square != null) return _square;
                const int n = 8;
                var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
                var px = new Color[n * n];
                for (int i = 0; i < px.Length; i++) px[i] = Color.white;
                t.SetPixels(px); t.Apply();
                _square = Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
                return _square;
            }
        }

        public static Font Font
        {
            get
            {
                if (_font == null) _font = Resources.Load<Font>("Fonts/Fredoka-Bold");
                if (_font == null) _font = Resources.Load<Font>("Fonts/Fredoka-SemiBold");
                if (_font == null) _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return _font;
            }
        }

        public static Sprite Warp
        {
            get
            {
                if (_warp != null) return _warp;
                const int n = 32;
                var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Repeat };
                var px = new Color[n * n];
                for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    bool thread = (x % 4) == 0;
                    px[y * n + x] = thread ? new Color(1f, 1f, 1f, 0.85f) : new Color(1f, 1f, 1f, 0.12f);
                }
                t.SetPixels(px); t.Apply();
                _warp = Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
                return _warp;
            }
        }

        public static Sprite Fill
        {
            get
            {
                if (_fill != null) return _fill;
                const int n = 32;
                var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
                var px = new Color[n * n];
                for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float weave = ((y / 2) % 2 == 0) ? 1f : 0.9f;
                    px[y * n + x] = new Color(weave, weave, weave, 1f);
                }
                t.SetPixels(px); t.Apply();
                _fill = Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
                return _fill;
            }
        }

        public static Sprite Ring
        {
            get
            {
                if (_ring != null) return _ring;
                const int n = 64;
                var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
                var px = new Color[n * n];
                float mid = (n - 1) * 0.5f;
                for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float dx = Mathf.Abs(x - mid), dy = Mathf.Abs(y - mid);
                    float ax = Mathf.Max(dx - mid + 8f, 0f), ay = Mathf.Max(dy - mid + 8f, 0f);
                    float d = Mathf.Sqrt(ax * ax + ay * ay);
                    float a = Mathf.Clamp01(1f - Mathf.Abs(d - 5.5f) / 2.2f);
                    px[y * n + x] = new Color(1f, 1f, 1f, a);
                }
                t.SetPixels(px); t.Apply();
                _ring = Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
                return _ring;
            }
        }

        public static Sprite Arrow
        {
            get
            {
                if (_arrow != null) return _arrow;
                const int n = 32;
                var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
                var px = new Color[n * n];
                for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float ny = (y + 0.5f) / n;
                    float tip = Mathf.Abs(ny - 0.5f) * 2f;
                    float a = x > n * tip && x < n * 0.92f ? 1f : 0f;
                    px[y * n + x] = new Color(1f, 1f, 1f, a);
                }
                t.SetPixels(px); t.Apply();
                _arrow = Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.15f, 0.5f), n);
                return _arrow;
            }
        }

        static Sprite _warp, _fill, _ring, _arrow;

        public static SpriteRenderer Quad(Transform parent, string name, Vector2 pos, Vector2 size, Color color, int order)
        {
            return Quad(parent, name, pos, size, color, order, null);
        }

        public static SpriteRenderer Quad(Transform parent, string name, Vector2 pos, Vector2 size, Color color, int order, Sprite sprite)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = new Vector3(size.x, size.y, 1);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite != null ? sprite : Square;
            sr.color = color;
            sr.sortingOrder = order;
            return sr;
        }

        public static TextMesh Label(Transform parent, string name, Vector2 pos, string text, float size, Color color, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(pos.x, pos.y, -0.1f);
            var tm = go.AddComponent<TextMesh>();
            tm.font = Font; tm.text = text; tm.characterSize = size; tm.fontSize = 48; tm.fontStyle = FontStyle.Bold;
            tm.anchor = TextAnchor.MiddleCenter; tm.alignment = TextAlignment.Center; tm.color = color;
            var mr = go.GetComponent<MeshRenderer>();
            mr.sharedMaterial = Font.material; mr.sortingOrder = order;
            return tm;
        }

        public static void Clear(Transform t)
        {
            // DestroyImmediate so a second Refresh in the same frame cannot leave the previous count label behind.
            for (int i = t.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(t.GetChild(i).gameObject);
        }

        /// <summary>Prototype spool: thread barrel, wood caps, motif, and a count badge.</summary>
        public static void SpoolIcon(Transform parent, string name, Vector2 pos, float height, char color, string count, bool hidden, int order, float brightness)
        {
            float b = Mathf.Clamp01(brightness);
            var thread = hidden ? Hex("#7A6A60") : Color.Lerp(Hex("#2A1C16"), Of(color), b);
            var wood = Color.Lerp(Hex("#8A5A32"), Hex("#C99256"), b);
            float w = height * 0.78f;
            Quad(parent, name + "Yarn", pos, new Vector2(w * 0.72f, height * 0.78f), thread, order, ArtKit.UiRound);
            Quad(parent, name + "Top", pos + new Vector2(0f, height * 0.4f), new Vector2(w, height * 0.16f), wood, order + 1, ArtKit.UiRound);
            Quad(parent, name + "Bot", pos + new Vector2(0f, -height * 0.4f), new Vector2(w, height * 0.16f), wood, order + 1, ArtKit.UiRound);
            if (!hidden)
                Quad(parent, name + "Mark", pos + new Vector2(0f, height * 0.06f), Vector2.one * height * 0.34f, InkFor(color), order + 2, Glyphs.Of(color));
            var badge = pos + new Vector2(w * 0.42f, -height * 0.32f);
            float br = Mathf.Clamp(height * 0.2f, 0.11f, 0.26f);
            Quad(parent, name + "Badge", badge, Vector2.one * br * 2f, Ink, order + 3, Glyphs.Circle);
            Label(parent, name + "Num", badge, count, br * 0.42f, Color.white, order + 4);
        }

        public static void Bobbin(Transform parent, string name, Vector2 pos, float height, Color thread, int order, float brightness)
        {
            SpoolIcon(parent, name, pos, height, 'R', "", true, order, brightness);
        }
    }
}
