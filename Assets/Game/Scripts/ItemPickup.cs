using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Настройки предмета")]
    public string itemName;
    public Sprite itemIcon; // Добавили иконку!
    public KeyCode pickupKey = KeyCode.E;

    [Header("Визуал")]
    public SpriteRenderer sprite;
    public GameObject pickupHint;

    private bool playerInRange = false;
    private Inventory playerInventory;

    void Start()
    {
        if (pickupHint != null)
            pickupHint.SetActive(false);

        playerInventory = FindObjectOfType<Inventory>();

        // Автоматически назначить иконку из спрайта
        if (itemIcon == null && sprite != null)
        {
            itemIcon = sprite.sprite;
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(pickupKey))
        {
            PickupItem();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (pickupHint != null)
                pickupHint.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (pickupHint != null)
                pickupHint.SetActive(false);
        }
    }

    void PickupItem()
    {
        if (playerInventory != null)
        {
            // Теперь передаем и иконку!
            playerInventory.AddItem(itemName, itemIcon);
            Destroy(gameObject);
        }
    }
}