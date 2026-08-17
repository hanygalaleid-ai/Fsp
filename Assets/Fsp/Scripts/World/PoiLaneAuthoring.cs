using UnityEngine;

namespace Fsp.World
{
    public sealed class PoiLaneAuthoring : MonoBehaviour
    {
        [SerializeField] private Transform[] lootLaneAnchors;
        [SerializeField] private Transform[] vehicleLaneAnchors;
        [SerializeField] private Transform[] flankAnchors;

        public Transform[] LootLaneAnchors => lootLaneAnchors;
        public Transform[] VehicleLaneAnchors => vehicleLaneAnchors;
        public Transform[] FlankAnchors => flankAnchors;

        public Transform GetLootAnchor(int index)
        {
            if (lootLaneAnchors == null || lootLaneAnchors.Length == 0) return null;
            return lootLaneAnchors[Mathf.Abs(index) % lootLaneAnchors.Length];
        }

        public Transform GetVehicleAnchor(int index)
        {
            if (vehicleLaneAnchors == null || vehicleLaneAnchors.Length == 0) return null;
            return vehicleLaneAnchors[Mathf.Abs(index) % vehicleLaneAnchors.Length];
        }

        public Transform GetFlankAnchor(int index)
        {
            if (flankAnchors == null || flankAnchors.Length == 0) return null;
            return flankAnchors[Mathf.Abs(index) % flankAnchors.Length];
        }
    }
}
