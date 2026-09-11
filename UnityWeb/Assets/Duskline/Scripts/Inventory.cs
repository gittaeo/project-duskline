using System;
using System.Collections.Generic;
using UnityEngine;
namespace Duskline {
[Serializable] public sealed class ItemSlot {public ItemDefinition item; public int count; public ItemSlot(ItemDefinition i,int n){item=i;count=n;}}
public readonly struct Transfer {
    public readonly int Processed; public readonly bool Success; public readonly string Reason;
    public Transfer(int n,int requested,string reason=""){Processed=n;Success=n==requested;Reason=reason;}
}
public sealed class Inventory : MonoBehaviour {
    public int maximumSlots=10;
    public float maximumWeight=24;
    readonly List<ItemSlot> slots=new List<ItemSlot>();
    public IReadOnlyList<ItemSlot> Slots => slots;
    public event Action Changed;
    public float Weight {get{float n=0;foreach(var s in slots) if(s.item) n+=s.item.weight*s.count;return n;}}
    public int Value {get{long n=0;foreach(var s in slots) if(s.item) n+=(long)s.item.value*s.count;return (int)Math.Min(n,int.MaxValue);}}
    public int Count {get{int n=0;foreach(var s in slots)n+=s.count;return n;}}
    public Transfer Add(ItemDefinition item,int count){
        if(!item || !item.Valid || count<=0) return new Transfer(0,count,"잘못된 아이템 데이터");
        foreach(var s in slots) if(s.item!=item && s.item.id==item.id) return new Transfer(0,count,"중복 아이템 ID");
        int left=(int)Math.Min(count,Math.Floor(Math.Max(0,maximumWeight-Weight)/(double)item.weight)); int allowed=left;
        foreach(var s in slots) if(s.item==item && left>0){int n=Math.Min(left,Math.Max(0,item.maxStack-s.count));s.count+=n;left-=n;}
        while(left>0 && slots.Count<Mathf.Clamp(maximumSlots,0,1000)){int n=Math.Min(left,item.maxStack);slots.Add(new ItemSlot(item,n));left-=n;}
        int processed=allowed-left; if(processed>0) Changed?.Invoke();
        return new Transfer(processed,count,processed==count?"":allowed<count?"무게 한도 초과 · 잔량은 상자에 남습니다":"슬롯 부족 · 잔량은 상자에 남습니다");
    }
    public Transfer Remove(ItemDefinition item,int count){
        if(!item || count<=0) return new Transfer(0,count,"잘못된 요청"); int left=count;
        for(int i=slots.Count-1;i>=0 && left>0;i--) if(slots[i].item==item){int n=Math.Min(left,slots[i].count);slots[i].count-=n;left-=n;if(slots[i].count==0)slots.RemoveAt(i);}
        if(left<count) Changed?.Invoke(); return new Transfer(count-left,count);
    }
    public Transfer UseMedicine(Health health){
        if(!health || !health.Alive || health.Current>=health.maximum) return new Transfer(0,1,"치료가 필요하지 않습니다");
        foreach(var s in slots) if(s.item && s.item.Valid && s.item.kind==ItemKind.Medicine && s.item.healing>0){var item=s.item;if(health.Heal(item.healing)>0)return Remove(item,1);break;}
        return new Transfer(0,1,"의약품이 없습니다");
    }
}
}
