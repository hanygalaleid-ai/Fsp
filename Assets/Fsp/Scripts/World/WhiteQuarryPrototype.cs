using Fsp.Inventory;
using UnityEngine;

namespace Fsp.World
{
    /// <summary>Vertical quarry POI with terraces, ramps, rock cover, crusher and loot.</summary>
    public sealed class WhiteQuarryPrototype : MonoBehaviour
    {
        private Material chalk, rock, metal, road;
        private void Start()=>BuildIfNeeded();
        public void BuildIfNeeded()
        {
            if(transform.Find("GeneratedWhiteQuarry")!=null)return;
            var root=new GameObject("GeneratedWhiteQuarry").transform;root.SetParent(transform,false);root.localPosition=new Vector3(-125,0,-82);
            chalk=Mat(new Color(.73f,.70f,.62f)); rock=Mat(new Color(.42f,.40f,.36f)); metal=Mat(new Color(.25f,.29f,.30f)); road=Mat(new Color(.31f,.29f,.25f));
            Block(root,"QuarryBase",new Vector3(0,-1.0f,0),new Vector3(118,2,98),chalk,true);
            BuildTerraces(root); BuildRamps(root); BuildCrusher(root); BuildRockCover(root); BuildOffice(root); SpawnLoot(root);
        }
        private void BuildTerraces(Transform r)
        {
            for(int level=0;level<4;level++)
            {
                float y=level*3.2f; float sx=112-level*19; float sz=92-level*15;
                Block(r,$"Terrace_{level}",new Vector3(0,y-0.25f,0),new Vector3(sx,.5f,sz),chalk,true);
                if(level>0){ Block(r,"CliffN",new Vector3(0,y-1.6f,sz*.5f),new Vector3(sx,3.2f,.8f),rock,true); Block(r,"CliffS",new Vector3(0,y-1.6f,-sz*.5f),new Vector3(sx,3.2f,.8f),rock,true); }
            }
        }
        private void BuildRamps(Transform r)
        {
            for(int level=0;level<3;level++)
            {
                float y=level*3.2f+1.6f; float z=30-level*12; var g=Block(r,"HaulRamp",new Vector3(28-level*12,y,z),new Vector3(22,.45f,7),road,true);
                g.transform.localRotation=Quaternion.Euler(0,level%2==0?25:-25,-8.2f);
            }
        }
        private void BuildCrusher(Transform r)
        {
            var c=new GameObject("CrusherPlant").transform;c.SetParent(r,false);c.localPosition=new Vector3(-24,9.8f,-9);
            Block(c,"Deck",new Vector3(0,.2f,0),new Vector3(18,.4f,11),metal,true);
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Block(c,"Support",new Vector3(x*7, -4.4f,z*4),new Vector3(.55f,9,.55f),metal,true);
            Block(c,"Crusher",new Vector3(0,2.0f,0),new Vector3(7,4,6),rock,true);
            Block(c,"Conveyor",new Vector3(10,1.0f,3),new Vector3(15,.55f,2.1f),metal,true).transform.localRotation=Quaternion.Euler(0,-18,8);
            for(int i=0;i<10;i++)Block(c,"AccessStep",new Vector3(-8.8f,-3.7f+i*.62f,-4.7f),new Vector3(1.8f,.25f,.7f),metal,true);
        }
        private void BuildRockCover(Transform r)
        {
            for(int i=0;i<34;i++)
            {
                var g=GameObject.CreatePrimitive(i%3==0?PrimitiveType.Sphere:PrimitiveType.Cube);g.name="QuarryRock";g.transform.SetParent(r,false);
                g.transform.localPosition=new Vector3(-50+(i*23)%101,.8f+(i%4)*.22f,-40+(i*31)%81);g.transform.localRotation=Quaternion.Euler(i*11,i*29,i*7);
                float s=1.1f+(i%5)*.38f;g.transform.localScale=new Vector3(s*1.5f,s,s);var rr=g.GetComponent<Renderer>();if(rr!=null)rr.sharedMaterial=rock;
            }
        }
        private void BuildOffice(Transform r)
        {
            var o=new GameObject("QuarryOffice").transform;o.SetParent(r,false);o.localPosition=new Vector3(38,3.5f,-25);
            Block(o,"Office",new Vector3(0,1.5f,0),new Vector3(9,3,6),metal,true);Block(o,"Roof",new Vector3(0,3.2f,0),new Vector3(9.6f,.3f,6.6f),chalk,true);
            Block(o,"Balcony",new Vector3(0,2.4f,3.8f),new Vector3(8,.25f,1.8f),metal,true);
        }
        private void SpawnLoot(Transform r)
        {
            for(int i=0;i<14;i++)
            {
                var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=$"quarry_loot_{i}";g.transform.SetParent(r,false);g.transform.localPosition=new Vector3(-45+(i*17)%92,.55f+(i%4)*3.2f,-35+(i*27)%72);g.transform.localScale=Vector3.one*.4f;
                var item=ScriptableObject.CreateInstance<InventoryItem>();item.hideFlags=HideFlags.DontSave;item.itemId=g.name;item.type=i%5==0?InventoryItemType.Medkit:i%5==1?InventoryItemType.Armor:InventoryItemType.Ammo;item.displayName=item.type.ToString();item.ammoAmount=item.type==InventoryItemType.Ammo?50:0;item.armorAmount=item.type==InventoryItemType.Armor?35:0;g.AddComponent<LootPickup>().Configure(item,g.name,true);
            }
        }
        private static GameObject Block(Transform p,string n,Vector3 pos,Vector3 scale,Material m,bool col){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localScale=scale;var rr=g.GetComponent<Renderer>();if(rr!=null)rr.sharedMaterial=m;if(!col){var c=g.GetComponent<Collider>();if(c!=null)Destroy(c);}return g;}
        private static Material Mat(Color c){var s=Shader.Find("Standard");var m=new Material(s!=null?s:Shader.Find("Sprites/Default"));m.color=c;return m;}
    }
}
