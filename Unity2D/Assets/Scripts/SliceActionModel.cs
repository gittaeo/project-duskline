using System;
namespace Duskline.Slice {
public sealed partial class SliceModel {
 public float stamina=100,focus,counterWindow,skillTimer,skillElapsed;
 public readonly float[] skillCooldowns=new float[4];
 public int skillPulse,runKills,lastDamage;public bool ultimate,counterAttack;
 public string SkillName=>ultimate?"황혼 해방":new[]{"섬광 돌파","철쇄 진동","집중 사격","뇌광 관통"}[activeHero];
 public bool Skill(bool finisher=false){
  if(place==Place.Shelter||attack>0||roll>0||skillTimer>0||hp<=0)return false;
  if(finisher){if(focus<100)return false;focus=0;}
  else {if(stamina<20||skillCooldowns[activeHero]>0)return false;stamina-=20;skillCooldowns[activeHero]=activeHero==3?5:activeHero==1?8:activeHero==2?7:6;}
  ultimate=finisher;skillTimer=finisher?.9f:activeHero==1?.85f:.65f;skillElapsed=0;skillPulse=0;
  crouching=false;queued=false;comboGrace=0;velocityX=0;counterAttack=false;
  Emit(finisher?"ultimate-start":"skill-start",x);return true;
 }
 void ResetAction(){
  stamina=100;focus=counterWindow=skillTimer=skillElapsed=0;skillPulse=0;counterAttack=ultimate=false;
  for(int i=0;i<skillCooldowns.Length;i++)skillCooldowns[i]=0;
 }
 void TickAction(float dt){
  stamina=Math.Min(100,stamina+dt*22);counterWindow=Math.Max(0,counterWindow-dt);
  for(int i=0;i<skillCooldowns.Length;i++)skillCooldowns[i]=Math.Max(0,skillCooldowns[i]-dt);
  if(skillTimer<=0)return;
  skillTimer=Math.Max(0,skillTimer-dt);skillElapsed+=dt;
  float windup=ultimate?.28f:activeHero==1?.36f:.12f;
  int pulses=activeHero==2&&!ultimate?3:1;
  while(skillPulse<pulses&&skillElapsed>=windup+skillPulse*.16f){skillPulse++;SpecialImpact();}
 }
 void SpecialImpact(){
  if(activeHero==0||activeHero==3)x=Math.Max(Left,Math.Min(Right,x+face*(activeHero==3?2.3f:1.5f)));
  float range=ultimate?(activeHero==2?9:5):activeHero==3?4.5f:activeHero==2?8:3.8f;
  range+=SignatureRank*.35f;
  int damage=ultimate?8+WeaponPower(activeHero)*2:activeHero==2?WeaponPower(activeHero):4+WeaponPower(activeHero);
  Emit("skill-impact",x);
  foreach(var e in enemies)if(e.hp>0&&e.delay<=0&&Math.Abs(e.x-x)<range&&(activeHero==1||(e.x-x)*face>-.4f)&&Math.Abs(e.y-y)<2.2f){
   lastDamage=damage;e.hp-=damage;e.hurt=e.boss?.1f:activeHero==3?1.1f:.65f;if(!e.boss||ultimate)e.attack=e.dash=0;e.cooldown=.65f;
   e.x=Math.Max(Left,Math.Min(Right,e.x+Math.Sign(e.x-x)*.5f));if(!e.brute&&activeHero!=2)e.vy=4;
   RegisterHit(e);Emit(e.hp<=0?"down":"hit",e.x);
  }
 }
 void RegisterHit(Enemy e){
  if(!ultimate||skillTimer<=0)focus=Math.Min(100,focus+6+(e.hp<=0?10:0));
  if(e.hp<=0&&place==Place.Expedition){runKills++;if(e.boss){lastEventX=e.x;DefeatBoss();}}
 }
 void CheckPerfectDodge(){
  foreach(var e in enemies)if(e.hp>0&&e.delay<=0&&!e.gateTarget&&Math.Abs(e.x-x)<(e.brute?2.7f:3.2f)&&((e.attack>0&&e.attack<.19f)||e.dash>0)){
   counterWindow=3;focus=Math.Min(100,focus+25);Emit("perfect-dodge",x);return;
  }
 }
}
}
