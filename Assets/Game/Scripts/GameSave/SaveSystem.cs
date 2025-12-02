using UnityEngine;
using System.IO;
using System;

[Serializable]
public class SaveData
{
    public float playerX;
    public float playerY;
    public float playerZ;
    public bool hasSaveData;

    public SaveData(Vector3 playerPosition)
    {
        playerX = playerPosition.x;
        playerY = playerPosition.y;
        playerZ = playerPosition.z;
        hasSaveData = true;
    }

    public Vector3 GetPlayerPosition()
    {
        return new Vector3(playerX, playerY, playerZ);
    }
}

public static class SaveSystem
{
    private static string savePath;
    private const string SAVE_FILE_NAME = "gamesave.json";

    static SaveSystem()
    {
        savePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
    }

    public static void SaveGame(Vector3 playerPosition)
    {
        SaveData saveData = new SaveData(playerPosition);
        string json = JsonUtility.ToJson(saveData);

        try
        {
            File.WriteAllText(savePath, json);
            Debug.Log("Игра сохранена: " + savePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Ошибка сохранения: " + e.Message);
        }
    }

    public static SaveData LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("Сохранение не найдено");
            return null;
        }

        try
        {
            string json = File.ReadAllText(savePath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);
            Debug.Log("Игра загружена");
            return saveData;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Ошибка загрузки: " + e.Message);
            return null;
        }
    }

    public static bool HasSaveData()
    {
        if (!File.Exists(savePath)) return false;

        try
        {
            string json = File.ReadAllText(savePath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);
            return saveData != null && saveData.hasSaveData;
        }
        catch
        {
            return false;
        }
    }

    public static void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }
    }
}