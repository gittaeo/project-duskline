# 황혼선 — Unity Web Edition

기존 Unreal 프로젝트와 같은 세계관·탐색/루팅/생존/탈출 루프를 C#으로 구현한 별도 Unity 프로젝트입니다. **Unity 6.2, `6000.2.0f1`**과 WebGL Build Support를 사용합니다. 상위 폴더는 Unreal 프로젝트이며, Unity Hub에서는 반드시 이 `UnityWeb` 폴더를 추가하세요.

## Unity 버전·도구·언어·스킬

| 구분 | 실제 사용 및 확인 상태 |
|---|---|
| 엔진 | **Unity 6.2 / 6000.2.0f1**, revision `eed1c594c913`. `ProjectSettings/ProjectVersion.txt` 기준 |
| 빌드 모듈 | 해당 에디터의 WebGL Build Support, IL2CPP → WebAssembly. 실제 Web 빌드 성공 |
| 렌더링 | Built-in Render Pipeline / Standard 셰이더, PBR Albedo·Normal·Metallic/Smoothness, 실시간 방향광·그림자·안개 |
| 언어 | C# 게임플레이·에디터 빌더·검사, HTML/CSS/JavaScript Web 템플릿, JavaScript `.jslib` 테스트 보고, Python 로컬 서버, PowerShell 빌드 실행 |
| 엔진 기능 | CharacterController, 내장 Input API, NavMesh/NavMeshAgent, ScriptableObject, IMGUI 한국어 HUD. URP/HDRP·외부 입력 패키지는 사용하지 않음 |
| 개발·검증 도구 | Unity Hub/Editor, Git, Python HTTP 서버, Codex 브라우저 도구로 Web 실행 화면·콘솔·통합 검사 확인 |
| Codex 스킬 | 전용 Unity 스킬은 적용하지 않음. 범용 computer-use 문서를 참고했으며 실제 Web 검증은 브라우저 도구 사용. 프로젝트 실행에 Codex 스킬 설치 불필요 |
| 외부 아트 | Poly Haven `asphalt_02`, `concrete_wall_003` (CC0), Noto Sans KR 기반 정적 폰트 (OFL). 유료 에셋 없음. 이미지 생성 AI는 사용하지 않음 |

에디터 Play Mode 검사는 이 환경의 라이선스 오류로 완료하지 못했습니다. C# 컴파일·Web 빌드·브라우저 통합 검사와 구분하며 자세한 결과는 [검증 보고서](Docs/Validation.md)에 기록합니다.

## 실행

1. Unity Hub → Add project from disk → 이 폴더 선택.
2. Unity `6000.2.0f1`로 열고 필요하면 해당 버전의 WebGL Build Support 설치.
3. 메뉴 `Duskline → Prepare Project` 실행. 기존 데이터 에셋과 씬은 덮어쓰지 않습니다.
4. `Assets/Duskline/Scenes/Duskline.unity` 열기 → Play → **수색 시작하기**.

씬에는 Bootstrap 하나만 저장되어 있습니다. Play에서 건물·바닥·컨테이너 7개·감염체 3마리·NavMesh·HUD가 자동 생성됩니다. 별도 Inspector 연결이나 NavMesh 수동 배치가 필요 없습니다.

## 브라우저 빌드

프로젝트를 에디터에서 닫은 뒤 PowerShell에서 실행합니다.

```powershell
powershell -ExecutionPolicy Bypass -File .\Scripts\BuildWeb.ps1
python .\Scripts\ServeWeb.py
```

`http://127.0.0.1:8765`를 브라우저에서 엽니다. 다른 설치 경로는 `BuildWeb.ps1 -UnityEditor '실제 Unity.exe 경로'`로 지정합니다. 에디터에서는 `Duskline → Build Web` 메뉴도 사용할 수 있습니다.

산출물은 `Builds/Web`입니다. `index.html`을 파일로 더블클릭하지 말고 HTTP 서버로 열어야 합니다. 압축은 비활성화되어 일반 정적 서버에서 실행할 수 있으며, 공개 호스팅은 설정하지 않았습니다. WebGL 2와 WebAssembly 지원 데스크톱 브라우저가 대상입니다.

## 조작

| 입력 | 동작 |
|---|---|
| WASD / 마우스 | 이동 / 시점 |
| Shift 유지 | 달리기. 고갈 시 키를 놓았다 다시 눌러 재시도 |
| Ctrl 또는 C | 웅크리기 전환 |
| Space | 점프 |
| 좌클릭 | 근접 공격 |
| E | 조준한 상자 열기 → 다시 눌러 획득 / 비콘 구조 요청 |
| Q | 의약품 사용 |
| Tab | 인벤토리 표시 전환 |
| Esc | 일시정지·마우스 해제 |
| R / 버튼 | 결과 후 재시작 |

가치 180 이상을 확보하고 지도 북동쪽 비콘으로 이동해 E를 누릅니다. 반경 3.5m 구역 안에서 8초를 유지하면 성공합니다. 제한 시간은 10분입니다. 노란 지도 표시는 물자, 청록색 표시는 탈출 지점, 흰색은 플레이어입니다.

## 구조와 차이

### 그래픽 개선과 수정 위치

기존 맵 동선과 UI 구성을 유지하면서 아스팔트 바닥·풍화 콘크리트 벽에 PBR 텍스처를 적용하고 창틀·건물 하단부·옥상 설비와 조명을 보완했습니다. 캐릭터와 물자 모델은 기본 도형인 프로토타입 상태입니다.

- `Assets/Duskline/Resources/Art`: 실제 런타임 텍스처·머티리얼.
- `Editor/ArtMaterialBuilder.cs`: 2K 컬러, 1K 노멀·거칠기 임포트. 거칠기를 `1 - roughness`로 변환해 Standard 셰이더의 Smoothness 알파에 저장.
- `Scripts/SurfaceArt.cs`: 재질 배정과 미터 기준 UV. 바닥 4m, 콘크리트 3m 단위로 반복해 큰 면의 텍스처 늘어짐을 줄임.
- `Scripts/WorldBootstrap.cs`: 창틀·외벽 장식과 조명·안개. 장식에는 충돌이나 NavMesh 장애물을 추가하지 않음.

`Prepare Project`와 Web 빌드는 ArtMaterialBuilder가 관리하는 두 머티리얼과 파생 PNG를 다시 생성합니다. 이를 직접 편집하면 다음 Prepare 때 덮어써지므로, 변경은 빌더 코드에 반영하거나 별도 머티리얼 이름으로 분리하세요. [공유 원본·출처·라이선스와 Unreal 적용 안내](../Art/README.md)를 함께 확인하세요.

- `Assets/Duskline/Scripts`: 체력, 스태미나, 인벤토리, 근접 공격, 이동·카메라, 상호작용, 루팅, AI, 결과 상태, HUD를 분리.
- `Resources/Items`: ScriptableObject 아이템 4종. 중첩·가치·무게·치유량 편집 가능.
- `WorldBootstrap`: 그레이박스와 런타임 NavMesh 생성. 좌표·색·구성은 이 파일에서 변경.
- `EnemyBrain`: 0.2초 주기의 시야/경로 판단과 이벤트 기반 소음 조사. Unity에는 Unreal AI Perception 대신 해당 구현을 사용.
- `GameHUD`: 한국어 IMGUI, 이벤트를 통한 상태 문자열 갱신, 지도와 결과 화면.
- `Editor/BuildTools`: 기존 에셋 보존 초기화, 실제 컴포넌트 규칙 검사, Web 빌드.
- `Editor/PlaySmoke`: 에디터 Play Mode에서 맵·루팅·NavMesh·탈출·재시작·사망·시간 초과를 검증.

Unreal 소스를 변환하거나 공유 바이너리를 사용하지 않습니다. Unity 버전은 유지 보수하기 쉬운 별도 구현이며 원본 요구사항의 장르·게임 루프를 공유합니다. 입력은 Unity 내장 Input API입니다. 멀티플레이, 저장, 게임패드·터치 입력, 정식 애니메이션·사운드는 범위 밖입니다. 10분은 제한 시간이며 난이도와 실제 플레이 길이는 추가 튜닝이 필요합니다.

## 검증

에디터 메뉴 `Duskline → Run Core Checks`로 컴포넌트 규칙 검사를 실행합니다. Play Mode 통합 검사는 에디터를 닫고 아래처럼 실행합니다. 통합 검사 명령에는 `-quit`를 넣지 않습니다. 검사가 완료되면 종료 코드 0 또는 1로 종료됩니다.

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.2.0f1\Editor\Unity.exe' -batchmode -nographics -projectPath "$PWD" -executeMethod Duskline.Editor.PlaySmoke.Run -logFile "$PWD\Logs\PlaySmoke.log"
```

`Validation/CoreChecks.json`, `PlaySmoke.json`, `WebBuild.txt` 및 [검증 보고서](Docs/Validation.md)를 확인하세요. 브라우저 간 호환성과 장시간 수동 플레이는 자동 검사와 구분합니다.

## 재배포와 Git

`Assets`, `.meta`, `Packages`, `ProjectSettings`, 스크립트와 문서를 커밋합니다. Library/Logs/UserSettings/Builds는 제외합니다. Noto Sans KR은 SIL Open Font License 1.1에 따라 포함했으며 `Assets/Duskline/Fonts/OFL.txt`에 라이선스를 보존했습니다. 출처는 [Google Fonts의 Noto Sans KR](https://github.com/google/fonts/tree/main/ofl/notosanskr)입니다. 외부 유료 에셋·패키지는 사용하지 않았습니다.
