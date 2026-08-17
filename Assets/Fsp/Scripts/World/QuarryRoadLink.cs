using UnityEngine;
namespace Fsp.World
{
    public sealed class QuarryRoadLink:MonoBehaviour
    {
        private void Start(){if(transform.childCount>0)return;Build(new Vector3(-60,.05f,35),new Vector3(-125,.05f,-82),7f);Build(new Vector3(72,.05f,-48),new Vector3(-125,.05f,-82),6f);}
        private void Build(Vector3 a,Vector3 b,float width){var mat=Mat(new Color(.24f,.23f,.21f));Vector3 prev=a;for(int i=1;i<=12;i++){float t=i/12f;Vector3 p=Vector3.Lerp(a,b,t);p.x+=Mathf.Sin(t*Mathf.PI)*10f;Vector3 d=p-prev;var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name="QuarryRoad";g.transform.SetParent(transform,false);g.transform.position=(p+prev)*.5f;g.transform.rotation=Quaternion.LookRotation(d.normalized);g.transform.localScale=new Vector3(width,.12f,d.magnitude+.7f);var r=g.GetComponent<Renderer>();if(r!=null)r.sharedMaterial=mat;prev=p;}}
        private static Material Mat(Color c){var s=Shader.Find("Standard");var m=new Material(s!=null?s:Shader.Find("Sprites/Default"));m.color=c;return m;}
    }
}
