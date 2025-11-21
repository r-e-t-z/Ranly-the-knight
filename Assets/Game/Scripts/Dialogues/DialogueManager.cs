using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ink.Runtime;
using UnityEngine.EventSystems;

public class DialogueManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    [Header("Speaker Left")]
    public Image portraitLeft;
    public TMP_Text nameLeft;

    [Header("Speaker Right")]
    public Image portraitRight;
    public TMP_Text nameRight;

    [Header("Choices")]
    public Transform choicesContainer;
    public GameObject choiceButtonPrefab;

    Story story;
    bool isPlaying = false;

    private MonoBehaviour playerController;

    public static DialogueManager Instance;

    void Awake()
    {
        Instance = this;
        dialoguePanel.SetActive(false);
        playerController = FindObjectOfType<PlayerMovement>();

    }

    void Update()
    {
        if (!isPlaying) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            ContinueDialogue();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            ContinueDialogue();
        }

       
    }

    public void StartDialogue(TextAsset inkJSON)
    {
        Debug.Log("=== START DIALOGUE ===");

        if (playerController != null)
            playerController.enabled = false;

        story = new Story(inkJSON.text);


        dialoguePanel.SetActive(true);
        isPlaying = true;

        ContinueDialogue();
    }

    public void ContinueDialogue()
    {
        foreach (Transform c in choicesContainer)
            Destroy(c.gameObject);

        if (story.canContinue)
        {
            string text = story.Continue().Trim();
            dialogueText.text = text;
            Debug.Log("Line: " + text);

            ApplyTags();
            return;
        }

        if (story.currentChoices.Count > 0)
        {
            Debug.Log("Количество выборов: " + story.currentChoices.Count);
            GenerateChoices();
            return;
        }

        EndDialogue();
    }

    void ApplyTags()
    {
        portraitLeft.gameObject.SetActive(false);
        portraitRight.gameObject.SetActive(false);
        nameLeft.gameObject.SetActive(false);
        nameRight.gameObject.SetActive(false);

        foreach (var tag in story.currentTags)
        {
            Debug.Log("Tag: " + tag);

            if (tag == "side:left")
            {
                portraitLeft.gameObject.SetActive(true);
                nameLeft.gameObject.SetActive(true);
            }

            if (tag == "side:right")
            {
                portraitRight.gameObject.SetActive(true);
                nameRight.gameObject.SetActive(true);
            }

            if (tag.StartsWith("speaker:"))
            {
                string speakerName = tag.Substring("speaker:".Length).Trim();

                if (portraitLeft.gameObject.activeSelf)
                    nameLeft.text = speakerName;

                if (portraitRight.gameObject.activeSelf)
                    nameRight.text = speakerName;
            }

            if (tag.StartsWith("portrait:"))
            {
                string portraitName = tag.Substring("portrait:".Length).Trim();
                Sprite s = Resources.Load<Sprite>("Portraits/" + portraitName);

                if (s != null)
                {
                    if (portraitLeft.gameObject.activeSelf)
                        portraitLeft.sprite = s;

                    if (portraitRight.gameObject.activeSelf)
                        portraitRight.sprite = s;
                }
            }
        }
    }

    void GenerateChoices()
    {
        choicesContainer.gameObject.SetActive(true);

        int i = 0;

        foreach (Choice choice in story.currentChoices)
        {
            int choiceIndex = choice.index;
            int displayNumber = i + 1;

            GameObject btnGO = Instantiate(choiceButtonPrefab, choicesContainer);
            TMP_Text btnText = btnGO.GetComponentInChildren<TMP_Text>();
            btnText.text = displayNumber + ") " + choice.text;

            Button btn = btnGO.GetComponent<Button>();

            btn.onClick.AddListener(() =>
            {
                Debug.Log("Клик был: " + choice.text);
                story.ChooseChoiceIndex(choiceIndex);
                ContinueDialogue();
            });

            i++;
        }
    }

    void EndDialogue()
    {
        Debug.Log("=== END DIALOGUE ===");
        isPlaying = false;
        dialoguePanel.SetActive(false);
        if (playerController != null)
            playerController.enabled = true;
    }

    public bool IsPlaying() => isPlaying;
}
