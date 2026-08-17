using UnityEngine;

namespace Fsp.World
{
    [CreateAssetMenu(menuName = "Fsp/World/POI Architecture Profile", fileName = "PoiArchitectureProfile")]
    public sealed class PoiArchitectureProfile : ScriptableObject
    {
        public string poiId;
        [TextArea] public string visualDescription;
        public Material[] sharedMaterials;
        public ModularBuildingPiece[] allowedPieces;
        [Range(0f, 1f)] public float verticality = 0.35f;
        [Range(0f, 1f)] public float interiorDensity = 0.5f;
        [Range(0f, 1f)] public float coverDensity = 0.55f;
        [Range(0f, 1f)] public float longSightlineBias = 0.4f;
        [Min(1)] public int maxUniqueMaterials = 4;
        [Min(1)] public int targetBuildings = 12;
    }
}
