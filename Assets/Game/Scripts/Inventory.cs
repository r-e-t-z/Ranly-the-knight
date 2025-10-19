using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    [Header("��������� ���������")]
    public int maxSlots = 12;

    [Header("������ ���������")]
    public List<InventoryItem> items = new List<InventoryItem>();

    [Header("UI ��������")]
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
        // ����� ��� ����� �������������
        slots = inventoryPanel.GetComponentsInChildren<InventorySlotUI>();

        // ������������� ��������� ��� �����
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

    // �������� �������
    public void AddItem(string itemName, Sprite itemIcon = null)
    {
        if (items.Count < maxSlots)
        {
            InventoryItem newItem = new InventoryItem();
            newItem.name = itemName;
            newItem.icon = itemIcon;
            items.Add(newItem);
            UpdateUI();
            Debug.Log("��������: " + itemName);
        }
        else
        {
            Debug.Log("��������� �����!");
        }
    }

    // �������� UI
    void UpdateUI()
    {
        // �������� ��� �����
        foreach (InventorySlotUI slot in slots)
        {
            slot.ClearSlot();
        }

        // ��������� ����� ����������
        for (int i = 0; i < items.Count; i++)
        {
            if (i < slots.Length)
            {
                slots[i].SetItem(items[i]);
            }
        }
    }
}