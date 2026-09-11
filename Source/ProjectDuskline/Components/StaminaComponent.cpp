#include "Components/StaminaComponent.h"
#include "GameFramework/Actor.h"
#include "Engine/World.h"
#include "TimerManager.h"
UStaminaComponent::UStaminaComponent(){ PrimaryComponentTick.bCanEverTick=false; }
void UStaminaComponent::BeginPlay(){ Super::BeginPlay(); Maximum=FMath::Max(1.f,Maximum); Current=Maximum; LastUpdate=GetWorld()->GetTimeSeconds(); GetWorld()->GetTimerManager().SetTimer(RecoveryTimer,this,&UStaminaComponent::Recover,0.1f,true); }
void UStaminaComponent::EndPlay(const EEndPlayReason::Type Reason){ GetWorld()->GetTimerManager().ClearTimer(RecoveryTimer); Super::EndPlay(Reason); }
bool UStaminaComponent::Spend(float Amount){
    if(!FMath::IsFinite(Amount)||Amount<0||Current<Amount||(GetOwner()&&!GetOwner()->HasAuthority())) return false;
    Current-=Amount; DelayRemaining=FMath::Max(0.f,RecoveryDelay); OnChanged.Broadcast(); return true;
}
void UStaminaComponent::Advance(float DeltaSeconds){
    if(!FMath::IsFinite(DeltaSeconds)||DeltaSeconds<=0) return;
    const float RecoverTime=FMath::Max(0.f,DeltaSeconds-DelayRemaining);
    DelayRemaining=FMath::Max(0.f,DelayRemaining-DeltaSeconds);
    float Old=Current; Current=FMath::Clamp(Current+FMath::Max(0.f,RecoveryRate)*RecoverTime,0.f,Maximum);
    if(Old!=Current) OnChanged.Broadcast();
}
void UStaminaComponent::Recover(){ double Now=GetWorld()->GetTimeSeconds(); Advance(float(Now-LastUpdate)); LastUpdate=Now; }
