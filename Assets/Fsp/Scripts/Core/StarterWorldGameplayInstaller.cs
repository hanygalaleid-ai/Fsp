using Fsp.BattleRoyale;
using Fsp.Presentation;
using Fsp.World;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Core
{
    public static class StarterWorldGameplayInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install() => EnsureInstalled();

        public static void EnsureInstalled()
        {
            bool isMatchScene = SceneManager.GetActiveScene().name == "Match";
            if (!isMatchScene && Object.FindFirstObjectByType<MatchManager>() == null) return;

            if (Object.FindFirstObjectByType<OldCrownInteriorPrototype>() == null)
            {
                var g = new GameObject("OldCrown_Interiors");
                g.transform.position = new Vector3(-60f, 0f, 35f);
                g.AddComponent<OldCrownInteriorPrototype>();
            }
            if (Object.FindFirstObjectByType<OldCrownDoorInstaller>() == null) new GameObject("OldCrown_Doors").AddComponent<OldCrownDoorInstaller>();
            if (Object.FindFirstObjectByType<CopperPortPrototype>() == null) new GameObject("CopperPort_Prototype").AddComponent<CopperPortPrototype>();
            if (Object.FindFirstObjectByType<DryfieldPrototype>() == null) new GameObject("Dryfield_Prototype").AddComponent<DryfieldPrototype>();
            if (Object.FindFirstObjectByType<WhiteQuarryPrototype>() == null) new GameObject("WhiteQuarry_Prototype").AddComponent<WhiteQuarryPrototype>();
            if (Object.FindFirstObjectByType<RedlineAirstripPrototype>() == null) new GameObject("RedlineAirstrip_Prototype").AddComponent<RedlineAirstripPrototype>();
            if (Object.FindFirstObjectByType<SaltworksPrototype>() == null) new GameObject("Saltworks_Prototype").AddComponent<SaltworksPrototype>();
            if (Object.FindFirstObjectByType<LanternCoastPrototype>() == null) new GameObject("LanternCoast_Prototype").AddComponent<LanternCoastPrototype>();

            if (Object.FindFirstObjectByType<StarterPoiRoadLink>() == null) new GameObject("OldCrown_CopperPort_Road").AddComponent<StarterPoiRoadLink>();
            if (Object.FindFirstObjectByType<DryfieldRoadLinks>() == null) new GameObject("Dryfield_Road_Links").AddComponent<DryfieldRoadLinks>();
            if (Object.FindFirstObjectByType<QuarryRoadLink>() == null) new GameObject("WhiteQuarry_Road_Links").AddComponent<QuarryRoadLink>();
            if (Object.FindFirstObjectByType<AirstripRoadLink>() == null) new GameObject("RedlineAirstrip_Road_Links").AddComponent<AirstripRoadLink>();
            if (Object.FindFirstObjectByType<SaltworksRoadLink>() == null) new GameObject("Saltworks_Road_Links").AddComponent<SaltworksRoadLink>();
            if (Object.FindFirstObjectByType<LanternCoastRoadLink>() == null) new GameObject("LanternCoast_Road_Links").AddComponent<LanternCoastRoadLink>();

            if (Object.FindFirstObjectByType<SunscarIslandPolish>() == null) new GameObject("SunscarIsland_Polish").AddComponent<SunscarIslandPolish>();
            if (Object.FindFirstObjectByType<SunscarSkyBackdrop>() == null) new GameObject("Sunscar_SkyBackdrop").AddComponent<SunscarSkyBackdrop>();
            if (Object.FindFirstObjectByType<StarterVehicleDistribution>() == null) new GameObject("SunscarIsland_Vehicles").AddComponent<StarterVehicleDistribution>();
            if (Object.FindFirstObjectByType<MobileQualityTier>() == null) new GameObject("Fsp_MobileQualityTier").AddComponent<MobileQualityTier>();
            if (Object.FindFirstObjectByType<MobileWorldOptimizer>() == null) new GameObject("SunscarIsland_MobileOptimizer").AddComponent<MobileWorldOptimizer>();
            if (Object.FindFirstObjectByType<StarterSpawnBalance>() == null) new GameObject("SunscarIsland_SpawnBalance").AddComponent<StarterSpawnBalance>();
            if (Object.FindFirstObjectByType<PoiLootTierBalancer>() == null) new GameObject("SunscarIsland_LootBalance").AddComponent<PoiLootTierBalancer>();

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.62f, 0.67f, 0.74f);
            RenderSettings.ambientEquatorColor = new Color(0.52f, 0.45f, 0.36f);
            RenderSettings.ambientGroundColor = new Color(0.29f, 0.24f, 0.19f);
            RenderSettings.ambientIntensity = 1.28f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.60f, 0.65f, 0.68f);
            RenderSettings.fogStartDistance = 135f;
            RenderSettings.fogEndDistance = 360f;

            Camera camera = Camera.main != null ? Camera.main : Object.FindFirstObjectByType<Camera>();
            if (camera != null)
            {
                camera.clearFlags = CameraClearFlags.Skybox;
                camera.nearClipPlane = 0.08f;
                camera.farClipPlane = 520f;
                camera.allowHDR = PlayerPrefs.GetInt("fsp_quality", 1) >= 2;
                camera.allowMSAA = PlayerPrefs.GetInt("fsp_quality", 1) >= 1;
            }
        }
    }
}
