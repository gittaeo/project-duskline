#include "Components/CombatComponent.h"
#include "GameFramework/Actor.h"
#include "Components/HealthComponent.h"
#include "Components/StaminaComponent.h"
#include "Components/NoiseEmitterComponent.h"
#include "Game/DusklineGameState.h"
#include "Engine/World.h"
#include "DrawDebugHelpers.h"
#include "HAL/IConsoleManager.h"
static TAutoConsoleVariable<int32> CVarDusklineCombat(TEXT("duskline.DebugCombat"),0,TEXT("Show melee sweeps"),ECVF_Cheat);
UCombatComponent::UCombatComponent(){PrimaryComponentTick.bCanEverTick=false;}
bool UCombatComponent::TryAttack(){
    AActor* Owner=GetOwner(); if(!IsValid(Owner)||!Owner->HasAuthority()||!GetWorld()) return false;
    auto* State=GetWorld()->GetGameState<ADusklineGameState>(); if(State&&!State->IsRunning()) return false;
    auto* Health=Owner->FindComponentByClass<UHealthComponent>(); if(!Health||!Health->IsAlive()||GetWorld()->GetTimeSeconds()<NextAttack) return false;
    if(auto* Stamina=Owner->FindComponentByClass<UStaminaComponent>()) if(!Stamina->Spend(StaminaCost)) return false;
    NextAttack=GetWorld()->GetTimeSeconds()+FMath::Max(0.1f,Cooldown);
    if(auto* Noise=Owner->FindComponentByClass<UNoiseEmitterComponent>()) Noise->EmitProfile(Noise->Melee);
    OnSwing.Broadcast();
    const FVector Start=Owner->GetActorLocation()+Owner->GetActorForwardVector()*35;
    const FVector End=Start+Owner->GetActorForwardVector()*FMath::Max(0.f,Reach);
    FCollisionQueryParams Params(SCENE_QUERY_STAT(DuskMelee),false,Owner);
    FCollisionObjectQueryParams Objects; Objects.AddObjectTypesToQuery(ECC_Pawn);
    TArray<FHitResult> Hits; GetWorld()->SweepMultiByObjectType(Hits,Start,End,FQuat::Identity,Objects,FCollisionShape::MakeSphere(FMath::Max(1.f,Radius)),Params);
    TSet<TWeakObjectPtr<AActor>> Damaged;
    for(const auto& Hit:Hits){
        AActor* Target=Hit.GetActor(); if(!IsValid(Target)||Target==Owner||Damaged.Contains(Target)||Target->ActorHasTag(Owner->ActorHasTag(TEXT("Player"))?TEXT("Player"):TEXT("Infected"))) continue;
        FHitResult Wall; if(GetWorld()->LineTraceSingleByChannel(Wall,Start,Target->GetActorLocation(),ECC_Visibility,Params)&&Wall.GetActor()!=Target) continue;
        Damaged.Add(Target); if(auto* Victim=Target->FindComponentByClass<UHealthComponent>()) if(Victim->IsAlive()) Victim->Damage(Damage);
    }
#if !UE_BUILD_SHIPPING
    if(CVarDusklineCombat.GetValueOnGameThread()) {DrawDebugSphere(GetWorld(),End,Radius,16,FColor::Red,false,0.4f); DrawDebugLine(GetWorld(),Start,End,FColor::Red,false,0.4f);}
#endif
    return true;
}
