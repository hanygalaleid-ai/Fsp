using Fsp.Bots;
using UnityEngine;
namespace Fsp.World{
public sealed class StarterSpawnBalance:MonoBehaviour{
 static readonly Vector3[] Anchors={new(-78,1,20),new(52,1,-38),new(92,1,78),new(-110,1,-66),new(132,1,-95),new(-8,1,138),new(-132,1,82),new(0,1,0)};
 void Start(){var spawner=Object.FindObjectOfType<BotSpawner>();if(spawner==null)return;var root=new GameObject("BalancedBotSpawns").transform;root.SetParent(transform,false);var points=new Transform[Anchors.Length];for(int i=0;i<Anchors.Length;i++){var g=new GameObject("BotSpawn_"+i);g.transform.SetParent(root,false);g.transform.position=Anchors[i];points[i]=g.transform;}
 // BotSpawner currently supports inspector spawn points only; anchors are created now for future binding and scene authoring.
 }
}}
