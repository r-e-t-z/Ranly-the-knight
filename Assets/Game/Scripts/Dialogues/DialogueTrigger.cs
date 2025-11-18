using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public TextAsset inkJSON;

    [Header("Left Character")]
    public string leftName;
    public Sprite leftPortrait;

    [Header("Right Character")]
    public string rightName;
    public Sprite rightPortrait;

    [Header("Режим работы")]
    public bool workOnlyOnce = false;
    public bool startOnEnter = false;
    public bool requirePressE = true;

    bool inRange = false;
    bool alreadyUsed = false;


    void OnTriggerEnter2D(Collider2D other)
    {
        if (alreadyUsed && workOnlyOnce) return;
        if (other.CompareTag("Player"))
        {
            inRange = true;
            if (startOnEnter && !requirePressE) StartDialogue();
            else if (requirePressE) UIInteractPrompt.Instance.Show("Нажми E");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = false;
            UIInteractPrompt.Instance.Hide();
        }
    }

    void Update()
    {
        if (inRange && requirePressE && Input.GetKeyDown(KeyCode.E))
        {
            UIInteractPrompt.Instance.Hide();
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        if (workOnlyOnce)
        {
            alreadyUsed = true;
            inRange = false;
        }

        DialogueManager.Instance.StartDialogue(
            inkJSON,
            leftName, leftPortrait,
            rightName, rightPortrait
        );
    }
}
