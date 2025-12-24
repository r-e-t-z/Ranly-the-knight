using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    void Start()
    {
        // Читаем имя точки из PlayerPrefs
        string targetPoint = PlayerPrefs.GetString("TargetSpawnPoint");

        if (!string.IsNullOrEmpty(targetPoint))
        {
            // Ищем объект точки спавна на сцене по имени
            GameObject spawnPoint = GameObject.Find(targetPoint);

            // Ищем игрока
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (spawnPoint != null && player != null)
            {
                Debug.Log("Переход: Перемещаю игрока в точку " + targetPoint);

                // Телепортируем
                player.transform.position = spawnPoint.transform.position;

                // Очищаем, чтобы при обычном рестарте сцены не телепортировало обратно
                PlayerPrefs.SetString("TargetSpawnPoint", "");
            }
            else
            {
                if (spawnPoint == null) Debug.LogWarning("SpawnManager: Не нашел на сцене точку с именем " + targetPoint);
            }
        }
    }
}