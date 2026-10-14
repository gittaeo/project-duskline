using System;
using System.Collections.Generic;
namespace Duskline.Slice {
public sealed partial class Progress {
 public int sorties,cores,bossKills;public bool starterKit;
}
public sealed partial class SliceModel {
 public sealed class Acid {public float x,y=1,vx,life=3;}
 public readonly List<Acid> acids=new List<Acid>();
 public int route,weather;public bool bossDefeated;
 public static readonly string[] Routes={"약국 골목","정비 구역","임시 진료소","폐철도·온실"};
 public static readonly string[] Weathers={"푸른 황혼","차가운 빗밤","녹빛 안개","붉은 잔광"};
 public int Threat=>progress.chapter==0?0:progress.chapter<3?1:progress.chapter<5?2:3;
 public int OpenRoutes=>Math.Min(4,progress.chapter+1);
 public void GrantStarterKit(){if(progress.starterKit)return;progress.starterKit=true;progress.metal+=12;}
 public bool Travel(int target){
  if(place!=Place.Shelter||target<0||target>=OpenRoutes||progress.raidPending)return false;
  route=target;if(!Depart())return false;
  x=new[]{-5.4f,12f,27f,61f}[target];return true;
 }
 public bool MapReturn(){
  if(place!=Place.Expedition||carried<=0||attack>0||skillTimer>0||roll>0||y>.1f)return false;
  foreach(var e in enemies)if(e.hp>0&&e.delay<=0&&Math.Abs(e.x-x)<6)return false;
  if(acids.Count>0)return false;
  return ReturnHome(true);
 }
 public bool VisitFloor(int floor){
  if(place!=Place.Shelter||floor<1||floor>progress.floors)return false;
  shelterFloor=floor;x=0;ResetCombat();Emit("floor",x);return true;
 }
 void PrepareRegion(){
  route=Math.Min(route,OpenRoutes-1);weather=progress.sorties%4;progress.sorties++;bossDefeated=false;acids.Clear();
  var random=new Random(progress.sorties*391+progress.chapter*53);
  for(int i=0;i<enemies.Count;i++){
   var e=enemies[i];e.x+=(float)(random.NextDouble()-.5)*2.0f;
   e.runner=Threat>=1&&i%3==2;e.brute=Threat>=2&&i>=10&&i%2==0;e.spitter=Threat>=3&&(i==6||i==9);
   if(e.spitter){e.runner=e.brute=false;}e.hp=(e.brute?14:e.spitter?8:e.runner?5:4)+Threat*2;e.maxHp=e.hp;
  }
  if(progress.chapter>=3)enemies.Add(new Enemy{x=83,hp=36+Threat*12,maxHp=36+Threat*12,brute=true,boss=true});
  Save?.Invoke();
 }
 void DefeatBoss(){if(bossDefeated)return;bossDefeated=true;carried+=25;Emit("boss-defeated",lastEventX);}
 void TickAcid(float dt){
  for(int i=acids.Count-1;i>=0;i--){var p=acids[i];p.life-=dt;p.x+=p.vx*dt;
   if(Math.Abs(p.x-x)<.45f&&Math.Abs(p.y-(y+(crouching?.4f:1)))<(crouching?.28f:.65f)){
    if(invulnerable<=0){hp-=8+Threat*2;invulnerable=.6f;Emit("damage",x);}p.life=0;
   }
   if(p.life<=0||p.x<Left||p.x>Right)acids.RemoveAt(i);
  }
 }
}
}
