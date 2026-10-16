using UnityEngine;
namespace Duskline.Slice {
public sealed partial class SliceGame {
 Sprite[] floorArt;SpriteRenderer seokju,mijeong;SpriteRenderer greenhouseDistrict;Sprite railwayDistrict;
 void SetupShelterArt(){
  floorArt=new Sprite[3];floorArt[0]=shelterBackdrop.sprite;
  for(int i=1;i<3;i++){var t=Resources.Load<Texture2D>(i==1?"ShelterFloor2":"ShelterFloor3");floorArt[i]=Sprite.Create(t,new Rect(0,0,t.width,t.height),new Vector2(.5f,.5f),100);}
  var p=Resources.Load<Texture2D>("NewResidents");
  seokju=Make("Seokju - blue cap",GridFrame(p,2,1,0,470,.5f),Vector3.zero,8);
  mijeong=Make("Mijeong - seed canister",GridFrame(p,2,1,1,470,.5f),Vector3.zero,8);
  var district=world.transform.Find("District 5");if(district){greenhouseDistrict=district.GetComponent<SpriteRenderer>();railwayDistrict=greenhouseDistrict.sprite;}
 }
 void RenderShelter(){
  bool home=model.place==Place.Shelter;int floor=model.shelterFloor;
  shelterBackdrop.sprite=floorArt[floor-1];
  if(floor==1){shelterBackdrop.transform.localScale=Vector3.one*1.35f;shelterBackdrop.transform.position=new Vector3(0,.48f,0);}
  else {var rect=floorArt[floor-1].rect;shelterBackdrop.transform.localScale=new Vector3(2257.2f/rect.width,920f/rect.height,1);shelterBackdrop.transform.position=new Vector3(0,-.3f,0);}
  if(home){
   doy.enabled=floor==1&&model.progress.chapter>=2&&model.activeHero!=1;
   harin.enabled=floor==(model.progress.floors>=2?2:1)&&model.progress.chapter>=3&&model.activeHero!=2;
  }
  seokju.enabled=home&&model.progress.chapter>=4&&floor==(model.progress.floors>=2?2:1);
  seokju.transform.position=new Vector3(-1.8f,-2.6f,0);
  mijeong.enabled=home?model.progress.chapter>=5&&floor==(model.progress.floors>=3?3:1):model.place==Place.Expedition&&model.progress.chapter==4&&!model.rescued;
  mijeong.transform.position=new Vector3(home?3:86,-2.6f,0);
  if(greenhouseDistrict){greenhouseDistrict.sprite=model.progress.chapter>=4?floorArt[2]:railwayDistrict;var rect=greenhouseDistrict.sprite.rect;greenhouseDistrict.transform.localScale=new Vector3(1672f/rect.width,model.progress.chapter>=4?920f/rect.height:1,1);greenhouseDistrict.transform.position=new Vector3(83.6f,model.progress.chapter>=4?-.3f:-.15f,0);}
 }
 void ShelterSignal(string value){
  switch(value){
   case "floor":Toast(model.shelterFloor+"층 · "+(model.shelterFloor==1?"작업장과 방어문":model.shelterFloor==2?"병실과 거주 공간":"옥상 재배·정수"));break;
   case "floor-built":Burst(new Vector3(model.x,-1.4f,0),Amber,40);break;
   case "floor-cost":Toast("상층 개방: 금속 20 + 판재 8 필요");break;
   case "floor-story-lock":Toast("2층은 하린 합류, 3층은 미정 구조 후 개방됩니다");break;
   case "clinic-cost":Toast("치료대 설치: 금속 12 + 판재 4");break;
   case "clinic-built":Toast("치료대 가동 · E로 붕대 2개 제작");break;
   case "bandage-cost":Toast("붕대 2개 제작: 물 2 + 금속 3 필요");break;
   case "bandage-made":Toast("붕대 2개 제작 · 수색 중 H로 치료");break;
   case "filter-needed":Toast("3-2 배수관 부품을 회수하고 성공 귀환해야 합니다");break;
   case "purifier-cost":Toast("정수 설비 수리: 금속 15 + 판재 4 필요");break;
   case "purifier-built":Toast("정수 설비 가동 · 물자를 가져오는 성공 귀환마다 물 +2");break;
   case "purifier-ready":Toast("정수 설비 가동 중 · 성공 귀환마다 물 +2, 보관 한도 50");break;
   case "farm-cost":Toast("재배대 설치: 종자 1 + 판재 6 + 금속 10 필요");break;
   case "farm-built":Toast("재배대 설치 · E 또는 B에서 종자1 물3으로 파종하세요");break;
   case "farm-ready":Toast("파종 후 성공 수색 2회 → 직접 수확 · B에서 관리");break;
   case "heal":Burst(new Vector3(model.x,-1.5f+model.y,0),new Color(.4f,1,.6f),20);Toast("붕대 -1 · 체력 +25");break;
   case "seokju-talk":ShowScene("석주 · 아직 싸 둔 짐",new[]{model.progress.harvests>0?"석주는 가방의 옷 한 벌을 꺼내 선반에 둔다.\n“저도 다음 수확 때 물을 나를게요.”\n짐을 모두 풀지는 않았지만, 오늘은 떠날 준비를 하지 않는다.":"석주는 큰 파란 모자를 벗지 않은 채 짐을 만진다.\n“제가 여기 계속 있어도 되는 거예요?”\n하린은 대답 대신 그의 그릇을 식탁에 놓는다."});break;
   case "mijeong-rescue":Toast("미정: 다음 파종분은 지켜야 해. · 종자와 함께 귀환하세요");break;
   case "need-seeds":Toast("온실 종자 보관함을 먼저 회수하세요");break;
   case "first-harvest":ShowScene("3장 · 첫 수확",new[]{"이번에는 상자 안에서 작은 잎이 살아남았다.\n석주는 그릇을 들고도 자기가 먹어도 되는지 물었다.\n하린은 자기 몫의 절반을 그의 그릇에 덜어 주었다.","미정은 다음 파종분을 따로 잠갔다.\n이 작은 수확으로 모두가 먹고살 수는 없다. 수색은 계속된다.\n하지만 석주의 가방에서는 옷 한 벌이 선반으로 옮겨졌다."});break;
   case "story-4":ShowScene("2장 · 구조의 값",new[]{
    "이현은 형의 흔적을 찾는 일을 미루고 약을 가져왔다.\n돌아오는 길에 이번 수색의 물과 식량 일부를 버려야 했다.\n하린은 약을 투여했지만, 석주의 어머니는 아침을 넘기지 못했다.",
    "석주는 침대에 다가가지 못했다. 하린은 손을 잡아도 된다고 말했다.\n이번 죽음은 라자루스 감염이 아닌, 오염된 상처의 일반 감염이었다.\n남은 약은 선반에 올랐다. 석주의 짐은 문 옆에 그대로 놓였다.",
    "석주가 쉘터에 머뭅니다. 2층 병실·거주 공간을 정리하세요.\n먹을 것이 모자랍니다. 철도 끝 온실의 재배자 미정을 찾아갑니다.\n다음 목표: 온실(86m)의 종자와 미정 구조."});break;
   case "story-5":ShowScene("3장 · 남겨 둔 씨앗",new[]{
    "미정은 종자통의 잠금쇠를 풀지 않았다.\n“오늘 다 먹으면 다음 계절에는 뭘 심지?”\n일행은 종자와 정수 부품, 살아 있는 작물을 나눠 메고 돌아왔다.",
    "첫 재배는 실패했다. 가져온 물이 오염되어 곰팡이가 번졌다.\n미정은 상한 작물을 버렸다. 도윤은 배수관을 분리했다.\n3층을 개방하고, 교역 은닉처(69m)에서 새 여과 부품을 가져오세요."});break;
   case "story-6":ShowScene("3장 · 다음 계절을 위한 방",new[]{
    "이현이 가져온 여과 부품을 도윤이 작업대에 늘어놓았다.\n오염된 배수관을 바꾸면 다시 심을 수 있다.\n미정은 식량으로 먹지 않고 남겨 둔 종자를 꺼냈다.",
    "3층에서 정수 설비와 재배대를 가동하세요.\n물자를 가져오는 성공 귀환마다 물 +2. 파종에 종자1·물3이 필요합니다.\n성공 수색 2회 후 직접 수확하세요. 주민 배급은 식량3입니다.",
    "형의 무전은 여전히 돌아오지 않는다. 그래도 돌아올 방은 늘었다.\n노란 후드의 재희가 교역로 정보를 남겼다. 47m 은닉처의 표식을 확인하자."});break;
  }
 }
 void DrawShelterHUD(){
  if(model.place==Place.Shelter){
   Box(new Rect(350,218,580,44),Panel);Label(new Rect(364,224,550,29),"물 "+model.progress.water+"  식량 "+model.progress.food+"  붕대 "+model.progress.bandages+"  판재 "+model.progress.boards+"  종자 "+model.progress.seeds,17,Cream);
   string info=model.shelterFloor==1?"1F 작업·방어":model.shelterFloor==2?"2F 병실·거주":"3F 정수·재배";
   Box(new Rect(948,155,310,51),Panel);Label(new Rect(962,160,288,38),info+"  [개방 "+model.progress.floors+"층]",17,Amber);
   if(Mathf.Abs(model.x-8.5f)<2)WorldLabel(new Vector3(8.5f,.6f,0),"↑/W 위층 · R 아래층",Amber);
   Label(new Rect(490,49,420,25),"계단: 오른쪽 끝 · ↑/W 상승 / R 하강",14,Cream);
   if(model.shelterFloor==2){
    if(model.x<-3)WorldLabel(new Vector3(-6,-.3f,0),model.progress.clinic?"E 붕대 제작 · 물2 금속3":"E 치료대 · 금속12 판재4",Amber);
    if(Mathf.Abs(model.x)<3)WorldLabel(new Vector3(0,.1f,0),"E 석주와 대화",Cream);
   }
   if(model.shelterFloor==3){
    if(model.x<-3)WorldLabel(new Vector3(-6,-.3f,0),model.progress.purifier?"정수기 가동 중":"E 정수기 · 금속15 판재4",Amber);
    if(Mathf.Abs(model.x)<3)WorldLabel(new Vector3(0,-.3f,0),model.progress.farm?"재배 · "+model.CropState+" · E 관리":"E 재배대 · 금속10 판재6",Amber);
    Box(new Rect(350,270,580,36),Panel);Label(new Rect(364,273,555,29),"정수 "+(model.progress.purifier?"가동":"미가동")+" / 재배 "+(model.progress.farm?"설치":"미설치")+" / 누적 수확 "+model.progress.harvests+"회",17,Cream);
   }
   if(model.settlement!="")Label(new Rect(350,541,750,27),"지난 수색 정산: "+model.settlement,16,Amber);
   if(model.progress.chapter>=4&&model.shelterFloor==(model.progress.floors>=2?2:1))WorldLabel(new Vector3(-1.8f,.3f,0),"석주 · 큰 파란 모자",Cream);
   if(model.progress.chapter>=5&&model.shelterFloor==(model.progress.floors>=3?3:1))WorldLabel(new Vector3(3,.3f,0),"미정 · 종자통",Cream);
  }else{
   Label(new Rect(36,552,430,26),"H 치료 · 붕대 "+model.progress.bandages+(model.healCooldown>0?" / "+model.healCooldown.ToString("0")+"초":""),15,Cream);
   if(model.progress.chapter==4&&!model.rescued)WorldLabel(new Vector3(86,.2f,0),"E 종자 회수 · 미정 구조",Amber);
  }
 }
}
}
