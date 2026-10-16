using System;
namespace Duskline.Slice {
public sealed partial class Progress {
 public int harinCareer=1;
 public int[] signatureRanks=new int[4];
 public void ValidateSignatures(){harinCareer=Math.Max(1,Math.Min(2,harinCareer));if(signatureRanks==null||signatureRanks.Length!=4){var old=signatureRanks;signatureRanks=new int[4];if(old!=null)Array.Copy(old,signatureRanks,Math.Min(4,old.Length));}for(int i=0;i<4;i++)signatureRanks[i]=Math.Max(0,Math.Min(3,signatureRanks[i]));}
}
public sealed partial class SliceModel {
 public static readonly string[] SignatureNames={"새벽서약","용광로 심장","백야의 맹세","천뢰 쌍성"};
 public int SignatureRank=>progress.signatureRanks[activeHero];
 public int WeaponPower(int hero)=>progress.weapon+progress.signatureRanks[hero]*2;
 public bool SignatureUnlocked=>progress.chapter>=6&&progress.weapon>=6&&progress.resonanceBlueprint;
 public int SignatureMetalCost=>60+SignatureRank*40;
 public int SignatureCoreCost=>SignatureRank+2;
 public bool CanForgeSignature=>place==Place.Shelter&&SignatureUnlocked&&SignatureRank<3&&progress.metal>=SignatureMetalCost&&progress.cores>=SignatureCoreCost;
 public bool SelectCareer(int branch){if(place!=Place.Shelter||activeHero!=2||SignatureRank<1||branch<1||branch>2)return false;progress.harinCareer=branch;Emit("career",x);Save?.Invoke();return true;}
 public bool AdvancedGunner=>activeHero==2&&SignatureRank>0;
 public string CareerName=>progress.harinCareer==1?"연사 사수":"관통 포격수";
 public bool ForgeSignature(){
  if(!CanForgeSignature)return false;
  progress.metal-=SignatureMetalCost;progress.cores-=SignatureCoreCost;progress.signatureRanks[activeHero]++;Emit("signature-forged",x);Save?.Invoke();return true;
 }
}
}
