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
    public LayerMask obstacleLayer;
    public float maxTimeFailsafe = 4.0f;

    [Header("Диалог")]
    public TextAsset inkJSON;
    public string knotName;

    [Header("Настройки триггера")]
    public bool triggerOnce = false;

    private bool hasTriggered = false;
    private bool isMoving = false;
    private PlayerMovement playerController;
    private Animator playerAnimator; // Ссылка на аниматор
    private Rigidbody2D playerRb;

    void Start()
    {
        playerController = FindObjectOfType<PlayerMovement>();
        if (playerController != null)
        {
            playerRb = playerController.GetComponent<Rigidbody2D>();
            // Ищем аниматор на игроке или детях (Visuals)
            playerAnimator = playerController.GetComponentInChildren<Animator>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isMoving)
        {
            if (triggerOnce && hasTriggered) return;

            if (DialogueManager.Instance != null && inkJSON != null)
            {
                DialogueManager.Instance.StartDialogue(inkJSON, knotName);
            }

            StartCoroutine(SmartMoveRoutine(other.gameObject));

            hasTriggered = true;
        }
    }

    private IEnumerator SmartMoveRoutine(GameObject player)
    {
        isMoving = true;

        if (playerController != null) playerController.enabled = false;

        Vector3 startPos = player.transform.position;
        Vector3 idealTargetPos = CalculateTargetPosition(startPos);
        Vector3 mainDirection = (idealTargetPos - startPos).normalized;

        float currentTravelTime = 0f;

        while (Vector3.Distance(player.transform.position, idealTargetPos) > 0.1f)
        {
            currentTravelTime += Time.deltaTime;
            if (currentTravelTime > maxTimeFailsafe)
            {
                break;
            }

            if (playerController != null) playerController.enabled = false;

            Vector3 moveDir = mainDirection;
            float rayLength = 0.5f;

            RaycastHit2D hitMain = Physics2D.Raycast(player.transform.position, mainDirection, rayLength, obstacleLayer);

            if (hitMain.collider != null)
            {
                Vector3 dirPos45 = Quaternion.Euler(0, 0, 45) * mainDirection;
                RaycastHit2D hitPos45 = Physics2D.Raycast(player.transform.position, dirPos45, rayLength, obstacleLayer);

                Vector3 dirNeg45 = Quaternion.Euler(0, 0, -45) * mainDirection;
                RaycastHit2D hitNeg45 = Physics2D.Raycast(player.transform.position, dirNeg45, rayLength, obstacleLayer);

                if (hitPos45.collider == null) moveDir = dirPos45;
                else if (hitNeg45.collider == null) moveDir = dirNeg45;
                else moveDir = Vector3.zero;
            }

            if (playerRb != null)
            {
                Vector2 newPos = playerRb.position + (Vector2)(moveDir * speed * Time.deltaTime);
                playerRb.MovePosition(newPos);
            }
            else
            {
                player.transform.position += moveDir * speed * Time.deltaTime;
            }

            // Обновляем Аниматор вместо спрайта
            UpdatePlayerAnimation(mainDirection);

            yield return null;
        }

        // Останавливаем анимацию
        if (playerAnimator != null) playerAnimator.SetFloat("Speed", 0f);

        isMoving = false;

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

    // Новый метод управления аниматором
    void UpdatePlayerAnimation(Vector3 dir)
    {
        if (playerAnimator == null) return;

        playerAnimator.SetFloat("Horizontal", dir.x);
        playerAnimator.SetFloat("Vertical", dir.y);
        playerAnimator.SetFloat("Speed", 1f);
    }
}