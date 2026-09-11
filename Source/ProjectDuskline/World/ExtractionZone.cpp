#include "World/ExtractionZone.h"
#include "Components/BoxComponent.h"
#include "Components/StaticMeshComponent.h"
#include "Characters/DusklineCharacter.h"
#include "Game/DusklineGameState.h"
#include "Components/InventoryComponent.h"
#include "Engine/World.h"
#include "UObject/ConstructorHelpers.h"
AExtractionZone::AExtractionZone(){
    PrimaryActorTick.bCanEverTick=false; Area=CreateDefaultSubobject<UBoxComponent>(TEXT("Area")); SetRootComponent(Area); Area->SetBoxExtent(FVector(260,260,180)); Area->SetCollisionProfileName(TEXT("Trigger")); Area->OnComponentEndOverlap.AddDynamic(this,&AExtractionZone::LeftZone);
    Beacon=CreateDefaultSubobject<UStaticMeshComponent>(TEXT("Beacon")); Beacon->SetupAttachment(Area);
    static ConstructorHelpers::FObjectFinder<UStaticMesh> Shape(TEXT("/Engine/BasicShapes/Cylinder.Cylinder")); if(Shape.Succeeded()) Beacon->SetStaticMesh(Shape.Object);
    Beacon->SetRelativeScale3D(FVector(0.8,0.8,2)); Beacon->SetCollisionProfileName(TEXT("BlockAll"));
}
void AExtractionZone::LeftZone(UPrimitiveComponent* Overlapped,AActor* Other,UPrimitiveComponent* OtherComponent,int32 BodyIndex){if(auto* S=GetWorld()->GetGameState<ADusklineGameState>()) S->CancelExtraction(Other);}
bool AExtractionZone::Contains(const AActor* P) const {return IsValid(P)&&Area->IsOverlappingActor(P);}
FText AExtractionZone::GetInteractionText_Implementation(AActor* User) const {return FText::FromString(TEXT("[E] 탈출 요청 · 구역 안에서 대기"));}
bool AExtractionZone::CanInteract_Implementation(AActor* User) const {auto* P=Cast<ADusklineCharacter>(User); return P&&P->CanAct()&&Contains(P);}
void AExtractionZone::Interact_Implementation(AActor* User){
    if(!HasAuthority()||!CanInteract_Implementation(User)) return; auto* P=CastChecked<ADusklineCharacter>(User); auto* State=GetWorld()->GetGameState<ADusklineGameState>();
    if(State&&!State->BeginExtraction(P,this,Countdown)) P->Notify(FText::FromString(TEXT("탈출 불가 — 요구 가치 또는 진행 중인 요청 확인")));
}
