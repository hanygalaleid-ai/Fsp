using Fsp.BattleRoyale;
using Fsp.Core;
using Fsp.Inventory;
using Fsp.Player;
using Fsp.UI;
using UnityEngine;

namespace Fsp.BattleRoyale
{
    /// <summary>
    /// Gameplay-only safety assembler for the checked-in Match scene.
    /// Release builds must use authored scene art. This component never creates placeholder
    /// characters, weapons, vehicles, aircraft, loot geometry, or fallback HUD visuals.
    /// </summary>
    public sealed class MatchSceneAssembler : MonoBehaviour
    {
        private MatchParticipant localParticipant;

        private void Awake()
        {
            EnsureMatchManager();
            localParticipant = FindLocalPlayer();
            if (localParticipant == null)
            {
                Debug.LogError("FSP Match: no authored local MatchParticipant found. Runtime placeholder player creation is disabled.");
                return;
            }

            EnsureGameplayComponents(localParticipant.gameObject);
            WireExistingHud(localParticipant.gameObject);
        }

        private static MatchManager EnsureMatchManager()
        {
            MatchManager existing = FindFirstObjectByType<MatchManager>();
            return existing != null ? existing : new GameObject("MatchManager").AddComponent<MatchManager>();
        }

        private static MatchParticipant FindLocalPlayer()
        {
            foreach (MatchParticipant participant in FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None))
            {
                if (participant != null && participant.IsLocalPlayer)
                    return participant;
            }
            return null;
        }

        private static void EnsureGameplayComponents(GameObject player)
        {
            if (player == null) return;
            if (player.GetComponent<PlayerDamageable>() == null) player.AddComponent<PlayerDamageable>();
            if (player.GetComponent<PlayerVitals>() == null) player.AddComponent<PlayerVitals>();
            if (player.GetComponent<ThirdPersonMotor>() == null) player.AddComponent<ThirdPersonMotor>();
            if (player.GetComponent<ParachuteController>() == null) player.AddComponent<ParachuteController>();
            if (player.GetComponent<PlayerInventory>() == null) player.AddComponent<PlayerInventory>();
        }

        private static void WireExistingHud(GameObject player)
        {
            if (player == null) return;
            BattleRoyaleHud hud = FindFirstObjectByType<BattleRoyaleHud>();
            if (hud == null)
            {
                Debug.LogWarning("FSP Match: authored BattleRoyaleHud not found; fallback HUD generation is disabled.");
                return;
            }

            hud.ConfigureSources(
                player.GetComponent<PlayerVitals>(),
                player.GetComponent<PlayerInventory>(),
                FindFirstObjectByType<MatchManager>(),
                FindFirstObjectByType<SafeZoneController>(),
                player.transform);
        }
    }
}
