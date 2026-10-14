using UnityEngine;
namespace Duskline.Slice {
public sealed partial class SliceGame {
 readonly LineRenderer[] weaponRadiance=new LineRenderer[8];float weaponPulse;Vector3 weaponOrigin;int pulseHero,pulseRank;bool pulseSkill;
 SpriteRenderer taeNpc,suhNpc;
 Color WeaponTint(){return WeaponTint(model.activeHero,model.SignatureRank);}
 Color WeaponTint(int id,int rank){
  Color hue=id==0?new Color(.45f,.85f,1):id==1?new Color(1,.35f,.06f):id==2?new Color(.35f,1,.72f):new Color(.7f,.32f,1);
  return rank>0?Color.Lerp(hue,Color.white,.15f):Color.Lerp(Cream,hue,Mathf.Clamp01((model.progress.weapon-1)/5f));
 }
 void SetupSignatureArt(){
  var tex=Resources.Load<Texture2D>("TaeoSuhyeong");
  taeNpc=Make("Taeo - rooftop scout",GridFrame(tex,2,1,0,tex.height*.96f/2.1f,.57f),new Vector3(2.8f,-2.6f,0),9);
  suhNpc=Make("Lee Suhyeong - infected witness",GridFrame(tex,2,1,1,tex.height*.96f/2.1f,.50f),new Vector3(58,-2.6f,0),9);
  var material=new Material(Shader.Find("Sprites/Default"));
  for(int i=0;i<weaponRadiance.Length;i++){var line=new GameObject("Weapon energy layer "+i).AddComponent<LineRenderer>();line.sharedMaterial=material;line.positionCount=48;line.sortingOrder=14+i;line.enabled=false;weaponRadiance[i]=line;}
 }
 void SignatureSignal(string value){
  if(value=="signature-forged"){
   upgradeTimer=2.5f;Toast(SliceModel.SignatureNames[model.activeHero]+" · 각성 "+model.SignatureRank);Burst(hero.transform.position+Vector3.up,WeaponTint(),reducedImpact?12:64);sound.PlayOneShot(skillClip,.4f);
  }
  if(value=="slash"||value=="skill-impact"||value=="signature-forged"||value=="upgrade"){
   weaponPulse=value=="skill-impact"?.6f:.4f;pulseHero=model.activeHero;pulseRank=model.SignatureRank;pulseSkill=value!="slash";weaponOrigin=new Vector3(model.x,-1.45f+model.y,0);
  }
  if(value=="taeo-talk")ShowScene("태오 · 지붕 위의 연락책",new[]{"청록 목도리의 태오는 먼저 무전을 끄고 웃었다. ‘돌아올 길부터 정하자.’\n그는 옥상 공동체와 제7대피소를 잇는 정찰자다.\n폐철도 입구 58m에, 아직 자기 이름을 말하는 감염자가 있다.","이수형은 성채 운송 경비대 출신이다. 결정화한 팔 때문에 경비대가 그를 버렸다.\n주변 감염체를 먼저 정리하고 E로 말을 걸자.\n그의 공명 설계도를 회수해 귀환하면 고유 무기를 제작할 수 있다."});
  if(value=="suhyeong-danger")Toast("이수형에게 접근하려면 주변 감염체와 거리를 벌리세요");
  if(value=="suhyeong-talk")ShowScene("이수형 · 이름을 잃지 않은 사람",new[]{"보랏빛 결정 아래서 이수형이 손을 움켜쥐었다.\n‘내 이름을… 한 번만 더 불러 줘.’\n태오의 호출부호를 듣자 그가 구겨진 설계도를 내밀었다.","‘이 코어는 사람마다 다르게 울린다. 성채는 그걸 알고 있어.’\n그는 함께 가자는 말에 고개를 저었다. 아직 발작을 억누를 수 없었다.\n설계도를 안전하게 가져가자. 치료나 그의 생존이 보장된 것은 아니다."});
  if(value=="blueprint-banked")ShowScene("태오 · 네 사람의 공명",new[]{"태오는 이수형의 이름을 수색 기록 맨 위에 적었다.\n도윤은 설계도를 작업대에 펼쳤다. 검, 의수, 권총, 창—각자의 코어가 다르게 빛났다.\n쉘터 TAB 지도에서 제작 대상을 선택해 고유 무기를 만들 수 있다."});
 }
 void RenderWeaponRadiance(){
  taeNpc.enabled=model.place==Place.Shelter&&model.shelterFloor==1&&model.progress.chapter>=6;
  suhNpc.enabled=model.place==Place.Expedition&&model.progress.chapter>=6;
  weaponPulse=Mathf.Max(0,weaponPulse-Time.deltaTime);int rank=model.SignatureRank;
  bool impact=weaponPulse>0;int id=impact?pulseHero:model.activeHero;int r=impact?pulseRank:rank;
  int layers=impact?Mathf.Min(8,model.progress.weapon/2+r*2):r>0?3:0;
  Color tint=WeaponTint(id,r);float u=1-weaponPulse/(pulseSkill?.6f:.4f);
  Vector3 origin=impact?weaponOrigin:hero.transform.position+Vector3.up*.95f;
  for(int layer=0;layer<8;layer++){
   var line=weaponRadiance[layer];line.enabled=layer<layers;if(!line.enabled)continue;
   float alpha=(impact?Mathf.Clamp01(1-u):.26f)*(reducedImpact?.35f:1);Color c=Color.Lerp(tint,Color.white,layer%3==0?.65f:0);c.a=alpha;
   line.startColor=c;line.endColor=new Color(tint.r,tint.g,tint.b,alpha*.1f);line.widthMultiplier=(layer%3==0?.026f:.07f)*(r>0?1.3f:1)*(reducedImpact?.5f:1);
   for(int j=0;j<48;j++){
    float t=j/47f;float a=t*Mathf.PI*2;Vector3 v;
    if(!impact){float rad=.28f+layer*.12f;v=new Vector3(Mathf.Cos(a+Time.time)*rad,Mathf.Sin(a+Time.time)*.5f,0);}
    else if(id==0){a=Mathf.Lerp(-1.7f,1.8f,t);float radius=.8f+u*2+layer*.12f;v=new Vector3(model.face*Mathf.Cos(a)*radius,Mathf.Sin(a)*radius*.7f,0);}
    else if(id==1){float radius=.3f+u*3.5f+layer*.12f;v=new Vector3(Mathf.Cos(a)*radius,-.9f+Mathf.Sin(a)*radius*.25f,0);}
    else if(id==2){v=new Vector3(model.face*(.5f+t*7),Mathf.Sin(t*14+layer)*.12f+(layer-3)*.08f,0);}
    else {v=new Vector3(model.face*(.2f+t*4.5f),Mathf.Sin(t*65+layer)*.15f+(layer-3)*.09f,0);}
    line.SetPosition(j,origin+v);
   }
  }
 }
 void DrawSignatureNPCs(){
  if(taeNpc.enabled)WorldLabel(new Vector3(2.8f,-.15f,0),"E 태오 · 공명 설계도",new Color(.4f,.9f,1));
  if(suhNpc.enabled)WorldLabel(new Vector3(58,-.15f,0),"E 이수형 · 잠식된 경비대원",new Color(.8f,.5f,1));
 }
}
}
