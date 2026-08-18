using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Presentation
{
    /// <summary>
    /// Applies checked-in Sunscar textures to runtime/cloud generated world geometry.
    /// The generator remains a fallback for geometry, while the shipped art is the visual source of truth.
    /// </summary>
    public sealed class FixedWorldArtRuntime : MonoBehaviour
    {
        private static readonly Dictionary<string, Material> Materials = new();
        private float nextScan;
        private float stopAt;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Match", StringComparison.OrdinalIgnoreCase)) return;
            if (FindFirstObjectByType<FixedWorldArtRuntime>() == null)
                new GameObject("Fsp_FixedWorldArtRuntime").AddComponent<FixedWorldArtRuntime>();
        }

        private void Awake()
        {
            stopAt = Time.unscaledTime + 18f;
            ApplyWorldArt();
        }

        private void Update()
        {
            if (Time.unscaledTime > stopAt)
            {
                enabled = false;
                return;
            }

            if (Time.unscaledTime < nextScan) return;
            nextScan = Time.unscaledTime + 0.8f;
            ApplyWorldArt();
        }

        private static void ApplyWorldArt()
        {
            Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null || renderer.GetComponentInParent<Fsp.BattleRoyale.MatchParticipant>() != null ||
                    renderer.GetComponentInParent<Fsp.Vehicles.SimpleVehicleController>() != null) continue;

                string hierarchyKey = BuildHierarchyKey(renderer.transform);
                string texture = ResolveTexture(hierarchyKey);
                if (texture == null) continue;

                Material material = GetMaterial(texture, renderer.sharedMaterial);
                if (material != null && renderer.sharedMaterial != material)
                    renderer.sharedMaterial = material;
            }
        }

        private static string BuildHierarchyKey(Transform start)
        {
            if (start == null) return string.Empty;
            System.Text.StringBuilder builder = new System.Text.StringBuilder(128);
            Transform current = start;
            int depth = 0;
            while (current != null && depth < 5)
            {
                if (builder.Length > 0) builder.Append(' ');
                builder.Append(current.name.ToLowerInvariant());
                current = current.parent;
                depth++;
            }
            return builder.ToString();
        }

        private static string ResolveTexture(string name)
        {
            if (ContainsAny(name, "road", "street", "runway", "airstrip", "path", "bridge")) return "World/road_dust";
            if (ContainsAny(name, "rock", "cliff", "mountain", "boulder", "ridge", "canyon")) return "World/rock_cliff";
            if (ContainsAny(name, "wall", "building", "house", "fort", "tower", "ruin", "warehouse", "port", "oldcrown", "copper")) return "World/fortress_wall";
            if (ContainsAny(name, "ground", "terrain", "sand", "dune", "desert", "dryfield", "sunscar", "coast", "plane")) return "World/sand_ground";
            return null;
        }

        private static bool ContainsAny(string value, params string[] terms)
        {
            foreach (string term in terms)
                if (value.Contains(term)) return true;
            return false;
        }

        private static Material GetMaterial(string resourcePath, Material template)
        {
            if (Materials.TryGetValue(resourcePath, out Material cached) && cached != null) return cached;

            Texture2D texture = Resources.Load<Texture2D>(resourcePath);
            if (texture == null) return null;
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Bilinear;

            Material material = template != null ? new Material(template) : CreateFallbackMaterial();
            if (material == null) return null;
            material.name = "FSP_" + resourcePath.Replace('/', '_');

            if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
            if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", Color.white);
            if (material.HasProperty("_Color")) material.SetColor("_Color", Color.white);

            Vector2 tiling = resourcePath.Contains("wall") ? new Vector2(3f, 3f) :
                resourcePath.Contains("road") ? new Vector2(7f, 7f) : new Vector2(10f, 10f);
            if (material.HasProperty("_BaseMap")) material.SetTextureScale("_BaseMap", tiling);
            if (material.HasProperty("_MainTex")) material.SetTextureScale("_MainTex", tiling);

            Materials[resourcePath] = material;
            return material;
        }

        private static Material CreateFallbackMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Unlit/Texture");
            return shader != null ? new Material(shader) : null;
        }
    }
}
