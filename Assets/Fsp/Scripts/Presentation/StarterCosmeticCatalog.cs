using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fsp.Presentation
{
    /// <summary>Free original starter wardrobe used by the runtime lobby and procedural match character.</summary>
    public static class StarterCosmeticCatalog
    {
        public readonly struct Item
        {
            public readonly string Id;
            public readonly CosmeticSlot Slot;
            public readonly string Label;
            public readonly Color Color;

            public Item(string id, CosmeticSlot slot, string label, Color color)
            {
                Id = id;
                Slot = slot;
                Label = label;
                Color = color;
            }
        }

        private static readonly Item[] Items =
        {
            new("head_default", CosmeticSlot.Head, "FIELD HELMET", new Color(.055f, .085f, .07f)),
            new("head_sand", CosmeticSlot.Head, "SAND HELMET", new Color(.34f, .27f, .16f)),
            new("head_night", CosmeticSlot.Head, "NIGHT HELMET", new Color(.035f, .045f, .06f)),
            new("face_none", CosmeticSlot.Face, "CLEAR VISOR", new Color(.06f, .08f, .09f)),
            new("face_amber", CosmeticSlot.Face, "AMBER VISOR", new Color(.75f, .27f, .035f)),
            new("face_ice", CosmeticSlot.Face, "ICE VISOR", new Color(.12f, .40f, .52f)),
            new("torso_default", CosmeticSlot.Torso, "FOREST UNIFORM", new Color(.12f, .23f, .16f)),
            new("torso_sand", CosmeticSlot.Torso, "DESERT UNIFORM", new Color(.38f, .30f, .18f)),
            new("torso_night", CosmeticSlot.Torso, "NIGHT UNIFORM", new Color(.045f, .07f, .10f)),
            new("legs_default", CosmeticSlot.Legs, "FOREST TROUSERS", new Color(.10f, .20f, .14f)),
            new("legs_sand", CosmeticSlot.Legs, "DESERT TROUSERS", new Color(.31f, .25f, .16f)),
            new("legs_night", CosmeticSlot.Legs, "NIGHT TROUSERS", new Color(.035f, .055f, .075f)),
            new("backpack_default", CosmeticSlot.Backpack, "FIELD PACK", new Color(.055f, .085f, .07f)),
            new("backpack_sand", CosmeticSlot.Backpack, "SAND PACK", new Color(.30f, .23f, .13f)),
            new("backpack_rescue", CosmeticSlot.Backpack, "RESCUE PACK", new Color(.50f, .12f, .04f)),
            new("parachute_default", CosmeticSlot.Parachute, "FOREST CANOPY", new Color(.10f, .16f, .12f)),
            new("parachute_sand", CosmeticSlot.Parachute, "SAND CANOPY", new Color(.35f, .27f, .15f)),
            new("parachute_night", CosmeticSlot.Parachute, "NIGHT CANOPY", new Color(.035f, .055f, .085f)),
        };

        public static IEnumerable<string> AllItemIds
        {
            get { foreach (Item item in Items) yield return item.Id; }
        }

        public static List<Item> GetItems(CosmeticSlot slot)
        {
            var result = new List<Item>();
            foreach (Item item in Items) if (item.Slot == slot) result.Add(item);
            return result;
        }

        public static Item Find(string id)
        {
            foreach (Item item in Items)
                if (string.Equals(item.Id, id, StringComparison.Ordinal)) return item;
            return DefaultFor(CosmeticSlot.Torso);
        }

        public static Item Find(string id, CosmeticSlot fallbackSlot)
        {
            foreach (Item item in Items)
                if (item.Slot == fallbackSlot && string.Equals(item.Id, id, StringComparison.Ordinal)) return item;
            return DefaultFor(fallbackSlot);
        }

        public static Item DefaultFor(CosmeticSlot slot)
        {
            foreach (Item item in Items) if (item.Slot == slot) return item;
            return default;
        }
    }
}
