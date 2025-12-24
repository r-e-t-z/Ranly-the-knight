using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public static event Action<InventorySlot[]> OnInventoryChanged;
    public static event Action<InventoryItem> OnActiveItemChanged;

    [Header("Настройки")]
    [SerializeField] private int inventorySize = 9;
    public InventorySlot[] inventorySlots;
    public InventorySlot activeItemSlot;

    [Header("Ссылки")]
    [SerializeField] private ItemDBSO itemDatabase;
    public GameObject inventoryPanel;

    private MonoBehaviour playerController;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeInventory();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => RefreshReferences();

    private void RefreshReferences()
    {
        playerController = FindObjectOfType<PlayerMovement>();
        if (inventoryPanel == null || !inventoryPanel.activeInHierarchy)
        {
            GameObject found = GameObject.Find("InventoryPanel");
            if (found != null) inventoryPanel = found;
        }
        Invoke("ForceInventoryUpdate", 0.1f);
    }

    private void InitializeInventory()
    {
        // Если массив не создан или его размер изменился в инспекторе - пересоздаем
        if (inventorySlots == null || inventorySlots.Length != inventorySize)
        {
            inventorySlots = new InventorySlot[inventorySize];
            for (int i = 0; i < inventorySlots.Length; i++)
                inventorySlots[i] = new InventorySlot();
        }

        // Проверка на случай если объекты внутри массива пустые
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] == null) inventorySlots[i] = new InventorySlot();
        }

        if (activeItemSlot == null) activeItemSlot = new InventorySlot();
    }

    private void Update()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying())
        {
            if (inventoryPanel != null && inventoryPanel.activeInHierarchy) inventoryPanel.SetActive(false);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab)) ToggleInventory();
    }

    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            bool newState = !inventoryPanel.activeInHierarchy;
            inventoryPanel.SetActive(newState);
            if (playerController != null) playerController.enabled = !newState;
        }
    }

    public bool AddItem(string itemID, int amount = 1)
    {
        ItemData itemToAdd = itemDatabase.GetItemByID(itemID);
        if (itemToAdd == null) return false;

        // 1. Стакаем в руках
        if (activeItemSlot.HasItem() && activeItemSlot.Item.data.itemID == itemID && itemToAdd.isStackable)
        {
            activeItemSlot.Item.AddToStack(amount);
            ForceInventoryUpdate();
            return true;
        }

        // 2. Стакаем в инвентаре
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

        // 3. Ищем ПЕРВЫЙ ПУСТОЙ слот
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (!inventorySlots[i].HasItem())
            {
                inventorySlots[i].SetItem(new InventoryItem(itemToAdd, amount));
                ForceInventoryUpdate();
                return true;
            }
        }

        Debug.Log($"Инвентарь полон! Проверено слотов: {inventorySlots.Length}");
        return false;
    }

    public void TryCraftItems(int fromSlotIndex, int toSlotIndex)
    {
        if (fromSlotIndex == toSlotIndex) return;

        InventoryItem itemA = inventorySlots[fromSlotIndex].Item;
        InventoryItem itemB = inventorySlots[toSlotIndex].Item;

        if (itemA == null || itemB == null) return;

        ItemData resultItem = FindCraftingResult(itemA.data, itemB.data);
        if (resultItem != null)
        {
            inventorySlots[fromSlotIndex].ClearSlot();
            inventorySlots[toSlotIndex].ClearSlot();
            AddItem(resultItem.itemID);
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
                        return potentialResult;
                    }
                }
            }
        }
        return null;
    }

    public void MoveToActiveSlot(int fromSlotIndex)
    {
        if (fromSlotIndex < 0 || fromSlotIndex >= inventorySlots.Length || !inventorySlots[fromSlotIndex].HasItem()) return;

        if (activeItemSlot.HasItem())
        {
            int emptySlot = FindEmptySlot();
            if (emptySlot != -1)
            {
                inventorySlots[emptySlot].SetItem(activeItemSlot.Item);
                activeItemSlot.ClearSlot();
            }
            else return;
        }

        activeItemSlot.SetItem(inventorySlots[fromSlotIndex].Item);
        inventorySlots[fromSlotIndex].ClearSlot();
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
            if (!inventorySlots[i].HasItem()) return i;
        return -1;
    }

    public void RemoveItemFromSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < inventorySlots.Length)
        {
            inventorySlots[slotIndex].ClearSlot();
            ForceInventoryUpdate();
        }
    }

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

    public void ClearAll()
    {
        foreach (var slot in inventorySlots) slot.ClearSlot();
        activeItemSlot.ClearSlot();
        ForceInventoryUpdate();
    }

    public void SetActiveItemFromSave(string id, int count)
    {
        ItemData data = itemDatabase.GetItemByID(id);
        if (data != null)
        {
            activeItemSlot.SetItem(new InventoryItem(data, count));
            ForceInventoryUpdate();
        }
    }

    public int GetFirstItemIndex(string id)
    {
        for (int i = 0; i < inventorySlots.Length; i++)
            if (inventorySlots[i].HasItem() && inventorySlots[i].Item.data.itemID == id) return i;
        return -1;
    }

    public int GetItemCount(string itemID)
    {
        int total = (activeItemSlot.HasItem() && activeItemSlot.Item.data.itemID == itemID) ? activeItemSlot.Item.stackSize : 0;
        foreach (var slot in inventorySlots)
            if (slot.HasItem() && slot.Item.data.itemID == itemID) total += slot.Item.stackSize;
        return total;
    }

    public int GetActiveSlotItemCount(string itemID) => (activeItemSlot.HasItem() && activeItemSlot.Item.data.itemID == itemID) ? activeItemSlot.Item.stackSize : 0;

    public void ForceInventoryUpdate()
    {
        OnInventoryChanged?.Invoke(inventorySlots);
        OnActiveItemChanged?.Invoke(activeItemSlot != null && activeItemSlot.HasItem() ? activeItemSlot.Item : null);
    }
}

// --- КЛАССЫ ДАННЫХ (ОБЯЗАТЕЛЬНО ДОЛЖНЫ БЫТЬ ТУТ) ---

[System.Serializable]
public class InventorySlot
{
    public InventoryItem Item;
    public bool HasItem() => Item != null && Item.data != null;
    public void SetItem(InventoryItem newItem) => Item = newItem;
    public void ClearSlot() => Item = null;
}