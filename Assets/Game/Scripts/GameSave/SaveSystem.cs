using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public string sceneName;

    // Координаты (названия теперь фиксированные для всех скриптов)
    public float playerX;
    public float playerY;
    public float playerZ;

    // Инвентарь
    public List<string> invIDs = new List<string>();
    public List<int> invCounts = new List<int>();
    public string activeID;
    public int activeCount;

    // Квесты
    public List<string> activeQuests = new List<string>();
    public List<string> completedQuests = new List<string>();

    // Состояние мира (мини-игры, поднятые предметы)
    public List<string> worldEvents = new List<string>();

    // Диалоги Ink
    public List<string> inkKeys = new List<string>();
    public List<string> inkValues = new List<string>();

    public bool hasSaveData = false;

    // Помощник для получения Vector3 (нужен для GameLoader)
    public Vector3 GetPlayerPosition()
    {
        return new Vector3(playerX, playerY, playerZ);
    }
}

public static class SaveSystem
{
    private static string path = Path.Combine(Application.persistentDataPath, "gamesave.json");

    // Сохранение полного объекта данных
    public static void Save(SaveData data)
    {
        data.hasSaveData = true;
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
    }

    // Сохранение только позиции (для Чекпоинтов)
    public static void SaveGame(Vector3 position)
    {
        SaveData data = LoadGame();
        if (data == null) data = new SaveData();

        data.playerX = position.x;
        data.playerY = position.y;
        data.playerZ = position.z;
        data.hasSaveData = true;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
    }

    // Загрузка данных
    public static SaveData LoadGame()
    {
        if (!File.Exists(path)) return null;
        try
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch
        {
            return null;
        }
    }

    public static bool HasSaveData() => File.Exists(path);
    public static void DeleteSave() { if (File.Exists(path)) File.Delete(path); }
}