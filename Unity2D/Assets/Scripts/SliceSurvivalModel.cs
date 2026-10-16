using System;
namespace Duskline.Slice {
public enum RecoveryItem { Bandage, Medicine, Relic }
public sealed partial class Progress {
 public int vitality=100,herbs,relics,cropStage;public bool survivalMigrated;
 public void ValidateSurvival(){
  if(!survivalMigrated){vitality=100;survivalMigrated=true;}
  vitality=Math.Max(1,Math.Min(100,vitality));herbs=Math.Max(0,herbs);relics=Math.Max(0,relics);cropStage=Math.Max(0,Math.Min(3,cropStage));if(!farm)cropStage=0;
 }
}
public sealed partial class SliceModel {
 public float restTimer;int restTarget;int healthBeforeDrill;
 public string CropState=>!progress.farm?"시설 없음":progress.cropStage==0?"파종 대기":progress.cropStage==3?"수확 준비":"성장 "+(progress.cropStage-1)+" / 2";
 public bool BeginRest(){
  if(place!=Place.Shelter||progress.raidPending||restTimer>0||hp>=100)return false;
  bool supplied=progress.food>=1&&progress.water>=1;
  if(!supplied&&hp>=30){Emit("rest-cost",x);return false;}
  if(supplied){progress.food--;progress.water--;restTarget=Math.Min(100,hp+45);restTimer=6;}
  else {restTarget=Math.Min(30,hp+15);restTimer=8;}
  Emit(supplied?"bed-rest":"emergency-rest",x);progress.vitality=hp;Save?.Invoke();return true;
 }
 bool TickRest(float dt){
  if(restTimer<=0)return false;restTimer=Math.Max(0,restTimer-dt);velocityX=0;
  if(restTimer<=0){hp=restTarget;progress.vitality=hp;Emit("rest-finished",x);Save?.Invoke();}return true;
 }
 public bool UseRecovery(RecoveryItem item){
  if(drill||restTimer>0||hp<=0||hp>=100||healCooldown>0||attack>0||skillTimer>0||roll>0)return false;
  int amount;
  if(item==RecoveryItem.Bandage){if(progress.bandages<=0)return false;progress.bandages--;amount=25;}
  else if(item==RecoveryItem.Medicine){if(progress.medicine<=0)return false;progress.medicine--;amount=50;}
  else {if(progress.relics<=0)return false;progress.relics--;amount=100;}
  hp=Math.Min(100,hp+amount);progress.vitality=hp;healCooldown=item==RecoveryItem.Relic?20:12;
  Emit(item==RecoveryItem.Bandage?"heal":item==RecoveryItem.Medicine?"medicine-used":"relic-used",x);Save?.Invoke();return true;
 }
 public bool Plant(){
  if(place!=Place.Shelter||!progress.farm||!progress.purifier||progress.cropStage!=0||progress.seeds<1||progress.water<3)return false;
  progress.seeds--;progress.water-=3;progress.cropStage=1;Emit("planted",x);Save?.Invoke();return true;
 }
 public bool Harvest(){
  if(place!=Place.Shelter||!progress.farm||progress.cropStage!=3)return false;
  progress.food=Math.Min(50,progress.food+4);progress.herbs+=2;progress.seeds++;progress.cropStage=0;progress.harvests++;
  Emit(progress.harvests==1?"first-harvest":"harvested",x);Save?.Invoke();return true;
 }
 public bool CraftMedicine(){
  if(place!=Place.Shelter||!progress.clinic||progress.herbs<2||progress.water<1)return false;
  progress.herbs-=2;progress.water--;progress.medicine++;Emit("medicine-made",x);Save?.Invoke();return true;
 }
 public bool CraftRelic(){
  if(place!=Place.Shelter||!progress.clinic||progress.medicine<2||progress.cores<1)return false;
  progress.medicine-=2;progress.cores--;progress.relics++;Emit("relic-made",x);Save?.Invoke();return true;
 }
}
}
