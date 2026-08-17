using UnityEngine;

namespace Fsp.World
{
    [CreateAssetMenu(menuName = "Fsp/World/Road Network", fileName = "RoadNetwork")]
    public sealed class RoadNetworkDefinition : ScriptableObject
    {
        [System.Serializable]
        public sealed class RoadNode
        {
            public string id;
            public Vector3 position;
        }

        [System.Serializable]
        public sealed class RoadLink
        {
            public string fromId;
            public string toId;
            [Min(2f)] public float width = 7f;
            public bool vehiclePreferred = true;
        }

        public RoadNode[] nodes;
        public RoadLink[] links;
    }
}
