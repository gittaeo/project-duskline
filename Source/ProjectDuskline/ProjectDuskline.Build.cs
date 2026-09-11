using UnrealBuildTool;
public class ProjectDuskline : ModuleRules {
    public ProjectDuskline(ReadOnlyTargetRules Target) : base(Target) {
        PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;
        PublicDependencyModuleNames.AddRange(new string[] { "Core", "CoreUObject", "Engine", "InputCore", "EnhancedInput", "AIModule", "NavigationSystem", "GameplayTasks", "UMG", "Slate", "SlateCore" });
    }
}
