using UnityEngine;

namespace Fsp.World
{
    [CreateAssetMenu(menuName = "Fsp/World/POI Blueprint", fileName = "PoiBlueprint")]
    public sealed class PoiBlueprint : ScriptableObject
    {
        [Header("Identity")]
        public string poiId = "poi";
        public string displayName = "POI";
        public PoiArchitectureProfile architecture;

        [Header("Layout")]
        [Min(1)] public int buildingCount = 18;
        [Min(0)] public int landmarkCount = 1;
        [Min(0)] public int coverClusters = 14;
        [Min(0)] public int lootLaneCount = 4;
        [Min(0)] public int vehicleLaneCount = 2;
        [Min(10f)] public float footprintRadius = 180f;

        [Header("Combat shape")]
        [Range(0f, 1f)] public float verticality = 0.35f;
        [Range(0f, 1f)] public float closeQuarters = 0.5f;
        [Range(0f, 1f)] public float longSightlines = 0.35f;
        [Range(0f, 1f)] public float flankDensity = 0.55f;

        [Header("Traversal")]
        [Range(0f, 1f)] public float roadDensity = 0.45f;
        [Range(0f, 1f)] public float alleyDensity = 0.5f;
        [Range(0f, 1f)] public float interiorAccess = 0.55f;
    }
}
