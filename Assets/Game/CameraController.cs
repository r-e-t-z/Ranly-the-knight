using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    [Header("Target")]
    public Transform target;

    [Tooltip("Примерное время долета до цели. Чем меньше, тем быстрее камера.")]
    public float smoothTime = 0.25f;

    [Header("State")]
    public bool isLocked = false;

    // Смещение камеры относительно игрока
    private Vector3 offset;
    // Служебная переменная для SmoothDamp (хранит текущую скорость камеры)
    private Vector3 currentVelocity = Vector3.zero;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Запоминаем начальное смещение
        if (target != null)
        {
            offset = transform.position - target.position;
        }
    }

    void LateUpdate()
    {
        if (isLocked || target == null) return;

        Vector3 targetPosition;

        if (target.CompareTag("Player"))
        {
            // Если игрок — учитываем смещение (offset)
            targetPosition = target.position + offset;
        }
        else
        {
            // Если NPC или объект — центрируемся прямо на нем, сохраняя Z камеры
            targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
        }

        // Плавное движение через SmoothDamp
        // currentVelocity обновляется движком автоматически
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            smoothTime
        );
    }

    public void SetTarget(string targetName)
    {
        // Сбрасываем скорость при смене цели, чтобы не было "инерционного рывка" от старой цели
        currentVelocity = Vector3.zero;

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