using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
namespace Duskline.Editor {
public static class BuildTools {
    public const string ScenePath="Assets/Duskline/Scenes/Duskline.unity";
    [MenuItem("Duskline/Prepare Project")]
    public static void Prepare(){
        Directory.CreateDirectory("Assets/Duskline/Resources/Items");Directory.CreateDirectory("Assets/Duskline/Scenes");
        var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/Duskline/Resources/DuskMaterial.mat");
        if(!material){material=new Material(Shader.Find("Standard"));material.SetFloat("_Glossiness",.12f);AssetDatabase.CreateAsset(material,"Assets/Duskline/Resources/DuskMaterial.mat");}
        Item("food","비상 식량","밀봉된 생존 식량",ItemKind.Food,1,12,5,new Color(.9f,.7f,.3f));
        Item("medicine","응급 의약품","Q로 체력 30 회복",ItemKind.Medicine,.5f,20,4,new Color(.9f,.4f,.4f));
        Item("scrap","정제 고철","도시 복구를 위한 금속 부품",ItemKind.Scrap,2,16,4,new Color(.5f,.6f,.7f));
        Item("mission","통신 기록 모듈","해원시 구조 신호 복구 자료",ItemKind.Mission,1,65,2,new Color(.3f,.85f,.7f));
        if(!File.Exists(ScenePath)){
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            new GameObject("WorldBootstrap").AddComponent<WorldBootstrap>();EditorSceneManager.SaveScene(scene,ScenePath);
        }
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        PlayerSettings.companyName="Duskline";PlayerSettings.productName="Duskline Web";PlayerSettings.bundleVersion="0.1.0";
        PlayerSettings.defaultWebScreenWidth=1280;PlayerSettings.defaultWebScreenHeight=720;
        PlayerSettings.colorSpace=ColorSpace.Gamma;PlayerSettings.runInBackground=false;
        PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.template="PROJECT:Duskline";
        PlayerSettings.WebGL.exceptionSupport=WebGLExceptionSupport.FullWithStacktrace;
        PlayerSettings.SetManagedStrippingLevel(UnityEditor.Build.NamedBuildTarget.WebGL,ManagedStrippingLevel.Low);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL,new[]{GraphicsDeviceType.OpenGLES3});
        var settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
        var input=settings.FindProperty("activeInputHandler");if(input!=null){input.intValue=0;settings.ApplyModifiedPropertiesWithoutUndo();}
        QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowDistance=45;QualitySettings.shadowResolution=ShadowResolution.Medium;
        AssetDatabase.SaveAssets();AssetDatabase.Refresh();Debug.Log("DUSKLINE_PREPARE_OK");
    }
    static void Item(string id,string name,string description,ItemKind kind,float weight,int value,int stack,Color color){
        string path="Assets/Duskline/Resources/Items/"+id+".asset";if(AssetDatabase.LoadAssetAtPath<ItemDefinition>(path))return;
        var item=ScriptableObject.CreateInstance<ItemDefinition>();item.id=id;item.displayName=name;item.description=description;item.kind=kind;item.weight=weight;item.value=value;item.maxStack=stack;item.iconColor=color;AssetDatabase.CreateAsset(item,path);
    }
    [MenuItem("Duskline/Build Web")]
    public static void BuildWeb(){
        Prepare();RunChecks();
        string output=Environment.GetEnvironmentVariable("DUSKLINE_WEB_OUTPUT");if(string.IsNullOrWhiteSpace(output))output="Builds/Web";
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName=output,target=BuildTarget.WebGL,options=BuildOptions.None});
        Directory.CreateDirectory("Validation");File.WriteAllText("Validation/WebBuild.txt",$"Result: {report.summary.result}\nErrors: {report.summary.totalErrors}\nWarnings: {report.summary.totalWarnings}\nBytes: {report.summary.totalSize}\nDuration: {report.summary.totalTime}\nUnity: {Application.unityVersion}\n");
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Duskline Web build failed");
        Debug.Log("DUSKLINE_WEB_BUILD_OK");
    }
    public static void BuildWebSmoke(){
        Prepare();RunChecks();
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName="Builds/Smoke",target=BuildTarget.WebGL,options=BuildOptions.None,extraScriptingDefines=new[]{"DUSKLINE_SMOKE_TEST"}});
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Web smoke build failed");
        Debug.Log("DUSKLINE_WEB_SMOKE_BUILD_OK");
    }
    [MenuItem("Duskline/Run Core Checks")]
    public static void RunChecks(){
        int count=0;void Check(bool pass,string name){count++;if(!pass)throw new Exception("Duskline test failed: "+name);}
        var go=new GameObject("Test components");var item=ScriptableObject.CreateInstance<ItemDefinition>();
        try{
            var h=go.AddComponent<Health>();int deaths=0;h.Died+=()=>deaths++;
            Check(h.Damage(25)==25&&h.Current==75,"health damage");Check(h.Heal(100)==25&&h.Current==100,"heal capped");
            Check(h.Damage(200)==100&&!h.Alive,"lethal damage");Check(h.Damage(10)==0&&deaths==1,"death once");Check(h.Heal(100)==0,"no resurrection");h.ResetState();
            var s=go.AddComponent<Stamina>();Check(s.Spend(100),"full stamina spend");Check(!s.Spend(20),"attack exhausted");Check(!s.Spend(2.4f),"sprint exhausted");s.Step(1);Check(s.Current==0,"recovery delay");s.Step(.5f);Check(s.Current==0,"exact delay");s.Step(1);Check(s.Current==18,"recovery rate");s.Step(100);Check(s.Current==100,"recovery clamp");Check(!s.Spend(-1),"negative spend");
            item.id="test";item.displayName="검사 물자";item.weight=2;item.maxStack=3;item.value=10;
            var inv=go.AddComponent<Inventory>();inv.maximumSlots=2;inv.maximumWeight=100;var r=inv.Add(item,7);
            Check(!r.Success&&r.Processed==6,"partial slot transfer");Check(inv.Slots.Count==2,"slot cap");Check(inv.Slots[0].count==3&&inv.Slots[1].count==3,"stack cap");Check(inv.Weight==12&&inv.Value==60,"totals");Check(inv.Remove(item,10).Processed==6&&inv.Slots.Count==0,"remove partial and erase");
            inv.maximumWeight=3;Check(inv.Add(item,2).Processed==1,"weight partial");Check(inv.Add(item,1).Processed==0,"overweight reject");inv.Remove(item,10);Check(inv.Add(null,1).Processed==0,"null item");item.weight=0;Check(inv.Add(item,1).Processed==0,"invalid item");item.weight=1;item.kind=ItemKind.Medicine;inv.Add(item,1);
            Check(!inv.UseMedicine(h).Success&&inv.Count==1,"full health medicine");h.Damage(20);Check(inv.UseMedicine(h).Success&&h.Current==100&&inv.Count==0,"medicine consume and cap");Check(!inv.UseMedicine(h).Success,"empty medicine");
            Check(RunRules.Resolve(RunResult.Running,RunResult.Escaped,true,600,600)==RunResult.TimedOut,"deadline tie");Check(RunRules.Resolve(RunResult.Running,RunResult.Escaped,false,599,600)==RunResult.Dead,"death priority");Check(RunRules.Resolve(RunResult.Escaped,RunResult.Dead,false,700,600)==RunResult.Escaped,"terminal idempotence");
            Check(RunRules.CanExtract(true,true,true,180,180,599,600),"value threshold");Check(!RunRules.CanExtract(true,true,true,179,180,599,600),"insufficient value");Check(!RunRules.CanExtract(true,true,false,180,180,599,600),"outside zone");Check(!RunRules.CanExtract(true,false,true,180,180,599,600),"dead player");Check(!RunRules.CanExtract(true,true,true,180,180,600,600),"deadline refusal");
            Directory.CreateDirectory("Validation");File.WriteAllText("Validation/CoreChecks.json",$"{{\"passed\":{count},\"failed\":0,\"unity\":\"{Application.unityVersion}\"}}");Debug.Log($"DUSKLINE_CORE_CHECKS_OK: {count}");
        }finally{UnityEngine.Object.DestroyImmediate(go);UnityEngine.Object.DestroyImmediate(item);}
    }
}
}
