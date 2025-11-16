using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ink.Runtime;
using UnityEngine.EventSystems;

public class DialogueManager : MonoBehaviour
{
    [Header("Main UI")]
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

    public static DialogueManager Instance;

    void Awake()
    {
        Instance = this;
        dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (!isPlaying) return;

        // --- ПЕРЕХОД ПО ЛКМ ---
        if (Input.GetMouseButtonDown(0))
        {
            // Если клик по UI — кнопка должна работать, а диалог НЕ листается
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("LMB on UI → buttons work, dialogue NOT advancing");
                return;
            }

            Debug.Log("LMB → ContinueDialogue()");
            ContinueDialogue();
        }

        // Переход по клавише E
        if (Input.GetKeyDown(KeyCode.E))
        {
            ContinueDialogue();
        }

       
    }

    public void StartDialogue(TextAsset inkJSON,
                              string leftCharName, Sprite leftCharPortrait,
                              string rightCharName, Sprite rightCharPortrait)
    {
        Debug.Log("=== START DIALOGUE ===");

        story = new Story(inkJSON.text);

        // Имена и портреты до тегов
        nameLeft.text = leftCharName;
        nameRight.text = rightCharName;

        portraitLeft.sprite = leftCharPortrait;
        portraitRight.sprite = rightCharPortrait;

        dialoguePanel.SetActive(true);
        isPlaying = true;

        ContinueDialogue();
    }

    public void ContinueDialogue()
    {
        // Удаляем старые кнопки выбора
        foreach (Transform c in choicesContainer)
            Destroy(c.gameObject);

        // Продолжаем текст
        if (story.canContinue)
        {
            string text = story.Continue().Trim();
            dialogueText.text = text;
            Debug.Log("Line: " + text);

            ApplyTags();
            return;
        }

        // Показать выборы
        if (story.currentChoices.Count > 0)
        {
            Debug.Log("Create choices: " + story.currentChoices.Count);
            GenerateChoices();
            return;
        }

        // Конец
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

            // Имя
            if (tag.StartsWith("speaker:"))
            {
                string speakerName = tag.Substring("speaker:".Length).Trim();

                if (portraitLeft.gameObject.activeSelf)
                    nameLeft.text = speakerName;

                if (portraitRight.gameObject.activeSelf)
                    nameRight.text = speakerName;
            }

            // Портрет
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
        Debug.Log("GenerateChoices: " + story.currentChoices.Count);

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
                Debug.Log("Choice CLICKED: " + choice.text);
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
    }

    public bool IsPlaying() => isPlaying;
}
