using UnityEngine;

public class GameLoader : MonoBehaviour
{
    [Header("Настройки загрузки")]
    public Transform defaultSpawnPoint;

    void Start()
    {
        LoadGameState();
    }

    void LoadGameState()
    {
        SaveData saveData = SaveSystem.LoadGame();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Игрок не найден на сцене!");
            return;
        }

        Vector3 spawnPosition;

        if (saveData != null && saveData.hasSaveData)
        {
            spawnPosition = saveData.GetPlayerPosition();
            Debug.Log("Загрузка сохраненной позиции: " + spawnPosition);
        }
        else
        {
            spawnPosition = defaultSpawnPoint != null ? defaultSpawnPoint.position : Vector3.zero;
            Debug.Log("Загрузка позиции по умолчанию");
        }

        player.transform.position = spawnPosition;
    }
}