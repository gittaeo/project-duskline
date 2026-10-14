using UnityEngine;
namespace Duskline.Slice {
public sealed partial class SliceGame {
 Sprite[][] walkEight;readonly LineRenderer[] flourish=new LineRenderer[6];
 float flourishLife,flourishDuration;int flourishHero;float flourishFace;bool flourishUltimate;Vector3 flourishOrigin;
 void SetupMotionV09(){
  walkEight=new Sprite[4][];string[] names={"IhyeonWalk8","DoyunWalk8","HarinWalk8","SerinWalk8"};
  for(int id=0;id<4;id++){
   var t=Resources.Load<Texture2D>(names[id]);walkEight[id]=new Sprite[8];
   for(int i=0;i<8;i++)walkEight[id][i]=GridFrame(t,4,2,i,t.height*new[]{.40f,.46f,.46f,.43f}[id]/2.05f,.5f);
  }
  for(int i=0;i<6;i++){
   var lr=new GameObject("Skill flourish "+i).AddComponent<LineRenderer>();lr.material=new Material(Shader.Find("Sprites/Default"));lr.positionCount=40;lr.sortingOrder=17+i;lr.useWorldSpace=true;
   lr.widthCurve=new AnimationCurve(new Keyframe(0,.05f),new Keyframe(.3f,1),new Keyframe(.8f,.7f),new Keyframe(1,.02f));lr.enabled=false;flourish[i]=lr;
  }
 }
 int WalkFrame(){float stride=model.activeHero==1?2.7f:model.activeHero==3?2.2f:2.45f;return Mathf.FloorToInt(model.walkPhase/2.2f/stride*8)%8;}
 void RenderWalkV09(){
  hero.transform.rotation=Quaternion.identity;
  if(model.y>.01f||model.landing>0||model.attack>0||model.roll>0||model.skillTimer>0||model.crouching||Mathf.Abs(model.velocityX)<.15f)return;
  int frame=WalkFrame();
  hero.sprite=walkEight[model.activeHero][frame];bladeEdge.enabled=false;
 }
 void MotionSignal(string value){
  if(value!="skill-impact")return;
  flourishHero=model.activeHero;flourishFace=model.face;flourishUltimate=model.ultimate;
  flourishDuration=flourishLife=model.ultimate?.6f:.4f;flourishOrigin=new Vector3(model.x,-1.5f+model.y,0);
 }
 void RenderSkillMotion(){
  if(model.skillTimer<=0)return;
  float windup=model.ultimate?.28f:model.activeHero==1?.36f:.12f;
  float t=model.skillElapsed,after=t-windup;
  Sprite ready=model.activeHero==3?serinFrames[3]:model.activeHero==0?motion[5]:tagFrames[(model.activeHero-1)*4+3];
  if(t<windup*.35f){hero.sprite=ready;hero.enabled=true;scoutWindup.enabled=false;}
  if(after>=0){
   float decay=Mathf.Clamp01(1-after/.33f);
   if(model.activeHero==0){hero.sprite=after<.09f?skillFrames[3]:after<.2f?combat[4]:combat[5];hero.transform.rotation=Quaternion.Euler(0,0,model.face*Mathf.Lerp(-10,4,Mathf.Clamp01(after/.32f)));}
   else if(model.activeHero==1){hero.transform.localScale=new Vector3(1+decay*.035f,1-decay*.025f,1);}
   else if(model.activeHero==2){float recoil=Mathf.Max(0,1-(after% .16f)/.1f);hero.transform.position+=new Vector3(-model.face*recoil*.10f,recoil*.025f,0);hero.transform.rotation=Quaternion.Euler(0,0,model.face*recoil*3);}
   else {hero.transform.rotation=Quaternion.Euler(0,0,model.face*decay*-5);}
   if(model.activeHero==3&&model.skillTimer>=.10f){hero.enabled=false;scoutLunge.enabled=true;scoutLunge.transform.position=hero.transform.position;scoutLunge.transform.rotation=hero.transform.rotation;scoutLunge.transform.localScale=new Vector3(model.face,1,1);}
   if(model.skillTimer<.10f){hero.sprite=model.activeHero==0?motion[0]:model.activeHero==3?serinFrames[0]:tagFrames[(model.activeHero-1)*4];hero.transform.rotation=Quaternion.identity;}
  }else{
   float lean=Mathf.Sin(Mathf.Clamp01(t/windup)*Mathf.PI)*3;hero.transform.rotation=Quaternion.Euler(0,0,model.face*lean);scoutWindup.transform.rotation=hero.transform.rotation;
  }
 }
 void MotionEffects(float dt){
  flourishLife=Mathf.Max(0,flourishLife-dt);
  bool charging=model.skillTimer>0&&model.skillPulse==0;
  for(int layer=0;layer<6;layer++){
   var lr=flourish[layer];bool chargeLayer=layer==0&&charging;
   lr.enabled=chargeLayer||flourishLife>0;if(!lr.enabled)continue;
   float u=chargeLayer?Mathf.Clamp01(model.skillElapsed/(model.ultimate?.28f:model.activeHero==1?.36f:.12f)):1-flourishLife/flourishDuration;
   int id=chargeLayer?model.activeHero:flourishHero;float face=chargeLayer?model.face:flourishFace;
   Vector3 origin=chargeLayer?new Vector3(model.x,-1.5f+model.y,0):flourishOrigin;
   Color c=id==0?new Color(.75f,.88f,1):id==1?new Color(1,.48f,.12f):id==2?new Color(.4f,1,.74f):new Color(.12f,.8f,1);
   if(flourishUltimate&&!chargeLayer)c=Color.Lerp(c,Amber,.35f);
   float fade=chargeLayer?.65f:Mathf.Pow(1-u,1.5f);c.a=fade*(layer>3?.42f:.8f);lr.startColor=c;lr.endColor=new Color(c.r,c.g,c.b,0);
   lr.widthMultiplier=(reducedImpact?.025f:layer==1?.14f:.055f)*(chargeLayer?1:1-u);
   for(int i=0;i<40;i++){
    float p=i/39f;Vector3 v;
    if(chargeLayer){float a=p*Mathf.PI*2;float radius=1.15f-u*.55f;v=new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius*.5f-.25f,0);}
    else if(id==0){float a=Mathf.Lerp(-120,115,p)*Mathf.Deg2Rad+u*.7f;float radius=1.3f+u*2.5f+layer*.13f;v=new Vector3(face*Mathf.Cos(a)*radius,Mathf.Sin(a)*radius*.6f,0);}
    else if(id==1){float a=p*Mathf.PI*2;float radius=.4f+u*4+layer*.22f;v=new Vector3(Mathf.Cos(a)*radius,-.93f+Mathf.Sin(a)*radius*.16f+Mathf.Sin(p*30+layer)*.08f*(1-u),0);}
    else if(id==2){v=new Vector3(face*(.6f+p*7.3f),-.08f+(layer-2.5f)*.055f+Mathf.Sin(p*Mathf.PI)*.12f*(layer-2),0);}
    else {float wave=Mathf.Sin(p*55+layer*2+u*16)*.16f*(1-p);v=new Vector3(face*(.3f+p*4.7f),wave+(layer-2.5f)*.11f*Mathf.Sin(p*Mathf.PI),0);}
    lr.SetPosition(i,origin+v);
   }
  }
 }
}
}
