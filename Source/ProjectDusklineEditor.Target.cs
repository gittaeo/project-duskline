using UnrealBuildTool;
public class ProjectDusklineEditorTarget : TargetRules {
    public ProjectDusklineEditorTarget(TargetInfo Target) : base(Target) {
        Type = TargetType.Editor;
        DefaultBuildSettings = BuildSettingsVersion.V5;
        ExtraModuleNames.Add("ProjectDuskline");
    }
}
