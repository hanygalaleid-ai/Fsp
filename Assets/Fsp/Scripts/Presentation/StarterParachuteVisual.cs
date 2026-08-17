using Fsp.BattleRoyale;
using UnityEngine;

namespace Fsp.Presentation
{
    public sealed class StarterParachuteVisual : MonoBehaviour
    {
        [SerializeField] private float canopyHeight = 2.9f;
        private GameObject visualRoot;
        private ParachuteController parachute;

        public void Show(bool visible)
        {
            EnsureBuilt();
            if (visualRoot != null) visualRoot.SetActive(visible);
        }

        private void Awake()
        {
            parachute = GetComponent<ParachuteController>();
            EnsureBuilt();
            if (visualRoot != null) visualRoot.SetActive(false);
        }

        private void Update()
        {
            if (parachute == null) parachute = GetComponent<ParachuteController>();
            if (visualRoot == null) EnsureBuilt();
            if (visualRoot != null)
                visualRoot.SetActive(parachute != null && parachute.IsActive && parachute.IsOpen);
        }

        private void EnsureBuilt()
        {
            if (visualRoot != null) return;
            Transform existing = transform.Find("StarterParachuteVisual");
            if (existing != null) { visualRoot = existing.gameObject; return; }

            visualRoot = new GameObject("StarterParachuteVisual");
            visualRoot.transform.SetParent(transform, false);
            Material sand = MakeMaterial(new Color(0.66f, 0.57f, 0.39f));
            Material navy = MakeMaterial(new Color(0.08f, 0.12f, 0.17f));

            for (int i = -3; i <= 3; i++)
            {
                GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                cell.name = "CanopyCell";
                cell.transform.SetParent(visualRoot.transform, false);
                cell.transform.localPosition = new Vector3(i * 0.48f, canopyHeight + Mathf.Abs(i) * -0.08f, 0f);
                cell.transform.localScale = new Vector3(0.7f, 0.28f, 1.05f);
                Collider c = cell.GetComponent<Collider>(); if (c != null) Destroy(c);
                Renderer r = cell.GetComponent<Renderer>(); if (r != null) r.sharedMaterial = (i % 2 == 0) ? sand : navy;
            }

            for (int i = -1; i <= 1; i += 2)
            {
                GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                line.name = "SuspensionLine";
                line.transform.SetParent(visualRoot.transform, false);
                line.transform.localPosition = new Vector3(i * 0.85f, canopyHeight * 0.5f, 0f);
                line.transform.localScale = new Vector3(0.015f, canopyHeight * 0.52f, 0.015f);
                Collider c = line.GetComponent<Collider>(); if (c != null) Destroy(c);
                Renderer r = line.GetComponent<Renderer>(); if (r != null) r.sharedMaterial = navy;
            }
        }

        private static Material MakeMaterial(Color color)
        {
            Shader shader = Shader.Find("Standard");
            Material mat = new Material(shader != null ? shader : Shader.Find("Sprites/Default"));
            mat.color = color; return mat;
        }
    }
}
