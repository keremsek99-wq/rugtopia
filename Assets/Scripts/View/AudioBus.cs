using System.Collections.Generic;
using Rugtopia.Meta;
using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>
    /// Pooled one-shots for Feel.SoundHook, plus one looping bed.
    /// Music starts on the first gesture so WebGL can unlock audio.
    /// </summary>
    public sealed class AudioBus : MonoBehaviour
    {
        const int Pool = 8;
        const float MusicGain = 0.22f;
        const float DuckGain = 0.10f;

        static AudioBus _bus;

        AudioSource _music;
        AudioSource[] _pool;
        int _cursor;
        int _run;
        float _duckUntil;
        bool _heard;
        Dictionary<string, AudioClip> _cache;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            if (_bus != null) return;
            var go = new GameObject("AudioBus");
            DontDestroyOnLoad(go);
            go.AddComponent<AudioBus>();
        }

        public static void NotifyGesture()
        {
            if (_bus != null) _bus.OnGesture();
        }

        public static void ApplyMusic()
        {
            if (_bus != null) _bus.TryMusic();
        }

        void Awake()
        {
            if (_bus != null && _bus != this)
            {
                Destroy(gameObject);
                return;
            }
            _bus = this;
            _pool = new AudioSource[Pool];
            for (int i = 0; i < Pool; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.loop = false;
                src.spatialBlend = 0f;
                src.priority = 64;
                _pool[i] = src;
            }
            _music = gameObject.AddComponent<AudioSource>();
            _music.playOnAwake = false;
            _music.loop = true;
            _music.spatialBlend = 0f;
            _music.priority = 128;
            _music.volume = MusicGain;
            Feel.GestureHook -= OnGesture;
            Feel.GestureHook += OnGesture;
            Feel.SoundHook -= OnSound;
            Feel.SoundHook += OnSound;
        }

        void OnDestroy()
        {
            if (_bus == this) _bus = null;
            Feel.GestureHook -= OnGesture;
            Feel.SoundHook -= OnSound;
        }

        void Update()
        {
            if (_heard) TryMusic();
            if (_music == null || !_music.isPlaying) return;
            float target = Time.unscaledTime < _duckUntil ? DuckGain : MusicGain;
            _music.volume = Mathf.MoveTowards(_music.volume, target, Time.unscaledDeltaTime * 0.45f);
        }

        void OnGesture()
        {
            _heard = true;
            TryMusic();
        }

        void TryMusic()
        {
            var save = SaveService.Data;
            if (save != null && save.musicMuted)
            {
                if (_music != null && _music.isPlaying) _music.Stop();
                return;
            }
            if (!_heard || _music == null) return;
            if (_music.clip == null) _music.clip = Resources.Load<AudioClip>("Music/home");
            if (_music.clip == null || _music.isPlaying) return;
            if (_music.clip.loadState == AudioDataLoadState.Unloaded) _music.clip.LoadAudioData();
            if (_music.clip.loadState != AudioDataLoadState.Loaded) return;
            _music.loop = true;
            _music.volume = MusicGain;
            _music.Play();
        }

        void OnSound(string id)
        {
            if (id == "row" || id == "pick" || id == "win" || id == "fail") _run = 0;
            var save = SaveService.Data;
            if (save != null && save.muted) return;
            var clip = Clip(id);
            if (clip == null || _pool == null) return;
            Feel.Served = true;
            float pitch = 1f;
            if (id == "stitch")
            {
                pitch = 1f + Mathf.Min(0.12f, _run * 0.03f);
                _run++;
            }
            if (id != "win" && id != "fail") pitch *= Random.Range(0.96f, 1.05f);
            var src = _pool[_cursor];
            _cursor = (_cursor + 1) % _pool.Length;
            src.pitch = Mathf.Clamp(pitch, 0.9f, 1.28f);
            src.volume = Gain(id);
            src.clip = clip;
            src.Play();
            if (id == "win") _duckUntil = Time.unscaledTime + clip.length + 0.25f;
        }

        AudioClip Clip(string id)
        {
            if (_cache == null) _cache = new Dictionary<string, AudioClip>();
            AudioClip clip;
            if (_cache.TryGetValue(id, out clip)) return clip;
            clip = Resources.Load<AudioClip>("Sfx/" + id);
            _cache[id] = clip;
            return clip;
        }

        static float Gain(string id)
        {
            switch (id)
            {
                case "stitch": return 0.26f;
                case "belt": return 0.28f;
                case "no": return 0.30f;
                case "fail": return 0.38f;
                case "pick": return 0.40f;
                case "tap": return 0.42f;
                case "drop": return 0.46f;
                case "booster": return 0.48f;
                case "coin": return 0.50f;
                case "row": return 0.55f;
                case "win": return 0.62f;
                default: return 0.45f;
            }
        }
    }
}
