using System.Collections;
using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Short-lived world sprites: a spool arc, a thread to the shuttle, and win confetti.</summary>
    public class PlayFx : MonoBehaviour
    {
        public IEnumerator Fly(Vector2 from, Vector2 to, Color color, float duration)
        {
            var sr = ViewKit.Quad(transform, "Fly", from, Vector2.one * 0.85f, color, 30);
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = EaseOut(t / duration);
                var p = Vector2.Lerp(from, to, k);
                p.y += Mathf.Sin(k * Mathf.PI) * 0.35f;
                sr.transform.localPosition = p;
                float squash = 1f + 0.18f * Mathf.Sin(k * Mathf.PI);
                sr.transform.localScale = new Vector3(0.85f / squash, 0.85f * squash, 1f);
                yield return null;
            }
            if (sr != null) Destroy(sr.gameObject);
        }

        public void Thread(Vector2 from, Vector2 to, Color color)
        {
            StartCoroutine(ThreadCo(from, to, color));
        }

        IEnumerator ThreadCo(Vector2 from, Vector2 to, Color color)
        {
            var sr = ViewKit.Quad(transform, "Thread", from, new Vector2(0.12f, 0.12f), color, 28);
            float t = 0f;
            const float dur = 0.11f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = EaseOut(t / dur);
                sr.transform.localPosition = Vector2.Lerp(from, to, k);
                sr.transform.localScale = new Vector3(Mathf.Lerp(0.18f, 0.55f, Mathf.Sin(k * Mathf.PI)), 0.14f, 1f);
                yield return null;
            }
            if (sr != null) Destroy(sr.gameObject);
        }

        static float EaseOut(float t)
        {
            t = Mathf.Clamp01(t);
            float k = 1f - t;
            return 1f - k * k * k;
        }

        public void Confetti()
        {
            StartCoroutine(ConfettiCo());
        }

        /// <summary>Places confetti immediately so a single captured frame shows it.</summary>
        public void StampConfetti()
        {
            for (int i = 0; i < 36; i++)
            {
                var c = Color.HSVToRGB((i * 0.17f) % 1f, 0.65f, 1f);
                float x = Random.Range(-3.6f, 3.6f);
                float y = Random.Range(-1.5f, 6.5f);
                ViewKit.Quad(transform, "Bit", new Vector2(x, y), new Vector2(Random.Range(0.16f, 0.36f), Random.Range(0.08f, 0.18f)), c, 45);
            }
        }

        IEnumerator ConfettiCo()
        {
            const int n = 28;
            var bits = new SpriteRenderer[n];
            var vel = new Vector2[n];
            var pos = new Vector2[n];
            for (int i = 0; i < n; i++)
            {
                var c = Color.HSVToRGB((i * 0.17f) % 1f, 0.55f, 0.95f);
                bits[i] = ViewKit.Quad(transform, "Bit", Vector2.zero, Vector2.one * 0.22f, c, 40);
                vel[i] = new Vector2(Random.Range(-3.2f, 3.2f), Random.Range(2.5f, 6f));
                pos[i] = new Vector2(Random.Range(-1.2f, 1.2f), 1.5f);
            }
            float t = 0f;
            while (t < 1.3f)
            {
                t += Time.deltaTime;
                for (int i = 0; i < n; i++)
                {
                    vel[i].y -= 9f * Time.deltaTime;
                    pos[i] += vel[i] * Time.deltaTime;
                    if (bits[i] != null) bits[i].transform.localPosition = pos[i];
                }
                yield return null;
            }
            for (int i = 0; i < n; i++) if (bits[i] != null) Destroy(bits[i].gameObject);
        }

        public void Clear()
        {
            StopAllCoroutines();
            for (int i = transform.childCount - 1; i >= 0; i--) Destroy(transform.GetChild(i).gameObject);
        }
    }
}
