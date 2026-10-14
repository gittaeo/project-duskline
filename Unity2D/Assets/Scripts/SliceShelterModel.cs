using System;
namespace Duskline.Slice {
public sealed partial class Progress {
 public int floors=1,food,medicine,seeds,harvests,waterProduced;
 public bool rescueBriefed,clinic,purifier,farm,cropFailed;
 void ValidateExpansion(){
  floors=Math.Max(1,Math.Min(3,floors));food=Math.Max(0,Math.Min(50,food));medicine=Math.Max(0,medicine);seeds=Math.Max(0,seeds);harvests=Math.Max(0,harvests);waterProduced=Math.Max(0,waterProduced);
  if(chapter<3)floors=1;else if(chapter<5)floors=Math.Min(2,floors);
  if(floors<2)clinic=false;if(floors<3){purifier=false;farm=false;}if(chapter<6)purifier=false;
 }
}
public sealed partial class SliceModel {
 public int shelterFloor=1;public float healCooldown;public string settlement="";
 public bool ChangeFloor(int direction){
  if(place!=Place.Shelter||Math.Abs(x-8.5f)>1.6f||y>.05f||direction==0)return false;
  int target=shelterFloor+Math.Sign(direction);if(target<1||target>3)return false;
  if(target>progress.floors){
   if(progress.chapter<(target==2?3:5)){Emit("floor-story-lock",x);return false;}
   if(progress.metal<20||progress.boards<8){Emit("floor-cost",x);return false;}
   progress.metal-=20;progress.boards-=8;progress.floors=target;Emit("floor-built",x);Save?.Invoke();
  }
  shelterFloor=target;ResetCombat();Emit("floor",x);return true;
 }
 public bool UseFacility(){
  if(place!=Place.Shelter||shelterFloor==1)return false;
  if(shelterFloor==2&&x<-4){
   if(!progress.clinic){if(progress.metal<12||progress.boards<4){Emit("clinic-cost",x);return false;}progress.metal-=12;progress.boards-=4;progress.clinic=true;Emit("clinic-built",x);}
   else {if(progress.water<2||progress.metal<3){Emit("bandage-cost",x);return false;}progress.water-=2;progress.metal-=3;progress.bandages+=2;Emit("bandage-made",x);}
  }else if(shelterFloor==3&&x<-4){
   if(progress.purifier){Emit("purifier-ready",x);return false;}
   if(progress.chapter<6){Emit("filter-needed",x);return false;}
   if(progress.metal<15||progress.boards<4){Emit("purifier-cost",x);return false;}
   progress.metal-=15;progress.boards-=4;progress.purifier=true;Emit("purifier-built",x);
  }else if(shelterFloor==3&&Math.Abs(x)<2){
   if(progress.farm){Emit("farm-ready",x);return false;}
   if(progress.seeds<1||progress.boards<6||progress.metal<10){Emit("farm-cost",x);return false;}
   progress.seeds--;progress.boards-=6;progress.metal-=10;progress.farm=true;Emit("farm-built",x);
  }else if(shelterFloor==2&&Math.Abs(x)<2){Emit("seokju-talk",x);return true;}
  else return false;
  Save?.Invoke();return true;
 }
 public bool Heal(){
  if(place==Place.Shelter||hp>=100||hp<=0||progress.bandages<1||healCooldown>0)return false;
  progress.bandages--;hp=Math.Min(100,hp+25);healCooldown=12;Emit("heal",x);Save?.Invoke();return true;
 }
 void SettleShelter(){
  settlement="";
  if(progress.purifier){int before=progress.water;progress.water+=Math.Min(2,Math.Max(0,50-progress.water));progress.waterProduced+=Math.Max(0,progress.water-before);settlement="정수 +"+(progress.water-before)+" / ";}
  if(progress.farm&&progress.purifier&&progress.water>=3){
   progress.water-=3;progress.food=Math.Min(50,progress.food+4);progress.harvests++;settlement+="수확 식량 +4 (물 -3) / ";
  }else if(progress.farm)settlement+="재배 중지: 정수·물 3 필요 / ";
  int meal=progress.chapter>=5?3:progress.chapter>=4?2:progress.chapter>=3?1:0;
  int consumed=Math.Min(meal,progress.food);progress.food-=consumed;
  settlement+="배급 "+consumed+"/"+meal;
 }
}
}
