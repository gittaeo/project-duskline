#pragma once
#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "StaminaComponent.generated.h"
DECLARE_DYNAMIC_MULTICAST_DELEGATE(FStaminaChanged);
UCLASS(ClassGroup=(Duskline), meta=(BlueprintSpawnableComponent))
class PROJECTDUSKLINE_API UStaminaComponent : public UActorComponent {
    GENERATED_BODY()
public:
    UStaminaComponent();
    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="Stamina") float Maximum=100;
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Stamina") float RecoveryRate=18;
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Stamina") float RecoveryDelay=1.5f;
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category="Stamina") float Current=100;
    UPROPERTY(BlueprintAssignable) FStaminaChanged OnChanged;
    UFUNCTION(BlueprintCallable) bool Spend(float Amount);
    void Advance(float DeltaSeconds);
protected:
    virtual void BeginPlay() override;
    virtual void EndPlay(const EEndPlayReason::Type Reason) override;
private:
    float DelayRemaining=0;
    double LastUpdate=0;
    FTimerHandle RecoveryTimer;
    void Recover();
};
