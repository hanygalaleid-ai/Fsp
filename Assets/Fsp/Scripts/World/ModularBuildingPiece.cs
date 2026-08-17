using UnityEngine;

namespace Fsp.World
{
    public enum BuildingPieceType { Wall, WallWindow, WallDoor, Corner, Floor, Roof, Stair, Balcony, Prop }

    public sealed class ModularBuildingPiece : MonoBehaviour
    {
        [SerializeField] private BuildingPieceType pieceType;
        [SerializeField] private Vector3Int gridSize = Vector3Int.one;
        [SerializeField] private Transform[] snapPoints;
        [SerializeField] private LODGroup lodGroup;
        [SerializeField] private Collider[] colliders;

        public BuildingPieceType PieceType => pieceType;
        public Vector3Int GridSize => gridSize;
        public Transform[] SnapPoints => snapPoints;

        public void SetGameplayActive(bool active)
        {
            if (colliders == null) return;
            foreach (var c in colliders) if (c != null) c.enabled = active;
        }

        public void ApplyLodBias(float multiplier)
        {
            if (lodGroup == null) return;
            lodGroup.size = Mathf.Max(0.01f, lodGroup.size * Mathf.Max(0.25f, multiplier));
        }
    }
}
