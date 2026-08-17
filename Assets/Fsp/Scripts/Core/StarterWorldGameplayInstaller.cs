using Fsp.World;
using UnityEngine;

namespace Fsp.Core
{
    public static class StarterWorldGameplayInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (Object.FindObjectOfType<OldCrownInteriorPrototype>() == null)
            {
                var oldCrown = new GameObject("OldCrown_Interiors");
                oldCrown.transform.position = new Vector3(-60f, 0f, 35f);
                oldCrown.AddComponent<OldCrownInteriorPrototype>();
            }

            if (Object.FindObjectOfType<OldCrownDoorInstaller>() == null)
            {
                var doors = new GameObject("OldCrown_Doors");
                doors.AddComponent<OldCrownDoorInstaller>();
            }

            if (Object.FindObjectOfType<CopperPortPrototype>() == null)
            {
                var copperPort = new GameObject("CopperPort_Prototype");
                copperPort.AddComponent<CopperPortPrototype>();
            }

            if (Object.FindObjectOfType<DryfieldPrototype>() == null)
            {
                var dryfield = new GameObject("Dryfield_Prototype");
                dryfield.AddComponent<DryfieldPrototype>();
            }

            if (Object.FindObjectOfType<StarterPoiRoadLink>() == null)
            {
                var road = new GameObject("OldCrown_CopperPort_Road");
                road.AddComponent<StarterPoiRoadLink>();
            }
        }
    }
}
