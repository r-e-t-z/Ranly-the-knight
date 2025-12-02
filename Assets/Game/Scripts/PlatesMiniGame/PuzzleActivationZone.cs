using UnityEngine;

public class PuzzleActivationZone : MonoBehaviour
{
    [Header("UI Prompt")]
    public GameObject pressEPrompt;

    

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && pressEPrompt != null)
        {
            pressEPrompt.SetActive(true);
        }

        
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && pressEPrompt != null)
        {
            pressEPrompt.SetActive(false);
        }
    }
}