using Fsp.Backend;
using Fsp.Networking;
using UnityEngine;

namespace Fsp.Inventory
{
    [RequireComponent(typeof(Collider))]
    public sealed class DeathLootCrate : MonoBehaviour
    {
        [SerializeField] private string crateId;
        [SerializeField] private MonoBehaviour transportBehaviour;

        private INetworkTransport transport;
        private DeathInventorySnapshot contents;
        private PlayerInventory pendingInventory;
        private bool claimPending;

        public void Initialize(string id, DeathInventorySnapshot snapshot)
        {
            crateId = id;
            contents = snapshot;
        }

        private void Awake()
        {
            transport = transportBehaviour as INetworkTransport;
            if (string.IsNullOrWhiteSpace(crateId)) crateId = gameObject.name;
            GetComponent<Collider>().isTrigger = true;
            InstallAuthoredVisual();
        }

        private void InstallAuthoredVisual()
        {
            if (transform.Find("BMG_LootCrate_Authored") != null) return;
            GameObject authored = Resources.Load<GameObject>("Models/BMG/bmg_loot_crate_mk1");
            if (authored == null) return;

            Renderer[] oldRenderers = GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < oldRenderers.Length; i++)
                if (oldRenderers[i] != null) oldRenderers[i].enabled = false;

            GameObject model = Instantiate(authored, transform, false);
            model.name = "BMG_LootCrate_Authored";
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one * .85f;

            Shader shader = Resources.Load<Shader>("Shaders/FspMobileSafe");
            if (shader == null) shader = Shader.Find("Fsp/MobileSafeLit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) return;

            Material material = new(shader)
            {
                color = new Color(.27f, .20f, .11f),
                hideFlags = HideFlags.DontSave
            };
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i] != null) renderers[i].sharedMaterial = material;
        }

        private void OnEnable()
        {
            if (transport != null) transport.LootClaimReceived += HandleClaim;
        }

        private void OnDisable()
        {
            if (transport != null) transport.LootClaimReceived -= HandleClaim;
        }

        private void OnTriggerEnter(Collider other)
        {
            TryLoot(other);
        }

        public bool TryLoot(Component target)
        {
            if (claimPending || contents.IsEmpty || target == null) return false;
            var inventory = target.GetComponentInParent<PlayerInventory>();
            if (inventory == null) return false;

            if (transport == null || !transport.IsConnected || !SupabaseSession.IsSignedIn)
            {
                inventory.AddDeathLoot(contents);
                Destroy(gameObject);
                return true;
            }

            pendingInventory = inventory;
            claimPending = true;
            transport.SendLootClaim(new NetworkLootClaimEvent
            {
                playerId = SupabaseSession.UserId,
                lootId = "crate:" + crateId,
                accepted = false,
                timestamp = Time.realtimeSinceStartupAsDouble
            });
            return true;
        }

        private void HandleClaim(NetworkLootClaimEvent result)
        {
            if (result == null || result.lootId != "crate:" + crateId) return;
            if (result.accepted && result.playerId == SupabaseSession.UserId && pendingInventory != null)
                pendingInventory.AddDeathLoot(contents);

            pendingInventory = null;
            claimPending = false;
            Destroy(gameObject);
        }
    }
}
