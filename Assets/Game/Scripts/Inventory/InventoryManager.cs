using UnityEngine;
using System;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    // События для уведомления UI об изменениях в инвентаре.
    public static event Action<InventorySlot[]> OnInventoryChanged;
    public static event Action<InventoryItem> OnActiveItemChanged;

    // Основные данные инвентаря.
    [SerializeField] private int inventorySize = 9;
    public InventorySlot[] inventorySlots;
    public InventorySlot activeItemSlot;

    // Ссылка на базу данных предметов.
    [SerializeField] private ItemDBSO itemDatabase;

    [Header("UI Settings")]
    public GameObject inventoryPanel;
    public GameObject activeSlotUI; // Перетащи сюда активный слот из сцены!

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
        // Открытие/закрытие инвентаря по Tab
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

    // Метод для открытия/закрытия инвентаря
    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            bool newState = !inventoryPanel.activeInHierarchy;
            inventoryPanel.SetActive(newState);
        }
    }

    // Методы для перемещения между инвентарем и активным слотом
    public void MoveToActiveSlot(int fromSlotIndex)
    {
        if (fromSlotIndex < 0 || fromSlotIndex >= inventorySlots.Length) return;
        if (!inventorySlots[fromSlotIndex].HasItem()) return;

        // Если в активном слоте уже есть предмет - сначала перемещаем его обратно
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
                Debug.Log("Нет свободного места в инвентаре для активного предмета!");
                return;
            }
        }

        // Перемещаем новый предмет в активный слот
        activeItemSlot.SetItem(inventorySlots[fromSlotIndex].Item);
        inventorySlots[fromSlotIndex].ClearSlot();

        // Обновляем UI
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

    // Вспомогательный метод для поиска пустого слота
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

    // Главный метод добавления предмета в инвентарь по его ID.
    public bool AddItem(string itemID, int amount = 1)
    {
        ItemData itemToAdd = itemDatabase.GetItemByID(itemID);
        if (itemToAdd == null) return false;

        // Логика добавления в существующий стек (если предмет стакаемый)
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

        // Логика поиска пустого слота для нового предмета
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (!inventorySlots[i].HasItem())
            {
                inventorySlots[i].SetItem(new InventoryItem(itemToAdd, amount));
                OnInventoryChanged?.Invoke(inventorySlots);
                return true;
            }
        }

        Debug.Log("Inventory is full!");
        return false;
    }

    // Метод для попытки скрафтить два предмета.
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

    // Вспомогательный метод для поиска результата крафта.
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

        Debug.Log($"Рецепт не найден для: {ingredient1.itemName} + {ingredient2.itemName}");
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
                    Debug.Log($"?? Полностью забраны предметы из активного слота: {itemID} x{amount}");
                    return true;
                }
                else
                {
                    activeSlot.Item.stackSize -= amount;
                    ForceInventoryUpdate();
                    Debug.Log($"?? Частично забраны предметы из активного слота: {itemID} x{amount}. Осталось: {activeSlot.Item.stackSize}");
                    return true;
                }
            }
        }

        Debug.LogWarning($"? Не удалось забрать предмет {itemID} x{amount} из активного слота");
        return false;
    }

    // Установить активный предмет
    public void SetActiveItem(InventoryItem item)
    {
        activeItemSlot.SetItem(item);
        OnActiveItemChanged?.Invoke(item);
    }

    // Убрать активный предмет
    public void ClearActiveItem()
    {
        activeItemSlot.ClearSlot();
        OnActiveItemChanged?.Invoke(null);
    }

    // Удалить предмет из определенного слота
    public void RemoveItemFromSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < inventorySlots.Length)
        {
            inventorySlots[slotIndex].ClearSlot();
            OnInventoryChanged?.Invoke(inventorySlots);
        }
    }

    // Добавь этот метод в класс InventoryManager
    public int GetItemCount(string itemID)
    {
        int totalCount = 0;

        // Проверяем активный слот
        if (activeItemSlot.HasItem() && activeItemSlot.Item.data.itemID == itemID)
        {
            totalCount += activeItemSlot.Item.stackSize;
        }

        // Проверяем обычные слоты инвентаря
        foreach (var slot in inventorySlots)
        {
            if (slot.HasItem() && slot.Item.data.itemID == itemID)
            {
                totalCount += slot.Item.stackSize;
            }
        }

        return totalCount;
    }

    public void ForceInventoryUpdate()
    {
        // Вызываем события для принудительного обновления UI
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
        // Вызываем события для обновления UI
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

// Класс, представляющий собой один слот инвентаря.
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