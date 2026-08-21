using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Presentation
{
    /// <summary>Places authored utility props into Build 149 world without changing gameplay colliders.</summary>
    public sealed class BmgUtilityPropsRuntime : MonoBehaviour
    {
        private static BmgUtilityPropsRuntime instance;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap(){if(instance!=null)return;var host=new GameObject("BMG_UtilityPropsRuntime");DontDestroyOnLoad(host);instance=host.AddComponent<BmgUtilityPropsRuntime>();SceneManager.sceneLoaded+=instance.OnSceneLoaded;instance.StartCoroutine(instance.ApplyDelayed());}
        private void OnDestroy(){if(instance!=this)return;SceneManager.sceneLoaded-=OnSceneLoaded;instance=null;}
        private void OnSceneLoaded(Scene s,LoadSceneMode m)=>StartCoroutine(ApplyDelayed());
        private IEnumerator ApplyDelayed(){yield return null;yield return null;yield return null;PlaceGenerators();PlaceFuelTanks();PlaceRadioMasts();PlaceSupplyCrates();PlaceSandbags();PlaceContainers();PlaceBarrels();PlaceFences();}

        private static void PlaceGenerators()=>PlaceSet("BMG_Generator_Authored_","Models/BMG/bmg_generator_mk1",new[]{new Vector3(115f,.55f,-90f),new Vector3(190f,.55f,-135f),new Vector3(-88f,.55f,-55f)},1.15f,new Color(.27f,.29f,.22f));
        private static void PlaceFuelTanks()=>PlaceSet("BMG_FuelTank_Authored_","Models/BMG/bmg_fuel_tank_mk1",new[]{new Vector3(145f,.15f,-145f),new Vector3(178f,.15f,-147f),new Vector3(-108f,.15f,-71f)},1.15f,new Color(.32f,.31f,.26f));
        private static void PlaceRadioMasts()=>PlaceSet("BMG_RadioMast_Authored_","Models/BMG/bmg_radio_mast_mk1",new[]{new Vector3(218f,0f,-124f),new Vector3(-74f,0f,-34f)},1.4f,new Color(.24f,.27f,.28f));
        private static void PlaceSupplyCrates()=>PlaceSet("BMG_SupplyCrate_Authored_","Models/BMG/bmg_supply_crate_mk1",new[]{new Vector3(131f,.15f,-111f),new Vector3(139f,.15f,-109f),new Vector3(-96f,.15f,-61f),new Vector3(-103f,.15f,-60f)},.9f,new Color(.28f,.22f,.13f));
        private static void PlaceSandbags()=>PlaceSet("BMG_SandbagWall_Authored_","Models/BMG/bmg_sandbag_wall_mk1",new[]{new Vector3(112f,.15f,-105f),new Vector3(124f,.15f,-105f),new Vector3(203f,.15f,-108f),new Vector3(-82f,.15f,-48f)},1.1f,new Color(.42f,.36f,.25f));
        private static void PlaceContainers()=>PlaceSet("BMG_Container_Authored_","Models/BMG/bmg_shipping_container_mk1",new[]{new Vector3(102f,.1f,-151f),new Vector3(112f,.1f,-151f),new Vector3(205f,.1f,-151f)},.9f,new Color(.36f,.20f,.12f));
        private static void PlaceBarrels()=>PlaceSet("BMG_Barrel_Authored_","Models/BMG/bmg_oil_barrel_mk1",new[]{new Vector3(120f,.1f,-139f),new Vector3(122f,.1f,-139f),new Vector3(124f,.1f,-139f),new Vector3(-91f,.1f,-67f)},.9f,new Color(.24f,.27f,.28f));
        private static void PlaceFences()=>PlaceSet("BMG_Fence_Authored_","Models/BMG/bmg_fence_panel_mk1",new[]{new Vector3(92f,.1f,-162f),new Vector3(98f,.1f,-162f),new Vector3(104f,.1f,-162f),new Vector3(110f,.1f,-162f)},1.0f,new Color(.27f,.29f,.29f));

        private static void PlaceSet(string prefix,string path,Vector3[] positions,float scale,Color color)
        {
            if(GameObject.Find(prefix+"0")!=null)return;
            var prefab=Resources.Load<GameObject>(path);if(prefab==null)return;
            for(int i=0;i<positions.Length;i++)
            {
                var g=Instantiate(prefab);g.name=prefix+i;g.transform.position=positions[i];g.transform.localScale=Vector3.one*scale;
                if(g.GetComponent<Collider>()==null)g.AddComponent<BoxCollider>();ApplyMaterial(g,color);
            }
        }
        private static void ApplyMaterial(GameObject root,Color color){Shader s=Resources.Load<Shader>("Shaders/FspMobileSafe");if(s==null)s=Shader.Find("Fsp/MobileSafeLit");if(s==null)s=Shader.Find("Standard");if(s==null)return;var m=new Material(s){color=color,hideFlags=HideFlags.DontSave};foreach(var r in root.GetComponentsInChildren<Renderer>(true))if(r!=null)r.sharedMaterial=m;}
    }
}
