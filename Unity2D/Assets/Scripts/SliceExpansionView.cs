using UnityEngine;
namespace Duskline.Slice {
public sealed partial class SliceGame {
 // Calibrate authored body size, not the bounding box of a bent pose or extended weapon.
 float PoseScale(int id,Sprite sprite){
  if(id==0){foreach(var frame in combat)if(sprite==frame)return .88f;}
  if(id==1||id==2){foreach(var frame in skillFrames)if(sprite==frame)return id==1?.88f:.90f;}
  return 1;
 }
 void MatchActorScale(){
  hero.transform.localScale*=PoseScale(model.activeHero,hero.sprite);
  if(coopPartner.enabled)coopPartner.transform.localScale=Vector3.one*PoseScale(model.coopOutgoing,coopPartner.sprite);
 }
 void ChapterFourSignal(string value){
  if(value=="story-7")ShowScene("4-1 · 난간 위의 거래",new[]{
   "노란 후드의 재희는 방에 들어오지 않고 계단부터 살폈다.\n그가 남긴 교역로 표식은 실제 귀환로와 맞아떨어졌다.\n하지만 표식 뒷면에는 성채 운송 구역의 배급 번호가 찍혀 있었다.",
   "이현은 거래상의 짐에서 빛바랜 주황 천을 보았다.\n형이 마지막으로 입었던 우의와 같은 방수 안감이었다.\n폐철도 끝 보관함(86m)을 회수하고 집행자를 쓰러뜨려 단서를 가져오자."});
  if(value=="story-8")ShowScene("4-2 · 빛바랜 주황",new[]{
   "우의 안쪽에는 수선한 자국이 두 줄 있었다.\n이현이 어릴 때 형의 옷을 찢고 서툴게 꿰맸던 자리였다.\n함께 묶인 운송표에는 사람 대신 작업 번호만 적혀 있었다.",
   "재희는 성채 외곽의 운송노동 구역을 가리켰다.\n서린은 옥상 공동체의 중계망을 통해 번호를 대조하기로 했다.\n형의 생존은 아직 확인되지 않았다. 그러나 출정할 방향은 생겼다.",
   "4장 전반부 완료. 쉘터 유지와 장비 강화를 이어갈 수 있습니다.\n재희의 배신과 연우·성채 본편은 이후 이야기입니다."});
 }
}
}
