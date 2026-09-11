using UnrealBuildTool;
public class ProjectDusklineTarget : TargetRules {
    public ProjectDusklineTarget(TargetInfo Target) : base(Target) {
        Type = TargetType.Game;
        DefaultBuildSettings = BuildSettingsVersion.V5;
        ExtraModuleNames.Add("ProjectDuskline");
    }
}
