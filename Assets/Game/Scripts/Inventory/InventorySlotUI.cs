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
    private CanvasGroup canvasGroup; // Компонент для управления видимостью

    private void Awake()
    {
        // Ищем CanvasGroup на этом же объекте
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        InventoryManager.OnInventoryChanged += UpdateSlotUI;
        InventoryManager.OnActiveItemChanged += UpdateActiveSlotUI;

        AssignSlot();
    }

    private void OnDestroy()
    {
        InventoryManager.OnInventoryChanged -= UpdateSlotUI;
        InventoryManager.OnActiveItemChanged -= UpdateActiveSlotUI;
    }

    private void AssignSlot()
    {
        if (InventoryManager.Instance == null) return;

        if (!isActiveItemSlot)
        {
            if (InventoryManager.Instance.inventorySlots != null && SlotIndex < InventoryManager.Instance.inventorySlots.Length)
            {
                assignedSlot = InventoryManager.Instance.inventorySlots[SlotIndex];
            }
        }
        else
        {
            assignedSlot = InventoryManager.Instance.activeItemSlot;
        }

        UpdateSlotDisplay();
    }

    private void UpdateSlotUI(InventorySlot[] inventory)
    {
        if (!isActiveItemSlot) UpdateSlotDisplay();
    }

    private void UpdateActiveSlotUI(InventoryItem activeItem)
    {
        if (isActiveItemSlot) UpdateSlotDisplay();
    }

    private void UpdateSlotDisplay()
    {
        bool hasItem = assignedSlot != null && assignedSlot.HasItem();

        // ЛОГИКА ДЛЯ АКТИВНОГО СЛОТА: Скрываем/Показываем целиком
        if (isActiveItemSlot && canvasGroup != null)
        {
            canvasGroup.alpha = hasItem ? 1f : 0f; // 1 = видно, 0 = невидимо
            canvasGroup.blocksRaycasts = hasItem; // Чтобы нельзя было нажать на невидимый слот
        }

        // ОБНОВЛЕНИЕ ИКОНКИ И ТЕКСТА
        if (itemIcon == null) return;

        if (hasItem)
        {
            itemIcon.sprite = assignedSlot.Item.data.icon;
            itemIcon.color = Color.white;
            itemIcon.enabled = true;

            if (stackCountText != null)
            {
                stackCountText.text = assignedSlot.Item.stackSize > 1 ? assignedSlot.Item.stackSize.ToString() : "";
                stackCountText.enabled = true;
            }
        }
        else
        {
            itemIcon.sprite = null;
            itemIcon.color = Color.clear;
            itemIcon.enabled = false;

            if (stackCountText != null) stackCountText.text = "";

            // Если это обычный слот и нет CanvasGroup, он просто покажет пустую рамку
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (assignedSlot == null || !assignedSlot.HasItem()) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (Time.time - lastClickTime <= 0.3f)
            {
                if (!isActiveItemSlot)
                    InventoryManager.Instance.MoveToActiveSlot(SlotIndex);
                else
                    InventoryManager.Instance.MoveToInventoryFromActive();
            }
            lastClickTime = Time.time;
        }
    }

    // --- Drag n Drop (без изменений) ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (assignedSlot == null || !assignedSlot.HasItem() || isActiveItemSlot) return;
        dragObject = new GameObject("DragIcon");
        dragObject.transform.SetParent(transform.root, false);
        dragObject.transform.SetAsLastSibling();
        Image img = dragObject.AddComponent<Image>();
        img.sprite = itemIcon.sprite;
        img.raycastTarget = false;
        itemIcon.color = new Color(1, 1, 1, 0.3f);
    }

    public void OnDrag(PointerEventData eventData) { if (dragObject != null) dragObject.transform.position = eventData.position; }

    public void OnEndDrag(PointerEventData eventData) { if (dragObject != null) Destroy(dragObject); if (assignedSlot != null && assignedSlot.HasItem()) itemIcon.color = Color.white; }

    public void OnDrop(PointerEventData eventData)
    {
        var from = eventData.pointerDrag?.GetComponent<InventorySlotUI>();
        if (from != null && from != this && !from.isActiveItemSlot && !this.isActiveItemSlot)
            InventoryManager.Instance.TryCraftItems(from.SlotIndex, this.SlotIndex);
    }
}