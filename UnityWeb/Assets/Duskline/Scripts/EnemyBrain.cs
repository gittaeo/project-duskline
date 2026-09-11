using UnityEngine;
using UnityEngine.AI;
namespace Duskline {
public enum EnemyState { Idle,Patrol,Investigate,Chase,Attack,Dead }
[RequireComponent(typeof(NavMeshAgent),typeof(Health),typeof(MeleeCombat))]
public sealed class EnemyBrain : MonoBehaviour {
    public Vector3[] patrol;
    public float sightRange=10,viewAngle=60,searchSeconds=7,attackDistance=1.65f;
    public EnemyState State {get;private set;}
    public Transform body;
    NavMeshAgent agent;Health health;MeleeCombat combat;PlayerMotor target;
    Vector3 lastKnown;float searchUntil;int patrolIndex;bool sawTarget;
    void Awake(){agent=GetComponent<NavMeshAgent>();health=GetComponent<Health>();combat=GetComponent<MeleeCombat>();}
    void OnEnable(){NoiseEmitter.Emitted+=Heard;health.Died+=Die;health.Changed+=Hit;}
    void Start(){target=RunDirector.Instance.player;InvokeRepeating(nameof(Think),.1f,.2f);}
    void OnDisable(){NoiseEmitter.Emitted-=Heard;health.Died-=Die;health.Changed-=Hit;CancelInvoke();}
    void Hit(){if(health.Alive&&body){body.localScale=new Vector3(1.12f,.88f,1.12f);CancelInvoke(nameof(ResetBody));Invoke(nameof(ResetBody),.15f);}}
    void ResetBody(){if(body&&health.Alive)body.localScale=Vector3.one;}
    void Die(){State=EnemyState.Dead;CancelInvoke();if(agent.isOnNavMesh){agent.isStopped=true;agent.ResetPath();}agent.enabled=false;foreach(var c in GetComponentsInChildren<Collider>())c.enabled=false;if(body){body.localRotation=Quaternion.Euler(0,0,90);body.localPosition=Vector3.up*.2f;}}
    bool Sees(){
        if(!target||!target.health.Alive)return false;Vector3 delta=target.transform.position-transform.position;
        float range=target.Crouched?sightRange*.55f:sightRange;
        return delta.sqrMagnitude<range*range&&Vector3.Angle(transform.forward,delta)<viewAngle&&!Physics.Linecast(transform.position+Vector3.up*1.4f,target.transform.position+Vector3.up,1,QueryTriggerInteraction.Ignore);
    }
    void Heard(NoiseEvent noise){if(!health.Alive||sawTarget||!RunDirector.Instance||!RunDirector.Instance.Playing)return;if((noise.Position-transform.position).sqrMagnitude>noise.Radius*noise.Radius)return;lastKnown=noise.Position;searchUntil=Time.time+noise.Duration;State=EnemyState.Investigate;}
    void Think(){
        if(!health.Alive||!agent.enabled||!agent.isOnNavMesh)return;
        if(!RunDirector.Instance||!RunDirector.Instance.Playing){agent.isStopped=true;return;}agent.isStopped=false;
        bool visible=Sees();
        if(visible){sawTarget=true;lastKnown=target.transform.position;
            if((lastKnown-transform.position).sqrMagnitude<attackDistance*attackDistance){agent.isStopped=true;State=EnemyState.Attack;Vector3 dir=lastKnown-transform.position;dir.y=0;if(dir.sqrMagnitude>.01f)transform.rotation=Quaternion.LookRotation(dir);combat.Attack();}
            else{State=EnemyState.Chase;agent.SetDestination(lastKnown);}return;
        }
        if(sawTarget){sawTarget=false;State=EnemyState.Investigate;searchUntil=Time.time+searchSeconds;}
        if(State==EnemyState.Investigate&&Time.time<searchUntil){agent.SetDestination(lastKnown);return;}
        if(patrol==null||patrol.Length==0){State=EnemyState.Idle;agent.isStopped=true;return;}
        State=EnemyState.Patrol;patrolIndex%=patrol.Length;
        if((patrol[patrolIndex]-transform.position).sqrMagnitude<2)patrolIndex=(patrolIndex+1)%patrol.Length;
        agent.SetDestination(patrol[patrolIndex]);
    }
}
}
