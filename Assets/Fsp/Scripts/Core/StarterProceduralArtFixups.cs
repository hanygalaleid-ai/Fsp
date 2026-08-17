using Fsp.Combat;
using Fsp.Presentation;
using Fsp.Vehicles;
using UnityEngine;

namespace Fsp.Core
{
    public static class StarterProceduralArtFixups
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Apply()
        {
            foreach (HitscanWeapon weapon in Object.FindObjectsOfType<HitscanWeapon>())
            {
                if (weapon != null && weapon.GetComponent<StarterProceduralWeaponVisual>() == null)
                    weapon.gameObject.AddComponent<StarterProceduralWeaponVisual>();
            }

            foreach (SimpleVehicleController vehicle in Object.FindObjectsOfType<SimpleVehicleController>())
            {
                if (vehicle != null && vehicle.GetComponent<StarterProceduralVehicleVisual>() == null)
                    vehicle.gameObject.AddComponent<StarterProceduralVehicleVisual>();
            }
        }
    }
}
