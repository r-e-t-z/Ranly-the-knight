using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Диалог")]
    public Dialogue dialogue;

    [Header("Настройки триггера")]
    public DialogueTriggerType triggerType = DialogueTriggerType.OnInteract;
    public KeyCode interactKey = KeyCode.E;

    [Header("Подсказка")]
    public GameObject interactHint;

    private bool playerInRange = false;
    private DialogueSystem dialogueSystem;

    void Start()
    {
        dialogueSystem = FindObjectOfType<DialogueSystem>();

        if (interactHint != null)
            interactHint.SetActive(false);
    }

    void Update()
    {
        if (triggerType == DialogueTriggerType.OnInteract &&
            playerInRange &&
            Input.GetKeyDown(interactKey))
        {
            TriggerDialogue();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (triggerType == DialogueTriggerType.OnInteract)
            {
                if (interactHint != null)
                    interactHint.SetActive(true);
            }
            else if (triggerType == DialogueTriggerType.OnTriggerEnter)
            {
                TriggerDialogue();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (interactHint != null)
                interactHint.SetActive(false);
        }
    }

    public void TriggerDialogue()
    {
        if (dialogue != null && dialogueSystem != null)
        {
            dialogueSystem.StartDialogue(dialogue);
        }
    }
}

public enum DialogueTriggerType
{
    OnTriggerEnter,
    OnInteract
}