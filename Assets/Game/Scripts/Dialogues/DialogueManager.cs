using Ink.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    [Header("Speaker UI")]
    public Image portraitLeft;
    public TMP_Text nameLeft;
    public Image portraitRight;
    public TMP_Text nameRight;

    [Header("Choices")]
    public Transform choicesContainer;
    public GameObject choiceButtonPrefab;

    private Story story;
    private bool isPlaying = false;
    private NPCData currentNPC;
    private MonoBehaviour playerController;

    private Dictionary<string, object> globalVariables = new Dictionary<string, object>();

    public static DialogueManager Instance;

    void Awake()
    {
        Instance = this;
        choicesContainer.gameObject.SetActive(false);
        dialoguePanel.SetActive(false);
        playerController = FindObjectOfType<PlayerMovement>();
    }

    void Update()
    {
        if (!isPlaying) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            ContinueDialogue();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            ContinueDialogue();
        }
    }

    public void StartDialogue(TextAsset inkJSON, string startKnot = null, NPCData npcData = null)
    {
        if (DialogueManager.Instance.IsPlaying()) return;

        if (playerController != null)
            playerController.enabled = false;

        story = new Story(inkJSON.text);
        currentNPC = npcData;

        RestoreGlobalVariables();

        UpdatePlayerStateVariables();

        if (!string.IsNullOrEmpty(startKnot))
        {
            story.ChoosePathString(startKnot);
        }

        if (npcData != null)
        {
            SetDefaultSpeaker(npcData);
        }

        dialoguePanel.SetActive(true);
        isPlaying = true;

        ContinueDialogue();
    }

    private void UpdatePlayerStateVariables()
    {
        if (story == null) return;

        CheckAllItemVariables();
    }

    private void CheckAllItemVariables()
    {
        if (story == null) return;

        List<string> variableNames = new List<string>();
        foreach (string varName in story.variablesState)
        {
            variableNames.Add(varName);
        }

        foreach (string varName in variableNames)
        {
            if (varName.StartsWith("has_item_"))
            {
                string[] parts = varName.Split('_');
                if (parts.Length >= 4)
                {
                    string itemId = parts[2];
                    if (int.TryParse(parts[3], out int requiredAmount))
                    {
                        try
                        {
                            bool hasItem = CheckActiveSlotForItem(itemId, requiredAmount);
                            story.variablesState[varName] = hasItem;
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogWarning($"Ошибка при обновлении {varName}: {e.Message}");
                        }
                    }
                }
            }
        }
    }

    private bool CheckActiveSlotForItem(string itemId, int requiredAmount)
    {
        if (InventoryManager.Instance == null) return false;

        var activeSlot = InventoryManager.Instance.activeItemSlot;

        if (activeSlot.HasItem() && activeSlot.Item.data.itemID == itemId)
        {
            return activeSlot.Item.stackSize >= requiredAmount;
        }

        return false;
    }

    private void RestoreGlobalVariables()
    {
        foreach (var variable in globalVariables)
        {
            try
            {
                story.variablesState[variable.Key] = variable.Value;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Не удалось восстановить переменную {variable.Key}: {e.Message}");
            }
        }
    }

    private void SaveGlobalVariables()
    {
        if (story == null) return;

        List<string> variableNames = new List<string>();
        foreach (string variableName in story.variablesState)
        {
            variableNames.Add(variableName);
        }

        foreach (string variableName in variableNames)
        {
            try
            {
                globalVariables[variableName] = story.variablesState[variableName];
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Не удалось сохранить переменную {variableName}: {e.Message}");
            }
        }
    }

    private void SetDefaultSpeaker(NPCData npcData)
    {
        if (npcData == null) return;

        portraitLeft.gameObject.SetActive(true);
        nameLeft.gameObject.SetActive(true);
        nameLeft.text = npcData.npcName;

        if (npcData.portrait != null)
        {
            portraitLeft.sprite = npcData.portrait;
        }
    }

    public void ContinueDialogue()
    {
        foreach (Transform child in choicesContainer)
            Destroy(child.gameObject);

        if (story.canContinue)
        {
            string text = story.Continue();
            dialogueText.text = text.Trim();

            ProcessAllTags();

            ApplyVisualTags();
        }
        else if (story.currentChoices.Count > 0)
        {
            ShowChoices();
        }
        else
        {
            EndDialogue();
        }
    }

    private void ProcessAllTags()
    {
        List<string> currentTags = story.currentTags;

        Dictionary<string, List<string>> actions = new Dictionary<string, List<string>>();
        string currentAction = "";

        foreach (string tag in currentTags)
        {
            if (tag.StartsWith("action:"))
            {
                currentAction = tag.Substring(7); 
                actions[currentAction] = new List<string>();
            }
            else if (!string.IsNullOrEmpty(currentAction))
            {
                actions[currentAction].Add(tag);
            }
            else if (tag.StartsWith("set_"))
            {
                ProcessSetTag(tag);
            }
        }

        foreach (var action in actions)
        {
            ExecuteAction(action.Key, action.Value);
        }
    }

    private void ExecuteAction(string actionType, List<string> parameters)
    {

        switch (actionType)
        {
            case "give_item":
                GiveItemAction(parameters);
                break;
            case "take_item":
                TakeItemAction(parameters);
                break;
            case "activate_trigger":
                ActivateTriggerAction(parameters);
                break;
            case "deactivate_object":
                DeactivateObjectAction(parameters);
                break;
            case "start_animation":
                StartAnimationAction(parameters);
                break;
            case "quest_text":
                QuestTextAction(parameters);
                break;
            case "change_scene":
                ChangeSceneAction(parameters);
                break;
            case "teleport_player":
                TeleportPlayerAction(parameters);
                break;
            case "unlock_ability":
                UnlockAbilityAction(parameters);
                break;
            default:
                Debug.LogWarning($"Неизвестное действие: {actionType}");
                break;
        }
    }

    private void GiveItemAction(List<string> parameters)
    {
        string itemId = GetParameterValue(parameters, "item_id");
        int amount = GetIntParameterValue(parameters, "amount", 1);

        if (!string.IsNullOrEmpty(itemId))
        {
            InventoryManager.Instance.AddItem(itemId, amount);
        }
    }

    private void TakeItemAction(List<string> parameters)
    {
        string itemId = GetParameterValue(parameters, "item_id");
        int amount = GetIntParameterValue(parameters, "amount", 1);

        if (!string.IsNullOrEmpty(itemId))
        {
            bool success = InventoryManager.Instance.RemoveItemFromActiveSlot(itemId, amount);
        }
    }


    private void ActivateTriggerAction(List<string> parameters)
    {
        string triggerName = GetParameterValue(parameters, "trigger_name");
        if (!string.IsNullOrEmpty(triggerName))
        {
            GameObject trigger = GameObject.Find(triggerName);
            if (trigger != null)
            {
                Collider2D collider = trigger.GetComponent<Collider2D>();
                if (collider != null) collider.enabled = true;
            }
        }
    }

    private void DeactivateObjectAction(List<string> parameters)
    {
        string objectName = GetParameterValue(parameters, "object_name");
        if (!string.IsNullOrEmpty(objectName))
        {
            GameObject obj = GameObject.Find(objectName);
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }

    private void StartAnimationAction(List<string> parameters)
    {
        string animationName = GetParameterValue(parameters, "animation_name");
        string animationNames = GetParameterValue(parameters, "animation_names");

        if (!string.IsNullOrEmpty(animationNames))
        {
            string[] names = animationNames.Split(',');
            AnimationManager.Instance.PlayMultipleAnimations(names);
        }
        else if (!string.IsNullOrEmpty(animationName))
        {
            AnimationManager.Instance.PlayAnimation(animationName);
        }
    }

    private void QuestTextAction(List<string> parameters)
    {
        Debug.Log($"3 - Это делается ");
        string questText = GetParameterValue(parameters, "quest_text");
        QuestsManager.Instance.AddQuestsTexts(questText);
        Debug.Log($"12 - Это делается {questText}");
    }

    private void ChangeSceneAction(List<string> parameters)
    {
        string sceneName = GetParameterValue(parameters, "scene_name");
    }

    private void TeleportPlayerAction(List<string> parameters)
    {
        string location = GetParameterValue(parameters, "location");
    }

    private void UnlockAbilityAction(List<string> parameters)
    {
        string ability = GetParameterValue(parameters, "ability");
    }

    private string GetParameterValue(List<string> parameters, string key)
    {
        foreach (string param in parameters)
        {
            if (param.StartsWith(key + ":"))
            {
                return param.Substring(key.Length + 1);
            }
        }
        return "";
    }

    private int GetIntParameterValue(List<string> parameters, string key, int defaultValue)
    {
        string value = GetParameterValue(parameters, key);
        return int.TryParse(value, out int result) ? result : defaultValue;
    }

    private void ProcessSetTag(string tag)
    {
        string[] parts = tag.Split(' ');
        if (parts.Length == 2)
        {
            string varName = parts[0].Substring(4); 
            string value = parts[1].ToLower();

            if (value == "true" || value == "false")
            {
                story.variablesState[varName] = value == "true";
            }
            else
            {
                story.variablesState[varName] = value;
            }
        }
    }

    private void ApplyVisualTags()
    {
        portraitLeft.gameObject.SetActive(false);
        portraitRight.gameObject.SetActive(false);
        nameLeft.gameObject.SetActive(false);
        nameRight.gameObject.SetActive(false);

        foreach (string tag in story.currentTags)
        {
            if (tag == "side:left")
            {
                portraitLeft.gameObject.SetActive(true);
                nameLeft.gameObject.SetActive(true);
            }
            else if (tag == "side:right")
            {
                portraitRight.gameObject.SetActive(true);
                nameRight.gameObject.SetActive(true);
            }
            else if (tag.StartsWith("speaker: "))
            {
                string speakerName = tag.Substring("speaker: ".Length);
                if (portraitLeft.gameObject.activeSelf)
                    nameLeft.text = speakerName;
                if (portraitRight.gameObject.activeSelf)
                    nameRight.text = speakerName;
            }
            else if (tag.StartsWith("portrait: "))
            {
                string portraitName = tag.Substring("portrait: ".Length);
                Sprite sprite = Resources.Load<Sprite>("Portraits/" + portraitName);
                if (sprite != null)
                {
                    if (portraitLeft.gameObject.activeSelf)
                        portraitLeft.sprite = sprite;
                    if (portraitRight.gameObject.activeSelf)
                        portraitRight.sprite = sprite;
                }
            }
        }
    }

    private void ShowChoices()
    {
        choicesContainer.gameObject.SetActive(true);
        for (int i = 0; i < story.currentChoices.Count; i++)
        {
            Choice choice = story.currentChoices[i];
            GameObject button = Instantiate(choiceButtonPrefab, choicesContainer);
            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
            buttonText.text = choice.text;

            Button btn = button.GetComponent<Button>();
            int choiceIndex = i;
            btn.onClick.AddListener(() => {
                story.ChooseChoiceIndex(choiceIndex);
                ContinueDialogue();
            });
        }
    }

    private void EndDialogue()
    {
        SaveGlobalVariables();

        isPlaying = false;
        dialoguePanel.SetActive(false);

        if (playerController != null)
            playerController.enabled = true;

        currentNPC = null;
        choicesContainer.gameObject.SetActive(false);
    }

    public bool IsPlaying() => isPlaying;
}