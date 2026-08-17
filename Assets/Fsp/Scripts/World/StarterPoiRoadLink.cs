using UnityEngine;

namespace Fsp.World
{
    /// <summary>
    /// Simple visible/playable road connecting Old Crown and Copper Port in the starter world.
    /// Uses a few long primitive segments to stay cheap on mobile.
    /// </summary>
    public sealed class StarterPoiRoadLink : MonoBehaviour
    {
        [SerializeField] private Vector3 start = new(-20f, 0.03f, 15f);
        [SerializeField] private Vector3 end = new(155f, 0.03f, -120f);
        [SerializeField] private float roadWidth = 8f;
        [SerializeField] private int segments = 7;

        private void Start()
        {
            BuildIfNeeded();
        }

        public void BuildIfNeeded()
        {
            if (transform.Find("GeneratedRoad") != null) return;
            var root = new GameObject("GeneratedRoad").transform;
            root.SetParent(transform, false);

            Material road = MakeMaterial(new Color(0.19f, 0.18f, 0.16f));
            Material edge = MakeMaterial(new Color(0.58f, 0.50f, 0.34f));

            Vector3 previous = start;
            for (int i = 0; i < Mathf.Max(2, segments); i++)
            {
                float t = (i + 1f) / Mathf.Max(2, segments);
                Vector3 target = Vector3.Lerp(start, end, t);
                float curve = Mathf.Sin(t * Mathf.PI) * 14f;
                Vector3 direction2D = end - start;
                Vector3 side = Vector3.Cross(Vector3.up, direction2D.normalized);
                target += side * curve;
                BuildSegment(root, previous, target, road, edge, i);
                previous = target;
            }
        }

        private void BuildSegment(Transform root, Vector3 a, Vector3 b, Material road, Material edge, int index)
        {
            Vector3 mid = (a + b) * 0.5f;
            Vector3 delta = b - a;
            float length = delta.magnitude;
            Quaternion rotation = delta.sqrMagnitude > 0.001f ? Quaternion.LookRotation(delta.normalized, Vector3.up) : Quaternion.identity;

            GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segment.name = $"Road_{index:00}";
            segment.transform.SetParent(root, false);
            segment.transform.position = mid;
            segment.transform.rotation = rotation;
            segment.transform.localScale = new Vector3(roadWidth, 0.08f, length + 0.6f);
            Renderer r = segment.GetComponent<Renderer>(); if (r != null) r.sharedMaterial = road;

            for (int sideIndex = -1; sideIndex <= 1; sideIndex += 2)
            {
                GameObject shoulder = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shoulder.name = "Shoulder";
                shoulder.transform.SetParent(root, false);
                shoulder.transform.position = mid + rotation * Vector3.right * (sideIndex * (roadWidth * 0.5f + 0.6f));
                shoulder.transform.rotation = rotation;
                shoulder.transform.localScale = new Vector3(1.0f, 0.05f, length + 0.6f);
                Renderer sr = shoulder.GetComponent<Renderer>(); if (sr != null) sr.sharedMaterial = edge;
            }
        }

        private static Material MakeMaterial(Color color)
        {
            Shader shader = Shader.Find("Standard");
            Material mat = new Material(shader != null ? shader : Shader.Find("Sprites/Default"));
            mat.color = color;
            return mat;
        }
    }
}
