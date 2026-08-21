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
        private IEnumerator ApplyDelayed(){yield return null;yield return null;yield return null;PlaceGenerators();}
        private static void PlaceGenerators()
        {
            if(GameObject.Find("BMG_Generator_Authored_0")!=null)return;
            var prefab=Resources.Load<GameObject>("Models/BMG/bmg_generator_mk1");if(prefab==null)return;
            Vector3[] positions={new Vector3(115f,.55f,-90f),new Vector3(190f,.55f,-135f),new Vector3(-88f,.55f,-55f)};
            for(int i=0;i<positions.Length;i++)
            {
                var g=Instantiate(prefab);g.name="BMG_Generator_Authored_"+i;g.transform.position=positions[i];g.transform.localScale=Vector3.one*1.15f;
                if(g.GetComponent<Collider>()==null)g.AddComponent<BoxCollider>();ApplyMaterial(g,new Color(.27f,.29f,.22f));
            }
        }
        private static void ApplyMaterial(GameObject root,Color color){Shader s=Resources.Load<Shader>("Shaders/FspMobileSafe");if(s==null)s=Shader.Find("Fsp/MobileSafeLit");if(s==null)s=Shader.Find("Standard");if(s==null)return;var m=new Material(s){color=color,hideFlags=HideFlags.DontSave};foreach(var r in root.GetComponentsInChildren<Renderer>(true))if(r!=null)r.sharedMaterial=m;}
    }
}
