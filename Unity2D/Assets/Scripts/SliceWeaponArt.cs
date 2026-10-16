using UnityEngine;
namespace Duskline.Slice {
public sealed partial class SliceGame {
 static Vector2 SpritePoint(Sprite sprite,float u,float v){return new Vector2((sprite.rect.width*u-sprite.pivot.x)/sprite.pixelsPerUnit,(sprite.rect.height*(1-v)-sprite.pivot.y)/sprite.pixelsPerUnit);}
 static readonly Vector4[][] WalkWeaponPoints={
  new[]{new Vector4(.51f,.58f,.89f,.68f),new Vector4(.63f,.58f,.98f,.68f),new Vector4(.49f,.58f,.83f,.67f),new Vector4(.60f,.58f,.96f,.67f),new Vector4(.61f,.54f,.99f,.66f),new Vector4(.58f,.53f,.96f,.65f),new Vector4(.57f,.53f,.93f,.64f),new Vector4(.61f,.54f,.98f,.65f)},
  new[]{new Vector4(.24f,.51f,.18f,.66f),new Vector4(.23f,.51f,.17f,.68f),new Vector4(.29f,.53f,.22f,.68f),new Vector4(.33f,.53f,.28f,.70f),new Vector4(.21f,.51f,.17f,.68f),new Vector4(.23f,.51f,.17f,.68f),new Vector4(.34f,.51f,.28f,.67f),new Vector4(.39f,.53f,.35f,.68f)},
  new[]{new Vector4(.77f,.49f,.88f,.56f),new Vector4(.78f,.49f,.89f,.57f),new Vector4(.73f,.50f,.86f,.57f),new Vector4(.79f,.50f,.91f,.58f),new Vector4(.79f,.48f,.92f,.56f),new Vector4(.76f,.49f,.88f,.56f),new Vector4(.73f,.49f,.86f,.55f),new Vector4(.77f,.49f,.90f,.57f)},
  new[]{new Vector4(.35f,.55f,.31f,.77f),new Vector4(.37f,.54f,.34f,.78f),new Vector4(.40f,.54f,.37f,.78f),new Vector4(.40f,.54f,.36f,.77f),new Vector4(.38f,.54f,.35f,.73f),new Vector4(.41f,.54f,.40f,.69f),new Vector4(.43f,.61f,.40f,.74f),new Vector4(.39f,.61f,.36f,.76f)}
 };
 Vector3 WeaponSocket(bool tip){
  var pose=WeaponAnchors(model.activeHero,hero.sprite);var point=SpritePoint(hero.sprite,tip?pose.z:pose.x,tip?pose.w:pose.y);if(hero.flipX)point.x=-point.x;return hero.transform.TransformPoint(point);
 }
 Vector3 EquippedMuzzle(){return WeaponSocket(true);}
 Vector4 WeaponAnchors(int id,Sprite sprite){
  for(int frame=0;frame<8;frame++)if(sprite==walkEight[id][frame])return WalkWeaponPoints[id][frame];
  if(id==0){for(int i=0;i<motion.Length;i++)if(sprite==motion[i])return MotionBlades[i];for(int i=0;i<combat.Length;i++)if(sprite==combat[i])return CombatBlades[i];if(sprite==skillFrames[0])return new Vector4(.51f,.54f,.24f,.45f);if(sprite==skillFrames[3])return new Vector4(.80f,.67f,.96f,.73f);return new Vector4(.51f,.58f,.89f,.68f);}
  if(id==1){if(sprite==tagFrames[2])return new Vector4(.77f,.30f,.99f,.32f);if(sprite==tagFrames[3])return new Vector4(.78f,.68f,.84f,.94f);if(sprite==skillFrames[1])return new Vector4(.55f,.31f,.64f,.24f);if(sprite==skillFrames[4])return new Vector4(.80f,.70f,.95f,.80f);return new Vector4(.24f,.51f,.18f,.66f);}
  if(id==2){if(sprite==tagFrames[4])return new Vector4(.52f,.40f,.62f,.46f);if(sprite==tagFrames[5])return new Vector4(.62f,.47f,.73f,.55f);if(sprite==tagFrames[6])return new Vector4(.83f,.22f,.97f,.20f);if(sprite==tagFrames[7])return new Vector4(.74f,.57f,.89f,.56f);if(sprite==skillFrames[2])return new Vector4(.64f,.52f,.66f,.39f);if(sprite==skillFrames[5])return new Vector4(.73f,.50f,.82f,.49f);return new Vector4(.77f,.49f,.88f,.56f);}
  if(sprite==serinFrames[0])return new Vector4(.24f,.55f,.29f,.93f);
  if(sprite==serinFrames[1])return new Vector4(.40f,.53f,.03f,.80f);
  if(sprite==serinFrames[2])return new Vector4(.51f,.43f,.97f,.43f);
  if(sprite==serinFrames[3])return new Vector4(.53f,.61f,.94f,.85f);
  if(sprite==serinFrames[4])return new Vector4(.46f,.22f,.04f,.04f);
  if(sprite==serinFrames[5])return new Vector4(.62f,.46f,.97f,.48f);
  return new Vector4(.36f,.55f,.31f,.77f);
 }
}
}
