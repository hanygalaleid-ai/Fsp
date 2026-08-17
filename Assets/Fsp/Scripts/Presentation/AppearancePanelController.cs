using System.Collections.Generic;
using Fsp.Backend;
using UnityEngine;

namespace Fsp.Presentation
{
    public sealed class AppearancePanelController : MonoBehaviour
    {
        [SerializeField] private CosmeticCatalog catalog;
        [SerializeField] private ModularCharacterCosmetics previewCharacter;
        [SerializeField] private SupabaseCosmeticsClient cosmeticsClient;
        [SerializeField] private CosmeticSlot activeSlot = CosmeticSlot.Torso;

        private readonly HashSet<string> owned = new();
        private CosmeticLoadout equipped;
        private CosmeticItemDefinition previewItem;

        public CosmeticSlot ActiveSlot => activeSlot;
        public CosmeticItemDefinition PreviewItem => previewItem;
        public CosmeticLoadout Equipped => equipped;

        public async void RefreshAsync()
        {
            if (cosmeticsClient == null) return;
            owned.Clear();
            foreach (string id in await cosmeticsClient.LoadOwnedAsync())
                if (!string.IsNullOrWhiteSpace(id)) owned.Add(id);

            equipped = await cosmeticsClient.LoadEquippedAsync();
            ApplyLoadout(equipped);
        }

        public void SelectSlot(CosmeticSlot slot)
        {
            activeSlot = slot;
            previewItem = null;
        }

        public List<CosmeticItemDefinition> GetVisibleItems(bool ownedOnly)
        {
            var result = new List<CosmeticItemDefinition>();
            if (catalog == null) return result;
            foreach (var item in catalog.GetBySlot(activeSlot))
            {
                if (item == null) continue;
                bool itemOwned = item.itemId.StartsWith("default_") || owned.Contains(item.itemId);
                if (ownedOnly && !itemOwned) continue;
                result.Add(item);
            }
            return result;
        }

        public bool IsOwned(CosmeticItemDefinition item)
        {
            if (item == null) return false;
            return item.itemId.StartsWith("default_") || owned.Contains(item.itemId);
        }

        public void Preview(CosmeticItemDefinition item)
        {
            if (item == null || previewCharacter == null) return;
            previewItem = item;
            previewCharacter.Equip(item);
        }

        public async void EquipPreviewAsync()
        {
            if (previewItem == null || cosmeticsClient == null || !IsOwned(previewItem)) return;
            equipped ??= new CosmeticLoadout();
            equipped.Set(previewItem.slot, previewItem.itemId);
            await cosmeticsClient.SaveEquippedAsync(equipped);
            ApplyLoadout(equipped);
        }

        public void CancelPreview()
        {
            previewItem = null;
            ApplyLoadout(equipped);
        }

        private void ApplyLoadout(CosmeticLoadout loadout)
        {
            if (previewCharacter == null || catalog == null || loadout == null) return;
            foreach (CosmeticSlot slot in System.Enum.GetValues(typeof(CosmeticSlot)))
            {
                string id = loadout.Get(slot);
                CosmeticItemDefinition item = catalog.Find(id);
                if (item != null) previewCharacter.Equip(item);
                else previewCharacter.Unequip(slot);
            }
        }
    }
}
