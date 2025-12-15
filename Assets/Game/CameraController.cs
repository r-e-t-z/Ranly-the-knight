using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    [Header("Target")]
    public Transform target;
    public float smoothSpeed = 5f;

    [Header("State")]
    public bool isLocked = false;

    // Смещение камеры относительно игрока
    private Vector3 offset;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Запоминаем начальное расстояние между камерой и целью
        if (target != null)
        {
            offset = transform.position - target.position;
        }
    }

    void LateUpdate()
    {
        if (isLocked || target == null) return;

        // Если цель - ИГРОК, используем жесткую привязку + сохраненный Offset
        if (target.CompareTag("Player"))
        {
            // Целевая позиция = позиция игрока + смещение
            Vector3 finalPosition = target.position + offset;

            // Жестко ставим позицию (убирает лаги), но Z оставляем от камеры (на всякий случай)
            transform.position = new Vector3(finalPosition.x, finalPosition.y, transform.position.z);
        }
        else
        {
            // Для катсцен (плавный полет к другим объектам)
            // Тут offset обычно не нужен, так как мы центрируемся на NPC
            Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
    }

    public void SetTarget(string targetName)
    {
        if (targetName.ToLower() == "player")
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }
        else
        {
            GameObject obj = GameObject.Find(targetName);
            if (obj != null) target = obj.transform;
        }
    }
}