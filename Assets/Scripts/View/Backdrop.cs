using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>One full-height gradient, the same brown the HTML prototype paints behind everything.</summary>
    public class Backdrop : MonoBehaviour
    {
        public void Build(Vector2 rugCenter, Vector2 rugSize)
        {
            ViewKit.Clear(transform);
            ViewKit.Quad(transform, "Background", new Vector2(0f, 0f), new Vector2(22f, 32f), Color.white, -30, Gradient());
        }

        static Sprite Gradient()
        {
            // 1×1 native sprite. Quad then scales it to the requested world size.
            // A 4px-wide texture with a tall pixels-per-unit left only a stripe down the middle.
            const int n = 64;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var top = ThemeConfig.BgTop;
            var bot = ThemeConfig.BgBot;
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
            {
                float k = y / (float)(n - 1);
                var c = Color.Lerp(bot, top, k);
                for (int x = 0; x < n; x++) px[y * n + x] = c;
            }
            t.SetPixels(px); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n, 0, SpriteMeshType.FullRect);
        }
    }
}
