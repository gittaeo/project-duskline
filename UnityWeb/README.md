# 황혼선 — Unity Web Edition

기존 Unreal 프로젝트와 같은 세계관·탐색/루팅/생존/탈출 루프를 C#으로 구현한 별도 Unity 프로젝트입니다. **Unity 6.2, `6000.2.0f1`**과 WebGL Build Support를 사용합니다. 상위 폴더는 Unreal 프로젝트이며, Unity Hub에서는 반드시 이 `UnityWeb` 폴더를 추가하세요.

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
