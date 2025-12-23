using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameSaveManager : MonoBehaviour
{
    public static GameSaveManager Instance;

    [Header("Настройки систем")]
    [Tooltip("Префаб, содержащий Игрока, Камеру, Канвасы и Менеджеры")]
    public GameObject gameSystemsPrefab;

    private HashSet<string> worldEvents = new HashSet<string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameSaveManager инициализирован.");
        }
        else if (Instance != this)
        {
            Debug.Log("Дубликат GameSaveManager удален.");
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Быстрое сохранение на F5
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Debug.Log("Нажата клавиша F5 для сохранения...");
            SaveGame();
        }

        // Быстрая загрузка на F9 (для теста)
        if (Input.GetKeyDown(KeyCode.F9))
        {
            Debug.Log("Нажата клавиша F9 для загрузки...");
            LoadGame();
        }
    }

    // РЕГИСТРАЦИЯ СОБЫТИЙ (вызывается из других скриптов)
    public void RegisterEvent(string id) => worldEvents.Add(id);
    public bool IsEventDone(string id) => worldEvents.Contains(id);

    // --- СОХРАНЕНИЕ ---
    public void SaveGame()
    {
        Debug.Log("--- ЗАПУСК СОХРАНЕНИЯ ---");
        try
        {
            SaveData data = new SaveData();

            // 1. Позиция
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                data.sceneName = SceneManager.GetActiveScene().name;
                data.playerX = player.transform.position.x;
                data.playerY = player.transform.position.y;
                data.playerZ = player.transform.position.z;
                Debug.Log("1. Позиция игрока записана.");
            }
            else
            {
                Debug.LogError("ОШИБКА: Игрок с тегом Player не найден на сцене!");
                return;
            }

            // 2. Инвентарь
            if (InventoryManager.Instance != null)
            {
                foreach (var slot in InventoryManager.Instance.inventorySlots)
                {
                    if (slot.HasItem())
                    {
                        data.invIDs.Add(slot.Item.data.itemID);
                        data.invCounts.Add(slot.Item.stackSize);
                    }
                }
                if (InventoryManager.Instance.activeItemSlot != null && InventoryManager.Instance.activeItemSlot.HasItem())
                {
                    data.activeID = InventoryManager.Instance.activeItemSlot.Item.data.itemID;
                    data.activeCount = InventoryManager.Instance.activeItemSlot.Item.stackSize;
                }
                Debug.Log("2. Инвентарь записан.");
            }

            // 3. Квесты
            if (QuestsManager.Instance != null)
            {
                foreach (var q in QuestsManager.Instance.allQuests)
                {
                    if (q.isCompleted) data.completedQuests.Add(q.id);
                    else data.activeQuests.Add(q.id);
                }
                Debug.Log("3. Квесты записаны.");
            }

            // 4. Мир
            data.worldEvents = new List<string>(worldEvents);
            Debug.Log($"4. События мира ({worldEvents.Count}) записаны.");

            // 5. Диалоги
            if (DialogueManager.Instance != null)
            {
                // Сначала заставляем DialogueManager забрать данные из Ink в словарь
                DialogueManager.Instance.SyncVariablesFromStory();

                var inkVars = DialogueManager.Instance.GetGlobalVariables();
                if (inkVars != null)
                {
                    foreach (var v in inkVars)
                    {
                        data.inkKeys.Add(v.Key);
                        // Сохраняем значение как строку (bool "True", int "5" и т.д.)
                        data.inkValues.Add(v.Value.ToString());
                    }
                    Debug.Log($"5. Сохранено {data.inkKeys.Count} переменных диалога.");
                }
            }

            // 6. ЗАПИСЬ НА ДИСК
            SaveSystem.Save(data);
            Debug.Log("--- ИГРА УСПЕШНО СОХРАНЕНА НА ДИСК ---");
        }
        catch (System.Exception e)
        {
            Debug.LogError("КРИТИЧЕСКАЯ ОШИБКА ПРИ СОХРАНЕНИИ: " + e.Message);
        }
    }

    // --- ЗАГРУЗКА ---
    public void LoadGame()
    {
        SaveData data = SaveSystem.LoadGame();
        if (data != null)
        {
            StartCoroutine(LoadRoutine(data));
        }
        else
        {
            Debug.LogError("Файл сохранения не найден!");
        }
    }

    private IEnumerator LoadRoutine(SaveData data)
    {
        Debug.Log("Загрузка сцены: " + data.sceneName);

        // 1. Загружаем нужную сцену
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(data.sceneName);
        while (!asyncLoad.isDone) yield return null;

        // Ждем завершения кадра, чтобы сцена прогрузилась
        yield return new WaitForEndOfFrame();

        // 2. Ищем игрока. Если его нет — создаем весь системный префаб
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            if (gameSystemsPrefab != null)
            {
                Debug.Log("Игрок не найден. Создаю системный префаб...");
                Instantiate(gameSystemsPrefab);
                player = GameObject.FindGameObjectWithTag("Player");
            }
            else
            {
                Debug.LogError("Критическая ошибка: В GameSaveManager не привязан префаб [GAME_SYSTEMS]!");
            }
        }

        // 3. Ставим игрока в сохраненную позицию
        if (player != null)
        {
            player.transform.position = new Vector3(data.playerX, data.playerY, data.playerZ);
        }

        // 4. Восстанавливаем мировые события (HashSet)
        worldEvents = new HashSet<string>(data.worldEvents);

        // Ждем один кадр, чтобы менеджеры из префаба успели вызвать свои Awake/Start
        yield return null;

        // 5. Восстанавливаем Инвентарь
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ClearAll();

            // Сначала обычные предметы
            for (int i = 0; i < data.invIDs.Count; i++)
            {
                InventoryManager.Instance.AddItem(data.invIDs[i], data.invCounts[i]);
            }

            // ЗАТЕМ активный слот через специальный метод
            if (!string.IsNullOrEmpty(data.activeID))
            {
                InventoryManager.Instance.SetActiveItemFromSave(data.activeID, data.activeCount);
            }

            InventoryManager.Instance.ForceInventoryUpdate();
        }

        // 6. Восстанавливаем Квесты (базовая логика)
        // Здесь можно добавить QuestsManager.Instance.RestoreQuests(data.activeQuests, data.completedQuests)

        // 7. Восстанавливаем переменные диалогов Ink
        if (DialogueManager.Instance != null)
        {
            for (int i = 0; i < data.inkKeys.Count; i++)
            {
                DialogueManager.Instance.SetGlobalVariable(data.inkKeys[i], data.inkValues[i]);
            }
        }

        Debug.Log("Загрузка полностью завершена!");
    }

    public void PrepareNewGame()
    {
        // 1. Очищаем список событий (чтобы предметы снова появились, а пазлы сбросились)
        worldEvents.Clear();

        // 2. Удаляем файл сохранения с диска
        SaveSystem.DeleteSave();

        Debug.Log("Данные сброшены для новой игры.");
    }
}