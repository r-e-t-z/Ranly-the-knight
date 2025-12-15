using UnityEngine;

public class PuzzleActivationZone : MonoBehaviour
{
    [Header("UI Prompt")]
    public GameObject pressEPrompt;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && pressEPrompt != null)
            pressEPrompt.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && pressEPrompt != null)
            pressEPrompt.SetActive(false);
    }

    // НОВЫЙ МЕТОД: Полное отключение
    public void DisableZone()
    {
        if (pressEPrompt != null) pressEPrompt.SetActive(false); // Скрываем текст
        gameObject.SetActive(false); // Выключаем весь объект (камень перестанет быть триггером)
        // ИЛИ: this.enabled = false; GetComponent<Collider2D>().enabled = false; (если хочешь оставить камень видимым, но неактивным)
    }
}