using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Plain dark gradient, the same family as the HTML prototype background.</summary>
    public class Backdrop : MonoBehaviour
    {
        public void Build(Vector2 rugCenter, Vector2 rugSize)
        {
            ViewKit.Clear(transform);
            ViewKit.Quad(transform, "Sky", new Vector2(0f, 2f), new Vector2(14f, 24f), ViewKit.Hex("#4A2C20"), -30);
            ViewKit.Quad(transform, "Floor", new Vector2(0f, -6f), new Vector2(14f, 12f), ViewKit.Hex("#22140F"), -29);
            ViewKit.Quad(transform, "RugShadow", rugCenter + new Vector2(0f, -0.12f), rugSize + new Vector2(0.35f, 0.28f), new Color(0f, 0f, 0f, 0.28f), -5);
        }
    }
}
