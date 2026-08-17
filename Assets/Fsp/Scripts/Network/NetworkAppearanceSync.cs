using System.Collections.Generic;
using Fsp.Backend;
using Fsp.Presentation;
using UnityEngine;

namespace Fsp.Networking
{
    public sealed class NetworkAppearanceSync : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour transportBehaviour;
        [SerializeField] private SupabaseCosmeticsClient cosmeticsBackend;
        [SerializeField] private CosmeticCatalog catalog;

        private INetworkTransport transport;
        private readonly Dictionary<string, CosmeticLoadout> pending = new();
        private bool localSent;

        private void Awake()
        {
            transport = transportBehaviour as INetworkTransport;
        }

        private void OnEnable()
        {
            if (transport != null) transport.AppearanceReceived += HandleAppearance;
        }

        private async void Update()
        {
            if (!localSent && transport != null && transport.IsConnected && cosmeticsBackend != null && SupabaseSession.IsSignedIn)
            {
                localSent = true;
                CosmeticLoadout loadout = await cosmeticsBackend.LoadEquippedAsync();
                transport.SendAppearance(new NetworkAppearanceEvent
                {
                    playerId = SupabaseSession.UserId,
                    loadout = loadout,
                    timestamp = Time.realtimeSinceStartupAsDouble
                });
            }

            if (pending.Count == 0) return;
            var completed = new List<string>();
            foreach (var pair in pending)
            {
                if (!RemotePlayerProxy.TryFind(pair.Key, out var proxy)) continue;
                var modular = proxy.GetComponentInChildren<ModularCharacterCosmetics>(true);
                if (modular == null) continue;
                Apply(modular, pair.Value);
                completed.Add(pair.Key);
            }
            foreach (string id in completed) pending.Remove(id);
        }

        private void HandleAppearance(NetworkAppearanceEvent evt)
        {
            if (evt == null || string.IsNullOrWhiteSpace(evt.playerId) || evt.playerId == SupabaseSession.UserId || evt.loadout == null) return;
            pending[evt.playerId] = evt.loadout;
        }

        private void Apply(ModularCharacterCosmetics target, CosmeticLoadout loadout)
        {
            if (target == null || catalog == null || loadout == null) return;
            Equip(target, loadout.headItemId);
            Equip(target, loadout.faceItemId);
            Equip(target, loadout.torsoItemId);
            Equip(target, loadout.legsItemId);
            Equip(target, loadout.backpackItemId);
            Equip(target, loadout.parachuteItemId);
        }

        private void Equip(ModularCharacterCosmetics target, string itemId)
        {
            CosmeticItemDefinition item = catalog.Find(itemId);
            if (item != null) target.Equip(item);
        }

        private void OnDisable()
        {
            if (transport != null) transport.AppearanceReceived -= HandleAppearance;
        }
    }
}
