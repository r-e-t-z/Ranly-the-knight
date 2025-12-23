using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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
        if (inventorySlots == null || inventorySlots.Length == 0)
        {
            inventorySlots = new InventorySlot[inventorySize];
            for (int i = 0; i < inventorySlots.Length; i++)
                inventorySlots[i] = new InventorySlot();
        }
        if (activeItemSlot == null) activeItemSlot = new InventorySlot();
    }

    private void Update()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying())
        {
            if (inventoryPanel != null && inventoryPanel.activeInHierarchy)
                inventoryPanel.SetActive(false);
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

        // 1. Ñíà÷àëà ïðîáóåì ñòàêíóòü â ÀÊÒÈÂÍÛÉ ñëîò (òâîÿ ëîãèêà)
        if (activeItemSlot.HasItem() && activeItemSlot.Item.data.itemID == itemID && itemToAdd.isStackable)
        {
            activeItemSlot.Item.AddToStack(amount);
            ForceInventoryUpdate();
            return true;
        }
        else
        {
            // 2. Ïðîáóåì ñòàêíóòü â èíâåíòàðå
            if (itemToAdd.isStackable)
            {
                for (int i = 0; i < inventorySlots.Length; i++)
                {
                    if (inventorySlots[i].HasItem() && inventorySlots[i].Item.data == itemToAdd)
                    {
                        if (inventorySlots[i].Item.AddToStack(amount))
                        {
                            ForceInventoryUpdate();
                            return true;
                        }
                    }
                }
            }

            // 3. Èùåì ïóñòîé ñëîò
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (!inventorySlots[i].HasItem())
                {
                    inventorySlots[i].SetItem(new InventoryItem(itemToAdd, amount));
                    ForceInventoryUpdate();
                    return true;
                }
            }
        }
        return false;
    }

    public void TryCraftItems(int from, int to)
    {
        if (from == to) return;
        InventoryItem itemA = inventorySlots[from].Item;
        InventoryItem itemB = inventorySlots[to].Item;
        if (itemA == null || itemB == null) return;

        foreach (ItemData res in itemDatabase.allItems)
        {
            if (res.craftingRecipes == null) continue;
            foreach (var recipe in res.craftingRecipes)
            {
                if ((recipe.item1 == itemA.data && recipe.item2 == itemB.data) ||
                    (recipe.item1 == itemB.data && recipe.item2 == itemA.data))
                {
                    inventorySlots[from].ClearSlot();
                    inventorySlots[to].ClearSlot();
                    AddItem(res.itemID);
                    return;
                }
            }
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
        foreach (var slot in inventorySlots) if (slot.HasItem() && slot.Item.data.itemID == itemID) total += slot.Item.stackSize;
        return total;
    }

    public int GetActiveSlotItemCount(string itemID)
    {
        if (activeItemSlot.HasItem() && activeItemSlot.Item.data.itemID == itemID) return activeItemSlot.Item.stackSize;
        return 0;
    }

    public void ForceInventoryUpdate()
    {
        OnInventoryChanged?.Invoke(inventorySlots);
        OnActiveItemChanged?.Invoke(activeItemSlot != null && activeItemSlot.HasItem() ? activeItemSlot.Item : null);
    }
}

// --- ÊËÀÑÑÛ ÄÀÍÍÛÕ (ÎÁßÇÀÒÅËÜÍÎ ÄÎËÆÍÛ ÁÛÒÜ ÒÓÒ) ---

[System.Serializable]
public class InventorySlot
{
    public InventoryItem Item;
    public bool HasItem() => Item != null && Item.data != null;
    public void SetItem(InventoryItem newItem) => Item = newItem;
    public void ClearSlot() => Item = null;
}