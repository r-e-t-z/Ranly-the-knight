using UnityEngine;

public enum ItemType
{
    Default,
    Consumable,
    Weapon,
    QuestItem
}

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemID;
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Settings")]
    public ItemType type;
    public bool isStackable = false;
    public int maxStackSize = 1;

    [Header("Crafting")]
    public CraftingRecipe[] craftingRecipes;
}

[System.Serializable]
public struct CraftingRecipe
{
    public ItemData item1;
    public ItemData item2;
}