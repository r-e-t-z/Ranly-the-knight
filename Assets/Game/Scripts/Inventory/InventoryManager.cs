using UnityEngine;
using System;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public static event Action<InventorySlot[]> OnInventoryChanged;
    public static event Action<InventoryItem> OnActiveItemChanged;

    [SerializeField] private int inventorySize = 9;
    public InventorySlot[] inventorySlots;
    public InventorySlot activeItemSlot;

    [SerializeField] private ItemDBSO itemDatabase;

    [Header("UI Settings")]
    public GameObject inventoryPanel;
    public GameObject activeSlotUI; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeInventory();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    private void InitializeInventory()
    {
        inventorySlots = new InventorySlot[inventorySize];
        activeItemSlot = new InventorySlot();

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            inventorySlots[i] = new InventorySlot();
        }
    }

    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            bool newState = !inventoryPanel.activeInHierarchy;
            inventoryPanel.SetActive(newState);
        }
    }

    public void MoveToActiveSlot(int fromSlotIndex)
    {
        if (fromSlotIndex < 0 || fromSlotIndex >= inventorySlots.Length) return;
        if (!inventorySlots[fromSlotIndex].HasItem()) return;

        if (activeItemSlot.HasItem())
        {
            int emptySlot = FindEmptySlot();
            if (emptySlot != -1)
            {
                inventorySlots[emptySlot].SetItem(activeItemSlot.Item);
                activeItemSlot.ClearSlot();
            }
            else
            {
                return;
            }
        }

        activeItemSlot.SetItem(inventorySlots[fromSlotIndex].Item);
        inventorySlots[fromSlotIndex].ClearSlot();

        OnInventoryChanged?.Invoke(inventorySlots);
        OnActiveItemChanged?.Invoke(activeItemSlot.Item);
    }

    public void MoveToInventoryFromActive()
    {
        if (!activeItemSlot.HasItem()) return;

        int emptySlot = FindEmptySlot();
        if (emptySlot != -1)
        {
            inventorySlots[emptySlot].SetItem(activeItemSlot.Item);
            activeItemSlot.ClearSlot();

            OnInventoryChanged?.Invoke(inventorySlots);
            OnActiveItemChanged?.Invoke(null);
        }
        else
        {
            Debug.Log("Нет свободного места в инвентаре!");
        }
    }

    private int FindEmptySlot()
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (!inventorySlots[i].HasItem())
            {
                return i;
            }
        }
        return -1;
    }

    public bool AddItem(string itemID, int amount = 1)
    {
        ItemData itemToAdd = itemDatabase.GetItemByID(itemID);
        if (itemToAdd == null) return false;

        if (activeItemSlot.HasItem() && itemToAdd == activeItemSlot.Item.data && itemToAdd.isStackable)
        {
            activeItemSlot.Item.AddToStack(amount);
            OnActiveItemChanged?.Invoke(null);
            return true;
            Debug.Log("Added to active slot stack.");
        }
        else
        {
            if (itemToAdd.isStackable)
            {
                for (int i = 0; i < inventorySlots.Length; i++)
                {
                    if (inventorySlots[i].HasItem() && inventorySlots[i].Item.data == itemToAdd)
                    {
                        if (inventorySlots[i].Item.AddToStack(amount))
                        {
                            OnInventoryChanged?.Invoke(inventorySlots);
                            return true;
                        }
                    }
                }
            }

            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (!inventorySlots[i].HasItem())
                {
                    inventorySlots[i].SetItem(new InventoryItem(itemToAdd, amount));
                    OnInventoryChanged?.Invoke(inventorySlots);
                    return true;
                }
            }
        }

        

        

        Debug.Log("Inventory is full!");
        return false;
    }

    public void TryCraftItems(int fromSlotIndex, int toSlotIndex)
    {
        InventoryItem itemA = inventorySlots[fromSlotIndex].Item;
        InventoryItem itemB = inventorySlots[toSlotIndex].Item;

        if (itemA == null || itemB == null) return;

        Debug.Log($"Пытаемся скрафтить: {itemA.data.itemName} + {itemB.data.itemName}");

        ItemData resultItem = FindCraftingResult(itemA.data, itemB.data);

        if (resultItem != null)
        {
            Debug.Log($"Успешный крафт! Получаем: {resultItem.itemName}");

            RemoveItemFromSlot(fromSlotIndex);
            RemoveItemFromSlot(toSlotIndex);

            AddItem(resultItem.itemID);
        }
        else
        {
            Debug.Log($"Нельзя скрафтить: {itemA.data.itemName} + {itemB.data.itemName}");
        }
    }

    private ItemData FindCraftingResult(ItemData ingredient1, ItemData ingredient2)
    {
        foreach (ItemData potentialResult in itemDatabase.allItems)
        {
            if (potentialResult.craftingRecipes != null)
            {
                foreach (CraftingRecipe recipe in potentialResult.craftingRecipes)
                {
                    if ((recipe.item1 == ingredient1 && recipe.item2 == ingredient2) ||
                        (recipe.item1 == ingredient2 && recipe.item2 == ingredient1))
                    {
                        Debug.Log($"Найден рецепт: {ingredient1.itemName} + {ingredient2.itemName} = {potentialResult.itemName}");
                        return potentialResult;
                    }
                }
            }
        }
        return null;
    }

    public bool RemoveItemFromActiveSlot(string itemID, int amount)
    {
        var activeSlot = activeItemSlot;

        if (activeSlot.HasItem() && activeSlot.Item.data.itemID == itemID)
        {
            if (activeSlot.Item.stackSize >= amount)
            {
                if (activeSlot.Item.stackSize == amount)
                {
                    activeSlot.ClearSlot();
                    ForceInventoryUpdate();

                    return true;
                }
                else
                {
                    activeSlot.Item.stackSize -= amount;
                    ForceInventoryUpdate();

                    return true;
                }
            }
        }

        return false;
    }

    public void SetActiveItem(InventoryItem item)
    {
        activeItemSlot.SetItem(item);
        OnActiveItemChanged?.Invoke(item);
    }

    public void ClearActiveItem()
    {
        activeItemSlot.ClearSlot();
        OnActiveItemChanged?.Invoke(null);
    }

    public void RemoveItemFromSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < inventorySlots.Length)
        {
            inventorySlots[slotIndex].ClearSlot();
            OnInventoryChanged?.Invoke(inventorySlots);
        }
    }

    public int GetItemCount(string itemID)
    {
        int totalCount = 0;

        if (activeItemSlot.HasItem() && activeItemSlot.Item.data.itemID == itemID)
        {
            totalCount += activeItemSlot.Item.stackSize;
        }

        foreach (var slot in inventorySlots)
        {
            if (slot.HasItem() && slot.Item.data.itemID == itemID)
            {
                totalCount += slot.Item.stackSize;
            }
        }

        return totalCount;
    }

    public int GetActiveSlotItemCount(string itemID)
    {
        if (activeItemSlot.HasItem() && activeItemSlot.Item.data.itemID == itemID)
        {
            return activeItemSlot.Item.stackSize;
        }
        return 0;
    }

    public void ForceInventoryUpdate()
    {
        OnInventoryChanged?.Invoke(inventorySlots);

        if (activeItemSlot.HasItem())
        {
            OnActiveItemChanged?.Invoke(activeItemSlot.Item);
        }
        else
        {
            OnActiveItemChanged?.Invoke(null);
        }
    }


    public void UpdateInventoryUI()
    {
        OnInventoryChanged?.Invoke(inventorySlots);
        if (activeItemSlot.HasItem())
        {
            OnActiveItemChanged?.Invoke(activeItemSlot.Item);
        }
        else
        {
            OnActiveItemChanged?.Invoke(null);
        }
    }
}

[System.Serializable]
public class InventorySlot
{
    public InventoryItem Item;

    public bool HasItem()
    {
        return Item != null;
    }

    public void SetItem(InventoryItem newItem)
    {
        Item = newItem;
    }

    public void ClearSlot()
    {
        Item = null;
    }
}