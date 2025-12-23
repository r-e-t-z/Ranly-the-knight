using System.Xml;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Unique Settings")]
    public string uniqueID;

    [Header("Item Settings")]
    public ItemData itemData;
    public int amount = 1;

    [Header("Interaction Settings")]
    public KeyCode interactKey = KeyCode.E;

    private bool playerInRange = false;

    void Awake()
    {
        if (GameSaveManager.Instance.IsEventDone(uniqueID))
        {
            Destroy(gameObject);
        }
    }

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
        if (InventoryManager.Instance.AddItem(itemData.itemID, amount))
        {
            GameSaveManager.Instance.RegisterEvent(uniqueID); 
            Destroy(gameObject);
        }

        Debug.Log("Попытка подобрать: " + itemData.itemID);
        if (itemData == null)
        {
            return;
        }
    }
}