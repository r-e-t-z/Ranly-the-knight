using UnityEngine;

[CreateAssetMenu(fileName = "Item Database", menuName = "Inventory/Item Database")]
public class ItemDBSO : ScriptableObject
{
    public ItemData[] allItems;

    public ItemData GetItemByID(string id)
    {
        if (allItems == null)
        {
            return null;
        }

        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        foreach (ItemData item in allItems)
        {
            if (item == null)
            {
                continue;
            }

            if (item.itemID == id)
            {
                return item;
            }
        }

        return null;
    }
}