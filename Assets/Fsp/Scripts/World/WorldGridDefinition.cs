using UnityEngine;

namespace Fsp.World
{
    [CreateAssetMenu(menuName = "Fsp/World/Grid Definition", fileName = "WorldGridDefinition")]
    public sealed class WorldGridDefinition : ScriptableObject
    {
        [Min(64f)] public float cellSize = 256f;
        [Min(1)] public int cellsX = 10;
        [Min(1)] public int cellsZ = 10;
        [Min(1)] public int activeRadiusCells = 2;
        [Min(1)] public int preloadRadiusCells = 3;

        public Vector2Int WorldToCell(Vector3 world)
        {
            int x = Mathf.FloorToInt(world.x / cellSize);
            int z = Mathf.FloorToInt(world.z / cellSize);
            return new Vector2Int(x, z);
        }

        public Vector3 CellCenter(Vector2Int cell)
        {
            return new Vector3((cell.x + 0.5f) * cellSize, 0f, (cell.y + 0.5f) * cellSize);
        }
    }
}
