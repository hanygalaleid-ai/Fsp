using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Core
{
    /// <summary>Replaces stripped/unsupported runtime materials before they render magenta on Android.</summary>
    public sealed class AndroidMaterialRecovery : MonoBehaviour
    {
        private Shader safeShader;
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
                    if (source != null)
                    {
                        if (source.HasProperty("_Color")) replacement.color = source.color;
                        if (source.HasProperty("_MainTex")) replacement.mainTexture = source.mainTexture;
                    }
                    materials[i] = replacement;
                    changed = true;
                }
                if (changed) renderer.sharedMaterials = materials;
            }
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
