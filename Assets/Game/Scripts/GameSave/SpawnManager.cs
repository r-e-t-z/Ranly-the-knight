using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    void Start()
    {
        string targetPoint = PlayerPrefs.GetString("NextSpawnPoint");

        if (!string.IsNullOrEmpty(targetPoint))
        {
            GameObject spawnPoint = GameObject.Find(targetPoint);
            if (spawnPoint != null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    player.transform.position = spawnPoint.transform.position;
                }
            }
        }
    }
}