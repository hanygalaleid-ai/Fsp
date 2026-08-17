using System;
using UnityEngine;

namespace Fsp.Voice
{
    public sealed class SquadVoiceState : MonoBehaviour
    {
        public static SquadVoiceState Instance { get; private set; }

        public bool Connected { get; private set; }
        public bool MicrophoneMuted { get; private set; } = true;
        public bool PushToTalk { get; private set; } = true;
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

        public void SetToken(CloudflareVoiceTokenClient.VoiceToken token)
        {
            CurrentToken = token;
            Connected = token != null && !string.IsNullOrWhiteSpace(token.token);
            MicrophoneMuted = true;
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
            Changed?.Invoke();
        }
    }
}
