using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Settings")]
    public ItemData itemData;
    public int amount = 1;

    [Header("Interaction Settings")]
    public KeyCode interactKey = KeyCode.E;

    private bool playerInRange = false;

    void Start()
    {
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
        Debug.Log("Попытка подобрать: " + itemData.itemID);
        if (itemData == null)
        {
            return;
        }

        if (InventoryManager.Instance.AddItem(itemData.itemID, amount))
        {
            Destroy(gameObject);
        }
    }
}