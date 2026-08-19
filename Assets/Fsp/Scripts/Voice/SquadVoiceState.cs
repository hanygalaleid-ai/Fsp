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
        public string RuntimeStatus { get; private set; } = "Voice runtime not connected";

        private CloudflareSfuVoiceRuntime runtime;

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

        public void BindRuntime(CloudflareSfuVoiceRuntime value)
        {
            runtime = value;
            MicrophoneMuted = true;
            runtime?.SetMuted(true);
            Changed?.Invoke();
        }

        public void UnbindRuntime(CloudflareSfuVoiceRuntime value)
        {
            if (runtime != value) return;
            runtime = null;
            Connected = false;
            MicrophoneMuted = true;
            Changed?.Invoke();
        }

        public void MarkRuntimeConnected()
        {
            Connected = true;
            MicrophoneMuted = true;
            RuntimeStatus = "Voice connected";
            runtime?.SetMuted(true);
            Changed?.Invoke();
        }

        public void MarkRuntimeError(string message)
        {
            Connected = false;
            MicrophoneMuted = true;
            RuntimeStatus = string.IsNullOrWhiteSpace(message) ? "Voice connection failed" : message.Trim();
            runtime?.SetMuted(true);
            Changed?.Invoke();
        }

        public void SetRuntimeStatus(string message)
        {
            RuntimeStatus = string.IsNullOrWhiteSpace(message) ? RuntimeStatus : message.Trim();
            Changed?.Invoke();
        }

        public void SetPushToTalk(bool enabled)
        {
            PushToTalk = enabled;
            if (enabled)
            {
                MicrophoneMuted = true;
                runtime?.SetMuted(true);
            }
            Changed?.Invoke();
        }

        public void SetMuted(bool muted)
        {
            if (!Connected) return;
            MicrophoneMuted = muted;
            runtime?.SetMuted(muted);
            Changed?.Invoke();
        }

        public void BeginTalking()
        {
            if (!Connected || !PushToTalk) return;
            MicrophoneMuted = false;
            runtime?.SetMuted(false);
            Changed?.Invoke();
        }

        public void EndTalking()
        {
            if (!Connected || !PushToTalk) return;
            MicrophoneMuted = true;
            runtime?.SetMuted(true);
            Changed?.Invoke();
        }

        public void Disconnect()
        {
            Connected = false;
            MicrophoneMuted = true;
            RuntimeStatus = "Voice disconnected";
            runtime?.SetMuted(true);
            Changed?.Invoke();
        }
    }
}
