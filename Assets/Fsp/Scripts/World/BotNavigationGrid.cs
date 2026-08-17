using System.Collections.Generic;
using UnityEngine;

namespace Fsp.World
{
    public sealed class BotNavigationGrid : MonoBehaviour
    {
        [SerializeField] private Vector2 worldSize = new(2400f, 2400f);
        [SerializeField] private float cellSize = 6f;
        [SerializeField] private LayerMask obstacleMask = ~0;
        [SerializeField] private float agentRadius = 0.5f;

        private bool[,] blocked;
        private int width;
        private int height;
        private Vector3 origin;

        private void Awake()
        {
            width = Mathf.Max(1, Mathf.CeilToInt(worldSize.x / cellSize));
            height = Mathf.Max(1, Mathf.CeilToInt(worldSize.y / cellSize));
            origin = transform.position - new Vector3(worldSize.x * 0.5f, 0f, worldSize.y * 0.5f);
            blocked = new bool[width, height];
        }

        public void RebuildLocal(Rect worldRect)
        {
            int minX = Mathf.Clamp(Mathf.FloorToInt((worldRect.xMin - origin.x) / cellSize), 0, width - 1);
            int maxX = Mathf.Clamp(Mathf.CeilToInt((worldRect.xMax - origin.x) / cellSize), 0, width - 1);
            int minY = Mathf.Clamp(Mathf.FloorToInt((worldRect.yMin - origin.z) / cellSize), 0, height - 1);
            int maxY = Mathf.Clamp(Mathf.CeilToInt((worldRect.yMax - origin.z) / cellSize), 0, height - 1);

            for (int x = minX; x <= maxX; x++)
                for (int y = minY; y <= maxY; y++)
                {
                    Vector3 p = CellToWorld(x, y) + Vector3.up;
                    blocked[x, y] = Physics.CheckSphere(p, agentRadius, obstacleMask, QueryTriggerInteraction.Ignore);
                }
        }

        public bool TryGetNearestWalkable(Vector3 world, out Vector3 result, int radiusCells = 6)
        {
            WorldToCell(world, out int cx, out int cy);
            for (int r = 0; r <= radiusCells; r++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    for (int dy = -r; dy <= r; dy++)
                    {
                        int x = cx + dx;
                        int y = cy + dy;
                        if (x < 0 || y < 0 || x >= width || y >= height || blocked[x, y]) continue;
                        result = CellToWorld(x, y);
                        return true;
                    }
                }
            }
            result = world;
            return false;
        }

        public IEnumerable<Vector3> GetNeighbors(Vector3 world)
        {
            WorldToCell(world, out int cx, out int cy);
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int x = cx + dx;
                    int y = cy + dy;
                    if (x >= 0 && y >= 0 && x < width && y < height && !blocked[x, y])
                        yield return CellToWorld(x, y);
                }
        }

        private Vector3 CellToWorld(int x, int y) => origin + new Vector3((x + 0.5f) * cellSize, 0f, (y + 0.5f) * cellSize);

        private void WorldToCell(Vector3 world, out int x, out int y)
        {
            x = Mathf.Clamp(Mathf.FloorToInt((world.x - origin.x) / cellSize), 0, width - 1);
            y = Mathf.Clamp(Mathf.FloorToInt((world.z - origin.z) / cellSize), 0, height - 1);
        }
    }
}
