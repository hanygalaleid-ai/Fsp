using System.Collections.Generic;
using UnityEngine;

namespace Fsp.Presentation
{
    [CreateAssetMenu(menuName = "Fsp/Presentation/Cosmetic Catalog", fileName = "CosmeticCatalog")]
    public sealed class CosmeticCatalog : ScriptableObject
    {
        [SerializeField] private CosmeticItemDefinition[] items;
        private Dictionary<string, CosmeticItemDefinition> lookup;

        public CosmeticItemDefinition Find(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId)) return null;
            EnsureLookup();
            lookup.TryGetValue(itemId, out var item);
            return item;
        }

        public IEnumerable<CosmeticItemDefinition> All
        {
            get
            {
                if (items == null) yield break;
                foreach (var item in items) if (item != null) yield return item;
            }
        }

        private void EnsureLookup()
        {
            if (lookup != null) return;
            lookup = new Dictionary<string, CosmeticItemDefinition>();
            if (items == null) return;
            foreach (var item in items)
                if (item != null && !string.IsNullOrWhiteSpace(item.itemId)) lookup[item.itemId] = item;
        }
    }
}
