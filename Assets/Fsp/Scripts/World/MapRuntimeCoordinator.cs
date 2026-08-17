using System;
using Fsp.Backend;
using Fsp.BattleRoyale;
using UnityEngine;

namespace Fsp.World
{
    public sealed class MapRuntimeCoordinator : MonoBehaviour
    {
        [SerializeField] private MapDefinition map;
        [SerializeField] private DropPlaneController plane;
        [SerializeField] private Transform routeStart;
        [SerializeField] private Transform routeEnd;

        public MapPoiDefinition[] Pois => map != null ? map.pois : Array.Empty<MapPoiDefinition>();
        public SafeZonePreset SelectedZone { get; private set; }
        public FlightPathPreset SelectedFlightPath { get; private set; }

        private void Awake()
        {
            if (map == null) return;
            int seed = StableSeed(MatchRoomState.MatchId, map.mapId);
            var rng = new System.Random(seed);

            if (map.safeZonePresets != null && map.safeZonePresets.Length > 0)
                SelectedZone = map.safeZonePresets[rng.Next(map.safeZonePresets.Length)];

            if (map.flightPaths != null && map.flightPaths.Length > 0)
                SelectedFlightPath = map.flightPaths[rng.Next(map.flightPaths.Length)];

            ApplyFlightPath();
        }

        private void ApplyFlightPath()
        {
            if (SelectedFlightPath == null || routeStart == null || routeEnd == null) return;
            Vector3 start = SelectedFlightPath.start;
            Vector3 end = SelectedFlightPath.end;
            start.y = SelectedFlightPath.altitude;
            end.y = SelectedFlightPath.altitude;
            routeStart.position = start;
            routeEnd.position = end;
            if (plane != null) plane.BeginRoute();
        }

        public MapPoiDefinition FindPoi(Vector3 worldPosition)
        {
            if (map?.pois == null) return null;
            MapPoiDefinition best = null;
            float bestSqr = float.MaxValue;
            foreach (var poi in map.pois)
            {
                if (poi == null) continue;
                float sqr = (poi.center - worldPosition).sqrMagnitude;
                if (sqr <= poi.radius * poi.radius && sqr < bestSqr)
                {
                    best = poi;
                    bestSqr = sqr;
                }
            }
            return best;
        }

        private static int StableSeed(string a, string b)
        {
            unchecked
            {
                int hash = 17;
                string value = (a ?? string.Empty) + "|" + (b ?? string.Empty);
                for (int i = 0; i < value.Length; i++) hash = hash * 31 + value[i];
                return hash;
            }
        }
    }
}
