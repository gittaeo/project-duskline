using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Duskline.Slice {
public sealed partial class SliceGame:MonoBehaviour {
 SliceModel model;Camera cam;SpriteRenderer hero;GameObject world,shelter,gate;
 readonly List<SpriteRenderer> foes=new List<SpriteRenderer>(),crates=new List<SpriteRenderer>(),lids=new List<SpriteRenderer>();
 readonly List<Glint> glints=new List<Glint>();
 SpriteRenderer shelterBackdrop;Sprite[] frames,combat,props;SpriteRenderer gateArt;Sprite square;Font font;Texture2D pixel;
 AudioSource sound;AudioClip hitClip,lootClip,swingClip,alarmClip;
 LineRenderer slash;float slashLife,shake,hitSlow,messageUntil,ghostClock,pulseClock,screenDamage;
 string message="쉘터에서 출발해 보급품을 회수하세요",capturePath,savePath;
 bool paused,automated,awakenedCapture;float captureClock;int captureStage;Place shown=(Place)(-1);
 sealed class Glint {public Transform t;public Vector3 velocity;public float life;public SpriteRenderer sprite;}
 static readonly Color Cream=new Color(.90f,.85f,.71f),Amber=new Color(1,.67f,.24f),Panel=new Color(.035f,.055f,.07f,.94f);
 void Start(){
  Application.targetFrameRate=60;QualitySettings.vSyncCount=0;font=Resources.Load<Font>("DuskFont");pixel=Texture2D.whiteTexture;
  square=Sprite.Create(pixel,new Rect(0,0,1,1),new Vector2(.5f,.5f),1);
  string[] args=Environment.GetCommandLineArgs();awakenedCapture=Array.IndexOf(args,"--awakened")>=0;for(int i=0;i<args.Length-1;i++)if(args[i]=="--capture"){capturePath=args[i+1];automated=true;}
  savePath=Path.GetFullPath(Path.Combine(Application.dataPath,"..","Save","progress-v10.json"));
  model=new SliceModel(automated?null:LoadProgress());model.GrantStarterKit();model.Signal+=OnSignal;model.Save+=SaveProgress;
  cam=new GameObject("Side camera").AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=3.75f;cam.transform.position=new Vector3(0,-.25f,-10);cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.02f,.025f,.04f);cam.gameObject.AddComponent<AudioListener>();
  world=new GameObject("Four connected districts");shelter=new GameObject("Shelter rooms");
  for(int i=0;i<6;i++){
   var t=Resources.Load<Texture2D>(i>=4?"Railway":i%2==0?"Alley":"Yard");var r=Make("District "+i,Sprite.Create(t,new Rect(0,0,t.width,t.height),new Vector2(.5f,.5f),100),new Vector3(i*16.72f,i%2==0?0:-.15f,0),-10);r.transform.parent=world.transform;
   if(i>=4)r.transform.localScale=new Vector3(1672f/t.width,1,1);
   if(i>=2)r.color=new Color(.81f,.87f,.96f);
   if(i>0){var seam=Make("Utility pole",square,new Vector3(i*16.72f-8.36f,0,0),-9);seam.transform.localScale=new Vector3(.13f,9,1);seam.color=new Color(.08f,.09f,.11f);seam.transform.parent=world.transform;}
  }
  var interior=Resources.Load<Texture2D>("Shelter");var room=Make("Workshop and infirmary",Sprite.Create(interior,new Rect(0,195,interior.width,580),new Vector2(.5f,.5f),100),new Vector3(0,.48f,0),-10);room.transform.localScale=Vector3.one*1.35f;room.transform.parent=shelter.transform;shelterBackdrop=room;
  var propAtlas=Resources.Load<Texture2D>("Props");props=new Sprite[4];
  for(int i=0;i<4;i++)props[i]=PropFrame(propAtlas,i);
  gate=new GameObject("Shelter barricade");gateArt=Make("Timber barricade",props[2],new Vector3(-5.8f,-2.6f,0),7);gateArt.transform.parent=gate.transform;
  var atlas=Resources.Load<Texture2D>("Actors");frames=new Sprite[8];
  for(int row=0;row<2;row++)for(int col=0;col<4;col++){int left=col*384,width=384;float anchor=170;if(col==3){left=1115;width=421;anchor=185;}frames[row*4+col]=Sprite.Create(atlas,new Rect(left,row==0?512:0,width,512),new Vector2(anchor/width,row==0?7f/512:64f/512),190);}
  SetupStoryArt();SetupTagArt();SetupShelterArt();SetupActionFX();SetupWorldArt();SetupCoop();SetupMotionV09();SetupSignatureArt();
  hero=Make("Kang Ihyeon",motion[0],Vector3.zero,10);
  for(int i=0;i<15;i++)foes.Add(Make("Infected "+i,frames[4],Vector3.zero,9));
  for(int i=0;i<7;i++){crates.Add(Make("Supply "+i,props[0],Vector3.zero,8));lids.Add(Make("Lid placeholder "+i,props[1],Vector3.zero,9));}
  slash=new GameObject("Sword arc").AddComponent<LineRenderer>();slash.material=new Material(Shader.Find("Sprites/Default"));slash.sortingOrder=18;slash.positionCount=18;slash.useWorldSpace=true;slash.widthMultiplier=.08f;slash.enabled=false;
  sound=gameObject.AddComponent<AudioSource>();hitClip=Tone("hit",95,.12f);lootClip=Tone("loot",740,.3f);swingClip=Tone("swing",210,.09f);alarmClip=Tone("warning",340,.22f);
  messageUntil=Time.unscaledTime+7;SaveProgress();Render(0);if(!automated&&!model.progress.prologueSeen)ShowStory(0);else if(!automated&&model.progress.chapter==3&&!model.progress.rescueBriefed){model.progress.rescueBriefed=true;ShowScene("2장 · 문 앞의 두 사람",new[]{"석주가 다친 어머니를 부축하고 문 앞에 섰다.\n하린은 상처를 확인하고, 필요한 항생제가 없다고 말했다.\n이현은 형을 찾는 출정을 미룬다. 교역 은닉처(69m)에서 약을 구해야 한다."});SaveProgress();}
 }
 Progress LoadProgress(){
  foreach(string path in new[]{savePath,savePath+".bak","D:/Unity/Duskline2D-Lore-v11/Save/progress-v10.json","D:/Unity/Duskline2D-Expansion-v10/Save/progress-v10.json","D:/Unity/Duskline2D-Motion-v09/Save/progress-v9.json","D:/Unity/Duskline2D-Tag-v08/Save/progress-v8.json","D:/Unity/Duskline2D-World-v07/Save/progress-v7.json","D:/Unity/Duskline2D-Action-v06/Save/progress-v6.json","D:/Unity/Duskline2D-Shelter-v05/Save/progress-v5.json","D:/Unity/Duskline2D-Tag-v04/Save/progress-v4.json","D:/Unity/Duskline2D-Story-v03/Save/progress-v3.json","D:/Unity/Duskline2D-Combat-v02/Save/progress-v2.json"})if(File.Exists(path))try{var data=JsonUtility.FromJson<Progress>(File.ReadAllText(path));if(data!=null){data.Validate();return data;}}catch(Exception e){Debug.LogWarning("Save recovery: "+e.Message);}
  return new Progress();
 }
 void SaveProgress(){
  if(automated)return;
  try{Directory.CreateDirectory(Path.GetDirectoryName(savePath));string temp=savePath+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(model.progress,true));if(File.Exists(savePath))File.Replace(temp,savePath,savePath+".bak");else File.Move(temp,savePath);}
  catch(Exception e){Debug.LogError("Save failed: "+e.Message);Toast("저장 실패 · 로그를 확인해주세요");}
 }
 void Toast(string value){message=value;messageUntil=Time.unscaledTime+4;}
 void OnSignal(string value){
  StorySignal(value);TagSignal(value);ShelterSignal(value);ActionSignal(value);WorldSignal(value);CoopSignal(value);MotionSignal(value);ChapterFourSignal(value);SignatureSignal(value);
  switch(value){
   case "depart":Toast("골목 → 정비소 → 진료소 → 폐철도 · 양 끝에서 귀환");break;
   case "loot":Burst(new Vector3(model.lastEventX,-2,0),Amber,30);sound.PlayOneShot(lootClip,.2f);Toast("보급품 확보  ·  수색 금속 "+model.carried);break;
   case "hit":case "down":Burst(new Vector3(model.lastEventX,-1.5f,0),Cream,value=="down"?20:12);sound.PlayOneShot(hitClip,.25f);shake=.09f;hitSlow=.06f;break;
   case "damage":screenDamage=.35f;shake=.18f;sound.PlayOneShot(hitClip,.35f);break;
   case "gate-hit":shake=.12f;Toast("방어문이 공격받고 있습니다!");break;
   case "swing":sound.pitch=1+model.combo*.2f;sound.PlayOneShot(swingClip,.18f);break;
   case "slash":ShowSlash();break;
   case "runner-warning":sound.pitch=.7f;sound.PlayOneShot(alarmClip,.15f);break;
   case "raid":case "drill":Toast(value=="drill"?"습격 연습  ·  왼쪽 방어문을 지키세요":"습격 시작! 왼쪽 방어문을 지키세요");sound.pitch=1;sound.PlayOneShot(alarmClip,.3f);break;
   case "extract":Toast(model.progress.raidPending?"귀환 완료 · 12초 뒤 좀비 습격! 방어문을 점검하세요":"생존 귀환  ·  금속이 창고에 보관되었습니다");break;
   case "failed":Toast("수색 실패 · 획득 금속 70% 회수 후 쉘터 복귀");break;
   case "raid-win":Toast("방어 성공! 금속 8 획득");break;
   case "raid-loss":Toast("방어 실패 · 금속 최대 10 손실, 쉘터는 유지됩니다");break;
   case "drill-end":Toast("습격 연습 종료 · 보상과 방어문 변화는 적용하지 않습니다");break;
   case "upgrade":Toast("공용 무기 강화 완료 · LV "+model.progress.weapon);sound.PlayOneShot(lootClip,.2f);break;
   case "repair":Toast("방어문 내구도 +30");break;
   case "rest":Toast("안전한 쉘터 · 체력이 회복되었습니다");break;
   case "max-level":Toast("현재 최대 강화 단계입니다");break;
   case "need-metal":Toast("강화에 금속 "+(12*model.progress.weapon)+" 필요");break;
   case "repair-unavailable":Toast("수리는 금속 5 필요 · 내구도 100이면 수리 불필요");break;
  }
 }
 void ShowSlash(){
  slashLife=.18f;slash.enabled=true;
  Color c=model.activeHero==1?new Color(1,.48f,.12f):model.activeHero==2?new Color(.35f,1,.85f):model.progress.weapon>=3?Amber:model.progress.weapon==2?new Color(.5f,.85f,1):Cream;if(model.progress.weapon==3)c=Color.Lerp(c,Amber,.6f);c=WeaponTint();slash.startColor=slash.endColor=c;
  for(int i=0;i<18;i++){float t=i/17f;Vector3 p;
   if(model.activeHero==3)p=new Vector3(.4f+t*3.2f,1.1f+Mathf.Sin(t*50)*.06f,0);
   else if(model.activeHero==2)p=new Vector3(.95f+t*6.2f,1.55f,0);
   else if(model.combo==0)p=new Vector3(.5f+t*1.55f,1.05f+t*.04f,0);
   else{float a=(model.combo==1?Mathf.Lerp(-70,65,t):Mathf.Lerp(115,-55,t))*Mathf.Deg2Rad;p=new Vector3(Mathf.Cos(a)*1.65f,1+Mathf.Sin(a)*1.45f,0);}
   p.x*=model.face;slash.SetPosition(i,new Vector3(model.x,-2.6f+model.y,0)+p);
  }
  if(model.activeHero==2)Burst(new Vector3(model.x+model.face*1.2f,-1.55f+model.y,0),c,12);
  if(model.activeHero==1){Burst(new Vector3(model.x+model.face*1.4f,-1.7f,0),new Color(1,.48f,.12f),24);shake=.17f;}
  if(model.combo==2){Burst(new Vector3(model.x+model.face*1.1f,-2.5f,0),Amber,22);shake=.13f;}
 }
 void Update(){
  if(model==null)return;if(!automated&&NarrativeInput())return;if(Input.GetKeyDown(KeyCode.Escape))paused=!paused;
  float raw=Time.deltaTime;if(!paused){
   float move=(Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0);
   if(!automated){if(Input.GetKeyDown(KeyCode.F2))reducedImpact=!reducedImpact;if(Input.GetKeyDown(KeyCode.K))model.Skill();if(Input.GetKeyDown(KeyCode.L))model.Skill(true);if(Input.GetKeyDown(KeyCode.W)||Input.GetKeyDown(KeyCode.UpArrow))model.ChangeFloor(1);if(Input.GetKeyDown(KeyCode.R))model.ChangeFloor(-1);if(Input.GetKeyDown(KeyCode.H)&&!model.Heal())Toast("치료: 수색 중 붕대 1개 필요 · 재사용 12초 · 체력 부족 시 사용");}
   if(Input.GetKeyDown(KeyCode.Q)&&!automated){if(!model.Tag())Toast(model.PartySize<2?"스토리에서 동료를 구조하면 태그가 열립니다":"태그 재사용 "+model.tagCooldown.ToString("0.0")+"초 · 끝나기 직전 예약 가능");}
   if(Input.GetKeyDown(KeyCode.F6)&&model.place==Place.Shelter&&!model.progress.raidPending)model.StartRaid(true);
   float dt=impactStop>0?raw*.04f:hitSlow>0?raw*.08f:raw;hitSlow=Mathf.Max(0,hitSlow-raw);
   if(!automated)model.Tick(dt,move,Input.GetKeyDown(KeyCode.Space),Input.GetKeyDown(KeyCode.J),Input.GetKeyDown(KeyCode.LeftShift),Input.GetKeyDown(KeyCode.E),Input.GetKey(KeyCode.DownArrow)||Input.GetKey(KeyCode.S));
   else Automation(raw);
   Render(move);Effects(raw);
   pulseClock-=raw;
   if(model.place!=Place.Shelter&&model.hp<40&&pulseClock<=0){sound.pitch=.6f;sound.PlayOneShot(hitClip,.15f);pulseClock=.9f;}
  }
 }
 void Render(float move){
  bool indoors=model.place==Place.Shelter;
  world.SetActive(!indoors);shelter.SetActive(indoors);gate.SetActive(model.place==Place.Raid);gateArt.sprite=props[model.progress.gate<50?3:2];
  if(shown!=model.place){shown=model.place;cam.transform.position=new Vector3(indoors?0:model.place==Place.Raid?0:-1.5f,-.25f,-10);slash.enabled=false;slashLife=0;foreach(var p in glints)Destroy(p.t.gameObject);glints.Clear();}

  hero.flipX=model.face<0;hero.transform.position=new Vector3(model.x,-2.6f+model.y,0);
  hero.transform.localScale=Vector3.one;RenderStoryArt();RenderTagArt();RenderShelter();RenderWalkV09();RenderActionFX();RenderSkillMotion();RenderWorldWeather();RenderCoop();MatchActorScale();RenderWeaponRadiance();
  hero.color=model.invulnerable>0&&Mathf.Sin(model.time*45)>0?new Color(1,.65f,.6f,.7f):Color.white;
  for(int i=0;i<foes.Count;i++){
   var sr=foes[i];bool active=i<model.enemies.Count&&model.enemies[i].hp>0&&model.enemies[i].delay<=0&&!indoors;sr.enabled=active;if(!active)continue;
   var e=model.enemies[i];sr.flipX=e.attack>0||e.dash>0?e.direction<0:e.x>model.x;
   sr.sprite=frames[4+(e.attack>0||e.dash>0?3:e.memory>0?1+(int)(model.time*(e.runner?7:5))%2:0)];
   if(e.brute)sr.sprite=bruteFrames[e.attack>0?2:e.memory>0?1:0];
   if(e.boss)sr.sprite=worldEnemies[e.attack>0||e.dash>0?1:0];else if(e.spitter)sr.sprite=worldEnemies[e.attack>0?3:2];
   sr.transform.localScale=Vector3.one;
   sr.transform.position=new Vector3(e.x,-2.6f+e.y,0);sr.color=e.hurt>0?new Color(1,.6f,.45f):e.brute?Color.white:e.runner?new Color(.86f,.82f,.76f):Color.white;
  }
  for(int i=0;i<crates.Count;i++){
   bool active=model.place==Place.Expedition&&i<model.supplies.Count;crates[i].enabled=active;lids[i].enabled=false;if(!active)continue;
   var s=model.supplies[i];crates[i].sprite=props[s.opened?1:0];crates[i].transform.position=new Vector3(s.x,-2.6f,0);lids[i].transform.position=new Vector3(s.x,s.opened?-2.1f:-2.6f,0);lids[i].transform.rotation=Quaternion.Euler(0,0,s.opened?12:0);
  }
  float half=cam.orthographicSize*cam.aspect;float left=indoors?-11.286f:-8.36f,right=indoors?11.286f:model.place==Place.Raid?8.36f:91.96f;
  float target=Mathf.Clamp(model.x+model.face*.7f,left+half,right-half);
  var desired=new Vector3(target,-.25f,-10);cam.transform.position=Vector3.Lerp(cam.transform.position,desired,1-Mathf.Exp(-Time.deltaTime*6));
  if(shake>0&&!reducedImpact)cam.transform.position+=new Vector3(Mathf.Sin(Time.unscaledTime*173),Mathf.Cos(Time.unscaledTime*127),0)*shake*.22f;
 }
 void Effects(float dt){
  shake=Mathf.Max(0,shake-dt);screenDamage=Mathf.Max(0,screenDamage-dt);slashLife=Mathf.Max(0,slashLife-dt);slash.enabled=slashLife>0;slash.widthMultiplier=.10f*slashLife/.18f;
  ActionEffects(dt);MotionEffects(dt);ghostClock-=dt;if((model.roll>0||model.attack>0||model.skillTimer>0)&&hero.enabled&&ghostClock<=0){ghostClock=.04f;var r=Make("Motion echo",hero.sprite,hero.transform.position,8);r.flipX=hero.flipX;r.transform.localScale=hero.transform.localScale;r.transform.rotation=hero.transform.rotation;r.color=new Color(.65f,.82f,1,.22f);glints.Add(new Glint{t=r.transform,sprite=r,life=.14f});}
  for(int i=glints.Count-1;i>=0;i--){var p=glints[i];p.life-=dt;p.t.position+=p.velocity*dt;p.velocity+=Vector3.down*dt*.2f;var c=p.sprite.color;c.a=Mathf.Min(c.a,p.life*2);p.sprite.color=c;if(p.life<=0){Destroy(p.t.gameObject);glints.RemoveAt(i);}}
 }
 readonly bool[,] seenWalk=new bool[4,8];int motionHero=-1,skillShots,attackShots;readonly bool[,] seenAttack=new bool[4,3];bool motionSkill,motionCaptured;
 void Automation(float dt){
  captureClock+=dt;if(captureClock<.5f)return;
  int id=(int)((captureClock-.5f)/6.5f);float phase=(captureClock-.5f)%6.5f;
  if(id>=5){int seen=0;foreach(bool value in seenWalk)if(value)seen++;File.WriteAllText(capturePath+"-run.txt","Walk frames "+seen+"/32\nSkill captures "+skillShots+"/4\nAttack captures "+attackShots+"/12");Application.Quit();return;}
  if(id==4){
   model.progress.chapter=6;model.attack=model.skillTimer=model.velocityX=0;model.queued=false;
   if(phase<2){model.place=Place.Expedition;model.x=55.5f;cam.transform.position=new Vector3(57,-.25f,-10);if(captureStage==0&&phase>.8f){ScreenCapture.CaptureScreenshot(capturePath+"-suhyeong.png");captureStage=1;}}
   else {model.place=Place.Shelter;model.shelterFloor=1;model.x=0;if(captureStage==1&&phase>3){ScreenCapture.CaptureScreenshot(capturePath+"-taeo.png");captureStage=2;}}
   return;
  }
  if(id!=motionHero){if(awakenedCapture){model.progress.weapon=6;model.progress.resonanceBlueprint=true;for(int n=0;n<4;n++)model.progress.signatureRanks[n]=3;}motionHero=id;motionSkill=motionCaptured=false;if(model.place==Place.Shelter){model.progress.chapter=3;model.Depart();}model.enemies.Clear();model.x=18;model.y=model.vy=0;model.activeHero=id;model.skillTimer=0;model.skillCooldowns[id]=0;model.stamina=100;cam.transform.position=new Vector3(18,-.25f,-10);shown=Place.Expedition;}
  model.Tick(dt,phase<2.2f?1:0,false,phase>4.2f&&phase<5.9f,false,false);
  if(phase>4.2f&&model.attackHit&&!seenAttack[id,model.combo]){seenAttack[id,model.combo]=true;attackShots++;ScreenCapture.CaptureScreenshot(capturePath+"-attack-"+id+"-"+model.combo+".png");}
  if(phase<2.2f){int frame=WalkFrame();if(!seenWalk[id,frame]){seenWalk[id,frame]=true;if(frame==3||frame==6)ScreenCapture.CaptureScreenshot(capturePath+"-walk-"+id+"-"+frame+".png");}}
  if(phase>=2.3f&&!motionSkill){model.Skill();motionSkill=true;}
  if(motionSkill&&!motionCaptured&&model.skillElapsed>(id==1?.42f:.22f)){ScreenCapture.CaptureScreenshot(capturePath+"-skill-"+id+".png");motionCaptured=true;skillShots++;}
 }
 void OnGUI(){
  if(model==null)return;float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);
  GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1280*scale)/2,(Screen.height-720*scale)/2,0),Quaternion.identity,new Vector3(scale,scale,1));
  bool inside=model.place==Place.Shelter,raid=model.place==Place.Raid;
  string zone=inside?"쉘터 "+model.shelterFloor+"층":raid?(model.drill?"습격 연습":"쉘터 방어"):model.x>78&&model.progress.chapter>=4?"철도 끝 온실":model.x>58?"폐철도 화물기지":model.x<8?"약국 골목":model.x<25?"폐상가":model.x<42?"정비 구역":model.progress.chapter==2?"임시 진료소":"보급 창고";
  Box(new Rect(22,20,284,73),Panel);Label(new Rect(36,27,260,25),"DUSKLINE  /  "+zone,17,Cream);
  Box(new Rect(36,61,254,13),new Color(.2f,.08f,.08f));Box(new Rect(36,61,254*model.hp/100f,13),new Color(.66f,.21f,.20f));Label(new Rect(42,55,238,25),"HP "+model.hp+" / 100",13,Color.white);
  Box(new Rect(933,20,325,73),Panel);Label(new Rect(948,28,300,24),inside?"창고 금속 "+model.progress.metal+"   |   무기 LV "+model.progress.weapon:raid?"방어문 "+model.progress.gate+" / 100":"수색 금속 "+model.carried+"   |   창고 "+model.progress.metal,17,Amber);
  Label(new Rect(948,57,300,22),inside?"생존 귀환 "+model.progress.expeditions+"회":raid?"남은 좀비 "+model.enemies.FindAll(e=>e.hp>0).Count+" / 6  ·  방어문을 지키세요":"양 끝 귀환 지점  ·  상자 7개",14,Cream);
  Box(new Rect(22,653,844,45),Panel);Label(new Rect(36,663,825,27),"A D 이동  SPACE 점프  ↓ 웅크리기  J 공격  SHIFT 회피  E 사용",16,Cream);
  Box(new Rect(885,653,373,45),Panel);Label(new Rect(903,663,345,27),inside?"F6 습격 연습  ·  ESC 일시정지":"K 스킬 · L 필살기 · F2 연출 강도",15,Cream);
  if(model.attack>0){Box(new Rect(885,585,373,51),Panel);Label(new Rect(903,596,345,28),(model.combo+1)+"  /  "+model.ComboName+(model.queued?"  ·  연계 예약":""),19,Amber);}
  if(Time.unscaledTime<messageUntil){Box(new Rect(319,106,639,40),Panel);Label(new Rect(334,113,614,28),message,16,Amber);}
  if(inside){
   if(model.raidWarning>=0){Box(new Rect(350,159,580,42),new Color(.30f,.055f,.04f,.95f));Label(new Rect(370,167,555,28),"습격 경보  "+Mathf.CeilToInt(model.raidWarning)+"초 · 문을 수리하고 대비하세요",19,Cream);}
   if(model.shelterFloor==1){WorldLabel(new Vector3(-7,-.25f,0),model.progress.weapon>=6?"강화 완료":"E 무기 강화 · 금속 "+12*model.progress.weapon,Amber);
   WorldLabel(new Vector3(0,-.25f,0),"E 휴식",Cream);
   WorldLabel(new Vector3(5.7f,-.25f,0),"E 방어문 수리 · 금속 5",Cream);
   WorldLabel(new Vector3(9,-.25f,0),model.progress.raidPending?"E 즉시 방어":"E 수색 출발",Amber);}
  }else{
   foreach(var e in model.enemies)if(e.hp>0&&e.delay<=0&&e.attack>0)WorldLabel(new Vector3(e.x,-.25f+e.y,0),e.spitter?"산성탄! 웅크리기":e.brute?"강타! · 뒤로 회피":e.runner?"돌진!":"!",new Color(1,.35f,.2f));
   if(!raid){
    foreach(var s in model.supplies)if(!s.opened&&Mathf.Abs(model.x-s.x)<1.25f&&model.attack<=0&&model.skillTimer<=0&&model.roll<=0&&model.counterWindow<=0)WorldLabel(new Vector3(s.x,-1.4f,0),"E 보급 상자",Amber);
    if(model.x<-5.8f||model.x>89.5f)WorldLabel(new Vector3(model.x,-.2f,0),model.carried>0?"E 쉘터 귀환":"보급품 회수 후 귀환",Cream);
    Box(new Rect(490,37,300,7),new Color(.2f,.25f,.28f,.8f));Box(new Rect(490,37,300*Mathf.InverseLerp(-7,90.6f,model.x),7),Amber);Label(new Rect(490,48,310,24),"골목 → 정비소 → 진료소 → 폐철도",13,Cream);
   }else WorldLabel(new Vector3(-5.8f,-1.4f,0),"방어문  "+model.progress.gate,Amber);
  }
  if(screenDamage>0)Box(new Rect(0,0,1280,720),new Color(.65f,0,0,screenDamage*.32f));
  DrawSignatureNPCs();DrawTagHUD();DrawShelterHUD();DrawActionHUD();DrawNarrative();DrawWorldHUD();
  if(paused){Box(new Rect(0,0,1280,720),new Color(0,0,0,.7f));Box(new Rect(370,236,540,230),Panel);Label(new Rect(402,264,475,40),"일시 정지",30,Cream);Label(new Rect(402,321,475,70),"J키를 연이어 눌러 3단 공격\n좀비의 붉은 예고를 보고 SHIFT로 회피",19,Amber);Label(new Rect(402,411,475,28),"ESC로 계속 · 마우스 클릭은 공격하지 않습니다",16,Cream);}
 }
 void WorldLabel(Vector3 pos,string text,Color color){var p=cam.WorldToViewportPoint(pos);if(p.x<.05f||p.x>.95f)return;float x=p.x*1280,y=(1-p.y)*720;Box(new Rect(x-110,y-16,220,30),Panel);Label(new Rect(x-101,y-13,206,28),text,14,color);}
 GUIStyle Style(int size,Color color){var s=new GUIStyle(GUI.skin.label){font=font,fontSize=size,alignment=TextAnchor.MiddleLeft};s.normal.textColor=color;return s;}
 void Label(Rect r,string t,int n,Color c){GUI.Label(r,t,Style(n,c));}
 void Box(Rect r,Color c){GUI.color=c;GUI.DrawTexture(r,pixel);GUI.color=Color.white;}
 SpriteRenderer Make(string name,Sprite sprite,Vector3 position,int order){var g=new GameObject(name);g.transform.position=position;var r=g.AddComponent<SpriteRenderer>();r.sprite=sprite;r.sortingOrder=order;return r;}
 void Burst(Vector3 pos,Color color,int count){for(int i=0;i<count&&glints.Count<160;i++){var r=Make("Glint",square,pos,20);r.transform.localScale=Vector3.one*UnityEngine.Random.Range(.025f,.065f);r.color=color;glints.Add(new Glint{t=r.transform,sprite=r,life=UnityEngine.Random.Range(.4f,1.2f),velocity=new Vector3(UnityEngine.Random.Range(-1.2f,1.2f),UnityEngine.Random.Range(.4f,1.7f),0)});}}
 static Sprite PropFrame(Texture2D atlas,int index){
  int cw=atlas.width/2,ch=atlas.height/2,left=(index%2)*cw,bottom=index<2?ch:0;
  var pixels=atlas.GetPixels32();int minX=cw,minY=ch,maxX=0,maxY=0;
  for(int y=0;y<ch;y++)for(int x=0;x<cw;x++)if(pixels[(bottom+y)*atlas.width+left+x].a>128){minX=Mathf.Min(minX,x);maxX=Mathf.Max(maxX,x);minY=Mathf.Min(minY,y);maxY=Mathf.Max(maxY,y);}
  var rect=new Rect(left+minX,bottom+minY,maxX-minX+1,maxY-minY+1);
  return Sprite.Create(atlas,rect,new Vector2(.5f,0),index<2?rect.width/1.35f:rect.height/2.1f);
 }
 static AudioClip Tone(string name,float frequency,float seconds){int n=(int)(44100*seconds);var data=new float[n];for(int i=0;i<n;i++){float t=i/44100f;data[i]=Mathf.Sin(t*frequency*6.28318f)*Mathf.Pow(1-i/(float)n,2)*.3f;}var clip=AudioClip.Create(name,n,1,44100,false);clip.SetData(data,0);return clip;}
}
}
