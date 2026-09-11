#pragma once
#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "CombatComponent.generated.h"
DECLARE_DYNAMIC_MULTICAST_DELEGATE(FCombatSwing);
UCLASS(ClassGroup=(Duskline), meta=(BlueprintSpawnableComponent))
class PROJECTDUSKLINE_API UCombatComponent : public UActorComponent {
    GENERATED_BODY()
public:
    UCombatComponent();
    UPROPERTY(EditAnywhere, BlueprintReadWrite) float Damage=34;
    UPROPERTY(EditAnywhere, BlueprintReadWrite) float StaminaCost=20;
    UPROPERTY(EditAnywhere, BlueprintReadWrite) float Cooldown=0.65f;
    UPROPERTY(EditAnywhere, BlueprintReadWrite) float Reach=150;
    UPROPERTY(EditAnywhere, BlueprintReadWrite) float Radius=45;
    UPROPERTY(BlueprintAssignable) FCombatSwing OnSwing;
    UFUNCTION(BlueprintCallable) bool TryAttack();
private:
    double NextAttack=0;
};
