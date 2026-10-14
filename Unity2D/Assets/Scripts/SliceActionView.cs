using System.Collections.Generic;
using UnityEngine;
namespace Duskline.Slice {
public sealed partial class SliceGame {
 sealed class CombatNumber {public Vector3 position;public string text;public float life;public Color color;}
 readonly List<CombatNumber> combatNumbers=new List<CombatNumber>();
 Sprite[] skillFrames;LineRenderer actionWave;AudioClip impactClip,skillClip,evadeClip;
 float actionLife,impactStop,impactKick,perfectBanner;Vector3 actionOrigin;Color actionColor;bool reducedImpact;
 void SetupActionFX(){
  var t=Resources.Load<Texture2D>("SkillActors");skillFrames=new Sprite[6];for(int i=0;i<6;i++)skillFrames[i]=GridFrame(t,3,2,i,180,.5f);
  skillFrames[3]=Sprite.Create(t,new Rect(0,0,536,t.height/2),new Vector2(256f/536,skillFrames[3].pivot.y/(t.height/2)),180);skillFrames[4]=Sprite.Create(t,new Rect(560,0,464,t.height/2),new Vector2(208f/464,skillFrames[4].pivot.y/(t.height/2)),180);
  actionWave=new GameObject("Skill impact wave").AddComponent<LineRenderer>();actionWave.material=new Material(Shader.Find("Sprites/Default"));actionWave.positionCount=49;actionWave.sortingOrder=19;actionWave.enabled=false;
  impactClip=CombatSound("Impact",70,.22f,.7f);skillClip=CombatSound("Skill",160,.3f,.3f);evadeClip=CombatSound("Perfect evade",620,.19f,.08f);
 }
 static AudioClip CombatSound(string name,float frequency,float seconds,float noise){
  int n=(int)(44100*seconds);float[] samples=new float[n];var rng=new System.Random(72);
  for(int i=0;i<n;i++){float t=i/44100f,u=i/(float)n,env=Mathf.Pow(1-u,3);samples[i]=(Mathf.Sin(t*frequency*(1-u*.4f)*Mathf.PI*2)*(1-noise)+(float)(rng.NextDouble()*2-1)*noise)*env*.55f;}
  var clip=AudioClip.Create(name,n,1,44100,false);clip.SetData(samples,0);return clip;
 }
 void ActionSignal(string value){
  if(value=="perfect-dodge"){
   perfectBanner=1.2f;impactStop=.18f;impactKick=.04f;
   Burst(new Vector3(model.x,-1.3f,0),new Color(.4f,.9f,1),28);sound.pitch=1;sound.PlayOneShot(evadeClip,.4f);
  }
  if(value=="skill-start"||value=="ultimate-start"){
   Toast((value=="ultimate-start"?"해방 · ":"스킬 · ")+model.SkillName);sound.pitch=model.activeHero==1?.7f:1.1f;sound.PlayOneShot(skillClip,.35f);
  }
  if(value=="skill-impact"){
   actionLife=model.ultimate?.48f:.32f;actionOrigin=new Vector3(model.x,-1.5f+model.y,0);actionColor=model.ultimate?Amber:model.activeHero==1?new Color(1,.45f,.16f):model.activeHero==2?new Color(.4f,1,.8f):new Color(.65f,.85f,1);
   for(int i=0;i<49;i++){float t=i/48f;Vector3 p;
    if(model.activeHero==3)p=new Vector3(model.face*(.3f+t*4.5f),Mathf.Sin(t*70)*.14f,0);
    else if(model.activeHero==2)p=new Vector3(model.face*(.65f+t*(model.ultimate?8.3f:7.3f)),-.15f,0);
    else if(model.activeHero==1)p=new Vector3(Mathf.Cos(t*Mathf.PI*2)*3.5f,-.85f+Mathf.Sin(t*Mathf.PI*2)*.45f,0);
    else {float a=Mathf.Lerp(-105,110,t)*Mathf.Deg2Rad;p=new Vector3(model.face*Mathf.Cos(a)*3.2f,Mathf.Sin(a)*1.5f,0);}
    actionWave.SetPosition(i,actionOrigin+p);
   }
   Burst(actionOrigin,actionColor,model.ultimate?65:32);impactStop=model.ultimate?.12f:.065f;impactKick=model.ultimate?.24f:.13f;
   sound.pitch=model.activeHero==1?.7f:1;sound.PlayOneShot(impactClip,.45f);
  }
  if(value=="hit"||value=="down"){
   if(combatNumbers.Count>=24)combatNumbers.RemoveAt(0);
   combatNumbers.Add(new CombatNumber{position=new Vector3(model.lastEventX,-.5f,0),text=model.lastDamage.ToString()+(model.counterAttack?"!":""),life=.75f,color=model.counterAttack?Amber:Cream});
   if(model.combo==2||model.counterAttack||value=="down"){impactStop=Mathf.Max(impactStop,.07f);impactKick=.13f;sound.pitch=.85f;sound.PlayOneShot(impactClip,.3f);}
  }
 }
 void RenderActionFX(){
  if(model.skillTimer>0){
   float windup=model.ultimate?.28f:model.activeHero==1?.36f:.12f;
   hero.sprite=model.activeHero==3?serinFrames[model.skillElapsed>=windup?5:4]:skillFrames[model.activeHero+(model.skillElapsed>=windup?3:0)];bladeEdge.enabled=false;
   if(model.activeHero==3&&model.skillElapsed<windup){hero.enabled=false;scoutWindup.enabled=true;scoutWindup.transform.position=hero.transform.position;scoutWindup.transform.localScale=new Vector3(model.face,1,1);}
  }
  cam.orthographicSize=Mathf.Lerp(cam.orthographicSize,!reducedImpact&&impactKick>0?3.67f:3.75f,.2f);
 }
 void ActionEffects(float dt){
  impactStop=Mathf.Max(0,impactStop-dt);impactKick=Mathf.Max(0,impactKick-dt);perfectBanner=Mathf.Max(0,perfectBanner-dt);actionLife=Mathf.Max(0,actionLife-dt);
  actionWave.enabled=false;actionWave.startColor=actionWave.endColor=new Color(actionColor.r,actionColor.g,actionColor.b,Mathf.Min(1,actionLife*4));actionWave.widthMultiplier=(reducedImpact?.025f:.10f)*Mathf.Min(1,actionLife*4);
  if(!reducedImpact&&impactKick>0)cam.transform.position+=new Vector3(Mathf.Sin(Time.unscaledTime*151),Mathf.Cos(Time.unscaledTime*193),0)*impactKick*.25f;
  for(int i=combatNumbers.Count-1;i>=0;i--){var n=combatNumbers[i];n.life-=dt;n.position+=Vector3.up*dt*.7f;if(n.life<=0)combatNumbers.RemoveAt(i);}
 }
 void DrawActionHUD(){
  if(model.place!=Place.Shelter){
   Box(new Rect(443,588,430,48),Panel);
   Box(new Rect(454,596,198,7),new Color(.12f,.2f,.23f));Box(new Rect(454,596,198*model.stamina/100f,7),new Color(.35f,.75f,.8f));
   Box(new Rect(664,596,198,7),new Color(.23f,.19f,.12f));Box(new Rect(664,596,198*model.focus/100f,7),Amber);
   Label(new Rect(454,608,208,24),"K "+(model.skillCooldowns[model.activeHero]>0?model.skillCooldowns[model.activeHero].ToString("0.0")+"초":"스킬 준비"),14,Cream);
   Label(new Rect(664,608,198,24),"L 해방 "+(int)model.focus+"%",14,Amber);
   Box(new Rect(933,125,325,30),Panel);Label(new Rect(949,128,309,26),model.place==Place.Raid?"방어전 · 스킬로 방어문을 지키세요":"토벌 "+Mathf.Min(6,model.runKills)+"/6 · 귀환 보너스 금속8",14,Cream);
   if(model.counterWindow>0){Box(new Rect(470,342,360,38),Panel);Label(new Rect(486,347,340,30),"강화 타격 · J 피해 +2",22,Amber);}
  }
  if(perfectBanner>0){Box(new Rect(430,272,420,56),Panel);Label(new Rect(464,281,372,39),"JUST DODGE · 저스트 회피",25,new Color(.55f,.9f,1));}
  foreach(var n in combatNumbers){var p=cam.WorldToViewportPoint(n.position);if(p.x<0||p.x>1)continue;Label(new Rect(p.x*1280-20,(1-p.y)*720,75,36),n.text,28,n.color);}
  if(reducedImpact)Label(new Rect(945,635,300,18),"F2 · 화면 흔들림 감소",12,Cream);
 }
}
}
