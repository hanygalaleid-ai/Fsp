using UnityEngine;

namespace Fsp.World
{
    /// <summary>Lightweight visual roads connecting Dryfield to Old Crown and Copper Port.</summary>
    public sealed class DryfieldRoadLinks : MonoBehaviour
    {
        private Material road;
        private void Start() => Build();
        private void Build()
        {
            if (transform.Find("GeneratedDryfieldRoads") != null) return;
            var root=new GameObject("GeneratedDryfieldRoads").transform; root.SetParent(transform,false);
            road=Mat(new Color(0.23f,0.22f,0.20f));
            BuildRoad(root,new Vector3(-60,0.04f,35),new Vector3(105,0.04f,92),7f,13);
            BuildRoad(root,new Vector3(72,0.04f,-48),new Vector3(105,0.04f,92),6.5f,10);
        }
        private void BuildRoad(Transform root,Vector3 a,Vector3 b,float width,int segments)
        {
            Vector3 prev=a;
            for(int i=1;i<=segments;i++)
            {
                float t=i/(float)segments; Vector3 p=Vector3.Lerp(a,b,t);
                p.x += Mathf.Sin(t*Mathf.PI)*8f; p.z += Mathf.Sin(t*Mathf.PI*2f)*3f;
                Vector3 d=p-prev; float len=d.magnitude; Vector3 mid=(p+prev)*0.5f;
                var g=GameObject.CreatePrimitive(PrimitiveType.Cube); g.name="RoadSegment"; g.transform.SetParent(root,false); g.transform.position=mid;
                g.transform.rotation=Quaternion.LookRotation(d.normalized,Vector3.up); g.transform.localScale=new Vector3(width,0.12f,len+0.8f);
                var r=g.GetComponent<Renderer>(); if(r!=null) r.sharedMaterial=road; prev=p;
            }
        }
        private static Material Mat(Color c){var s=Shader.Find("Standard");var m=new Material(s!=null?s:Shader.Find("Sprites/Default"));m.color=c;return m;}
    }
}
