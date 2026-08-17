using Fsp.BattleRoyale;
using Fsp.Player;
using Fsp.Vehicles;
using UnityEngine;

namespace Fsp.Core
{
    public static class StarterRuntimeFixups
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Apply()
        {
            MatchParticipant[] participants = Object.FindObjectsOfType<MatchParticipant>();
            foreach (MatchParticipant participant in participants)
            {
                if (participant == null || !participant.IsLocalPlayer) continue;
                GameObject player = participant.gameObject;

                if (player.GetComponent<PlayerDamageable>() == null)
                    player.AddComponent<PlayerDamageable>();
                if (player.GetComponent<StarterThirdPersonRig>() == null)
                    player.AddComponent<StarterThirdPersonRig>();
                if (player.GetComponent<SafeZoneDamageApplier>() == null)
                    player.AddComponent<SafeZoneDamageApplier>();
                if (player.GetComponent<StarterVehicleInput>() == null)
                    player.AddComponent<StarterVehicleInput>();
                break;
            }
        }
    }
}
