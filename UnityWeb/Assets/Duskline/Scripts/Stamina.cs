using System;
using UnityEngine;
namespace Duskline {
public sealed class Stamina : MonoBehaviour {
    public float maximum=100, recovery=18, delay=1.5f;
    public float Current {get; private set;}=100;
    float delayLeft;
    public event Action Changed;
    public bool Spend(float amount){
        if(amount<0 || float.IsNaN(amount) || float.IsInfinity(amount) || Current<amount) return false;
        Current-=amount; delayLeft=Mathf.Max(0,delay); Changed?.Invoke(); return true;
    }
    public void Step(float dt){
        if(dt<=0 || float.IsNaN(dt) || float.IsInfinity(dt)) return;
        float active=Mathf.Max(0,dt-delayLeft); delayLeft=Mathf.Max(0,delayLeft-dt);
        float before=Current; Current=Mathf.Clamp(Current+active*Mathf.Max(0,recovery),0,maximum); if(before!=Current) Changed?.Invoke();
    }
}
}
