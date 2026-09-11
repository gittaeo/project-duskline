using UnityEngine;
namespace Duskline {
public sealed partial class WorldBootstrap {
    static readonly Color cloth=new Color(.10f,.12f,.13f), leather=new Color(.22f,.20f,.15f), metal=new Color(.14f,.16f,.17f);
    Transform Shape(string name,Transform parent,Vector3 p,Vector3 scale,Color color,PrimitiveType type=PrimitiveType.Cube){
        var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=p;go.transform.localScale=scale;
        // Explicit references preserve primitive collider bindings in stripped Web players.
        Collider col=type==PrimitiveType.Sphere?go.GetComponent<SphereCollider>():type==PrimitiveType.Cube?go.GetComponent<BoxCollider>():go.GetComponent<CapsuleCollider>();if(col)Destroy(col);
        go.GetComponent<Renderer>().sharedMaterial=SurfaceArt.Flat(baseMaterial,color);return go.transform;
    }
    Transform DetailedBody(Transform parent,bool infected){
        var root=new GameObject("Survivor articulated body").transform;root.SetParent(parent,false);
        Color suit=infected?new Color(.25f,.27f,.23f):cloth;
        Shape("Jacket",root,new Vector3(0,1.18f,0),new Vector3(.52f,.38f,.32f),suit,PrimitiveType.Capsule);
        Shape("Head",root,new Vector3(0,1.72f,.01f),new Vector3(.27f,.34f,.28f),new Color(.43f,.39f,.32f),PrimitiveType.Sphere);
        Shape("Face mask",root,new Vector3(0,1.65f,.12f),new Vector3(.25f,.13f,.11f),cloth,PrimitiveType.Sphere);
        Shape("Cap",root,new Vector3(0,1.87f,0),new Vector3(.31f,.14f,.31f),cloth,PrimitiveType.Sphere);
        Shape("Cap brim",root,new Vector3(0,1.84f,.16f),new Vector3(.28f,.025f,.22f),cloth);
        for(int side=-1;side<=1;side+=2){
            Shape("Trousers",root,new Vector3(side*.15f,.52f,0),new Vector3(.23f,.29f,.24f),suit,PrimitiveType.Capsule);
            Shape("Boot",root,new Vector3(side*.15f,.13f,.055f),new Vector3(.24f,.25f,.36f),metal,PrimitiveType.Capsule);
            Shape("Knee pad",root,new Vector3(side*.15f,.48f,.13f),new Vector3(.19f,.23f,.09f),metal,PrimitiveType.Sphere);
            Shape("Vest strap",root,new Vector3(side*.19f,1.25f,.165f),new Vector3(.065f,.57f,.055f),leather);
            Shape("Utility pouch",root,new Vector3(side*.15f,1.05f,.22f),new Vector3(.20f,.21f,.12f),leather);
        }
        Shape("Left sleeve",root,new Vector3(-.34f,1.12f,0),new Vector3(.20f,.27f,.22f),suit,PrimitiveType.Capsule);
        Shape("Left glove",root,new Vector3(-.34f,.81f,.03f),new Vector3(.18f,.2f,.18f),metal,PrimitiveType.Sphere);
        if(infected)Shape("Right sleeve",root,new Vector3(.34f,1.12f,0),new Vector3(.20f,.27f,.22f),suit,PrimitiveType.Capsule);
        else {
            Shape("Backpack",root,new Vector3(0,1.22f,-.26f),new Vector3(.49f,.65f,.29f),leather,PrimitiveType.Capsule);
            Shape("Pack flap",root,new Vector3(0,1.38f,-.42f),new Vector3(.43f,.24f,.07f),leather);
            for(int i=0;i<3;i++)Shape("Pack webbing",root,new Vector3(0,1.02f+i*.13f,-.425f),new Vector3(.39f,.025f,.025f),metal);
            for(int side=-1;side<=1;side+=2)Shape("Pack buckle",root,new Vector3(side*.13f,1.16f,-.46f),new Vector3(.045f,.18f,.035f),cloth);
        }
        return root;
    }
    Transform OverheadClub(Transform parent){
        var pivot=new GameObject("Overhead shoulder pivot").transform;pivot.SetParent(parent,false);pivot.localPosition=new Vector3(.34f,1.43f,0);
        Shape("Right sleeve",pivot,new Vector3(0,-.22f,0),new Vector3(.2f,.22f,.22f),cloth,PrimitiveType.Capsule);
        Shape("Right glove",pivot,new Vector3(0,-.46f,.035f),new Vector3(.18f,.18f,.18f),metal,PrimitiveType.Sphere);
        var club=Shape("Wooden club",pivot,new Vector3(0,-.43f,.55f),new Vector3(.085f,.58f,.085f),new Color(.28f,.17f,.08f),PrimitiveType.Cylinder);club.localRotation=Quaternion.Euler(90,0,0);
        var grip=Shape("Cloth grip",pivot,new Vector3(0,-.43f,.12f),new Vector3(.092f,.13f,.092f),cloth,PrimitiveType.Cylinder);grip.localRotation=Quaternion.Euler(90,0,0);
        return pivot;
    }
    void Sign(string words,Vector3 p,float yaw,float size=1){
        var go=new GameObject("Haewon sign "+words);go.transform.position=p;go.transform.rotation=Quaternion.Euler(0,yaw,0);
        var text=go.AddComponent<TextMesh>();text.text=words;text.font=Resources.Load<Font>("DuskFont");text.fontSize=64;text.characterSize=.085f*size;text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.color=new Color(.78f,.82f,.75f);
        if(text.font){var material=new Material(Resources.Load<Shader>("WorldText"));material.mainTexture=text.font.material.mainTexture;go.GetComponent<MeshRenderer>().sharedMaterial=material;go.AddComponent<WorldLabelAtlas>().font=text.font;}
    }
    void BuildHaewonArt(){
        RenderSettings.ambientSkyColor=new Color(.22f,.28f,.34f);RenderSettings.ambientEquatorColor=new Color(.18f,.21f,.23f);RenderSettings.ambientGroundColor=new Color(.10f,.12f,.12f);
        RenderSettings.fogColor=new Color(.12f,.17f,.20f);RenderSettings.fogDensity=.009f;
        foreach(var light in FindObjectsByType<Light>(FindObjectsSortMode.None))if(light.type==LightType.Directional){light.intensity=.65f;light.color=new Color(.63f,.73f,.84f);}
        for(int i=0;i<6;i++){
            float x=i%2==0?-46:46,z=-28+(i/2)*28;
            var lamp=new GameObject("Warm street pool").AddComponent<Light>();lamp.type=LightType.Point;lamp.transform.position=new Vector3(x,4,z);lamp.color=new Color(1,.65f,.29f);lamp.intensity=3;lamp.range=13;lamp.shadows=LightShadows.None;
        }
        Bus(new Vector3(-46,0,-9));Bus(new Vector3(46,0,12));
        for(int i=0;i<9;i++){
            float x=i%2==0?-48:48,z=-30+i*7;
            var root=new GameObject("Roadside tree").transform;root.position=new Vector3(x,0,z);
            Shape("Trunk",root,new Vector3(0,2.2f,0),new Vector3(.20f,2.2f,.20f),leather,PrimitiveType.Cylinder);
            for(int j=0;j<5;j++){
                float a=j*2.4f;Shape("Canopy",root,new Vector3(Mathf.Cos(a)*.85f,4.6f+j*.22f,Mathf.Sin(a)*.85f),new Vector3(2.1f,1.6f,1.8f),new Color(.13f+j*.01f,.18f,.105f),PrimitiveType.Sphere);
            }
        }
        // Props stay beside the original navigation corridors.
        for(int i=0;i<12;i++){
            Vector3 p=new Vector3(-22.5f,0,-13+i*2.2f);
            Block("Rubble",p+Vector3.up*.12f,new Vector3(.6f,.24f,.45f),metal,false);
            for(int j=0;j<3;j++){var blade=Block("Roadside weeds",p+new Vector3(j*.18f,.26f,.3f),new Vector3(.045f,.5f,.035f),new Color(.23f,.27f,.12f),false);blade.transform.rotation=Quaternion.Euler(12*j,20*j,15-j*15);}
        }
        Block("Safe zone arch",new Vector3(42,4,36.5f),new Vector3(10,.45f,.35f),metal,false);
        for(int side=-1;side<=1;side+=2)Block("Safe zone gate post",new Vector3(42+side*4.7f,2,36.5f),new Vector3(.35f,4,.35f),metal,true);
        Sign("안전구역\nHAEWON SAFE ZONE",new Vector3(42,3.25f,36.2f),0,1.4f);
        Sign("해원시\nHAEWON CITY",new Vector3(-15,7,-15.05f),0,2);
        Sign("해원약국  +",new Vector3(3,3,-12.08f),0,1.1f);
        Sign("격리구역 · 출입통제",new Vector3(-35,2,-25.38f),0,.9f);
    }
    void Bus(Vector3 p){
        Block("Abandoned bus",p+Vector3.up*1.4f,new Vector3(2.6f,2.5f,7),new Color(.12f,.21f,.27f),true);
        Block("Bus roof",p+Vector3.up*2.72f,new Vector3(2.65f,.15f,7),metal,false);
        for(int i=0;i<5;i++)for(int side=-1;side<=1;side+=2)Block("Window",p+new Vector3(side*1.31f,2,-2.6f+i*1.25f),new Vector3(.035f,.85f,.95f),metal,false);
        for(int side=-1;side<=1;side+=2)for(int z=-1;z<=1;z+=2){var wheel=Shape("Bus tyre",null,p+new Vector3(side*1.32f,.48f,z*2.3f),new Vector3(.8f,.18f,.8f),cloth,PrimitiveType.Cylinder);wheel.rotation=Quaternion.Euler(0,0,90);}
    }
}
// Dynamic font atlases may be resized as the Korean HUD requests more glyphs.
sealed class WorldLabelAtlas : MonoBehaviour {
    public Font font;
    void LateUpdate(){if(font)GetComponent<Renderer>().sharedMaterial.mainTexture=font.material.mainTexture;}
    void OnDestroy(){var r=GetComponent<Renderer>();if(r&&r.sharedMaterial)Destroy(r.sharedMaterial);}
}
}
