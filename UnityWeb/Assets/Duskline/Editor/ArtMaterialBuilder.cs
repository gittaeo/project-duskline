using System.IO;
using UnityEditor;
using UnityEngine;
namespace Duskline.Editor {
public static class ArtMaterialBuilder {
    public static void Prepare(){
        Create("asphalt_02","Asphalt",.75f);
        Create("concrete_wall_003","Concrete",.6f);
    }
    static void Create(string asset,string name,float bump){
        string folder="Assets/Duskline/Resources/Art/"+asset+"/";
        string diffuse=folder+asset+"_diff_2k.jpg",normal=folder+asset+"_nor_gl_1k.jpg",rough=folder+asset+"_rough_1k.jpg";
        if(!File.Exists(diffuse)||!File.Exists(normal)||!File.Exists(rough))throw new FileNotFoundException("Required Poly Haven PBR maps missing: "+asset);
        Import(diffuse,false,false);Import(normal,true,false);Import(rough,false,true);
        var roughness=AssetDatabase.LoadAssetAtPath<Texture2D>(rough);var pixels=roughness.GetPixels32();
        for(int i=0;i<pixels.Length;i++)pixels[i]=new Color32(0,0,0,(byte)(255-pixels[i].r));
        var smooth=new Texture2D(roughness.width,roughness.height,TextureFormat.RGBA32,false,true);smooth.SetPixels32(pixels);smooth.Apply();
        string packed=folder+asset+"_metallic_smoothness.png";File.WriteAllBytes(packed,smooth.EncodeToPNG());Object.DestroyImmediate(smooth);AssetDatabase.ImportAsset(packed,ImportAssetOptions.ForceUpdate);Import(packed,false,true);
        string path="Assets/Duskline/Resources/Art/"+name+".mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(!material){material=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(material,path);}
        material.color=Color.white;material.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(diffuse);
        material.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(normal));material.SetFloat("_BumpScale",bump);material.EnableKeyword("_NORMALMAP");
        material.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(packed));material.EnableKeyword("_METALLICGLOSSMAP");material.SetFloat("_GlossMapScale",.65f);material.SetFloat("_Metallic",0);material.enableInstancing=true;
        EditorUtility.SetDirty(material);
    }
    static void Import(string path,bool normal,bool linear){
        AssetDatabase.ImportAsset(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;
        importer.sRGBTexture=!normal&&!linear;importer.isReadable=linear;importer.mipmapEnabled=true;importer.wrapMode=TextureWrapMode.Repeat;importer.anisoLevel=4;
        importer.maxTextureSize=path.Contains("2k")?2048:1024;importer.textureCompression=TextureImporterCompression.Compressed;importer.SaveAndReimport();
    }
}
}
