# 구현 보고서

## 결과

빈 작업공간에서 ProjectDuskline C++ 프로젝트를 새로 작성했습니다. 핵심 게임 루프의 코드, C++ UMG HUD, 기본 에셋 생성 스크립트, 테스트와 설정 문서를 제공합니다. **Unreal 전체 컴파일·플레이 실행이 검증되지 않았으므로 사용자 정의 완료 조건은 아직 충족하지 않았습니다.**

## 환경 조사·설치

- 기존 `.uproject`, 소스, Blueprint, Git 저장소 또는 사용자 변경 파일 없음. 기존 `work`, `outputs` 폴더는 보존.
- Epic 기본 설치 경로, HKLM/HKCU 엔진 등록, PATH, Launcher 설치 목록에서 엔진 설치본을 찾지 못함.
- 사용자 추가 지시에 따라 무료 도구 설치 수행: `winget install Microsoft.VisualStudio.2022.BuildTools`에 VCTools와 권장 구성 요소 지정, EpicGames.EpicGamesLauncher 설치. 두 설치 명령 모두 성공.
- `vswhere`로 C++ 도구 설치 확인. VS 17.14.40, MSVC 14.44.35207, Windows SDK 10.0.26100.0 확인.
- Epic Games Launcher는 `C:\Program Files\Epic Games\Launcher`에 설치됨. 엔진 다운로드는 계정 로그인과 버전 선택이 남음. 이 세션의 컴퓨터 사용 도구는 네이티브 앱 조작이 비활성화되어 Launcher 화면을 조작할 수 없음.
- 무료 개발 도구와 엔진 기본 콘텐츠만 사용. 유료 상품 구매·결제·유료 에셋 다운로드 없음.

## 구현 범위와 결정

Health/Stamina/Inventory/Combat/NoiseEmitter 컴포넌트, 인터페이스 루팅, Data Asset 아이템, 명시적 AI 상태 머신, 단일 권한 결과 처리, 런타임 Enhanced Input, 이벤트 UMG를 작성했습니다. 여러 시스템을 캐릭터 하나에 넣지 않았습니다.

Phase별 파일을 순차적으로 작성하고 연결부를 수정했습니다. 엔진이 없어 각 Phase의 컴파일 게이트는 실행하지 못했습니다. 의존 컴포넌트가 모두 작성되기 전 중간 단계가 컴파일됐다고 주장하지 않습니다. 최종 연결 검토에서 UI 소유 Pawn 생성 순서, 구역 이탈 즉시 취소, 시간 종료 시 물자 집계, 권한 검사, 잘못된 아이템 ID, Python enum 이름을 보완했습니다.

## 실제 실행한 검증

1. 설치된 MSVC x64 환경에서 `Tests/RulesTest.cpp`를 `/std:c++17 /EHsc /W4 /WX`로 컴파일: 성공, 경고 없음.
2. 생성 실행 파일 실행: **20 rule checks passed**, 종료 코드 0. 실제 런타임 코드가 사용하는 `RunRules.h` 검증.
3. `Scripts/StaticCheck.py`: Python AST, 로컬 include, generated header 순서, 괄호 균형, 문서 존재, 전체 액터 검색·직접 Actor Tick 금지 확인. 결과는 `Validation/StaticCheck.json` 참조.

검증 로그는 `Validation` 폴더에 저장합니다. 정적 검사는 UHT/C++ 타입 검사나 런타임 검증을 대체하지 않습니다.

## 미실행 항목

- Unreal Build Tool / Header Tool 전체 컴파일 및 버전별 API 검증
- Unreal Automation의 컴포넌트 테스트 4개 그룹
- `build_duskline.py` 에디터 실행, 실제 `.uasset`/`.umap` 생성과 저장
- NavMesh 생성, 경로 연결, AI 지각·이동 및 전투 플레이 검증
- 한국어 글꼴, UI 레이아웃, 충돌·카메라·조명 검증
- 10분 플레이와 경계 상황 인수 테스트, 패키징, 성능 측정

## 사람이 해야 하는 설정

Epic 계정 로그인 → UE 최신 정식 버전 설치 → Build.ps1 → 에디터 Python 스크립트 → NavMeshBoundsVolume과 Build Paths → Play. 상세 좌표와 체크리스트는 EditorSetup.md에 있습니다. 필수 Blueprint/입력/UI 수동 연결은 없습니다.

## 알려진 제한

실행 파일과 검증된 바이너리 맵은 없습니다. 소스는 UE 5.5 이상 API를 기준으로 작성했지만 설치 버전에서 빌드하기 전 호환성을 보장할 수 없습니다. Data Asset 임시 아이콘은 흰 사각형이며 HUD 목록은 텍스트입니다. 전투는 즉시 sweep, 플레이어 공격 연출은 짧은 텍스트, AI 피격은 도형 변화입니다. 애니메이션·사운드·온라인·영구 저장은 없습니다. 생성 맵의 플레이 시간과 난이도는 튜닝 전입니다.

## 다음 개발 우선순위

1. 실제 UE 버전과 설치 경로 확보 후 UHT/UBT 오류 수정.
2. Python 에디터 스크립트 실행 및 NavMesh 경로 검증.
3. 컴포넌트 Automation과 수동 인수 테스트 실행, 필요한 회귀 테스트 추가.
4. 플레이 타이밍·AI 난이도·UI 가독성 조정 후 Windows Development 패키지 생성.

## 생성 파일

전체 상대 경로는 `FileManifest.txt`에 기록됩니다. 주요 변경 그룹은 Source/ProjectDuskline의 Characters, Components, AI, Interaction, Items, World, Game, UI, Tests와 프로젝트 설정·빌드 스크립트·문서입니다. 기존 사용자 파일 수정·삭제는 없습니다.
