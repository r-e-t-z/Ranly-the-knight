using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Настройки контрольной точки")]
    public bool isActive = true;
    public ParticleSystem activationEffect;
    public string playerTag = "Player";

    [Header("Визуальные эффекты")]
    public Material activeMaterial;
    public Material inactiveMaterial;
    private Renderer checkpointRenderer;

    void Start()
    {
        checkpointRenderer = GetComponent<Renderer>();
        UpdateVisuals();
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
            UpdateVisuals();

            // Визуальный эффект активации
            if (activationEffect != null)
            {
                activationEffect.Play();
            }

            Debug.Log("Сохранение на контрольной точке!");
        }
    }

    private void UpdateVisuals()
    {
        if (checkpointRenderer != null && activeMaterial != null && inactiveMaterial != null)
        {
            checkpointRenderer.material = isActive ? activeMaterial : inactiveMaterial;
        }
    }

    // Метод для сброса контрольной точки (если нужно)
    public void ResetCheckpoint()
    {
        isActive = true;
        UpdateVisuals();
    }
}