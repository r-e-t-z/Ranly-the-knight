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

    [Header("Настройки быстрого нажатия")]
    public float requiredPresses = 10f;
    public float moveDistancePerPress = 0.2f;
    public float timeLimit = 3f;

    [Header("UI элементы")]
    public TextMeshProUGUI quickTimeText;

    private int exitAttempts = 0;
    private PlayerMovement playerController;
    private SpriteRenderer playerSprite;
    private bool isReturning = false;
    private bool quickTimeActive = false;
    private float currentPresses = 0f;
    private float quickTimeTimer = 0f;
    private Vector3 quickTimeStartPosition;
    private Vector3 quickTimeTargetPosition;

    void Start()
    {
        playerController = FindObjectOfType<PlayerMovement>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerSprite = player.GetComponent<SpriteRenderer>();

        if (quickTimeText != null)
            quickTimeText.gameObject.SetActive(false);
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
        quickTimeTargetPosition = returnPoint.position;

        if (quickTimeText != null)
        {
            quickTimeText.text = $"Быстро нажимай A! {currentPresses}/{requiredPresses}";
            quickTimeText.gameObject.SetActive(true);
        }

        if (playerController != null)
            playerController.enabled = false;
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

            playerSprite.sprite = playerController.leftsprite;
        }
    }

    void UpdateQuickTimeUI()
    {
        if (quickTimeText != null)
        {
            quickTimeText.text = $"Быстро нажимай A! {currentPresses}/{requiredPresses}\nВремя: {quickTimeTimer:F1}с";
        }
    }

    void QuickTimeSuccess()
    {
        quickTimeActive = false;

        if (quickTimeText != null)
            quickTimeText.gameObject.SetActive(false);

        if (playerController != null)
            playerController.enabled = true;

        StartAnDialogue();
    }

    void QuickTimeFail()
    {
        quickTimeActive = false;

        if (quickTimeText != null)
            quickTimeText.gameObject.SetActive(false);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = quickTimeStartPosition;
        }


        if (playerController != null)
            playerController.enabled = true;

        StartAnDialogue();
    }

    IEnumerator ReturnPlayer(GameObject player)
    {
        isReturning = true;

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        yield return new WaitForSeconds(0.3f);

        while (Vector3.Distance(player.transform.position, returnPoint.position) > 0.1f)
        {
            Vector3 direction = (returnPoint.position - player.transform.position).normalized;
            player.transform.position += direction * moveSpeed * Time.deltaTime;
            UpdatePlayerSprite(direction);
            yield return null;
        }

        if (playerController != null)
        {
            playerController.enabled = true;
        }

        isReturning = false;
        StartAnDialogue();
    }

    void UpdatePlayerSprite(Vector3 direction)
    {
        if (playerSprite == null || playerController == null) return;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0)
            {
                playerSprite.sprite = playerController.rightsprite;
            }
            else
            {
                playerSprite.sprite = playerController.leftsprite;
            }
        }
        else
        {
            if (direction.y > 0)
            {
                playerSprite.sprite = playerController.backsprite;
            }
            else
            {
                playerSprite.sprite = playerController.frontsprite;
            }
        }
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
        if (exitAttempts <= dialogues.Length)
        {
            return dialogues[exitAttempts - 1];
        }
        else
        {
            return dialogues[dialogues.Length - 1];
        }
    }

    public void ResetAttempts()
    {
        exitAttempts = 0;
        quickTimeActive = false;
        currentPresses = 0f;

        if (quickTimeText != null)
            quickTimeText.gameObject.SetActive(false);
    }
}