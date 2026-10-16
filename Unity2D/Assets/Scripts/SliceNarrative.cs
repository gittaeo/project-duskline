using System;
using UnityEngine;
namespace Duskline.Slice {
public sealed partial class SliceGame {
 Sprite[] motion,residents;SpriteRenderer doy,harin;LineRenderer bladeEdge;
 float upgradeTimer;bool journal;string storyTitle;string[] storyPages;int storyPage;
 bool StoryOpen=>storyPages!=null;
 static readonly string[] WeaponNames={"회수한 철검","연마한 강철검","황혼의 보강검","코어 합금검","황혼의 진동검","새벽의 해방검"};
 static readonly Vector2[] MotionRoots={new Vector2(.43f,0),new Vector2(.46f,0),new Vector2(.46f,0),new Vector2(.43f,0),new Vector2(.43f,0),new Vector2(.48f,0),new Vector2(.46f,0),new Vector2(.51f,0),new Vector2(.53f,0)};
 // Blade endpoints are authored per pose; the overlay follows the actual weapon rather than the character's body.
 static readonly Vector4[] MotionBlades={
  new Vector4(.60f,.63f,.98f,.73f),new Vector4(.59f,.61f,.99f,.72f),new Vector4(.64f,.60f,.99f,.71f),
  new Vector4(.50f,.55f,.96f,.67f),new Vector4(.57f,.55f,.99f,.67f),new Vector4(.73f,.70f,.99f,.79f),
  new Vector4(.51f,.25f,.91f,.23f),new Vector4(.52f,.39f,.92f,.56f),new Vector4(.61f,.66f,.94f,.75f)};
 static readonly Vector4[] CombatBlades={
  new Vector4(.33f,.43f,.17f,.41f),new Vector4(.56f,.79f,.89f,.86f),new Vector4(.61f,.30f,.29f,.42f),
  new Vector4(.74f,.42f,.999f,.42f),new Vector4(.67f,.27f,.89f,.11f),new Vector4(.65f,.56f,.87f,.75f)};
 void SetupStoryArt(){
  var t=Resources.Load<Texture2D>("IhyeonMotion");motion=new Sprite[9];
  for(int i=0;i<9;i++)motion[i]=GridFrame(t,3,3,i,190,MotionRoots[i].x);
  t=Resources.Load<Texture2D>("IhyeonCombat");combat=new Sprite[6];
  for(int i=0;i<6;i++)combat[i]=GridFrame(t,3,2,i,128,i==3?.47f:.52f);
  t=Resources.Load<Texture2D>("Residents");residents=new Sprite[4];
  for(int i=0;i<4;i++)residents[i]=GridFrame(t,2,2,i,260,i==1?.43f:.51f);
  doy=Make("Doyun - metal prosthetic arm",residents[0],Vector3.zero,8);
  harin=Make("Harin - white medical bag",residents[2],Vector3.zero,8);
  bladeEdge=new GameObject("Upgraded blade finish").AddComponent<LineRenderer>();bladeEdge.material=new Material(Shader.Find("Sprites/Default"));bladeEdge.sortingOrder=12;bladeEdge.positionCount=2;bladeEdge.useWorldSpace=true;bladeEdge.enabled=false;
 }
 static Sprite GridFrame(Texture2D t,int columns,int rows,int index,float ppu,float pivotX){
  int w=t.width/columns,h=t.height/rows,x=(index%columns)*w,y=(rows-1-index/columns)*h;
  var pixels=t.GetPixels32();int floor=h;
  for(int yy=0;yy<h;yy++)for(int xx=0;xx<w;xx++)if(pixels[(y+yy)*t.width+x+xx].a>180){floor=Math.Min(floor,yy);break;}
  if(floor==h)throw new InvalidOperationException("Empty sprite frame "+t.name+" "+index);
  return Sprite.Create(t,new Rect(x,y,w,h),new Vector2(pivotX,floor/(float)h),ppu);
 }
 int MotionIndex(){
  if(model.y>.005f)return model.vy>0?6:7;
  if(model.crouching||model.roll>0)return 5;
  if(model.landing>0)return 8;
  if(Mathf.Abs(model.velocityX)>.15f)return 1+(int)(model.walkPhase*.65f)%4;
  return 0;
 }
 void RenderStoryArt(){
  bool fighting=model.attack>0;int index=fighting?model.combo+(model.attackElapsed<model.Windup?0:3):MotionIndex();
  hero.sprite=fighting?combat[index]:motion[index];hero.transform.localScale=Vector3.one;
  int level=model.progress.weapon;bladeEdge.enabled=level>1&&model.activeHero==0;
  if(level>1){
   Color color=level==2?new Color(.55f,.85f,.98f,.85f):new Color(1,.76f,.25f,.95f);
   bladeEdge.startColor=bladeEdge.endColor=color;bladeEdge.widthMultiplier=level==2?.018f:.034f;
   Vector4 points=fighting?CombatBlades[index]:MotionBlades[index];Sprite sprite=hero.sprite;
   Vector3 BladePoint(float u,float v){return hero.transform.position+new Vector3((u*sprite.rect.width-sprite.pivot.x)/sprite.pixelsPerUnit*model.face,((1-v)*sprite.rect.height-sprite.pivot.y)/sprite.pixelsPerUnit,0);}
   bladeEdge.SetPosition(0,BladePoint(points.x,points.y));bladeEdge.SetPosition(1,BladePoint(points.z,points.w));
  }
  bool indoors=model.place==Place.Shelter;
  doy.enabled=indoors?model.progress.chapter>=2:model.place==Place.Expedition&&model.progress.chapter==1&&!model.rescued;
  harin.enabled=indoors?model.progress.chapter>=3:model.place==Place.Expedition&&model.progress.chapter==2&&!model.rescued;
  doy.transform.position=new Vector3(indoors?-7.2f:32,-2.6f,0);
  harin.transform.position=new Vector3(indoors?5.2f:48,-2.6f,0);
  // Keep Doyun's authored right-arm silhouette; do not mirror or alternate incompatible prosthetic poses.
  harin.sprite=residents[indoors&&Mathf.Sin(model.time*.9f)>0?3:2];
  upgradeTimer=Mathf.Max(0,upgradeTimer-Time.deltaTime);
 }
 void StorySignal(string value){
  if(value=="upgrade"){upgradeTimer=2.5f;Burst(new Vector3(model.x,-1.6f,0),model.progress.weapon>=3?Amber:new Color(.5f,.85f,1),45);shake=.12f;}
  if(value=="land")Burst(new Vector3(model.x,-2.58f,0),new Color(.52f,.48f,.40f,.5f),5);
  if(value=="story-1")ShowStory(1);if(value=="story-2")ShowStory(2);if(value=="story-3")ShowStory(3);
  if(value=="doyun-rescue"){Toast("도윤: 길부터 봐. 대피로에서 합류하자. · 귀환해야 합류 확정");}
  if(value=="harin-rescue"){ShowScene("약국의 마지막 환자",new[]{"하린은 옛 약국을 임시 진료소로 쓰고 있었다.\n진행된 감염을 멈출 치료는 없었다. 일행은 환자가 있는 문을 막았다.","잠금장치 너머에서 한동안 기다려 달라는 목소리가 들렸다.\n하린은 치료 도구를 쥐고 고개를 돌렸다. 이제 함께 돌아가야 한다."});}
  if(value=="need-parts")Toast("도윤의 의수 관절을 고칠 부품이 필요합니다 · 두 번째 은닉처");
  if(value=="need-tools")Toast("진료 도구부터 회수해야 합니다 · 세 번째 은닉처");
 }
 void ShowScene(string title,string[] pages){if(automated)return;storyTitle=title;storyPages=pages;storyPage=0;}
 void ShowStory(int stage){
  if(stage==0)ShowScene("프롤로그 · 닫힌 문",new[]{
   "붕괴력 50년, 해온시.\n강이현은 형 강준과 폐철도 화물기지에 들어갔다.\n누군가 경보를 울렸고, 수색대의 출구부터 차단기가 내려갔다.",
   "이현은 부상자를 끌고 서비스 문을 통과했다.\n등 뒤에서 넘어진 대원이 도움을 청했다. 감염체가 다가왔다.\n이현은 뻗었던 손을 당겼다. 철문이 닫혔다.",
   "형의 무전은 끊겼다. 이현은 혼자 제7대피소에 들어섰다.\n벽에는 문장이 남아 있었다. “밖에 아직 사람이 있습니다.”\n형을 찾으려면, 먼저 마실 물과 붕대부터 구해야 한다."});
  if(stage==1)ShowScene("1-1 · 빈 그릇",new[]{
   "은닉처의 배급표에는 여덟 이름이 있었다. 다섯은 지워져 있었다.\n물과 붕대, 문을 보강할 판재를 회수했다.\n오래된 약국과 상자는 누군가 최근 물자를 숨겨 둔 장소였다.",
   "이현은 작은 외투의 이름표를 보고 그것을 버리지 못했다.\n잠자리를 문 가까이 붙이고, 안쪽 자리는 비워 두었다.\n다음 목표: 정비소에서 들려오는 타격음."});
  if(stage==2)ShowScene("1-2 · 철을 두드리는 사람",new[]{
   "도윤은 고장 난 금속 의수를 붙잡고 버티고 있었다.\n“팔 하나쯤 안 움직여도 싸울 수 있어. 네가 길부터 봐.”\n서로의 속도를 몰라 돌아오는 실수를 거치며 두 사람은 귀환했다.",
   "도윤은 머물 자격을 얻으려 자기 식량부터 내놓았다.\n다음 날, 의수는 여전히 불편했지만 문 잠금장치는 고쳐져 있었다.\n도윤이 쉘터 작업대에 합류했다. 다음 목표: 약국의 진료 도구."});
  if(stage==3)ShowScene("1장 끝 · 죽은 사람의 방",new[]{
   "하린은 살리지 못한 환자의 기록을 가져왔다.\n세 사람은 두 번째 방을 격리와 치료 공간으로 정리했다.\n그날 저녁의 식량은 셋이 나누기에는 부족했다.",
   "도윤은 먹고 왔다고 거짓말했다. 하린은 알고도 묻지 않았다.\n집이 생겼지만, 셋은 왜 잠들지 못하는지 말하지 않았다.\n끊긴 무전 속 형의 목소리가 이현에게 남아 있었다.",
   "첫 장을 완료했습니다. 문 앞에 석주와 다친 어머니가 도착했습니다.\n하린은 약을 구해도 늦을 수 있다고 말합니다. 이현은 문을 엽니다.\n다음 목표: 교역 은닉처(69m)의 의약품. 2층을 정리할 수 있습니다."});
 }
 bool NarrativeInput(){
  if(StoryOpen){
   if(Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.E)){
    storyPage++;if(storyPage>=storyPages.Length){storyPages=null;model.progress.prologueSeen=true;SaveProgress();}
   }return true;
  }
  if(Input.GetKeyDown(KeyCode.Tab)){worldMap=!worldMap;journal=false;}
  if(Input.GetKeyDown(KeyCode.I)){journal=!journal;worldMap=false;}
  if(journal){
   if(Input.GetKeyDown(KeyCode.RightArrow)||Input.GetKeyDown(KeyCode.D))lorePage=(lorePage+1)%8;
   if(Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown(KeyCode.A))lorePage=(lorePage+7)%8;
  }
  if(worldMap){MapKeyboard();return worldMap;}return journal;
 }
 string Objective(){
  if(model.progress.chapter==0)return "물 "+((model.missionMask&1)!=0?"✓":"0")+"  붕대 "+((model.missionMask&2)!=0?"✓":"0")+"  판재 "+((model.missionMask&4)!=0?"✓":"0")+" · 회수 후 귀환";
  if(model.progress.chapter==1)return model.rescued?"도윤과 성공 귀환":"두 번째 은닉처 부품 → 정비소(32m) 도윤";
  if(model.progress.chapter==2)return model.rescued?"하린과 성공 귀환":"세 번째 은닉처 도구 → 진료소(48m) 하린";
  if(model.progress.chapter==3)return (model.missionMask&32)!=0?"의약품 확보 · 성공 귀환":"교역 은닉처(69m) 의약품 회수 → 귀환";
  if(model.progress.chapter==4)return model.rescued?"미정과 종자를 데리고 귀환":"종자 보관함(86m) 회수 → 미정 구조";
  if(model.progress.chapter==5)return (model.missionMask&32)!=0?"정수 부품 확보 · 성공 귀환":"배수관 부품(69m) 회수 → 귀환 → 3층 정수기 수리";
  if(model.progress.chapter==6)return (model.missionMask&8)!=0?"교역로 표식 확보 · 성공 귀환":"재희의 교역로 표식(47m) 회수 → 귀환";
  if(model.progress.chapter==7)return "우의 단서(86m) "+((model.missionMask&64)!=0?"확보":"미확보")+" · 집행자 "+(model.bossDefeated?"처치":"처치 필요")+" → 귀환";
  if(model.progress.chapter==8)return "털린 보관함(47m·69m) 조사 → 귀환";
  if(model.progress.chapter==9)return "복구 물자(31m·86m) 회수 · 집행자 처치 → 귀환";
  return "4장 완료 · 성채의 계약을 앞두고 쉘터 비축";
 }
 void DrawNarrative(){
  if(model.place==Place.Expedition){
   Box(new Rect(350,152,580,57),Panel);Label(new Rect(364,157,555,24),model.MissionTitle,16,Amber);Label(new Rect(364,182,555,23),Objective(),15,Cream);
   if(model.progress.chapter==1&&!model.rescued)WorldLabel(new Vector3(32,-.2f,0),"E 도윤 구조",Amber);
   if(model.progress.chapter==2&&!model.rescued)WorldLabel(new Vector3(48,-.2f,0),"E 하린과 대화",Cream);
  }
  if(model.place==Place.Shelter){
   Label(new Rect(35,102,270,27),"제7대피소 · "+model.MissionTitle,16,Cream);
   if(model.shelterFloor==1&&model.progress.chapter>=2&&model.activeHero!=1)WorldLabel(new Vector3(-7.2f,.2f,0),"도윤 · 금속 의수",Cream);
   if((model.shelterFloor==(model.progress.floors>=2?2:1))&&model.progress.chapter>=3&&model.activeHero!=2)WorldLabel(new Vector3(5.2f,.2f,0),"하린 · 의료가방",Cream);
  }
  Label(new Rect(965,99,295,25),(model.SignatureRank>0?SliceModel.SignatureNames[model.activeHero]+" ★"+model.SignatureRank:model.activeHero==0?WeaponNames[model.progress.weapon-1]:model.activeHero==1?"분쇄 의수 LV "+model.progress.weapon:model.activeHero==3?"전격 창 LV "+model.progress.weapon:"전술 권총 LV "+model.progress.weapon)+"   [TAB 지도]",14,model.progress.weapon>=3?Amber:Cream);
  if(upgradeTimer>0){Box(new Rect(385,260,510,108),Panel);Label(new Rect(412,280,470,38),model.SignatureRank>0?SliceModel.SignatureNames[model.activeHero]+" · 각성 "+model.SignatureRank:"공용 무기 강화 · LV "+model.progress.weapon,24,Amber);Label(new Rect(412,324,470,28),model.SignatureRank>0?"개인 무기 피해·스킬 사거리 상승 · 공명 광휘":"동료 전체 피해량 상승 · 검날과 공격 광휘 변화",17,Cream);}
  if(journal||StoryOpen){
   Box(new Rect(0,0,1280,720),new Color(0,0,0,.8f));Box(new Rect(210,157,860,405),Panel);
   Label(new Rect(247,190,787,43),StoryOpen?storyTitle:"제7대피소 · 수색 기록",27,Amber);
   string body=StoryOpen?storyPages[storyPage]:"강이현, 28세. 붉은 목도리는 형 강준이 남긴 담요 조각이다.\n\n현재: "+model.MissionTitle+"\n"+Objective()+"\n\n주민: "+(model.progress.chapter>=2?"도윤":"이현")+(model.progress.chapter>=3?", 하린":"")+(model.progress.chapter>=4?", 석주":"")+(model.progress.chapter>=5?", 미정":"")+"\n비축: 물 "+model.progress.water+" / 붕대 "+model.progress.bandages+" / 판재 "+model.progress.boards;
   if(!StoryOpen&&lorePage>0)body=LoreJournal();
   var style=Style(21,Cream);style.wordWrap=true;GUI.Label(new Rect(247,251,785,240),body,style);
   Label(new Rect(247,510,780,27),StoryOpen?"ENTER / E  다음  ·  "+(storyPage+1)+" / "+storyPages.Length:"← / → 기록 넘기기 · "+(lorePage+1)+" / 8 · I 닫기 · TAB 지도",17,Cream);
  }
 }
}
}
