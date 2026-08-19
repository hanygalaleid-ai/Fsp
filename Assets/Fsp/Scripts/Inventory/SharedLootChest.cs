using Fsp.Backend;
using Fsp.Networking;
using UnityEngine;

namespace Fsp.Inventory
{
    [RequireComponent(typeof(Collider))]
    public sealed class SharedLootChest : MonoBehaviour
    {
        [SerializeField] private string chestId = "chest_01";
        [SerializeField] private GameObject[] lootPrefabs;
        [SerializeField] private Transform[] dropPoints;
        [SerializeField] private Animator animator;
        [SerializeField] private string openTrigger = "Open";
        [SerializeField] private MonoBehaviour transportBehaviour;

        private INetworkTransport transport;
        private bool opened;
        private bool claimPending;
        private bool subscribed;

        private void Awake()
        {
            TryResolveTransport();
            GetComponent<Collider>().isTrigger = true;
            if (string.IsNullOrWhiteSpace(chestId)) chestId = gameObject.name;
        }

        private void OnEnable()
        {
            TryResolveTransport();
            TrySubscribe();
        }

        private void Update()
        {
            if (subscribed) return;
            TryResolveTransport();
            TrySubscribe();
        }

        private void OnDisable()
        {
            if (subscribed && transport != null) transport.LootClaimReceived -= HandleClaim;
            subscribed = false;
        }

        private void TryResolveTransport()
        {
            transport = transportBehaviour as INetworkTransport;
            if (transport != null) return;
            foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (behaviour is not INetworkTransport candidate) continue;
                transportBehaviour = behaviour;
                transport = candidate;
                return;
            }
        }

        private void TrySubscribe()
        {
            if (subscribed || transport == null) return;
            transport.LootClaimReceived += HandleClaim;
            subscribed = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<PlayerInventory>() != null) TryOpen();
        }

        public void TryOpen()
        {
            if (opened || claimPending) return;

            bool onlineMatch = SupabaseSession.IsSignedIn && MatchRoomState.HasMatch;
            if (!onlineMatch)
            {
                OpenForEveryone();
                return;
            }

            TryResolveTransport();
            TrySubscribe();
            if (transport == null || !transport.IsConnected) return;

            claimPending = true;
            transport.SendLootClaim(new NetworkLootClaimEvent
            {
                playerId = SupabaseSession.UserId,
                lootId = "chest-open:" + chestId,
                accepted = false,
                timestamp = Time.realtimeSinceStartupAsDouble
            });
        }

        private void HandleClaim(NetworkLootClaimEvent result)
        {
            if (result == null || result.lootId != "chest-open:" + chestId) return;
            claimPending = false;
            if (result.accepted) OpenForEveryone();
        }

        private void OpenForEveryone()
        {
            if (opened) return;
            opened = true;
            if (animator != null && !string.IsNullOrWhiteSpace(openTrigger)) animator.SetTrigger(openTrigger);

            int count = Mathf.Min(lootPrefabs != null ? lootPrefabs.Length : 0, dropPoints != null ? dropPoints.Length : 0);
            for (int i = 0; i < count; i++)
            {
                if (lootPrefabs[i] == null || dropPoints[i] == null) continue;
                GameObject go = Instantiate(lootPrefabs[i], dropPoints[i].position, dropPoints[i].rotation);
                LootPickup pickup = go.GetComponent<LootPickup>();
                if (pickup != null) pickup.SetLootId($"chest:{chestId}:{i}");
            }
        }
    }
}
