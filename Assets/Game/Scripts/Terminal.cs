using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Terminal : MonoBehaviour
{
    [Tooltip("Èìÿ")]
    public string terminalName = "Terminal";

    private bool playerInRange = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            FastTravelUI.Instance.OpenMenu(this);
        }
    }
}
