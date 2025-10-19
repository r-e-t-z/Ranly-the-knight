using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    public Image itemIcon;
    [HideInInspector] public int index;
    [HideInInspector] public Inventory inventory;

    void Start()
    {
        // Создаем Image для иконки если его нет
        if (itemIcon == null)
        {
            GameObject iconObject = new GameObject("ItemIcon");
            iconObject.transform.SetParent(transform);
            iconObject.transform.localPosition = Vector3.zero;
            iconObject.transform.localScale = Vector3.one;

            itemIcon = iconObject.AddComponent<Image>();
            itemIcon.rectTransform.sizeDelta = new Vector2(60, 60);
        }
    }

    public void SetItem(Inventory.InventoryItem item)
    {
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
        itemIcon.sprite = null;
        itemIcon.enabled = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log("Использован предмет: " + index);
        }
    }
}