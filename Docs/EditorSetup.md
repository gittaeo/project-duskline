# 에디터 설정

## 설치·빌드

1. 설치된 Epic Games Launcher를 열고 본인 계정으로 로그인합니다. Unreal Engine → 라이브러리 → 엔진 버전 추가 → 최신 **정식** UE 5를 선택합니다. Preview는 선택하지 않습니다. 유료 콘텐츠는 필요하지 않습니다.
2. 설치 경로를 기록합니다. C++ 빌드 도구는 이미 설치했습니다: VS Build Tools 2022 17.14.40, MSVC 14.44.35207, Windows SDK 10.0.26100.0. 엔진이 다른 toolchain을 요구하면 UBT 진단에 맞춰 무료 개별 구성 요소를 설치합니다.
3. `Scripts/Build.ps1 -EngineRoot '실제 엔진 루트'` 실행. C++ 전체 빌드가 성공하기 전 Python 스크립트를 실행하지 않습니다.
4. `.uproject`의 엔진 연결을 설치 버전으로 지정하고 엽니다. 필요한 플러그인은 Enhanced Input, Python Editor Script Plugin, Editor Scripting Utilities이며 프로젝트 파일에서 활성화됩니다.

## 자동 생성

1. 현재 편집 중인 레벨과 에셋을 저장합니다.
2. Tools → Execute Python Script → 프로젝트의 `Content/Python/build_duskline.py` 선택.
3. Output Log에서 예외가 없는지 확인합니다. `/Game/Items`의 Data Asset 4개와 `/Game/Maps/Duskline`이 생성돼야 합니다.
4. World Outliner에서 `Loot_00`~`Loot_06`, `Infected_0`~`Infected_2`, `SafeStart`, `Extraction_NE`를 확인합니다.
5. 스크립트는 기존 맵을 덮어쓰지 않습니다. 부분 생성 중 실패하면 로그 원인을 수정한 뒤 에디터에서 부분 맵을 다른 이름으로 보존하고 재시도하거나 누락된 요소를 아래 체크리스트로 보완합니다.

## 필수 NavMesh 수동 단계

1. Place Actors → Volumes → **Nav Mesh Bounds Volume**을 드래그합니다.
2. Location `(0,0,200)`, Rotation `(0,0,0)`, Scale `(1,1,1)`로 설정합니다.
3. Details → Brush Settings의 Box 크기 X=`10000`, Y=`8000`, Z=`1000`으로 설정합니다. 버전에 따라 Brush Settings → Advanced에 보일 수 있습니다. 기본 박스 크기 200을 사용하는 경우 Scale `(50,40,5)`로 같은 크기를 만들 수도 있습니다. 두 방법을 동시에 적용하지 않습니다.
4. Build → Build Paths를 실행합니다. `P`를 눌러 바닥의 녹색 NavMesh를 확인합니다. 실내 출입문, 중앙 골목, 동·서 우회로가 연결돼야 합니다.
5. 필요하면 RecastNavMesh의 Agent Radius를 기본 캐릭터 캡슐 반경 이상으로 맞추고 Runtime Generation=Dynamic인지 확인합니다. 길이 끊기면 먼저 장애물·바닥 충돌과 볼륨 높이를 확인합니다.
6. Save All 후 Play. NavMesh가 없으면 AI가 감지하더라도 이동하지 않습니다.

Python의 브러시 볼륨 생성은 엔진별 에디터 내부 구현 차이가 있어, 아직 확인하지 못한 브러시 API로 자동 생성했다고 가정하지 않았습니다.

## Blueprint·입력·UI

필수 Blueprint는 없습니다. 기본 GameMode, Character, Controller, UMG HUD, Input Action/Mapping Context는 C++에서 생성합니다. Project Settings → Maps & Modes에서 GameMode가 DusklineGameMode인지 확인합니다. 입력 기본 클래스는 EnhancedPlayerInput과 EnhancedInputComponent여야 합니다.

디자이너용 선택 사항:

1. C++ Classes → ProjectDuskline → Characters → DusklineCharacter 우클릭 → Create Blueprint class, `BP_DusklineCharacter` 생성.
2. Class Defaults에서 WalkSpeed=260, SprintSpeed=520, SprintDrain=24를 조정하고 각 컴포넌트의 체력·소음·무게 한도를 설정합니다.
3. DusklineGameMode 파생 `BP_DusklineGameMode`를 만들고 Default Pawn Class에 위 BP를 지정한 뒤 맵의 GameMode Override를 설정합니다.
4. GameState 파생 BP로 TimeLimit·RequiredValue를 조정할 경우 GameMode의 Game State Class에 연결합니다.
5. 현재 키 변경은 `SetupPlayerInputComponent`에서 합니다. 에디터 에셋형 Input Mapping을 사용하려면 해당 함수의 런타임 매핑을 대체하고 중복 등록하지 않습니다.

HUD는 이벤트 연결과 생성 직후 갱신을 C++에서 수행합니다. 한글이 네모로 표시되는 환경은 에디터 기본 폰트의 한국어 fallback을 확인하고, 허용된 한글 폰트 에셋으로 `UDusklineHUD`의 폰트를 지정합니다. 아이템의 임시 아이콘은 엔진 흰 사각형이며 현재 HUD는 이름과 수량을 표시합니다.

## 수동 맵 복원 체크리스트

생성 스크립트에 모든 좌표·치수가 있습니다. 기본 Cube는 100cm 기준입니다.

- 바닥 `(0,0,-50)`, 크기 `(10000,8000,100)`, BlockAll
- 시작 `(-4100,-3200,100)`
- 중앙 건물 두 개 사이 400cm 골목, 양끝 연결과 바깥 우회로
- 서북쪽 실내 `(-3500,1600)`에 벽·지붕과 폭 600cm 출입구
- LootContainer 7개: 스크립트 `positions` 배열 좌표, `LootTable`에 유효 ItemDataAsset 지정
- 감염체 3마리: `PatrolPoints` 월드 좌표 지정, Auto Possess AI=Placed in World or Spawned
- ExtractionZone `(4200,3200,100)`, Area 반경 박스 기본값, Countdown=8
- Directional Light, Sky Light, 실내 Point Light
- 위 NavMesh 볼륨, 경로 빌드, 저장

생성 결과의 조명·한글 표시·캐릭터 카메라·실내 충돌·NavMesh는 에디터에서 검증해야 합니다.
