using System.Collections;
using Fsp.BattleRoyale;
using Fsp.Combat;
using Fsp.Vehicles;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Presentation
{
    /// <summary>Strict authored-only BMG presentation. No runtime mesh generation.</summary>
    public sealed class BmgAuthoredVisualRuntime : MonoBehaviour
    {
        private const string RiflePath="Models/BMG/bmg_assault_rifle_mk1",SmgPath="Models/BMG/bmg_smg_mk1",SniperPath="Models/BMG/bmg_sniper_mk1",ShotgunPath="Models/BMG/bmg_shotgun_mk1";
        private const string HelmetPath="Models/BMG/bmg_helmet_mk1",FaceMaskPath="Models/BMG/bmg_face_mask_mk1",BackpackPath="Models/BMG/bmg_backpack_mk1",VestPath="Models/BMG/bmg_tactical_vest_mk1",BootPath="Models/BMG/bmg_combat_boot_mk1",GlovePath="Models/BMG/bmg_tactical_glove_mk1",KneePath="Models/BMG/bmg_knee_pad_mk1";
        private const string MaleTorsoPath="Models/BMG/bmg_male_torso_mk1",FemaleTorsoPath="Models/BMG/bmg_female_torso_mk1",HeadPath="Models/BMG/bmg_head_mk1",ArmPath="Models/BMG/bmg_arm_mk1",LegPath="Models/BMG/bmg_leg_mk1";
        private const string BuggyPath="Models/BMG/bmg_buggy_mk1",DesertCarPath="Models/BMG/bmg_desert_car_mk1",PlanePath="Models/BMG/bmg_transport_plane_mk1";
        private static BmgAuthoredVisualRuntime instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap(){if(instance!=null)return;var host=new GameObject("BMG_AuthoredOnlyRuntime");DontDestroyOnLoad(host);instance=host.AddComponent<BmgAuthoredVisualRuntime>();SceneManager.sceneLoaded+=instance.OnSceneLoaded;instance.StartCoroutine(instance.ApplyWhenReady());}
        private void OnDestroy(){if(instance!=this)return;SceneManager.sceneLoaded-=OnSceneLoaded;instance=null;}
        private void OnSceneLoaded(Scene s,LoadSceneMode m)=>StartCoroutine(ApplyWhenReady());
        private IEnumerator ApplyWhenReady(){for(int i=0;i<12;i++){yield return i==0?null:new WaitForSeconds(.25f);UpgradeCharacters();UpgradeVehicles();UpgradePlanes();UpgradeWeapons();}}

        private static void UpgradeCharacters(){
            var players=FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None);
            foreach(var p in players){if(p==null||p.transform.Find("BMG_Character_Authored")!=null)continue;HideLegacy(p.transform,"FSP_CharacterVisual");
                string selected=Fsp.Lobby.LobbyState.Instance!=null?Fsp.Lobby.LobbyState.Instance.SelectedCharacterId:"soldier_01";bool female=p.IsBot?(p.GetInstanceID()&1)==0:selected=="soldier_03";
                var root=new GameObject("BMG_Character_Authored").transform;root.SetParent(p.transform,false);
                Add(root,female?FemaleTorsoPath:MaleTorsoPath,"Torso",new Vector3(0,1.15f,0),Vector3.one*.78f,Quaternion.identity);
                Add(root,VestPath,"Vest",new Vector3(0,1.20f,.08f),Vector3.one,Quaternion.identity);
                Add(root,HeadPath,"Head",new Vector3(0,1.83f,0),Vector3.one*.58f,Quaternion.identity);
                Add(root,FaceMaskPath,"FaceMask",new Vector3(0,1.79f,.16f),Vector3.one,Quaternion.identity);
                Add(root,HelmetPath,"Helmet",new Vector3(0,2.02f,0),Vector3.one,Quaternion.identity);
                Add(root,BackpackPath,"Backpack",new Vector3(0,1.22f,-.28f),Vector3.one*.92f,Quaternion.Euler(0,180,0));
                Add(root,ArmPath,"LeftArm",new Vector3(-.42f,1.42f,0),Vector3.one*1.55f,Quaternion.Euler(0,0,8));
                Add(root,ArmPath,"RightArm",new Vector3(.42f,1.42f,0),Vector3.one*1.55f,Quaternion.Euler(0,0,-8));
                Add(root,LegPath,"LeftLeg",new Vector3(-.18f,.72f,0),Vector3.one*1.72f,Quaternion.identity);
                Add(root,LegPath,"RightLeg",new Vector3(.18f,.72f,0),Vector3.one*1.72f,Quaternion.identity);
                Add(root,GlovePath,"LeftGlove",new Vector3(-.47f,.86f,.02f),Vector3.one*.72f,Quaternion.identity);Add(root,GlovePath,"RightGlove",new Vector3(.47f,.86f,.02f),Vector3.one*.72f,Quaternion.identity);
                Add(root,KneePath,"LeftKnee",new Vector3(-.18f,.48f,.16f),Vector3.one*.68f,Quaternion.identity);Add(root,KneePath,"RightKnee",new Vector3(.18f,.48f,.16f),Vector3.one*.68f,Quaternion.identity);
                Add(root,BootPath,"LeftBoot",new Vector3(-.18f,.12f,.08f),Vector3.one,Quaternion.identity);Add(root,BootPath,"RightBoot",new Vector3(.18f,.12f,.08f),Vector3.one,Quaternion.identity);
            }}

        private static void UpgradeVehicles(){var vehicles=FindObjectsByType<SimpleVehicleController>(FindObjectsSortMode.None);for(int i=0;i<vehicles.Length;i++){var v=vehicles[i];if(v==null||v.transform.Find("BMG_Vehicle_Authored")!=null)continue;HideLegacy(v.transform,"FSP_ScoutVehicleVisual");var prefab=Resources.Load<GameObject>((i&1)==0?BuggyPath:DesertCarPath)??Resources.Load<GameObject>(BuggyPath);if(prefab==null)continue;var model=Instantiate(prefab,v.transform,false);model.name="BMG_Vehicle_Authored";model.transform.localScale=Vector3.one*.92f;}}
        private static void UpgradePlanes(){var prefab=Resources.Load<GameObject>(PlanePath);if(prefab==null)return;foreach(var plane in FindObjectsByType<DropPlaneController>(FindObjectsSortMode.None)){if(plane==null||plane.transform.Find("BMG_TransportPlane_Authored")!=null)continue;HideLegacy(plane.transform,"FSP_TransportPlaneVisual");var model=Instantiate(prefab,plane.transform,false);model.name="BMG_TransportPlane_Authored";}}
        private static void UpgradeWeapons(){foreach(var w in FindObjectsByType<HitscanWeapon>(FindObjectsSortMode.None)){if(w==null||w.transform.Find("BMG_Weapon_Authored")!=null)continue;foreach(var r in w.GetComponentsInChildren<Renderer>(true))if(r!=null&&r.transform!=w.transform)r.enabled=false;var prefab=Resources.Load<GameObject>(PathFor(w.Config));if(prefab==null)continue;var model=Instantiate(prefab,w.transform,false);model.name="BMG_Weapon_Authored";model.transform.localScale=ScaleFor(w.Config);}}
        private static string PathFor(WeaponConfig c)=>c==null?RiflePath:c.weaponClass switch{WeaponClass.SMG=>SmgPath,WeaponClass.Marksman=>SniperPath,WeaponClass.Shotgun=>ShotgunPath,_=>RiflePath};
        private static Vector3 ScaleFor(WeaponConfig c)=>Vector3.one*(c==null?.72f:c.weaponClass switch{WeaponClass.SMG=>.66f,WeaponClass.Marksman=>.76f,WeaponClass.Shotgun=>.74f,_=>.72f});
        private static void Add(Transform parent,string path,string name,Vector3 pos,Vector3 scale,Quaternion rot){var prefab=Resources.Load<GameObject>(path);if(prefab==null)return;var model=Instantiate(prefab,parent,false);model.name="BMG_"+name;model.transform.localPosition=pos;model.transform.localScale=scale;model.transform.localRotation=rot;}
        private static void HideLegacy(Transform host,string childName){var old=host.Find(childName);if(old==null)return;foreach(var r in old.GetComponentsInChildren<Renderer>(true))if(r!=null)r.enabled=false;old.gameObject.SetActive(false);}
    }
}
