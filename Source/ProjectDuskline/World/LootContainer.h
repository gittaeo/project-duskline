#pragma once
#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "Interaction/InteractableInterface.h"
#include "Components/InventoryComponent.h"
#include "LootContainer.generated.h"
USTRUCT(BlueprintType)
struct FLootEntry {
    GENERATED_BODY()
    UPROPERTY(EditAnywhere, BlueprintReadWrite) TObjectPtr<UItemDataAsset> Item=nullptr;
    UPROPERTY(EditAnywhere, BlueprintReadWrite) int32 Minimum=1;
    UPROPERTY(EditAnywhere, BlueprintReadWrite) int32 Maximum=3;
    UPROPERTY(EditAnywhere, BlueprintReadWrite, meta=(ClampMin="0",ClampMax="1")) float Chance=1;
};
UCLASS()
class PROJECTDUSKLINE_API ALootContainer : public AActor, public IInteractableInterface {
    GENERATED_BODY()
public:
    ALootContainer();
    UPROPERTY(VisibleAnywhere) TObjectPtr<class UStaticMeshComponent> Mesh;
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Loot") TArray<FLootEntry> LootTable;
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Loot") bool bFixedSeed=true;
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Loot") int32 Seed=1337;
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Loot") bool bForcedOpen=false;
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly) TArray<FInventorySlot> Contents;
    virtual FText GetInteractionText_Implementation(AActor* User) const override;
    virtual bool CanInteract_Implementation(AActor* User) const override;
    virtual void Interact_Implementation(AActor* User) override;
protected:
    virtual void BeginPlay() override;
private:
    bool bOpened=false;
};
