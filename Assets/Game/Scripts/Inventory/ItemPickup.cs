using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Settings")]
    public ItemData itemData; // Перетащи сюда ItemData из Project!
    public int amount = 1;

    [Header("Interaction Settings")]
    public KeyCode interactKey = KeyCode.E;

    private bool playerInRange = false;

    void Start()
    {
        // Автоматически настраиваем спрайт, если он есть в ItemData
        if (itemData != null && itemData.icon != null)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = itemData.icon;
            }
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            TryPickupItem();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
        UIInteractPrompt.Instance.Show("Нажми E, чтобы поднять");
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Игрок отошел от предмета");
        }
        UIInteractPrompt.Instance.Hide();
    }

    void TryPickupItem()
    {
        if (itemData == null)
        {
            Debug.LogError("ItemData не назначен!");
            return;
        }

        if (InventoryManager.Instance.AddItem(itemData.itemID, amount))
        {
            Debug.Log($"Подобран предмет: {itemData.itemName} x{amount}");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Не удалось подобрать предмет - инвентарь полный!");
        }
    }
}