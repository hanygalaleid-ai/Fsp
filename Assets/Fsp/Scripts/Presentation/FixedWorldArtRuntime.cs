using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Fsp.Presentation
{
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

        private void Awake() { ConfigureMatchRendering(); stopAt = Time.unscaledTime + 20f; ApplyWorldArt(); }
        private void Update()
        {
            if (Time.unscaledTime > stopAt) { enabled = false; return; }
            if (Time.unscaledTime < nextScan) return;
            nextScan = Time.unscaledTime + 0.5f;
            ConfigureMatchRendering();
            ApplyWorldArt();
        }

        private static void ConfigureMatchRendering()
        {
            foreach (Camera camera in FindObjectsByType<Camera>(FindObjectsSortMode.None))
            {
                if (camera == null) continue;
                camera.allowHDR = false;
                camera.allowMSAA = true;
            }
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.22f, 0.20f, 0.18f, 1f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.40f, 0.48f, 0.52f, 1f);
            RenderSettings.fogStartDistance = 500f;
            RenderSettings.fogEndDistance = 1500f;

            Light sun = RenderSettings.sun;
            if (sun == null)
            {
                foreach (Light light in FindObjectsByType<Light>(FindObjectsSortMode.None))
                    if (light != null && light.type == LightType.Directional) { sun = light; break; }
            }
            if (sun == null)
            {
                GameObject go = new GameObject("Sunscar_DirectionalLight");
                sun = go.AddComponent<Light>();
                sun.type = LightType.Directional;
                RenderSettings.sun = sun;
            }
            sun.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
            sun.color = new Color(1f, 0.80f, 0.62f, 1f);
            sun.intensity = 0.65f;
            sun.shadows = LightShadows.Soft;
        }

        private static void ApplyWorldArt()
        {
            foreach (Renderer renderer in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if (renderer == null || renderer.GetComponentInParent<Fsp.BattleRoyale.MatchParticipant>() != null ||
                    renderer.GetComponentInParent<Fsp.Vehicles.SimpleVehicleController>() != null ||
                    renderer.GetComponentInParent<Fsp.BattleRoyale.SafeZoneController>() != null) continue;

                string leaf = renderer.gameObject.name.ToLowerInvariant();
                if (PreserveAuthoredMaterial(leaf)) continue;
                string texture = ResolveTexture(renderer.transform, leaf);
                if (texture == null && LooksLikeGenericFallbackGround(renderer, leaf)) texture = "World/sand_ground";
                if (texture == null) continue;
                Material material = GetMaterial(texture);
                if (material != null && renderer.sharedMaterial != material) renderer.sharedMaterial = material;
            }
        }

        private static bool PreserveAuthoredMaterial(string leaf) => ContainsAny(leaf, "sea", "water", "ocean", "brine", "canal", "crop", "hay", "salt", "pipe", "container", "roof", "glass", "window", "loot", "mark", "lamp", "lantern", "boat", "pump");

        private static string ResolveTexture(Transform transform, string leaf)
        {
            if (ContainsAny(leaf, "road", "street", "runway", "airstrip", "path", "bridge", "walkway")) return "World/road_dust";
            if (ContainsAny(leaf, "rock", "cliff", "mountain", "boulder", "ridge", "canyon", "quarry")) return "World/rock_cliff";
            if (ContainsAny(leaf, "wall", "fort", "ruin", "warehouse", "barrier", "post")) return "World/fortress_wall";
            if (ContainsAny(leaf, "ground", "terrain", "floor", "arena", "sand", "dune", "desert")) return "World/sand_ground";
            if (ContainsAny(leaf, "body", "base", "back", "left", "right", "frontl", "frontr"))
            {
                string parents = BuildParentKey(transform.parent, 4);
                if (ContainsAny(parents, "house", "barn", "hangar", "tower", "building", "warehouse", "farmhouse", "pumphouse")) return "World/fortress_wall";
            }
            return null;
        }

        private static bool LooksLikeGenericFallbackGround(Renderer renderer, string leaf)
        {
            if (!ContainsAny(leaf, "cube", "plane", "arenafloor", "ground_base")) return false;
            Bounds b = renderer.bounds;
            float horizontal = Mathf.Max(b.size.x, b.size.z);
            return horizontal >= 80f && b.size.y <= Mathf.Max(8f, horizontal * 0.08f) && b.center.y < 25f;
        }

        private static string BuildParentKey(Transform start, int maxDepth)
        {
            if (start == null) return string.Empty;
            System.Text.StringBuilder b = new System.Text.StringBuilder(96);
            Transform current = start;
            for (int depth = 0; current != null && depth < maxDepth; depth++, current = current.parent)
            { if (b.Length > 0) b.Append(' '); b.Append(current.name.ToLowerInvariant()); }
            return b.ToString();
        }

        private static bool ContainsAny(string value, params string[] terms)
        { foreach (string term in terms) if (value.Contains(term)) return true; return false; }

        private static Material GetMaterial(string resourcePath)
        {
            if (Materials.TryGetValue(resourcePath, out Material cached) && cached != null) return cached;
            Texture2D texture = Resources.Load<Texture2D>(resourcePath);
            Material material = CreateMobileMaterial();
            if (material == null) return null;
            material.name = "FSP_FIXED_" + resourcePath.Replace('/', '_');
            Color tint = ResolveTint(resourcePath);

            // Tint is mandatory, not optional: even if Android fails to import a checked-in texture,
            // the map can never fall back to Unity's blinding default-white material again.
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", tint);
            if (material.HasProperty("_Color")) material.SetColor("_Color", tint);
            if (texture != null)
            {
                texture.wrapMode = TextureWrapMode.Repeat;
                texture.filterMode = FilterMode.Bilinear;
                if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
                if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
                Vector2 tiling = resourcePath.Contains("wall") ? new Vector2(3f, 3f) : resourcePath.Contains("road") ? new Vector2(7f, 7f) : new Vector2(12f, 12f);
                if (material.HasProperty("_BaseMap")) material.SetTextureScale("_BaseMap", tiling);
                if (material.HasProperty("_MainTex")) material.SetTextureScale("_MainTex", tiling);
            }
            else Debug.LogWarning("FSP texture missing at runtime; using safe tinted fallback: " + resourcePath);

            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.08f);
            if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", 0.08f);
            Materials[resourcePath] = material;
            return material;
        }

        private static Material CreateMobileMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Mobile/Diffuse");
            return shader != null ? new Material(shader) : null;
        }

        private static Color ResolveTint(string p)
        {
            if (p.Contains("sand")) return new Color(0.48f, 0.32f, 0.17f, 1f);
            if (p.Contains("road")) return new Color(0.25f, 0.20f, 0.16f, 1f);
            if (p.Contains("rock")) return new Color(0.30f, 0.25f, 0.22f, 1f);
            if (p.Contains("wall")) return new Color(0.38f, 0.28f, 0.20f, 1f);
            return new Color(0.35f, 0.30f, 0.25f, 1f);
        }
    }
}