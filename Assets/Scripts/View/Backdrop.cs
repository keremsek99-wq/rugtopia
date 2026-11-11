using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Workshop wall, wooden floor, vignette and the rug's soft shadow. Covers the whole camera.</summary>
    public class Backdrop : MonoBehaviour
    {
        public void Build(Vector2 rugCenter, Vector2 rugSize)
        {
            ViewKit.Clear(transform);
            Stamp("Wall", Vector2.zero, new Vector2(16f, 26f), new Vector2(4f, 4f), ArtKit.Wall, -30);
            Stamp("Floor", new Vector2(0f, -5.6f), new Vector2(16f, 11f), new Vector2(4f, 2f), ArtKit.Floor, -25);
            ViewKit.Quad(transform, "Vignette", new Vector2(0f, 0.2f), new Vector2(11.2f, 22.4f), Color.white, -22, ArtKit.Vignette);
            ViewKit.Quad(transform, "RugShadow", rugCenter + new Vector2(0.08f, -0.35f), rugSize + new Vector2(1.6f, 0.85f), Color.white, -5, ArtKit.Shadow);
        }

        void Stamp(string name, Vector2 origin, Vector2 area, Vector2 tile, Sprite sprite, int order)
        {
            int nx = Mathf.Max(1, Mathf.CeilToInt(area.x / tile.x));
            int ny = Mathf.Max(1, Mathf.CeilToInt(area.y / tile.y));
            var start = origin - area * 0.5f + tile * 0.5f;
            for (int y = 0; y < ny; y++)
            for (int x = 0; x < nx; x++)
                ViewKit.Quad(transform, name + x + "_" + y, start + new Vector2(x * tile.x, y * tile.y), tile, Color.white, order, sprite);
        }
    }
}
