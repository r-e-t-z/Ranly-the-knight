using UnityEngine;
using TMPro;

public class UIInteractPrompt : MonoBehaviour
{
    public static UIInteractPrompt Instance;
    public TMP_Text promptText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Если этот скрипт часть [GAME_SYSTEMS], он не должен удаляться
            if (transform.parent == null) DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    public void Show(string text)
    {
        // ПРОВЕРКА: если текст был удален при смене сцены, ничего не делаем
        if (promptText == null) return;

        promptText.text = text;
        promptText.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (promptText == null) return;
        promptText.gameObject.SetActive(false);
    }
}