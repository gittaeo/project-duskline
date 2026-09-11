#if WITH_DEV_AUTOMATION_TESTS
#include "Misc/AutomationTest.h"
#include "Components/HealthComponent.h"
#include "Components/StaminaComponent.h"
#include "Components/InventoryComponent.h"
IMPLEMENT_SIMPLE_AUTOMATION_TEST(FDuskHealthTest,"Duskline.Components.Health",EAutomationTestFlags::EditorContext|EAutomationTestFlags::EngineFilter)
bool FDuskHealthTest::RunTest(const FString& Parameters){
    auto* H=NewObject<UHealthComponent>();
    TestEqual(TEXT("damage amount"),H->Damage(25),25.f); TestEqual(TEXT("health"),H->Health,75.f);
    TestEqual(TEXT("heal capped"),H->Heal(100),25.f); TestEqual(TEXT("full health"),H->Health,100.f);
    TestEqual(TEXT("lethal capped"),H->Damage(150),100.f); TestFalse(TEXT("dead"),H->IsAlive());
    TestEqual(TEXT("no repeated lethal processing"),H->Damage(10),0.f); TestEqual(TEXT("cannot resurrect with medicine"),H->Heal(100),0.f); return true;
}
IMPLEMENT_SIMPLE_AUTOMATION_TEST(FDuskStaminaTest,"Duskline.Components.Stamina",EAutomationTestFlags::EditorContext|EAutomationTestFlags::EngineFilter)
bool FDuskStaminaTest::RunTest(const FString& Parameters){
    auto* S=NewObject<UStaminaComponent>(); TestTrue(TEXT("spend full"),S->Spend(100)); TestFalse(TEXT("attack rejected"),S->Spend(20)); TestFalse(TEXT("sprint rejected"),S->Spend(2.4f));
    S->Advance(1); TestEqual(TEXT("recovery delayed"),S->Current,0.f); S->Advance(0.5f); TestEqual(TEXT("exact delay boundary"),S->Current,0.f);
    S->Advance(1); TestEqual(TEXT("recovery rate"),S->Current,18.f); S->Advance(100); TestEqual(TEXT("recovery cap"),S->Current,100.f); TestFalse(TEXT("negative spend"),S->Spend(-1)); return true;
}
IMPLEMENT_SIMPLE_AUTOMATION_TEST(FDuskInventoryTest,"Duskline.Components.Inventory",EAutomationTestFlags::EditorContext|EAutomationTestFlags::EngineFilter)
bool FDuskInventoryTest::RunTest(const FString& Parameters){
    auto* I=NewObject<UInventoryComponent>(); I->MaxSlots=2; I->MaxWeight=100;
    auto* A=NewObject<UItemDataAsset>(); A->ID=TEXT("Food"); A->DisplayName=FText::FromString(TEXT("식량")); A->MaxStack=3; A->Weight=2; A->Value=10;
    auto R=I->Add(A,7); TestFalse(TEXT("partial result"),R.bSuccess); TestEqual(TEXT("six processed"),R.Processed,6); TestEqual(TEXT("two slots"),I->Slots.Num(),2);
    for(const auto& Slot:I->Slots) TestTrue(TEXT("stack limit"),Slot.Quantity<=3);
    TestEqual(TEXT("weight"),I->GetWeight(),12.f); TestEqual(TEXT("value"),I->GetValue(),60);
    TestEqual(TEXT("partial remove"),I->Remove(A,9).Processed,6); TestEqual(TEXT("empty slots removed"),I->Slots.Num(),0);
    I->MaxWeight=3; TestEqual(TEXT("weight partial"),I->Add(A,2).Processed,1); TestEqual(TEXT("overweight refused"),I->Add(A,1).Processed,0);
    TestEqual(TEXT("null data refused"),I->Add(nullptr,1).Processed,0); return true;
}
IMPLEMENT_SIMPLE_AUTOMATION_TEST(FDuskMedicineTest,"Duskline.Components.Medicine",EAutomationTestFlags::EditorContext|EAutomationTestFlags::EngineFilter)
bool FDuskMedicineTest::RunTest(const FString& Parameters){
    auto* I=NewObject<UInventoryComponent>(); auto* H=NewObject<UHealthComponent>(); auto* M=NewObject<UItemDataAsset>();
    M->ID=TEXT("Medicine"); M->DisplayName=FText::FromString(TEXT("의약품")); M->Kind=EItemKind::Medicine; M->Healing=30; I->Add(M,1);
    TestFalse(TEXT("full health no consumption"),I->UseMedicine(H).bSuccess); TestEqual(TEXT("still one"),I->GetCount(),1);
    H->Damage(20); TestTrue(TEXT("medicine consumed"),I->UseMedicine(H).bSuccess); TestEqual(TEXT("heal cap"),H->Health,100.f); TestEqual(TEXT("slot removed"),I->Slots.Num(),0);
    H->Damage(10); TestFalse(TEXT("empty medicine"),I->UseMedicine(H).bSuccess); return true;
}
#endif
