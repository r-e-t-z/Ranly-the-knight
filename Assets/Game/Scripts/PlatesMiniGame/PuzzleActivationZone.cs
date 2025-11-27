using UnityEngine;

public class PuzzleActivationZone : MonoBehaviour
{
    [Header("UI Prompt")]
    public GameObject pressEPrompt; // —сылка на GameObject текста на Canvas

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && pressEPrompt != null)
        {
            // ƒелаем текст видимым
            pressEPrompt.SetActive(true);
            Debug.Log("Press E prompt activated");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && pressEPrompt != null)
        {
            // ƒелаем текст невидимым
            pressEPrompt.SetActive(false);
            Debug.Log("Press E prompt deactivated");
        }
    }
}