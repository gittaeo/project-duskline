#pragma once
#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "Interaction/InteractableInterface.h"
#include "ExtractionZone.generated.h"
UCLASS()
class PROJECTDUSKLINE_API AExtractionZone : public AActor,public IInteractableInterface {
    GENERATED_BODY()
public:
    AExtractionZone();
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly) TObjectPtr<class UBoxComponent> Area;
    UPROPERTY(VisibleAnywhere) TObjectPtr<class UStaticMeshComponent> Beacon;
    UPROPERTY(EditAnywhere, BlueprintReadWrite) float Countdown=8;
    bool Contains(const AActor* Player) const;
    virtual FText GetInteractionText_Implementation(AActor* User) const override;
    virtual bool CanInteract_Implementation(AActor* User) const override;
    virtual void Interact_Implementation(AActor* User) override;
private:
    UFUNCTION() void LeftZone(class UPrimitiveComponent* Overlapped,AActor* Other,class UPrimitiveComponent* OtherComponent,int32 BodyIndex);
};
