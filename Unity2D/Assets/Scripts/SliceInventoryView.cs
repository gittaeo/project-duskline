using UnityEngine;
namespace Duskline.Slice {
public sealed partial class SliceGame {
 bool inventory;int inventoryTab;
 bool InventoryInput(){
  if(StoryOpen)return false;
  if(Input.GetKeyDown(KeyCode.B)){inventory=!inventory;worldMap=journal=false;}
  if(!inventory)return false;
  if(Input.GetKeyDown(KeyCode.Escape)){inventory=false;return true;}
  if(Input.GetKeyDown(KeyCode.Tab)){inventory=false;worldMap=true;return true;}
  return true;
 }
 void SurvivalSignal(string value){
  string text=value=="bed-rest"?"침대 휴식 6초 · 식량1 물1 소비 · 체력 +45":value=="emergency-rest"?"응급 휴식 8초 · 자원 없이 체력30까지만 회복":value=="rest-cost"?"휴식에 식량1·물1 필요 · 만피에는 사용하지 않습니다":value=="rest-finished"?"휴식 완료 · 체력 "+model.hp:value=="planted"?"파종 완료 · 물자를 가져오는 성공 수색 2회 후 수확":value=="harvested"?"수확 · 식량4 약초2 종자1":value=="medicine-made"?"약 제작 완료 · 약초2 물1 소비":value=="relic-made"?"회복 아티팩트 제작 · 약2 코어1 소비":value=="medicine-used"?"약 -1 · 체력 +50":value=="relic-used"?"회복 아티팩트 -1 · 체력 전부 회복":value=="career"?"전직 선택 · "+model.CareerName:null;
  if(text!=null)Toast(text);
 }
 bool ItemButton(float y,string text,bool enabled){return MapButton(new Rect(735,y,340,42),text,enabled);}
 void DrawInventory(){
  if(model.restTimer>0){Box(new Rect(420,295,440,74),Panel);Label(new Rect(440,310,400,48),"침대 휴식 중 · "+model.restTimer.ToString("0.0")+"초",24,Amber);}
  Label(new Rect(950,616,310,24),"B 인벤토리 · I 이야기 기록",14,Cream);
  if(!inventory)return;
  Box(new Rect(0,0,1280,720),new Color(.01f,.02f,.035f,.98f));
  Label(new Rect(110,52,1080,42),"제7대피소 · 인벤토리",30,Amber);
  Label(new Rect(110,99,1080,30),"체력 "+model.hp+" / 100 · "+(model.place==Place.Shelter?"쉘터 보관품":"수색 중 / 귀환 전 물자는 별도")+" · 치료 대기 "+model.healCooldown.ToString("0")+"초",18,Cream);
  string[] tabs={"자원·재배","치료품·아티팩트","장비·전직"};for(int i=0;i<3;i++)if(MapButton(new Rect(110+i*360,153,344,42),tabs[i],true))inventoryTab=i;
  var p=model.progress;bool home=model.place==Place.Shelter;
  if(inventoryTab==0){
   Label(new Rect(120,222,580,42),"금속 "+p.metal+"     판재 "+p.boards+"     코어 "+p.cores,22,Cream);
   Label(new Rect(120,279,580,42),"물 "+p.water+"     식량 "+p.food+" / 50",22,Cream);
   Label(new Rect(120,336,580,42),"종자 "+p.seeds+"     약초 "+p.herbs,22,Cream);
   Label(new Rect(120,405,580,65),"재배: "+model.CropState+"\n수확물: 식량4 · 약초2 · 종자1",20,Amber);
   if(ItemButton(238,"파종 · 종자1 물3",home&&p.farm&&p.purifier&&p.cropStage==0&&p.seeds>0&&p.water>=3))model.Plant();
   if(ItemButton(299,"직접 수확",home&&p.cropStage==3)){if(model.Harvest())inventory=false;}
   Label(new Rect(735,366,360,110),"3층 정수기·재배대 필요\n물자와 성공 귀환할 때 성장\n빈손 귀환·실패에는 성장하지 않음",16,Cream);
   Label(new Rect(120,510,980,65),"이번 수색 미보관: 금속 "+model.carried+" / 물 "+model.runWater+" / 붕대 "+model.runBandages+" / 판재 "+model.runBoards,17,Cream);
  }else if(inventoryTab==1){
   bool usable=model.hp<100&&model.hp>0&&model.healCooldown<=0&&model.restTimer<=0&&!model.drill&&model.attack<=0&&model.skillTimer<=0&&model.roll<=0;
   Label(new Rect(120,220,580,45),"붕대 "+p.bandages+" · 체력 +25",22,Cream);
   if(ItemButton(220,"붕대 사용",usable&&p.bandages>0))model.UseRecovery(RecoveryItem.Bandage);
   Label(new Rect(120,280,580,45),"약 "+p.medicine+" · 체력 +50",22,Cream);
   if(ItemButton(280,"약 사용",usable&&p.medicine>0))model.UseRecovery(RecoveryItem.Medicine);
   Label(new Rect(120,340,600,45),"회복 아티팩트 "+p.relics+" · 전부 회복 / 소모형",21,Amber);
   if(ItemButton(340,"아티팩트 사용",usable&&p.relics>0))model.UseRecovery(RecoveryItem.Relic);
   if(ItemButton(415,"약 제작 · 약초2 물1",home&&p.clinic&&p.herbs>=2&&p.water>=1))model.CraftMedicine();
   if(ItemButton(472,"아티팩트 제작 · 약2 코어1",home&&p.clinic&&p.medicine>=2&&p.cores>=1))model.CraftRelic();
   Label(new Rect(120,420,590,110),"제작에는 2층 치료대가 필요합니다.\n치료품은 쉘터와 수색에서 사용 가능.\n재사용: 붕대·약 12초 / 아티팩트 20초",17,Cream);
  }else{
   Label(new Rect(120,220,590,45),"공용 무기 LV "+p.weapon+" / 6",23,Cream);
   for(int i=0;i<4;i++)Label(new Rect(120,275+i*52,590,45),SliceModel.HeroNames[i]+" · "+SliceModel.SignatureNames[i]+" "+(p.signatureRanks[i]>0?"★"+p.signatureRanks[i]:"미제작"),20,p.signatureRanks[i]>0?Amber:Cream);
   Label(new Rect(735,218,400,60),"제작 대상: "+model.HeroName+"\n고유 각성: 공용 LV6 + 설계도",18,Cream);
   if(ItemButton(295,"다음 캐릭터",home&&model.restTimer<=0)){model.tagCooldown=0;model.Tag();}
   if(ItemButton(350,model.SignatureRank>=3?"최대 각성 ★3":"각성/강화 · 금속"+model.SignatureMetalCost+" 코어"+model.SignatureCoreCost,model.CanForgeSignature))model.ForgeSignature();
   if(ItemButton(420,"연사 사수 · 6연발",home&&model.activeHero==2&&model.SignatureRank>0))model.SelectCareer(1);
   if(ItemButton(477,"관통 포격수 · 충전 단발",home&&model.activeHero==2&&model.SignatureRank>0))model.SelectCareer(2);
   Label(new Rect(120,520,590,48),"하린 전직: "+(p.signatureRanks[2]>0?model.CareerName:"고유 무기 제작 후 선택"),20,Amber);
  }
  Label(new Rect(110,617,1050,36),"B / ESC 닫기 · 인벤토리를 닫으면 치료 재사용 시간이 흐릅니다",17,Cream);
 }
}
}
