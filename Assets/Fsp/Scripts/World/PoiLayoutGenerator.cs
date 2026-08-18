using System;
using System.Collections.Generic;
using Fsp.Backend;
using Fsp.Networking;
using UnityEngine;

namespace Fsp.World
{
    public sealed class PoiLayoutGenerator : MonoBehaviour
    {
        [Serializable]
        public sealed class SpawnSet
        {
            public GameObject[] buildingPrefabs;
            public GameObject[] coverPrefabs;
            public GameObject[] landmarkPrefabs;
        }

        [SerializeField] private PoiBlueprint blueprint;
        [SerializeField] private SpawnSet prefabs;
        [SerializeField] private Transform generatedRoot;
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private float buildingSpacing = 18f;
        [SerializeField] private float coverSpacing = 7f;

        private readonly List<GameObject> spawned = new();

        private void Start()
        {
            Generate(MatchRoomState.MatchId);
        }

        public void Generate(string matchId)
        {
            Clear();
            if (blueprint == null || generatedRoot == null) return;

            int seed = StableHash((matchId ?? "offline") + ":" + blueprint.poiId);
            var random = new System.Random(seed);

            SpawnGroup(prefabs?.landmarkPrefabs, blueprint.landmarkCount, blueprint.footprintRadius * 0.45f, 22f, random);
            SpawnGroup(prefabs?.buildingPrefabs, blueprint.buildingCount, blueprint.footprintRadius, buildingSpacing, random);
            SpawnGroup(prefabs?.coverPrefabs, blueprint.coverClusters, blueprint.footprintRadius, coverSpacing, random);
        }

        private void SpawnGroup(GameObject[] options, int count, float radius, float minSpacing, System.Random random)
        {
            if (options == null || options.Length == 0 || count <= 0) return;
            var accepted = new List<Vector3>(count);

            for (int i = 0; i < count; i++)
            {
                bool placed = false;
                for (int attempt = 0; attempt < 30 && !placed; attempt++)
                {
                    float angle = (float)(random.NextDouble() * Math.PI * 2.0);
                    float distance = Mathf.Sqrt((float)random.NextDouble()) * radius;
                    Vector3 local = new(Mathf.Cos(angle) * distance, 100f, Mathf.Sin(angle) * distance);
                    Vector3 probe = transform.TransformPoint(local);

                    if (!Physics.Raycast(probe, Vector3.down, out var hit, 240f, groundMask, QueryTriggerInteraction.Ignore)) continue;
                    Vector3 world = hit.point;
                    bool tooClose = false;
                    for (int p = 0; p < accepted.Count; p++)
                    {
                        if ((accepted[p] - world).sqrMagnitude < minSpacing * minSpacing) { tooClose = true; break; }
                    }
                    if (tooClose) continue;

                    GameObject prefab = options[random.Next(options.Length)];
                    if (prefab == null) continue;
                    float yaw = Mathf.Round((float)random.NextDouble() * 4f) * 90f;
                    GameObject instance = Instantiate(prefab, world, Quaternion.Euler(0f, yaw, 0f), generatedRoot);
                    spawned.Add(instance);
                    accepted.Add(world);
                    placed = true;
                }
            }
        }

        public void Clear()
        {
            for (int i = spawned.Count - 1; i >= 0; i--)
                if (spawned[i] != null) Destroy(spawned[i]);
            spawned.Clear();
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                int hash = 23;
                for (int i = 0; i < value.Length; i++) hash = hash * 31 + value[i];
                return hash;
            }
        }
    }
}
