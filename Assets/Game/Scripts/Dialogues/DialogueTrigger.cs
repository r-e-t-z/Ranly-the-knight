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

    public bool startOnEnter = false;
    public bool requirePressE = true;
    bool inRange = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = true;
            if (startOnEnter && !requirePressE) StartDialogue();
            else if (requirePressE) UIInteractPrompt.Instance.Show("ֽאזלט E");
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
        DialogueManager.Instance.StartDialogue(
            inkJSON,
            leftName, leftPortrait,
            rightName, rightPortrait
        );
    }
}
