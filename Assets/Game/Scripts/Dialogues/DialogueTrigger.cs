using Ink.Runtime;
using System.Xml.Linq;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("NPC Settings")]
    public NPCData npcData;

    [Header("Trigger Settings")]
    public bool workOnlyOnce = false;
    public bool startOnEnter = false;
    public bool requirePressE = true;

    private bool inRange = false;
    private bool alreadyUsed = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (alreadyUsed && workOnlyOnce) return;
        if (other.CompareTag("Player"))
        {
            inRange = true;
            if (startOnEnter && !requirePressE)
                StartDialogue();
            else if (requirePressE)
                UIInteractPrompt.Instance.Show("Нажми E");
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
            // Проверяем, не идет ли уже диалог
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying())
            {
                return;
            }

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

        DialogueManager.Instance.StartDialogue(npcData.inkFile, "start", npcData);
    }

    public void ResetMeeting()
    {
        alreadyUsed = false;
    }
}