using UnityEngine;
using System.Collections;

public class ZoneBlockerTrigger : MonoBehaviour
{
    public enum PushDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    [Header("Настройки движения")]
    public PushDirection direction = PushDirection.Down;
    public float distance = 2.0f;
    public float speed = 3.5f;

    [Header("Обход препятствий")]
    [Tooltip("Слои, которые считаются препятствием (стены, камни)")]
    public LayerMask obstacleLayer;
    [Tooltip("Максимальное время попытки отойти (защита от зависания игры)")]
    public float maxTimeFailsafe = 4.0f;

    [Header("Диалог")]
    public TextAsset inkJSON;
    public string knotName;

    [Header("Настройки триггера")]
    public bool triggerOnce = false;

    private bool hasTriggered = false;
    private bool isMoving = false;
    private PlayerMovement playerController;
    private SpriteRenderer playerSprite;
    private Rigidbody2D playerRb;

    void Start()
    {
        playerController = FindObjectOfType<PlayerMovement>();
        if (playerController != null)
        {
            playerSprite = playerController.GetComponent<SpriteRenderer>();
            playerRb = playerController.GetComponent<Rigidbody2D>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isMoving)
        {
            if (triggerOnce && hasTriggered) return;

            // 1. Запуск диалога
            if (DialogueManager.Instance != null && inkJSON != null)
            {
                DialogueManager.Instance.StartDialogue(inkJSON, knotName);
            }

            // 2. Начало движения
            StartCoroutine(SmartMoveRoutine(other.gameObject));

            hasTriggered = true;
        }
    }

    private IEnumerator SmartMoveRoutine(GameObject player)
    {
        isMoving = true;

        // Отключаем управление
        if (playerController != null) playerController.enabled = false;

        // Рассчитываем идеальную конечную точку
        Vector3 startPos = player.transform.position;
        Vector3 idealTargetPos = CalculateTargetPosition(startPos);
        Vector3 mainDirection = (idealTargetPos - startPos).normalized;

        float currentTravelTime = 0f;

        // Цикл работает пока мы далеко от цели И не превысили лимит времени
        while (Vector3.Distance(player.transform.position, idealTargetPos) > 0.1f)
        {
            // --- ЗАЩИТА ОТ СОФТЛОКА (TIME OUT) ---
            currentTravelTime += Time.deltaTime;
            if (currentTravelTime > maxTimeFailsafe)
            {
                Debug.LogWarning("ZoneBlocker: Время вышло! Принудительная остановка во избежание софтлока.");
                break; // Выходим из цикла, даже если не дошли
            }

            // Гарантируем отключение управления каждый кадр
            if (playerController != null) playerController.enabled = false;

            // --- ЛОГИКА ОБХОДА (RAYCAST) ---
            Vector3 moveDir = mainDirection;
            float rayLength = 0.5f; // Длина проверки

            // Пускаем луч вперед
            RaycastHit2D hitMain = Physics2D.Raycast(player.transform.position, mainDirection, rayLength, obstacleLayer);

            // Если впереди стена
            if (hitMain.collider != null)
            {
                // Пробуем угол +45 градусов
                Vector3 dirPos45 = Quaternion.Euler(0, 0, 45) * mainDirection;
                RaycastHit2D hitPos45 = Physics2D.Raycast(player.transform.position, dirPos45, rayLength, obstacleLayer);

                // Пробуем угол -45 градусов
                Vector3 dirNeg45 = Quaternion.Euler(0, 0, -45) * mainDirection;
                RaycastHit2D hitNeg45 = Physics2D.Raycast(player.transform.position, dirNeg45, rayLength, obstacleLayer);

                if (hitPos45.collider == null)
                {
                    moveDir = dirPos45; // Идем по диагонали (вверх-вправо / вверх-влево и т.д.)
                }
                else if (hitNeg45.collider == null)
                {
                    moveDir = dirNeg45; // Идем по другой диагонали
                }
                else
                {
                    // Если заблокированы все стороны — просто останавливаемся, чтобы не трястись
                    // Цикл прервется по таймеру maxTimeFailsafe, если путь не освободится
                    moveDir = Vector3.zero;
                }
            }

            // --- ДВИЖЕНИЕ ---
            if (playerRb != null)
            {
                // Используем MovePosition для корректной физики
                Vector2 newPos = playerRb.position + (Vector2)(moveDir * speed * Time.deltaTime);
                playerRb.MovePosition(newPos);
            }
            else
            {
                // Фоллбэк если нет Rigidbody
                player.transform.position += moveDir * speed * Time.deltaTime;
            }

            // Обновляем спрайт (передаем mainDirection, чтобы он не дергался при обходе)
            UpdatePlayerSprite(mainDirection);

            yield return null; // Ждем следующий кадр
        }

        isMoving = false;

        // Возвращаем управление, если диалог уже закончился
        if (DialogueManager.Instance != null && !DialogueManager.Instance.IsPlaying())
        {
            if (playerController != null) playerController.enabled = true;
        }
    }

    private Vector3 CalculateTargetPosition(Vector3 startPos)
    {
        switch (direction)
        {
            case PushDirection.Left: return startPos + Vector3.left * distance;
            case PushDirection.Right: return startPos + Vector3.right * distance;
            case PushDirection.Up: return startPos + Vector3.up * distance;
            case PushDirection.Down: return startPos + Vector3.down * distance;
            default: return startPos;
        }
    }

    void UpdatePlayerSprite(Vector3 dir)
    {
        if (playerSprite == null || playerController == null) return;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            if (dir.x > 0) playerSprite.sprite = playerController.rightsprite;
            else playerSprite.sprite = playerController.leftsprite;
        }
        else
        {
            if (dir.y > 0) playerSprite.sprite = playerController.backsprite;
            else playerSprite.sprite = playerController.frontsprite;
        }
    }
}