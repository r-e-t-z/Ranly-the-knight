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
            return;
        }

        Vector3 spawnPosition;

        if (saveData != null && saveData.hasSaveData)
        {
            spawnPosition = saveData.GetPlayerPosition();
        }
        else
        {
            spawnPosition = defaultSpawnPoint != null ? defaultSpawnPoint.position : Vector3.zero;
        }

        player.transform.position = spawnPosition;
    }
}