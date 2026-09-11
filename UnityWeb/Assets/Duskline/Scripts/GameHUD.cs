using System.Collections.Generic;
using UnityEngine;
namespace Duskline {
public sealed class GameHUD : MonoBehaviour {
    readonly Dictionary<int,GUIStyle> styles=new Dictionary<int,GUIStyle>();
    RunDirector run;PlayerMotor player;Font font;
    string supplies="",clock="10:00",value="0",weight="0.0",notice="";
    float lastHealth=100,hitUntil;bool showInventory=true;
    static readonly Color ink=new Color(.035f,.07f,.085f,.94f),muted=new Color(.62f,.72f,.75f),mint=new Color(.33f,.91f,.75f),gold=new Color(1,.73f,.4f);
    void Start(){
        run=RunDirector.Instance;player=run.player;font=Resources.Load<Font>("DuskFont");
        run.Changed+=Refresh;player.inventory.Changed+=Refresh;player.health.Changed+=HealthChanged;player.stamina.Changed+=Refresh;Refresh();
    }
    void OnDestroy(){if(run)run.Changed-=Refresh;if(player){player.inventory.Changed-=Refresh;player.health.Changed-=HealthChanged;player.stamina.Changed-=Refresh;}}
    void HealthChanged(){if(player.health.Current<lastHealth)hitUntil=Time.unscaledTime+.45f;lastHealth=player.health.Current;Refresh();}
    void Refresh(){if(!run||!player)return;int seconds=Mathf.CeilToInt(run.Remaining);clock=$"{seconds/60:00}:{seconds%60:00}";value=player.inventory.Value.ToString();weight=player.inventory.Weight.ToString("0.0");notice=run.Notice;supplies="";foreach(var slot in player.inventory.Slots)if(slot.item)supplies+=$"{slot.item.displayName}   × {slot.count}\n";if(supplies.Length==0)supplies="아직 확보한 물자가 없습니다";}
    void Update(){if(Input.GetKeyDown(KeyCode.Tab))showInventory=!showInventory;if(run&&run.Result!=RunResult.Ready&&run.Result!=RunResult.Running&&Input.GetKeyDown(KeyCode.R))run.Restart();}
    GUIStyle Style(int size,Color color,TextAnchor align=TextAnchor.UpperLeft){
        if(!styles.TryGetValue(size,out var s)){s=new GUIStyle(GUI.skin.label){font=font,fontSize=size,wordWrap=true,richText=false};styles[size]=s;}
        s.normal.textColor=color;s.alignment=align;return s;
    }
    void Text(Rect rect,string text,int size,Color color,TextAnchor align=TextAnchor.UpperLeft){GUI.Label(rect,text,Style(size,color,align));}
    void Box(Rect rect,Color color){var old=GUI.color;GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=old;}
    bool Button(Rect rect,string label){bool hover=rect.Contains(Event.current.mousePosition);Box(rect,hover?new Color(.28f,.75f,.63f):mint);Text(rect,label,18,ink,TextAnchor.MiddleCenter);return GUI.Button(rect,GUIContent.none,GUIStyle.none);}
    void Bar(Rect rect,float percent,Color color){Box(rect,new Color(.2f,.28f,.3f,.85f));Box(new Rect(rect.x,rect.y,rect.width*Mathf.Clamp01(percent),rect.height),color);}
    void OnGUI(){
        if(!run||!player)return;
        float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1280*scale)/2,(Screen.height-720*scale)/2,0),Quaternion.identity,Vector3.one*scale);
        if(run.Result==RunResult.Ready){Menu(false);return;}
        Box(new Rect(24,22,234,70),ink);Box(new Rect(24,22,3,70),mint);Text(new Rect(40,30,210,26),"DUSKLINE / 황혼선",18,Color.white);Text(new Rect(40,60,200,24),"해원시 · 제04 격리 구역",12,muted);
        Box(new Rect(404,22,472,78),ink);Text(new Rect(424,32,350,23),$"물자 가치 {value} / {run.requiredValue} 확보 후 탈출",16,Color.white);Text(new Rect(786,31,72,26),clock,22,run.Remaining<60?gold:mint,TextAnchor.MiddleRight);Bar(new Rect(424,74,432,5),player.inventory.Value/(float)run.requiredValue,mint);
        MiniMap();
        Box(new Rect(24,533,272,144),ink);Text(new Rect(42,547,230,24),"수색원 상태  /  "+player.MovementName,14,muted);
        Text(new Rect(42,581,200,21),$"체력  {player.health.Current:0} / 100",14,Color.white);Bar(new Rect(42,607,234,7),player.health.Current/player.health.maximum,new Color(.94f,.43f,.35f));
        Text(new Rect(42,624,220,21),$"스태미나  {player.stamina.Current:0} / 100",14,Color.white);Bar(new Rect(42,650,234,7),player.stamina.Current/player.stamina.maximum,mint);
        if(showInventory){Box(new Rect(998,414,258,263),ink);Text(new Rect(1016,430,220,25),"확보한 물자",18,Color.white);Text(new Rect(1016,461,220,23),$"{weight} / {player.inventory.maximumWeight:0} kg   ·   가치 {value}",13,mint);Text(new Rect(1016,501,220,151),supplies,14,muted);}
        Text(new Rect(590,328,100,50),"+",23,Color.white,TextAnchor.MiddleCenter);
        if(player.InteractionPrompt.Length>0){Box(new Rect(324,568,638,49),ink);Text(new Rect(342,576,602,36),player.InteractionPrompt,15,mint,TextAnchor.MiddleCenter);}
        if(notice.Length>0){Box(new Rect(354,504,578,45),ink);Text(new Rect(369,511,548,32),notice,15,gold,TextAnchor.MiddleCenter);}
        if(run.ExtractionLeft>0){Box(new Rect(434,120,412,68),ink);Text(new Rect(450,131,380,30),$"구조선 접근 중  ·  {run.ExtractionLeft:0.0}초",22,mint,TextAnchor.MiddleCenter);Bar(new Rect(454,171,372,4),1-run.ExtractionLeft/run.extractionDuration,mint);}
        Box(new Rect(0,693,1280,27),ink);Text(new Rect(24,695,1232,25),"WASD 이동   SHIFT 달리기   CTRL 은신   SPACE 점프   좌클릭 공격   E 상호작용   Q 치료   TAB 물자   ESC 일시정지",12,muted,TextAnchor.MiddleCenter);
        if(Time.unscaledTime<hitUntil){var red=new Color(.95f,.12f,.08f,.5f);Box(new Rect(0,0,1280,10),red);Box(new Rect(0,710,1280,10),red);Box(new Rect(0,0,10,720),red);Box(new Rect(1270,0,10,720),red);}
        if(run.Paused){Menu(true);return;}
        if(run.Result!=RunResult.Running){Box(new Rect(0,0,1280,720),new Color(.02f,.04f,.05f,.86f));Box(new Rect(370,166,540,380),ink);Text(new Rect(410,201,460,30),"수색 보고서 / AFTER ACTION",14,muted);Text(new Rect(410,258,460,60),run.Result==RunResult.Escaped?"탈출 성공":run.Result==RunResult.Dead?"수색 실패 · 사망":"수색 실패 · 시간 초과",32,run.Result==RunResult.Escaped?mint:gold);Text(new Rect(410,346,460,64),$"회수한 물자 {run.FinalCount}개\n총 가치 {run.FinalValue}",20,Color.white);if(Button(new Rect(410,452,460,56),"다시 수색하기  [R]"))run.Restart();}
    }
    void Menu(bool pause){
        Box(new Rect(0,0,1280,720),new Color(.025f,.055f,.07f,.86f));Box(new Rect(80,86,4,44),mint);
        Text(new Rect(104,86,800,32),"PROJECT DUSKLINE  /  WEB EDITION",19,mint);
        Text(new Rect(100,152,900,95),pause?"잠시 숨을 고르세요":"황혼선",64,Color.white);
        Text(new Rect(104,257,620,70),pause?"수색이 일시정지되었습니다. 돌아갈 준비가 되면 계속하세요.":"통신이 끊긴 해원시. 해가 지기 전,\n필요한 물자를 챙겨 격리 구역을 빠져나오세요.",23,muted);
        string[] heads={"01  탐색과 수집","02  소음을 관리하세요","03  살아서 돌아오세요"};string[] details={"상자에 조준 후 E로 열고, 다시 E로 획득합니다.\n물자 가치 180 이상을 확보하세요.","걷기와 달리기는 감염체를 유인합니다.\nCtrl로 웅크리고, Q로 의약품을 사용하세요.","우측 지도 북동쪽의 청록색 비콘으로 이동하세요.\nE로 요청하고 8초 동안 구역을 지키세요."};
        for(int i=0;i<3;i++){float x=104+i*363;Box(new Rect(x,372,343,134),ink);Text(new Rect(x+18,389,309,29),heads[i],18,mint);Text(new Rect(x+18,433,309,67),details[i],14,muted);}
        if(Button(new Rect(104,554,326,61),pause?"수색 계속하기":"수색 시작하기")){if(pause)run.TogglePause();else run.Begin();}
        Text(new Rect(456,568,640,40),"마우스 시점 · 데스크톱 키보드 권장 · ESC 일시정지",15,muted);
        Text(new Rect(104,662,1060,30),"싱글플레이 프로토타입  /  제한 시간 10분  /  자동 저장 없음",12,muted);
    }
    void MiniMap(){
        Box(new Rect(1038,22,218,222),ink);Text(new Rect(1054,33,186,25),"해원시 구역도      N ↑",13,muted);
        Rect map=new Rect(1054,66,186,149);Box(map,new Color(.13f,.19f,.21f));
        MapBlock(map,-22,-15,14,30);MapBlock(map,-4,-12,14,24);MapBlock(map,13,13,12,18);
        if(WorldBootstrap.Instance)foreach(var c in WorldBootstrap.Instance.Crates)if(c&&!c.Empty)Dot(map,c.transform.position,gold,4);
        if(run.exit)Dot(map,run.exit.transform.position,mint,8);Dot(map,player.transform.position,Color.white,6);
        Text(new Rect(1054,219,192,23),"● 나   ■ 물자   ■ 탈출",11,muted);
    }
    void Dot(Rect r,Vector3 p,Color c,float size){Box(new Rect(r.x+(p.x+50)/100*r.width-size/2,r.y+(40-p.z)/80*r.height-size/2,size,size),c);}
    void MapBlock(Rect r,float x,float z,float w,float h){Box(new Rect(r.x+(x+50)/100*r.width,r.y+(40-z-h)/80*r.height,w/100*r.width,h/80*r.height),new Color(.3f,.37f,.39f));}
}
}
