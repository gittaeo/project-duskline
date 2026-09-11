using UnityEngine;
namespace Duskline {
public enum ItemKind { Food, Medicine, Scrap, Mission }
[CreateAssetMenu(menuName="Duskline/Item")]
public sealed class ItemDefinition : ScriptableObject {
    public string id, displayName;
    [TextArea] public string description;
    public ItemKind kind;
    [Min(.01f)] public float weight=1;
    [Min(0)] public int value=10;
    [Min(1)] public int maxStack=5;
    public float healing=30;
    public Color iconColor=Color.white;
    public bool Valid => !string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(displayName) && weight>0 && !float.IsInfinity(weight) && !float.IsNaN(weight) && value>=0 && maxStack>0 && maxStack<=10000 && healing>=0 && !float.IsNaN(healing) && !float.IsInfinity(healing);
}
}
