using Fsp.Backend;
using Fsp.Bots;
using UnityEngine;

namespace Fsp.BattleRoyale
{
    public sealed class MatchPopulationBootstrap : MonoBehaviour
    {
        [SerializeField] private BotSpawner botSpawner;
        [SerializeField, Min(1)] private int fallbackHumanPlayers = 1;

        private void Start()
        {
            if (botSpawner == null) botSpawner = FindFirstObjectByType<BotSpawner>();
            if (botSpawner == null) return;

            int humans = fallbackHumanPlayers;
            if (MatchRoomState.Instance != null && MatchRoomState.Instance.HasMatch)
                humans = Mathf.Max(1, MatchRoomState.Instance.MemberCount);

            botSpawner.FillToTarget(humans);
        }
    }
}
