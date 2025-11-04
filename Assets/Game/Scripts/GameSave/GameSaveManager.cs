using UnityEngine;

public class GameSaveManager : MonoBehaviour
{
    [Header("Способы сохранения")]
    public bool enableQuickSaveKey = true;
    public KeyCode quickSaveKey = KeyCode.F5;

    [Header("UI Кнопка сохранения")]
    public bool enableSaveButton = true;

    [Header("Контрольные точки")]
    public bool enableCheckpoints = true;

    void Update()
    {
        // Быстрое сохранение по клавише
        if (enableQuickSaveKey && Input.GetKeyDown(quickSaveKey))
        {
            QuickSave();
        }
    }

    // Сохранение по кнопке в UI
    public void SaveFromButton()
    {
        if (enableSaveButton)
        {
            SaveGame();
            Debug.Log("Игра сохранена через кнопку!");
        }
    }

    // Быстрое сохранение
    public void QuickSave()
    {
        SaveGame();
        Debug.Log("Быстрое сохранение выполнено!");
    }

    // Основной метод сохранения
    private void SaveGame()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            SaveSystem.SaveGame(player.transform.position);
        }
        else
        {
            Debug.LogWarning("Игрок не найден для сохранения!");
        }
    }
}