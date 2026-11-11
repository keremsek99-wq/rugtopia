using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>One full-height gradient, the same brown the HTML prototype paints behind everything.</summary>
    public class Backdrop : MonoBehaviour
    {
        static Sprite _gradient;

        public void Build(Vector2 rugCenter, Vector2 rugSize)
        {
            ViewKit.Clear(transform);
            ViewKit.Quad(transform, "Background", new Vector2(0f, 0f), new Vector2(16f, 28f), Color.white, -30, Gradient());
        }

        static Sprite Gradient()
        {
            if (_gradient != null) return _gradient;
            const int h = 256;
            var t = new Texture2D(4, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var top = ViewKit.Hex("#4A2C20");
            var bot = ViewKit.Hex("#22140F");
            var px = new Color[4 * h];
            for (int y = 0; y < h; y++)
            {
                float k = y / (float)(h - 1);
                var c = Color.Lerp(bot, top, k);
                for (int x = 0; x < 4; x++) px[y * 4 + x] = c;
            }
            t.SetPixels(px); t.Apply();
            _gradient = Sprite.Create(t, new Rect(0, 0, 4, h), new Vector2(0.5f, 0.5f), h);
            return _gradient;
        }
    }
}
