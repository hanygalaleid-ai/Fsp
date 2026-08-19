using Fsp.Bots;
using UnityEngine;

namespace Fsp.World
{
    public sealed class StarterSpawnBalance : MonoBehaviour
    {
        private static readonly Vector3[] Anchors =
        {
            new(-78, 1, 20), new(52, 1, -38), new(92, 1, 78), new(-110, 1, -66),
            new(132, 1, -95), new(-8, 1, 138), new(-132, 1, 82), new(0, 1, 0)
        };

        private void Start()
        {
            BotSpawner spawner = Object.FindFirstObjectByType<BotSpawner>();
            if (spawner == null) return;

            Transform root = new GameObject("BalancedBotSpawns").transform;
            root.SetParent(transform, false);

            Transform[] points = new Transform[Anchors.Length];
            for (int i = 0; i < Anchors.Length; i++)
            {
                GameObject point = new GameObject("BotSpawn_" + i);
                point.transform.SetParent(root, false);
                point.transform.position = Anchors[i];
                points[i] = point.transform;
            }

            spawner.ConfigureSpawnPoints(points);
        }
    }
}
