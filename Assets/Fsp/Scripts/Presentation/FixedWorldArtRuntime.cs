using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Fsp.Presentation
{
    /// <summary>
    /// Applies checked-in Sunscar textures to appropriate generated world geometry without flattening
    /// authored POI materials such as water, crops, metal roofs, salt, containers or loot.
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
            ConfigureMatchRendering();
            stopAt = Time.unscaledTime + 12f;
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
            ConfigureMatchRendering();
            ApplyWorldArt();
        }

        private static void ConfigureMatchRendering()
        {
            Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (Camera camera in cameras)
            {
                if (camera == null) continue;
                camera.allowHDR = false;
                camera.allowMSAA = true;
            }

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.30f, 0.27f, 0.23f, 1f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.47f, 0.52f, 0.55f, 1f);
            RenderSettings.fogStartDistance = 450f;
            RenderSettings.fogEndDistance = 1450f;

            Light sun = RenderSettings.sun;
            if (sun == null)
            {
                foreach (Light light in FindObjectsByType<Light>(FindObjectsSortMode.None))
                {
                    if (light != null && light.type == LightType.Directional) { sun = light; break; }
                }
            }
            if (sun == null)
            {
                GameObject sunObject = new GameObject("Sunscar_DirectionalLight");
                sun = sunObject.AddComponent<Light>();
                sun.type = LightType.Directional;
                sun.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
                RenderSettings.sun = sun;
            }
            sun.color = new Color(1f, 0.83f, 0.66f, 1f);
            sun.intensity = 0.85f;
            sun.shadows = LightShadows.Soft;
        }

        private static void ApplyWorldArt()
        {
            Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null ||
                    renderer.GetComponentInParent<Fsp.BattleRoyale.MatchParticipant>() != null ||
                    renderer.GetComponentInParent<Fsp.Vehicles.SimpleVehicleController>() != null ||
                    renderer.GetComponentInParent<Fsp.BattleRoyale.SafeZoneController>() != null)
                    continue;

                string leaf = renderer.gameObject.name.ToLowerInvariant();
                if (PreserveAuthoredMaterial(leaf)) continue;

                string texture = ResolveTexture(renderer.transform, leaf);
                if (texture == null && LooksLikeGenericFallbackGround(renderer, leaf))
                    texture = "World/sand_ground";
                if (texture == null) continue;

                Material material = GetMaterial(texture);
                if (material != null && renderer.sharedMaterial != material)
                    renderer.sharedMaterial = material;
            }
        }

        private static bool PreserveAuthoredMaterial(string leaf)
        {
            // These POI surfaces deliberately carry their own semantic colours/materials.
            return ContainsAny(leaf,
                "sea", "water", "ocean", "brine", "canal",
                "crop", "hay", "salt", "pipe", "container",
                "roof", "glass", "window", "loot", "mark",
                "lamp", "lantern", "boat", "pump");
        }

        private static string ResolveTexture(Transform transform, string leaf)
        {
            if (ContainsAny(leaf, "road", "street", "runway", "airstrip", "path", "bridge", "walkway"))
                return "World/road_dust";
            if (ContainsAny(leaf, "rock", "cliff", "mountain", "boulder", "ridge", "canyon", "quarry"))
                return "World/rock_cliff";
            if (ContainsAny(leaf, "wall", "fort", "ruin", "warehouse", "barrier", "post"))
                return "World/fortress_wall";
            if (ContainsAny(leaf, "ground", "terrain", "floor", "arena", "sand", "dune", "desert"))
                return "World/sand_ground";

            // Generic child names such as Body/Base can inherit only from an architectural parent,
            // never from a broad POI name like CopperPort/Dryfield/Coast.
            if (ContainsAny(leaf, "body", "base", "back", "left", "right", "frontl", "frontr"))
            {
                string parents = BuildParentKey(transform.parent, 4);
                if (ContainsAny(parents, "house", "barn", "hangar", "tower", "building", "warehouse", "farmhouse", "pumphouse"))
                    return "World/fortress_wall";
            }

            return null;
        }

        private static bool LooksLikeGenericFallbackGround(Renderer renderer, string leaf)
        {
            if (!ContainsAny(leaf, "cube", "plane", "arenafloor", "ground_base")) return false;
            Bounds b = renderer.bounds;
            float horizontal = Mathf.Max(b.size.x, b.size.z);
            bool veryLarge = horizontal >= 80f;
            bool flat = b.size.y <= Mathf.Max(8f, horizontal * 0.08f);
            bool low = b.center.y < 25f;
            return veryLarge && flat && low;
        }

        private static string BuildParentKey(Transform start, int maxDepth)
        {
            if (start == null) return string.Empty;
            System.Text.StringBuilder builder = new System.Text.StringBuilder(96);
            Transform current = start;
            int depth = 0;
            while (current != null && depth < maxDepth)
            {
                if (builder.Length > 0) builder.Append(' ');
                builder.Append(current.name.ToLowerInvariant());
                current = current.parent;
                depth++;
            }
            return builder.ToString();
        }

        private static bool ContainsAny(string value, params string[] terms)
        {
            foreach (string term in terms)
                if (value.Contains(term)) return true;
            return false;
        }

        private static Material GetMaterial(string resourcePath)
        {
            if (Materials.TryGetValue(resourcePath, out Material cached) && cached != null) return cached;

            Texture2D texture = Resources.Load<Texture2D>(resourcePath);
            if (texture == null)
            {
                Debug.LogError("FSP world art missing texture: " + resourcePath);
                return null;
            }
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Bilinear;

            Material material = CreateMobileMaterial();
            if (material == null) return null;
            material.name = "FSP_FIXED_" + resourcePath.Replace('/', '_');

            Color tint = ResolveTint(resourcePath);
            if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
            if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", tint);
            if (material.HasProperty("_Color")) material.SetColor("_Color", tint);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.18f);
            if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", 0.18f);

            Vector2 tiling = resourcePath.Contains("wall") ? new Vector2(3f, 3f) :
                resourcePath.Contains("road") ? new Vector2(7f, 7f) : new Vector2(12f, 12f);
            if (material.HasProperty("_BaseMap")) material.SetTextureScale("_BaseMap", tiling);
            if (material.HasProperty("_MainTex")) material.SetTextureScale("_MainTex", tiling);

            Materials[resourcePath] = material;
            return material;
        }

        private static Material CreateMobileMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Mobile/Diffuse");
            if (shader == null)
            {
                Debug.LogError("FSP could not find a supported mobile world shader.");
                return null;
            }
            return new Material(shader);
        }

        private static Color ResolveTint(string resourcePath)
        {
            if (resourcePath.Contains("sand")) return new Color(0.72f, 0.58f, 0.39f, 1f);
            if (resourcePath.Contains("road")) return new Color(0.52f, 0.43f, 0.33f, 1f);
            if (resourcePath.Contains("rock")) return new Color(0.47f, 0.39f, 0.33f, 1f);
            if (resourcePath.Contains("wall")) return new Color(0.58f, 0.45f, 0.34f, 1f);
            return Color.gray;
        }
    }
}
