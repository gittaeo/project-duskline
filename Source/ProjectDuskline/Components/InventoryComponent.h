#pragma once
#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "Items/ItemDataAsset.h"
#include "InventoryComponent.generated.h"
USTRUCT(BlueprintType)
struct FInventorySlot {
    GENERATED_BODY()
    UPROPERTY(EditAnywhere, BlueprintReadOnly) TObjectPtr<UItemDataAsset> Item=nullptr;
    UPROPERTY(EditAnywhere, BlueprintReadOnly) int32 Quantity=0;
};
USTRUCT(BlueprintType)
struct FItemTransfer {
    GENERATED_BODY()
    UPROPERTY(BlueprintReadOnly) bool bSuccess=false;
    UPROPERTY(BlueprintReadOnly) int32 Processed=0;
    UPROPERTY(BlueprintReadOnly) FText Reason;
};
DECLARE_DYNAMIC_MULTICAST_DELEGATE(FInventoryChanged);
UCLASS(ClassGroup=(Duskline), meta=(BlueprintSpawnableComponent))
class PROJECTDUSKLINE_API UInventoryComponent : public UActorComponent {
    GENERATED_BODY()
public:
    UInventoryComponent();
    UPROPERTY(EditAnywhere, BlueprintReadOnly) int32 MaxSlots=10;
    UPROPERTY(EditAnywhere, BlueprintReadOnly) float MaxWeight=24;
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly) TArray<FInventorySlot> Slots;
    UPROPERTY(BlueprintAssignable) FInventoryChanged OnChanged;
    UFUNCTION(BlueprintPure) float GetWeight() const;
    UFUNCTION(BlueprintPure) int32 GetValue() const;
    UFUNCTION(BlueprintPure) int32 GetCount() const;
    UFUNCTION(BlueprintCallable) FItemTransfer Add(UItemDataAsset* Item, int32 Count);
    UFUNCTION(BlueprintCallable) FItemTransfer Remove(UItemDataAsset* Item, int32 Count);
    UFUNCTION(BlueprintCallable) FItemTransfer UseMedicine(class UHealthComponent* Health);
};
