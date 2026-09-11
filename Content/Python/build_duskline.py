"""Run inside Unreal Editor: Tools > Execute Python Script. Never overwrites a map."""
import unreal

MAP = "/Game/Maps/Duskline"
if unreal.EditorAssetLibrary.does_asset_exist(MAP):
    raise RuntimeError("Duskline map already exists. Use the existing map; this script will not overwrite it.")
actors = unreal.get_editor_subsystem(unreal.EditorActorSubsystem)
levels = unreal.get_editor_subsystem(unreal.LevelEditorSubsystem)
tools = unreal.AssetToolsHelpers.get_asset_tools()
unreal.EditorAssetLibrary.make_directory("/Game/Maps")
unreal.EditorAssetLibrary.make_directory("/Game/Items")
if not levels.save_all_dirty_levels():
    raise RuntimeError("Save current levels before creating Duskline")
if not levels.new_level(MAP):
    raise RuntimeError("Could not create Duskline level")

def spawn(cls, name, location, rotation=None):
    a = actors.spawn_actor_from_class(cls, unreal.Vector(*location), rotation or unreal.Rotator())
    if not a:
        raise RuntimeError("Failed to spawn " + name)
    a.set_actor_label(name)
    return a

cube = unreal.load_asset("/Engine/BasicShapes/Cube.Cube")
if not cube:
    raise RuntimeError("Engine basic cube unavailable")
def block(name, location, dimensions):
    a = spawn(unreal.StaticMeshActor, name, location)
    a.static_mesh_component.set_static_mesh(cube)
    a.static_mesh_component.set_collision_profile_name("BlockAll")
    a.set_actor_scale3d(unreal.Vector(*(v / 100 for v in dimensions)))
    return a

block("Ground", (0,0,-50), (10000,8000,100))
for name, p, d in [
    ("NorthBoundary",(0,4000,250),(10000,100,500)),
    ("SouthBoundary",(0,-4000,250),(10000,100,500)),
    ("EastBoundary",(5000,0,250),(100,8000,500)),
    ("WestBoundary",(-5000,0,250),(100,8000,500)),
    ("AlleyWest",(-1500,0,350),(1400,3000,700)),
    ("AlleyEast",(300,0,350),(1400,2400,700)),
    ("NorthBlock",(1900,2200,300),(1200,1800,600)),
    ("RoomBack",(-3500,2300,220),(1700,80,440)),
    ("RoomLeft",(-4350,1600,220),(80,1400,440)),
    ("RoomRight",(-2650,1600,220),(80,1400,440)),
    ("RoomFrontLeft",(-4075,900,220),(550,80,440)),
    ("RoomFrontRight",(-2925,900,220),(550,80,440)),
    ("RoomRoof",(-3500,1600,470),(1780,1480,60)),
    ("StartCover",(-3500,-2500,160),(1000,100,320)),
    ("EastCover",(3400,-500,100),(500,150,200))]:
    block(name,p,d)
spawn(unreal.PlayerStart,"SafeStart",(-4100,-3200,100),unreal.Rotator(0,45,0))
sun=spawn(unreal.DirectionalLight,"DuskSun",(0,0,1500),unreal.Rotator(-45,-30,0))
sun.light_component.set_editor_property("intensity",3.0)
sky=spawn(unreal.SkyLight,"SkyFill",(0,0,1000))
sky.light_component.set_editor_property("intensity",1.0)
lamp=spawn(unreal.PointLight,"RoomLight",(-3500,1600,300))
lamp.light_component.set_editor_property("intensity",6000.0)
lamp.light_component.set_editor_property("attenuation_radius",1800.0)

definitions=[
    ("Food","비상 식량","밀봉된 생존 식량",unreal.ItemKind.FOOD,1.0,12,5),
    ("Medicine","응급 의약품","Q로 체력 30 회복",unreal.ItemKind.MEDICINE,0.5,20,4),
    ("Scrap","정제 고철","복구에 필요한 금속 부품",unreal.ItemKind.SCRAP,2.0,16,4),
    ("Mission","통신 기록 모듈","해원시 구조 신호 복구 자료",unreal.ItemKind.MISSION,1.0,65,2)]
items=[]
for item_id,name,description,kind,weight,value,stack in definitions:
    path="/Game/Items/DA_"+item_id
    item=unreal.load_asset(path)
    if not item:
        factory=unreal.DataAssetFactory()
        factory.set_editor_property("data_asset_class",unreal.ItemDataAsset)
        item=tools.create_asset("DA_"+item_id,"/Game/Items",unreal.ItemDataAsset,factory)
        if not item:
            raise RuntimeError("Failed item asset " + item_id)
        for key,val in {"id":item_id,"display_name":name,"description":description,"kind":kind,"weight":weight,"value":value,"max_stack":stack}.items():
            item.set_editor_property(key,val)
        icon=unreal.load_asset("/Engine/EngineResources/WhiteSquareTexture.WhiteSquareTexture")
        if icon:
            item.set_editor_property("icon",icon)
        unreal.EditorAssetLibrary.save_loaded_asset(item)
    items.append(item)

positions=[(-4100,-1800,40),(-3500,1600,40),(-650,-900,40),(1500,-2100,40),(3800,-600,40),(1200,3300,40),(3500,2200,40)]
for index,pos in enumerate(positions):
    c=spawn(unreal.LootContainer,"Loot_%02d"%index,pos)
    entries=[]
    for item in [items[index%4],items[1 if index%2==0 else 0]]:
        row=unreal.LootEntry()
        row.set_editor_property("item",item)
        row.set_editor_property("minimum",1)
        row.set_editor_property("maximum",2)
        row.set_editor_property("chance",1.0)
        entries.append(row)
    c.set_editor_property("loot_table",entries)
    c.set_editor_property("seed",1337+index)
    c.set_editor_property("forced_open",index in (1,3,5))

for index,route in enumerate([
    [(-600,-1300,100),(-600,1400,100),(-2400,2100,100)],
    [(1700,-2000,100),(3800,-1800,100),(3800,500,100)],
    [(3300,2000,100),(3800,3100,100),(1400,3200,100)]
]):
    enemy=spawn(unreal.DusklineAICharacter,"Infected_%d"%index,route[0])
    enemy.set_editor_property("patrol_points",[unreal.Vector(*p) for p in route])
spawn(unreal.ExtractionZone,"Extraction_NE",(4200,3200,100))
levels.save_current_level()
unreal.log_warning("Duskline map and items saved. REQUIRED: add NavMeshBoundsVolume at (0,0,200), size (10000,8000,1000), Build Paths, press P to verify green routes, then Save and Play. See Docs/EditorSetup.md.")
