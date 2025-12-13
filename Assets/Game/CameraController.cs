using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    public Transform target; // За кем следим сейчас
    public float smoothSpeed = 5f; // Плавность

    void Awake()
    {
        Instance = this;
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // Плавно летим к цели (Z оставляем как у камеры -10)
            Vector3 desiredPosition = new Vector3(target.position.x, target.position.y + 1f, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
    }

    // Метод для смены цели через Диалог
    public void SetTarget(string targetName)
    {
        if (targetName.ToLower() == "player")
        {
            // Пытаемся найти игрока
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }
        else
        {
            // Пытаемся найти объект по имени (например, "Knight")
            GameObject obj = GameObject.Find(targetName);
            if (obj != null) target = obj.transform;
        }
    }
}