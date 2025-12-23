using UnityEngine;
using System.Collections;

public class ForestExitTrigger : MonoBehaviour
{
    [Header("Настройки возврата")]
    public Transform returnPoint;

    [Header("Диалоги (по попыткам)")]
    public TextAsset[] dialogues;

    [Header("Настройки сложности")]
    public float moveSpeed = 3f;
    public int maxAttempts = 3;
    public bool enableQuickTimeEvent = true;

    [Header("Настройки QTE")]
    public float requiredPresses = 10f;
    public float moveDistancePerPress = 0.2f;
    public float timeLimit = 3f;

    [Header("Аудио")]
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
    private GameObject quickTimeAnimationObject;

    void Start()
    {
        RefreshPlayerReferences();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void RefreshPlayerReferences()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<PlayerMovement>();
            playerAnimator = player.GetComponentInChildren<Animator>();
        }
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
        RefreshPlayerReferences();
        quickTimeAnimationObject = QTEButton.Instance;

        if (quickTimeAnimationObject != null)
        {
            quickTimeActive = true;
            currentPresses = 0f;
            quickTimeTimer = timeLimit;

            // 1. Включаем сам объект
            quickTimeAnimationObject.SetActive(true);

            // 2. ПРИНУДИТЕЛЬНО запускаем аниматор
            Animator anim = quickTimeAnimationObject.GetComponent<Animator>();
            if (anim != null)
            {
                anim.enabled = true;
                // Play(0, -1, 0f) — запускает анимацию в первом слое с самого начала
                anim.Play(0, -1, 0f);
            }

            if (playerController != null) playerController.enabled = false;
        }
        else
        {
            Debug.LogError("Кнопка QTE не найдена!");
            QuickTimeFail();
        }
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
        exitAttempts--;
    }

    void EndQuickTimeEvent()
    {
        quickTimeActive = false;
        if (playerAnimator != null) playerAnimator.SetFloat("Speed", 0f);
        if (quickTimeAnimationObject != null) quickTimeAnimationObject.SetActive(false);
        if (playerController != null) playerController.enabled = true;
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
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
            if (playerAnimator != null)
            {
                playerAnimator.SetFloat("Horizontal", direction.x);
                playerAnimator.SetFloat("Vertical", direction.y);
                playerAnimator.SetFloat("Speed", 1f);
            }
            yield return null;
        }

        if (playerAnimator != null) playerAnimator.SetFloat("Speed", 0f);
        if (playerController != null) playerController.enabled = true;
        isReturning = false;
        StartAnDialogue();
    }

    void StartAnDialogue()
    {
        TextAsset dialogueToPlay = GetDialogue();
        if (DialogueManager.Instance != null && dialogueToPlay != null)
        {
            DialogueManager.Instance.StartDialogue(dialogueToPlay);
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