#pragma once
#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "HealthComponent.generated.h"
DECLARE_DYNAMIC_MULTICAST_DELEGATE(FHealthChanged);
DECLARE_DYNAMIC_MULTICAST_DELEGATE(FDied);
UCLASS(ClassGroup=(Duskline), meta=(BlueprintSpawnableComponent))
class PROJECTDUSKLINE_API UHealthComponent : public UActorComponent {
    GENERATED_BODY()
public:
    UHealthComponent();
    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="Health", meta=(ClampMin="1")) float MaxHealth=100;
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category="Health") float Health=100;
    UPROPERTY(BlueprintAssignable) FHealthChanged OnChanged;
    UPROPERTY(BlueprintAssignable) FDied OnDied;
    UFUNCTION(BlueprintCallable) float Damage(float Amount);
    UFUNCTION(BlueprintCallable) float Heal(float Amount);
    UFUNCTION(BlueprintPure) bool IsAlive() const { return Health>0; }
protected:
    virtual void BeginPlay() override;
};
