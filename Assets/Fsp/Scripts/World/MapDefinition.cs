using System;
using UnityEngine;

namespace Fsp.World
{
    [CreateAssetMenu(menuName = "Fsp/World/Map Definition", fileName = "MapDefinition")]
    public sealed class MapDefinition : ScriptableObject
    {
        public string mapId = "sunscar_island";
        public string displayName = "Sunscar Island";
        public Vector2 worldSize = new(2400f, 2400f);
        public MapPoiDefinition[] pois;
        public SafeZonePreset[] safeZonePresets;
        public FlightPathPreset[] flightPaths;
    }

    [Serializable]
    public sealed class MapPoiDefinition
    {
        public string poiId;
        public string displayName;
        public Vector3 center;
        [Min(40f)] public float radius = 120f;
        [Range(0.1f, 3f)] public float lootDensity = 1f;
        [Range(0f, 1f)] public float highTierLootChance = 0.15f;
        [Range(0, 12)] public int vehicleSpawnBudget = 2;
        public bool hotDrop;
    }

    [Serializable]
    public sealed class SafeZonePreset
    {
        public string presetId;
        public Vector3 center;
        [Min(100f)] public float initialRadius = 950f;
        [Min(20f)] public float finalRadius = 40f;
    }

    [Serializable]
    public sealed class FlightPathPreset
    {
        public string presetId;
        public Vector3 start;
        public Vector3 end;
        [Min(80f)] public float altitude = 220f;
    }
}
