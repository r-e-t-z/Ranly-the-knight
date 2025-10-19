using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    [Header("Настройки инвентаря")]
    public int maxSlots = 12;

    [Header("Список предметов")]
    public List<InventoryItem> items = new List<InventoryItem>();

    [Header("UI элементы")]
    public GameObject inventoryPanel;
    public InventorySlotUI[] slots;

    [System.Serializable]
    public class InventoryItem
    {
        public string name;
        public Sprite icon;
    }

    void Start()
    {
        // Найти все слоты автоматически
        slots = inventoryPanel.GetComponentsInChildren<InventorySlotUI>();

        // Автоматически настроить все слоты
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].index = i;
            slots[i].inventory = this;
        }

        inventoryPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        }
    }

    // Добавить предмет
    public void AddItem(string itemName, Sprite itemIcon = null)
    {
        if (items.Count < maxSlots)
        {
            InventoryItem newItem = new InventoryItem();
            newItem.name = itemName;
            newItem.icon = itemIcon;
            items.Add(newItem);
            UpdateUI();
            Debug.Log("Добавлен: " + itemName);
        }
        else
        {
            Debug.Log("Инвентарь полон!");
        }
    }

    // Обновить UI
    void UpdateUI()
    {
        // Очистить все слоты
        foreach (InventorySlotUI slot in slots)
        {
            slot.ClearSlot();
        }

        // Заполнить слоты предметами
        for (int i = 0; i < items.Count; i++)
        {
            if (i < slots.Length)
            {
                slots[i].SetItem(items[i]);
            }
        }
    }
}