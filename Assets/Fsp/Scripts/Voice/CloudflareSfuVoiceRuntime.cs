using System;
using System.Collections;
using Unity.WebRTC;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

namespace Fsp.Voice
{
    public sealed class CloudflareSfuVoiceRuntime : MonoBehaviour
    {
        [SerializeField] private AudioSource microphoneSource;
        [SerializeField] private AudioSource remoteAudioSource;
        [SerializeField] private int sampleRate = 48000;

        private RTCPeerConnection peer;
        private MediaStream sendStream;
        private AudioStreamTrack microphoneTrack;
        private AudioClip microphoneClip;
        private Coroutine webRtcUpdate;
        private string microphoneDevice;
        private bool initialized;
        private bool muted = true;

        public bool IsInitialized => initialized && peer != null;
        public bool IsMuted => muted;
        public RTCPeerConnection Peer => peer;

        public event Action<string> StatusChanged;
        public event Action Connected;
        public event Action Disconnected;

        private void Awake() => EnsureAudioSources();

        public IEnumerator InitializeAudio(Action success, Action<string> failed)
        {
            if (IsInitialized)
            {
                success?.Invoke();
                yield break;
            }

            yield return EnsureMicrophonePermission();
            if (!HasMicrophonePermission())
            {
                failed?.Invoke("Microphone permission denied.");
                yield break;
            }

            EnsureAudioSources();
            if (microphoneSource == null || remoteAudioSource == null)
            {
                failed?.Invoke("Voice AudioSource setup failed.");
                yield break;
            }

            microphoneDevice = Microphone.devices != null && Microphone.devices.Length > 0 ? Microphone.devices[0] : null;
            microphoneClip = Microphone.Start(microphoneDevice, true, 1, Mathf.Max(16000, sampleRate));
            if (microphoneClip == null)
            {
                failed?.Invoke("Could not start microphone.");
                yield break;
            }

            float deadline = Time.realtimeSinceStartup + 3f;
            while (Microphone.GetPosition(microphoneDevice) <= 0 && Time.realtimeSinceStartup < deadline) yield return null;
            if (Microphone.GetPosition(microphoneDevice) <= 0)
            {
                Microphone.End(microphoneDevice);
                failed?.Invoke("Microphone did not begin capturing audio.");
                yield break;
            }

            microphoneSource.loop = true;
            microphoneSource.clip = microphoneClip;
            microphoneSource.playOnAwake = false;
            microphoneSource.volume = 0f;
            microphoneSource.Play();

            webRtcUpdate = StartCoroutine(WebRTC.Update());
            RTCConfiguration configuration = default;
            peer = new RTCPeerConnection(ref configuration);
            peer.OnConnectionStateChange = HandleConnectionState;
            peer.OnTrack = HandleRemoteTrack;

            sendStream = new MediaStream();
            microphoneTrack = new AudioStreamTrack(microphoneSource) { Loopback = false };
            microphoneTrack.Enabled = false;
            peer.AddTrack(microphoneTrack, sendStream);

            initialized = true;
            muted = true;
            StatusChanged?.Invoke("WebRTC audio ready");
            success?.Invoke();
        }

        private IEnumerator EnsureMicrophonePermission()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Permission.RequestUserPermission(Permission.Microphone);
                float deadline = Time.realtimeSinceStartup + 8f;
                while (!Permission.HasUserAuthorizedPermission(Permission.Microphone) && Time.realtimeSinceStartup < deadline)
                    yield return null;
            }
#elif UNITY_IOS && !UNITY_EDITOR
            if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
                yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
#else
            if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
                yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
#endif
        }

        private static bool HasMicrophonePermission()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return Permission.HasUserAuthorizedPermission(Permission.Microphone);
#else
            return Application.HasUserAuthorization(UserAuthorization.Microphone);
#endif
        }

        public IEnumerator CreateLocalOffer(Action<string> success, Action<string> failed)
        {
            if (!IsInitialized) { failed?.Invoke("Voice runtime is not initialized."); yield break; }
            var offer = peer.CreateOffer();
            yield return offer;
            if (offer.IsError) { failed?.Invoke(offer.Error.message); yield break; }
            RTCSessionDescription description = offer.Desc;
            var local = peer.SetLocalDescription(ref description);
            yield return local;
            if (local.IsError) { failed?.Invoke(local.Error.message); yield break; }
            success?.Invoke(description.sdp);
        }

        public IEnumerator ApplyRemoteAnswer(string sdp, Action success, Action<string> failed) => ApplyRemoteDescription(sdp, RTCSdpType.Answer, success, failed);

        public IEnumerator ApplyRemoteOfferAndCreateAnswer(string sdp, Action<string> success, Action<string> failed)
        {
            bool remoteApplied = false;
            string remoteError = null;
            yield return ApplyRemoteDescription(sdp, RTCSdpType.Offer, () => remoteApplied = true, e => remoteError = e);
            if (!remoteApplied) { failed?.Invoke(remoteError ?? "Remote offer failed."); yield break; }
            var answer = peer.CreateAnswer();
            yield return answer;
            if (answer.IsError) { failed?.Invoke(answer.Error.message); yield break; }
            RTCSessionDescription description = answer.Desc;
            var local = peer.SetLocalDescription(ref description);
            yield return local;
            if (local.IsError) { failed?.Invoke(local.Error.message); yield break; }
            success?.Invoke(description.sdp);
        }

        private IEnumerator ApplyRemoteDescription(string sdp, RTCSdpType type, Action success, Action<string> failed)
        {
            if (!IsInitialized || string.IsNullOrWhiteSpace(sdp)) { failed?.Invoke("Invalid remote SDP."); yield break; }
            var description = new RTCSessionDescription { type = type, sdp = sdp };
            var operation = peer.SetRemoteDescription(ref description);
            yield return operation;
            if (operation.IsError) { failed?.Invoke(operation.Error.message); yield break; }
            success?.Invoke();
        }

        public void SetMuted(bool value)
        {
            muted = value;
            if (microphoneTrack != null) microphoneTrack.Enabled = !value;
            StatusChanged?.Invoke(value ? "Microphone muted" : "Microphone live");
        }

        private void HandleRemoteTrack(RTCTrackEvent trackEvent)
        {
            if (trackEvent.Track is not AudioStreamTrack audioTrack || remoteAudioSource == null) return;
            remoteAudioSource.SetTrack(audioTrack);
            remoteAudioSource.loop = true;
            if (!remoteAudioSource.isPlaying) remoteAudioSource.Play();
        }

        private void HandleConnectionState(RTCPeerConnectionState state)
        {
            StatusChanged?.Invoke("WebRTC: " + state);
            if (state == RTCPeerConnectionState.Connected) Connected?.Invoke();
            if (state == RTCPeerConnectionState.Closed || state == RTCPeerConnectionState.Failed || state == RTCPeerConnectionState.Disconnected)
                Disconnected?.Invoke();
        }

        private void EnsureAudioSources()
        {
            if (microphoneSource == null)
            {
                var input = new GameObject("VoiceMicrophoneSource");
                input.transform.SetParent(transform, false);
                microphoneSource = input.AddComponent<AudioSource>();
                microphoneSource.spatialBlend = 0f;
            }
            if (remoteAudioSource == null)
            {
                var output = new GameObject("VoiceRemoteAudioSource");
                output.transform.SetParent(transform, false);
                remoteAudioSource = output.AddComponent<AudioSource>();
                remoteAudioSource.spatialBlend = 0f;
            }
        }

        public void Shutdown()
        {
            initialized = false;
            muted = true;
            if (microphoneTrack != null) { microphoneTrack.Enabled = false; microphoneTrack.Dispose(); microphoneTrack = null; }
            sendStream?.Dispose(); sendStream = null;
            peer?.Close(); peer?.Dispose(); peer = null;
            if (microphoneSource != null) microphoneSource.Stop();
            if (remoteAudioSource != null) remoteAudioSource.Stop();
            if (Microphone.IsRecording(microphoneDevice)) Microphone.End(microphoneDevice);
            microphoneClip = null;
            if (webRtcUpdate != null) { StopCoroutine(webRtcUpdate); webRtcUpdate = null; }
            StatusChanged?.Invoke("Voice stopped");
            Disconnected?.Invoke();
        }

        private void OnDestroy() => Shutdown();
    }
}
