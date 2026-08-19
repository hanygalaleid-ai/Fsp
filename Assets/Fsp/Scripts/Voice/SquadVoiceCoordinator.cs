using System;
using System.Collections;
using Fsp.Backend;
using Fsp.Lobby;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.WebRTC;

namespace Fsp.Voice
{
    public sealed class SquadVoiceCoordinator : MonoBehaviour
    {
        [SerializeField, Min(0.5f)] private float syncInterval = 2f;

        private CloudflareSfuVoiceRuntime runtime;
        private CloudflareSfuSignalingClient signaling;
        private SupabaseRuntimeSettingsClient settings;
        private string squadId;
        private string sessionId;
        private bool joining;
        private bool joined;
        private bool syncing;
        private float nextSync;

        private void Awake()
        {
            runtime = GetComponent<CloudflareSfuVoiceRuntime>();
            if (runtime == null) runtime = gameObject.AddComponent<CloudflareSfuVoiceRuntime>();
            signaling = GetComponent<CloudflareSfuSignalingClient>();
            if (signaling == null) signaling = gameObject.AddComponent<CloudflareSfuSignalingClient>();
            settings = GetComponent<SupabaseRuntimeSettingsClient>();
            if (settings == null) settings = gameObject.AddComponent<SupabaseRuntimeSettingsClient>();

            runtime.StatusChanged += HandleRuntimeStatus;
            runtime.Connected += HandleConnected;
            runtime.Disconnected += HandleDisconnected;
        }

        private IEnumerator Start()
        {
            if (!IsMatchScene()) yield break;
            if (!SupabaseSession.IsSignedIn) yield break;
            if (SquadLobbyState.Instance == null || !SquadLobbyState.Instance.HasSquad) yield break;

            squadId = SquadLobbyState.Instance.SquadId;
            if (SquadVoiceState.Instance == null)
                new GameObject("SquadVoiceState").AddComponent<SquadVoiceState>();

            SquadVoiceState.Instance?.BindRuntime(runtime);

            bool settingLoaded = false;
            string settingValue = null;
            yield return settings.GetValue("voice_token_endpoint", (ok, value) =>
            {
                settingLoaded = ok;
                settingValue = value;
            });

            if (!settingLoaded || string.IsNullOrWhiteSpace(settingValue))
            {
                SquadVoiceState.Instance?.MarkRuntimeError("Voice service endpoint is not configured.");
                yield break;
            }

            signaling.Configure(settingValue);
            yield return JoinVoice();
        }

        private IEnumerator JoinVoice()
        {
            if (joining || joined || !signaling.IsConfigured || string.IsNullOrWhiteSpace(squadId)) yield break;
            joining = true;
            SquadVoiceState.Instance?.SetRuntimeStatus("Preparing squad voice...");

            bool audioReady = false;
            string error = null;
            yield return runtime.InitializeAudio(() => audioReady = true, e => error = e);
            if (!audioReady)
            {
                joining = false;
                SquadVoiceState.Instance?.MarkRuntimeError(error);
                yield break;
            }

            string offerSdp = null;
            yield return runtime.CreateLocalOffer(s => offerSdp = s, e => error = e);
            if (string.IsNullOrWhiteSpace(offerSdp))
            {
                joining = false;
                SquadVoiceState.Instance?.MarkRuntimeError(error ?? "Could not create voice offer.");
                yield break;
            }

            CloudflareSfuSignalingClient.SignalResponse joinedResponse = null;
            yield return signaling.Join(squadId, offerSdp, r => joinedResponse = r, e => error = e);
            if (joinedResponse == null || string.IsNullOrWhiteSpace(joinedResponse.sessionId) || string.IsNullOrWhiteSpace(joinedResponse.sdp))
            {
                joining = false;
                SquadVoiceState.Instance?.MarkRuntimeError(error ?? "Voice join failed.");
                yield break;
            }

            bool answerApplied = false;
            yield return runtime.ApplyRemoteAnswer(joinedResponse.sdp, () => answerApplied = true, e => error = e);
            if (!answerApplied)
            {
                joining = false;
                SquadVoiceState.Instance?.MarkRuntimeError(error ?? "Voice answer failed.");
                yield break;
            }

            sessionId = joinedResponse.sessionId;
            joined = true;
            joining = false;
            nextSync = 0f;
            runtime.SetMuted(true);
            SquadVoiceState.Instance?.MarkRuntimeConnected();
        }

        private void Update()
        {
            if (!joined || syncing || Time.unscaledTime < nextSync) return;
            nextSync = Time.unscaledTime + Mathf.Max(0.5f, syncInterval);
            StartCoroutine(SyncRemoteTracks());
        }

        private IEnumerator SyncRemoteTracks()
        {
            if (!joined || syncing || string.IsNullOrWhiteSpace(sessionId)) yield break;
            syncing = true;

            CloudflareSfuSignalingClient.SignalResponse sync = null;
            string error = null;
            yield return signaling.Sync(squadId, sessionId, r => sync = r, e => error = e);
            if (sync == null)
            {
                syncing = false;
                if (!string.IsNullOrWhiteSpace(error)) SquadVoiceState.Instance?.SetRuntimeStatus("Voice sync retrying");
                yield break;
            }
            if (!sync.changed || string.IsNullOrWhiteSpace(sync.sdp))
            {
                syncing = false;
                yield break;
            }

            string answerSdp = null;
            yield return runtime.ApplyRemoteOfferAndCreateAnswer(sync.sdp, s => answerSdp = s, e => error = e);
            if (string.IsNullOrWhiteSpace(answerSdp))
            {
                syncing = false;
                SquadVoiceState.Instance?.MarkRuntimeError(error ?? "Voice renegotiation failed.");
                yield break;
            }

            bool renegotiated = false;
            yield return signaling.Renegotiate(squadId, sessionId, answerSdp, _ => renegotiated = true, e => error = e);
            syncing = false;
            if (!renegotiated && !string.IsNullOrWhiteSpace(error))
                SquadVoiceState.Instance?.SetRuntimeStatus("Voice renegotiation retrying");
        }

        private void HandleConnected() => SquadVoiceState.Instance?.MarkRuntimeConnected();
        private void HandleDisconnected()
        {
            if (joined) SquadVoiceState.Instance?.SetRuntimeStatus("Voice disconnected");
        }
        private void HandleRuntimeStatus(string status) => SquadVoiceState.Instance?.SetRuntimeStatus(status);

        private static bool IsMatchScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            return scene.IsValid() && string.Equals(scene.name, "Match", StringComparison.OrdinalIgnoreCase);
        }

        private void OnDestroy()
        {
            if (runtime != null)
            {
                runtime.StatusChanged -= HandleRuntimeStatus;
                runtime.Connected -= HandleConnected;
                runtime.Disconnected -= HandleDisconnected;
            }
            if (joined && signaling != null && !string.IsNullOrWhiteSpace(squadId))
                StartCoroutine(signaling.Leave(squadId, sessionId));
            runtime?.Shutdown();
            SquadVoiceState.Instance?.UnbindRuntime(runtime);
        }
    }

    public static class SquadVoiceRuntimeInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !string.Equals(scene.name, "Match", StringComparison.OrdinalIgnoreCase)) return;
            if (!SupabaseSession.IsSignedIn) return;
            if (SquadLobbyState.Instance == null || !SquadLobbyState.Instance.HasSquad) return;
            if (UnityEngine.Object.FindFirstObjectByType<SquadVoiceCoordinator>() != null) return;
            new GameObject("SquadVoiceRuntime").AddComponent<SquadVoiceCoordinator>();
        }
    }
}
