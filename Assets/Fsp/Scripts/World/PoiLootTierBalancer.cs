using System;
using Fsp.Inventory;
using UnityEngine;
namespace Fsp.World{
public sealed class PoiLootTierBalancer:MonoBehaviour{
 [Serializable] public struct PoiTier{public string id;public Vector3 center;public float radius;[Range(0,1)]public float highTierChance;public int ammoBonus;public float armorBonus;}
 [SerializeField] PoiTier[] tiers={
  new PoiTier{id="old_crown",center=new Vector3(-60,0,35),radius=70,highTierChance=.32f,ammoBonus=10,armorBonus=5},
  new PoiTier{id="copper_port",center=new Vector3(72,0,-48),radius=75,highTierChance=.28f,ammoBonus=8,armorBonus=4},
  new PoiTier{id="dryfield",center=new Vector3(105,0,92),radius=78,highTierChance=.14f,ammoBonus=4,armorBonus=2},
  new PoiTier{id="white_quarry",center=new Vector3(-125,0,-82),radius=72,highTierChance=.35f,ammoBonus=12,armorBonus=8},
  new PoiTier{id="redline",center=new Vector3(155,0,-120),radius=90,highTierChance=.38f,ammoBonus=14,armorBonus=7},
  new PoiTier{id="saltworks",center=new Vector3(-15,0,165),radius=78,highTierChance=.22f,ammoBonus=7,armorBonus=4},
  new PoiTier{id="lantern_coast",center=new Vector3(-155,0,105),radius=74,highTierChance=.25f,ammoBonus=6,armorBonus=5}
 };
 void Start(){Balance();}
 public void Balance(){var pickups=Object.FindObjectsOfType<LootPickup>();foreach(var pickup in pickups){if(pickup==null)continue;var tier=FindTier(pickup.transform.position);if(!tier.HasValue)continue;ApplyTier(pickup,tier.Value);}}
 PoiTier? FindTier(Vector3 p){PoiTier? best=null;float bestD=float.MaxValue;foreach(var t in tiers){float d=(new Vector2(p.x-t.center.x,p.z-t.center.z)).sqrMagnitude;if(d<=t.radius*t.radius&&d<bestD){best=t;bestD=d;}}return best;}
 void ApplyTier(LootPickup pickup,PoiTier tier){var item=pickup.Item;if(item==null)return;int seed=StableHash(pickup.name+tier.id);var rng=new System.Random(seed);bool high=rng.NextDouble()<tier.highTierChance;if(item.type==InventoryItemType.Ammo)item.ammoAmount=Mathf.Max(item.ammoAmount,high?75+tier.ammoBonus:40+tier.ammoBonus);else if(item.type==InventoryItemType.Armor)item.armorAmount=Mathf.Max(item.armorAmount,high?50+tier.armorBonus:25+tier.armorBonus);}
 static int StableHash(string s){unchecked{int h=23;for(int i=0;i<s.Length;i++)h=h*31+s[i];return h;}}
}
}
