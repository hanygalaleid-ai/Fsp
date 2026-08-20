using UnityEngine;

namespace Fsp.Core
{
    /// <summary>Applies a safe graphics preset before the first scene renders on Android.</summary>
    public static class DeviceGraphicsConfigurator
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Apply()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            int saved = PlayerPrefs.GetInt("fsp_quality", -1);
            int preset = saved >= 0 ? Mathf.Clamp(saved, 0, 2) : DetectPreset();
            ApplyPreset(preset);
        }

        public static void ApplyPreset(int preset)
        {
            preset = Mathf.Clamp(preset, 0, 2);
            PlayerPrefs.SetInt("fsp_quality", preset);
            int qualityIndex = Mathf.Clamp(preset, 0, Mathf.Max(0, QualitySettings.names.Length - 1));
            if (QualitySettings.names.Length > 0) QualitySettings.SetQualityLevel(qualityIndex, true);
            QualitySettings.vSyncCount = 0;

            if (preset == 0)
            {
                Application.targetFrameRate = 30;
                QualitySettings.antiAliasing = 0;
                QualitySettings.shadows = ShadowQuality.Disable;
                QualitySettings.shadowDistance = 20f;
                QualitySettings.lodBias = 0.65f;
                QualitySettings.globalTextureMipmapLimit = 1;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                QualitySettings.realtimeReflectionProbes = false;
                QualitySettings.softParticles = false;
                ScalableBufferManager.ResizeBuffers(0.72f, 0.72f);
            }
            else if (preset == 1)
            {
                Application.targetFrameRate = 45;
                QualitySettings.antiAliasing = 2;
                QualitySettings.shadows = ShadowQuality.HardOnly;
                QualitySettings.shadowDistance = 45f;
                QualitySettings.lodBias = 1f;
                QualitySettings.globalTextureMipmapLimit = 0;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
                QualitySettings.realtimeReflectionProbes = false;
                QualitySettings.softParticles = false;
                ScalableBufferManager.ResizeBuffers(0.86f, 0.86f);
            }
            else
            {
                Application.targetFrameRate = 60;
                QualitySettings.antiAliasing = 4;
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.shadowDistance = 75f;
                QualitySettings.lodBias = 1.35f;
                QualitySettings.globalTextureMipmapLimit = 0;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
                QualitySettings.realtimeReflectionProbes = true;
                QualitySettings.softParticles = true;
                ScalableBufferManager.ResizeBuffers(1f, 1f);
            }
        }

        private static int DetectPreset()
        {
            int memory = SystemInfo.systemMemorySize;
            int graphicsMemory = SystemInfo.graphicsMemorySize;
            int cores = SystemInfo.processorCount;
            if (memory > 0 && (memory < 3500 || graphicsMemory > 0 && graphicsMemory < 1000 || cores <= 4)) return 0;
            if (memory >= 7500 && (graphicsMemory <= 0 || graphicsMemory >= 2500) && cores >= 8) return 2;
            return 1;
        }
    }
}
