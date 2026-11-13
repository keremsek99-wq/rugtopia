using System;
using System.Collections.Generic;
using Rugtopia.Meta;
using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>
    /// Juice hooks. AudioBus plays the real clips from SoundHook.
    /// A short tone remains when a clip is missing.
    /// </summary>
    public static class Feel
    {
        public static event Action HapticHook;
        public static event Action GestureHook;
        public static event Action<string> SoundHook;

        /// <summary>Set by a SoundHook listener that already played a real clip.</summary>
        public static bool Served;

        static AudioSource _src;
        static readonly Dictionary<string, AudioClip> Tones = new Dictionary<string, AudioClip>();

        public static void Tap()
        {
            if (GestureHook != null) GestureHook();
            if (HapticHook != null) HapticHook();
#if UNITY_IOS && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }

        public static void Play(string id)
        {
            if (GestureHook != null) GestureHook();
            Served = false;
            if (SoundHook != null) SoundHook(id);
            if (Served)
            {
                Served = false;
                return;
            }
            var save = SaveService.Data;
            if (save != null && save.muted) return;
            Ensure();
            var clip = Clip(id);
            if (_src == null || clip == null) return;
            _src.pitch = id == "win" || id == "fail" ? 1f : UnityEngine.Random.Range(0.96f, 1.05f);
            _src.PlayOneShot(clip, Gain(id));
        }

        static float Gain(string id)
        {
            if (id == "stitch") return 0.28f;
            if (id == "win") return 0.6f;
            if (id == "no" || id == "belt" || id == "fail") return 0.34f;
            return 0.5f;
        }

        static void Ensure()
        {
            if (_src != null) return;
            var go = new GameObject("Feel");
            UnityEngine.Object.DontDestroyOnLoad(go);
            _src = go.AddComponent<AudioSource>();
            _src.playOnAwake = false;
        }

        static AudioClip Clip(string id)
        {
            var clip = Resources.Load<AudioClip>("Sfx/" + id);
            if (clip != null) return clip;
            if (Tones.TryGetValue(id, out var cached)) return cached;
            float hz = 520f, dur = 0.06f;
            if (id == "pick") { hz = 640f; dur = 0.05f; }
            else if (id == "drop") { hz = 280f; dur = 0.07f; }
            else if (id == "belt") { hz = 220f; dur = 0.08f; }
            else if (id == "stitch") { hz = 740f; dur = 0.07f; }
            else if (id == "row") { hz = 980f; dur = 0.12f; }
            else if (id == "win") { hz = 660f; dur = 0.28f; }
            else if (id == "fail" || id == "no") { hz = 180f; dur = 0.16f; }
            else if (id == "coin") { hz = 880f; dur = 0.09f; }
            else if (id == "booster") { hz = 500f; dur = 0.1f; }
            var tone = Tone(hz, dur);
            Tones[id] = tone;
            return tone;
        }

        static AudioClip Tone(float freq, float dur)
        {
            int rate = 22050;
            int n = Mathf.Max(1, (int)(rate * dur));
            var clip = AudioClip.Create("rug-" + freq, n, 1, rate, false);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)rate;
                float env = 1f - t / dur;
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * env * 0.25f;
            }
            clip.SetData(data, 0);
            return clip;
        }
    }
}
