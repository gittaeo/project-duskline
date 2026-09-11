#include "Game/DusklinePlayerController.h"
#include "Characters/DusklineCharacter.h"
#include "Game/DusklineGameState.h"
#include "UI/DusklineHUD.h"
#include "Kismet/GameplayStatics.h"
#include "Engine/World.h"
void ADusklinePlayerController::BeginPlay(){
    Super::BeginPlay(); if(!IsLocalController()) return; bShowMouseCursor=false; SetInputMode(FInputModeGameOnly());
    Display=CreateWidget<UDusklineHUD>(this,UDusklineHUD::StaticClass());
    if(Display){Display->AddToViewport(); Display->InitializeFor(Cast<ADusklineCharacter>(GetPawn()),GetWorld()->GetGameState<ADusklineGameState>());}
}
void ADusklinePlayerController::RestartRun(){auto* S=GetWorld()->GetGameState<ADusklineGameState>(); if(S&&!S->IsRunning()) UGameplayStatics::OpenLevel(this,FName(*UGameplayStatics::GetCurrentLevelName(this,true)));}

void ADusklinePlayerController::OnPossess(APawn* InPawn){Super::OnPossess(InPawn); if(Display) Display->InitializeFor(Cast<ADusklineCharacter>(InPawn),GetWorld()->GetGameState<ADusklineGameState>());}
