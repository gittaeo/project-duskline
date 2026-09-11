using System.Collections.Generic;
using UnityEngine;
namespace Duskline {
public sealed class MeleeCombat : MonoBehaviour {
    public float damage=34,reach=1.9f,radius=.7f,cooldown=.65f,cost=20;
    public bool playerSide;
    public Transform weapon;
    public float LastSwing {get;private set;}=-100;
    public const float ImpactTime=.46f, SwingDuration=.86f;
    public bool Swinging=>playerSide&&Time.time-LastSwing<SwingDuration;
    bool pendingImpact;
    Health health;Stamina stamina;NoiseEmitter noise;
    readonly HashSet<Health> struck=new HashSet<Health>();
    void Awake(){health=GetComponent<Health>();stamina=GetComponent<Stamina>();noise=GetComponent<NoiseEmitter>();}
    public bool Attack(){
        if(!health||!health.Alive||!RunDirector.Instance||!RunDirector.Instance.Playing||Time.time<LastSwing+Mathf.Max(cooldown,playerSide?SwingDuration:0))return false;
        if(stamina&&!stamina.Spend(cost))return false;
        LastSwing=Time.time;if(noise)noise.Emit(noise.attackRadius);struck.Clear();
        if(playerSide){pendingImpact=true;return true;}
        Hit();return true;
    }
    void Hit(){
        Vector3 start=transform.position+Vector3.up, end=start+transform.forward*reach;
        foreach(var hit in Physics.OverlapCapsule(start,end,radius,1<<(playerSide?10:8),QueryTriggerInteraction.Ignore)){
            var target=hit.GetComponentInParent<Health>();if(!target||target==health||!target.Alive||!struck.Add(target))continue;
            if(Physics.Linecast(start,target.transform.position+Vector3.up,1,QueryTriggerInteraction.Ignore))continue;
            target.Damage(damage);
        }
    }
    void Update(){
        if(!pendingImpact)return;
        if(!health.Alive||!RunDirector.Instance||RunDirector.Instance.Result!=RunResult.Running){pendingImpact=false;return;}
        if(RunDirector.Instance.Playing&&Time.time>=LastSwing+ImpactTime){pendingImpact=false;Hit();}
    }
    public static float SwingAngle(float elapsed){
        if(elapsed<0||elapsed>=SwingDuration)return 12;
        if(elapsed<.3f)return Mathf.Lerp(12,-150,Mathf.SmoothStep(0,1,elapsed/.3f));
        if(elapsed<ImpactTime)return Mathf.Lerp(-150,72,Mathf.Pow((elapsed-.3f)/(.16f),1.7f));
        return Mathf.Lerp(72,12,Mathf.SmoothStep(0,1,(elapsed-ImpactTime)/(SwingDuration-ImpactTime)));
    }
    void LateUpdate(){if(weapon)weapon.localRotation=Quaternion.Euler(SwingAngle(Time.time-LastSwing),0,-8);}
    void OnDrawGizmosSelected(){Gizmos.color=Color.red;Gizmos.DrawWireSphere(transform.position+Vector3.up+transform.forward*reach,radius);}
}
}
