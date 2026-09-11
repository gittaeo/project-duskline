#pragma once
#include "CoreMinimal.h"
#include "GameFramework/Character.h"
#include "InputActionValue.h"
#include "DusklineCharacter.generated.h"
UENUM(BlueprintType) enum class EMovementState : uint8 { Walking, Sprinting, Crouching, Airborne };
DECLARE_DYNAMIC_MULTICAST_DELEGATE(FPlayerChanged);
UCLASS()
class PROJECTDUSKLINE_API ADusklineCharacter : public ACharacter {
    GENERATED_BODY()
public:
    ADusklineCharacter();
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly) TObjectPtr<class UHealthComponent> Health;
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly) TObjectPtr<class UStaminaComponent> Stamina;
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly) TObjectPtr<class UInventoryComponent> Inventory;
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly) TObjectPtr<class UCombatComponent> Combat;
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly) TObjectPtr<class UNoiseEmitterComponent> Noise;
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Movement") float WalkSpeed=260;
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Movement") float SprintSpeed=520;
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Movement") float SprintDrain=24;
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Interaction") float InteractionRange=280;
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly) EMovementState MovementState=EMovementState::Walking;
    UPROPERTY(BlueprintAssignable) FPlayerChanged OnChanged;
    UPROPERTY(BlueprintReadOnly) FText Prompt;
    UPROPERTY(BlueprintReadOnly) FText Notice;
    void Notify(const FText& Text);
    bool CanAct() const;
    UFUNCTION() void Died();
protected:
    virtual void BeginPlay() override;
    virtual void EndPlay(const EEndPlayReason::Type Reason) override;
    virtual void SetupPlayerInputComponent(UInputComponent* Input) override;
private:
    UPROPERTY() TObjectPtr<class USpringArmComponent> Arm;
    UPROPERTY() TObjectPtr<class UCameraComponent> Camera;
    UPROPERTY() TObjectPtr<class UInputMappingContext> Mapping;
    UPROPERTY() TArray<TObjectPtr<class UInputAction>> Actions;
    TWeakObjectPtr<AActor> Focus;
    FTimerHandle UpdateTimer;
    bool bSprintHeld=false;
    double LastUpdate=0, NextStep=0;
    void Update();
    void Forward(const FInputActionValue& Value);
    void Right(const FInputActionValue& Value);
    void Turn(const FInputActionValue& Value);
    void Look(const FInputActionValue& Value);
    void SprintStart(); void SprintStop(); void ToggleCrouch(); void StartJump(); void EndJump();
    void Interact(); void Attack(); void UseMedicine(); void Restart();
};
