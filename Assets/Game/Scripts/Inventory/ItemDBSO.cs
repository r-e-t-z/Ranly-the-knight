using UnityEngine;

[CreateAssetMenu(fileName = "Item Database", menuName = "Inventory/Item Database")]
public class ItemDBSO : ScriptableObject
{
    public ItemData[] allItems;

    // Метод для поиска предмета по его ID.
    public ItemData GetItemByID(string id)
    {
        if (allItems == null)
        {
            Debug.LogError("Item Database: allItems array is null!");
            return null;
        }

        if (string.IsNullOrEmpty(id))
        {
            Debug.LogError("Item Database: itemID is null or empty!");
            return null;
        }

        foreach (ItemData item in allItems)
        {
            if (item == null)
            {
                Debug.LogWarning("Item Database: found null item in allItems array");
                continue;
            }

            if (item.itemID == id)
            {
                return item;
            }
        }

        Debug.LogWarning($"Item with ID '{id}' not found in database!");
        return null;
    }
}