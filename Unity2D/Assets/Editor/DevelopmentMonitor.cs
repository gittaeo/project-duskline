using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public sealed class DevelopmentMonitor : EditorWindow {
    static DevelopmentMonitor(){EditorApplication.delayCall+=()=>{
        if(Application.isBatchMode||SessionState.GetBool("DusklineMonitorOpened",false))return;
        SessionState.SetBool("DusklineMonitorOpened",true);
        if(string.IsNullOrEmpty(EditorSceneManager.GetActiveScene().path))EditorSceneManager.OpenScene("Assets/Scenes/Slice.unity");
        Open();
    };}
    [MenuItem("Duskline/Development Monitor")]
    public static void Open(){GetWindow<DevelopmentMonitor>("Duskline Monitor").Show();}
    void OnGUI(){
        GUILayout.Label("DUSKLINE / LIVE DEVELOPMENT",EditorStyles.boldLabel);
        GUILayout.Label("Unity 6.2 / C# / Windows standalone");
        GUILayout.Label("Project: D:/Unity/Duskline2D-Unity",EditorStyles.wordWrappedLabel);
        GUILayout.Space(12);
        GUILayout.Label(EditorApplication.isCompiling?"Compiling C#...":EditorApplication.isPlaying?"PLAYING — Game tab shows live gameplay":"EDITING — press Play to inspect gameplay",EditorStyles.wordWrappedLabel);
        if(GUILayout.Button(EditorApplication.isPlaying?"Stop play":"Play current scene"))EditorApplication.isPlaying=!EditorApplication.isPlaying;
        if(GUILayout.Button("Show Game view"))EditorApplication.ExecuteMenuItem("Window/General/Game");
        if(GUILayout.Button("Show Console"))EditorApplication.ExecuteMenuItem("Window/General/Console");
        if(GUILayout.Button("Inspect gameplay logic"))Selection.activeObject=AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Scripts/SliceModel.cs");
        if(GUILayout.Button("Inspect input / sprites / effects"))Selection.activeObject=AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Scripts/SliceGame.cs");
        GUILayout.Space(12);
        GUILayout.Label("Latest completed build",EditorStyles.boldLabel);
        string path="D:/Unity/Duskline2D-Expansion-v10/Validation/build.txt";
        if(File.Exists(path))GUILayout.Label(File.GetLastWriteTime(path)+"\n"+File.ReadAllText(path));
        GUILayout.Label("Scope: chapters 1-3, tag combat, three shelter floors, clinic, purification and farming.\nEnergy, free furniture placement and later chapters are future work.",EditorStyles.wordWrappedLabel);
    }
    void OnInspectorUpdate(){Repaint();}
}
