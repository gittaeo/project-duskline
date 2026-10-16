using System;
using System.Collections.Generic;
namespace Duskline.Slice {
public enum Place { Shelter, Expedition, Raid }
[Serializable] public sealed partial class Progress {
 public int version=13,chapter,water,bandages,boards,metal,expeditions,weapon=1,gate=100;public bool raidPending,prologueSeen;
 public void Validate(){version=13;chapter=Math.Max(0,Math.Min(10,chapter));water=Math.Max(0,water);bandages=Math.Max(0,bandages);boards=Math.Max(0,boards);metal=Math.Max(0,Math.Min(99999,metal));expeditions=Math.Max(0,expeditions);weapon=Math.Max(1,Math.Min(6,weapon));gate=Math.Max(0,Math.Min(100,gate));ValidateSignatures();loreMask&=127;ValidateExpansion();ValidateSurvival();sorties=Math.Max(0,sorties);cores=Math.Max(0,cores);bossKills=Math.Max(0,bossKills);}
}
public sealed partial class SliceModel {
 public sealed class Enemy { public float x,y,vy,attack,hurt,cooldown,memory,delay,dash,direction=-1;public int hp=4,maxHp=4;public bool runner,brute,boss,spitter,enraged,gateTarget,hit; }
 public sealed class Supply {public float x;public int amount;public bool opened;}
 public readonly List<Enemy> enemies=new List<Enemy>();
 public readonly List<Supply> supplies=new List<Supply>();
 public readonly Progress progress;
 public Place place=Place.Shelter;
 public float x,y,vy,face=1,attack,attackElapsed,comboGrace,invulnerable,roll,rollCooldown,time,raidWarning=-1,lastEventX;
 public int hp=100,carried,combo;public bool attackHit,queued,drill;
 public bool crouching,rescued;public float landing,walkPhase,velocityX;
 public int missionMask,runWater,runBandages,runBoards,runFood;
 public string MissionTitle=>new[]{"1-1 빈 그릇","1-2 철을 두드리는 사람","1-3 약국의 마지막 환자","2-1 구조의 값","3-1 남겨 둔 씨앗","3-2 오염된 배수관","4-1 난간 위의 거래","4-2 빛바랜 주황","4-3 털린 비축처","4-4 갚아야 할 물자","5장 진입 · 성채의 배급표"}[progress.chapter];
 public bool MissionReady=>progress.chapter==0?(missionMask&7)==7:progress.chapter<3?rescued:progress.chapter==3?(missionMask&32)!=0:progress.chapter==4?rescued:progress.chapter==5?(missionMask&32)!=0:progress.chapter==6?(missionMask&8)!=0:progress.chapter==7?(missionMask&64)!=0&&bossDefeated:progress.chapter==8?(missionMask&40)==40:progress.chapter==9?(missionMask&68)==68&&bossDefeated:false;
 int gateBeforeDrill;float attackLock;
 public event Action<string> Signal;public event Action Save;
 static readonly float[] Durations={.38f,.46f,.60f},Windups={.13f,.18f,.25f};
 public float Windup=>Windups[combo]*(activeHero==1?1.35f:activeHero==2?.65f:1);
 public float Left=>place==Place.Shelter?-10.5f:-7f;
 public float Right=>place==Place.Expedition?90.6f:place==Place.Shelter?10.5f:7f;
 public string ComboName=>activeHero==3?new[]{"전격 찌르기","연속 관통","뇌광 쓸기"}[combo]:activeHero==1?new[]{"강철 주먹","분쇄 올려치기","지면 강타"}[combo]:activeHero==2?new[]{"조준 사격","연속 사격","관통 사격"}[combo]:new[]{"찌르기","올려치기","내려치기"}[combo];
 public SliceModel(Progress data=null){progress=data??new Progress();progress.Validate();hp=progress.vitality;if(progress.raidPending)raidWarning=12;}
 void Emit(string value,float at){lastEventX=at;Signal?.Invoke(value);}
 void ResetCombat(){coopTimer=coopElapsed=0;coopPulse=0;tagBuffer=0;acids.Clear();ResetAction();y=vy=attack=attackElapsed=comboGrace=roll=rollCooldown=attackLock=0;queued=attackHit=crouching=false;velocityX=landing=0;combo=0;invulnerable=.5f;}
 public bool Depart(){
  if(place!=Place.Shelter||progress.raidPending||restTimer>0)return false;
  place=Place.Expedition;shelterFloor=1;x=-5.4f;face=1;carryingBlueprint=false;runLoreMask=0;carried=missionMask=runWater=runBandages=runBoards=runFood=0;rescued=false;runKills=0;ResetCombat();enemies.Clear();supplies.Clear();
  float[] positions={3.8f,9,15,22,26,33,39,44,49,54,63,70,78,86};
  for(int i=0;i<positions.Length;i++)enemies.Add(new Enemy{x=positions[i],runner=i%3==2&&i<10,brute=i>=10&&i%2==0,hp=i>=10&&i%2==0?14:i%3==2?5:4});
  float[] locations={.2f,16,31,47,55,69,86};int[] amounts={10,12,14,18,24,30,36};
  for(int i=0;i<locations.Length;i++)supplies.Add(new Supply{x=locations[i],amount=amounts[i]});
  PrepareRegion();Emit("depart",x);return true;
 }
 public bool StartRaid(bool practice=false){
  if(place!=Place.Shelter||(!practice&&!progress.raidPending))return false;
  if(restTimer>0)return false;healthBeforeDrill=hp;drill=practice;gateBeforeDrill=progress.gate;if(practice)progress.gate=100;
  place=Place.Raid;shelterFloor=1;x=-3;if(practice)hp=100;raidWarning=-1;ResetCombat();enemies.Clear();supplies.Clear();
  for(int i=0;i<6;i++)enemies.Add(new Enemy{x=6.6f,delay=i*3.2f,runner=i%3==2&&i<10,brute=i>=10&&i%2==0,hp=i>=10&&i%2==0?14:i%3==2?5:4});
  Emit(practice?"drill":"raid",x);return true;
 }
 void Home(string reason){
  place=Place.Shelter;shelterFloor=1;x=0;hp=Math.Max(1,hp);progress.vitality=hp;enemies.Clear();supplies.Clear();ResetCombat();
  if(progress.raidPending)raidWarning=12;
  Emit(reason,x);Save?.Invoke();
 }
 public bool ReturnHome(bool success){
  if(place!=Place.Expedition)return false;
  bool bankedBlueprint=success&&carryingBlueprint&&!progress.resonanceBlueprint;if(bankedBlueprint)progress.resonanceBlueprint=true;carryingBlueprint=false;
  if(success)progress.loreMask|=runLoreMask;runLoreMask=0;
  bool advance=success&&MissionReady;bool productive=success&&carried>0;int completed=progress.chapter;int previousHarvests=progress.harvests;
  if(success&&bossDefeated){progress.cores++;progress.bossKills++;}
  if(success&&runKills>=6){carried+=8;Emit("bounty",x);}
  progress.metal+=success?carried:carried*7/10;carried=0;
  progress.water+=success?runWater:runWater*7/10;progress.bandages+=success?runBandages:runBandages*7/10;progress.boards+=success?runBoards:runBoards*7/10;
  progress.food=Math.Min(50,progress.food+(success?runFood:runFood*7/10));
  if(advance){progress.chapter++;if(completed==8){progress.water=Math.Max(0,progress.water-4);progress.food=Math.Max(0,progress.food-3);}if(completed==9){progress.water+=4;progress.food=Math.Min(50,progress.food+6);progress.relics++;}if(completed==3){progress.water=Math.Max(0,progress.water-runWater);progress.medicine+=2;progress.food=Math.Max(0,progress.food-2);}if(completed==4){progress.seeds+=3;progress.cropFailed=true;}}
  if(productive)SettleShelter();
  if(success){progress.expeditions++;if(progress.expeditions%3==0)progress.raidPending=true;}
  Home(success?"extract":"failed");if(bankedBlueprint)Emit("blueprint-banked",x);if(advance)Emit("story-"+progress.chapter,x);if(previousHarvests==0&&progress.harvests>0)Emit("first-harvest",x);return true;
 }
 public bool Upgrade(){
  int cost=12*progress.weapon;if(place!=Place.Shelter||progress.weapon>=6||progress.metal<cost)return false;
  if(progress.weapon>=3&&progress.cores<1){Emit("upgrade-core",x);return false;}if(progress.weapon>=3)progress.cores--;
  progress.metal-=cost;progress.weapon++;Emit("upgrade",x);Save?.Invoke();return true;
 }
 public bool Repair(){
  if(place!=Place.Shelter||progress.metal<5||progress.gate>=100)return false;
  progress.metal-=5;progress.gate=Math.Min(100,progress.gate+30);Emit("repair",x);Save?.Invoke();return true;
 }
 void BeginAttack(int step){counterAttack=counterWindow>0;counterWindow=0;ultimate=false;crouching=false;combo=step;attack=Durations[step]*(activeHero==1?1.25f:activeHero==2?.8f:1);attackElapsed=0;attackHit=false;queued=false;comboGrace=0;Emit("swing",x);}
 public void Tick(float dt,float move,bool jump,bool strike,bool dodge,bool interact,bool crouch=false){
  dt=Math.Max(0,Math.Min(dt,.05f));time+=dt;if(TickRest(dt))return;if(!drill)progress.vitality=Math.Max(1,hp);TickCoop(dt);TickAction(dt);healCooldown=Math.Max(0,healCooldown-dt);tagCooldown=Math.Max(0,tagCooldown-dt);landing=Math.Max(0,landing-dt);crouching=crouch&&y<=0&&attack<=0&&roll<=0&&skillTimer<=0;
  invulnerable=Math.Max(0,invulnerable-dt);rollCooldown=Math.Max(0,rollCooldown-dt);attackLock=Math.Max(0,attackLock-dt);comboGrace=Math.Max(0,comboGrace-dt);
  if(place==Place.Shelter&&raidWarning>=0){raidWarning-=dt;if(raidWarning<=0)StartRaid();}
  if(Math.Abs(move)>.01f&&roll<=0&&attack<=0)face=Math.Sign(move);
  if(dodge&&rollCooldown<=0&&(attack<=0||attackHit)&&skillTimer<=0&&stamina>=20&&y<.01f){stamina-=20;CheckPerfectDodge();attack=0;queued=false;comboGrace=0;roll=.27f;rollCooldown=.65f;invulnerable=.33f;Emit("roll",x);}
  if(strike&&place!=Place.Shelter&&roll<=0&&skillTimer<=0){
   if(attack>0&&attackElapsed>.06f&&combo<2)queued=true;
   else if(attack<=0&&attackLock<=0){BeginAttack(y>.3f?2:comboGrace>0&&combo<2?combo+1:0);if(y>.3f)vy=-8;}
  }
  if(jump&&y<=0&&roll<=0&&skillTimer<=0){crouching=false;landing=0;vy=6;Emit("jump",x);}
  roll=Math.Max(0,roll-dt);
  float targetSpeed=(skillTimer>0?0:move)*(crouching?1.0f:attack>0?.7f:3.2f);
  float delta=targetSpeed-velocityX;velocityX+=Math.Sign(delta)*Math.Min(Math.Abs(delta),dt*24);
  float speed=roll>0?face*7:velocityX;
  float previousX=x;x=Math.Max(Left,Math.Min(Right,x+speed*dt));if(y<=.01f&&roll<=0&&skillTimer<=0)walkPhase+=Math.Abs(x-previousX)*2.2f;
  float previousY=y;vy-=18*dt;y=Math.Max(0,y+vy*dt);if(y==0){if(previousY>0){landing=.13f;Emit("land",x);}vy=0;}
  if(attack>0){
   attack-=dt;attackElapsed+=dt;
   if(!attackHit&&attackElapsed>=Windup){
    attackHit=true;x=Math.Max(Left,Math.Min(Right,x+face*(combo==0?.30f:.12f)));Emit("slash",x);
    foreach(var e in enemies)if(e.hp>0&&e.delay<=0&&Math.Abs(e.x-x)<(activeHero==3?3.4f:activeHero==2?7f:activeHero==1?2.7f:combo==0?1.9f:2.2f)&&(e.x-x)*face>-.3f&&Math.Abs(e.y-y)<1.4f){
     lastDamage=Math.Max(1,(combo==2?2:1)+(WeaponPower(activeHero)-1)+(activeHero==1?1:0)-(e.brute&&activeHero!=1?1:0)+(counterAttack?2:0));e.hp-=lastDamage;e.hurt=e.boss?.06f:combo==2?.55f:.3f;if(!e.boss)e.attack=e.dash=0;e.cooldown=.35f;
     e.x=Math.Max(Left,Math.Min(Right,e.x+face*(activeHero==1?1.2f:combo==2?.8f:.32f)));if(combo==1&&!e.brute)e.vy=5;
     RegisterHit(e);Emit(e.hp<=0?"down":"hit",e.x);
    }
   }
   if(attack<=0){attack=0;if(queued&&combo<2)BeginAttack(combo+1);else if(combo==2){comboGrace=0;attackLock=.16f;}else comboGrace=.32f;}
  }
  TickTag(dt);if(place!=Place.Shelter)UpdateEnemies(dt);
  if(interact&&!TalkWitness()){
   if(place==Place.Shelter){
    if(shelterFloor>1){UseFacility();}
    else if(x>8){if(progress.raidPending)StartRaid();else Depart();}
    else if(x<-5.5f){if(!Upgrade())Emit(progress.weapon>=6?"max-level":progress.weapon>=3&&progress.cores<1?"upgrade-core":"need-metal",x);}
    else if(x>4.5f&&x<7){if(!Repair())Emit("repair-unavailable",x);}
    else if(Math.Abs(x)<2){BeginRest();}
   }else if(place==Place.Expedition){
    foreach(var s in supplies)if(!s.opened&&Math.Abs(s.x-x)<1.25f){s.opened=true;carried+=s.amount;int index=supplies.IndexOf(s);missionMask|=1<<index;CollectLore(index);if(index==0)runWater+=2;if(index==1)runBandages+=2;if(index==2)runBoards+=4;if(index==4||index==6)runFood+=4;Emit("loot",s.x);break;}
    if(progress.chapter==1&&Math.Abs(x-32)<1.5f&&!rescued){if((missionMask&2)!=0){rescued=true;Emit("doyun-rescue",x);}else Emit("need-parts",x);}
    if(progress.chapter==2&&Math.Abs(x-48)<1.5f&&!rescued){if((missionMask&4)!=0){rescued=true;Emit("harin-rescue",x);}else Emit("need-tools",x);}
    if(progress.chapter==4&&Math.Abs(x-86)<1.5f&&!rescued){if((missionMask&64)!=0){rescued=true;Emit("mijeong-rescue",x);}else Emit("need-seeds",x);}
    if((x<-5.8f||x>89.5f)&&carried>0)ReturnHome(true);
   }
  }
  if(hp<=0){hp=0;if(place==Place.Expedition)ReturnHome(false);else if(place==Place.Raid)FinishRaid(false);}
 }
 void UpdateEnemies(float dt){
  bool any=false;
  foreach(var e in enemies){
   if(e.hp<=0)continue;any=true;
   if(e.boss&&!e.enraged&&e.hp<=e.maxHp/2){e.enraged=true;e.runner=true;Emit("boss-enrage",e.x);}
   if(e.delay>0){e.delay-=dt;continue;}
   e.cooldown=Math.Max(0,e.cooldown-dt);e.hurt=Math.Max(0,e.hurt-dt);e.memory=Math.Max(0,e.memory-dt);
   e.vy-=18*dt;e.y=Math.Max(0,e.y+e.vy*dt);if(e.y==0)e.vy=0;
   if(e.hurt>0||e.y>.1f)continue;
   bool gateTarget=place==Place.Raid&&Math.Abs(e.x-x)>3;
   float target=gateTarget?-5.8f:x,distance=Math.Abs(target-e.x);
   if(Math.Abs(e.x-x)<(crouching?4:8)||place==Place.Raid)e.memory=6;
   if(e.dash>0){
    e.dash-=dt;e.x=Math.Max(Left,Math.Min(Right,e.x+e.direction*7*dt));
    if(!e.hit&&Math.Abs(e.x-(e.gateTarget?-5.8f:x))<1){EnemyHit(e);e.hit=true;}
    if(e.dash<=0)e.cooldown=.9f;continue;
   }
   if(e.attack>0){
    e.attack-=dt;
    if(e.attack<=0){if(e.spitter){if(acids.Count<16)acids.Add(new Acid{x=e.x,vx=e.direction*5});e.cooldown=1.7f;Emit("acid-shot",e.x);continue;}if(e.brute)Emit("brute-slam",e.x);if(e.runner){e.dash=.42f;e.hit=false;Emit("charge",e.x);}else{if(Math.Abs(e.x-(e.gateTarget?-5.8f:x))<(e.brute?2.6f:1.15f))EnemyHit(e);e.cooldown=.7f;}}
    continue;
   }
   if(e.cooldown<=0&&distance<(e.spitter?6:e.boss&&e.enraged?4:e.brute?2.1f:e.runner?3.1f:1)){
    e.attack=e.spitter?.9f:e.boss&&e.enraged?.65f:e.brute?1.05f:e.runner?.65f:.5f;e.gateTarget=gateTarget;e.direction=Math.Sign(target-e.x);if(e.direction==0)e.direction=-1;
    Emit(e.spitter?"acid-warning":e.brute?"brute-warning":e.runner?"runner-warning":"warning",e.x);
   }else if(e.memory>0&&distance>.7f&&(!e.spitter||distance>4.5f))e.x+=Math.Sign(target-e.x)*(e.brute?.85f:e.runner?1.8f:1.35f)*dt;
  }
  TickAcid(dt);
  if(place==Place.Raid){if(progress.gate<=0)FinishRaid(false);else if(!any)FinishRaid(true);}
 }
 void EnemyHit(Enemy e){
  if(e.gateTarget){progress.gate=Math.Max(0,progress.gate-(e.brute?25:e.runner?18:10));Emit("gate-hit",-5.8f);}
  else if(invulnerable<=0&&y<.9f){hp-=(e.boss?30:e.brute?25:e.runner?18:12)+Threat*2;invulnerable=.6f;Emit("damage",x);}
 }
 void FinishRaid(bool won){
  if(place!=Place.Raid)return;bool practice=drill;
  if(practice){progress.gate=gateBeforeDrill;hp=healthBeforeDrill;}
  else{progress.raidPending=false;if(won)progress.metal+=8;else{progress.metal=Math.Max(0,progress.metal-10);progress.gate=Math.Max(25,progress.gate);}}
  drill=false;Home(practice?"drill-end":won?"raid-win":"raid-loss");
 }
}
}
