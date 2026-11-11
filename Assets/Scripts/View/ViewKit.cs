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
        public static readonly Color Background = Hex("#F3E6D3");
        public static readonly Color Unwoven = Hex("#E2D3BD");
        public static readonly Color Veil = Hex("#B9A88F");
        public static readonly Color Ink = Hex("#2B1A14");

        public static Color Hex(string h) { Color c; ColorUtility.TryParseHtmlString(h, out c); return c; }
        public static Color Of(char c) { Color v; return Palette.TryGetValue(c, out v) ? v : Color.magenta; }
        public static Color InkFor(char c) { var k = Of(c); return (0.299f * k.r + 0.587f * k.g + 0.114f * k.b) > 0.59f ? Ink : Color.white; }

        public static Sprite Square
        {
            get
            {
                if (_square != null) return _square;
                var t = new Texture2D(4, 4) { filterMode = FilterMode.Point };
                var px = new Color[16]; for (int i = 0; i < 16; i++) px[i] = Color.white;
                t.SetPixels(px); t.Apply();
                _square = Sprite.Create(t, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
                return _square;
            }
        }

        public static Font Font
        {
            get { if (_font == null) _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); return _font; }
        }

        public static SpriteRenderer Quad(Transform parent, string name, Vector2 pos, Vector2 size, Color color, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = new Vector3(size.x, size.y, 1);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Square; sr.color = color; sr.sortingOrder = order;
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

        public static void Clear(Transform t) { for (int i = t.childCount - 1; i >= 0; i--) Object.Destroy(t.GetChild(i).gameObject); }
    }
}
