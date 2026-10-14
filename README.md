# 황혼선 — Duskline

**Unity 6.2 기반 2D 횡스크롤 태그 액션 생존 RPG**입니다. 해온시에서 좀비를 피해 물자를 회수하고 동료와 교대하며 싸워, 제7대피소를 유지하는 싱글플레이 시제품입니다.

## 현재 개발 버전

`Unity2D/`가 최신 프로젝트입니다. Unity Hub에서 이 폴더를 열고 `Assets/Scenes/Slice.unity`를 실행하세요. 에디터 버전은 **6000.2.0f1**입니다.

- 이현·도윤·하린·서린의 전투 태그, 공격 취소·교대 예약, 2인 협공.
- 캐릭터별 8프레임 보행과 준비·타격·반동·회복 스킬 연출. 걷기·공격 시트 배율 보정.
- 연결된 횡스크롤 수색 맵, 출정별 네 분위기, 돌진형·중장갑·산성 투척형·철도 집행자 보스.
- Tab 지도 출정·안전 귀환, 무기 LV6 강화와 보스 코어, 3층 쉘터 시설·생산·습격.
- 1~3장 임무와 4장 전반부: 재희의 교역로 표식 회수, 주황 우의 단서 및 보스 처치 후 귀환.

지역 기록 7종을 은닉처에서 회수하고 안전 귀환하면 I 기록 메뉴에 보관합니다. 좌우 방향키로 기록을 넘길 수 있습니다.

## 조작

A/D·방향키 이동, 아래 웅크리기, Space 점프, J 공격, Shift 회피, K 스킬, L 필살기, Q 태그, E 상호작용, H 치료, Tab 지도, I 기록, F2 연출 감소.

## 빌드

PowerShell 예시(에디터 경로는 설치 위치에 맞게 수정):

```powershell
$env:DUSKLINE_BUILD_OUTPUT = 'D:/Builds/Duskline'
& 'D:/Unity/6000.2.0f1/Editor/Unity.exe' -batchmode -nographics -quit -projectPath "$PWD/Unity2D" -buildTarget Win64 -executeMethod SliceBuild.Build -logFile 'D:/Builds/duskline-build.log'
```

출력 경로를 지정하지 않으면 프로젝트의 상위 `Builds/Duskline`에 저장됩니다. 빌드 과정에서 모델 검사를 실행합니다. `--capture <출력접두사>` 인자로 실행하면 보행·스킬 자동 화면 검사 후 종료합니다. 검사용 실행은 사용자 저장을 수정하지 않습니다.

## 검증과 범위

v0.11: 모델 검사172개, Windows 빌드 오류0·경고0. 실제 실행에서 보행32/32 프레임과 스킬4종 확인. 결과는 [검증 기록](Docs/Validation-v0.11.md), 변경 내용은 [CHANGELOG](CHANGELOG.md)를 참고하세요.

4장 후반 배신 사건과 성채 본편, 자유 가구 배치, 무기별 상세 제작 트리는 아직 미구현입니다. 지도는 연결된 기존 지형이며 절차적 지형 생성이 아닙니다. 새 4장 임무는 기존 구역을 재사용합니다.

## 문서

- [PRD](Docs/PRD/Duskline-Shelter-PRD.md)
- [전체 이야기 초안](Docs/Story/Duskline-Storyline.md): 기획 범위이며 전체가 구현된 것은 아닙니다.
- [아트 제작 기록](Docs/Art/): 내장 image_gen 생성 및 참조·프롬프트 기록.
- 이전 Unreal·3D 프로젝트 파일을 제거하고 최신 Unity2D 프로젝트로 전환했습니다.

실행 파일·Unity 캐시·개인 저장 파일은 Git에 포함하지 않습니다. `DuskFont.ttf`는 Source 계열 폰트를 Duskline Sans로 이름 변경한 파일이며 원본 저작권 표기와 [SIL OFL](Unity2D/ThirdPartyNotices/SourceHanSans-OFL.txt)을 유지합니다.
