#include "Game/DusklineGameMode.h"
#include "Game/DusklineGameState.h"
#include "Game/DusklinePlayerController.h"
#include "Characters/DusklineCharacter.h"
ADusklineGameMode::ADusklineGameMode(){DefaultPawnClass=ADusklineCharacter::StaticClass(); PlayerControllerClass=ADusklinePlayerController::StaticClass(); GameStateClass=ADusklineGameState::StaticClass();}
