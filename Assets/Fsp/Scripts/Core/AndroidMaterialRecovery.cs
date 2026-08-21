using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Core
{
    /// <summary>Replaces stripped/unsupported runtime materials before they render magenta on Android.</summary>
    public sealed class AndroidMaterialRecovery : MonoBehaviour
    {
        private Shader safeShader;
        private Texture2D sandTexture;
        private Texture2D roadTexture;
        private Texture2D rockTexture;
        private Texture2D wallTexture;
        private Texture2D woodTexture;
        private float nextScan;
        private float stopAt;

        public static void EnsureInstalled()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Match", StringComparison.OrdinalIgnoreCase)) return;
            if (FindFirstObjectByType<AndroidMaterialRecovery>() != null) return;
            new GameObject("Fsp_AndroidMaterialRecovery").AddComponent<AndroidMaterialRecovery>();
        }

        private void Awake()
        {
            safeShader = Resources.Load<Shader>("Shaders/FspMobileSafe");
            if (safeShader == null) safeShader = Shader.Find("Fsp/MobileSafeLit");
            sandTexture = Resources.Load<Texture2D>("World/bmg_desert_ground_v3");
            roadTexture = Resources.Load<Texture2D>("World/road_dust_v2");
            rockTexture = Resources.Load<Texture2D>("World/rock_cliff_v2");
            wallTexture = Resources.Load<Texture2D>("World/bmg_fortress_wall_v3");
            woodTexture = Resources.Load<Texture2D>("World/bmg_wood_floor_v3");
            ConfigureTexture(sandTexture);
            ConfigureTexture(roadTexture);
            ConfigureTexture(rockTexture);
            ConfigureTexture(wallTexture);
            ConfigureTexture(woodTexture);
            stopAt = Time.unscaledTime + 30f;
            RepairAllRenderers();
        }

        private void Update()
        {
            if (Time.unscaledTime > stopAt)
            {
                Destroy(this);
                return;
            }
            if (Time.unscaledTime < nextScan) return;
            nextScan = Time.unscaledTime + 0.5f;
            RepairAllRenderers();
        }

        private void RepairAllRenderers()
        {
            if (safeShader == null) return;
            foreach (Renderer renderer in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if (renderer == null) continue;
                if (renderer is SpriteRenderer || renderer is ParticleSystemRenderer) continue;
                Material[] materials = renderer.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < materials.Length; i++)
                {
                    Material source = materials[i];
                    if (!NeedsRepair(source)) continue;
                    Material replacement = new Material(safeShader) { name = "FSP_AndroidSafeMaterial" };
                    Color sourceColor = Color.white;
                    if (source != null)
                    {
                        if (source.HasProperty("_Color")) sourceColor = source.color;
                        if (source.HasProperty("_MainTex")) replacement.mainTexture = source.mainTexture;
                        if (replacement.mainTexture == null && source.HasProperty("_BaseMap"))
                            replacement.mainTexture = source.GetTexture("_BaseMap");
                    }
                    replacement.color = SafeSourceColor(sourceColor, renderer);
                    Texture2D worldTexture = TextureFor(renderer);
                    if (worldTexture != null)
                    {
                        replacement.mainTexture = worldTexture;
                        replacement.color = Color.white;
                        replacement.mainTextureScale = ScaleFor(renderer);
                    }
                    else if (source != null && source.HasProperty("_MainTex"))
                    {
                        replacement.mainTextureScale = source.mainTextureScale;
                        replacement.mainTextureOffset = source.mainTextureOffset;
                    }
                    materials[i] = replacement;
                    changed = true;
                }
                if (changed) renderer.sharedMaterials = materials;
            }
        }

        private Texture2D TextureFor(Renderer renderer)
        {
            string n = Descriptor(renderer);
            if (n.Contains("road") || n.Contains("runway") || n.Contains("asphalt")) return roadTexture;
            if (n.Contains("rock") || n.Contains("ridge") || n.Contains("quarry") || n.Contains("stone") || n.Contains("cliff")) return rockTexture;
            if (n.Contains("wall") || n.Contains("fort") || n.Contains("hangar") || n.Contains("building") || n.Contains("sign")) return wallTexture;
            if (n.Contains("floor") || n.Contains("table") || n.Contains("crate")) return woodTexture;
            if (n.Contains("ground") || n.Contains("island") || n.Contains("sand") || IsLargeHorizontalSurface(renderer)) return sandTexture;
            return null;
        }

        private static Vector2 ScaleFor(Renderer renderer)
        {
            string n = Descriptor(renderer);
            Vector3 size = renderer != null ? renderer.bounds.size : Vector3.one;
            if (n.Contains("ground") || n.Contains("island") || IsLargeHorizontalSurface(renderer))
                return new Vector2(Mathf.Clamp(size.x / 5f, 4f, 48f), Mathf.Clamp(size.z / 5f, 4f, 48f));
            if (n.Contains("road") || n.Contains("runway"))
                return new Vector2(Mathf.Clamp(size.x / 4f, 1f, 12f), Mathf.Clamp(size.z / 4f, 2f, 30f));
            if (n.Contains("floor"))
                return new Vector2(Mathf.Clamp(size.x / 2.2f, 1f, 12f), Mathf.Clamp(size.z / 2.2f, 1f, 12f));
            if (n.Contains("wall") || n.Contains("building") || n.Contains("fort"))
                return new Vector2(Mathf.Clamp(Mathf.Max(size.x, size.z) / 2.2f, 1f, 12f), Mathf.Clamp(size.y / 2.2f, 1f, 8f));
            return new Vector2(2f, 2f);
        }

        private static void ConfigureTexture(Texture2D texture)
        {
            if (texture == null) return;
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Bilinear;
            texture.anisoLevel = 2;
        }

        private static Color SafeSourceColor(Color source, Renderer renderer)
        {
            string n = Descriptor(renderer);
            if (n.Contains("ocean") || n.Contains("water") || n.Contains("sea")) return new Color(.12f, .34f, .44f, 1f);
            if (n.Contains("tree") || n.Contains("crown") || n.Contains("grass") || n.Contains("plant")) return new Color(.22f, .34f, .18f, 1f);
            if (n.Contains("road") || n.Contains("runway") || n.Contains("asphalt")) return new Color(.20f, .18f, .15f, 1f);
            if (n.Contains("rock") || n.Contains("ridge") || n.Contains("quarry") || n.Contains("stone") || n.Contains("cliff")) return new Color(.34f, .30f, .25f, 1f);
            if (n.Contains("wall") || n.Contains("fort") || n.Contains("hangar") || n.Contains("building") || n.Contains("sign")) return new Color(.43f, .34f, .23f, 1f);
            if (n.Contains("ground") || n.Contains("island") || n.Contains("sand") || n.Contains("floor") || IsLargeHorizontalSurface(renderer)) return new Color(.52f, .42f, .27f, 1f);

            bool nearWhite = source.r > .92f && source.g > .92f && source.b > .92f;
            bool magenta = source.r > .75f && source.b > .75f && source.g < .35f;
            if (!nearWhite && !magenta && source.a > .01f) return source;
            return new Color(.32f, .30f, .27f, 1f);
        }

        private static string Descriptor(Renderer renderer)
        {
            if (renderer == null) return string.Empty;
            System.Text.StringBuilder value = new System.Text.StringBuilder(96);
            Transform current = renderer.transform;
            int depth = 0;
            while (current != null && depth++ < 8)
            {
                value.Append(current.name).Append(' ');
                current = current.parent;
            }
            return value.ToString().ToLowerInvariant();
        }

        private static bool IsLargeHorizontalSurface(Renderer renderer)
        {
            if (renderer == null) return false;
            Vector3 size = renderer.bounds.size;
            return size.x >= 40f && size.z >= 40f && size.y <= Mathf.Max(12f, Mathf.Min(size.x, size.z) * .2f);
        }

        private static bool NeedsRepair(Material material)
        {
            if (material == null || material.shader == null || !material.shader.isSupported) return true;
            string shaderName = material.shader.name ?? string.Empty;
            if (string.Equals(shaderName, "Fsp/MobileSafeLit", StringComparison.Ordinal)) return false;

#if UNITY_ANDROID && !UNITY_EDITOR
            // Runtime-created Standard/URP materials can report isSupported=true after shader
            // stripping while still rendering with Unity's magenta error pass on the device.
            // All world mesh materials use the checked-in mobile-safe shader in Android players.
            return true;
#else
            return shaderName.IndexOf("InternalErrorShader", StringComparison.OrdinalIgnoreCase) >= 0;
#endif
        }
    }
}
