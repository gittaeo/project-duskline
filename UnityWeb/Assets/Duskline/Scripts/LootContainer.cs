using System;
using System.Collections.Generic;
using UnityEngine;
namespace Duskline {
[Serializable] public sealed class LootRow {public ItemDefinition item;public int minimum=1,maximum=2;[Range(0,1)]public float chance=1;}
public sealed class LootContainer : MonoBehaviour,IInteractable {
    public LootRow[] table;public int seed=1337;public bool fixedSeed=true,forced;
    public Transform lid;public Renderer indicator;
    public IReadOnlyList<ItemSlot> Contents=>contents;
    readonly List<ItemSlot> contents=new List<ItemSlot>();bool opened,generated;
    public bool Empty=>generated&&contents.Count==0;
    public string Prompt {
        get{if(!opened)return forced?"[E] 봉인 상자 열기 · 큰 소음":"[E] 보급 상자 열기";
        if(Empty)return "비어 있는 보급 상자";
        string text="[E] 모두 획득";foreach(var s in contents)if(s.item)text+="  ·  "+s.item.displayName+" ×"+s.count;return text;}
    }
    void Start(){Generate();}
    public void Generate(){
        if(generated)return;generated=true;var random=new System.Random(fixedSeed?seed:Environment.TickCount);
        if(table==null)return;foreach(var row in table){if(row==null||!row.item||!row.item.Valid||row.minimum<0||row.maximum<row.minimum||row.maximum>10000){Debug.LogWarning("Duskline: invalid loot row skipped",this);continue;}
            if(random.NextDouble()>=Mathf.Clamp01(row.chance))continue;int n=random.Next(row.minimum,row.maximum+1);if(n>0)contents.Add(new ItemSlot(row.item,n));}
    }
    public bool CanInteract(PlayerMotor p)=>p&&p.CanAct&&Vector3.Distance(p.transform.position,transform.position)<3.5f;
    public void Interact(PlayerMotor p){
        if(!CanInteract(p))return;Generate();var run=RunDirector.Instance;
        if(!opened){opened=true;if(lid)lid.localRotation=Quaternion.Euler(-60,0,0);if(forced)p.noise.Emit(p.noise.forcedRadius,8);run.Notify("내용물을 확인했습니다 · E로 획득");return;}
        if(Empty){run.Notify("이미 비워진 상자입니다");return;}
        int total=0;string reason="";
        for(int i=contents.Count-1;i>=0;i--){var s=contents[i];var r=p.inventory.Add(s.item,s.count);s.count-=r.Processed;total+=r.Processed;if(!r.Success)reason=r.Reason;if(s.count<=0)contents.RemoveAt(i);}
        if(indicator&&Empty)indicator.material.color=new Color(.25f,.3f,.3f);
        run.Notify(reason.Length>0?reason:$"물자 {total}개 획득");
    }
}
}
