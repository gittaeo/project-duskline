using UnityEngine;
namespace Duskline.Slice {
public sealed partial class SliceGame {
 bool worldMap;int selectedRoute;Sprite[] worldEnemies;SpriteRenderer[] weatherMarks,acidMarks,districtArt;
 void SetupWorldArt(){
  var t=Resources.Load<Texture2D>("WorldEnemies");worldEnemies=new Sprite[4];
  for(int i=0;i<4;i++)worldEnemies[i]=EnemyTile(t,new Rect(i%2*768,i<2?440:0,768,i<2?584:430),i<2?210:190);
  districtArt=new SpriteRenderer[6];for(int i=0;i<6;i++)districtArt[i]=world.transform.Find("District "+i).GetComponent<SpriteRenderer>();
  weatherMarks=new SpriteRenderer[32];for(int i=0;i<32;i++)weatherMarks[i]=Make("Atmosphere "+i,square,Vector3.zero,25);
  acidMarks=new SpriteRenderer[16];for(int i=0;i<16;i++)acidMarks[i]=Make("Acid projectile "+i,square,Vector3.zero,16);
 }
 static Sprite EnemyTile(Texture2D t,Rect rect,float ppu){
  int floor=(int)rect.height;var pixels=t.GetPixels32();for(int y=0;y<(int)rect.height;y++)for(int x=0;x<(int)rect.width;x++)if(pixels[((int)rect.y+y)*t.width+(int)rect.x+x].a>180){floor=Mathf.Min(floor,y);break;}
  return Sprite.Create(t,rect,new Vector2(.5f,floor/rect.height),ppu);
 }
 void RenderWorldWeather(){
  bool outside=model.place!=Place.Shelter;
  Color[] palette={new Color(.87f,.85f,1),new Color(.57f,.68f,.84f),new Color(.70f,.83f,.72f),new Color(1,.74f,.54f)};
  foreach(var r in districtArt)r.color=palette[model.weather];
  float now=Time.time;
  for(int i=0;i<weatherMarks.Length;i++){
   var r=weatherMarks[i];r.enabled=outside;if(!outside)continue;
   float px=cam.transform.position.x+Mathf.Repeat(i*3.717f+now*(model.weather==1?-.8f:.13f),16)-8;
   float py=Mathf.Repeat(i*1.417f-now*(model.weather==1?6:.22f),8)-4;
   r.transform.position=new Vector3(px,py,0);
   r.transform.localScale=model.weather==1?new Vector3(.018f,.28f,1):model.weather==2?new Vector3(2.8f,.18f,1):new Vector3(.028f,.028f,1);
   r.transform.rotation=Quaternion.Euler(0,0,model.weather==1?-16:0);
   r.color=model.weather==1?new Color(.55f,.75f,1,.35f):model.weather==2?new Color(.65f,.85f,.75f,.04f):model.weather==3?new Color(1,.7f,.25f,.7f):new Color(.65f,.8f,1,.35f);
  }
  for(int i=0;i<acidMarks.Length;i++){
   var r=acidMarks[i];r.enabled=outside&&i<model.acids.Count;if(!r.enabled)continue;
   var p=model.acids[i];r.transform.position=new Vector3(p.x,-2.6f+p.y,0);r.transform.localScale=new Vector3(.3f,.12f,1);r.transform.rotation=Quaternion.Euler(0,0,now*500);r.color=new Color(.6f,1,.2f);
  }
 }
 void WorldSignal(string value){
  if(value=="boss-enrage"){Toast("철도 집행자 격노! · 돌진 예고를 확인하세요");shake=.25f;sound.pitch=.45f;sound.PlayOneShot(alarmClip,.35f);}
  if(value=="boss-defeated"){Toast("보스 격파! 금속25 확보 · 성공 귀환 시 강화 코어1");Burst(new Vector3(model.lastEventX,-1,0),Amber,55);}
  if(value=="upgrade-core")Toast("4~6단계 강화는 보스 코어 1개가 추가로 필요합니다");
  if(value=="acid-warning")sound.PlayOneShot(alarmClip,.15f);
 }
 string RescueHint(){
  switch(model.progress.chapter){
   case 0:return "1-1 물·붕대·판재를 회수하고 귀환 → 다음 임무에서 도윤 구조";
   case 1:return "지금 도윤 구조 가능! 정비 구역: 부품(16m) → 도윤(32m) → 귀환";
   case 2:return "지금 하린 구조 가능! 도구(31m) → 하린(48m) → 귀환";
   case 3:return "의약품(69m) 회수 후 귀환하면 석주가 정착합니다";
   case 4:return "온실(86m) 종자 회수·미정 구조 → 성공 귀환";
   case 5:return "여과 부품(69m) 회수 → 귀환 → 3층 정수 설비 복구";
   case 6:return "4-1 재희가 남긴 교역로 표식(47m)을 회수하고 귀환";
   case 7:return "4-2 우의 단서(86m) 회수 + 철도 집행자 처치 후 귀환";
   case 8:return "4-3 털린 비축처 47m·69m 조사 후 귀환";
   case 9:return "4-4 복구 물자 31m·86m 회수와 집행자 처치 후 귀환";
   default:return "성채 외곽 운송 구역 단서 확보 · 다음 출정을 준비하세요";
  }
 }
 void MapKeyboard(){
  if(Input.GetKeyDown(KeyCode.Escape)){worldMap=false;return;}
  for(int i=0;i<4;i++)if(Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1+i)))selectedRoute=i;
  if(Input.GetKeyDown(KeyCode.Return)&&model.place==Place.Shelter)MapDepart();
 }
 void MapDepart(){if(model.Travel(selectedRoute)){worldMap=false;journal=false;Render(0);cam.transform.position=new Vector3(model.x,-.25f,-10);}else Toast(model.progress.raidPending?"습격 방어를 마친 뒤 출정할 수 있습니다":"아직 열리지 않은 지역입니다");}
 bool MapButton(Rect rect,string text,bool enabled=true){bool old=GUI.enabled;GUI.enabled=enabled;var style=new GUIStyle(GUI.skin.button){font=font,fontSize=16,wordWrap=true};bool clicked=GUI.Button(rect,text,style);GUI.enabled=old;return clicked;}
 void DrawWorldHUD(){
  if(model.place==Place.Expedition){
   Label(new Rect(36,127,278,24),SliceModel.Weathers[model.weather]+" · 위험도 "+(model.Threat+1),14,Cream);
   foreach(var e in model.enemies)if(e.boss&&e.hp>0&&Mathf.Abs(e.x-model.x)<9){
    Box(new Rect(375,222,530,61),Panel);Label(new Rect(393,227,500,28),"철도 집행자"+(e.enraged?" · 격노":" · 보스"),20,Amber);
    Box(new Rect(393,264,494,8),new Color(.2f,.1f,.1f));Box(new Rect(393,264,494*Mathf.Clamp01(e.hp/(float)e.maxHp),8),new Color(.8f,.22f,.13f));
   }
  }
  if(!worldMap||StoryOpen)return;
  Box(new Rect(0,0,1280,720),new Color(.01f,.02f,.035f,.96f));
  for(int i=0;i<18;i++)Box(new Rect(100+i*64,110,1,500),new Color(.22f,.35f,.38f,.14f));
  for(int i=0;i<9;i++)Box(new Rect(100,110+i*60,1080,1),new Color(.22f,.35f,.38f,.14f));
  Label(new Rect(110,49,1060,45),"해온시 수색 지도 · 황혼선 제7대피소",30,Amber);
  Label(new Rect(110,99,1080,32),model.MissionTitle+" / 위험도 "+(model.Threat+1)+" / 다음 출정: "+SliceModel.Weathers[model.progress.sorties%4],18,Cream);
  Box(new Rect(105,140,1070,60),Panel);Label(new Rect(122,149,1035,40),RescueHint(),19,Amber);
  for(int i=0;i<4;i++){
   float xx=110+i*272;bool unlocked=i<model.OpenRoutes;
   Box(new Rect(xx,245,246,175),selectedRoute==i?new Color(.12f,.24f,.28f,.98f):Panel);
   Label(new Rect(xx+15,254,219,29),(i+1)+" · "+SliceModel.Routes[i],21,unlocked?Cream:Color.gray);
   string detail=i==0?"물·붕대·판재\n첫 수색 / 입구 귀환":i==1?"부품 · 도윤 구조\n진입 12m":i==2?"진료 도구 · 하린 구조\n진입 27m": "의약품·종자 · 미정\n진입 61m / 보스 83m";
   var style=Style(16,Cream);style.wordWrap=true;GUI.Label(new Rect(xx+15,290,217,65),detail,style);
   if(MapButton(new Rect(xx+14,369,218,37),unlocked?"목적지 선택":"스토리 진행 후 개방",unlocked))selectedRoute=i;
  }
  string threat=model.Threat==0?"일반 감염체":model.Threat==1?"일반 · 돌진형":model.Threat==2?"돌진형 · 중장갑 · 보스":"중장갑 · 산성 투척형 · 보스";
  Label(new Rect(110,431,1050,27),"출현: "+threat+" / 출정마다 조명·날씨·적 위치가 달라집니다",17,Cream);
  if(model.place==Place.Shelter){
   if(MapButton(new Rect(110,481,310,48),model.progress.raidPending?"습격 방어 후 출정":"선택 지역으로 출정 [ENTER]",!model.progress.raidPending&&selectedRoute<model.OpenRoutes))MapDepart();
   for(int i=1;i<=3;i++)if(MapButton(new Rect(450+(i-1)*150,481,136,48),i+"층 이동",i<=model.progress.floors)){model.VisitFloor(i);worldMap=false;}
   bool can=model.progress.weapon<6&&model.progress.metal>=12*model.progress.weapon&&(model.progress.weapon<3||model.progress.cores>0);
   if(MapButton(new Rect(110,554,480,49),model.progress.weapon>=6?"무기 최대 강화 LV6":"무기 LV"+model.progress.weapon+" → "+(model.progress.weapon+1)+" · 금속"+12*model.progress.weapon+(model.progress.weapon>=3?" 코어1":""),can))model.Upgrade();
   Label(new Rect(614,555,540,46),"금속 "+model.progress.metal+" / 보스 코어 "+model.progress.cores+" · 첫 강화용 금속12 지급",17,Cream);
   string forge=model.SignatureRank>=3?"고유 무기 최대 각성":!model.SignatureUnlocked?"고유 무기: 4장 · LV6 · 이수형 설계도 필요":SliceModel.SignatureNames[model.activeHero]+" "+(model.SignatureRank==0?"제작":"각성 "+(model.SignatureRank+1))+" · 금속"+model.SignatureMetalCost+" 코어"+model.SignatureCoreCost;
   if(MapButton(new Rect(110,608,710,32),forge,model.CanForgeSignature))model.ForgeSignature();
   if(MapButton(new Rect(842,608,326,32),"제작 대상: "+model.HeroName+" →",true)){model.tagCooldown=0;model.Tag();}
  }else{
   if(MapButton(new Rect(110,488,410,48),"안전 귀환 · 물자 필요 / 주변 적 6m 이내 불가",model.place==Place.Expedition)){if(model.MapReturn())worldMap=false;}
   Label(new Rect(545,482,615,65),"귀환 불가 시 적과 거리를 벌린 뒤 다시 시도하세요.\n지역 간 재출정은 쉘터에서 선택합니다.",17,Cream);
  }
  Label(new Rect(110,640,1060,30),"TAB / ESC 닫기  ·  숫자 1~4 목적지  ·  I 스토리 기록  ·  구조는 성공 귀환 시 확정",16,Cream);
 }
}
}
