#include "Components/NoiseEmitterComponent.h"
#include "GameFramework/Actor.h"
#include "Perception/AISense_Hearing.h"
#include "DrawDebugHelpers.h"
#include "HAL/IConsoleManager.h"
static TAutoConsoleVariable<int32> CVarDusklineNoise(TEXT("duskline.DebugNoise"),0,TEXT("Show noise spheres"),ECVF_Cheat);
UNoiseEmitterComponent::UNoiseEmitterComponent(){
    PrimaryComponentTick.bCanEverTick=false;
    Crouch.Radius=100; Crouch.Duration=1; Walk.Radius=450; Walk.Duration=2;
    Sprint.Radius=1500; Sprint.Duration=5; Melee.Radius=1800; Melee.Duration=5; ForcedOpen.Radius=2600; ForcedOpen.Duration=8;
}
void UNoiseEmitterComponent::EmitProfile(const FNoiseProfile& P){
    if(!IsValid(GetOwner())||!GetOwner()->HasAuthority()||!GetWorld()||!FMath::IsFinite(P.Radius)||!FMath::IsFinite(P.Loudness)||!FMath::IsFinite(P.Duration)||P.Radius<=0||P.Loudness<=0) return;
    // Perception carries a compact lifetime tag; listeners own their investigation deadline.
    const FName Tag(*FString::Printf(TEXT("DuskNoise_%d"),FMath::RoundToInt(FMath::Clamp(P.Duration,0.1f,30.f)*1000)));
    UAISense_Hearing::ReportNoiseEvent(this,GetOwner()->GetActorLocation(),P.Loudness,GetOwner(),P.Radius,Tag);
#if !UE_BUILD_SHIPPING
    if(CVarDusklineNoise.GetValueOnGameThread()) DrawDebugSphere(GetWorld(),GetOwner()->GetActorLocation(),P.Radius,24,FColor::Yellow,false,FMath::Clamp(P.Duration,0.1f,30.f));
#endif
}
