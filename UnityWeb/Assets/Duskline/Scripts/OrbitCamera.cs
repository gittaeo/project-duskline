using UnityEngine;
namespace Duskline {
public sealed class OrbitCamera : MonoBehaviour {
    public Transform target;
    public float distance=4.5f,sensitivity=2;
    public float Yaw {get;private set;}=30;
    float pitch=19;
    void LateUpdate(){
        if(!target)return;
        if(RunDirector.Instance&&RunDirector.Instance.Playing&&Cursor.lockState==CursorLockMode.Locked){Yaw+=Input.GetAxisRaw("Mouse X")*sensitivity;pitch=Mathf.Clamp(pitch-Input.GetAxisRaw("Mouse Y")*sensitivity,-25,65);}
        var rotation=Quaternion.Euler(pitch,Yaw,0);Vector3 pivot=target.position+Vector3.up*1.5f;
        float range=distance;if(Physics.SphereCast(pivot,.2f,rotation*Vector3.back,out var hit,distance,1,QueryTriggerInteraction.Ignore))range=Mathf.Max(.35f,hit.distance-.1f);
        transform.SetPositionAndRotation(pivot+rotation*Vector3.back*range,rotation);
    }
}
}
