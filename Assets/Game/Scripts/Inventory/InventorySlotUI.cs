using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDropHandler, IDragHandler
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMPro.TextMeshProUGUI stackCountText;

    public int SlotIndex;
    public bool isActiveItemSlot = false;

    private InventorySlot assignedSlot;
    private GameObject dragObject;
    private float lastClickTime;
    private const float doubleClickThreshold = 0.3f;

    private void Start()
    {
        InventoryManager.OnInventoryChanged += UpdateSlotUI;
        InventoryManager.OnActiveItemChanged += UpdateActiveSlotUI;

        if (!isActiveItemSlot)
        {
            assignedSlot = InventoryManager.Instance.inventorySlots[SlotIndex];
        }
        else
        {
            assignedSlot = InventoryManager.Instance.activeItemSlot;
            // Активный слот должен быть всегда видим, управляем через скрипт
            UpdateActiveSlotVisibility();
        }
        UpdateSlotDisplay();
    }

    private void OnDestroy()
    {
        InventoryManager.OnInventoryChanged -= UpdateSlotUI;
        InventoryManager.OnActiveItemChanged -= UpdateActiveSlotUI;
    }

    private void UpdateSlotUI(InventorySlot[] inventory)
    {
        if (!isActiveItemSlot)
        {
            UpdateSlotDisplay();
        }
    }

    private void UpdateActiveSlotUI(InventoryItem activeItem)
    {
        if (isActiveItemSlot)
        {
            UpdateSlotDisplay();
            UpdateActiveSlotVisibility();
        }
    }

    private void UpdateSlotDisplay()
    {
        if (assignedSlot.HasItem())
        {
            itemIcon.sprite = assignedSlot.Item.data.icon;
            itemIcon.color = Color.white;
            stackCountText.text = assignedSlot.Item.stackSize > 1 ? assignedSlot.Item.stackSize.ToString() : "";
        }
        else
        {
            itemIcon.sprite = null;
            itemIcon.color = Color.clear;
            stackCountText.text = "";
        }
    }

    // Управление видимостью активного слота
    private void UpdateActiveSlotVisibility()
    {
        if (isActiveItemSlot)
        {
            // Делаем весь слот активным/неактивным в зависимости от наличия предмета
            gameObject.SetActive(assignedSlot.HasItem());
        }
    }

    // Обработка двойного клика
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            float timeSinceLastClick = Time.time - lastClickTime;

            if (timeSinceLastClick <= doubleClickThreshold)
            {
                // Двойной клик
                if (!isActiveItemSlot && assignedSlot.HasItem())
                {
                    // Двойной клик по предмету в инвентаре - перемещаем в активный слот
                    InventoryManager.Instance.MoveToActiveSlot(SlotIndex);
                }
                else if (isActiveItemSlot && assignedSlot.HasItem())
                {
                    // Двойной клик по активному предмету - возвращаем в инвентарь
                    InventoryManager.Instance.MoveToInventoryFromActive();
                }
            }

            lastClickTime = Time.time;
        }
    }

    // Остальные методы перетаскивания остаются без изменений
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!assignedSlot.HasItem() || isActiveItemSlot) return;

        dragObject = new GameObject("DragIcon");
        dragObject.transform.SetParent(transform.root, false);
        dragObject.transform.SetAsLastSibling();

        Image dragImage = dragObject.AddComponent<Image>();
        dragImage.sprite = itemIcon.sprite;
        dragImage.raycastTarget = false;

        CanvasGroup canvasGroup = dragObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0.7f;
        canvasGroup.blocksRaycasts = false;

        itemIcon.color = new Color(1, 1, 1, 0.3f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragObject != null)
        {
            dragObject.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragObject != null)
        {
            Destroy(dragObject);
            dragObject = null;
        }

        if (assignedSlot.HasItem())
        {
            itemIcon.color = Color.white;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObject = eventData.pointerDrag;
        if (droppedObject == null) return;

        InventorySlotUI fromSlotUI = droppedObject.GetComponent<InventorySlotUI>();
        if (fromSlotUI == null || fromSlotUI.isActiveItemSlot) return;

        int fromIndex = fromSlotUI.SlotIndex;
        int toIndex = SlotIndex;

        if (fromIndex != toIndex)
        {
            InventoryManager.Instance.TryCraftItems(fromIndex, toIndex);
        }
    }
}