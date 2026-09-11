#include "Game/DusklineGameState.h"
#include "Game/RunRules.h"
#include "GameFramework/PlayerController.h"
#include "Characters/DusklineCharacter.h"
#include "Components/HealthComponent.h"
#include "Components/InventoryComponent.h"
#include "World/ExtractionZone.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "Engine/World.h"
#include "TimerManager.h"
ADusklineGameState::ADusklineGameState(){PrimaryActorTick.bCanEverTick=false;}
void ADusklineGameState::BeginPlay(){Super::BeginPlay(); Deadline=GetWorld()->GetTimeSeconds()+FMath::Max(1.f,TimeLimit); Remaining=TimeLimit; GetWorld()->GetTimerManager().SetTimer(RunTimer,this,&ADusklineGameState::Advance,0.1f,true); OnChanged.Broadcast();}
void ADusklineGameState::EndPlay(const EEndPlayReason::Type Reason){GetWorld()->GetTimerManager().ClearTimer(RunTimer); Super::EndPlay(Reason);}
bool ADusklineGameState::BeginExtraction(ADusklineCharacter* P,AExtractionZone* Zone,float Duration){
    if(!HasAuthority()||!IsValid(P)||!IsValid(Zone)||!DusklineRules::CanExtract(IsRunning(),P->Health->IsAlive(),Zone->Contains(P),P->Inventory->GetValue(),RequiredValue,GetWorld()->GetTimeSeconds(),Deadline)||ExtractingPlayer.IsValid()) return false;
    ExtractingPlayer=P; ExtractionZone=Zone; ExtractionDeadline=GetWorld()->GetTimeSeconds()+FMath::Max(0.1f,Duration); ExtractionRemaining=FMath::Max(0.1f,Duration); OnChanged.Broadcast(); return true;
}
void ADusklineGameState::CancelExtraction(AActor* Player){
    if(!HasAuthority()||!IsRunning()||ExtractingPlayer.Get()!=Player) return;
    ExtractionDeadline=0; ExtractionRemaining=0; ExtractingPlayer.Reset(); ExtractionZone.Reset();
    if(auto* P=Cast<ADusklineCharacter>(Player)) P->Notify(FText::FromString(TEXT("탈출 취소 — 구역을 벗어났습니다")));
    OnChanged.Broadcast();
}
void ADusklineGameState::Advance(){
    if(!HasAuthority()||!IsRunning()) return;
    const double Now=GetWorld()->GetTimeSeconds(); Remaining=FMath::Max(0.f,float(Deadline-Now));
    if(Now>=Deadline){Finish(ERunResult::TimedOut,ExtractingPlayer.Get()); return;}
    if(ExtractionDeadline>0){
        auto* P=ExtractingPlayer.Get(); auto* Zone=ExtractionZone.Get();
        if(P&&!P->Health->IsAlive()){Finish(ERunResult::Dead,P); return;}
        if(!P||!Zone||!Zone->Contains(P)||P->Inventory->GetValue()<RequiredValue){ExtractionDeadline=0; ExtractionRemaining=0; ExtractingPlayer.Reset(); ExtractionZone.Reset(); if(P) P->Notify(FText::FromString(TEXT("탈출 취소 — 구역 또는 요구 가치 확인")));}
        else {ExtractionRemaining=FMath::Max(0.f,float(ExtractionDeadline-Now)); if(Now>=ExtractionDeadline){Finish(ERunResult::Escaped,P); return;}}
    }
    OnChanged.Broadcast();
}
void ADusklineGameState::Finish(ERunResult Requested,ADusklineCharacter* P){
    if(!HasAuthority()||!IsRunning()||Requested==ERunResult::Running) return;
    // Deadline wins ties. All terminal transitions go through this idempotent authority boundary.
    if(!P) if(auto* PC=GetWorld()->GetFirstPlayerController()) P=Cast<ADusklineCharacter>(PC->GetPawn());
    Requested=static_cast<ERunResult>(DusklineRules::Resolve(static_cast<DusklineRules::Result>(Result),static_cast<DusklineRules::Result>(Requested),!P||P->Health->IsAlive(),GetWorld()->GetTimeSeconds(),Deadline));
    if(Requested==ERunResult::Escaped&&(!P||!ExtractionZone.IsValid()||!ExtractionZone->Contains(P)||P->Inventory->GetValue()<RequiredValue)) return;
    Result=Requested; ExtractionRemaining=0; ExtractionDeadline=0;
    if(P){FinalCount=P->Inventory->GetCount(); FinalValue=P->Inventory->GetValue(); P->GetCharacterMovement()->StopMovementImmediately(); P->GetCharacterMovement()->DisableMovement();}
    GetWorld()->GetTimerManager().ClearTimer(RunTimer); OnChanged.Broadcast();
}
