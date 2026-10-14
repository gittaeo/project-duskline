using UnityEngine;
namespace Duskline.Slice {
public sealed partial class SliceGame {
 MeshRenderer scoutWindup,scoutLunge;Sprite[] tagFrames,serinFrames,partyWalk,serinWalk;float tagFlash;LineRenderer upgradeRing;
 void SetupTagArt(){
  var scout=Resources.Load<Texture2D>("SerinActors-v3");
  serinFrames=new[]{ScoutFrame(scout,new Rect(55,512,320,512),215),ScoutFrame(scout,new Rect(395,512,465,512),680),ScoutFrame(scout,new Rect(890,512,646,512),1110),ScoutFrame(scout,new Rect(0,0,470,350),190),ScoutFrame(scout,new Rect(430,0,520,512),700),ScoutFrame(scout,new Rect(900,0,636,512),1150)};
  // The atlas windup tip extends left above the neighbouring crouch spear. Trim that lower corner in the sprite mesh.
  var sf=serinFrames[4];var pts=new[]{new Vector2(62,3),new Vector2(517,3),new Vector2(517,509),new Vector2(3,509),new Vector2(3,250),new Vector2(62,250)};
  // Explicit atlas mesh keeps the neighbouring crouch spear outside this pose.
  var verts=new Vector3[pts.Length];var uv=new Vector2[pts.Length];var colors=new Color[pts.Length];
  for(int i=0;i<pts.Length;i++){verts[i]=(pts[i]-sf.pivot)/210f;uv[i]=new Vector2((430+pts[i].x)/scout.width,pts[i].y/scout.height);colors[i]=Color.white;}
  var mesh=new Mesh();mesh.vertices=verts;mesh.uv=uv;mesh.colors=colors;mesh.triangles=new int[]{0,1,2,0,2,5,5,2,3,5,3,4};mesh.RecalculateBounds();
  var windup=new GameObject("Serin windup atlas mesh");windup.AddComponent<MeshFilter>().sharedMesh=mesh;scoutWindup=windup.AddComponent<MeshRenderer>();scoutWindup.sharedMaterial=new Material(Shader.Find("Sprites/Default"));scoutWindup.sharedMaterial.mainTexture=scout;scoutWindup.sortingOrder=10;scoutWindup.enabled=false;
  var lp=new[]{new Vector2(0,0),new Vector2(636,0),new Vector2(636,512),new Vector2(55,512),new Vector2(55,220),new Vector2(0,220)};
  var lv=new Vector3[6];var lu=new Vector2[6];for(int i=0;i<6;i++){lv[i]=(lp[i]-serinFrames[5].pivot)/210f;lu[i]=new Vector2((900+lp[i].x)/scout.width,lp[i].y/scout.height);}
  var lm=new Mesh();lm.vertices=lv;lm.uv=lu;lm.colors=colors;lm.triangles=new int[]{0,1,4,1,2,4,2,3,4,0,4,5};lm.RecalculateBounds();
  var lg=new GameObject("Serin lunge atlas mesh");lg.AddComponent<MeshFilter>().sharedMesh=lm;scoutLunge=lg.AddComponent<MeshRenderer>();scoutLunge.sharedMaterial=scoutWindup.sharedMaterial;scoutLunge.sortingOrder=10;scoutLunge.enabled=false;
  var walking=Resources.Load<Texture2D>("PartyWalk");partyWalk=new Sprite[8];for(int i=0;i<8;i++)partyWalk[i]=GridFrame(walking,4,2,i,walking.height*.48f/2.05f,.55f);
  walking=Resources.Load<Texture2D>("SerinWalk");serinWalk=new Sprite[4];for(int i=0;i<4;i++)serinWalk[i]=GridFrame(walking,4,1,i,walking.height*.83f/2.05f,.53f);
  upgradeRing=new GameObject("Upgrade shockwave").AddComponent<LineRenderer>();upgradeRing.material=new Material(Shader.Find("Sprites/Default"));upgradeRing.positionCount=49;upgradeRing.sortingOrder=19;upgradeRing.enabled=false;
  var t=Resources.Load<Texture2D>("TagActors");tagFrames=new Sprite[8];
  for(int i=0;i<8;i++)tagFrames[i]=GridFrame(t,4,2,i,210,.5f);
   // Wide claw strike needs a bespoke crop; uniform slicing clips it into the crouch cell.
  int h=t.height/2;
  tagFrames[2]=Sprite.Create(t,new Rect(880,h,585,h),new Vector2(.35f,.025f),210);
  tagFrames[3]=Sprite.Create(t,new Rect(1465,h,t.width-1465,h),new Vector2(.5f,.025f),210);
  var z=Resources.Load<Texture2D>("Brute");bruteFrames=new Sprite[3];
  for(int i=0;i<3;i++)bruteFrames[i]=GridFrame(z,3,1,i,240,.5f);
 }
 static Sprite ScoutFrame(Texture2D t,Rect rect,float rootX){
  int floor=(int)rect.height;var px=t.GetPixels32();for(int yy=0;yy<(int)rect.height;yy++)for(int xx=0;xx<(int)rect.width;xx++)if(px[((int)rect.y+yy)*t.width+(int)rect.x+xx].a>180){floor=Mathf.Min(floor,yy);break;}
  return Sprite.Create(t,rect,new Vector2((rootX-rect.x)/rect.width,floor/rect.height),210,0,SpriteMeshType.Tight);
 }
 Sprite[] bruteFrames;
 void RenderTagArt(){
  hero.enabled=true;scoutWindup.enabled=false;scoutLunge.enabled=false;
  if(model.activeHero>0){
   int pose=model.attack>0?2:model.crouching||model.roll>0?3:Mathf.Abs(model.velocityX)>.15f?1:0;
   hero.sprite=model.activeHero==3?serinFrames[pose]:tagFrames[(model.activeHero-1)*4+pose];
   if(pose==1&&model.y<=.01f){int frame=(int)(model.walkPhase*(model.activeHero==1?.53f:model.activeHero==3?.70f:.62f))%4;hero.sprite=model.activeHero==3?serinWalk[frame]:partyWalk[(model.activeHero-1)*4+frame];}
   bladeEdge.enabled=false;
  }
  if(model.place==Place.Shelter){if(model.activeHero==1)doy.enabled=false;if(model.activeHero==2)harin.enabled=false;}
  upgradeRing.enabled=upgradeTimer>1.3f;
  if(upgradeRing.enabled){float t=(2.5f-upgradeTimer)/1.2f;Color c=model.progress.weapon==3?Amber:new Color(.45f,.85f,1);c.a=1-t;upgradeRing.startColor=upgradeRing.endColor=c;upgradeRing.widthMultiplier=.06f*(1-t);
   for(int i=0;i<49;i++){float a=i/48f*Mathf.PI*2;upgradeRing.SetPosition(i,new Vector3(model.x+Mathf.Cos(a)*(.2f+t*2),-1.55f+Mathf.Sin(a)*(.2f+t*2),0));}
  }
  tagFlash=Mathf.Max(0,tagFlash-Time.deltaTime);
 }
 void TagSignal(string value){
  if(value=="tag-buffer")Toast("교대 예약 · 동작 종료 후 "+SliceModel.HeroNames[model.NextHero]);
  if(value=="tag"){
   tagFlash=.4f;slashLife=0;slash.enabled=false;
   Burst(new Vector3(model.x,-1.5f+model.y,0),model.activeHero==1?Amber:model.activeHero==2?new Color(.35f,1,.85f):new Color(.5f,.8f,1),40);
   Toast("TAG · "+model.HeroName+" / "+(model.activeHero==3?"전기 창 · 긴 사거리·감전":model.activeHero==1?"의수 · 중장갑 분쇄":model.activeHero==2?"권총 · 원거리 관통":"철검 · 3단 연계"));
   sound.pitch=1.2f;sound.PlayOneShot(lootClip,.15f);
  }
  if(value=="brute-slam"){Burst(new Vector3(model.lastEventX,-2.5f,0),new Color(.78f,.48f,.25f),35);shake=.2f;}
  if(value=="brute-warning"){sound.pitch=.45f;sound.PlayOneShot(alarmClip,.3f);}
 }
 void DrawTagHUD(){
  string roster="Q "+model.HeroName+" → "+SliceModel.HeroNames[model.NextHero]+" · "+(model.tagBuffer>0?"교대 예약":model.tagCooldown>0?model.tagCooldown.ToString("0.0")+"초":"준비");
  if(model.PartySize==1)roster="Q 태그 · 도윤 구조 후 개방";
  Box(new Rect(22,585,410,51),Panel);Label(new Rect(36,588,388,24),roster,17,Amber);
  Label(new Rect(36,612,388,20),"이현 / "+(model.progress.chapter>=2?"도윤":"미합류")+" / "+(model.progress.chapter>=3?"하린":"미합류")+" / 서린 · 협공 "+(model.coopCooldown>0?model.coopCooldown.ToString("0")+"초":"준비"),13,Cream);
  if(tagFlash>0)Box(new Rect(0,0,1280,720),new Color(.25f,.75f,1,tagFlash*.13f));
 }
}
}
