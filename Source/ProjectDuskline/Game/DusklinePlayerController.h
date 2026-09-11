#pragma once
#include "CoreMinimal.h"
#include "GameFramework/PlayerController.h"
#include "DusklinePlayerController.generated.h"
UCLASS()
class PROJECTDUSKLINE_API ADusklinePlayerController : public APlayerController {
    GENERATED_BODY()
public:
    UFUNCTION(BlueprintCallable) void RestartRun();
protected:
    virtual void BeginPlay() override;
    virtual void OnPossess(APawn* InPawn) override;
    UPROPERTY() TObjectPtr<class UDusklineHUD> Display;
};
