using UnityEngine;

namespace Fsp.Networking
{
    /// <summary>
    /// Legacy scene component retained so existing authored scenes do not lose a script reference.
    /// Online kill feed is now driven exclusively by server-authoritative elimination events through
    /// NetworkEliminationBridge, preventing duplicate or spoofed kill messages.
    /// </summary>
    public sealed class NetworkKillFeedReporter : MonoBehaviour
    {
    }
}
