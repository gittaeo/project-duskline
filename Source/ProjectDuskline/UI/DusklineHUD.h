#pragma once
#include "CoreMinimal.h"
#include "Blueprint/UserWidget.h"
#include "DusklineHUD.generated.h"
UCLASS()
class PROJECTDUSKLINE_API UDusklineHUD : public UUserWidget {
    GENERATED_BODY()
public:
    void InitializeFor(class ADusklineCharacter* Player,class ADusklineGameState* State);
protected:
    virtual void NativeConstruct() override;
    virtual void NativeDestruct() override;
private:
    UPROPERTY() TObjectPtr<class UTextBlock> Status;
    UPROPERTY() TObjectPtr<class UTextBlock> Items;
    UPROPERTY() TObjectPtr<class UTextBlock> Prompt;
    UPROPERTY() TObjectPtr<class UTextBlock> ResultText;
    UPROPERTY() TObjectPtr<class UProgressBar> HealthBar;
    UPROPERTY() TObjectPtr<class UProgressBar> StaminaBar;
    UPROPERTY() TObjectPtr<class UButton> RestartButton;
    TWeakObjectPtr<class ADusklineCharacter> Player;
    TWeakObjectPtr<class ADusklineGameState> Run;
    float PreviousHealth=100;
    double HitUntil=0,SwingUntil=0;
    bool bResultShown=false;
    UFUNCTION() void Refresh();
    UFUNCTION() void HealthChanged();
    UFUNCTION() void Swing();
    UFUNCTION() void Restart();
    void Unbind();
};
