using Fsp.Inventory;
using UnityEngine;

namespace Fsp.World
{
    /// <summary>Open farm POI: fields, barns, irrigation, hay cover, watchtower and loot.</summary>
    public sealed class DryfieldPrototype : MonoBehaviour
    {
        private Material soil, crop, wood, stone, metal;
        private void Start() => BuildIfNeeded();

        public void BuildIfNeeded()
        {
            if (transform.Find("GeneratedDryfield") != null) return;
            var root = new GameObject("GeneratedDryfield").transform;
            root.SetParent(transform, false);
            root.localPosition = new Vector3(105f, 0f, 92f);

            soil = Mat(new Color(0.43f,0.31f,0.18f)); crop = Mat(new Color(0.55f,0.48f,0.22f));
            wood = Mat(new Color(0.32f,0.22f,0.13f)); stone = Mat(new Color(0.50f,0.46f,0.37f)); metal = Mat(new Color(0.20f,0.24f,0.25f));

            Block(root,"FarmGround",new Vector3(0,-0.2f,0),new Vector3(125,0.4f,105),soil,true);
            BuildFields(root); BuildBarn(root,new Vector3(-28,0,-12),0); BuildBarn(root,new Vector3(30,0,20),90);
            BuildFarmhouse(root,new Vector3(4,0,-28)); BuildTower(root,new Vector3(-5,0,15));
            BuildIrrigation(root); BuildHayCover(root); SpawnLoot(root);
        }

        private void BuildFields(Transform r)
        {
            for(int f=0;f<4;f++) for(int i=0;i<12;i++)
            {
                float x=-48+f*31; float z=-5+i*3.8f;
                Block(r,"CropRow",new Vector3(x,0.42f,z),new Vector3(18,0.75f,0.55f),crop,false);
            }
        }

        private void BuildBarn(Transform r,Vector3 p,float yaw)
        {
            var b=new GameObject("Barn").transform; b.SetParent(r,false); b.localPosition=p; b.localRotation=Quaternion.Euler(0,yaw,0);
            Block(b,"Floor",new Vector3(0,0.1f,0),new Vector3(12,0.2f,8),stone,true);
            Block(b,"Back",new Vector3(0,2.4f,-4),new Vector3(12,4.8f,0.3f),wood,true);
            Block(b,"Left",new Vector3(-6,2.4f,0),new Vector3(0.3f,4.8f,8),wood,true);
            Block(b,"Right",new Vector3(6,2.4f,0),new Vector3(0.3f,4.8f,8),wood,true);
            Block(b,"FrontL",new Vector3(-4.1f,2.4f,4),new Vector3(3.8f,4.8f,0.3f),wood,true);
            Block(b,"FrontR",new Vector3(4.1f,2.4f,4),new Vector3(3.8f,4.8f,0.3f),wood,true);
            Block(b,"Roof",new Vector3(0,5.0f,0),new Vector3(12.7f,0.3f,8.7f),metal,true);
            for(int i=0;i<5;i++) Block(b,"BarnCover",new Vector3(-3.6f+i*1.8f,0.7f,-1.5f),new Vector3(1.25f,1.4f,1.25f),wood,true);
        }

        private void BuildFarmhouse(Transform r,Vector3 p)
        {
            var h=new GameObject("Farmhouse").transform; h.SetParent(r,false); h.localPosition=p;
            Block(h,"Base",new Vector3(0,1.6f,0),new Vector3(8,3.2f,7),stone,true);
            Block(h,"DoorCutCoverL",new Vector3(-2.7f,1.2f,3.65f),new Vector3(2.2f,2.4f,0.4f),wood,true);
            Block(h,"DoorCutCoverR",new Vector3(2.7f,1.2f,3.65f),new Vector3(2.2f,2.4f,0.4f),wood,true);
            Block(h,"Roof",new Vector3(0,3.45f,0),new Vector3(8.6f,0.35f,7.6f),metal,true);
        }

        private void BuildTower(Transform r,Vector3 p)
        {
            var t=new GameObject("WaterWatchTower").transform; t.SetParent(r,false); t.localPosition=p;
            for(int x=-1;x<=1;x+=2) for(int z=-1;z<=1;z+=2) Block(t,"Leg",new Vector3(x*1.4f,3,z*1.4f),new Vector3(0.25f,6,0.25f),metal,true);
            Block(t,"Platform",new Vector3(0,6.1f,0),new Vector3(4,0.3f,4),wood,true);
            Block(t,"Tank",new Vector3(0,7.3f,0),new Vector3(3,2.1f,3),metal,true);
            for(int i=0;i<12;i++) Block(t,"Ladder",new Vector3(1.55f,0.35f+i*0.48f,-1.55f),new Vector3(0.7f,0.12f,0.12f),metal,true);
        }

        private void BuildIrrigation(Transform r)
        {
            Block(r,"Canal",new Vector3(0,0.12f,3),new Vector3(105,0.25f,2.4f),stone,true);
            for(int i=-4;i<=4;i++) Block(r,"CanalBridge",new Vector3(i*12,0.48f,3),new Vector3(3.5f,0.28f,3.2f),wood,true);
        }

        private void BuildHayCover(Transform r)
        {
            for(int i=0;i<22;i++)
            {
                float x=-52+(i*17)%104; float z=-42+(i*29)%82;
                var go=GameObject.CreatePrimitive(PrimitiveType.Cylinder); go.name="HayBale"; go.transform.SetParent(r,false);
                go.transform.localPosition=new Vector3(x,0.7f,z); go.transform.localRotation=Quaternion.Euler(90,0,(i%4)*15); go.transform.localScale=new Vector3(1.15f,0.75f,1.15f);
                var rr=go.GetComponent<Renderer>(); if(rr!=null) rr.sharedMaterial=crop;
            }
        }

        private void SpawnLoot(Transform r)
        {
            for(int i=0;i<12;i++)
            {
                var go=GameObject.CreatePrimitive(PrimitiveType.Cube); go.name=$"dryfield_loot_{i}"; go.transform.SetParent(r,false);
                go.transform.localPosition=new Vector3(-38+(i*13)%78,0.4f,-30+(i*19)%62); go.transform.localScale=Vector3.one*0.4f;
                var item=ScriptableObject.CreateInstance<InventoryItem>(); item.hideFlags=HideFlags.DontSave; item.itemId=go.name;
                item.type=i%4==0?InventoryItemType.Medkit:i%4==1?InventoryItemType.Armor:InventoryItemType.Ammo; item.displayName=item.type.ToString();
                item.ammoAmount=item.type==InventoryItemType.Ammo?45:0; item.armorAmount=item.type==InventoryItemType.Armor?25f:0;
                go.AddComponent<LootPickup>().Configure(item,go.name,true);
            }
        }

        private static GameObject Block(Transform p,string n,Vector3 pos,Vector3 scale,Material m,bool col)
        { var g=GameObject.CreatePrimitive(PrimitiveType.Cube); g.name=n; g.transform.SetParent(p,false); g.transform.localPosition=pos; g.transform.localScale=scale; var rr=g.GetComponent<Renderer>(); if(rr!=null) rr.sharedMaterial=m; if(!col){var c=g.GetComponent<Collider>(); if(c!=null) Destroy(c);} return g; }
        private static Material Mat(Color c){var s=Shader.Find("Standard"); var m=new Material(s!=null?s:Shader.Find("Sprites/Default")); m.color=c; return m;}
    }
}
