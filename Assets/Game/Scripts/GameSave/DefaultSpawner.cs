using UnityEngine;

public class DefaultSpawner : MonoBehaviour
{
    [Header("Куда поставить игрока в начале игры")]
    public Transform startPoint;

    void Start()
    {
        // Проверяем, есть ли сохранение. 
        // Если мы начали НОВУЮ ИГРУ, файла сохранения нет.
        if (!SaveSystem.HasSaveData())
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            // Если игрока еще нет (мы загрузились из меню), создаем его
            if (player == null && GameSaveManager.Instance.gameSystemsPrefab != null)
            {
                Instantiate(GameSaveManager.Instance.gameSystemsPrefab);
                player = GameObject.FindGameObjectWithTag("Player");
            }

            // Перемещаем в точку старта
            if (player != null && startPoint != null)
            {
                player.transform.position = startPoint.position;
                Debug.Log("Новая игра: Игрок установлен в стартовую точку.");
            }
        }
    }
}