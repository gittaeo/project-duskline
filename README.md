# 황혼선 — Project Duskline

가상의 해원시에서 물자를 회수하고 탈출하는 3인칭 싱글플레이 생존 액션 프로토타입입니다. 기본 제한 시간 600초, 요구 가치 180, 탈출 대기 8초입니다. 독창적인 명칭과 기본 도형만 사용하며 외부 유료 에셋은 없습니다.

**현재 상태: C++ 소스 구현 및 일부 독립 규칙 검증 완료. Unreal Engine 전체 컴파일, 맵 생성 실행, 플레이 검증은 아직 완료하지 못했습니다. 완성된 실행 파일이나 검증된 수직 슬라이스로 간주하면 안 됩니다.**

## 다른 환경에서 개발 이어가기

```sh
git clone https://github.com/gittaeo/project-duskline.git
cd project-duskline
```

먼저 [개발 인수인계](Docs/Handoff.md)를 읽으세요. 원본 요구사항부터 구현 구조, 검증 근거, 남은 작업까지 다음 문서로 연결됩니다.

- [원본 요구사항](Docs/OriginalRequirements.md): 기능·안정성·완료 기준 전체
- [개발 인수인계](Docs/Handoff.md): 환경 복원, 다음 작업 순서, 주의할 검증 공백
- [구조와 설계](Docs/Architecture.md): 클래스 책임과 데이터 흐름
- [에디터 설정](Docs/EditorSetup.md): 맵 생성·NavMesh·Blueprint 설정
- [테스트 계획](Docs/TestPlan.md): 자동 테스트 및 수동 인수 조건
- [구현 보고서](Docs/ImplementationReport.md): 작성 범위와 미검증 사항

이 저장소에는 소스와 맵 생성 스크립트가 포함됩니다. Unreal Engine, 빌드 도구, 생성된 바이너리와 맵은 포함하지 않습니다.

## 빠른 실행

1. Epic Games Launcher에 로그인하고 최신 정식 UE 5를 설치합니다. 엔진 설치본이 아직 없어 프로젝트의 `EngineAssociation`은 비워 두었습니다. 코드의 API 기준은 UE 5.5 이상이며 설치된 버전에서 UHT/UBT 검증이 필요합니다.
2. 프로젝트 루트에서 `powershell -ExecutionPolicy Bypass -File .\Scripts\Build.ps1 -EngineRoot 'C:\Program Files\Epic Games\UE_5.x'`를 실행합니다. `UE_5.x`를 실제 경로로 바꿉니다.
3. `.uproject` 우클릭 → Switch Unreal Engine version에서 설치 버전을 지정하고 프로젝트를 엽니다. 첫 실행에는 맵 미생성 경고가 있을 수 있습니다.
4. 에디터 Tools → Execute Python Script에서 `Content/Python/build_duskline.py`를 실행합니다.
5. [에디터 설정](Docs/EditorSetup.md)에 따라 NavMeshBoundsVolume을 배치하고 경로를 빌드합니다. `/Game/Maps/Duskline`을 저장하고 Play를 누릅니다.

## 조작법

| 입력 | 기능 |
|---|---|
| WASD / 마우스 | 이동 / 시점 |
| Shift 유지 | 달리기. 고갈 후 다시 누르면 재시도 |
| Ctrl | 웅크리기 전환 |
| Space | 점프 |
| 좌클릭 | 카메라 방향으로 근접 공격 |
| E | 조준한 컨테이너 열기 → 다시 E로 모두 획득 / 비콘 탈출 요청 |
| Q | 의약품 1개 사용, 체력 최대 30 회복 |
| R / 결과 화면 버튼 | 결과 후 전체 맵 재시작 |

중앙 조준점으로 근거리 물체를 바라보세요. 컨테이너는 먼저 내용물을 보여 주고 다음 입력에서 획득합니다. 무게나 슬롯이 부족하면 가능한 수량만 가져오고 나머지는 컨테이너에 남습니다. 북동쪽 `(4200, 3200)` 원통 비콘 주변에서 E를 누르면 탈출을 요청합니다.

## 작성된 기능

- Health, Stamina, Inventory, Combat, NoiseEmitter 독립 컴포넌트
- C++ Enhanced Input 매핑, 3인칭 카메라, 걷기·달리기·웅크리기·점프
- 인터페이스 기반 근거리 상호작용, Data Asset 아이템 4종, 시드 기반 루팅
- 중첩·무게·슬롯 제한 및 부분 처리 결과, 의약품 소비
- 한 공격 내 중복 피해 방지, 장애물 차폐, 쿨다운과 스태미나 비용
- AI Perception 시각·청각과 6개 상태의 타이머 기반 AI
- 결과 상태 단일 전이, 구역 이탈 즉시 취소, 시간 초과 우선 판정
- 이벤트 구독 UMG HUD, 피격·공격 텍스트 연출, 결과와 재시작
- 100m × 80m 그레이박스 생성 스크립트: 7개 컨테이너, 3마리 AI, 실내와 우회로

## 검증 및 제한

MSVC 독립 규칙 테스트 20개 통과. Unreal 컴포넌트 자동 테스트 4개 그룹을 작성했으나 엔진 미설치로 미실행입니다. [테스트 계획](Docs/TestPlan.md)과 [구현 보고서](Docs/ImplementationReport.md)를 확인하세요.

실제 `.umap`과 `.uasset`은 에디터 스크립트 실행 시 생성됩니다. NavMesh는 수동 단계입니다. 애니메이션·사운드·저장·멀티플레이·게임패드·최종 아트는 포함하지 않습니다. 10분은 제한 시간이며 실제 플레이 길이와 난이도는 플레이 테스트 후 조정해야 합니다.
