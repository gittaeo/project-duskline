using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
namespace Duskline {
[DefaultExecutionOrder(-100)]
public sealed class WorldBootstrap : MonoBehaviour {
    readonly List<NavMeshBuildSource> sources=new List<NavMeshBuildSource>();
    readonly List<LootContainer> crates=new List<LootContainer>();
    readonly List<EnemyBrain> enemies=new List<EnemyBrain>();
    NavMeshDataInstance navigation;
    Material baseMaterial;
    public IReadOnlyList<LootContainer> Crates=>crates;
    public IReadOnlyList<EnemyBrain> Enemies=>enemies;
    static readonly Color concrete=new Color(.25f,.29f,.31f),dark=new Color(.1f,.15f,.18f),gold=new Color(.88f,.62f,.3f),teal=new Color(.16f,.83f,.72f);
    public static WorldBootstrap Instance {get;private set;}
    void Awake(){
        Instance=this;Application.targetFrameRate=60;QualitySettings.vSyncCount=0;
        baseMaterial=Resources.Load<Material>("DuskMaterial");
        if(!baseMaterial){Debug.LogError("DuskMaterial missing. Run Duskline > Prepare Project before playing.");enabled=false;return;}
        RenderSettings.ambientLight=new Color(.42f,.48f,.53f);RenderSettings.fog=true;RenderSettings.fogColor=new Color(.18f,.25f,.3f);RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.008f;
        var sun=new GameObject("Late afternoon light").AddComponent<Light>();sun.type=LightType.Directional;sun.transform.rotation=Quaternion.Euler(43,-35,0);sun.color=new Color(1,.83f,.61f);sun.intensity=1.3f;sun.shadows=LightShadows.Soft;
        Block("Ground",new Vector3(0,-.25f,0),new Vector3(100,.5f,80),new Color(.16f,.2f,.22f),true);
        Block("North wall",new Vector3(0,2.5f,40),new Vector3(100,5,.5f),dark,true);
        Block("South wall",new Vector3(0,2.5f,-40),new Vector3(100,5,.5f),dark,true);
        Block("East wall",new Vector3(50,2.5f,0),new Vector3(.5f,5,80),dark,true);
        Block("West wall",new Vector3(-50,2.5f,0),new Vector3(.5f,5,80),dark,true);
        Building(new Vector3(-15,0,0),new Vector3(14,10,30));Building(new Vector3(3,0,0),new Vector3(14,8,24));Building(new Vector3(19,0,22),new Vector3(12,12,18));
        Block("Room back",new Vector3(-35,2.2f,23),new Vector3(17,4.4f,.4f),concrete,true);
        Block("Room left",new Vector3(-43.5f,2.2f,16),new Vector3(.4f,4.4f,14),concrete,true);
        Block("Room right",new Vector3(-26.5f,2.2f,16),new Vector3(.4f,4.4f,14),concrete,true);
        Block("Door left",new Vector3(-40.75f,2.2f,9),new Vector3(5.5f,4.4f,.4f),concrete,true);
        Block("Door right",new Vector3(-29.25f,2.2f,9),new Vector3(5.5f,4.4f,.4f),concrete,true);
        Block("Room roof",new Vector3(-35,4.7f,16),new Vector3(17.8f,.6f,14.8f),dark,true);
        Block("Safe cover",new Vector3(-35,1.3f,-25),new Vector3(8,2.6f,.7f),concrete,true);
        Block("East cover",new Vector3(34,1,-5),new Vector3(5,2,1.5f),concrete,true);
        for(int z=-35;z<36;z+=5){Block("Lane marking",new Vector3(-6,.012f,z),new Vector3(.12f,.015f,2),gold,false);Block("East lane marking",new Vector3(34,.012f,z),new Vector3(.12f,.015f,2),gold,false);}
        for(int i=0;i<12;i++){float x=-46+(i%4)*28,z=-34+(i/4)*31;Block("Street pole",new Vector3(x,2.4f,z),new Vector3(.14f,4.8f,.14f),dark,false);Block("Street lamp",new Vector3(x,4.8f,z),new Vector3(.55f,.15f,.35f),gold,false);}
        BakeNavigation();
        var run=new GameObject("RunDirector").AddComponent<RunDirector>();
        var player=new GameObject("Player");player.layer=8;player.transform.position=new Vector3(-41,.1f,-32);
        var motor=player.AddComponent<CharacterController>();motor.height=1.8f;motor.radius=.32f;motor.center=Vector3.up*.9f;motor.stepOffset=.3f;
        player.AddComponent<Health>();player.AddComponent<Stamina>();player.AddComponent<Inventory>();player.AddComponent<NoiseEmitter>();
        var fight=player.AddComponent<MeleeCombat>();fight.playerSide=true;
        var p=player.AddComponent<PlayerMotor>();run.player=p;p.body=MakeBody(player.transform,new Color(.24f,.48f,.56f),false);fight.weapon=Weapon(p.body);
        var cameraObject=new GameObject("Main Camera");cameraObject.tag="MainCamera";var camera=cameraObject.AddComponent<Camera>();camera.fieldOfView=64;camera.farClipPlane=180;camera.backgroundColor=RenderSettings.fogColor;camera.clearFlags=CameraClearFlags.SolidColor;
        cameraObject.AddComponent<AudioListener>();p.view=cameraObject.AddComponent<OrbitCamera>();p.view.target=p.transform;
        var defs=Resources.LoadAll<ItemDefinition>("Items");var byId=new Dictionary<string,ItemDefinition>();foreach(var d in defs)byId[d.id]=d;
        if(byId.Count<4){Debug.LogError("Item definitions missing. Run Prepare Project.");return;}
        Vector3[] places={new Vector3(-41,0,-22),new Vector3(-35,0,16),new Vector3(-6.5f,0,-9),new Vector3(15,0,-21),new Vector3(38,0,-6),new Vector3(12,0,33),new Vector3(35,0,22)};
        string[] ids={"food","medicine","scrap","mission"};
        for(int i=0;i<places.Length;i++){
            var go=new GameObject("Supply "+(i+1));go.transform.position=places[i];go.layer=9;
            var box=Block("Crate",places[i]+Vector3.up*.45f,new Vector3(1.25f,.9f,.85f),new Color(.27f,.36f,.3f),false,go.transform,true);box.layer=9;
            var c=go.AddComponent<LootContainer>();c.seed=1337+i;c.forced=i==1||i==3||i==5;
            c.table=new[]{new LootRow{item=byId[ids[i%4]],minimum=1,maximum=2},new LootRow{item=byId[i%2==0?"medicine":"food"],minimum=1,maximum=2}};
            c.lid=Block("Lid",places[i]+Vector3.up*.94f,new Vector3(1.3f,.1f,.9f),gold,false,go.transform).transform;
            c.indicator=Block("Supply strip",places[i]+new Vector3(0,.6f,-.435f),new Vector3(.8f,.1f,.03f),teal,false,go.transform).GetComponent<Renderer>();crates.Add(c);
        }
        var beacon=new GameObject("Extraction");beacon.transform.position=new Vector3(42,0,32);beacon.layer=9;
        var pillar=Block("Rescue terminal",beacon.transform.position+Vector3.up, new Vector3(.7f,2,.7f),teal,false,beacon.transform,true);pillar.layer=9;
        run.exit=beacon.AddComponent<ExtractionPoint>();Ring(beacon.transform.position+Vector3.up*.03f,3.5f,teal,beacon.transform);
        Block("Beacon beam",beacon.transform.position+Vector3.up*7,new Vector3(.15f,12,.15f),teal,false,beacon.transform);
        SpawnEnemy(new[]{new Vector3(-6,0,-13),new Vector3(-6,0,14),new Vector3(-24,0,21)});
        SpawnEnemy(new[]{new Vector3(17,0,-20),new Vector3(38,0,-18),new Vector3(38,0,5)});
        SpawnEnemy(new[]{new Vector3(33,0,20),new Vector3(38,0,31),new Vector3(14,0,32)});
        gameObject.AddComponent<GameHUD>();
    }
    void OnDestroy(){if(navigation.valid)navigation.Remove();if(Instance==this)Instance=null;}
    GameObject Block(string name,Vector3 position,Vector3 scale,Color color,bool navigable,Transform parent=null,bool collision=false){
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.position=position;go.transform.localScale=scale;if(parent)go.transform.SetParent(parent,true);
        var mat=new Material(baseMaterial);mat.color=color;go.GetComponent<Renderer>().sharedMaterial=mat;
        // Explicit type reference keeps primitive collision bindings in stripped Web players.
        var collider=go.GetComponent<BoxCollider>();if(!collider)collider=go.AddComponent<BoxCollider>();
        if(!navigable&&!collision)Destroy(collider);
        if(navigable)sources.Add(new NavMeshBuildSource{shape=NavMeshBuildSourceShape.Box,transform=go.transform.localToWorldMatrix,size=Vector3.one,area=0});
        return go;
    }
    void Building(Vector3 center,Vector3 size){
        Block("Closed building",center+Vector3.up*size.y*.5f,size,concrete,true);
        Block("Roof rim",center+Vector3.up*(size.y+.1f),new Vector3(size.x+.3f,.25f,size.z+.3f),dark,false);
        for(float y=2;y<size.y;y+=2.5f)for(float z=-size.z*.5f+2;z<size.z*.5f;z+=3){
            Block("Window",center+new Vector3(size.x*.5f+.015f,y,z),new Vector3(.03f,1.1f,1.2f),new Color(.12f,.22f,.27f),false);
            Block("Window",center+new Vector3(-size.x*.5f-.015f,y,z),new Vector3(.03f,1.1f,1.2f),new Color(.14f,.26f,.29f),false);
        }
    }
    Transform MakeBody(Transform parent,Color jacket,bool infected){
        var root=new GameObject("Visual body").transform;root.SetParent(parent,false);
        Block("Torso",parent.position+Vector3.up*1.13f,new Vector3(.65f,.7f,.38f),jacket,false,root);
        Block("Head",parent.position+Vector3.up*1.7f,new Vector3(.38f,.4f,.38f),infected?new Color(.58f,.46f,.41f):new Color(.65f,.67f,.63f),false,root);
        Block("Visor",parent.position+new Vector3(0,1.75f,.2f),new Vector3(.31f,.09f,.04f),infected?new Color(1,.25f,.18f):gold,false,root);
        Block("Left leg",parent.position+new Vector3(-.18f,.4f,0),new Vector3(.22f,.7f,.25f),dark,false,root);
        Block("Right leg",parent.position+new Vector3(.18f,.4f,0),new Vector3(.22f,.7f,.25f),dark,false,root);
        Block("Arm",parent.position+new Vector3(-.45f,1.06f,0),new Vector3(.2f,.65f,.2f),jacket,false,root);
        Block("Arm",parent.position+new Vector3(.45f,1.06f,0),new Vector3(.2f,.65f,.2f),jacket,false,root);
        if(!infected)Block("Pack",parent.position+new Vector3(0,1.16f,-.3f),new Vector3(.45f,.55f,.25f),gold,false,root);
        return root;
    }
    Transform Weapon(Transform parent){var pivot=new GameObject("Melee pivot").transform;pivot.SetParent(parent,false);pivot.localPosition=new Vector3(.45f,1,0);Block("Steel baton",pivot.position+new Vector3(0,.2f,.4f),new Vector3(.1f,.1f,.8f),gold,false,pivot);return pivot;}
    void SpawnEnemy(Vector3[] route){
        var go=new GameObject("Infected "+enemies.Count);go.layer=10;
        go.transform.position=NavMesh.SamplePosition(route[0],out var hit,3,NavMesh.AllAreas)?hit.position:route[0];
        var capsule=go.AddComponent<CapsuleCollider>();capsule.radius=.35f;capsule.height=1.8f;capsule.center=Vector3.up*.9f;
        var h=go.AddComponent<Health>();h.maximum=90;h.ResetState();var combat=go.AddComponent<MeleeCombat>();combat.damage=14;combat.reach=1.25f;combat.radius=.5f;combat.cooldown=1.25f;
        var agent=go.AddComponent<NavMeshAgent>();agent.speed=3.1f;agent.angularSpeed=300;agent.acceleration=12;agent.radius=.35f;agent.height=1.8f;agent.stoppingDistance=1.1f;
        var brain=go.AddComponent<EnemyBrain>();brain.patrol=route;brain.body=MakeBody(go.transform,new Color(.53f,.24f,.2f),true);enemies.Add(brain);
    }
    void BakeNavigation(){
        var settings=NavMesh.GetSettingsByIndex(0);settings.agentRadius=.38f;settings.agentHeight=1.8f;settings.agentClimb=.35f;
        var data=NavMeshBuilder.BuildNavMeshData(settings,sources,new Bounds(Vector3.zero,new Vector3(104,30,84)),Vector3.zero,Quaternion.identity);
        if(data)navigation=NavMesh.AddNavMeshData(data);else Debug.LogError("Duskline navigation bake failed");
    }
    void Ring(Vector3 center,float radius,Color color,Transform parent){
        var line=new GameObject("Extraction ring").AddComponent<LineRenderer>();line.transform.SetParent(parent);line.useWorldSpace=true;line.positionCount=65;line.startWidth=.1f;line.endWidth=.1f;var mat=new Material(baseMaterial);mat.color=color;line.sharedMaterial=mat;
        for(int i=0;i<=64;i++){float a=i*Mathf.PI/32;line.SetPosition(i,center+new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius));}
    }
}
}
