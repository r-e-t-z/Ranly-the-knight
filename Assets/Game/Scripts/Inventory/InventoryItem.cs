using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public ItemData data;
    public int stackSize;

    public InventoryItem(ItemData itemData, int amount = 1)
    {
        data = itemData;
        stackSize = amount;
    }

    public bool AddToStack(int amount = 1)
    {
        if (stackSize + amount <= data.maxStackSize)
        {
            stackSize += amount;
            return true;
        }
        return false;
    }

    public bool RemoveFromStack(int amount = 1)
    {
        stackSize -= amount;
        return stackSize <= 0;
    }
}