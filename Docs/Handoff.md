# 개발 인수인계 — Project Duskline

## 현재 위치

2026-09-11에 빈 Windows 작업공간에서 작성한 UE 5 C++ 소스 프로젝트입니다. 사용자 목표는 약 10분 동안 탐색 → 루팅 → 전투/은신 → 탈출을 플레이하는 수직 슬라이스입니다. 원본 요구사항은 OriginalRequirements.md에 보존했습니다.

**다음 개발자는 전체 Unreal 빌드부터 시작해야 합니다.** 소스와 일부 독립 규칙이 검증되었을 뿐, 에디터에서 실행되는 완성 상태는 아닙니다. 엔진 미설치로 로컬 엔진 헤더 대조, UHT, UBT, PIE가 수행되지 않았습니다. 엔진 설치 후 발생하는 첫 컴파일 오류는 회귀 오류라고 단정하지 말고 최초 통합 검증으로 처리하세요.

## 저장소 내용과 제외 항목

- Source: 런타임 C++ 모듈, Game/Editor Target, Unreal Automation 테스트.
- Config: 기본 GameMode, 생성 예정 맵 경로, Enhanced Input, 동적 NavMesh 설정.
- Content/Python/build_duskline.py: 맵과 아이템 생성 스크립트. 일반 Python으로 실행할 수 없으며 C++ 모듈을 로드한 Unreal Editor가 필요합니다.
- Scripts: 전체 빌드, Unreal 테스트, 소스 정적 검사.
- Tests/RulesTest.cpp: 엔진 없이 실행 가능한 C++ 테스트.
- Validation: 초기 환경의 설치 로그와 검증 결과. 새 환경의 통과 결과가 아닙니다.
- Docs: 요구사항·설계·설정·테스트·현재 보고서.

UE 설치본, Visual Studio 설치본, 인증 정보, Intermediate/Binaries/Saved, 컴파일된 실행 파일은 추적하지 않습니다. `.uasset`과 `.umap`은 아직 생성되지 않았습니다. 추후 생성 결과를 커밋할 때는 대형 바이너리용 Git LFS 도입 여부를 검토하세요. 현재 `.gitattributes`는 바이너리로만 표시하며 LFS를 활성화하지 않습니다.

## 환경 복원

1. Windows에서 저장소를 clone합니다. 기존 작성 환경은 Windows x64입니다. 다른 플랫폼은 미검증입니다.
2. Epic Games Launcher에서 계정 로그인 후 최신 정식 UE 5를 설치하고 정확한 버전과 설치 경로를 기록합니다. 유료 에셋은 필요 없습니다.
3. VS C++ Build Tools와 Windows SDK를 설치합니다. 원래 확인된 도구는 VS 2022 17.14.40, MSVC 14.44.35207, SDK 10.0.26100.0입니다. 새 엔진이 요구하는 버전은 UBT 진단과 공식 지원표를 따릅니다.
4. `.uproject`의 EngineAssociation은 비어 있습니다. 설치 버전을 지정한 후 변경된 파일을 커밋하여 팀의 기준 버전을 고정합니다.

프로젝트 루트의 PowerShell에서 실행합니다. 아래 엔진 경로는 예시이며 반드시 실제 설치 경로로 바꿉니다.

```powershell
$engineRoot = 'C:\Program Files\Epic Games\UE_5.x'
powershell -ExecutionPolicy Bypass -File .\Scripts\Build.ps1 -EngineRoot $engineRoot
```

전체 빌드 성공 후 `.uproject`를 열고 EditorSetup.md의 스크립트·NavMesh 절차를 수행합니다. 스크립트는 기존 Duskline 맵을 덮어쓰지 않으므로 실패한 부분 맵은 보존·이름 변경 후 복구하세요.

## 처음 실행할 검증

```powershell
python .\Scripts\StaticCheck.py
powershell -ExecutionPolicy Bypass -File .\Scripts\Test.ps1 -EngineRoot $engineRoot
```

정적 검사는 Python 구문, include 존재, generated header 순서, 괄호 등만 검사합니다. C++ 컴파일을 대체하지 않습니다. Unreal 테스트 결과는 `Saved/Automation/index.json`으로 확인합니다. 독립 규칙 테스트의 MSVC 명령은 TestPlan.md에 있습니다.

초기 확인 결과: 독립 규칙 20개 통과, 정적 검사 190개 통과. 컴포넌트 Automation 4그룹은 작성만 했습니다. 이전 Validation 결과를 새 빌드의 증거로 사용하지 마세요.

## 핵심 구현 위치

| 변경 목적 | 시작할 파일 |
|---|---|
| 입력·카메라·속도·상호작용 거리 | Source/ProjectDuskline/Characters/DusklineCharacter.* |
| 체력·스태미나·전투·무게 | Source/ProjectDuskline/Components/ |
| 아이템 정의와 유효성 | Source/ProjectDuskline/Items/ItemDataAsset.h |
| 루팅 생성·부분 이전 | Source/ProjectDuskline/World/LootContainer.* |
| AI 감각·조사·상태 전이 | Source/ProjectDuskline/AI/DusklineAIController.* |
| 제한 시간·탈출·중복 결과 방지 | Source/ProjectDuskline/Game/DusklineGameState.* 및 RunRules.h |
| 구역 포함과 이탈 취소 | Source/ProjectDuskline/World/ExtractionZone.* |
| HUD·결과 버튼·이벤트 연결 | Source/ProjectDuskline/UI/DusklineHUD.* 및 Game/DusklinePlayerController.* |
| 맵 좌표·아이템 값·배치 | Content/Python/build_duskline.py |

기본 값: 시간 600초, 가치 180, 탈출 8초. 걷기 260cm/s, 달리기 520cm/s, 스태미나 비용 24/s, 회복 18/s와 지연 1.5초. 근접 피해 34, 비용 20, 쿨다운 0.65초. 감염체 체력 90, 피해 14, 공격 간격 1.25초. 컨테이너 7개와 감염체 3마리의 고정 좌표는 스크립트에 있습니다.

## 보존해야 할 동작

- 인벤토리 `bSuccess`는 전량 처리 여부입니다. 부분 획득은 `Processed`만 컨테이너에서 차감합니다.
- 한 번 열린 컨테이너는 재생성하지 않습니다. 재시작은 맵 재로드로 전체 상태를 초기화합니다.
- 시간 초과 > 사망 > 탈출 순서로 동일 시각의 종료를 판정하고, 확정 결과를 후속 요청이 바꾸지 않습니다.
- UI는 시스템 이벤트를 읽고, 명령은 Controller/게임 시스템에 전달합니다.
- 런타임 액터 참조는 weak pointer로 보관합니다. 전체 액터를 매 프레임 검색하지 않습니다.
- 네트워크는 미구현입니다. HasAuthority 검사만으로 서버 RPC·복제가 구현된 것으로 해석하면 안 됩니다.
- 기존 상용 작품의 고유 콘텐츠를 복제하지 않고 유료 에셋을 추가하지 않습니다.

## 다음 작업 우선순위

1. **P0 전체 빌드:** 설치 엔진 버전 확정 → UHT/UBT → 오류를 최소 수정으로 해결 → 정확한 명령·결과 기록.
2. **P0 레벨 실행:** Python API의 설치 버전 호환성 검증 → 4종 에셋·맵 생성 → NavMesh 배치 → PIE 이동·루팅 확인.
3. **P0 AI·루프:** 순찰/소음/추격/시야 상실/공격/죽음 확인 → 요구 가치 회수 → 탈출·실패·재시작 확인.
4. **P1 경계 검증:** TestPlan.md의 무게·슬롯·대상 파괴·시간 충돌·UI 수명 테스트. 관찰한 실패에 회귀 테스트 추가.
5. **P1 플레이 튜닝:** 실제 10분에 가까운 플레이 흐름, 난이도, 카메라, UI 크기, 한글 폰트 확인. 현재 600초 제한만으로 10분 플레이가 보장되지 않습니다.
6. **P1 전달 가능한 빌드:** Windows Development 패키징, 깨끗한 환경에서 실행, 실행 절차와 알려진 문제 갱신.

최초 완료 판정에는 전체 빌드 로그, 자동 테스트 결과, 맵 실행과 주요 인수 테스트 결과가 필요합니다. 부족한 기능을 삭제해서 테스트를 우회하지 말고 구현·요구사항 간 차이를 보고서에 남기세요.
