using System;
using Fsp.BattleRoyale;

namespace Fsp.UI
{
    public static class KillFeedBus
    {
        public static event Action<MatchParticipant, MatchParticipant> KillReported;

        public static int LocalPlayerKills { get; private set; }

        public static void Report(MatchParticipant attacker, MatchParticipant victim)
        {
            if (attacker != null && attacker.IsLocalPlayer && victim != null && victim != attacker)
                LocalPlayerKills++;

            KillReported?.Invoke(attacker, victim);
        }

        public static void ResetForMatch()
        {
            LocalPlayerKills = 0;
        }
    }
}
