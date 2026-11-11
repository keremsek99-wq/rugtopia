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
        SpriteRenderer _ring;
        SpriteRenderer _arrow;
        Transform _strip;
        float _cs; Vector2 _origin;
        int _visualHp = -1;
        int _popRow = -1;
        float _popUntil;

        public void Build(GameState g)
        {
            ViewKit.Clear(transform);
            int rows = g.Grid.Rows, cols = g.Grid.Cols;
            _cs = Mathf.Min(area.width / cols, area.height / rows);
            _origin = new Vector2(area.center.x - _cs * cols / 2f + _cs / 2f, area.center.y + _cs * rows / 2f - _cs / 2f);
            var rug = new Vector2(_cs * cols, _cs * rows);
            ViewKit.Quad(transform, "Border", area.center, rug + new Vector2(0.55f, 0.7f), ViewKit.Hex("#5C3A28"), 0);
            ViewKit.Quad(transform, "Backing", area.center, rug + new Vector2(0.16f, 0.16f), ViewKit.Hex("#C4A27A"), 0);
            float fringeY = area.center.y - rug.y * 0.5f - 0.22f;
            for (int c = 0; c < cols; c++)
                ViewKit.Quad(transform, "Fringe" + c, new Vector2(CellPos(rows - 1, c).x, fringeY), new Vector2(_cs * 0.18f, 0.38f), ViewKit.Hex("#E7D3AE"), 1);
            _cells = new SpriteRenderer[rows, cols];
            var cell = Vector2.one * _cs * 1.02f;
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    _cells[r, c] = ViewKit.Quad(transform, "Cell" + r + "_" + c, CellPos(r, c), cell, ViewKit.Unwoven, 2, ViewKit.Warp);
            _ring = ViewKit.Quad(transform, "TipRing", Vector2.zero, Vector2.one * _cs * 1.35f, new Color(1f, 0.92f, 0.55f), 4, ViewKit.Ring);
            _tip = ViewKit.Quad(transform, "ShuttleTip", Vector2.zero, Vector2.one * _cs * 0.72f, Color.white, 5, ViewKit.Fill);
            _arrow = ViewKit.Quad(transform, "ShuttleArrow", Vector2.zero, new Vector2(_cs * 0.55f, _cs * 0.4f), new Color(1f, 0.95f, 0.7f), 6, ViewKit.Arrow);
            _strip = new GameObject("NextStrip").transform; _strip.SetParent(transform, false);
            Refresh(g);
        }

        public Vector2 CellPos(int r, int c) { return _origin + new Vector2(c * _cs, -r * _cs); }

        public void Refresh(GameState g)
        {
            if (_cells == null) return;
            int rows = g.Grid.Rows, cols = g.Grid.Cols;
            int hp = _visualHp >= 0 ? Mathf.Clamp(_visualHp, 0, g.Grid.Length) : g.Hp;
            int active = g.Grid.RowOf(hp);
            var woven = new bool[rows, cols];
            for (int i = 0; i < hp; i++) woven[g.Grid.PathRow[i], g.Grid.PathCol[i]] = true;
            for (int r = 0; r < rows; r++)
            {
                bool visible = hp >= g.Grid.Length || !g.Grid.CoveredRow[r] || (active >= 0 && r >= active);
                for (int c = 0; c < cols; c++)
                {
                    char col = g.Grid.Color[r, c];
                    var cellSr = _cells[r, c];
                    if (woven[r, c])
                    {
                        cellSr.sprite = ViewKit.Fill;
                        cellSr.color = ViewKit.Of(col);
                    }
                    else if (!visible)
                    {
                        cellSr.sprite = ViewKit.Warp;
                        cellSr.color = ViewKit.Veil;
                    }
                    else
                    {
                        cellSr.sprite = ViewKit.Warp;
                        cellSr.color = Color.Lerp(ViewKit.Unwoven, ViewKit.Of(col), 0.55f);
                    }
                }
            }
            if (hp < g.Grid.Length)
            {
                var pos = CellPos(g.Grid.PathRow[hp], g.Grid.PathCol[hp]);
                int dir = g.Grid.DirectionOfRow(g.Grid.PathRow[hp]);
                var tc = ViewKit.Of(g.Grid.PathColor[hp]);
                _tip.enabled = true;
                _ring.enabled = true;
                _arrow.enabled = true;
                _tip.transform.localPosition = pos;
                _ring.transform.localPosition = pos;
                _arrow.transform.localPosition = pos + new Vector2(dir * _cs * 0.05f, _cs * 0.62f);
                _arrow.transform.localScale = new Vector3(dir * _cs * 0.55f, _cs * 0.38f, 1f);
                _tip.color = Color.Lerp(tc, Color.white, 0.35f);
                _ring.color = new Color(1f, 0.92f, 0.45f, 1f);
                _ring.transform.localScale = Vector3.one * _cs * 1.42f;
            }
            else
            {
                _tip.enabled = false;
                _ring.enabled = false;
                _arrow.enabled = false;
            }
            DrawStrip(g);
        }

        void DrawStrip(GameState g)
        {
            ViewKit.Clear(_strip);
            var runs = g.PreviewRuns();
            ViewKit.Label(_strip, "Next", new Vector2(area.center.x, stripY + 0.48f), "NEXT", 0.08f, ViewKit.Ink, 5);
            float x = area.xMin + 0.15f, h = 0.42f;
            foreach (var run in runs)
            {
                if (x > area.xMax - 0.25f) break;
                if (run.Key == '|') { ViewKit.Quad(_strip, "Sep", new Vector2(x + 0.06f, stripY), new Vector2(0.05f, h), ViewKit.Ink, 4); x += 0.24f; continue; }
                if (run.Key == '?') { ViewKit.Label(_strip, "Veiled", new Vector2(x + 0.25f, stripY), "?", 0.1f, ViewKit.Ink, 5); break; }
                float w = Mathf.Clamp(0.38f * run.Value, 0.42f, 1.8f);
                ViewKit.Quad(_strip, "Chip", new Vector2(x + w / 2f, stripY), new Vector2(w, h), ViewKit.Of(run.Key), 4, ViewKit.Fill);
                ViewKit.Label(_strip, "N", new Vector2(x + w / 2f, stripY), run.Value.ToString(), 0.06f, ViewKit.InkFor(run.Key), 6);
                x += w + 0.16f;
            }
        }

        /// <summary>While a take animates, draw the rug as of this shuttle index. Pass -1 to follow the core.</summary>
        public void SetVisualHp(int hp) { _visualHp = hp; }

        public void PopRow(int row) { _popRow = row; _popUntil = Time.time + 0.32f; }

        void Update()
        {
            if (_tip != null && _tip.enabled && _ring != null)
            {
                float p = 0.5f + 0.5f * Mathf.Sin(Time.time * 6f);
                var glow = _ring.color;
                glow.a = 0.45f + 0.55f * p;
                _ring.color = glow;
                _ring.transform.localScale = Vector3.one * _cs * (1.28f + 0.18f * p);
            }
            if (_cells == null || Time.time > _popUntil || _popRow < 0) return;
            float s = 1f + 0.22f * Mathf.Sin((1f - (_popUntil - Time.time) / 0.32f) * Mathf.PI);
            int cols = _cells.GetLength(1);
            for (int c = 0; c < cols; c++)
                if (_cells[_popRow, c] != null) _cells[_popRow, c].transform.localScale = Vector3.one * _cs * 0.94f * s;
        }
    }
}
