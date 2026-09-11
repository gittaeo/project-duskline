using System.Collections.Generic;
using UnityEngine;
namespace Duskline {
public static class SurfaceArt {
    static readonly Dictionary<Color,Material> flat=new Dictionary<Color,Material>();
    static Material asphalt,concrete,glass;
    public static void Initialize(Material fallback){
        foreach(var material in flat.Values)if(material)Object.Destroy(material);flat.Clear();
        asphalt=Resources.Load<Material>("Art/Asphalt");concrete=Resources.Load<Material>("Art/Concrete");
        if(!asphalt||!concrete)Debug.LogWarning("PBR materials unavailable; using graybox fallback. Run Prepare Project.");
        if(glass)Object.Destroy(glass);glass=new Material(fallback){color=new Color(.13f,.26f,.32f)};glass.SetFloat("_Metallic",.55f);glass.SetFloat("_Glossiness",.72f);
    }
    public static Material Flat(Material source,Color color){
        if(!flat.TryGetValue(color,out var material)||!material){material=new Material(source){color=color};flat[color]=material;}return material;
    }
    public static void Apply(GameObject target,string label,bool solid,Material fallback,Color color){
        var renderer=target.GetComponent<Renderer>();Material material=null;float meters=3;
        if(label=="Ground"){material=asphalt;meters=4;}
        else if(solid||label=="Facade base"||label=="Window sill")material=concrete;
        else if(label=="Window")material=glass;
        renderer.sharedMaterial=material?material:Flat(fallback,color);
        if(material==asphalt||material==concrete){
            // Each cube face uses meter-scaled UVs so large walls do not stretch one photograph.
            var filter=target.GetComponent<MeshFilter>();var mesh=filter.mesh;var vertices=mesh.vertices;var normals=mesh.normals;var uv=new Vector2[vertices.Length];
            for(int i=0;i<uv.Length;i++){Vector3 p=Vector3.Scale(vertices[i],target.transform.lossyScale)/meters,n=normals[i];
                if(Mathf.Abs(n.y)>.5f)uv[i]=new Vector2(p.x,p.z*Mathf.Sign(n.y));
                else if(Mathf.Abs(n.x)>.5f)uv[i]=new Vector2(p.z*-Mathf.Sign(n.x),p.y);
                else uv[i]=new Vector2(p.x*Mathf.Sign(n.z),p.y);
            }
            mesh.uv=uv;mesh.RecalculateTangents();
        }
    }
}
}
