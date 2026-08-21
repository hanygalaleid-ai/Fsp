using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Presentation
{
    /// <summary>Adds authored head/tail lamp meshes to Build 149 authored vehicles without touching vehicle physics.</summary>
    public sealed class BmgVehicleLightingRuntime : MonoBehaviour
    {
        private static BmgVehicleLightingRuntime instance;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap(){if(instance!=null)return;var host=new GameObject("BMG_VehicleLightingRuntime");DontDestroyOnLoad(host);instance=host.AddComponent<BmgVehicleLightingRuntime>();SceneManager.sceneLoaded+=instance.OnSceneLoaded;instance.StartCoroutine(instance.ApplyDelayed());}
        private void OnDestroy(){if(instance!=this)return;SceneManager.sceneLoaded-=OnSceneLoaded;instance=null;}
        private void OnSceneLoaded(Scene s,LoadSceneMode m)=>StartCoroutine(ApplyDelayed());
        private IEnumerator ApplyDelayed(){yield return null;yield return null;yield return null;Upgrade();}
        private static void Upgrade()
        {
            var prefab=Resources.Load<GameObject>("Models/BMG/bmg_vehicle_light_mk1");if(prefab==null)return;
            var all=FindObjectsByType<Transform>(FindObjectsSortMode.None);
            foreach(var t in all)
            {
                if(t==null||t.name!="BMG_Vehicle_Authored"||t.Find("BMG_Headlight_L")!=null)continue;
                Add(prefab,t,"BMG_Headlight_L",new Vector3(-.72f,.66f,1.92f),new Color(.82f,.78f,.58f));
                Add(prefab,t,"BMG_Headlight_R",new Vector3(.72f,.66f,1.92f),new Color(.82f,.78f,.58f));
                Add(prefab,t,"BMG_Taillight_L",new Vector3(-.72f,.64f,-1.90f),new Color(.55f,.08f,.04f));
                Add(prefab,t,"BMG_Taillight_R",new Vector3(.72f,.64f,-1.90f),new Color(.55f,.08f,.04f));
            }
        }
        private static void Add(GameObject prefab,Transform parent,string name,Vector3 pos,Color color)
        {
            var g=Instantiate(prefab,parent,false);g.name=name;g.transform.localPosition=pos;g.transform.localScale=Vector3.one*.65f;ApplyMaterial(g,color);
        }
        private static void ApplyMaterial(GameObject root,Color color){Shader s=Resources.Load<Shader>("Shaders/FspMobileSafe");if(s==null)s=Shader.Find("Fsp/MobileSafeLit");if(s==null)s=Shader.Find("Standard");if(s==null)return;var m=new Material(s){color=color,hideFlags=HideFlags.DontSave};foreach(var r in root.GetComponentsInChildren<Renderer>(true))if(r!=null)r.sharedMaterial=m;}
    }
}
