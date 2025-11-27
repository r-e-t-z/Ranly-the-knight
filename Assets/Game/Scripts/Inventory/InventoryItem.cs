using UnityEngine;

// Этот класс представляет собой экземпляр предмета в ячейке инвентаря.
// Он не ScriptableObject, а обычный C# класс.
[System.Serializable]
public class InventoryItem
{
    public ItemData data;
    public int stackSize;

    // Конструктор для создания нового предмета в инвентаре.
    public InventoryItem(ItemData itemData, int amount = 1)
    {
        data = itemData;
        stackSize = amount;
    }

    // Попробовать добавить к существующей стопке. Возвращает true, если получилось.
    public bool AddToStack(int amount = 1)
    {
        if (stackSize + amount <= data.maxStackSize)
        {
            stackSize += amount;
            return true;
        }
        return false;
    }

    // Убрать из стопки. Возвращает true, если стопка пуста и предмет можно удалить.
    public bool RemoveFromStack(int amount = 1)
    {
        stackSize -= amount;
        return stackSize <= 0;
    }
}