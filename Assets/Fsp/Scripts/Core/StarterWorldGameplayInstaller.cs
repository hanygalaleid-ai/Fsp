using Fsp.World;
using UnityEngine;
namespace Fsp.Core
{
 public static class StarterWorldGameplayInstaller
 {
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Install()
  {
   if(Object.FindObjectOfType<OldCrownInteriorPrototype>()==null){var g=new GameObject("OldCrown_Interiors");g.transform.position=new Vector3(-60,0,35);g.AddComponent<OldCrownInteriorPrototype>();}
   if(Object.FindObjectOfType<OldCrownDoorInstaller>()==null)new GameObject("OldCrown_Doors").AddComponent<OldCrownDoorInstaller>();
   if(Object.FindObjectOfType<CopperPortPrototype>()==null)new GameObject("CopperPort_Prototype").AddComponent<CopperPortPrototype>();
   if(Object.FindObjectOfType<DryfieldPrototype>()==null)new GameObject("Dryfield_Prototype").AddComponent<DryfieldPrototype>();
   if(Object.FindObjectOfType<WhiteQuarryPrototype>()==null)new GameObject("WhiteQuarry_Prototype").AddComponent<WhiteQuarryPrototype>();
   if(Object.FindObjectOfType<RedlineAirstripPrototype>()==null)new GameObject("RedlineAirstrip_Prototype").AddComponent<RedlineAirstripPrototype>();
   if(Object.FindObjectOfType<StarterPoiRoadLink>()==null)new GameObject("OldCrown_CopperPort_Road").AddComponent<StarterPoiRoadLink>();
   if(Object.FindObjectOfType<DryfieldRoadLinks>()==null)new GameObject("Dryfield_Road_Links").AddComponent<DryfieldRoadLinks>();
   if(Object.FindObjectOfType<QuarryRoadLink>()==null)new GameObject("WhiteQuarry_Road_Links").AddComponent<QuarryRoadLink>();
   if(Object.FindObjectOfType<AirstripRoadLink>()==null)new GameObject("RedlineAirstrip_Road_Links").AddComponent<AirstripRoadLink>();
  }
 }
}
