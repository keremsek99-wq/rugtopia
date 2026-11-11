using System;
using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Placeholder juice: a short tone, a haptic hook, and an iOS vibration when the player build is on device.</summary>
    public static class Feel
    {
        public static event Action HapticHook;
        public static event Action<string> SoundHook;

        static AudioSource _src;
        static AudioClip _tap, _stitch, _row, _win, _fail;

        public static void Tap()
        {
            if (HapticHook != null) HapticHook();
#if UNITY_IOS && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }

        public static void Play(string id)
        {
            if (SoundHook != null) SoundHook(id);
            var save = Rugtopia.Meta.SaveService.Data;
            if (save != null && save.muted) return;
            Ensure();
            AudioClip clip = _tap;
            if (id == "stitch") clip = _stitch;
            else if (id == "row") clip = _row;
            else if (id == "win") clip = _win;
            else if (id == "fail" || id == "no") clip = _fail;
            if (_src != null && clip != null) _src.PlayOneShot(clip, id == "stitch" ? 0.35f : 0.55f);
        }

        static void Ensure()
        {
            if (_src != null) return;
            var go = new GameObject("Feel");
            UnityEngine.Object.DontDestroyOnLoad(go);
            _src = go.AddComponent<AudioSource>();
            _src.playOnAwake = false;
            _tap = Clip("tap", 520f, 0.06f);
            _stitch = Clip("stitch", 740f, 0.07f);
            _row = Clip("row", 980f, 0.12f);
            _win = Clip("win", 660f, 0.28f);
            _fail = Clip("fail", 180f, 0.22f);
        }

        static AudioClip Clip(string id, float fallbackHz, float fallbackDur)
        {
            var clip = Resources.Load<AudioClip>("Sfx/" + id);
            return clip != null ? clip : Tone(fallbackHz, fallbackDur);
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
