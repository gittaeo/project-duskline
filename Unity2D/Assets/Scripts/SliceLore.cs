using UnityEngine;
namespace Duskline.Slice {
public sealed partial class Progress { public int loreMask; }
public sealed partial class SliceModel {
 public int runLoreMask;
 public static readonly int[] LoreChapters={0,1,2,4,5,6,7};
 public static readonly string[] LoreTitles={"골목의 배급 명부","정비소의 호출부호","약국의 야간 기록","옥상 공동체의 신호","배수관 점검표","난간 거래 장부","성채 운송표"};
 public static readonly string[] LoreBodies={
  "같은 주소의 배급 인원이 여덟에서 셋으로 줄었다. 명부 끝에는 배급을 받지 않는 집에도 물을 놓고 가라는 메모가 있다. 제7대피소의 표식은 원래 구조대가 빈집에 남기던 표시였다.",
  "정비공들은 무전 대신 배관을 두 번 두드렸다. 한 번은 위험, 두 번은 통과. 도윤은 이 소리를 기억한다. 성채의 전력이 끊긴 뒤에도 골목의 작업자들은 자기들만의 길을 유지했다.",
  "하린의 약국은 환자 이름 옆에 치료비 대신 마지막으로 연락한 사람을 적었다. 지워진 무전 주파수 아래에는 ‘대답이 없어도 한 번 더’라는 문장이 남았다.",
  "옥상 공동체는 해 질 무렵 세 번 빛을 보낸다. 서린은 이를 안전 신호라고 부른다. 하지만 오늘의 마지막 빛은 강 건너에서 왔다. 아직 연결되지 않은 사람들이 있다.",
  "미정이 가져온 씨앗의 발아 실패는 흙만의 문제가 아니었다. 석주의 배수도에는 외곽 공장으로 이어지는 오래된 우회관이 표시되어 있다. 쉘터의 물은 도시의 다른 생존자들과 연결되어 있다.",
  "재희의 거래 장부에는 물자 값보다 통과 시간이 자세하다. 일부 행선지는 지워져 있고 같은 운송 번호가 반복된다. 그는 물건뿐 아니라 사람의 이동도 알고 있었던 것 같다.",
  "주황 우의와 함께 발견한 운송표. 이름 대신 작업 번호, 목적지 대신 교대 시간이 적혀 있다. 이현은 형의 생존을 확신할 수 없다. 서린은 옥상 중계망으로 번호를 추적하기로 한다."
 };
 void CollectLore(int index){
  int bit=1<<index;if(progress.chapter<LoreChapters[index]||((progress.loreMask|runLoreMask)&bit)!=0)return;
  runLoreMask|=bit;Emit("lore",x);
 }
}
public sealed partial class SliceGame {
 int lorePage;
 string LoreJournal(){
  int i=lorePage-1;bool saved=(model.progress.loreMask&(1<<i))!=0;bool carried=(model.runLoreMask&(1<<i))!=0;
  return SliceModel.LoreTitles[i]+"\n\n"+(saved?SliceModel.LoreBodies[i]:carried?"기록을 수색 가방에 보관했습니다. 안전하게 귀환하면 내용을 정리할 수 있습니다.":"아직 회수하지 못한 기록입니다. 이야기 진행 후 해당 구역의 은닉처를 다시 수색하세요.")+"\n\n"+(saved?"쉘터 기록 보관 완료":carried?"귀환 전 · 수색 실패 시 유실":"필요 진행: "+new[]{"첫 출정","1-2","1-3","3-1","3-2","4-1","4-2"}[i]);
 }
}
}
