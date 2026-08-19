using System;
using Fsp.BattleRoyale;

namespace Fsp.UI
{
    public static class KillFeedBus
    {
        public static event Action<MatchParticipant, MatchParticipant> KillReported;
        public static event Action<string, string> NetworkKillReported;

        public static int LocalPlayerKills { get; private set; }

        public static void Report(MatchParticipant attacker, MatchParticipant victim)
        {
            if (attacker != null && attacker.IsLocalPlayer && victim != null && victim != attacker)
                LocalPlayerKills++;
            KillReported?.Invoke(attacker, victim);
        }

        public static void ReportNetwork(string killerName, string victimName)
        {
            NetworkKillReported?.Invoke(killerName, victimName);
        }

        public static void ReportNetworkElimination(string killerId, string victimId, string localPlayerId)
        {
            if (!string.IsNullOrWhiteSpace(killerId) && killerId == localPlayerId && killerId != victimId)
                LocalPlayerKills++;
            NetworkKillReported?.Invoke(ShortName(killerId), ShortName(victimId));
        }

        public static void ResetForMatch() => LocalPlayerKills = 0;

        private static string ShortName(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return "Player";
            return id.Length <= 8 ? id : id.Substring(0, 8);
        }
    }
}
