using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;
    public Image portraitImage;

    [Header("Settings")]
    public float textSpeed = 0.05f;

    private Dialogue currentDialogue;
    private int currentLineIndex;
    private bool isTyping = false;

    void Start()
    {
        dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && dialoguePanel.activeSelf)
        {
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = currentDialogue.lines[currentLineIndex].text;
                isTyping = false;
            }
            else
            {
                NextLine();
            }
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {

        if (dialogue.textFile != null)
        {
            dialogue.LoadFromTextFile();
        }

        currentDialogue = dialogue;
        currentLineIndex = 0;
        dialoguePanel.SetActive(true);

        ShowLine(currentDialogue.lines[currentLineIndex]);
    }

    void ShowLine(Dialogue.DialogueLine line)
    {

        if (line.speaker != null)
        {
            portraitImage.sprite = line.speaker.portrait;
            nameText.text = line.speaker.characterName;
            nameText.color = line.speaker.nameColor;
        }
        else
        {
            nameText.text = "";
            portraitImage.sprite = null;
        }

        StartCoroutine(TypeLine(line.text));
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }

    void NextLine()
    {
        currentLineIndex++;

        if (currentLineIndex < currentDialogue.lines.Length)
        {
            ShowLine(currentDialogue.lines[currentLineIndex]);
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        Debug.Log("Диалог завершен");
    }
}