using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToScene : MonoBehaviour
{
    public string sceneName;
    public string spawnPointName;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerPrefs.SetString("NextSpawnPoint", spawnPointName);
            SceneManager.LoadScene(sceneName);
        }
    }
}