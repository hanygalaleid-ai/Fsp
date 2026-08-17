using Fsp.Inventory;
using UnityEngine;
namespace Fsp.World
{
 public sealed class RedlineAirstripPrototype:MonoBehaviour
 {
  Material asphalt,sand,metal,red;
  void Start()=>Build();
  void Build(){if(transform.Find("GeneratedRedline")!=null)return;var r=new GameObject("GeneratedRedline").transform;r.SetParent(transform,false);r.localPosition=new Vector3(155,0,-120);asphalt=Mat(new Color(.18f,.19f,.19f));sand=Mat(new Color(.48f,.39f,.27f));metal=Mat(new Color(.27f,.30f,.31f));red=Mat(new Color(.48f,.18f,.12f));
   Block(r,"AirfieldGround",new Vector3(0,-.2f,0),new Vector3(190,.4f,105),sand,true);Block(r,"Runway",new Vector3(0,.03f,0),new Vector3(170,.12f,18),asphalt,true);BuildMarks(r);BuildHangar(r,new Vector3(-45,0,30));BuildHangar(r,new Vector3(5,0,30));BuildHangar(r,new Vector3(55,0,30));BuildTower(r);BuildWrecks(r);BuildCover(r);SpawnLoot(r);
  }
  void BuildMarks(Transform r){for(int i=-7;i<=7;i++)Block(r,"RunwayMark",new Vector3(i*10,.11f,0),new Vector3(4,.04f,.45f),Mat(Color.white),false);}
  void BuildHangar(Transform r,Vector3 p){var h=new GameObject("Hangar").transform;h.SetParent(r,false);h.localPosition=p;Block(h,"Floor",new Vector3(0,.1f,0),new Vector3(28,.2f,18),asphalt,true);Block(h,"Back",new Vector3(0,4.5f,-9),new Vector3(28,9,.35f),metal,true);Block(h,"Left",new Vector3(-14,4.5f,0),new Vector3(.35f,9,18),metal,true);Block(h,"Right",new Vector3(14,4.5f,0),new Vector3(.35f,9,18),metal,true);Block(h,"Roof",new Vector3(0,9,0),new Vector3(28.5f,.4f,18.5f),metal,true);for(int i=0;i<6;i++)Block(h,"Crate",new Vector3(-10+i*4,.8f,-3+(i%2)*6),new Vector3(2.2f,1.6f,2.2f),red,true);}
  void BuildTower(Transform r){var t=new GameObject("ControlTower").transform;t.SetParent(r,false);t.localPosition=new Vector3(70,0,-30);Block(t,"Base",new Vector3(0,4,0),new Vector3(7,8,7),metal,true);Block(t,"Cab",new Vector3(0,9,0),new Vector3(10,3,10),red,true);Block(t,"Roof",new Vector3(0,10.7f,0),new Vector3(11,.3f,11),metal,true);for(int i=0;i<16;i++)Block(t,"Stair",new Vector3(-4.2f,.35f+i*.52f,-3.8f+i*.38f),new Vector3(1.6f,.22f,.65f),metal,true);}
  void BuildWrecks(Transform r){for(int i=0;i<5;i++){var w=new GameObject("AircraftWreck").transform;w.SetParent(r,false);w.localPosition=new Vector3(-60+i*30,.9f,-28+(i%2)*18);w.localRotation=Quaternion.Euler(0,i*37,0);Block(w,"Body",Vector3.zero,new Vector3(8,1.4f,1.5f),metal,true);Block(w,"Wing",new Vector3(0,0,0),new Vector3(2,0.25f,8),red,true);}}
  void BuildCover(Transform r){for(int i=0;i<26;i++)Block(r,"Barrier",new Vector3(-80+(i*19)%160,.65f,-42+(i*23)%84),new Vector3(2.8f,1.3f,.7f),i%3==0?red:metal,true).transform.localRotation=Quaternion.Euler(0,(i%6)*30,0);}
  void SpawnLoot(Transform r){for(int i=0;i<16;i++){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=$"airstrip_loot_{i}";g.transform.SetParent(r,false);g.transform.localPosition=new Vector3(-75+(i*21)%150,.45f,-38+(i*31)%78);g.transform.localScale=Vector3.one*.4f;var it=ScriptableObject.CreateInstance<InventoryItem>();it.hideFlags=HideFlags.DontSave;it.itemId=g.name;it.type=i%6==0?InventoryItemType.Medkit:i%6==1?InventoryItemType.Armor:InventoryItemType.Ammo;it.displayName=it.type.ToString();it.ammoAmount=it.type==InventoryItemType.Ammo?55:0;it.armorAmount=it.type==InventoryItemType.Armor?35:0;g.AddComponent<LootPickup>().Configure(it,g.name,true);}}
  static GameObject Block(Transform p,string n,Vector3 pos,Vector3 scale,Material m,bool col){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localScale=scale;var rr=g.GetComponent<Renderer>();if(rr!=null)rr.sharedMaterial=m;if(!col){var c=g.GetComponent<Collider>();if(c!=null)Destroy(c);}return g;}static Material Mat(Color c){var s=Shader.Find("Standard");var m=new Material(s!=null?s:Shader.Find("Sprites/Default"));m.color=c;return m;}
 }
}
