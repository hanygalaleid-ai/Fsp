using Fsp.World;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Presentation
{
    /// <summary>
    /// Adds cheap visual detail to modular/generated Sunscar structures without changing gameplay colliders.
    /// </summary>
    public sealed class FixedBuildingPolishRuntime : MonoBehaviour
    {
        private float nextScan;
        private float stopAt;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Match", System.StringComparison.OrdinalIgnoreCase)) return;
            if (FindFirstObjectByType<FixedBuildingPolishRuntime>() == null)
                new GameObject("Fsp_FixedBuildingPolishRuntime").AddComponent<FixedBuildingPolishRuntime>();
        }

        private void Awake()
        {
            stopAt = Time.unscaledTime + 14f;
            Polish();
        }

        private void Update()
        {
            if (Time.unscaledTime > stopAt) { enabled = false; return; }
            if (Time.unscaledTime < nextScan) return;
            nextScan = Time.unscaledTime + 1.5f;
            Polish();
        }

        private static void Polish()
        {
            ModularBuildingPiece[] pieces = FindObjectsByType<ModularBuildingPiece>(FindObjectsSortMode.None);
            foreach (ModularBuildingPiece piece in pieces)
            {
                if (piece == null || piece.transform.Find("Fsp_VisualTrim") != null) continue;
                AddTrim(piece);
            }
        }

        private static void AddTrim(ModularBuildingPiece piece)
        {
            Transform root = new GameObject("Fsp_VisualTrim").transform;
            root.SetParent(piece.transform, false);

            switch (piece.PieceType)
            {
                case BuildingPieceType.Wall:
                case BuildingPieceType.WallWindow:
                case BuildingPieceType.WallDoor:
                case BuildingPieceType.Corner:
                    AddCube(root, "WallTrimTop", new Vector3(0f, 0.48f, 0f), new Vector3(1.04f, 0.08f, 1.04f));
                    AddCube(root, "WallTrimBase", new Vector3(0f, -0.48f, 0f), new Vector3(1.03f, 0.07f, 1.03f));
                    break;
                case BuildingPieceType.Roof:
                    AddCube(root, "RoofCap", new Vector3(0f, 0.08f, 0f), new Vector3(1.06f, 0.12f, 1.06f));
                    break;
                case BuildingPieceType.Balcony:
                    AddCube(root, "BalconyRail", new Vector3(0f, 0.42f, 0.46f), new Vector3(1f, 0.08f, 0.08f));
                    break;
            }
        }

        private static void AddCube(Transform parent, string name, Vector3 localPosition, Vector3 localScale)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            Collider collider = go.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }
        }
    }
}
