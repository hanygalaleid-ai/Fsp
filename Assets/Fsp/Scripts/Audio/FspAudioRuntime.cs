using System;
using System.Collections.Generic;
using Fsp.BattleRoyale;
using Fsp.Combat;
using Fsp.Inventory;
using Fsp.Player;
using Fsp.Vehicles;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.Audio
{
    /// <summary>Original BMG sound system. Release clips were synthesized for this project.</summary>
    public sealed class FspAudioRuntime : MonoBehaviour
    {
        private static FspAudioRuntime instance;
        private readonly HashSet<HitscanWeapon> boundWeapons = new();
        private AudioSource musicSource, ambienceSource, effectsSource, planeSource, windSource, vehicleSource;
        private AudioClip lobbyTheme, matchAmbience, shotClip, reloadClip, emptyClip, clickClip, confirmClip, backClip;
        private AudioClip footstepOne, footstepTwo, jumpClip, landClip, pickupClip, healClip, damageClip, parachuteOpenClip;
        private AudioClip warningClip, victoryClip, defeatClip;
        private MatchParticipant localParticipant;
        private PlayerInventory inventory;
        private PlayerVitals vitals;
        private ThirdPersonMotor motor;
        private DropPlanePassenger passenger;
        private ParachuteController parachute;
        private StarterVehicleInput vehicleInput;
        private MatchManager matchManager;
        private SafeZoneController safeZone;
        private Vector3 previousPosition;
        private bool previousGrounded, previousAboard, previousParachuting, previousParachuteOpen, alternateFootstep, resultPlayed;
        private float nextBind, nextFootstep, nextZoneWarning;

        public static bool MusicEnabled => PlayerPrefs.GetInt("fsp_music", 1) == 1;
        public static bool SfxEnabled => PlayerPrefs.GetInt("fsp_sfx", 1) == 1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (instance == null) new GameObject("FspAudioRuntime").AddComponent<FspAudioRuntime>();
        }

        private void Awake()
        {
            if (instance != null && instance != this) { Destroy(gameObject); return; }
            instance = this;
            DontDestroyOnLoad(gameObject);
            musicSource = CreateSource("BMG_Music", true, .30f);
            ambienceSource = CreateSource("BMG_Ambience", true, .15f);
            effectsSource = CreateSource("BMG_Effects", false, .78f);
            planeSource = CreateSource("BMG_PlaneEngine", true, 0f);
            windSource = CreateSource("BMG_ParachuteWind", true, 0f);
            vehicleSource = CreateSource("BMG_VehicleEngine", true, 0f);

            lobbyTheme = Load("bmg_lobby_theme", CreateMusic);
            matchAmbience = Load("bmg_match_ambience", CreateAmbience);
            shotClip = Load("bmg_rifle_shot", CreateShot);
            reloadClip = Load("bmg_reload", () => CreateTone("BMG_ReloadFallback", .28f, 330f, .24f));
            emptyClip = Load("bmg_empty", () => CreateTone("BMG_EmptyFallback", .10f, 1450f, .18f));
            clickClip = Load("bmg_ui_click", () => CreateTone("BMG_ClickFallback", .09f, 620f, .18f));
            confirmClip = Load("bmg_ui_confirm", () => CreateTone("BMG_ConfirmFallback", .18f, 920f, .18f));
            backClip = Load("bmg_ui_back", () => CreateTone("BMG_BackFallback", .16f, 420f, .16f));
            footstepOne = Load("bmg_footstep_sand_01", () => CreateTone("BMG_Step1Fallback", .12f, 82f, .14f));
            footstepTwo = Load("bmg_footstep_sand_02", () => CreateTone("BMG_Step2Fallback", .12f, 96f, .14f));
            jumpClip = Load("bmg_jump", () => CreateTone("BMG_JumpFallback", .18f, 360f, .14f));
            landClip = Load("bmg_land", () => CreateTone("BMG_LandFallback", .24f, 70f, .24f));
            pickupClip = Load("bmg_pickup", () => CreateTone("BMG_PickupFallback", .25f, 760f, .16f));
            healClip = Load("bmg_heal", () => CreateTone("BMG_HealFallback", .45f, 520f, .15f));
            damageClip = Load("bmg_damage", () => CreateTone("BMG_DamageFallback", .32f, 115f, .22f));
            parachuteOpenClip = Load("bmg_parachute_open", () => CreateTone("BMG_ParachuteOpenFallback", .38f, 520f, .16f));
            warningClip = Load("bmg_zone_warning", () => CreateTone("BMG_WarningFallback", .55f, 740f, .20f));
            victoryClip = Load("bmg_victory", () => CreateTone("BMG_VictoryFallback", .85f, 523f, .20f));
            defeatClip = Load("bmg_defeat", () => CreateTone("BMG_DefeatFallback", .85f, 164f, .18f));
            planeSource.clip = Load("bmg_plane_engine_loop", () => CreateTone("BMG_PlaneFallback", 2f, 47f, .12f));
            windSource.clip = Load("bmg_parachute_wind_loop", CreateAmbience);
            vehicleSource.clip = Load("bmg_vehicle_engine_loop", () => CreateTone("BMG_VehicleFallback", 2f, 62f, .12f));
            SceneManager.sceneLoaded += HandleSceneLoaded;
            ConfigureScene(SceneManager.GetActiveScene());
        }

        private void OnDestroy()
        {
            if (instance != this) return;
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            UnbindPlayer();
            UnbindMatch();
            instance = null;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode) => ConfigureScene(scene);

        private void ConfigureScene(Scene scene)
        {
            resultPlayed = false;
            boundWeapons.Clear();
            UnbindPlayer();
            UnbindMatch();
            bool lobby = string.Equals(scene.name, "Lobby", StringComparison.OrdinalIgnoreCase);
            SetLoopClip(musicSource, lobbyTheme, lobby ? .30f : .10f);
            SetLoopClip(ambienceSource, matchAmbience, lobby ? .035f : .16f);
            planeSource.volume = windSource.volume = vehicleSource.volume = 0f;
            EnsureLoopPlaying(planeSource);
            EnsureLoopPlaying(windSource);
            EnsureLoopPlaying(vehicleSource);
            ApplySettings();
            BindRuntimeObjects();
        }

        private void Update()
        {
            if (Time.unscaledTime >= nextBind)
            {
                nextBind = Time.unscaledTime + .5f;
                BindRuntimeObjects();
            }
            UpdateMovementAudio();
            UpdateDropAudio();
            UpdateZoneAudio();
        }

        public static void ApplySettings()
        {
            if (instance == null) return;
            instance.musicSource.mute = !MusicEnabled;
            instance.ambienceSource.mute = !SfxEnabled;
            instance.effectsSource.mute = !SfxEnabled;
            instance.planeSource.mute = !SfxEnabled;
            instance.windSource.mute = !SfxEnabled;
            instance.vehicleSource.mute = !SfxEnabled;
            if (MusicEnabled) EnsureLoopPlaying(instance.musicSource);
            if (SfxEnabled)
            {
                EnsureLoopPlaying(instance.ambienceSource);
                EnsureLoopPlaying(instance.planeSource);
                EnsureLoopPlaying(instance.windSource);
                EnsureLoopPlaying(instance.vehicleSource);
            }
        }

        public static void PlayUiClick() => Play(instance?.clickClip, .60f);
        public static void PlayUiConfirm() => Play(instance?.confirmClip, .72f);
        public static void PlayUiBack() => Play(instance?.backClip, .62f);
        public static void PlayActionTap() => Play(instance?.clickClip, .22f);
        public static void PlayEmptyWeapon() => Play(instance?.emptyClip, .64f);

        private static void Play(AudioClip clip, float volume)
        {
            if (instance != null && SfxEnabled && clip != null)
                instance.effectsSource.PlayOneShot(clip, Mathf.Clamp01(volume));
        }

        private void BindRuntimeObjects()
        {
            BindWeapons();
            MatchParticipant found = FindLocalParticipant();
            if (found != localParticipant) BindPlayer(found);
            MatchManager manager = FindFirstObjectByType<MatchManager>();
            if (manager != matchManager) BindMatch(manager);
            if (safeZone == null) safeZone = FindFirstObjectByType<SafeZoneController>();
        }

        private void BindWeapons()
        {
            foreach (HitscanWeapon weapon in FindObjectsByType<HitscanWeapon>(FindObjectsSortMode.None))
            {
                if (weapon == null || !boundWeapons.Add(weapon)) continue;
                weapon.ShotFired += (origin, direction) => PlayWeaponShot(weapon, origin);
                weapon.ReloadStarted += () => { if (IsLocalWeapon(weapon)) Play(reloadClip, .82f); };
                weapon.DryFired += () => { if (IsLocalWeapon(weapon)) Play(emptyClip, .64f); };
            }
        }

        private void PlayWeaponShot(HitscanWeapon weapon, Vector3 origin)
        {
            if (!SfxEnabled || shotClip == null) return;
            if (IsLocalWeapon(weapon))
            {
                Play(shotClip, .94f);
                return;
            }

            AudioSource.PlayClipAtPoint(shotClip, origin, .58f);
        }

        private static bool IsLocalWeapon(HitscanWeapon weapon)
        {
            MatchParticipant owner = weapon != null ? weapon.GetComponentInParent<MatchParticipant>() : null;
            return owner != null && owner.IsLocalPlayer;
        }

        private void BindPlayer(MatchParticipant participant)
        {
            UnbindPlayer();
            localParticipant = participant;
            if (participant == null) return;
            inventory = participant.GetComponent<PlayerInventory>();
            vitals = participant.GetComponent<PlayerVitals>();
            motor = participant.GetComponent<ThirdPersonMotor>();
            passenger = participant.GetComponent<DropPlanePassenger>();
            parachute = participant.GetComponent<ParachuteController>();
            vehicleInput = participant.GetComponent<StarterVehicleInput>();
            previousPosition = participant.transform.position;
            previousGrounded = motor != null && motor.IsGrounded;
            previousAboard = passenger != null && passenger.IsAboard;
            previousParachuting = parachute != null && parachute.IsActive;
            previousParachuteOpen = parachute != null && parachute.IsOpen;
            if (inventory != null)
            {
                inventory.ItemPickedUp += HandlePickup;
                inventory.MedkitUsed += HandleHeal;
                inventory.WeaponSwitched += HandleWeaponSwitch;
            }
            if (vitals != null)
            {
                vitals.Damaged += HandleDamage;
                vitals.Died += HandleLocalDeath;
            }
        }

        private void UnbindPlayer()
        {
            if (inventory != null)
            {
                inventory.ItemPickedUp -= HandlePickup;
                inventory.MedkitUsed -= HandleHeal;
                inventory.WeaponSwitched -= HandleWeaponSwitch;
            }
            if (vitals != null)
            {
                vitals.Damaged -= HandleDamage;
                vitals.Died -= HandleLocalDeath;
            }
            localParticipant = null;
            inventory = null;
            vitals = null;
            motor = null;
            passenger = null;
            parachute = null;
            vehicleInput = null;
            safeZone = null;
        }

        private void BindMatch(MatchManager manager)
        {
            UnbindMatch();
            matchManager = manager;
            if (matchManager == null) return;
            matchManager.MatchWon += HandleMatchWon;
            matchManager.ParticipantEliminated += HandleParticipantEliminated;
            matchManager.PhaseChanged += HandleMatchPhase;
        }

        private void UnbindMatch()
        {
            if (matchManager != null)
            {
                matchManager.MatchWon -= HandleMatchWon;
                matchManager.ParticipantEliminated -= HandleParticipantEliminated;
                matchManager.PhaseChanged -= HandleMatchPhase;
            }
            matchManager = null;
        }

        private void UpdateMovementAudio()
        {
            if (localParticipant == null || motor == null || passenger == null || parachute == null) return;
            Vector3 current = localParticipant.transform.position;
            float speed = Time.unscaledDeltaTime > .001f ? Vector3.ProjectOnPlane(current - previousPosition, Vector3.up).magnitude / Time.unscaledDeltaTime : 0f;
            previousPosition = current;
            bool deploying = passenger.IsAboard || parachute.IsActive;
            bool grounded = motor.IsGrounded && !deploying;
            if (previousGrounded && !grounded && !deploying) Play(jumpClip, .62f);
            if (!previousGrounded && grounded && !previousParachuting) Play(landClip, .76f);
            if (grounded && speed > .7f && Time.unscaledTime >= nextFootstep)
            {
                nextFootstep = Time.unscaledTime + (speed > 5.5f ? .28f : .43f);
                Play(alternateFootstep ? footstepOne : footstepTwo, speed > 5.5f ? .48f : .37f);
                alternateFootstep = !alternateFootstep;
            }
            previousGrounded = grounded;
        }

        private void UpdateDropAudio()
        {
            bool aboard = passenger != null && passenger.IsAboard;
            bool falling = parachute != null && parachute.IsActive;
            bool parachuteOpen = parachute != null && parachute.IsOpen;
            bool driving = vehicleInput != null && vehicleInput.IsDriving;
            planeSource.volume = Mathf.MoveTowards(planeSource.volume, aboard ? .34f : 0f, Time.unscaledDeltaTime * .7f);
            windSource.volume = Mathf.MoveTowards(windSource.volume, falling ? .28f : 0f, Time.unscaledDeltaTime * .8f);
            vehicleSource.volume = Mathf.MoveTowards(vehicleSource.volume, driving ? .26f : 0f, Time.unscaledDeltaTime * .8f);
            if (previousAboard && !aboard && falling) Play(jumpClip, .82f);
            if (!previousParachuteOpen && parachuteOpen) Play(parachuteOpenClip, .86f);
            if (previousParachuting && !falling && motor != null && motor.IsGrounded) Play(landClip, .92f);
            previousAboard = aboard;
            previousParachuting = falling;
            previousParachuteOpen = parachuteOpen;
        }

        private void UpdateZoneAudio()
        {
            if (localParticipant == null || safeZone == null || passenger == null || parachute == null) return;
            if (passenger.IsAboard || parachute.IsActive || safeZone.IsInside(localParticipant.transform.position)) return;
            if (Time.unscaledTime < nextZoneWarning) return;
            nextZoneWarning = Time.unscaledTime + 3.2f;
            Play(warningClip, .68f);
        }

        private void HandlePickup(InventoryItem item) => Play(pickupClip, .68f);
        private void HandleHeal() => Play(healClip, .76f);
        private void HandleDamage(float amount) => Play(damageClip, Mathf.Clamp(.35f + amount / 100f, .42f, .82f));
        private void HandleWeaponSwitch() => Play(confirmClip, .32f);
        private void HandleLocalDeath() => PlayResult(false);
        private void HandleMatchWon(MatchParticipant winner) { if (winner != null && winner.IsLocalPlayer) PlayResult(true); }
        private void HandleParticipantEliminated(MatchParticipant participant, int placement) { if (participant != null && participant.IsLocalPlayer) PlayResult(false); }

        private void HandleMatchPhase(MatchManager.MatchPhase phase)
        {
            if (phase == MatchManager.MatchPhase.Finished && !resultPlayed)
                PlayResult(localParticipant != null && localParticipant.IsAlive && localParticipant.Placement == 1);
        }

        private void PlayResult(bool victory)
        {
            if (resultPlayed) return;
            resultPlayed = true;
            Play(victory ? victoryClip : defeatClip, .92f);
        }

        private static MatchParticipant FindLocalParticipant()
        {
            foreach (MatchParticipant participant in FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None))
                if (participant != null && participant.IsLocalPlayer) return participant;
            return null;
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

        private static AudioClip Load(string name, Func<AudioClip> fallback)
        {
            AudioClip clip = Resources.Load<AudioClip>("Audio/" + name);
            if (clip != null) return clip;
            Debug.LogError("BMG audio asset missing; using synthesized fallback: " + name);
            return fallback?.Invoke();
        }

        private static void SetLoopClip(AudioSource source, AudioClip clip, float volume)
        {
            if (source == null) return;
            if (source.clip != clip) { source.Stop(); source.clip = clip; }
            source.volume = volume;
            EnsureLoopPlaying(source);
        }

        private static void EnsureLoopPlaying(AudioSource source)
        {
            if (source != null && source.clip != null && !source.isPlaying) source.Play();
        }

        private static AudioClip CreateMusic()
        {
            const int rate = 22050;
            int count = rate * 8;
            float[] data = new float[count];
            float[] notes = { 55f, 65.41f, 73.42f, 49f };
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)rate;
                float root = notes[Mathf.FloorToInt(t / 2f) % notes.Length];
                data[i] = Mathf.Sin(2f * Mathf.PI * root * t) * .10f;
            }
            AudioClip clip = AudioClip.Create("BMG_ThemeFallback", count, 1, rate, false);
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
                smooth = Mathf.Lerp(smooth, noise, .018f);
                data[i] = smooth * .16f;
            }
            AudioClip clip = AudioClip.Create("BMG_AmbienceFallback", count, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateShot()
        {
            const int rate = 22050;
            int count = (int)(rate * .18f);
            float[] data = new float[count];
            uint seed = 918273u;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)rate;
                seed = seed * 1103515245u + 12345u;
                float noise = ((seed >> 9) / 8388607f) * 2f - 1f;
                data[i] = Mathf.Clamp(noise * Mathf.Exp(-28f * t), -1f, 1f);
            }
            AudioClip clip = AudioClip.Create("BMG_RifleFallback", count, 1, rate, false);
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
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * Mathf.Sin(Mathf.PI * i / count) * gain;
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
