using UnityEngine;
using System.Collections;
using TMPro;

public class ForestExitTrigger : MonoBehaviour
{
    [Header("Точка возврата")]
    public Transform returnPoint;

    [Header("Диалоги")]
    public TextAsset[] dialogues;

    [Header("Настройки выхода")]
    public float moveSpeed = 3f;
    public int maxAttempts = 3;
    public bool enableQuickTimeEvent = true;

    [Header("Настройки QTE")]
    public float requiredPresses = 10f;
    public float moveDistancePerPress = 0.2f;
    public float timeLimit = 3f;

    [Header("Визуал QTE")]
    [Tooltip("Объект с анимацией кнопки 'A' (например, спрайт или UI Image)")]
    public GameObject quickTimeAnimationObject;
    public TextMeshProUGUI timerText; // Опционально: текст таймера/счетчика

    [Header("Звуки")]
    public AudioSource audioSource;
    public AudioClip successSound;
    public AudioClip failSound;

    private int exitAttempts = 0;
    private PlayerMovement playerController;
    private Animator playerAnimator;
    private bool isReturning = false;
    private bool quickTimeActive = false;
    private float currentPresses = 0f;
    private float quickTimeTimer = 0f;
    private Vector3 quickTimeStartPosition;

    void Start()
    {
        playerController = FindObjectOfType<PlayerMovement>();
        if (playerController != null)
        {
            playerAnimator = playerController.GetComponentInChildren<Animator>();
        }

        // Если AudioSource не назначен, пробуем найти на этом объекте
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        // Скрываем анимацию и текст при старте
        if (quickTimeAnimationObject != null) quickTimeAnimationObject.SetActive(false);
        if (timerText != null) timerText.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && exitAttempts < maxAttempts && !isReturning && !quickTimeActive)
        {
            exitAttempts++;

            if (exitAttempts == maxAttempts && enableQuickTimeEvent)
            {
                StartQuickTimeEvent(other.gameObject);
            }
            else
            {
                StartCoroutine(ReturnPlayer(other.gameObject));
            }
        }
    }

    void StartQuickTimeEvent(GameObject player)
    {
        quickTimeActive = true;
        currentPresses = 0f;
        quickTimeTimer = timeLimit;
        quickTimeStartPosition = player.transform.position;

        // ВКЛЮЧАЕМ АНИМАЦИЮ
        if (quickTimeAnimationObject != null) quickTimeAnimationObject.SetActive(true);
        if (timerText != null) timerText.gameObject.SetActive(true);

        if (playerController != null) playerController.enabled = false;
    }

    void Update()
    {
        if (quickTimeActive)
        {
            quickTimeTimer -= Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.A))
            {
                OnQuickTimePress();
            }

            UpdateQuickTimeUI();

            if (currentPresses >= requiredPresses)
            {
                QuickTimeSuccess();
            }
            else if (quickTimeTimer <= 0f)
            {
                QuickTimeFail();
            }
        }
    }

    void OnQuickTimePress()
    {
        currentPresses++;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position += Vector3.left * moveDistancePerPress;

            if (playerAnimator != null)
            {
                playerAnimator.SetFloat("Horizontal", -1f);
                playerAnimator.SetFloat("Vertical", 0f);
                playerAnimator.SetFloat("Speed", 1f);
            }
        }
    }

    void UpdateQuickTimeUI()
    {
        if (timerText != null)
        {
            // Показываем только таймер (или счетчик)
            timerText.text = $"{quickTimeTimer:F1}";
        }
    }

    void QuickTimeSuccess()
    {
        EndQuickTimeEvent();
        PlaySound(successSound);
        StartAnDialogue();
    }

    void QuickTimeFail()
    {
        EndQuickTimeEvent();
        PlaySound(failSound);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = returnPoint.transform.position;
        }

        exitAttempts--; // Даем еще попытку
    }

    void EndQuickTimeEvent()
    {
        quickTimeActive = false;
        StopPlayerAnimation();

        // СКРЫВАЕМ АНИМАЦИЮ
        if (quickTimeAnimationObject != null) quickTimeAnimationObject.SetActive(false);
        if (timerText != null) timerText.gameObject.SetActive(false);

        if (playerController != null) playerController.enabled = true;
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // --- Остальной код без изменений ---

    void StopPlayerAnimation()
    {
        if (playerAnimator != null) playerAnimator.SetFloat("Speed", 0f);
    }

    IEnumerator ReturnPlayer(GameObject player)
    {
        isReturning = true;
        if (playerController != null) playerController.enabled = false;
        yield return new WaitForSeconds(0.3f);

        while (Vector3.Distance(player.transform.position, returnPoint.position) > 0.1f)
        {
            Vector3 direction = (returnPoint.position - player.transform.position).normalized;
            player.transform.position += direction * moveSpeed * Time.deltaTime;
            UpdatePlayerAnimation(direction);
            yield return null;
        }

        StopPlayerAnimation();
        if (playerController != null) playerController.enabled = true;
        isReturning = false;
        StartAnDialogue();
    }

    void UpdatePlayerAnimation(Vector3 direction)
    {
        if (playerAnimator == null) return;
        playerAnimator.SetFloat("Horizontal", direction.x);
        playerAnimator.SetFloat("Vertical", direction.y);
        playerAnimator.SetFloat("Speed", 1f);
    }

    void StartAnDialogue()
    {
        TextAsset dialogueToPlay = GetDialogue();
        DialogueManager dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager != null && dialogueToPlay != null)
        {
            dialogueManager.StartDialogue(dialogueToPlay);
        }
    }

    TextAsset GetDialogue()
    {
        if (exitAttempts <= dialogues.Length) return dialogues[exitAttempts - 1];
        else return dialogues[dialogues.Length - 1];
    }

    public void ResetAttempts()
    {
        exitAttempts = 0;
        EndQuickTimeEvent();
    }
}