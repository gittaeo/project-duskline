using UnityEngine;
namespace Duskline.Slice {
public sealed partial class SliceGame {
 float bodyScale=1,bodyScaleVelocity;int bodyScaleHero=-1;
 // Calibrate authored body size, not the bounding box of a bent pose or extended weapon.
 float PoseScale(int id,Sprite sprite){
  if(sprite!=null&&walkScales.TryGetValue(sprite,out float gaitScale))return gaitScale;
  if(id==0){foreach(var frame in combat)if(sprite==frame)return .88f;if(sprite==skillFrames[0]||sprite==skillFrames[3])return .88f;}
  if(id==1||id==2){foreach(var frame in skillFrames)if(sprite==frame)return id==1?.88f:.90f;}
  return 1;
 }
 void MatchActorScale(){
  float target=PoseScale(model.activeHero,hero.sprite);if(bodyScaleHero!=model.activeHero){bodyScale=target;bodyScaleVelocity=0;bodyScaleHero=model.activeHero;}
  bodyScale=Mathf.SmoothDamp(bodyScale,target,ref bodyScaleVelocity,.075f,10,Mathf.Min(Time.deltaTime,.05f));hero.transform.localScale*=bodyScale;
  // Blade points were authored before final pose/scale selection. Follow the final transform.
  if(bladeEdge.enabled){
   if(model.skillTimer>0||model.coopTimer>0)bladeEdge.enabled=false;
   else for(int i=0;i<2;i++){
    Vector3 local=bladeEdge.GetPosition(i)-hero.transform.position;
    bladeEdge.SetPosition(i,hero.transform.position+hero.transform.rotation*Vector3.Scale(local,hero.transform.localScale));
   }
  }
  if(coopPartner.enabled)coopPartner.transform.localScale=Vector3.one*PoseScale(model.coopOutgoing,coopPartner.sprite);
 }
 void ChapterFourSignal(string value){
  if(value=="lore")Toast("지역 기록 발견 · 귀환 후 I 기록에서 확인");
  if(value=="story-9")ShowScene("4-3 · 노란 후드의 자백",new[]{"보관함은 안에서 열렸다. 도윤은 출정 시간과 운송표를 나란히 놓았다.\n재희가 고개를 숙였다. 성채에 붙잡힌 동생 연우를 돌려받는 대가로\n보관 위치를 넘겼다고 했다. 물 4와 식량 3을 잃었다.","본거지는 알리지 않았다는 말이 주민들의 빈 그릇을 채우지는 못했다.\n탈출자의 기록에는 연우가 이미 다른 시설로 옮겨졌다고 적혀 있었다.\n거래 상대에게는 처음부터 석방 권한이 없었다.","이현은 재희의 무기와 단독 출입 권한을 거뒀다. 당장 용서하지 않았다.\n31m·86m에서 복구 물자를 회수하고 집행자를 처치한 뒤 귀환하자.\n앞으로 재희가 고른 길은 태오와 서린이 먼저 검증한다."});
  if(value=="story-10")ShowScene("4장 끝 · 갚아야 할 물자",new[]{"복구 물자와 함께 돌아온 재희를 아무도 반기지 않았다.\n석주는 재희가 가까이 오자 물통을 들어 옮겼다.\n도움을 한 번 보탰다고 잃은 신뢰가 돌아오지는 않았다.","물 4·식량 6·회복 아티팩트 1개를 쉘터에 보관했다.\n태오는 준의 운송 번호가 성채 배급표와 연결되어 있다고 말했다.\n5장: 식량과 형의 기록을 얻으려면 조건이 붙은 계약을 마주해야 한다.","이번 버전은 4장 후반까지 진행됩니다. 성채 호송 본편은 다음 단계입니다.\nB 인벤토리에서 물자와 회복품을 확인하고, 재배와 휴식으로 다음 출정을 준비하세요."});
  if(value=="story-7")ShowScene("4-1 · 난간 위의 거래",new[]{
   "노란 후드의 재희는 방에 들어오지 않고 계단부터 살폈다.\n그가 남긴 교역로 표식은 실제 귀환로와 맞아떨어졌다.\n하지만 표식 뒷면에는 성채 운송 구역의 배급 번호가 찍혀 있었다.",
   "이현은 거래상의 짐에서 빛바랜 주황 천을 보았다.\n형이 마지막으로 입었던 우의와 같은 방수 안감이었다.\n폐철도 끝 보관함(86m)을 회수하고 집행자를 쓰러뜨려 단서를 가져오자."});
  if(value=="story-8")ShowScene("4-2 · 빛바랜 주황",new[]{
   "우의 안쪽에는 수선한 자국이 두 줄 있었다.\n이현이 어릴 때 형의 옷을 찢고 서툴게 꿰맸던 자리였다.\n함께 묶인 운송표에는 사람 대신 작업 번호만 적혀 있었다.",
   "재희는 성채 외곽의 운송노동 구역을 가리켰다.\n서린은 옥상 공동체의 중계망을 통해 번호를 대조하기로 했다.\n형의 생존은 아직 확인되지 않았다. 그러나 출정할 방향은 생겼다.",
   "물자 보관 지점의 무전이 끊겼다. 문은 부서지지 않았다고 한다.\n47m와 69m 보관함을 조사하고 돌아오자. 누군가 출정 일정을 알고 있었다."});
 }
}
}
