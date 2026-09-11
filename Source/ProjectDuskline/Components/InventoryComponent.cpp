#include "Components/InventoryComponent.h"
#include "GameFramework/Actor.h"
#include "Components/HealthComponent.h"
#include "Game/RunRules.h"
UInventoryComponent::UInventoryComponent(){ PrimaryComponentTick.bCanEverTick=false; }
float UInventoryComponent::GetWeight() const { float W=0; for(const auto& S:Slots) if(IsValid(S.Item)) W+=S.Item->Weight*S.Quantity; return W; }
int32 UInventoryComponent::GetValue() const { int64 V=0; for(const auto& S:Slots) if(IsValid(S.Item)) V+=int64(S.Item->Value)*S.Quantity; return int32(FMath::Min<int64>(V,MAX_int32)); }
int32 UInventoryComponent::GetCount() const { int32 N=0; for(const auto& S:Slots) N+=S.Quantity; return N; }
FItemTransfer UInventoryComponent::Add(UItemDataAsset* Item,int32 Count){
    FItemTransfer R;
    if(!IsValid(Item)||!Item->IsValidItem()||Count<=0||(GetOwner()&&!GetOwner()->HasAuthority())){
        R.Reason=FText::FromString(TEXT("잘못된 아이템 또는 요청")); UE_LOG(LogTemp,Warning,TEXT("Duskline: rejected invalid inventory addition")); return R;
    }
    for(const auto& S:Slots) if(IsValid(S.Item)&&S.Item!=Item&&S.Item->ID==Item->ID){R.Reason=FText::FromString(TEXT("아이템 ID 중복")); return R;}
    const float Available=FMath::Max(0.f,MaxWeight-GetWeight());
    int32 Remaining=DusklineRules::WeightCapacity(Available,Item->Weight,Count);
    const int32 Allowed=Remaining;
    for(auto& S:Slots) if(S.Item==Item&&Remaining>0){ int32 N=FMath::Min(Remaining,FMath::Max(0,Item->MaxStack-S.Quantity)); S.Quantity+=N; Remaining-=N; }
    while(Remaining>0&&Slots.Num()<FMath::Clamp(MaxSlots,0,1000)){
        FInventorySlot S; S.Item=Item; S.Quantity=FMath::Min(Remaining,Item->MaxStack); Remaining-=S.Quantity; Slots.Add(S);
    }
    R.Processed=Allowed-Remaining; R.bSuccess=R.Processed==Count;
    if(!R.bSuccess) R.Reason=FText::FromString(Allowed<Count?TEXT("무게 한도 초과 — 남은 물자는 보관됩니다"):TEXT("인벤토리 슬롯 부족 — 남은 물자는 보관됩니다"));
    if(R.Processed>0) OnChanged.Broadcast(); return R;
}
FItemTransfer UInventoryComponent::Remove(UItemDataAsset* Item,int32 Count){
    FItemTransfer R; if(!IsValid(Item)||Count<=0||(GetOwner()&&!GetOwner()->HasAuthority())) return R;
    int32 Remaining=Count;
    for(int32 I=Slots.Num()-1;I>=0&&Remaining>0;--I) if(Slots[I].Item==Item){int32 N=FMath::Min(Remaining,Slots[I].Quantity); Slots[I].Quantity-=N; Remaining-=N; if(Slots[I].Quantity==0) Slots.RemoveAt(I);}
    R.Processed=Count-Remaining; R.bSuccess=Remaining==0; if(R.Processed>0) OnChanged.Broadcast(); return R;
}
FItemTransfer UInventoryComponent::UseMedicine(UHealthComponent* Health){
    FItemTransfer R;
    if((GetOwner()&&!GetOwner()->HasAuthority())||!IsValid(Health)||!Health->IsAlive()||Health->Health>=Health->MaxHealth){R.Reason=FText::FromString(TEXT("지금은 치료할 수 없습니다")); return R;}
    for(const auto& S:Slots) if(IsValid(S.Item)&&S.Item->IsValidItem()&&S.Item->Kind==EItemKind::Medicine&&S.Quantity>0&&S.Item->Healing>0){
        UItemDataAsset* Item=S.Item; if(Health->Heal(Item->Healing)>0) return Remove(Item,1); return R;
    }
    R.Reason=FText::FromString(TEXT("의약품이 없습니다")); return R;
}
