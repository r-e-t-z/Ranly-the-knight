using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Настройки контрольной точки")]
    public bool isActive = true;
    public string playerTag = "Player";


    private Renderer checkpointRenderer;

    void Start()
    {
        checkpointRenderer = GetComponent<Renderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        if (other.CompareTag(playerTag))
        {
            SaveAtCheckpoint();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        if (other.CompareTag(playerTag))
        {
            SaveAtCheckpoint();
        }
    }

    private void SaveAtCheckpoint()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            SaveSystem.SaveGame(player.transform.position);
            isActive = false;
            Debug.Log("Сохранение на контрольной точке!");
        }
    }
}