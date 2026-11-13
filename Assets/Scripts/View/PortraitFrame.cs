using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Keeps the playfield at 9:19.5 inside whatever window the browser gives us.</summary>
    [RequireComponent(typeof(Camera))]
    public class PortraitFrame : MonoBehaviour
    {
        public const float Aspect = 9f / 19.5f;
        Camera _cam;

        void Awake()
        {
            _cam = GetComponent<Camera>();
            var bg = new GameObject("Letterbox");
            var c = bg.AddComponent<Camera>();
            c.depth = _cam.depth - 1;
            c.cullingMask = 0;
            c.clearFlags = CameraClearFlags.SolidColor;
            c.backgroundColor = ViewKit.Letterbox;
            c.orthographic = true;
        }

        void Update()
        {
            if (Screen.height < 1 || Screen.width < 1) return;
            float window = (float)Screen.width / Screen.height;
            if (window > Aspect)
            {
                float w = Aspect / window;
                _cam.rect = new Rect((1f - w) * 0.5f, 0f, w, 1f);
            }
            else
            {
                float h = window / Aspect;
                _cam.rect = new Rect(0f, (1f - h) * 0.5f, 1f, h);
            }
        }
    }
}
