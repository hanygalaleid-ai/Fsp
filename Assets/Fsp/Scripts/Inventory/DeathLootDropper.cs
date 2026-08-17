using Fsp.Backend;
using Fsp.Player;
using UnityEngine;

namespace Fsp.Inventory
{
    [RequireComponent(typeof(PlayerVitals))]
    public sealed class DeathLootDropper : MonoBehaviour
    {
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private DeathLootCrate cratePrefab;
        [SerializeField] private Transform dropPoint;

        private PlayerVitals vitals;
        private bool dropped;

        private void Awake()
        {
            vitals = GetComponent<PlayerVitals>();
            if (inventory == null) inventory = GetComponent<PlayerInventory>();
        }

        private void OnEnable()
        {
            if (vitals != null) vitals.Died += Drop;
        }

        private void OnDisable()
        {
            if (vitals != null) vitals.Died -= Drop;
        }

        private void Drop()
        {
            if (dropped || inventory == null || cratePrefab == null) return;
            dropped = true;

            DeathInventorySnapshot snapshot = inventory.DrainForDeath();
            if (snapshot.IsEmpty) return;

            Vector3 position = dropPoint != null ? dropPoint.position : transform.position;
            DeathLootCrate crate = Instantiate(cratePrefab, position, Quaternion.identity);
            string match = MatchRoomState.HasMatch ? MatchRoomState.MatchId : "offline";
            string owner = SupabaseSession.IsSignedIn ? SupabaseSession.UserId : gameObject.name;
            crate.Initialize($"{match}:{owner}", snapshot);
        }
    }
}
