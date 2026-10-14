using System;
namespace Duskline.Slice {
public sealed partial class Progress { public bool resonanceBlueprint; }
public sealed partial class SliceModel {
 public bool carryingBlueprint;
 public bool TalkWitness(){
  if(progress.chapter<6)return false;
  if(place==Place.Shelter&&shelterFloor==1&&Math.Abs(x-2.8f)<1){Emit("taeo-talk",x);return true;}
  if(place!=Place.Expedition||Math.Abs(x-58)>1.3f)return false;
  foreach(var e in enemies)if(e.hp>0&&e.delay<=0&&Math.Abs(e.x-x)<4){Emit("suhyeong-danger",x);return true;}
  carryingBlueprint=!progress.resonanceBlueprint;Emit("suhyeong-talk",x);return true;
 }
}
}
