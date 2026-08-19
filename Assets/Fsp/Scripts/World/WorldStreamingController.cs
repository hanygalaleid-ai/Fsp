using System.Collections.Generic;
using UnityEngine;

namespace Fsp.World
{
    public sealed class WorldStreamingController : MonoBehaviour
    {
        [SerializeField] private WorldGridDefinition grid;
        [SerializeField] private Transform target;
        [SerializeField] private WorldCell[] cells;
        [SerializeField, Min(0.05f)] private float refreshSeconds = 0.35f;

        private float nextRefresh;
        private readonly Dictionary<Vector2Int, WorldCell> lookup = new();

        private void Awake()
        {
            if (cells == null || cells.Length == 0)
                cells = FindObjectsByType<WorldCell>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var cell in cells)
                if (cell != null) lookup[cell.Coordinates] = cell;
        }

        private void Update()
        {
            if (grid == null || target == null || Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + refreshSeconds;
            Refresh();
        }

        private void Refresh()
        {
            Vector2Int center = grid.WorldToCell(target.position);
            foreach (var pair in lookup)
            {
                int dx = Mathf.Abs(pair.Key.x - center.x);
                int dz = Mathf.Abs(pair.Key.y - center.y);
                int distance = Mathf.Max(dx, dz);
                bool active = distance <= grid.activeRadiusCells;
                bool preloaded = distance <= grid.preloadRadiusCells;
                pair.Value.SetState(active, preloaded);
            }
        }
    }
}
