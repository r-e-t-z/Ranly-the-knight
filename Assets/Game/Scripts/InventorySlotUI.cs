using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI элементы")]
    public Image itemIcon;

    [HideInInspector]
    public int index;
    [HideInInspector]
    public Inventory inventory;

    void Start()
    {
        if (itemIcon == null)
        {
            itemIcon = GetComponentInChildren<Image>();

        }
    }

    public void SetItem(Inventory.InventoryItem item)
    {
        if (itemIcon == null)
        {
            Debug.LogError("ItemIcon is null в слоте: " + gameObject.name);
            return;
        }

        if (item != null && item.icon != null)
        {
            itemIcon.sprite = item.icon;
            itemIcon.enabled = true;
        }
        else
        {
            ClearSlot();
        }
    }

    public void ClearSlot()
    {
        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log("Клик по слоту: " + index);
        }
    }
}