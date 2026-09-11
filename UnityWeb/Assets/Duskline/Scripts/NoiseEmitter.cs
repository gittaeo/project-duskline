using System;
using UnityEngine;
namespace Duskline {
public readonly struct NoiseEvent {
    public readonly Vector3 Position; public readonly float Radius,Duration;
    public NoiseEvent(Vector3 p,float r,float d){Position=p;Radius=r;Duration=d;}
}
public sealed class NoiseEmitter : MonoBehaviour {
    public static event Action<NoiseEvent> Emitted;
    public float crouchRadius=1.5f,walkRadius=6,sprintRadius=19,attackRadius=23,forcedRadius=30;
    public float LastRadius {get; private set;}
    public float LastTime {get; private set;}=-100;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void ResetBus(){Emitted=null;}
    public void Emit(float radius,float duration=5){
        if(radius<=0 || float.IsNaN(radius) || float.IsInfinity(radius))return;
        LastRadius=radius;LastTime=Time.time;Emitted?.Invoke(new NoiseEvent(transform.position,radius,Mathf.Clamp(duration,.1f,30)));
    }
}
}
