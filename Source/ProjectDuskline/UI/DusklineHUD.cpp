#include "UI/DusklineHUD.h"
#include "Blueprint/WidgetTree.h"
#include "Components/Overlay.h"
#include "Components/OverlaySlot.h"
#include "Components/VerticalBox.h"
#include "Components/VerticalBoxSlot.h"
#include "Components/SizeBox.h"
#include "Components/TextBlock.h"
#include "Components/Button.h"
#include "Components/ProgressBar.h"
#include "Characters/DusklineCharacter.h"
#include "Game/DusklineGameState.h"
#include "Game/DusklinePlayerController.h"
#include "Components/HealthComponent.h"
#include "Components/StaminaComponent.h"
#include "Components/InventoryComponent.h"
#include "Components/CombatComponent.h"
#include "Engine/World.h"
void UDusklineHUD::NativeConstruct(){
    Super::NativeConstruct(); if(Status) return;
    auto* Root=WidgetTree->ConstructWidget<UOverlay>(); WidgetTree->RootWidget=Root;
    auto* Width=WidgetTree->ConstructWidget<USizeBox>(); Width->SetWidthOverride(460);
    auto* Position=Root->AddChildToOverlay(Width); Position->SetHorizontalAlignment(HAlign_Left); Position->SetVerticalAlignment(VAlign_Top); Position->SetPadding(FMargin(24));
    auto* Column=WidgetTree->ConstructWidget<UVerticalBox>(); Width->SetContent(Column);
    auto Text=[this,Column](int32 Size){auto* T=WidgetTree->ConstructWidget<UTextBlock>(); auto Font=T->GetFont(); Font.Size=Size; T->SetFont(Font); T->SetAutoWrapText(true); T->SetShadowColorAndOpacity(FLinearColor::Black); T->SetShadowOffset(FVector2D(1,1)); Column->AddChildToVerticalBox(T)->SetPadding(FMargin(0,4)); return T;};
    auto* Title=Text(26); Title->SetText(FText::FromString(TEXT("황혼선 | 해원시 격리 구역")));
    HealthBar=WidgetTree->ConstructWidget<UProgressBar>(); HealthBar->SetFillColorAndOpacity(FLinearColor(0.8f,0.15f,0.12f)); Column->AddChildToVerticalBox(HealthBar);
    StaminaBar=WidgetTree->ConstructWidget<UProgressBar>(); StaminaBar->SetFillColorAndOpacity(FLinearColor(0.1f,0.7f,0.5f)); Column->AddChildToVerticalBox(StaminaBar);
    Status=Text(18); Items=Text(16); Prompt=Text(19); ResultText=Text(25);
    RestartButton=WidgetTree->ConstructWidget<UButton>(); auto* Label=WidgetTree->ConstructWidget<UTextBlock>(); Label->SetText(FText::FromString(TEXT("다시 수색하기 [R]"))); RestartButton->SetContent(Label); Column->AddChildToVerticalBox(RestartButton); RestartButton->OnClicked.AddDynamic(this,&UDusklineHUD::Restart); RestartButton->SetVisibility(ESlateVisibility::Collapsed);
    auto* Help=Text(14); Help->SetText(FText::FromString(TEXT("WASD 이동 · 마우스 시점 · Shift 달리기\nCtrl 웅크리기 · Space 점프 · 좌클릭 공격\nE 확인/획득/탈출 · Q 의약품 · R 결과 후 재시작\n북동쪽 원통 비콘이 탈출 지점입니다")));
    auto* Cross=WidgetTree->ConstructWidget<UTextBlock>(); Cross->SetText(FText::FromString(TEXT("+"))); auto* Center=Root->AddChildToOverlay(Cross); Center->SetHorizontalAlignment(HAlign_Center); Center->SetVerticalAlignment(VAlign_Center); Cross->SetVisibility(ESlateVisibility::HitTestInvisible);
    Refresh();
}
void UDusklineHUD::InitializeFor(ADusklineCharacter* P,ADusklineGameState* S){
    Unbind(); Player=P; Run=S;
    if(P){PreviousHealth=P->Health->Health; P->Health->OnChanged.AddDynamic(this,&UDusklineHUD::HealthChanged); P->Stamina->OnChanged.AddDynamic(this,&UDusklineHUD::Refresh); P->Inventory->OnChanged.AddDynamic(this,&UDusklineHUD::Refresh); P->OnChanged.AddDynamic(this,&UDusklineHUD::Refresh); P->Combat->OnSwing.AddDynamic(this,&UDusklineHUD::Swing);}
    if(S) S->OnChanged.AddDynamic(this,&UDusklineHUD::Refresh); Refresh();
}
void UDusklineHUD::Unbind(){if(auto* P=Player.Get()){P->Health->OnChanged.RemoveDynamic(this,&UDusklineHUD::HealthChanged); P->Stamina->OnChanged.RemoveDynamic(this,&UDusklineHUD::Refresh); P->Inventory->OnChanged.RemoveDynamic(this,&UDusklineHUD::Refresh); P->OnChanged.RemoveDynamic(this,&UDusklineHUD::Refresh); P->Combat->OnSwing.RemoveDynamic(this,&UDusklineHUD::Swing);} if(auto* S=Run.Get()) S->OnChanged.RemoveDynamic(this,&UDusklineHUD::Refresh);}
void UDusklineHUD::NativeDestruct(){Unbind(); Super::NativeDestruct();}
void UDusklineHUD::HealthChanged(){if(auto* P=Player.Get()){if(P->Health->Health<PreviousHealth) HitUntil=GetWorld()->GetTimeSeconds()+0.6; PreviousHealth=P->Health->Health;} Refresh();}
void UDusklineHUD::Swing(){SwingUntil=GetWorld()->GetTimeSeconds()+0.25; Refresh();}
void UDusklineHUD::Refresh(){
    auto* P=Player.Get(); auto* S=Run.Get(); if(!Status||!P||!S) return;
    HealthBar->SetPercent(P->Health->Health/FMath::Max(1.f,P->Health->MaxHealth)); StaminaBar->SetPercent(P->Stamina->Current/FMath::Max(1.f,P->Stamina->Maximum));
    int32 Seconds=FMath::CeilToInt(S->Remaining);
    Status->SetText(FText::FromString(FString::Printf(TEXT("체력 %.0f / %.0f · 스태미나 %.0f / %.0f\n남은 시간 %02d:%02d\n목표: 가치 %d 확보 후 북동쪽 비콘에서 탈출\n확보 가치 %d · 무게 %.1f / %.1f kg"),P->Health->Health,P->Health->MaxHealth,P->Stamina->Current,P->Stamina->Maximum,Seconds/60,Seconds%60,S->RequiredValue,P->Inventory->GetValue(),P->Inventory->GetWeight(),P->Inventory->MaxWeight)));
    FString List=TEXT("보유 물자\n"); for(const auto& Slot:P->Inventory->Slots) if(IsValid(Slot.Item)) List+=FString::Printf(TEXT("%s × %d\n"),*Slot.Item->DisplayName.ToString(),Slot.Quantity); Items->SetText(FText::FromString(List));
    FString Hint=P->Prompt.ToString()+TEXT("\n")+P->Notice.ToString();
    if(S->ExtractionRemaining>0) Hint+=FString::Printf(TEXT("\n탈출까지 %.1f초 — 구역을 유지하세요"),S->ExtractionRemaining);
    const double Now=GetWorld()->GetTimeSeconds(); if(Now<HitUntil) Hint+=TEXT("\n피격!"); else if(Now<SwingUntil) Hint+=TEXT("\n근접 공격!");
    Prompt->SetColorAndOpacity(FSlateColor(Now<HitUntil?FLinearColor::Red:FLinearColor::White)); Prompt->SetText(FText::FromString(Hint));
    if(!S->IsRunning()){
        FString Result=S->Result==ERunResult::Escaped?TEXT("탈출 성공"):S->Result==ERunResult::Dead?TEXT("수색 실패 — 사망"):TEXT("수색 실패 — 시간 초과");
        Result+=FString::Printf(TEXT("\n물자 %d개 · 가치 %d"),S->FinalCount,S->FinalValue); ResultText->SetText(FText::FromString(Result)); RestartButton->SetVisibility(ESlateVisibility::Visible);
        if(!bResultShown){bResultShown=true; if(auto* PC=GetOwningPlayer()){PC->bShowMouseCursor=true; PC->SetInputMode(FInputModeGameAndUI());}}
    }
}
void UDusklineHUD::Restart(){if(auto* PC=Cast<ADusklinePlayerController>(GetOwningPlayer())) PC->RestartRun();}
