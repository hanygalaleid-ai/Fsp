using System.Collections;
using UnityEngine;

namespace Fsp.World
{
    /// <summary>
    /// Adds cheap hinged doors after the procedural Old Crown houses have been generated.
    /// Kept separate from the geometry builder so the prototype remains easy to replace later.
    /// </summary>
    public sealed class OldCrownDoorInstaller : MonoBehaviour
    {
        private IEnumerator Start()
        {
            yield return null;
            InstallDoors();
        }

        public void InstallDoors()
        {
            OldCrownInteriorPrototype interiors = FindObjectOfType<OldCrownInteriorPrototype>();
            if (interiors == null) return;

            Transform generated = interiors.transform.Find("GeneratedInteriors");
            if (generated == null) return;

            Material doorMaterial = MakeMaterial(new Color(0.29f, 0.18f, 0.10f));
            for (int i = 0; i < generated.childCount; i++)
            {
                Transform house = generated.GetChild(i);
                if (house == null || house.Find("EntryDoorPivot") != null) continue;

                Transform pivot = new GameObject("EntryDoorPivot").transform;
                pivot.SetParent(house, false);
                pivot.localPosition = new Vector3(-0.675f, 0f, 3.08f);

                GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
                slab.name = "EntryDoor";
                slab.transform.SetParent(pivot, false);
                slab.transform.localPosition = new Vector3(0.675f, 1.15f, 0f);
                slab.transform.localScale = new Vector3(1.30f, 2.30f, 0.10f);
                Renderer renderer = slab.GetComponent<Renderer>();
                if (renderer != null) renderer.sharedMaterial = doorMaterial;

                pivot.gameObject.AddComponent<LightweightDoor>();
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
