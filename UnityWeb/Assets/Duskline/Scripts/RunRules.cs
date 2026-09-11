namespace Duskline {
public enum RunResult { Ready, Running, Escaped, Dead, TimedOut }
public static class RunRules {
    public static RunResult Resolve(RunResult current,RunResult request,bool alive,float now,float deadline){
        if(current!=RunResult.Running || request==RunResult.Running || request==RunResult.Ready)return current;
        if(now>=deadline)return RunResult.TimedOut;
        return alive?request:RunResult.Dead;
    }
    public static bool CanExtract(bool running,bool alive,bool inside,int value,int required,float now,float deadline)=>running&&alive&&inside&&value>=required&&now<deadline;
}
}
