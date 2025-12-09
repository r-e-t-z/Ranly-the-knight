using UnityEngine;

public class GameSaveManager : MonoBehaviour
{
    [Header("Способы сохранения")]
    public bool enableQuickSaveKey = true;

    [Header("UI Кнопка сохранения")]
    public bool enableSaveButton = true;

    [Header("Контрольные точки")]
    public bool enableCheckpoints = true;

    public void SaveFromButton()
    {
        if (enableSaveButton)
        {
            SaveGame();
        }
    }

    public void QuickSave()
    {
        SaveGame();
    }

    private void SaveGame()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            SaveSystem.SaveGame(player.transform.position);
        }
    }
}