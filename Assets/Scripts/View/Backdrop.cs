using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Workshop wall, vignette and the rug's soft shadow. Sits behind the board.</summary>
    public class Backdrop : MonoBehaviour
    {
        public void Build(Vector2 rugCenter, Vector2 rugSize)
        {
            ViewKit.Clear(transform);
            ViewKit.Quad(transform, "Wall", new Vector2(0f, 1.2f), new Vector2(22f, 28f), Color.white, -30, ArtKit.Wall);
            ViewKit.Quad(transform, "Vignette", new Vector2(0f, 0.5f), new Vector2(20f, 26f), Color.white, -20, ArtKit.Vignette);
            ViewKit.Quad(transform, "RugShadow", rugCenter + new Vector2(0.12f, -0.28f), rugSize + new Vector2(1.4f, 0.9f), Color.white, -5, ArtKit.Shadow);
        }
    }
}
