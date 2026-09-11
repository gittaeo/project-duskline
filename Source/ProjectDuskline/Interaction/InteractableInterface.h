#pragma once
#include "CoreMinimal.h"
#include "UObject/Interface.h"
#include "InteractableInterface.generated.h"
UINTERFACE(BlueprintType)
class PROJECTDUSKLINE_API UInteractableInterface : public UInterface { GENERATED_BODY() };
class PROJECTDUSKLINE_API IInteractableInterface {
    GENERATED_BODY()
public:
    UFUNCTION(BlueprintNativeEvent, BlueprintCallable) FText GetInteractionText(AActor* User) const;
    UFUNCTION(BlueprintNativeEvent, BlueprintCallable) bool CanInteract(AActor* User) const;
    UFUNCTION(BlueprintNativeEvent, BlueprintCallable) void Interact(AActor* User);
};
