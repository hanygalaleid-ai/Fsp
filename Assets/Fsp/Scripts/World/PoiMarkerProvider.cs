using System;
using UnityEngine;

namespace Fsp.World
{
    public sealed class PoiMarkerProvider : MonoBehaviour
    {
        [Serializable]
        public struct PoiMarker
        {
            public string id;
            public string label;
            public Vector3 worldPosition;
            public bool hotDrop;
        }

        [SerializeField] private MapRuntimeCoordinator mapRuntime;

        public PoiMarker[] BuildMarkers()
        {
            if (mapRuntime == null || mapRuntime.Pois == null) return Array.Empty<PoiMarker>();
            var pois = mapRuntime.Pois;
            var result = new PoiMarker[pois.Length];
            for (int i = 0; i < pois.Length; i++)
            {
                var poi = pois[i];
                if (poi == null) continue;
                result[i] = new PoiMarker
                {
                    id = poi.poiId,
                    label = poi.displayName,
                    worldPosition = poi.center,
                    hotDrop = poi.hotDrop
                };
            }
            return result;
        }
    }
}
