# 구조와 설계

## 책임

| 클래스 | 책임 |
|---|---|
| ADusklineGameMode | 기본 캐릭터·컨트롤러·GameState 지정 |
| ADusklineGameState | 제한 시각, 요구 가치, 탈출 상태, 최종 결과의 유일한 소유자 |
| ADusklinePlayerController | 로컬 HUD 생성, 소유 Pawn 연결, 재시작 명령 |
| ADusklineCharacter | 입력을 컴포넌트로 전달, 이동 상태, 조준 상호작용 |
| UHealthComponent | 피해·회복 범위 제한, 사망 1회 이벤트 |
| UStaminaComponent | 소비 승인, 지연 회복, 상한 |
| UInventoryComponent | 슬롯·중첩·무게·가치, 부분 추가/제거, 의약품 |
| UCombatComponent | 쿨다운, 자원 지불, Pawn sweep, 장애물 검사, 대상 중복 제거 |
| UNoiseEmitterComponent | 행동별 프로필 → AI Hearing 이벤트 |
| IInteractableInterface | 표시 문구·실행 가능 여부·상호작용 |
| UItemDataAsset | 불변 아이템 정의, 데이터 유효성 검증 |
| ALootContainer | 인스턴스별 한 번 생성한 내용물, 개방·획득 |
| AExtractionZone | 공간 포함 판정, 구역 이탈 이벤트, 탈출 명령 전달 |
| ADusklineAICharacter | 감염체 상태 컴포넌트, 피격·사망 도형 연출 |
| ADusklineAIController | Perception 이벤트와 Idle/Patrol/Investigate/Chase/Attack/Dead 상태 |
| UDusklineHUD | 이벤트를 구독해 읽기 전용 표시, 재시작 명령만 Controller로 전달 |
| DusklineRules | 엔진과 독립적인 실제 무게·탈출·종료 정책 함수 |

## 데이터 흐름

Enhanced Input → Character → Combat/Inventory/상호작용 인터페이스 → Health/GameState → multicast delegate → HUD.

행동 → NoiseEmitter의 위치·크기·지속 시간 → `UAISense_Hearing::ReportNoiseEvent` → AI Perception → 마지막 소음 위치와 조사 종료 시각 → AIController의 이동 요청.

획득은 컨테이너 내용물에서 인벤토리로 실제 처리 수량만 이전합니다. `FItemTransfer.bSuccess`는 요청 전량 처리 여부이며 `Processed > 0`은 부분 성공도 포함합니다. 데이터 ID가 같으나 다른 정의 객체를 전달하면 오류로 거절합니다. 정의 에셋은 실행 중 변경하지 않는 전제입니다.

## 시간과 상태

캐릭터의 이동·조준 갱신과 스태미나, Run은 0.1초 타이머, AI 판단은 0.25초 타이머입니다. 스태미나 갱신은 실제 경과 시간을 계산합니다. CharacterMovement, 카메라, Perception 등 엔진 내부 Tick은 그대로 사용합니다. 게임 코드 자체 Actor Tick과 전체 AI 매 프레임 검색은 없습니다.

AI는 Behavior Tree 에셋에 의존하지 않는 명시적 상태 머신입니다. 6개 상태의 작은 슬라이스에서 코드만으로 조정·검토가 가능하고, 엔진 설치 전에도 상태 흐름을 검토할 수 있다는 이유로 선택했습니다. 순찰 지점은 레벨 인스턴스의 월드 좌표입니다. 감지 대상과 상호작용 대상은 weak pointer로 보관합니다.

탈출은 절대 종료 시각을 사용합니다. 체력 0, 구역 이탈, 요구 가치 미달, 대상 파괴를 재검사합니다. Overlap 종료 이벤트로 구역 이탈을 즉시 취소하고 타이머가 파괴·조건 변경을 보완합니다. 모든 종료 요청은 `Finish`를 거칩니다. 이미 종료된 상태는 바뀌지 않으며 동일 시각에는 **시간 초과 > 사망 > 탈출** 순서입니다. UI는 생성 전 이벤트를 받을 필요 없이 연결 직후 현재 스냅샷을 읽습니다.

## 확장 지점

현재 네트워크 기능은 없습니다. `HasAuthority()` 검사는 변경 함수 경계에 존재하지만 RPC나 Replication을 대신하지 않습니다.

협동 확장 시 Character의 입력 명령을 검증 가능한 Server RPC로 변경하고, 서버에서 거리·시야·생존·인벤토리 조건을 다시 검사해야 합니다. Health/Stamina/Inventory/Run을 복제하고 OnRep에서 기존 표시 이벤트를 발생시킵니다. 참가자별 결과·인벤토리는 PlayerState로 옮기고 탈출 요청은 플레이어별 구조로 확장합니다. 지금의 로컬 플레이어 1명 조회와 단일 탈출 슬롯은 싱글플레이 전용입니다. UI·노이즈 프로필·인터페이스는 재사용할 수 있습니다.

콘솔 `duskline.DebugNoise 1`, `duskline.DebugCombat 1`로 개발 빌드의 범위를 표시합니다. Shipping에서는 시각화 호출이 제외됩니다.

## 참고한 공식 API

- [Enhanced Input](https://dev.epicgames.com/documentation/en-us/unreal-engine/enhanced-input-in-unreal-engine)
- [AI Hearing](https://dev.epicgames.com/documentation/en-us/unreal-engine/API/Runtime/AIModule/Perception/UAISense_Hearing)
- [LevelEditorSubsystem](https://dev.epicgames.com/documentation/en-us/unreal-engine/python-api/class/LevelEditorSubsystem)
- [DataAssetFactory](https://dev.epicgames.com/documentation/en-us/unreal-engine/python-api/class/DataAssetFactory)

문서 확인은 로컬 엔진 헤더 검증이나 컴파일을 대체하지 않습니다.
