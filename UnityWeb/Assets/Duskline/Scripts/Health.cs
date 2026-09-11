using System;
using UnityEngine;
namespace Duskline {
public sealed class Health : MonoBehaviour {
    [Min(1)] public float maximum=100;
    public float Current {get; private set;}=100;
    public bool Alive => Current>0;
    public event Action Changed, Died;
    public void ResetState(){Current=Mathf.Max(1,maximum); Changed?.Invoke();}
    void Awake(){ResetState();}
    public float Damage(float amount){
        if(!Alive || amount<=0 || float.IsNaN(amount) || float.IsInfinity(amount)) return 0;
        float actual=Mathf.Min(Current,amount); Current-=actual; Changed?.Invoke();
        if(!Alive) Died?.Invoke(); return actual;
    }
    public float Heal(float amount){
        if(!Alive || amount<=0 || float.IsNaN(amount) || float.IsInfinity(amount)) return 0;
        float actual=Mathf.Clamp(amount,0,maximum-Current); Current+=actual; if(actual>0) Changed?.Invoke(); return actual;
    }
}
}
