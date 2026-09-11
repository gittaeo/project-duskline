#include "Characters/DusklineCharacter.h"
#include "Components/HealthComponent.h"
#include "Components/StaminaComponent.h"
#include "Components/InventoryComponent.h"
#include "Components/CombatComponent.h"
#include "Components/NoiseEmitterComponent.h"
#include "Components/StaticMeshComponent.h"
#include "Components/CapsuleComponent.h"
#include "Camera/CameraComponent.h"
#include "GameFramework/SpringArmComponent.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "Game/DusklineGameState.h"
#include "Game/DusklinePlayerController.h"
#include "Interaction/InteractableInterface.h"
#include "EnhancedInputComponent.h"
#include "EnhancedInputSubsystems.h"
#include "InputAction.h"
#include "InputMappingContext.h"
#include "InputModifiers.h"
#include "Engine/LocalPlayer.h"
#include "Engine/World.h"
#include "TimerManager.h"
#include "UObject/ConstructorHelpers.h"
ADusklineCharacter::ADusklineCharacter(){
    PrimaryActorTick.bCanEverTick=false; Tags.Add(TEXT("Player"));
    Health=CreateDefaultSubobject<UHealthComponent>(TEXT("Health")); Stamina=CreateDefaultSubobject<UStaminaComponent>(TEXT("Stamina")); Inventory=CreateDefaultSubobject<UInventoryComponent>(TEXT("Inventory")); Combat=CreateDefaultSubobject<UCombatComponent>(TEXT("Combat")); Noise=CreateDefaultSubobject<UNoiseEmitterComponent>(TEXT("Noise"));
    GetCharacterMovement()->GetNavAgentPropertiesRef().bCanCrouch=true; GetCharacterMovement()->MaxWalkSpeedCrouched=130; GetCharacterMovement()->bOrientRotationToMovement=true; GetCharacterMovement()->RotationRate=FRotator(0,540,0); bUseControllerRotationYaw=false;
    Arm=CreateDefaultSubobject<USpringArmComponent>(TEXT("CameraArm")); Arm->SetupAttachment(GetRootComponent()); Arm->TargetArmLength=340; Arm->bUsePawnControlRotation=true; Arm->SocketOffset=FVector(0,45,65);
    Camera=CreateDefaultSubobject<UCameraComponent>(TEXT("Camera")); Camera->SetupAttachment(Arm);
    auto* Body=CreateDefaultSubobject<UStaticMeshComponent>(TEXT("Body")); Body->SetupAttachment(GetRootComponent()); Body->SetCollisionEnabled(ECollisionEnabled::NoCollision); Body->SetRelativeScale3D(FVector(0.55,0.55,1.5));
    static ConstructorHelpers::FObjectFinder<UStaticMesh> Shape(TEXT("/Engine/BasicShapes/Cylinder.Cylinder")); if(Shape.Succeeded()) Body->SetStaticMesh(Shape.Object);
}
void ADusklineCharacter::BeginPlay(){Super::BeginPlay(); GetCharacterMovement()->MaxWalkSpeed=WalkSpeed; Health->OnDied.AddDynamic(this,&ADusklineCharacter::Died); LastUpdate=GetWorld()->GetTimeSeconds(); GetWorld()->GetTimerManager().SetTimer(UpdateTimer,this,&ADusklineCharacter::Update,0.1f,true);}
void ADusklineCharacter::EndPlay(const EEndPlayReason::Type Reason){
    GetWorld()->GetTimerManager().ClearTimer(UpdateTimer);
    if(auto* PC=Cast<APlayerController>(Controller)) if(auto* Local=PC->GetLocalPlayer()) if(auto* S=Local->GetSubsystem<UEnhancedInputLocalPlayerSubsystem>()) if(Mapping) S->RemoveMappingContext(Mapping);
    Super::EndPlay(Reason);
}
bool ADusklineCharacter::CanAct() const {auto* S=GetWorld()?GetWorld()->GetGameState<ADusklineGameState>():nullptr; return Health&&Health->IsAlive()&&(!S||S->IsRunning());}
void ADusklineCharacter::Notify(const FText& Text){Notice=Text; OnChanged.Broadcast();}
void ADusklineCharacter::Died(){bSprintHeld=false; GetCharacterMovement()->DisableMovement(); if(auto* S=GetWorld()->GetGameState<ADusklineGameState>()) S->Finish(ERunResult::Dead,this);}
void ADusklineCharacter::Forward(const FInputActionValue& V){if(CanAct()&&Controller) AddMovementInput(FRotationMatrix(FRotator(0,Controller->GetControlRotation().Yaw,0)).GetUnitAxis(EAxis::X),V.Get<float>());}
void ADusklineCharacter::Right(const FInputActionValue& V){if(CanAct()&&Controller) AddMovementInput(FRotationMatrix(FRotator(0,Controller->GetControlRotation().Yaw,0)).GetUnitAxis(EAxis::Y),V.Get<float>());}
void ADusklineCharacter::Turn(const FInputActionValue& V){if(CanAct()) AddControllerYawInput(V.Get<float>());}
void ADusklineCharacter::Look(const FInputActionValue& V){if(CanAct()) AddControllerPitchInput(-V.Get<float>());}
void ADusklineCharacter::SprintStart(){if(CanAct()) bSprintHeld=true;}
void ADusklineCharacter::SprintStop(){bSprintHeld=false;}
void ADusklineCharacter::ToggleCrouch(){if(CanAct()){bSprintHeld=false; if(bIsCrouched) UnCrouch(); else Crouch();}}
void ADusklineCharacter::StartJump(){if(CanAct()) Jump();}
void ADusklineCharacter::EndJump(){StopJumping();}
void ADusklineCharacter::Attack(){if(CanAct()){if(Controller) SetActorRotation(FRotator(0,Controller->GetControlRotation().Yaw,0)); if(!Combat->TryAttack()) Notify(FText::FromString(TEXT("공격 대기 또는 스태미나 부족")));}}
void ADusklineCharacter::UseMedicine(){if(CanAct()){auto R=Inventory->UseMedicine(Health); Notify(R.bSuccess?FText::FromString(TEXT("의약품 사용")):R.Reason);}}
void ADusklineCharacter::Restart(){if(auto* PC=Cast<ADusklinePlayerController>(Controller)) PC->RestartRun();}
void ADusklineCharacter::Interact(){
    if(!CanAct()||!HasAuthority()) return;
    Update(); AActor* Target=Focus.Get();
    if(IsValid(Target)&&Target->Implements<UInteractableInterface>()&&IInteractableInterface::Execute_CanInteract(Target,this)) IInteractableInterface::Execute_Interact(Target,this);
}
void ADusklineCharacter::Update(){
    double Now=GetWorld()->GetTimeSeconds(); float Delta=FMath::Max(0.f,float(Now-LastUpdate)); LastUpdate=Now;
    if(!CanAct()){GetCharacterMovement()->StopMovementImmediately(); Focus.Reset(); Prompt=FText::GetEmpty(); OnChanged.Broadcast(); return;}
    bool bMoving=GetVelocity().SizeSquared2D()>100;
    bool bSprint=bSprintHeld&&bMoving&&!bIsCrouched&&!GetCharacterMovement()->IsFalling();
    if(bSprint&&!Stamina->Spend(FMath::Max(0.f,SprintDrain)*Delta)){bSprint=false; bSprintHeld=false;}
    GetCharacterMovement()->MaxWalkSpeed=bSprint?SprintSpeed:WalkSpeed;
    MovementState=GetCharacterMovement()->IsFalling()?EMovementState::Airborne:(bIsCrouched?EMovementState::Crouching:(bSprint?EMovementState::Sprinting:EMovementState::Walking));
    if(bMoving&&!GetCharacterMovement()->IsFalling()&&Now>=NextStep){Noise->EmitProfile(bIsCrouched?Noise->Crouch:(bSprint?Noise->Sprint:Noise->Walk)); NextStep=Now+(bSprint?0.4:0.7);}
    Focus.Reset(); Prompt=FText::GetEmpty();
    if(Controller){
        FVector View; FRotator Rotation; Controller->GetPlayerViewPoint(View,Rotation);
        FHitResult Hit; FCollisionQueryParams Params(SCENE_QUERY_STAT(DuskInteract),false,this);
        if(GetWorld()->LineTraceSingleByChannel(Hit,View,View+Rotation.Vector()*(Arm->TargetArmLength+InteractionRange),ECC_Visibility,Params)){
            AActor* A=Hit.GetActor(); if(IsValid(A)&&FVector::DistSquared(GetActorLocation(),Hit.ImpactPoint)<=FMath::Square(InteractionRange)&&A->Implements<UInteractableInterface>()){
                Focus=A; Prompt=IInteractableInterface::Execute_GetInteractionText(A,this);
            }
        }
    }
    OnChanged.Broadcast();
}
void ADusklineCharacter::SetupPlayerInputComponent(UInputComponent* Input){
    Super::SetupPlayerInputComponent(Input); auto* E=Cast<UEnhancedInputComponent>(Input); auto* PC=Cast<APlayerController>(Controller);
    if(!E||!PC||!PC->GetLocalPlayer()){UE_LOG(LogTemp,Error,TEXT("Duskline: Enhanced Input unavailable")); return;}
    Mapping=NewObject<UInputMappingContext>(this);
    auto Action=[this](EInputActionValueType Type){auto* A=NewObject<UInputAction>(this); A->ValueType=Type; Actions.Add(A); return A;};
    auto Axis=[this,&Action](FKey Positive,FKey Negative){auto* A=Action(EInputActionValueType::Axis1D); Mapping->MapKey(A,Positive); auto& M=Mapping->MapKey(A,Negative); M.Modifiers.Add(NewObject<UInputModifierNegate>(Mapping)); return A;};
    E->BindAction(Axis(EKeys::W,EKeys::S),ETriggerEvent::Triggered,this,&ADusklineCharacter::Forward);
    E->BindAction(Axis(EKeys::D,EKeys::A),ETriggerEvent::Triggered,this,&ADusklineCharacter::Right);
    auto* Yaw=Action(EInputActionValueType::Axis1D); Mapping->MapKey(Yaw,EKeys::MouseX); E->BindAction(Yaw,ETriggerEvent::Triggered,this,&ADusklineCharacter::Turn);
    auto* Pitch=Action(EInputActionValueType::Axis1D); Mapping->MapKey(Pitch,EKeys::MouseY); E->BindAction(Pitch,ETriggerEvent::Triggered,this,&ADusklineCharacter::Look);
    auto Button=[this,&Action](FKey Key){auto* A=Action(EInputActionValueType::Boolean); Mapping->MapKey(A,Key); return A;};
    auto* SprintAction=Button(EKeys::LeftShift); E->BindAction(SprintAction,ETriggerEvent::Started,this,&ADusklineCharacter::SprintStart); E->BindAction(SprintAction,ETriggerEvent::Completed,this,&ADusklineCharacter::SprintStop); E->BindAction(SprintAction,ETriggerEvent::Canceled,this,&ADusklineCharacter::SprintStop);
    auto* JumpAction=Button(EKeys::SpaceBar); E->BindAction(JumpAction,ETriggerEvent::Started,this,&ADusklineCharacter::StartJump); E->BindAction(JumpAction,ETriggerEvent::Completed,this,&ADusklineCharacter::EndJump);
    E->BindAction(Button(EKeys::LeftControl),ETriggerEvent::Started,this,&ADusklineCharacter::ToggleCrouch);
    E->BindAction(Button(EKeys::E),ETriggerEvent::Started,this,&ADusklineCharacter::Interact);
    E->BindAction(Button(EKeys::LeftMouseButton),ETriggerEvent::Started,this,&ADusklineCharacter::Attack);
    E->BindAction(Button(EKeys::Q),ETriggerEvent::Started,this,&ADusklineCharacter::UseMedicine);
    E->BindAction(Button(EKeys::R),ETriggerEvent::Started,this,&ADusklineCharacter::Restart);
    if(auto* S=PC->GetLocalPlayer()->GetSubsystem<UEnhancedInputLocalPlayerSubsystem>()) S->AddMappingContext(Mapping,0);
}
