#include "AI/DusklineAIController.h"
#include "AI/DusklineAICharacter.h"
#include "Characters/DusklineCharacter.h"
#include "Components/HealthComponent.h"
#include "Components/CombatComponent.h"
#include "Game/DusklineGameState.h"
#include "Perception/AIPerceptionComponent.h"
#include "Perception/AISenseConfig_Sight.h"
#include "Perception/AISenseConfig_Hearing.h"
#include "Perception/AISense_Sight.h"
#include "Perception/AISense_Hearing.h"
#include "TimerManager.h"
#include "Engine/World.h"
ADusklineAIController::ADusklineAIController(){
    PrimaryActorTick.bCanEverTick=false;
    Senses=CreateDefaultSubobject<UAIPerceptionComponent>(TEXT("Senses")); SetPerceptionComponent(*Senses);
    Sight=CreateDefaultSubobject<UAISenseConfig_Sight>(TEXT("Sight")); Sight->SightRadius=850; Sight->LoseSightRadius=1100; Sight->PeripheralVisionAngleDegrees=55; Sight->SetMaxAge(2);
    Sight->DetectionByAffiliation.bDetectEnemies=true; Sight->DetectionByAffiliation.bDetectNeutrals=true; Sight->DetectionByAffiliation.bDetectFriendlies=true;
    Hearing=CreateDefaultSubobject<UAISenseConfig_Hearing>(TEXT("Hearing")); Hearing->HearingRange=3000; Hearing->SetMaxAge(5);
    Hearing->DetectionByAffiliation.bDetectEnemies=true; Hearing->DetectionByAffiliation.bDetectNeutrals=true; Hearing->DetectionByAffiliation.bDetectFriendlies=true;
    Senses->ConfigureSense(*Sight); Senses->ConfigureSense(*Hearing); Senses->SetDominantSense(UAISense_Sight::StaticClass());
}
void ADusklineAIController::BeginPlay(){Super::BeginPlay(); Senses->OnTargetPerceptionUpdated.AddDynamic(this,&ADusklineAIController::Perceived); GetWorld()->GetTimerManager().SetTimer(ThinkTimer,this,&ADusklineAIController::Think,0.25f,true);}
void ADusklineAIController::EndPlay(const EEndPlayReason::Type Reason){GetWorld()->GetTimerManager().ClearTimer(ThinkTimer); Super::EndPlay(Reason);}
void ADusklineAIController::StopAI(){State=EInfectedState::Dead; Target.Reset(); StopMovement(); ClearFocus(EAIFocusPriority::Gameplay); GetWorld()->GetTimerManager().ClearTimer(ThinkTimer); Senses->Deactivate();}
void ADusklineAIController::Perceived(AActor* Actor,FAIStimulus Stimulus){
    if(State==EInfectedState::Dead) return; auto* P=Cast<ADusklineCharacter>(Actor); if(!P||!P->CanAct()) return;
    const double Now=GetWorld()->GetTimeSeconds();
    if(Stimulus.Type==UAISense::GetSenseID<UAISense_Sight>()){
        bVisible=Stimulus.WasSuccessfullySensed(); Target=P;
        if(bVisible){ LastKnown=P->GetActorLocation(); State=EInfectedState::Chase; }
        else {State=EInfectedState::Investigate; SearchUntil=Now+FMath::Max(1.f,SearchSeconds); ClearFocus(EAIFocusPriority::Gameplay);}
    } else if(Stimulus.Type==UAISense::GetSenseID<UAISense_Hearing>()&&Stimulus.WasSuccessfullySensed()&&!bVisible){
        LastKnown=Stimulus.StimulusLocation; State=EInfectedState::Investigate;
        FString Tag=Stimulus.Tag.ToString(); float Duration=SearchSeconds;
        if(Tag.RemoveFromStart(TEXT("DuskNoise_"))) Duration=FCString::Atof(*Tag)/1000.f;
        SearchUntil=Now+FMath::Clamp(Duration,1.f,30.f);
    }
}
void ADusklineAIController::Think(){
    auto* Self=Cast<ADusklineAICharacter>(GetPawn()); if(!Self||!Self->Health->IsAlive()){StopAI(); return;}
    auto* Run=GetWorld()->GetGameState<ADusklineGameState>(); if(Run&&!Run->IsRunning()){StopMovement(); ClearFocus(EAIFocusPriority::Gameplay); return;}
    if(bVisible&&(!Target.IsValid()||!Target->CanAct())){bVisible=false; Target.Reset(); State=EInfectedState::Investigate; SearchUntil=GetWorld()->GetTimeSeconds()+SearchSeconds;}
    if(bVisible&&Target.IsValid()){
        LastKnown=Target->GetActorLocation(); SetFocus(Target.Get());
        if(FVector::DistSquared(Self->GetActorLocation(),LastKnown)<=FMath::Square(AttackDistance)){
            StopMovement(); State=EInfectedState::Attack;
            Self->SetActorRotation(FRotator(0,(LastKnown-Self->GetActorLocation()).Rotation().Yaw,0)); Self->Combat->TryAttack();
        } else {State=EInfectedState::Chase; MoveToActor(Target.Get(),90);}
        return;
    }
    ClearFocus(EAIFocusPriority::Gameplay);
    if(State==EInfectedState::Investigate){
        if(GetWorld()->GetTimeSeconds()<SearchUntil){MoveToLocation(LastKnown,70); return;}
        Target.Reset(); State=EInfectedState::Patrol;
    }
    if(Self->PatrolPoints.IsEmpty()){State=EInfectedState::Idle; StopMovement(); return;}
    State=EInfectedState::Patrol; PatrolIndex%=Self->PatrolPoints.Num();
    if(FVector::DistSquared(Self->GetActorLocation(),Self->PatrolPoints[PatrolIndex])<FMath::Square(130.f)) PatrolIndex=(PatrolIndex+1)%Self->PatrolPoints.Num();
    MoveToLocation(Self->PatrolPoints[PatrolIndex],70);
}
