using System;

namespace Fsp.Presentation
{
    [Serializable]
    public sealed class CosmeticLoadout
    {
        public string headItemId = "head_default";
        public string faceItemId = "face_none";
        public string torsoItemId = "torso_default";
        public string legsItemId = "legs_default";
        public string backpackItemId = "backpack_default";
        public string parachuteItemId = "parachute_default";

        public string Get(CosmeticSlot slot)
        {
            return slot switch
            {
                CosmeticSlot.Head => headItemId,
                CosmeticSlot.Face => faceItemId,
                CosmeticSlot.Torso => torsoItemId,
                CosmeticSlot.Legs => legsItemId,
                CosmeticSlot.Backpack => backpackItemId,
                CosmeticSlot.Parachute => parachuteItemId,
                _ => string.Empty
            };
        }

        public void Set(CosmeticSlot slot, string itemId)
        {
            switch (slot)
            {
                case CosmeticSlot.Head: headItemId = itemId; break;
                case CosmeticSlot.Face: faceItemId = itemId; break;
                case CosmeticSlot.Torso: torsoItemId = itemId; break;
                case CosmeticSlot.Legs: legsItemId = itemId; break;
                case CosmeticSlot.Backpack: backpackItemId = itemId; break;
                case CosmeticSlot.Parachute: parachuteItemId = itemId; break;
            }
        }
    }
}
