using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.World
{
    /// <summary>Deterministic mobile daylight, ambient light and distance fog for the Sunscar match.</summary>
    public static class MatchLightingRuntime
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Match", StringComparison.OrdinalIgnoreCase)) return;

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.54f, 0.59f, 0.66f);
            RenderSettings.ambientEquatorColor = new Color(0.42f, 0.36f, 0.29f);
            RenderSettings.ambientGroundColor = new Color(0.18f, 0.15f, 0.12f);
            RenderSettings.ambientIntensity = 1.05f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.60f, 0.65f, 0.68f);
            RenderSettings.fogStartDistance = 135f;
            RenderSettings.fogEndDistance = 360f;

            Light sun = RenderSettings.sun;
            if (sun == null)
            {
                GameObject sunObject = new("SunscarSun");
                sun = sunObject.AddComponent<Light>();
                sun.type = LightType.Directional;
                RenderSettings.sun = sun;
            }
            sun.transform.rotation = Quaternion.Euler(38f, -32f, 0f);
            sun.color = new Color(1f, 0.86f, 0.68f);
            sun.intensity = 1.15f;
            sun.shadows = PlayerPrefs.GetInt("fsp_quality", 1) == 0 ? LightShadows.None : LightShadows.Hard;

            Camera camera = Camera.main;
            if (camera != null)
            {
                camera.clearFlags = CameraClearFlags.Skybox;
                camera.farClipPlane = 420f;
                camera.allowHDR = PlayerPrefs.GetInt("fsp_quality", 1) >= 2;
                camera.allowMSAA = PlayerPrefs.GetInt("fsp_quality", 1) >= 1;
            }
        }
    }
}
