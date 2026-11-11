using Rugtopia.Core;
using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Draws the rug: woven stitches in colour, unwoven ones faded, the shuttle tip highlighted, covered rows veiled,
    /// plus the NEXT preview strip under the rug. Snaps to state (no animation yet — week 2).</summary>
    public class BoardView : MonoBehaviour
    {
        public Rect area = new Rect(-4.3f, 1.2f, 8.6f, 7.4f);   // world rect for the rug
        public float stripY = 0.4f;

        SpriteRenderer[,] _cells;
        SpriteRenderer _tip;
        Transform _strip;
        float _cs; Vector2 _origin;

        public void Build(GameState g)
        {
            ViewKit.Clear(transform);
            int rows = g.Grid.Rows, cols = g.Grid.Cols;
            _cs = Mathf.Min(area.width / cols, area.height / rows);
            _origin = new Vector2(area.center.x - _cs * cols / 2f + _cs / 2f, area.center.y + _cs * rows / 2f - _cs / 2f);
            ViewKit.Quad(transform, "Frame", area.center, new Vector2(_cs * cols + 0.3f, _cs * rows + 0.3f), ViewKit.Hex("#8B5E3C"), 0);
            _cells = new SpriteRenderer[rows, cols];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    _cells[r, c] = ViewKit.Quad(transform, "Cell" + r + "_" + c, CellPos(r, c), Vector2.one * _cs * 0.94f, ViewKit.Unwoven, 1);
            _tip = ViewKit.Quad(transform, "ShuttleTip", Vector2.zero, Vector2.one * _cs * 1.08f, Color.white, 2);
            _strip = new GameObject("NextStrip").transform; _strip.SetParent(transform, false);
            Refresh(g);
        }

        public Vector2 CellPos(int r, int c) { return _origin + new Vector2(c * _cs, -r * _cs); }

        public void Refresh(GameState g)
        {
            if (_cells == null) return;
            int rows = g.Grid.Rows, cols = g.Grid.Cols;
            var woven = new bool[rows, cols];
            for (int i = 0; i < g.Hp; i++) woven[g.Grid.PathRow[i], g.Grid.PathCol[i]] = true;
            for (int r = 0; r < rows; r++)
            {
                bool visible = g.IsRowVisible(r);
                for (int c = 0; c < cols; c++)
                {
                    char col = g.Grid.Color[r, c];
                    Color k;
                    if (woven[r, c]) k = ViewKit.Of(col);
                    else if (!visible) k = ViewKit.Veil;
                    else k = Color.Lerp(ViewKit.Unwoven, ViewKit.Of(col), 0.35f);
                    _cells[r, c].color = k;
                }
            }
            if (g.Hp < g.Grid.Length)
            {
                _tip.enabled = true;
                _tip.transform.localPosition = CellPos(g.Grid.PathRow[g.Hp], g.Grid.PathCol[g.Hp]);
                var tc = ViewKit.Of(g.Grid.PathColor[g.Hp]);
                _tip.color = Color.Lerp(tc, Color.white, 0.5f + 0.5f * Mathf.Sin(Time.time * 6f) * 0.5f);
            }
            else _tip.enabled = false;
            DrawStrip(g);
        }

        void DrawStrip(GameState g)
        {
            ViewKit.Clear(_strip);
            var runs = g.PreviewRuns();
            float x = area.xMin + 0.2f, h = 0.45f;
            ViewKit.Label(_strip, "Next", new Vector2(x + 0.4f, stripY), "NEXT", 0.1f, ViewKit.Ink, 5);
            x += 1.0f;
            foreach (var run in runs)
            {
                if (x > area.xMax - 0.3f) break;
                if (run.Key == '|') { ViewKit.Quad(_strip, "Sep", new Vector2(x + 0.05f, stripY), new Vector2(0.06f, h), ViewKit.Ink, 4); x += 0.2f; continue; }
                if (run.Key == '?') { ViewKit.Label(_strip, "Veiled", new Vector2(x + 0.2f, stripY), "?", 0.12f, ViewKit.Ink, 5); break; }
                float w = Mathf.Min(0.35f * run.Value, 2.2f);
                ViewKit.Quad(_strip, "Run", new Vector2(x + w / 2f, stripY), new Vector2(w, h), ViewKit.Of(run.Key), 4);
                ViewKit.Label(_strip, "N", new Vector2(x + w / 2f, stripY), run.Value.ToString(), 0.08f, ViewKit.InkFor(run.Key), 6);
                x += w + 0.08f;
            }
        }

        void Update()
        {
            if (_tip != null && _tip.enabled) { var c = _tip.color; c.a = 0.65f + 0.35f * Mathf.Sin(Time.time * 6f); _tip.color = c; }
        }
    }
}
