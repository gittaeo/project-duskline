using UnityEngine;
namespace Duskline {
[RequireComponent(typeof(CharacterController),typeof(Health),typeof(Stamina))]
public sealed class PlayerMotor : MonoBehaviour {
    public float walkSpeed=3.2f,sprintSpeed=6,crouchSpeed=1.5f,sprintDrain=24,jumpHeight=1.2f;
    public Health health;public Stamina stamina;public Inventory inventory;public NoiseEmitter noise;public MeleeCombat combat;
    public OrbitCamera view;
    public Transform body;
    public bool Crouched {get;private set;}
    public bool Sprinting {get;private set;}
    public string InteractionPrompt {get;private set;}="";
    public string MovementName=>Crouched?"은신":Sprinting?"달리기":"걷기";
    CharacterController motor;MonoBehaviour focused;float vertical,stepAt,traceAt;bool exhausted;
    public bool CanAct=>health&&health.Alive&&RunDirector.Instance&&RunDirector.Instance.Playing;
    void Awake(){motor=GetComponent<CharacterController>();health=GetComponent<Health>();stamina=GetComponent<Stamina>();inventory=GetComponent<Inventory>();noise=GetComponent<NoiseEmitter>();combat=GetComponent<MeleeCombat>();}
    void Update(){
        var run=RunDirector.Instance;if(!run)return;
        if(Input.GetKeyDown(KeyCode.Escape)&&run.Result==RunResult.Running)run.TogglePause();
        if(!CanAct){Sprinting=false;return;}
        stamina.Step(Time.deltaTime);
        if(Input.GetKeyDown(KeyCode.LeftControl)||Input.GetKeyDown(KeyCode.C)){
            if(!Crouched){Crouched=true;motor.height=1.15f;motor.center=Vector3.up*.575f;}
            else if(!Physics.CheckCapsule(transform.position+Vector3.up*.45f,transform.position+Vector3.up*1.65f,.3f,1)){Crouched=false;motor.height=1.8f;motor.center=Vector3.up*.9f;}
        }
        if(body)body.localScale=new Vector3(1,Crouched?.64f:1,1);
        Vector2 input=new Vector2((Input.GetKey(KeyCode.D)?1:0)-(Input.GetKey(KeyCode.A)?1:0),(Input.GetKey(KeyCode.W)?1:0)-(Input.GetKey(KeyCode.S)?1:0));input=Vector2.ClampMagnitude(input,1);
        if(!Input.GetKey(KeyCode.LeftShift))exhausted=false;
        Sprinting=input.sqrMagnitude>.01f&&!Crouched&&!exhausted&&Input.GetKey(KeyCode.LeftShift)&&motor.isGrounded;
        if(Sprinting&&!stamina.Spend(sprintDrain*Time.deltaTime)){Sprinting=false;exhausted=true;}
        Vector3 move=Quaternion.Euler(0,view.Yaw,0)*new Vector3(input.x,0,input.y);
        if(move.sqrMagnitude>.01f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(move),Time.deltaTime*15);
        if(motor.isGrounded&&vertical<0)vertical=-2;
        if(Input.GetKeyDown(KeyCode.Space)&&motor.isGrounded&&!Crouched)vertical=Mathf.Sqrt(jumpHeight*24);
        vertical-=24*Time.deltaTime;
        motor.Move((move*(Crouched?crouchSpeed:Sprinting?sprintSpeed:walkSpeed)+Vector3.up*vertical)*Time.deltaTime);
        if(move.sqrMagnitude>.01f&&motor.isGrounded&&Time.time>=stepAt){noise.Emit(Crouched?noise.crouchRadius:Sprinting?noise.sprintRadius:noise.walkRadius,Sprinting?5:2);stepAt=Time.time+(Sprinting?.38f:.68f);}
        if(Time.time>=traceAt){Trace();traceAt=Time.time+.08f;}
        if(Input.GetKeyDown(KeyCode.E)){Trace();if(focused&&focused is IInteractable target&&target.CanInteract(this))target.Interact(this);}
        if(Input.GetKeyDown(KeyCode.Q)){var r=inventory.UseMedicine(health);run.Notify(r.Success?"응급 처치 · 의약품 1개 사용":r.Reason);}
        if(Input.GetMouseButtonDown(0)){transform.rotation=Quaternion.Euler(0,view.Yaw,0);if(!combat.Attack())run.Notify("공격 대기 또는 스태미나 부족");}
        if(run.ExtractionLeft>0&&run.exit&&!run.exit.Contains(transform.position))run.CancelExtraction();
    }
    void Trace(){
        focused=null;InteractionPrompt="";if(!view)return;
        if(Physics.Raycast(view.transform.position,view.transform.forward,out var hit,7,~((1<<8)|(1<<10)),QueryTriggerInteraction.Ignore)){
            if(Vector3.Distance(transform.position,hit.point)>3.2f)return;
            foreach(var candidate in hit.collider.GetComponentsInParent<MonoBehaviour>())if(candidate is IInteractable target){focused=candidate;InteractionPrompt=target.Prompt;break;}
        }
    }
}
}
