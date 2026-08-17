using System.Collections.Generic;
using UnityEngine;

namespace Fsp.Presentation
{
    public sealed class ModularCharacterCosmetics : MonoBehaviour
    {
        [System.Serializable]
        private sealed class SlotAnchor
        {
            public CosmeticSlot slot;
            public Transform anchor;
        }

        [System.Serializable]
        private sealed class BodyRegionRenderer
        {
            public BodyRegionMask region;
            public Renderer renderer;
        }

        [SerializeField] private SlotAnchor[] anchors;
        [SerializeField] private BodyRegionRenderer[] bodyRegions;

        private readonly Dictionary<CosmeticSlot, GameObject> equipped = new();
        private readonly Dictionary<CosmeticSlot, CosmeticItemDefinition> definitions = new();

        public void Equip(CosmeticItemDefinition item)
        {
            if (item == null || item.prefab == null) return;
            Transform anchor = FindAnchor(item.slot);
            if (anchor == null) return;

            Unequip(item.slot);
            GameObject instance = Instantiate(item.prefab, anchor);
            instance.transform.localPosition = item.localPosition;
            instance.transform.localRotation = Quaternion.Euler(item.localEuler);
            instance.transform.localScale = item.localScale;
            equipped[item.slot] = instance;
            definitions[item.slot] = item;
            RefreshBodyVisibility();
        }

        public void Unequip(CosmeticSlot slot)
        {
            if (equipped.TryGetValue(slot, out GameObject current) && current != null)
                Destroy(current);
            equipped.Remove(slot);
            definitions.Remove(slot);
            RefreshBodyVisibility();
        }

        private Transform FindAnchor(CosmeticSlot slot)
        {
            if (anchors == null) return null;
            foreach (var entry in anchors)
                if (entry != null && entry.slot == slot) return entry.anchor;
            return null;
        }

        private void RefreshBodyVisibility()
        {
            BodyRegionMask hidden = BodyRegionMask.None;
            foreach (var pair in definitions)
                if (pair.Value != null) hidden |= pair.Value.hiddenBodyRegions;

            if (bodyRegions == null) return;
            foreach (var entry in bodyRegions)
            {
                if (entry == null || entry.renderer == null) continue;
                entry.renderer.enabled = (hidden & entry.region) == 0;
            }
        }
    }
}
