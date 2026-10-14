using System;
namespace Duskline.Slice {
public sealed partial class SliceModel {
 public float coopTimer,coopElapsed,coopCooldown,coopX,coopY,coopFace;
 public int coopOutgoing,coopIncoming,coopPulse;
 void BeginCoop(int outgoing){
  if(place==Place.Shelter||coopCooldown>0)return;
  coopOutgoing=outgoing;coopIncoming=activeHero;coopX=x;coopY=y;coopFace=face;
  coopTimer=.72f;coopElapsed=0;coopPulse=0;coopCooldown=8;Emit("coop-start",x);
 }
 void TickCoop(float dt){
  coopCooldown=Math.Max(0,coopCooldown-dt);if(coopTimer<=0)return;
  coopTimer=Math.Max(0,coopTimer-dt);coopElapsed+=dt;
  while(coopPulse<2&&coopElapsed>=(coopPulse==0?.12f:.34f)){
   int attacker=coopPulse==0?coopOutgoing:coopIncoming;coopPulse++;
   float reach=attacker==2?7:attacker==3?4:3;Emit("coop-impact",coopX);
   foreach(var e in enemies)if(e.hp>0&&e.delay<=0&&(e.x-coopX)*coopFace>-.5f&&Math.Abs(e.x-coopX)<reach&&Math.Abs(e.y-coopY)<1.8f){
    lastDamage=progress.weapon+1;e.hp-=lastDamage;e.hurt=e.boss?.08f:.6f;if(!e.boss)e.attack=e.dash=0;
    if(e.hp<=0){runKills++;if(e.boss){lastEventX=e.x;DefeatBoss();}}
    Emit(e.hp<=0?"down":"hit",e.x);
   }
  }
 }
}
}
