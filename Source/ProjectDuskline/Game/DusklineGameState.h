#pragma once
#include "CoreMinimal.h"
#include "GameFramework/GameStateBase.h"
#include "DusklineGameState.generated.h"
UENUM(BlueprintType) enum class ERunResult : uint8 { Running, Escaped, Dead, TimedOut };
DECLARE_DYNAMIC_MULTICAST_DELEGATE(FRunChanged);
UCLASS()
class PROJECTDUSKLINE_API ADusklineGameState : public AGameStateBase {
    GENERATED_BODY()
public:
    ADusklineGameState();
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Run") float TimeLimit=600;
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Run") int32 RequiredValue=180;
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly) float Remaining=600;
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly) ERunResult Result=ERunResult::Running;
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly) float ExtractionRemaining=0;
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly) int32 FinalCount=0;
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly) int32 FinalValue=0;
    UPROPERTY(BlueprintAssignable) FRunChanged OnChanged;
    UFUNCTION(BlueprintPure) bool IsRunning() const {return Result==ERunResult::Running;}
    bool BeginExtraction(class ADusklineCharacter* Player,class AExtractionZone* Zone,float Duration);
    void Finish(ERunResult Requested,class ADusklineCharacter* Player);
    void CancelExtraction(AActor* Player);
protected:
    virtual void BeginPlay() override;
    virtual void EndPlay(const EEndPlayReason::Type Reason) override;
private:
    double Deadline=0, ExtractionDeadline=0;
    FTimerHandle RunTimer;
    TWeakObjectPtr<class ADusklineCharacter> ExtractingPlayer;
    TWeakObjectPtr<class AExtractionZone> ExtractionZone;
    void Advance();
};
