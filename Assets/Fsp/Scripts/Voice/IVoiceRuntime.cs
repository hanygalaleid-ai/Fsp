using System;

namespace Fsp.Voice
{
    /// <summary>
    /// Platform/runtime bridge responsible for joining a Cloudflare RealtimeKit meeting
    /// and carrying microphone/remote audio. Token acquisition alone is not a voice connection.
    /// </summary>
    public interface IVoiceRuntime
    {
        bool IsAvailable { get; }
        bool IsConnected { get; }
        bool IsMuted { get; }
        string Status { get; }

        event Action StateChanged;

        void Connect(CloudflareVoiceTokenClient.VoiceToken token, Action success, Action<string> failed);
        void SetMuted(bool muted);
        void Disconnect();
    }
}
