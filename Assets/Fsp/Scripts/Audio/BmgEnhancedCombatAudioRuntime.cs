using System.Collections.Generic;
using Fsp.Combat;
using Fsp.BattleRoyale;
using UnityEngine;

namespace Fsp.Audio
{
    /// <summary>
    /// Adds an original synthesized transient/body layer per weapon class on top of the core BMG audio runtime.
    /// No third-party recordings are used.
    /// </summary>
    public sealed class BmgEnhancedCombatAudioRuntime : MonoBehaviour
    {
        private static BmgEnhancedCombatAudioRuntime instance;
        private readonly HashSet<HitscanWeapon> bound = new();
        private AudioSource localSource;
        private AudioClip assault, smg, marksman, shotgun;
        private float nextBind;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null) return;
            var go = new GameObject("BMG_EnhancedCombatAudio");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<BmgEnhancedCombatAudioRuntime>();
        }

        private void Awake()
        {
            localSource = gameObject.AddComponent<AudioSource>();
            localSource.playOnAwake = false;
            localSource.spatialBlend = 0f;
            localSource.volume = .82f;
            assault = BuildShot("BMG_AssaultLayer", .34f, 88f, 1680f, 110351u);
            smg = BuildShot("BMG_SmgLayer", .24f, 118f, 2450f, 220733u);
            marksman = BuildShot("BMG_MarksmanLayer", .52f, 62f, 1380f, 991821u);
            shotgun = BuildShot("BMG_ShotgunLayer", .64f, 48f, 980f, 718331u);
        }

        private void Update()
        {
            if (Time.unscaledTime < nextBind) return;
            nextBind = Time.unscaledTime + .6f;
            foreach (var weapon in FindObjectsByType<HitscanWeapon>(FindObjectsSortMode.None))
            {
                if (weapon == null || !bound.Add(weapon)) continue;
                weapon.ShotFired += (origin, direction) => PlayLayer(weapon, origin);
            }
        }

        private void PlayLayer(HitscanWeapon weapon, Vector3 origin)
        {
            if (!FspAudioRuntime.SfxEnabled || weapon == null) return;
            AudioClip clip = Select(weapon.Config != null ? weapon.Config.weaponClass : WeaponClass.Assault);
            MatchParticipant owner = weapon.GetComponentInParent<MatchParticipant>();
            bool local = owner != null && owner.IsLocalPlayer;
            if (local)
            {
                localSource.pitch = Random.Range(.965f, 1.025f);
                localSource.PlayOneShot(clip, .72f);
            }
            else
            {
                AudioSource.PlayClipAtPoint(clip, origin, .34f);
            }
        }

        private AudioClip Select(WeaponClass cls) => cls switch
        {
            WeaponClass.SMG => smg,
            WeaponClass.Marksman => marksman,
            WeaponClass.Shotgun => shotgun,
            _ => assault
        };

        private static AudioClip BuildShot(string name, float seconds, float bodyHz, float crackHz, uint seed)
        {
            const int rate = 22050;
            int count = Mathf.Max(1, Mathf.RoundToInt(rate * seconds));
            float[] data = new float[count];
            float lp = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)rate;
                seed = seed * 1664525u + 1013904223u;
                float noise = ((seed >> 8) / 16777215f) * 2f - 1f;
                lp = Mathf.Lerp(lp, noise, .18f);
                float body = Mathf.Sin(2f * Mathf.PI * bodyHz * t) * Mathf.Exp(-10f * t);
                float mid = Mathf.Sin(2f * Mathf.PI * (bodyHz * 3.1f) * t) * Mathf.Exp(-19f * t);
                float crack = noise * Mathf.Exp(-58f * t);
                float metal = Mathf.Sin(2f * Mathf.PI * crackHz * t) * Mathf.Exp(-44f * t);
                float tail = lp * Mathf.Exp(-5.2f * t) * .16f;
                data[i] = Mathf.Clamp(body * .48f + mid * .23f + crack * .82f + metal * .14f + tail, -1f, 1f);
            }
            var clip = AudioClip.Create(name, count, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
