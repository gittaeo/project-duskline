#pragma once
#include "CoreMinimal.h"
#include "Engine/DataAsset.h"
#include "ItemDataAsset.generated.h"
UENUM(BlueprintType) enum class EItemKind : uint8 { Food, Medicine, Scrap, Mission };
UCLASS(BlueprintType)
class PROJECTDUSKLINE_API UItemDataAsset : public UDataAsset {
    GENERATED_BODY()
public:
    UPROPERTY(EditAnywhere, BlueprintReadOnly) FName ID;
    UPROPERTY(EditAnywhere, BlueprintReadOnly) FText DisplayName;
    UPROPERTY(EditAnywhere, BlueprintReadOnly, meta=(MultiLine=true)) FText Description;
    UPROPERTY(EditAnywhere, BlueprintReadOnly) EItemKind Kind=EItemKind::Food;
    UPROPERTY(EditAnywhere, BlueprintReadOnly) float Weight=1;
    UPROPERTY(EditAnywhere, BlueprintReadOnly) int32 Value=10;
    UPROPERTY(EditAnywhere, BlueprintReadOnly) int32 MaxStack=5;
    UPROPERTY(EditAnywhere, BlueprintReadOnly) float Healing=30;
    UPROPERTY(EditAnywhere, BlueprintReadOnly) TSoftObjectPtr<class UTexture2D> Icon;
    UFUNCTION(BlueprintPure) bool IsValidItem() const { return !ID.IsNone() && !DisplayName.IsEmpty() && FMath::IsFinite(Weight) && Weight>0 && Value>=0 && MaxStack>0 && MaxStack<=10000 && FMath::IsFinite(Healing) && Healing>=0; }
};
