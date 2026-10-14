using UnityEngine;
namespace Duskline.Slice {
public sealed partial class SliceGame {
 SpriteRenderer coopPartner;LineRenderer coopArc;float coopFlash;
 void SetupCoop(){
  coopPartner=Make("Tag partner attack",motion[0],Vector3.zero,11);coopPartner.enabled=false;
  coopArc=new GameObject("Cross tag impact").AddComponent<LineRenderer>();coopArc.material=new Material(Shader.Find("Sprites/Default"));coopArc.positionCount=32;coopArc.sortingOrder=20;coopArc.widthCurve=new AnimationCurve(new Keyframe(0,.15f),new Keyframe(.4f,1),new Keyframe(1,0));coopArc.enabled=false;
 }
 Sprite CoopPose(int id){return id==3?serinFrames[2]:id==0?combat[0]:tagFrames[(id-1)*4+2];}
 void RenderCoop(){
  bool visible=model.coopTimer>0&&model.place!=Place.Shelter;coopPartner.enabled=visible;
  if(visible){
   coopPartner.sprite=CoopPose(model.coopOutgoing);coopPartner.flipX=model.coopFace<0;
   coopPartner.transform.position=new Vector3(model.coopX-model.coopFace*.9f,-2.6f+model.coopY+.15f,0);
   coopPartner.color=new Color(.68f,.9f,1,Mathf.Min(.9f,model.coopTimer*3));
   if(model.attack<=0&&model.skillTimer<=0&&model.roll<=0){hero.sprite=CoopPose(model.coopIncoming);bladeEdge.enabled=false;}
  }
  coopFlash=Mathf.Max(0,coopFlash-Time.deltaTime);coopArc.enabled=coopFlash>0;
  coopArc.startColor=new Color(.3f,.85f,1,Mathf.Min(1,coopFlash*7));coopArc.endColor=new Color(1,.72f,.3f,Mathf.Min(1,coopFlash*7));coopArc.widthMultiplier=reducedImpact?.025f:.055f;
 }
 void CoopSignal(string value){
  if(value=="coop-start")Toast("협공 · "+SliceModel.HeroNames[model.coopOutgoing]+" × "+model.HeroName);
  if(value!="coop-impact")return;
  coopFlash=.28f;int id=model.coopPulse==1?model.coopOutgoing:model.coopIncoming;float range=id==2?7:id==3?4:3;
  for(int i=0;i<32;i++){float t=i/31f;float yy=(model.coopPulse==1?1-t:t)*1.7f;coopArc.SetPosition(i,new Vector3(model.coopX+model.coopFace*t*range,-2.3f+model.coopY+yy+Mathf.Sin(t*45)*.09f,0));}
  Burst(new Vector3(model.coopX+model.coopFace*2,-1.5f+model.coopY,0),new Color(.3f,.9f,1),30);impactStop=.065f;impactKick=.12f;sound.PlayOneShot(impactClip,.35f);
 }
}
}
