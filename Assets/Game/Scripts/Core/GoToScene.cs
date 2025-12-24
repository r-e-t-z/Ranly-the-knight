using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToScene : MonoBehaviour
{
    public string sceneName; // Имя сцены, куда идем
    public string spawnPointName; // Имя пустой точки в СЛЕДУЮЩЕЙ сцене (например, "From_Forest")

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Запоминаем имя точки перед загрузкой
            PlayerPrefs.SetString("TargetSpawnPoint", spawnPointName);
            SceneManager.LoadScene(sceneName);
        }
    }
}