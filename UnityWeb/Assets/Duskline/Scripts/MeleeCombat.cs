using System.Collections.Generic;
using UnityEngine;
namespace Duskline {
public sealed class MeleeCombat : MonoBehaviour {
    public float damage=34,reach=1.9f,radius=.7f,cooldown=.65f,cost=20;
    public bool playerSide;
    public Transform weapon;
    public float LastSwing {get;private set;}=-100;
    Health health;Stamina stamina;NoiseEmitter noise;
    readonly HashSet<Health> struck=new HashSet<Health>();
    void Awake(){health=GetComponent<Health>();stamina=GetComponent<Stamina>();noise=GetComponent<NoiseEmitter>();}
    public bool Attack(){
        if(!health||!health.Alive||!RunDirector.Instance||!RunDirector.Instance.Playing||Time.time<LastSwing+cooldown)return false;
        if(stamina&&!stamina.Spend(cost))return false;
        LastSwing=Time.time;if(noise)noise.Emit(noise.attackRadius);struck.Clear();
        Vector3 start=transform.position+Vector3.up, end=start+transform.forward*reach;
        foreach(var hit in Physics.OverlapCapsule(start,end,radius,1<<(playerSide?10:8),QueryTriggerInteraction.Ignore)){
            var target=hit.GetComponentInParent<Health>();if(!target||target==health||!target.Alive||!struck.Add(target))continue;
            if(Physics.Linecast(start,target.transform.position+Vector3.up,1,QueryTriggerInteraction.Ignore))continue;
            target.Damage(damage);
        }
        return true;
    }
    void LateUpdate(){if(weapon){float t=Mathf.Clamp01((Time.time-LastSwing)/.28f);weapon.localRotation=Quaternion.Euler(0,0,t<1?Mathf.Sin(t*Mathf.PI)*-110:0);}}
    void OnDrawGizmosSelected(){Gizmos.color=Color.red;Gizmos.DrawWireSphere(transform.position+Vector3.up+transform.forward*reach,radius);}
}
}
