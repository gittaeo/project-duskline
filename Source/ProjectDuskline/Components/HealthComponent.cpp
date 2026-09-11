#include "Components/HealthComponent.h"
#include "GameFramework/Actor.h"
UHealthComponent::UHealthComponent() { PrimaryComponentTick.bCanEverTick=false; }
void UHealthComponent::BeginPlay() { Super::BeginPlay(); MaxHealth=FMath::Max(1.f,MaxHealth); Health=MaxHealth; OnChanged.Broadcast(); }
float UHealthComponent::Damage(float Amount) {
    if (!FMath::IsFinite(Amount)||Amount<=0||!IsAlive()||(GetOwner()&&!GetOwner()->HasAuthority())) return 0;
    const float Taken=FMath::Min(Health,Amount); Health-=Taken; OnChanged.Broadcast();
    if (!IsAlive()) OnDied.Broadcast(); return Taken;
}
float UHealthComponent::Heal(float Amount) {
    if (!FMath::IsFinite(Amount)||Amount<=0||!IsAlive()||(GetOwner()&&!GetOwner()->HasAuthority())) return 0;
    const float Added=FMath::Clamp(Amount,0.f,MaxHealth-Health); Health+=Added;
    if(Added>0) OnChanged.Broadcast(); return Added;
}
