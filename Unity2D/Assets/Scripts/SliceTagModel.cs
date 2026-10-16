using System;
namespace Duskline.Slice {
public sealed partial class SliceModel {
 public int activeHero;public float tagCooldown,tagBuffer;
 public int PartySize=>progress.chapter>=3?4:progress.chapter>=2?3:2;
 public string HeroName=>HeroNames[activeHero];
 public static readonly string[] HeroNames={"강이현","도윤","하린","서린"};
 public int NextHero {get {for(int n=1;n<=4;n++){int id=(activeHero+n)%4;if(id==0||id==3||id==1&&progress.chapter>=2||id==2&&progress.chapter>=3)return id;}return 0;}}
 public bool Tag(){
  if(restTimer>0||hp<=0||tagCooldown>.4f)return false;
  if(roll>0||skillTimer>0||tagCooldown>0){tagBuffer=1.2f;Emit("tag-buffer",x);return true;}
  ExecuteTag();return true;
 }
 void ExecuteTag(){
  int outgoing=activeHero;activeHero=NextHero;tagBuffer=0;attack=attackElapsed=attackLock=comboGrace=0;combo=0;queued=attackHit=false;counterAttack=false;crouching=false;
  tagCooldown=2;invulnerable=Math.Max(invulnerable,.45f);if(place!=Place.Shelter)counterWindow=1.5f;
  Emit("tag",x);BeginCoop(outgoing);
 }
 void TickTag(float dt){
  if(tagBuffer<=0)return;tagBuffer=Math.Max(0,tagBuffer-dt);
  if(tagBuffer>0&&hp>0&&roll<=0&&skillTimer<=0&&tagCooldown<=0)ExecuteTag();
 }
}
}
