using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
namespace Duskline.Editor {
[InitializeOnLoad]
public static class PlaySmoke {
    static int stage,passed;
    static double started,next;
    static RunDirector previous;
    static Vector3 initialEnemy;
    static bool hadError;
    static PlaySmoke(){EditorApplication.update+=Update;Application.logMessageReceived+=Log;}
    static void Log(string message,string stack,LogType type){if(SessionState.GetBool("DuskSmoke",false)&&(type==LogType.Exception||type==LogType.Error||type==LogType.Assert)){hadError=true;File.AppendAllText("Validation/PlaySmokeErrors.txt",message+"\n"+stack+"\n");}}
    public static void Run(){BuildTools.Prepare();Directory.CreateDirectory("Validation");EditorSceneManager.OpenScene(BuildTools.ScenePath);SessionState.SetBool("DuskSmoke",true);EditorApplication.EnterPlaymode();}
    static void Check(bool pass,string name){if(!pass)throw new Exception("Play smoke failed: "+name);passed++;Debug.Log("DUSKLINE_PLAY_CHECK: "+name);}
    static void Move(PlayerMotor p,Vector3 position){var c=p.GetComponent<CharacterController>();c.enabled=false;p.transform.position=position;c.enabled=true;Physics.SyncTransforms();}
    static void Update(){
        if(!SessionState.GetBool("DuskSmoke",false)||!EditorApplication.isPlaying)return;
        if(started==0){started=EditorApplication.timeSinceStartup;next=started+1;}
        try{
            if(EditorApplication.timeSinceStartup-started>65)throw new Exception("Play smoke timeout");
            if(EditorApplication.timeSinceStartup<next)return;
            var run=RunDirector.Instance;var world=WorldBootstrap.Instance;if(!run||!world||!run.player)return;
            var p=run.player;
            switch(stage){
                case 0:
                    Check(run.Result==RunResult.Ready,"ready screen");Check(world.Crates.Count==7&&world.Enemies.Count==3,"map populations");
                    Check(NavMesh.SamplePosition(p.transform.position,out var nav,2,NavMesh.AllAreas),"walkable start");
                    var path=new NavMeshPath();Check(NavMesh.CalculatePath(p.transform.position,run.exit.transform.position,NavMesh.AllAreas,path)&&path.status==NavMeshPathStatus.PathComplete,"route to extraction");
                    run.Begin();Check(run.Playing,"run starts");
                    var crate=world.Crates[0];Move(p,crate.transform.position+Vector3.back*1.8f);crate.Interact(p);crate.Interact(p);Check(p.inventory.Count>0&&crate.Empty,"loot transfer");int count=p.inventory.Count;crate.Interact(p);Check(p.inventory.Count==count,"empty no regeneration");
                    p.health.Damage(40);Check(p.inventory.UseMedicine(p.health).Success&&p.health.Current==90,"medicine gameplay");
                    initialEnemy=world.Enemies[0].transform.position;p.noise.Emit(100,8);
                    stage=1;next=EditorApplication.timeSinceStartup+1.5;break;
                case 1:
                    Check(world.Enemies[0].State==EnemyState.Investigate,"noise investigation");Check(Vector3.Distance(initialEnemy,world.Enemies[0].transform.position)>.2f,"navigation movement");
                    foreach(var enemy in world.Enemies){enemy.GetComponent<Health>().Damage(1000);Check(enemy.State==EnemyState.Dead&&!enemy.GetComponent<MeleeCombat>().Attack(),"dead AI cannot attack");}
                    foreach(var item in Resources.LoadAll<ItemDefinition>("Items"))if(item.kind==ItemKind.Mission)p.inventory.Add(item,3);
                    Move(p,run.exit.transform.position+Vector3.back*1.5f);Check(run.RequestExtraction(),"valid extraction request");
                    Move(p,run.exit.transform.position+Vector3.back*8);stage=2;next=EditorApplication.timeSinceStartup+.3;break;
                case 2:
                    Check(run.ExtractionLeft==0&&run.Result==RunResult.Running,"exit cancels countdown");
                    Move(p,run.exit.transform.position+Vector3.back*1.5f);run.extractionDuration=.3f;Check(run.RequestExtraction(),"extraction can restart");stage=3;next=EditorApplication.timeSinceStartup+.7;break;
                case 3:
                    Check(run.Result==RunResult.Escaped&&run.FinalValue>=180&&run.FinalCount>0,"successful extraction totals");run.Finish(RunResult.Dead);Check(run.Result==RunResult.Escaped,"result cannot change");previous=run;run.Restart();stage=4;next=EditorApplication.timeSinceStartup+1;break;
                case 4:
                    if(run==previous)return;Check(run.Result==RunResult.Ready&&p.inventory.Count==0&&p.health.Current==100,"fresh restart state");run.Begin();p.health.Damage(1000);Check(run.Result==RunResult.Dead,"death result");previous=run;run.Restart();stage=5;next=EditorApplication.timeSinceStartup+1;break;
                case 5:
                    if(run==previous)return;run.timeLimit=.2f;run.Begin();stage=6;next=EditorApplication.timeSinceStartup+.6;break;
                case 6:
                    Check(run.Result==RunResult.TimedOut,"timeout result");Check(!hadError,"no runtime errors");File.WriteAllText("Validation/PlaySmoke.json",$"{{\"passed\":{passed},\"failed\":0,\"unity\":\"{Application.unityVersion}\"}}");SessionState.SetBool("DuskSmoke",false);Debug.Log("DUSKLINE_PLAY_SMOKE_OK "+passed);EditorApplication.Exit(0);break;
            }
        }catch(Exception e){File.WriteAllText("Validation/PlaySmoke.json","{\"failed\":1}");SessionState.SetBool("DuskSmoke",false);Debug.LogException(e);EditorApplication.Exit(1);}
    }
}
}
