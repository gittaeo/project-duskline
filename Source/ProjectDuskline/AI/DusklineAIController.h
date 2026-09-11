#pragma once
#include "CoreMinimal.h"
#include "AIController.h"
#include "Perception/AIPerceptionTypes.h"
#include "DusklineAIController.generated.h"
UENUM(BlueprintType) enum class EInfectedState : uint8 { Idle, Patrol, Investigate, Chase, Attack, Dead };
UCLASS()
class PROJECTDUSKLINE_API ADusklineAIController : public AAIController {
    GENERATED_BODY()
public:
    ADusklineAIController();
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly) EInfectedState State=EInfectedState::Idle;
    UPROPERTY(EditAnywhere, BlueprintReadWrite) float SearchSeconds=7;
    UPROPERTY(EditAnywhere, BlueprintReadWrite) float AttackDistance=145;
    UFUNCTION(BlueprintCallable) void StopAI();
protected:
    virtual void BeginPlay() override;
    virtual void EndPlay(const EEndPlayReason::Type Reason) override;
private:
    UPROPERTY() TObjectPtr<class UAIPerceptionComponent> Senses;
    UPROPERTY(VisibleAnywhere) TObjectPtr<class UAISenseConfig_Sight> Sight;
    UPROPERTY(VisibleAnywhere) TObjectPtr<class UAISenseConfig_Hearing> Hearing;
    TWeakObjectPtr<class ADusklineCharacter> Target;
    FVector LastKnown=FVector::ZeroVector;
    double SearchUntil=0;
    int32 PatrolIndex=0;
    bool bVisible=false;
    FTimerHandle ThinkTimer;
    UFUNCTION() void Perceived(AActor* Actor, FAIStimulus Stimulus);
    void Think();
};
