#include "World/LootContainer.h"
#include "Components/StaticMeshComponent.h"
#include "Characters/DusklineCharacter.h"
#include "Components/NoiseEmitterComponent.h"
#include "UObject/ConstructorHelpers.h"
ALootContainer::ALootContainer(){
    PrimaryActorTick.bCanEverTick=false; Mesh=CreateDefaultSubobject<UStaticMeshComponent>(TEXT("Container")); SetRootComponent(Mesh);
    static ConstructorHelpers::FObjectFinder<UStaticMesh> Shape(TEXT("/Engine/BasicShapes/Cube.Cube"));
    if(Shape.Succeeded()) Mesh->SetStaticMesh(Shape.Object); Mesh->SetRelativeScale3D(FVector(0.9,0.9,0.8));
}
void ALootContainer::BeginPlay(){
    Super::BeginPlay(); if(!HasAuthority()) return; FRandomStream Random(bFixedSeed?Seed:FMath::Rand());
    for(const auto& Entry:LootTable){
        if(!IsValid(Entry.Item)||!Entry.Item->IsValidItem()||Entry.Minimum<0||Entry.Maximum<Entry.Minimum||Entry.Maximum>10000){UE_LOG(LogTemp,Warning,TEXT("Duskline: invalid loot row in %s"),*GetName()); continue;}
        if(Random.FRand()>=FMath::Clamp(Entry.Chance,0.f,1.f)) continue;
        FInventorySlot S; S.Item=Entry.Item; S.Quantity=Random.RandRange(Entry.Minimum,Entry.Maximum); if(S.Quantity>0) Contents.Add(S);
    }
}
FText ALootContainer::GetInteractionText_Implementation(AActor* User) const {
    FString Text=bOpened?TEXT("[E] 남은 물자 획득"):TEXT("[E] 컨테이너 열기 · 물자 확인");
    if(bOpened){ if(Contents.IsEmpty()) Text=TEXT("빈 컨테이너"); else for(const auto& S:Contents) if(IsValid(S.Item)) Text+=FString::Printf(TEXT("\n%s × %d"),*S.Item->DisplayName.ToString(),S.Quantity); }
    return FText::FromString(Text);
}
bool ALootContainer::CanInteract_Implementation(AActor* User) const { const auto* P=Cast<ADusklineCharacter>(User); return P&&P->CanAct()&&FVector::DistSquared(P->GetActorLocation(),GetActorLocation())<FMath::Square(350.f); }
void ALootContainer::Interact_Implementation(AActor* User){
    if(!HasAuthority()||!CanInteract_Implementation(User)) return; auto* P=CastChecked<ADusklineCharacter>(User);
    if(!bOpened){bOpened=true; if(bForcedOpen) P->Noise->EmitProfile(P->Noise->ForcedOpen); P->Notify(FText::FromString(TEXT("내용물을 확인했습니다. E로 모두 획득합니다"))); return;}
    if(Contents.IsEmpty()){P->Notify(FText::FromString(TEXT("컨테이너가 비어 있습니다"))); return;}
    int32 Taken=0; FText Reason;
    for(int32 I=Contents.Num()-1;I>=0;--I){auto R=P->Inventory->Add(Contents[I].Item,Contents[I].Quantity); Taken+=R.Processed; Contents[I].Quantity-=R.Processed; if(!R.bSuccess) Reason=R.Reason; if(Contents[I].Quantity<=0) Contents.RemoveAt(I);}
    P->Notify(Reason.IsEmpty()?FText::FromString(FString::Printf(TEXT("물자 %d개 획득"),Taken)):Reason);
}
