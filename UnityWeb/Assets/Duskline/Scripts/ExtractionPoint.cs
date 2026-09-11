using UnityEngine;
namespace Duskline {
public sealed class ExtractionPoint : MonoBehaviour,IInteractable {
    public float radius=3.5f;
    public string Prompt=>"[E] 구조 요청 · 구역 안에서 8초 대기";
    public bool Contains(Vector3 p)=>Vector2.Distance(new Vector2(p.x,p.z),new Vector2(transform.position.x,transform.position.z))<=radius&&Mathf.Abs(p.y-transform.position.y)<3;
    public bool CanInteract(PlayerMotor p)=>p&&p.CanAct&&Contains(p.transform.position);
    public void Interact(PlayerMotor p){if(CanInteract(p)&&!RunDirector.Instance.RequestExtraction())RunDirector.Instance.Notify("탈출 불가 · 물자 가치 180 이상 필요 / 대기 중");}
}
}
