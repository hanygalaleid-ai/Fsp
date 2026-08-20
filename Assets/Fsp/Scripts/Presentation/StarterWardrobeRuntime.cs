using UnityEngine;

namespace Fsp.Presentation
{
    public static class StarterWardrobeRuntime
    {
        private const string Prefix = "fsp.wardrobe.";

        public static CosmeticLoadout LoadLocal()
        {
            return new CosmeticLoadout
            {
                headItemId = PlayerPrefs.GetString(Prefix + "head", "head_default"),
                faceItemId = PlayerPrefs.GetString(Prefix + "face", "face_none"),
                torsoItemId = PlayerPrefs.GetString(Prefix + "torso", "torso_default"),
                legsItemId = PlayerPrefs.GetString(Prefix + "legs", "legs_default"),
                backpackItemId = PlayerPrefs.GetString(Prefix + "backpack", "backpack_default"),
                parachuteItemId = PlayerPrefs.GetString(Prefix + "parachute", "parachute_default")
            };
        }

        public static void SaveLocal(CosmeticLoadout loadout)
        {
            if (loadout == null) return;
            PlayerPrefs.SetString(Prefix + "head", loadout.headItemId ?? "head_default");
            PlayerPrefs.SetString(Prefix + "face", loadout.faceItemId ?? "face_none");
            PlayerPrefs.SetString(Prefix + "torso", loadout.torsoItemId ?? "torso_default");
            PlayerPrefs.SetString(Prefix + "legs", loadout.legsItemId ?? "legs_default");
            PlayerPrefs.SetString(Prefix + "backpack", loadout.backpackItemId ?? "backpack_default");
            PlayerPrefs.SetString(Prefix + "parachute", loadout.parachuteItemId ?? "parachute_default");
            PlayerPrefs.Save();
        }
    }
}
