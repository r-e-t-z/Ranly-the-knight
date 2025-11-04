using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Настройки инвентаря")]
    public int maxSlots = 12;

    [Header("UI элементы InventoryPanel")]
    public GameObject inventoryPanel;

    private List<InventoryItem> items = new List<InventoryItem>();
    private InventorySlotUI[] slots;

    [System.Serializable]
    public class InventoryItem
    {
        public string name;
        public Sprite icon;
    }

    void Start()
    {
        if (inventoryPanel != null)
        {
            slots = inventoryPanel.GetComponentsInChildren<InventorySlotUI>();

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null)
                {
                    slots[i].index = i;
                    slots[i].inventory = this;
                }
            }

            inventoryPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    void ToggleInventory()
    {
        if (inventoryPanel == null)
        {
            return;
        }

        bool newState = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(newState);
    }

    public void AddItem(string itemName, Sprite itemIcon = null)
    {
        if (items.Count < maxSlots)
        {
            InventoryItem newItem = new InventoryItem();
            newItem.name = itemName;
            newItem.icon = itemIcon;
            items.Add(newItem);

            UpdateUI();
        }
    }

    void UpdateUI()
    {
        if (slots == null) return;

        foreach (InventorySlotUI slot in slots)
        {
            if (slot != null)
                slot.ClearSlot();
        }

        for (int i = 0; i < items.Count; i++)
        {
            if (i < slots.Length && slots[i] != null)
            {
                slots[i].SetItem(items[i]);
            }
        }
    }
}