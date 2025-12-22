using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    // События, на которые подписываются ячейки UI
    public static event Action<InventorySlot[]> OnInventoryChanged;
    public static event Action<InventoryItem> OnActiveItemChanged;

    [Header("Настройки инвентаря")]
    [SerializeField] private int inventorySize = 9;
    public InventorySlot[] inventorySlots;
    public InventorySlot activeItemSlot;

    [Header("Ссылки")]
    [SerializeField] private ItemDBSO itemDatabase;
    public GameObject inventoryPanel;

    private MonoBehaviour playerController;

    private void Awake()
    {
        // Паттерн Singleton + DontDestroyOnLoad
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Важно: инициализируем данные только один раз при старте игры
            InitializeInventory();
        }
        else
        {
            // Если мы перешли на сцену, где уже есть менеджер, удаляем дубликат
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Вызывается автоматически при каждой смене сцены
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshReferences();
    }

    private void RefreshReferences()
    {
        // Ищем игрока на новой сцене
        playerController = FindObjectOfType<PlayerMovement>();

        // Пытаемся найти панель инвентаря на новой сцене, если старая потеряна
        if (inventoryPanel == null || !inventoryPanel.activeInHierarchy)
        {
            // Ищем объект по имени в новой сцене
            GameObject foundPanel = GameObject.Find("InventoryPanel");
            if (foundPanel != null) inventoryPanel = foundPanel;
        }

        // Принудительно обновляем UI (сообщаем новым ячейкам, что у нас лежит в памяти)
        Invoke("ForceInventoryUpdate", 0.1f);
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

    private void Update()
    {
        // Не открываем инвентарь во время диалогов
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying())
        {
            if (inventoryPanel != null && inventoryPanel.activeInHierarchy)
                inventoryPanel.SetActive(false);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            bool newState = !inventoryPanel.activeInHierarchy;
            inventoryPanel.SetActive(newState);

            // Блокируем движение игрока при открытом инвентаре
            if (playerController != null) playerController.enabled = !newState;
        }
    }

    public bool AddItem(string itemID, int amount = 1)
    {
        if (itemDatabase == null)
        {
            Debug.LogError("InventoryManager: Нет ссылки на Item Database!");
            return false;
        }

        ItemData itemToAdd = itemDatabase.GetItemByID(itemID);
        if (itemToAdd == null)
        {
            Debug.LogError($"InventoryManager: Предмет {itemID} не найден в БД!");
            return false;
        }

        // 1. Проверяем активный слот (если там такой же стакающийся предмет)
        if (activeItemSlot.HasItem() && activeItemSlot.Item.data.itemID == itemID && itemToAdd.isStackable)
        {
            if (activeItemSlot.Item.AddToStack(amount))
            {
                ForceInventoryUpdate();
                return true;
            }
        }

        // 2. Ищем такой же предмет в инвентаре для стака
        if (itemToAdd.isStackable)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i].HasItem() && inventorySlots[i].Item.data.itemID == itemID)
                {
                    if (inventorySlots[i].Item.AddToStack(amount))
                    {
                        ForceInventoryUpdate();
                        return true;
                    }
                }
            }
        }

        // 3. Ищем пустую ячейку
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (!inventorySlots[i].HasItem())
            {
                inventorySlots[i].SetItem(new InventoryItem(itemToAdd, amount));
                ForceInventoryUpdate();
                return true;
            }
        }

        Debug.Log("Inventory is full!");
        return false;
    }

    // --- ЛОГИКА ПЕРЕМЕЩЕНИЯ ---

    public void MoveToActiveSlot(int fromSlotIndex)
    {
        if (fromSlotIndex < 0 || fromSlotIndex >= inventorySlots.Length) return;
        if (!inventorySlots[fromSlotIndex].HasItem()) return;

        if (activeItemSlot.HasItem())
        {
            // Если в активном слоте уже что-то есть, пробуем поменять местами или вернуть в инвентарь
            InventoryItem temp = activeItemSlot.Item;
            activeItemSlot.SetItem(inventorySlots[fromSlotIndex].Item);
            inventorySlots[fromSlotIndex].SetItem(temp);
        }
        else
        {
            activeItemSlot.SetItem(inventorySlots[fromSlotIndex].Item);
            inventorySlots[fromSlotIndex].ClearSlot();
        }

        ForceInventoryUpdate();
    }

    public void MoveToInventoryFromActive()
    {
        if (!activeItemSlot.HasItem()) return;

        int emptySlot = FindEmptySlot();
        if (emptySlot != -1)
        {
            inventorySlots[emptySlot].SetItem(activeItemSlot.Item);
            activeItemSlot.ClearSlot();
            ForceInventoryUpdate();
        }
    }

    private int FindEmptySlot()
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (!inventorySlots[i].HasItem()) return i;
        }
        return -1;
    }

    // --- КРАФТИНГ ---

    public void TryCraftItems(int fromSlotIndex, int toSlotIndex)
    {
        if (fromSlotIndex == toSlotIndex) return;

        InventoryItem itemA = inventorySlots[fromSlotIndex].Item;
        InventoryItem itemB = inventorySlots[toSlotIndex].Item;

        if (itemA == null || itemB == null) return;

        ItemData resultData = FindCraftingResult(itemA.data, itemB.data);

        if (resultData != null)
        {
            RemoveItemFromSlot(fromSlotIndex);
            RemoveItemFromSlot(toSlotIndex);
            AddItem(resultData.itemID);
        }
    }

    private ItemData FindCraftingResult(ItemData ing1, ItemData ing2)
    {
        foreach (ItemData res in itemDatabase.allItems)
        {
            if (res.craftingRecipes == null) continue;
            foreach (var recipe in res.craftingRecipes)
            {
                if ((recipe.item1 == ing1 && recipe.item2 == ing2) ||
                    (recipe.item1 == ing2 && recipe.item2 == ing1))
                    return res;
            }
        }
        return null;
    }

    // --- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ---

    public bool RemoveItemFromActiveSlot(string itemID, int amount)
    {
        if (activeItemSlot.HasItem() && activeItemSlot.Item.data.itemID == itemID)
        {
            if (activeItemSlot.Item.stackSize >= amount)
            {
                activeItemSlot.Item.stackSize -= amount;
                if (activeItemSlot.Item.stackSize <= 0) activeItemSlot.ClearSlot();
                ForceInventoryUpdate();
                return true;
            }
        }
        return false;
    }

    public void RemoveItemFromSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < inventorySlots.Length)
        {
            inventorySlots[slotIndex].ClearSlot();
            ForceInventoryUpdate();
        }
    }

    public int GetItemCount(string itemID)
    {
        int count = GetActiveSlotItemCount(itemID);
        foreach (var slot in inventorySlots)
        {
            if (slot.HasItem() && slot.Item.data.itemID == itemID)
                count += slot.Item.stackSize;
        }
        return count;
    }

    public int GetActiveSlotItemCount(string itemID)
    {
        if (activeItemSlot != null && activeItemSlot.HasItem() && activeItemSlot.Item.data.itemID == itemID)
            return activeItemSlot.Item.stackSize;
        return 0;
    }

    public void ForceInventoryUpdate()
    {
        OnInventoryChanged?.Invoke(inventorySlots);
        OnActiveItemChanged?.Invoke(activeItemSlot != null ? activeItemSlot.Item : null);
    }
}

// --- КЛАССЫ ДАННЫХ ---

[System.Serializable]
public class InventorySlot
{
    public InventoryItem Item;
    public bool HasItem() => Item != null && Item.data != null;
    public void SetItem(InventoryItem newItem) => Item = newItem;
    public void ClearSlot() => Item = null;
}