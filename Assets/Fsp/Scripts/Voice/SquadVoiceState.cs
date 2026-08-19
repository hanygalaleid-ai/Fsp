using System;
using UnityEngine;

namespace Fsp.Voice
{
    public sealed class SquadVoiceState : MonoBehaviour
    {
        public static SquadVoiceState Instance { get; private set; }

        public bool TokenReady => CurrentToken != null && !string.IsNullOrWhiteSpace(CurrentToken.token);
        public bool Connected { get; private set; }
        public bool MicrophoneMuted { get; private set; } = true;
        public bool PushToTalk { get; private set; } = true;
        public string RuntimeStatus { get; private set; } = "Voice runtime not connected";
        public CloudflareVoiceTokenClient.VoiceToken CurrentToken { get; private set; }

        public event Action Changed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Stores a server-issued RealtimeKit token. This does NOT mean audio is connected.
        /// A platform voice runtime must successfully join the meeting before Connected becomes true.
        /// </summary>
        public void SetToken(CloudflareVoiceTokenClient.VoiceToken token)
        {
            CurrentToken = token;
            Connected = false;
            MicrophoneMuted = true;
            RuntimeStatus = TokenReady ? "Voice token ready; runtime connection required" : "Voice token unavailable";
            Changed?.Invoke();
        }

        public void MarkRuntimeConnected()
        {
            if (!TokenReady) return;
            Connected = true;
            MicrophoneMuted = true;
            RuntimeStatus = "Voice connected";
            Changed?.Invoke();
        }

        public void MarkRuntimeError(string message)
        {
            Connected = false;
            MicrophoneMuted = true;
            RuntimeStatus = string.IsNullOrWhiteSpace(message) ? "Voice connection failed" : message.Trim();
            Changed?.Invoke();
        }

        public void SetPushToTalk(bool enabled)
        {
            PushToTalk = enabled;
            if (enabled) MicrophoneMuted = true;
            Changed?.Invoke();
        }

        public void SetMuted(bool muted)
        {
            if (!Connected) return;
            MicrophoneMuted = muted;
            Changed?.Invoke();
        }

        public void BeginTalking()
        {
            if (!Connected || !PushToTalk) return;
            MicrophoneMuted = false;
            Changed?.Invoke();
        }

        public void EndTalking()
        {
            if (!Connected || !PushToTalk) return;
            MicrophoneMuted = true;
            Changed?.Invoke();
        }

        public void Disconnect()
        {
            Connected = false;
            CurrentToken = null;
            MicrophoneMuted = true;
            RuntimeStatus = "Voice disconnected";
            Changed?.Invoke();
        }
    }
}
