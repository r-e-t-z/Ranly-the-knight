using UnityEngine;

// Этот enum можно расширять. Он определяет, что можно делать с предметом.
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
    public string itemID; // Уникальный строковый ключ предмета
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Settings")]
    public ItemType type;
    public bool isStackable = false;
    public int maxStackSize = 1;

    [Header("Crafting")]
    // Это массив структур, определяющих, во что можно скрафтить этот предмет.
    // "Рецепт" будет храниться в самом предмете, в который превращаются два других.
    public CraftingRecipe[] craftingRecipes;
}

// Структура, описывающая рецепт крафта.
// "Для создания этого предмета (в котором лежит этот рецепт) нужны item1 и item2."
[System.Serializable]
public struct CraftingRecipe
{
    public ItemData item1;
    public ItemData item2;
}