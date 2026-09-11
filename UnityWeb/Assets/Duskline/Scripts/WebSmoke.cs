#if DUSKLINE_SMOKE_TEST
using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.AI;
namespace Duskline {
public sealed class WebSmoke : MonoBehaviour {
    static int checks;
    static bool failed;
    [DllImport("__Internal")] static extern void DusklineTestReport(string report);
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Launch(){if(!failed){var go=new GameObject("WebSmoke");DontDestroyOnLoad(go);go.AddComponent<WebSmoke>();}}
    static bool launched;
    void Awake(){if(launched){Destroy(gameObject);return;}launched=true;Application.logMessageReceived+=Capture;}
    void OnDestroy(){Application.logMessageReceived-=Capture;}
    void Capture(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert){failed=true;Report("FAIL: "+message);}}
    void Check(bool condition,string name){if(!condition)throw new Exception("Web integration failed: "+name);checks++;Report("RUNNING: "+checks+" checks · "+name);}
    void Report(string text){
#if UNITY_WEBGL && !UNITY_EDITOR
        DusklineTestReport(text);
#endif
        Debug.Log("DUSKLINE_WEB_SMOKE "+text);
    }
    void Move(PlayerMotor p,Vector3 position){var c=p.GetComponent<CharacterController>();c.enabled=false;p.transform.position=position;c.enabled=true;Physics.SyncTransforms();}
    IEnumerator Start(){
        if(!launched||gameObject.name!="WebSmoke")yield break;
        yield return new WaitForSeconds(.6f);
        var run=RunDirector.Instance;var world=WorldBootstrap.Instance;var p=run.player;
        Check(run.Result==RunResult.Ready,"ready screen");Check(world.Crates.Count==7&&world.Enemies.Count==3,"map population");
        Check(Physics.Raycast(p.transform.position+Vector3.up,Vector3.down,3,1),"floor collider survives Web stripping");
        Check(NavMesh.SamplePosition(p.transform.position,out var nav,2,NavMesh.AllAreas),"walkable start");
        var path=new NavMeshPath();Check(NavMesh.CalculatePath(p.transform.position,run.exit.transform.position,NavMesh.AllAreas,path)&&path.status==NavMeshPathStatus.PathComplete,"navigation to extraction");
        run.Begin();Check(run.Playing,"begin run");
        yield return new WaitForSeconds(.7f);Check(p.transform.position.y>-.5f,"player remains grounded");
        var crate=world.Crates[0];Move(p,crate.transform.position+Vector3.back*1.8f);crate.Interact(p);crate.Interact(p);
        Check(p.inventory.Count>0&&crate.Empty,"loot transfer");int count=p.inventory.Count;crate.Interact(p);Check(p.inventory.Count==count,"empty container stays empty");
        p.health.Damage(40);Check(p.inventory.UseMedicine(p.health).Success&&p.health.Current==90,"medicine heals and consumes");
        var enemy=world.Enemies[0];Vector3 first=enemy.transform.position;p.noise.Emit(100,8);
        yield return new WaitForSeconds(1.5f);
        Check(enemy.State==EnemyState.Investigate,"noise investigation");Check(Vector3.Distance(first,enemy.transform.position)>.2f,"AI follows NavMesh");
        enemy.enabled=false;enemy.GetComponent<NavMeshAgent>().isStopped=true;
        Move(p,enemy.transform.position+Vector3.back*1.3f);p.transform.rotation=Quaternion.identity;
        var victim=enemy.GetComponent<Health>();float before=victim.Current;
        Check(p.combat.Attack(),"overhead attack starts");Check(victim.Current==before,"windup has no early damage");
        Check(!p.combat.Attack(),"swing cannot retrigger");
        yield return new WaitForSeconds(.32f);Check(victim.Current==before,"raised club still has no damage");
        yield return new WaitForSeconds(.2f);Check(victim.Current==before-p.combat.damage,"downstroke impact damage");
        yield return new WaitForSeconds(.4f);Check(victim.Current==before-p.combat.damage,"single hit per swing");
        enemy.enabled=true;
        foreach(var ai in world.Enemies){ai.GetComponent<Health>().Damage(1000);Check(ai.State==EnemyState.Dead&&!ai.GetComponent<MeleeCombat>().Attack(),"dead AI cannot attack");}
        foreach(var item in Resources.LoadAll<ItemDefinition>("Items"))if(item.kind==ItemKind.Mission)p.inventory.Add(item,3);
        Move(p,run.exit.transform.position+Vector3.back*1.5f);Check(run.RequestExtraction(),"start extraction");Move(p,run.exit.transform.position+Vector3.back*8);
        yield return new WaitForSeconds(.3f);Check(run.ExtractionLeft==0&&run.Result==RunResult.Running,"leaving cancels extraction");
        Move(p,run.exit.transform.position+Vector3.back*1.5f);run.extractionDuration=.3f;Check(run.RequestExtraction(),"retry extraction");
        yield return new WaitForSeconds(.7f);Check(run.Result==RunResult.Escaped&&run.FinalValue>=180&&run.FinalCount>0,"successful extraction totals");
        run.Finish(RunResult.Dead);Check(run.Result==RunResult.Escaped,"terminal result immutable");run.Restart();
        yield return new WaitForSeconds(.7f);run=RunDirector.Instance;p=run.player;
        Check(run.Result==RunResult.Ready&&p.inventory.Count==0&&p.health.Current==100,"restart clears state");run.Begin();p.health.Damage(1000);Check(run.Result==RunResult.Dead,"death failure");run.Restart();
        yield return new WaitForSeconds(.7f);run=RunDirector.Instance;run.timeLimit=.2f;run.Begin();
        yield return new WaitForSeconds(.5f);Check(run.Result==RunResult.TimedOut,"timeout failure");
        if(!failed)Report("PASS: "+checks+" Web integration checks; no runtime errors");
    }
}
}
#endif
