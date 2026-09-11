using System;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Duskline {
public sealed class RunDirector : MonoBehaviour {
    public static RunDirector Instance {get; private set;}
    public PlayerMotor player;
    public ExtractionPoint exit;
    public float timeLimit=600, extractionDuration=8;
    public int requiredValue=180;
    public RunResult Result {get; private set;}=RunResult.Ready;
    public bool Paused {get; private set;}
    public bool Playing => Result==RunResult.Running&&!Paused;
    public float Remaining {get; private set;}=600;
    public float ExtractionLeft {get; private set;}
    public int FinalCount {get; private set;}
    public int FinalValue {get; private set;}
    public string Notice {get; private set;}="";
    public event Action Changed;
    float deadline,extractionEnd,noticeEnd;
    void Awake(){Instance=this;Time.timeScale=1;}
    void Start(){player.health.Died+=OnDeath;InvokeRepeating(nameof(Step),.1f,.1f);}
    void OnDestroy(){if(player)player.health.Died-=OnDeath;if(Instance==this)Instance=null;Time.timeScale=1;}
    public void Begin(){if(Result!=RunResult.Ready)return;Result=RunResult.Running;deadline=Time.time+timeLimit;Remaining=timeLimit;Cursor.lockState=CursorLockMode.Locked;Cursor.visible=false;Changed?.Invoke();}
    public void TogglePause(){if(Result!=RunResult.Running)return;Paused=!Paused;Time.timeScale=Paused?0:1;Cursor.lockState=Paused?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=Paused;Changed?.Invoke();}
    void OnApplicationFocus(bool focus){if(!focus&&Playing)TogglePause();}
    void Step(){
        if(!Playing)return;Remaining=Mathf.Max(0,deadline-Time.time);
        if(Time.time>=deadline){Finish(RunResult.TimedOut);return;}
        if(extractionEnd>0){
            if(!player || !player.health.Alive){Finish(RunResult.Dead);return;}
            if(!exit || !exit.Contains(player.transform.position)||player.inventory.Value<requiredValue)CancelExtraction();
            else {ExtractionLeft=Mathf.Max(0,extractionEnd-Time.time);if(Time.time>=extractionEnd){Finish(RunResult.Escaped);return;}}
        }
        if(noticeEnd>0&&Time.time>noticeEnd){Notice="";noticeEnd=0;}
        Changed?.Invoke();
    }
    public bool RequestExtraction(){
        if(!player||!exit||extractionEnd>0||!RunRules.CanExtract(Playing,player.health.Alive,exit.Contains(player.transform.position),player.inventory.Value,requiredValue,Time.time,deadline))return false;
        extractionEnd=Time.time+extractionDuration;ExtractionLeft=extractionDuration;Changed?.Invoke();return true;
    }
    public void CancelExtraction(){if(extractionEnd<=0)return;extractionEnd=0;ExtractionLeft=0;Notify("탈출 취소 · 구역을 벗어났습니다");}
    public void Notify(string text){Notice=text;noticeEnd=Time.time+4;Changed?.Invoke();}
    void OnDeath(){Finish(RunResult.Dead);}
    public void Finish(RunResult request){
        if(Result!=RunResult.Running)return;
        if(request==RunResult.Escaped&&(!exit||!player||!exit.Contains(player.transform.position)||player.inventory.Value<requiredValue))return;
        Result=RunRules.Resolve(Result,request,player&&player.health.Alive,Time.time,deadline);
        if(Result==RunResult.Running)return;
        FinalCount=player?player.inventory.Count:0;FinalValue=player?player.inventory.Value:0;
        extractionEnd=0;ExtractionLeft=0;Paused=false;Time.timeScale=1;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;Changed?.Invoke();
    }
    public void Restart(){Time.timeScale=1;SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);}
}
}
