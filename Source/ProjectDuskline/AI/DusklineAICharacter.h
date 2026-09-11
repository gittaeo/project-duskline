#pragma once
#include "CoreMinimal.h"
#include "GameFramework/Character.h"
#include "DusklineAICharacter.generated.h"
UCLASS()
class PROJECTDUSKLINE_API ADusklineAICharacter : public ACharacter {
    GENERATED_BODY()
public:
    ADusklineAICharacter();
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly) TObjectPtr<class UHealthComponent> Health;
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly) TObjectPtr<class UCombatComponent> Combat;
    UPROPERTY(EditAnywhere, BlueprintReadWrite) TArray<FVector> PatrolPoints;
protected:
    virtual void BeginPlay() override;
    virtual void EndPlay(const EEndPlayReason::Type Reason) override;
private:
    UPROPERTY() TObjectPtr<class UStaticMeshComponent> Body;
    FTimerHandle ReactionTimer;
    UFUNCTION() void Died();
    UFUNCTION() void React();
    void ResetReaction();
};
