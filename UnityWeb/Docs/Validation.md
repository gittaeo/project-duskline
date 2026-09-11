# Unity Web 검증 보고서

## 확인된 결과

- Unity `6000.2.0f1` 프로젝트 전체 C# 컴파일 성공.
- `BuildTools.RunChecks`: 실제 컴포넌트와 게임 규칙 검사 **33개 통과**.
- IL2CPP/WebAssembly Web 빌드 성공. 최종 크기·경고 수는 `Validation/WebBuild.txt` 참조.
- 별도 `DUSKLINE_SMOKE_TEST` Web 빌드의 통합 검사 **23개 통과**, 브라우저 오류 로그 0개.
- 브라우저에서 한국어 시작 화면, 수색 시작 버튼과 HUD, 통합 검사 결과 화면을 확인.

## Web 통합 검사의 실제 범위

Ready 상태와 컨테이너 7개/AI 3마리 생성, Web 빌드의 바닥 BoxCollider 유지, 시작 지점 NavMesh, 탈출 지점까지의 완전한 경로, 수색 시작, 시간이 흘러도 바닥 아래로 떨어지지 않음, 루팅 이전, 빈 컨테이너 재생성 방지, 의약품 회복·소비, 소음 조사, 실제 NavMesh 이동, 사망한 AI의 공격 거절, 탈출 시작, 이탈 취소, 재요청, 성공 시 아이템 수·가치, 확정 결과 불변성, 재시작 초기화, 사망 결과, 시간 초과 결과를 검증했습니다.

자동 테스트는 게임 컴포넌트를 직접 호출하고 플레이어 위치를 옮깁니다. 실제 사용자의 모든 키 입력이나 전체 플레이 경로를 자동 재현한 것은 아닙니다. 일반 Web 빌드에서는 테스트 전용 심볼이 없으므로 이 동작이 포함되지 않습니다.

## 발견하고 수정한 문제

첫 Web 빌드는 컴파일에 성공했으나, 런타임 생성에만 쓰인 BoxCollider의 타입 참조가 없어 최적화 시 제거됐습니다. 브라우저 로그에서 `BoxCollider doesn't exist`를 확인했으며, WorldBootstrap에서 명시적인 제네릭 타입 참조와 생성 경로를 추가했습니다. 이후 Web 통합 검사에서 바닥 충돌과 지면 유지가 통과했습니다.

가변 Noto Sans KR의 기본 굵기가 얇게 표시되어 weight 500 정적 인스턴스로 변경하고, 수정 폰트 이름과 라이선스를 기록했습니다. 브라우저 화면에서 한글 표시를 다시 확인했습니다.

## 에디터 Play Mode 검사 제한

`Editor/PlaySmoke.cs`는 에디터 Play Mode 통합 검사도 제공합니다. 이 컴퓨터에서는 해당 실행이 `No valid Unity Editor license found`와 종료 코드 198로 실패했습니다. 프로젝트 생성·C# 컴파일·Web 빌드·에디터 기반 컴포넌트 검사는 정상 수행됐습니다. 라이선스 상태를 우회하거나 유료 라이선스를 구매하지 않았으며, 실행 검증을 Web 테스트 빌드로 옮겼습니다. `PlaySmoke.json`의 에디터 성공 결과는 제공하지 않습니다.

## 재현 명령

프로젝트 루트에서 에디터를 닫고 실행합니다. 설치 경로를 맞춰 주세요.

```powershell
$editor = 'C:\Program Files\Unity\Hub\Editor\6000.2.0f1\Editor\Unity.exe'
& $editor -batchmode -nographics -quit -projectPath "$PWD" -executeMethod Duskline.Editor.BuildTools.BuildWebSmoke -logFile "$PWD\Logs\WebSmokeBuild.log"
python .\Scripts\ServeWeb.py --smoke --port 8766
```

`http://127.0.0.1:8766`을 브라우저에서 열면 약 10초 내 테스트가 진행되고 화면 상단에 PASS 또는 FAIL이 표시됩니다. 진단 스크립트는 별도 `Builds/Smoke` 산출물에만 활성화됩니다. 일반 게임은 `Scripts/BuildWeb.ps1`과 `Scripts/ServeWeb.py`로 빌드·실행합니다.

## 남은 검증과 한계

- 데스크톱 키보드·마우스 기준이며 모바일/터치·게임패드 미구현.
- Chrome/Firefox/Safari 전체 브라우저 호환성, 저사양 장치 성능, 10분 전체 플레이 난이도는 별도 수동 검증 필요.
- AI의 모든 순찰·시야 상실 타이밍과 모든 전투 입력 조합을 자동 검증한 것은 아님.
- 로컬 미리보기와 재현 가능한 Web 산출물을 제공하며 공개 사이트로 배포하지 않음.
- 멀티플레이·저장·정식 애니메이션·사운드 없음. 행동 소음은 AI 이벤트이며 실제 오디오 재생은 없음.
- 기존 Unreal 버전의 미완료 빌드 상태는 그대로이며, Unity 성공 결과를 Unreal의 검증으로 간주하면 안 됨.
