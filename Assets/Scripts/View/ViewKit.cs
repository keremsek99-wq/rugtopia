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
        public static readonly Color Background = Hex("#F4E4CF");
        public static readonly Color Letterbox = Hex("#2A1C16");
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
                const int n = 64;
                var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
                var px = new Color[n * n];
                float r = n * 0.5f - 1.5f;
                for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float dx = Mathf.Abs(x + 0.5f - n * 0.5f);
                    float dy = Mathf.Abs(y + 0.5f - n * 0.5f);
                    float ax = Mathf.Max(dx - r + 6f, 0f), ay = Mathf.Max(dy - r + 6f, 0f);
                    float corner = Mathf.Sqrt(ax * ax + ay * ay);
                    float a = Mathf.Clamp01(6.5f - corner);
                    float thread = 0.92f + 0.08f * Mathf.Sin((y + x * 0.15f) * 1.35f);
                    float sheen = y > n * 0.62f ? 1.08f : 1f;
                    px[y * n + x] = new Color(thread * sheen, thread * sheen, thread * sheen, a);
                }
                t.SetPixels(px); t.Apply();
                _square = Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
                return _square;
            }
        }

        public static Font Font
        {
            get { if (_font == null) _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); return _font; }
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
            tm.font = Font; tm.text = text; tm.characterSize = size; tm.fontSize = 48;
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

        /// <summary>One bobbin: wooden flanges and a single thread barrel. The count label is the caller's job, once.</summary>
        public static void Bobbin(Transform parent, string name, Vector2 pos, float height, Color thread, int order, float brightness)
        {
            float b = Mathf.Clamp01(brightness);
            var wood = Color.Lerp(Hex("#4A3020"), Hex("#E7D3AE"), b);
            var yarn = Color.Lerp(Hex("#2A1C16"), thread, b);
            yarn.a = 1f;
            float w = height * 0.82f;
            Quad(parent, name + "Yarn", pos, new Vector2(w * 0.58f, height * 0.62f), yarn, order);
            Quad(parent, name + "Top", pos + new Vector2(0f, height * 0.34f), new Vector2(w, height * 0.16f), wood, order + 1);
            Quad(parent, name + "Bot", pos + new Vector2(0f, -height * 0.34f), new Vector2(w, height * 0.16f), wood, order + 1);
        }
    }
}
