#include "AI/DusklineAICharacter.h"
#include "AI/DusklineAIController.h"
#include "Components/HealthComponent.h"
#include "Components/CombatComponent.h"
#include "Components/StaticMeshComponent.h"
#include "Components/CapsuleComponent.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "UObject/ConstructorHelpers.h"
#include "TimerManager.h"
#include "Engine/World.h"
ADusklineAICharacter::ADusklineAICharacter(){
    PrimaryActorTick.bCanEverTick=false; Tags.Add(TEXT("Infected"));
    Health=CreateDefaultSubobject<UHealthComponent>(TEXT("Health")); Health->MaxHealth=90;
    Combat=CreateDefaultSubobject<UCombatComponent>(TEXT("Combat")); Combat->Damage=14; Combat->Cooldown=1.25f; Combat->Reach=100; Combat->Radius=35;
    AIControllerClass=ADusklineAIController::StaticClass(); AutoPossessAI=EAutoPossessAI::PlacedInWorldOrSpawned;
    GetCharacterMovement()->MaxWalkSpeed=310; GetCharacterMovement()->bOrientRotationToMovement=true; bUseControllerRotationYaw=false;
    Body=CreateDefaultSubobject<UStaticMeshComponent>(TEXT("Body")); Body->SetupAttachment(GetRootComponent()); Body->SetCollisionEnabled(ECollisionEnabled::NoCollision);
    static ConstructorHelpers::FObjectFinder<UStaticMesh> Shape(TEXT("/Engine/BasicShapes/Cone.Cone")); if(Shape.Succeeded()) Body->SetStaticMesh(Shape.Object); Body->SetRelativeScale3D(FVector(0.7,0.7,1.6));
}
void ADusklineAICharacter::BeginPlay(){Super::BeginPlay(); Health->OnDied.AddDynamic(this,&ADusklineAICharacter::Died); Health->OnChanged.AddDynamic(this,&ADusklineAICharacter::React);}
void ADusklineAICharacter::React(){if(Health->IsAlive()){Body->SetRelativeScale3D(FVector(0.9,0.9,1.2)); GetWorld()->GetTimerManager().SetTimer(ReactionTimer,this,&ADusklineAICharacter::ResetReaction,0.15f,false);}}
void ADusklineAICharacter::ResetReaction(){if(Health->IsAlive()) Body->SetRelativeScale3D(FVector(0.7,0.7,1.6));}
void ADusklineAICharacter::Died(){GetWorld()->GetTimerManager().ClearTimer(ReactionTimer); GetCharacterMovement()->DisableMovement(); GetCapsuleComponent()->SetCollisionEnabled(ECollisionEnabled::NoCollision); Body->SetRelativeRotation(FRotator(90,0,0)); Body->SetRelativeLocation(FVector(0,0,-65)); if(auto* AI=Cast<ADusklineAIController>(GetController())) AI->StopAI();}
void ADusklineAICharacter::EndPlay(const EEndPlayReason::Type Reason){GetWorld()->GetTimerManager().ClearTimer(ReactionTimer); Super::EndPlay(Reason);}
