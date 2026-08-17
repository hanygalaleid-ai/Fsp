using Fsp.World;
using UnityEngine;
namespace Fsp.Core{public static class StarterWorldGameplayInstaller{[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Install(){
if(Object.FindObjectOfType<OldCrownInteriorPrototype>()==null){var g=new GameObject("OldCrown_Interiors");g.transform.position=new Vector3(-60,0,35);g.AddComponent<OldCrownInteriorPrototype>();}
if(Object.FindObjectOfType<OldCrownDoorInstaller>()==null)new GameObject("OldCrown_Doors").AddComponent<OldCrownDoorInstaller>();
if(Object.FindObjectOfType<CopperPortPrototype>()==null)new GameObject("CopperPort_Prototype").AddComponent<CopperPortPrototype>();
if(Object.FindObjectOfType<DryfieldPrototype>()==null)new GameObject("Dryfield_Prototype").AddComponent<DryfieldPrototype>();
if(Object.FindObjectOfType<WhiteQuarryPrototype>()==null)new GameObject("WhiteQuarry_Prototype").AddComponent<WhiteQuarryPrototype>();
if(Object.FindObjectOfType<RedlineAirstripPrototype>()==null)new GameObject("RedlineAirstrip_Prototype").AddComponent<RedlineAirstripPrototype>();
if(Object.FindObjectOfType<SaltworksPrototype>()==null)new GameObject("Saltworks_Prototype").AddComponent<SaltworksPrototype>();
if(Object.FindObjectOfType<LanternCoastPrototype>()==null)new GameObject("LanternCoast_Prototype").AddComponent<LanternCoastPrototype>();
if(Object.FindObjectOfType<StarterPoiRoadLink>()==null)new GameObject("OldCrown_CopperPort_Road").AddComponent<StarterPoiRoadLink>();
if(Object.FindObjectOfType<DryfieldRoadLinks>()==null)new GameObject("Dryfield_Road_Links").AddComponent<DryfieldRoadLinks>();
if(Object.FindObjectOfType<QuarryRoadLink>()==null)new GameObject("WhiteQuarry_Road_Links").AddComponent<QuarryRoadLink>();
if(Object.FindObjectOfType<AirstripRoadLink>()==null)new GameObject("RedlineAirstrip_Road_Links").AddComponent<AirstripRoadLink>();
if(Object.FindObjectOfType<SaltworksRoadLink>()==null)new GameObject("Saltworks_Road_Links").AddComponent<SaltworksRoadLink>();
if(Object.FindObjectOfType<LanternCoastRoadLink>()==null)new GameObject("LanternCoast_Road_Links").AddComponent<LanternCoastRoadLink>();
if(Object.FindObjectOfType<SunscarIslandPolish>()==null)new GameObject("SunscarIsland_Polish").AddComponent<SunscarIslandPolish>();
if(Object.FindObjectOfType<StarterVehicleDistribution>()==null)new GameObject("SunscarIsland_Vehicles").AddComponent<StarterVehicleDistribution>();
if(Object.FindObjectOfType<MobileWorldOptimizer>()==null)new GameObject("SunscarIsland_MobileOptimizer").AddComponent<MobileWorldOptimizer>();
if(Object.FindObjectOfType<StarterSpawnBalance>()==null)new GameObject("SunscarIsland_SpawnBalance").AddComponent<StarterSpawnBalance>();
if(Object.FindObjectOfType<PoiLootTierBalancer>()==null)new GameObject("SunscarIsland_LootBalance").AddComponent<PoiLootTierBalancer>();}}
}
