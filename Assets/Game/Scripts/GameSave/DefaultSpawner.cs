using UnityEngine;

public class DefaultSpawner : MonoBehaviour
{
    [Header("Главный префаб систем")]
    public GameObject gameSystemsPrefab;

    [Header("Точка старта")]
    public Transform startPoint;

    void Awake()
    {
        // 1. Ищем игрока на сцене сразу при пробуждении
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // 2. Если его нет — создаем системы НЕМЕДЛЕННО
        if (player == null)
        {
            if (gameSystemsPrefab != null)
            {
                Debug.Log("ГРУБАЯ ЗАГРУЗКА: Игрок не найден, создаю системы...");
                GameObject systems = Instantiate(gameSystemsPrefab);
                player = GameObject.FindGameObjectWithTag("Player");
            }
            else
            {
                Debug.LogError("КРИТИЧЕСКАЯ ОШИБКА: Забыли привязать префаб в инспекторе DefaultSpawner!");
            }
        }

        // 3. Если это НОВАЯ ИГРА (нет сохранения), ставим игрока в точку старта
        // Если загрузка идет через LoadGame, менеджер сам его передвинет позже
        if (!SaveSystem.HasSaveData() || (GameSaveManager.Instance != null && GameSaveManager.Instance.isNewGame))
        {
            if (player != null && startPoint != null)
            {
                player.transform.position = startPoint.position;
                Debug.Log("СПАВН: Игрок установлен в стартовую точку.");

                if (GameSaveManager.Instance != null)
                    GameSaveManager.Instance.isNewGame = false;
            }
        }
    }
}