#pragma once
#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "NoiseEmitterComponent.generated.h"
USTRUCT(BlueprintType)
struct FNoiseProfile {
    GENERATED_BODY()
    UPROPERTY(EditAnywhere, BlueprintReadWrite) float Radius=600;
    UPROPERTY(EditAnywhere, BlueprintReadWrite) float Loudness=1;
    UPROPERTY(EditAnywhere, BlueprintReadWrite) float Duration=3;
};
UCLASS(ClassGroup=(Duskline), meta=(BlueprintSpawnableComponent))
class PROJECTDUSKLINE_API UNoiseEmitterComponent : public UActorComponent {
    GENERATED_BODY()
public:
    UNoiseEmitterComponent();
    UPROPERTY(EditAnywhere, BlueprintReadWrite) FNoiseProfile Crouch;
    UPROPERTY(EditAnywhere, BlueprintReadWrite) FNoiseProfile Walk;
    UPROPERTY(EditAnywhere, BlueprintReadWrite) FNoiseProfile Sprint;
    UPROPERTY(EditAnywhere, BlueprintReadWrite) FNoiseProfile Melee;
    UPROPERTY(EditAnywhere, BlueprintReadWrite) FNoiseProfile ForcedOpen;
    UFUNCTION(BlueprintCallable) void EmitProfile(const FNoiseProfile& Profile);
};
