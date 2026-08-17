using System.Collections.Generic;
using System.Threading.Tasks;
using Fsp.Backend;
using UnityEngine;

namespace Fsp.Presentation
{
    public sealed class LobbyCosmeticsController : MonoBehaviour
    {
        [SerializeField] private SupabaseCosmeticsClient backend;
        [SerializeField] private CosmeticCatalog catalog;
        [SerializeField] private ModularCharacterCosmetics character;

        private HashSet<string> owned = new();
        private CosmeticLoadout loadout = new();

        public CosmeticLoadout Loadout => loadout;

        private async void Start()
        {
            await RefreshAsync();
        }

        public async Task RefreshAsync()
        {
            if (backend == null || catalog == null || character == null) return;
            owned = await backend.LoadOwnedAsync();
            loadout = await backend.LoadEquippedAsync();
            ApplyLoadout();
        }

        public async void TryEquip(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId) || !owned.Contains(itemId)) return;
            CosmeticItemDefinition item = catalog.Find(itemId);
            if (item == null) return;

            loadout.Set(item.slot, item.itemId);
            character.Equip(item);
            await backend.SaveEquippedAsync(loadout);
        }

        public bool IsOwned(string itemId) => !string.IsNullOrWhiteSpace(itemId) && owned.Contains(itemId);

        private void ApplyLoadout()
        {
            ApplySlot(CosmeticSlot.Head);
            ApplySlot(CosmeticSlot.Face);
            ApplySlot(CosmeticSlot.Torso);
            ApplySlot(CosmeticSlot.Legs);
            ApplySlot(CosmeticSlot.Backpack);
            ApplySlot(CosmeticSlot.Parachute);
        }

        private void ApplySlot(CosmeticSlot slot)
        {
            string id = loadout.Get(slot);
            CosmeticItemDefinition item = catalog.Find(id);
            if (item != null) character.Equip(item);
        }
    }
}
