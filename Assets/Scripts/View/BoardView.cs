using System.Collections;
using Rugtopia.Core;
using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Draws the rug: woven stitches in colour, unwoven ones faded, the shuttle tip highlighted, covered rows veiled,
    /// plus the NEXT preview strip under the rug. Snaps to state (no animation yet — week 2).</summary>
    public class BoardView : MonoBehaviour
    {
        public Rect area = new Rect(-4.45f, -2.2f, 8.9f, 9.05f);
        public float stripY = -2.82f;

        SpriteRenderer[,] _cells;
        SpriteRenderer[,] _marks;
        SpriteRenderer _tip;
        SpriteRenderer _dot;
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
            // Camera is ±10 tall. Header owns the top. NEXT stays just above the belt.
            // A short rug is centred in that band; a tall one fills it.
            const float safeTop = 6.85f;
            const float floor = -2.2f;
            const float availW = 8.9f;
            float availH = safeTop - floor;
            _cs = Mathf.Min(availW / cols, availH / rows);
            float rugW = _cs * cols;
            float rugH = _cs * rows;
            float slack = Mathf.Max(0f, availH - rugH);
            float bottom = floor + slack * 0.5f;
            float topCells = bottom + rugH;
            area = new Rect(-rugW * 0.5f, bottom, rugW, rugH);
            _origin = new Vector2(area.xMin + _cs * 0.5f, topCells - _cs * 0.5f);
            var rug = new Vector2(rugW, rugH);
            var origin = new Vector2(0f, (topCells + bottom) * 0.5f);
            stripY = floor - 0.62f;
            var shadow = ThemeConfig.IsLight ? new Color(0.28f, 0.14f, 0.06f, 0.32f) : new Color(0f, 0f, 0f, 0.28f);
            ViewKit.Quad(transform, "RugShadow", origin + new Vector2(0.06f, -0.2f), rug + new Vector2(0.85f, 1.05f), shadow, -2, ArtKit.Shadow);
            ViewKit.Quad(transform, "Frame", origin, rug + new Vector2(0.36f, 0.42f), ThemeConfig.RugFrame, 0, ArtKit.UiRound);
            float left = _origin.x - _cs * 0.5f, right = _origin.x + (cols - 0.5f) * _cs;
            float top = origin.y + rug.y * 0.5f + 0.18f;
            float bot = origin.y - rug.y * 0.5f - 0.18f;
            for (float x = left; x <= right + 0.01f; x += 0.22f)
            {
                ViewKit.Quad(transform, "FringeT", new Vector2(x, top), new Vector2(0.045f, 0.28f), ViewKit.Hex("#D8C3A0"), 1);
                ViewKit.Quad(transform, "FringeB", new Vector2(x, bot), new Vector2(0.045f, 0.28f), ViewKit.Hex("#D8C3A0"), 1);
            }
            ViewKit.Quad(transform, "Backing", origin, rug + new Vector2(0.1f, 0.1f), ViewKit.Hex("#EFE2C8"), 1);
            ViewKit.Quad(transform, "WarpBed", origin, rug, ViewKit.Hex("#6E5A4C"), 1);
            _cells = new SpriteRenderer[rows, cols];
            _marks = new SpriteRenderer[rows, cols];
            var cell = Vector2.one * _cs * 0.88f;
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    var p = CellPos(r, c);
                    _cells[r, c] = ViewKit.Quad(transform, "Cell" + r + "_" + c, p, cell, ViewKit.Unwoven, 2);
                    _marks[r, c] = ViewKit.Quad(transform, "Mark" + r + "_" + c, p, cell * 0.42f, Color.white, 3, Glyphs.Diamond);
                }
            _ring = ViewKit.Quad(transform, "TipRing", Vector2.zero, Vector2.one * _cs, ViewKit.Gold, 4, Glyphs.Box);
            _tip = ViewKit.Quad(transform, "Shuttle", Vector2.zero, new Vector2(_cs * 0.55f, _cs * 0.18f), ViewKit.Hex("#C9A36B"), 5, ArtKit.UiRound);
            _dot = ViewKit.Quad(transform, "Eye", Vector2.zero, Vector2.one * _cs * 0.12f, Color.white, 6, Glyphs.Circle);
            _arrow = ViewKit.Quad(transform, "RowArrow", Vector2.zero, new Vector2(0.42f, 0.32f), ViewKit.Gold, 6, ViewKit.Arrow);
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
                    var mark = _marks[r, c];
                    var tint = ViewKit.Of(col);
                    if (woven[r, c])
                    {
                        cellSr.color = tint;
                        mark.enabled = true;
                        mark.sprite = Glyphs.Chevron;
                        var dark = tint * 0.62f; dark.a = 0.9f;
                        mark.color = dark;
                        mark.transform.localScale = Vector3.one * _cs * 0.7f;
                    }
                    else if (!visible)
                    {
                        cellSr.color = ViewKit.Hex("#5A4A40");
                        mark.enabled = false;
                    }
                    else
                    {
                        tint.a = 0.4f;
                        cellSr.color = tint;
                        mark.enabled = true;
                        mark.sprite = Glyphs.Of(col);
                        mark.color = new Color(1f, 1f, 1f, 0.85f);
                        mark.transform.localScale = Vector3.one * _cs * 0.36f;
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
                _ring.transform.localPosition = pos;
                _ring.transform.localScale = Vector3.one * _cs * 0.98f;
                _ring.color = ViewKit.Gold;
                var boat = pos + new Vector2(0f, _cs * 0.62f);
                _tip.transform.localPosition = boat;
                _tip.transform.localScale = new Vector3(_cs * 0.5f, _cs * 0.16f, 1f);
                _tip.color = ViewKit.Hex("#C9A36B");
                _dot.enabled = true;
                _dot.transform.localPosition = boat;
                _dot.transform.localScale = Vector3.one * _cs * 0.12f;
                _dot.color = tc;
                float edge = dir > 0 ? (_origin.x - _cs * 0.5f - 0.38f) : (_origin.x + (cols - 0.5f) * _cs + 0.38f);
                _arrow.transform.localPosition = new Vector3(edge, pos.y, -0.2f);
                _arrow.transform.localScale = new Vector3(dir * 0.46f, 0.36f, 1f);
                _arrow.color = ViewKit.Gold;
            }
            else
            {
                _tip.enabled = false;
                _dot.enabled = false;
                _ring.enabled = false;
                _arrow.enabled = false;
            }
            DrawStrip(g);
        }

        void DrawStrip(GameState g)
        {
            ViewKit.Clear(_strip);
            var runs = g.PreviewRuns();
            var bar = ThemeConfig.IsLight ? new Color(0.28f, 0.16f, 0.1f, 0.55f) : new Color(0f, 0f, 0f, 0.35f);
            ViewKit.Quad(_strip, "Bar", new Vector2(0f, stripY), new Vector2(8.9f, 0.72f), bar, 3, ArtKit.UiRound);
            ViewKit.Label(_strip, "Next", new Vector2(-3.9f, stripY), "NEXT", 0.07f, ThemeConfig.IsLight ? ThemeConfig.Ink : new Color(0.96f, 0.91f, 0.81f, 0.85f), 5);
            float x = area.xMin + 1.25f;
            const float h = 0.48f;
            foreach (var run in runs)
            {
                if (x > area.xMax - 0.3f) break;
                if (run.Key == '|')
                {
                    ViewKit.Quad(_strip, "Flip", new Vector2(x + 0.16f, stripY), new Vector2(0.28f, 0.22f), ViewKit.Gold, 5, ViewKit.Arrow);
                    x += 0.42f;
                    continue;
                }
                if (run.Key == '?') { ViewKit.Label(_strip, "Veiled", new Vector2(x + 0.3f, stripY), "?", 0.16f, ViewKit.Cream, 5); break; }
                const float w = 1.15f;
                var mid = new Vector2(x + w * 0.5f, stripY);
                ViewKit.Quad(_strip, "Chip", mid, new Vector2(w, h), ViewKit.Of(run.Key), 4, ArtKit.UiRound);
                ViewKit.Quad(_strip, "G", mid + new Vector2(-0.28f, 0f), Vector2.one * 0.26f, ViewKit.InkFor(run.Key), 5, Glyphs.Of(run.Key));
                ViewKit.Label(_strip, "N", mid + new Vector2(0.24f, 0f), "×" + run.Value, 0.08f, ViewKit.InkFor(run.Key), 6);
                x += w + 0.12f;
            }
        }

        /// <summary>While a take animates, draw the rug as of this shuttle index. Pass -1 to follow the core.</summary>
        public void SetVisualHp(int hp) { _visualHp = hp; }

        public void PopRow(int row) { _popRow = row; _popUntil = Time.time + 0.32f; }

        public void Shine() { StartCoroutine(ShineCo()); }

        IEnumerator ShineCo()
        {
            var sweep = ViewKit.Quad(transform, "Shine", new Vector2(area.xMin, area.center.y), new Vector2(0.35f, area.height * 0.92f), new Color(1f, 0.97f, 0.85f, 0.55f), 8);
            float t = 0f;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                float k = 1f - (1f - Mathf.Clamp01(t / 0.5f));
                k = 1f - (1f - k) * (1f - k);
                sweep.transform.localPosition = new Vector3(Mathf.Lerp(area.xMin, area.xMax, k), area.center.y, -0.2f);
                yield return null;
            }
            if (sweep != null) Destroy(sweep.gameObject);
        }

        void Update()
        {
            if (_ring != null && _ring.enabled)
            {
                float p = 0.65f + 0.35f * Mathf.Sin(Time.time * 6f);
                var glow = ViewKit.Gold;
                glow.a = p;
                _ring.color = glow;
            }
            if (_cells == null || Time.time > _popUntil || _popRow < 0) return;
            float s = 1f + 0.22f * Mathf.Sin((1f - (_popUntil - Time.time) / 0.32f) * Mathf.PI);
            int cols = _cells.GetLength(1);
            for (int c = 0; c < cols; c++)
                if (_cells[_popRow, c] != null) _cells[_popRow, c].transform.localScale = Vector3.one * _cs * 0.94f * s;
        }
    }
}
