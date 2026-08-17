using Fsp.Presentation;
using UnityEngine;
namespace Fsp.World{
public sealed class MobileWorldOptimizer:MonoBehaviour{
 [SerializeField] float highDetailDistance=95f;[SerializeField] float visibleDistance=175f;[SerializeField] float colliderDistance=70f;[SerializeField] float updateInterval=.65f;
 Transform target;float nextUpdate;
 void Start(){ResolveTarget();ApplyQualityTier();ApplyStaticHints();}
 void Update(){if(Time.time<nextUpdate)return;nextUpdate=Time.time+updateInterval;if(target==null)ResolveTarget();if(target==null)return;OptimizeScene();}
 void ResolveTarget(){var cc=Object.FindObjectOfType<CharacterController>();if(cc!=null)target=cc.transform;else if(Camera.main!=null)target=Camera.main.transform;}
 void ApplyQualityTier(){var q=Object.FindObjectOfType<MobileQualityTier>();if(q==null)return;switch(q.ActiveTier){case MobileQualityTier.Tier.Low:highDetailDistance=65f;visibleDistance=125f;colliderDistance=52f;updateInterval=.85f;break;case MobileQualityTier.Tier.High:highDetailDistance=125f;visibleDistance=225f;colliderDistance=88f;updateInterval=.5f;break;default:highDetailDistance=95f;visibleDistance=175f;colliderDistance=70f;updateInterval=.65f;break;}}
 void ApplyStaticHints(){foreach(var r in Object.FindObjectsOfType<Renderer>()){if(r==null)continue;r.allowOcclusionWhenDynamic=true;}}
 void OptimizeScene(){Vector3 p=target.position;foreach(var r in Object.FindObjectsOfType<Renderer>()){if(r==null)continue;float d=Vector3.Distance(p,r.bounds.center);r.enabled=d<=visibleDistance;}foreach(var c in Object.FindObjectsOfType<Collider>()){if(c==null||c is CharacterController)continue;float d=Vector3.Distance(p,c.bounds.center);bool essential=c.gameObject.name.Contains("Floor")||c.gameObject.name.Contains("Ground")||c.gameObject.name.Contains("Road")||c.gameObject.name.Contains("Wall")||c.gameObject.name.Contains("Roof")||c.gameObject.name.Contains("Rock")||c.gameObject.name.Contains("Vehicle")||c.gameObject.name.Contains("Door");c.enabled=essential||d<=colliderDistance;}}
 public bool IsHighDetail(Vector3 worldPosition)=>target!=null&&Vector3.Distance(target.position,worldPosition)<=highDetailDistance;
}}
