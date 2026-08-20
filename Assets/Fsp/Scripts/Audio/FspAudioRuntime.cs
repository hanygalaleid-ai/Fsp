using System;
using System.Collections.Generic;
using Fsp.Combat;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.Audio
{
    /// <summary>Original procedural FSP music and effects. No third-party recordings or licenses are used.</summary>
    public sealed class FspAudioRuntime : MonoBehaviour
    {
        private static FspAudioRuntime instance;
        private readonly HashSet<HitscanWeapon> boundWeapons = new();
        private AudioSource musicSource;
        private AudioSource ambienceSource;
        private AudioSource effectsSource;
        private AudioClip shotClip;
        private AudioClip reloadClip;
        private AudioClip clickClip;
        private float nextBind;

        public static bool MusicEnabled => PlayerPrefs.GetInt("fsp_music", 1) == 1;
        public static bool SfxEnabled => PlayerPrefs.GetInt("fsp_sfx", 1) == 1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null) return;
            new GameObject("FspAudioRuntime").AddComponent<FspAudioRuntime>();
        }

        private void Awake()
        {
            if (instance != null && instance != this) { Destroy(gameObject); return; }
            instance = this;
            DontDestroyOnLoad(gameObject);
            musicSource = CreateSource("Music", true, 0.34f);
            ambienceSource = CreateSource("Ambience", true, 0.16f);
            effectsSource = CreateSource("Effects", false, 0.72f);
            musicSource.clip = CreateMusic();
            ambienceSource.clip = CreateAmbience();
            shotClip = CreateShot();
            reloadClip = CreateTone("FSP_Reload", 0.28f, 330f, 0.24f);
            clickClip = CreateTone("FSP_Click", 0.09f, 620f, 0.18f);
            SceneManager.sceneLoaded += HandleSceneLoaded;
            ApplySettings();
        }

        private void OnDestroy()
        {
            if (instance == this) SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            musicSource.volume = scene.name == "Lobby" ? 0.34f : 0.20f;
            ambienceSource.volume = scene.name == "Match" ? 0.16f : 0.05f;
            ApplySettings();
            BindWeapons();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextBind) return;
            nextBind = Time.unscaledTime + 1f;
            BindWeapons();
        }

        public static void ApplySettings()
        {
            if (instance == null) return;
            instance.musicSource.mute = !MusicEnabled;
            instance.ambienceSource.mute = !SfxEnabled;
            instance.effectsSource.mute = !SfxEnabled;
            if (MusicEnabled && !instance.musicSource.isPlaying) instance.musicSource.Play();
            if (SfxEnabled && !instance.ambienceSource.isPlaying) instance.ambienceSource.Play();
        }

        public static void PlayUiClick()
        {
            if (instance != null && SfxEnabled) instance.effectsSource.PlayOneShot(instance.clickClip, 0.55f);
        }

        private void BindWeapons()
        {
            foreach (HitscanWeapon weapon in FindObjectsByType<HitscanWeapon>(FindObjectsSortMode.None))
            {
                if (weapon == null || !boundWeapons.Add(weapon)) continue;
                weapon.ShotFired += (origin, direction) => { if (SfxEnabled) effectsSource.PlayOneShot(shotClip); };
                weapon.ReloadStarted += () => { if (SfxEnabled) effectsSource.PlayOneShot(reloadClip); };
            }
        }

        private AudioSource CreateSource(string sourceName, bool loop, float volume)
        {
            GameObject go = new(sourceName);
            go.transform.SetParent(transform, false);
            AudioSource source = go.AddComponent<AudioSource>();
            source.loop = loop;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = volume;
            return source;
        }

        private static AudioClip CreateMusic()
        {
            const int rate = 22050;
            const float seconds = 8f;
            int count = (int)(rate * seconds);
            float[] data = new float[count];
            float[] notes = { 55f, 65.41f, 73.42f, 49f };
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)rate;
                int bar = Mathf.FloorToInt(t / 2f) % notes.Length;
                float root = notes[bar];
                float pulse = Mathf.Exp(-18f * (t % 0.5f));
                float drum = Mathf.Sin(2f * Mathf.PI * 52f * t) * pulse;
                float pad = Mathf.Sin(2f * Mathf.PI * root * t) + 0.45f * Mathf.Sin(2f * Mathf.PI * root * 1.5f * t);
                data[i] = Mathf.Clamp((pad * 0.10f) + (drum * 0.14f), -0.45f, 0.45f);
            }
            AudioClip clip = AudioClip.Create("FSP_Original_CommandTheme", count, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateAmbience()
        {
            const int rate = 16000;
            int count = rate * 5;
            float[] data = new float[count];
            uint seed = 178923u;
            float smooth = 0f;
            for (int i = 0; i < count; i++)
            {
                seed = seed * 1664525u + 1013904223u;
                float noise = ((seed >> 8) / 16777215f) * 2f - 1f;
                smooth = Mathf.Lerp(smooth, noise, 0.018f);
                data[i] = smooth * 0.16f;
            }
            AudioClip clip = AudioClip.Create("FSP_Original_DesertWind", count, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateShot()
        {
            const int rate = 22050;
            int count = (int)(rate * 0.18f);
            float[] data = new float[count];
            uint seed = 918273u;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)rate;
                seed = seed * 1103515245u + 12345u;
                float noise = ((seed >> 9) / 8388607f) * 2f - 1f;
                float envelope = Mathf.Exp(-28f * t);
                data[i] = Mathf.Clamp((noise * 0.7f + Mathf.Sin(2f * Mathf.PI * 95f * t) * 0.3f) * envelope, -1f, 1f);
            }
            AudioClip clip = AudioClip.Create("FSP_Original_Rifle", count, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateTone(string clipName, float seconds, float frequency, float gain)
        {
            const int rate = 16000;
            int count = Mathf.Max(1, (int)(rate * seconds));
            float[] data = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)rate;
                float envelope = Mathf.Sin(Mathf.PI * i / count);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * gain;
            }
            AudioClip clip = AudioClip.Create(clipName, count, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }

    [RequireComponent(typeof(Button))]
    public sealed class FspUiClickAudio : MonoBehaviour
    {
        private void Awake() => GetComponent<Button>().onClick.AddListener(FspAudioRuntime.PlayUiClick);
    }
}
