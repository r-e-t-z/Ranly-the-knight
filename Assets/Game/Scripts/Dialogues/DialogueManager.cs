using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

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

    [Header("Audio & Typing")]
    public AudioSource audioSource; 
    public AudioClip defaultTypingSound; 

    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private string currentFullLine = "";

    private AudioClip currentVoiceSound;
    private float currentTypingSpeed = 0.04f;

    private Story story;
    private bool isPlaying = false;
    private bool isWaiting = false; // Флаг ожидания катсцены
    private NPCData currentNPC;
    private MonoBehaviour playerController;

    private Dictionary<string, object> globalVariables = new Dictionary<string, object>();

    void Awake()
    {
        Instance = this;
        if (choicesContainer != null) choicesContainer.gameObject.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        playerController = FindObjectOfType<PlayerMovement>();
    }

    void Update()
    {
        // Если диалог не идет ИЛИ мы ждем окончания катсцены - ничего не делаем
        if (!isPlaying || isWaiting) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            ContinueDialogue();
        }
    }

    public void StartDialogue(TextAsset inkJSON, string startKnot = null, NPCData npcData = null)
    {
        if (isPlaying) return;

        if (playerController != null)
        {
            playerController.enabled = false;

            Rigidbody2D rb = playerController.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            Animator anim = playerController.GetComponentInChildren<Animator>();

            if (anim != null)
            {
                anim.SetFloat("Speed", 0f);

                anim.Play("Idle");

                anim.Update(0f);
            }
            else
            {
                Debug.LogWarning("DialogueManager: Аниматор игрока не найден");
            }
        }

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

        List<string> variableNames = new List<string>();
        foreach (string varName in story.variablesState) variableNames.Add(varName);

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
                        bool hasItem = CheckActiveSlotForItem(itemId, requiredAmount);
                        story.variablesState[varName] = hasItem;
                    }
                }
            }
        }
    }

    private bool CheckActiveSlotForItem(string itemId, int requiredAmount)
    {
        if (InventoryManager.Instance == null) return false;
        int amountInHand = InventoryManager.Instance.GetActiveSlotItemCount(itemId);
        return amountInHand >= requiredAmount;
    }

    private void RestoreGlobalVariables()
    {
        foreach (var variable in globalVariables)
        {
            if (story.variablesState.Contains(variable.Key))
            {
                try { story.variablesState[variable.Key] = variable.Value; } catch { }
            }
        }
    }

    private void SaveGlobalVariables()
    {
        if (story == null) return;
        List<string> variableNames = new List<string>();
        foreach (string variableName in story.variablesState) variableNames.Add(variableName);

        foreach (string variableName in variableNames)
        {
            globalVariables[variableName] = story.variablesState[variableName];
        }
    }


    public void ContinueDialogue()
    {

        if (isWaiting) return;

        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.maxVisibleCharacters = currentFullLine.Length;
            isTyping = false;
            return;
        }

        foreach (Transform child in choicesContainer) Destroy(child.gameObject);

        if (story.canContinue)
        {
            string text = story.Continue();
            currentFullLine = text.Trim();

            ProcessAllTags();

            if (!isWaiting)
            {
                ApplyVisualTags();
                typingCoroutine = StartCoroutine(TypewriterRoutine(currentFullLine));
            }
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
                currentAction = tag.Substring(7).Trim();
                actions[currentAction] = new List<string>();
            }
            else if (!string.IsNullOrEmpty(currentAction) && tag.Contains(":"))
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

    private IEnumerator TypewriterRoutine(string line)
    {
        isTyping = true;
        dialogueText.text = line;
        dialogueText.maxVisibleCharacters = 0; 

        yield return null;

        for (int i = 0; i < line.Length; i++)
        {
            dialogueText.maxVisibleCharacters = i + 1; 

            if (i % 2 == 0 && audioSource != null)
            {
                AudioClip clipToPlay = currentVoiceSound != null ? currentVoiceSound : defaultTypingSound;

                if (clipToPlay != null)
                {
                    audioSource.pitch = Random.Range(0.95f, 1.05f);
                    audioSource.PlayOneShot(clipToPlay);
                }
            }

            yield return new WaitForSeconds(currentTypingSpeed);
        }

        isTyping = false;
    }

    private void ExecuteAction(string actionType, List<string> parameters)
    {
        switch (actionType)
        {
            case "play_cutscene": PlayCutsceneAction(parameters); break;
            case "camera_target": CameraTargetAction(parameters); break;
            case "give_item": GiveItemAction(parameters); break;
            case "take_item": TakeItemAction(parameters); break;
            case "quest_add": AddQuestAction(parameters); break;
            case "quest_add_item": AddQuestItemAction(parameters); break;
            case "quest_complete": CompleteQuestAction(parameters); break;
            case "activate_trigger": ActivateTriggerAction(parameters); break;
            case "deactivate_object": DeactivateObjectAction(parameters); break;
            case "start_animation": StartAnimationAction(parameters); break;
            case "quest_text": QuestTextAction(parameters); break;
            case "change_scene": ChangeSceneAction(parameters); break;
            case "teleport_player": TeleportPlayerAction(parameters); break;
            case "unlock_ability": UnlockAbilityAction(parameters); break;
            case "text_speed": ChangeSpeedAction(parameters); break;
            case "play_animation_sequence": PlayAnimationSequenceAction(parameters); break;

            default:
                Debug.LogWarning($"Неизвестное действие: {actionType}");
                break;
        }
    }

    private void PlayCutsceneAction(List<string> parameters)
    {
        string targetName = GetParameterValue(parameters, "target");     
        string animName = GetParameterValue(parameters, "animation_name"); 
        string destination = GetParameterValue(parameters, "move_after");  

        string moveTargetName = GetParameterValue(parameters, "move_target");

        if (string.IsNullOrEmpty(targetName))
        {
            targetName = animName;
        }

        if (!string.IsNullOrEmpty(targetName) && !string.IsNullOrEmpty(animName))
        {

            StartCoroutine(PlayCutsceneRoutine(targetName, animName, destination, moveTargetName));
        }
    }

    private IEnumerator PlayCutsceneRoutine(string targetName, string animName, string destination, string moveTargetName)
    {
        isWaiting = true;
        dialoguePanel.SetActive(false);

        if (AnimationManager.Instance != null)
        {
            AnimationManager.Instance.PlayAnimation(targetName, animName);
        }

        float duration = 1.5f;
        if (AnimationManager.Instance != null)
        {
            duration = AnimationManager.Instance.GetAnimationLength(targetName, animName);
            if (duration <= 0) duration = 1.5f;
        }

        yield return new WaitForSeconds(duration);

        if (!string.IsNullOrEmpty(destination))
        {
            string actualObjectToMoveName = !string.IsNullOrEmpty(moveTargetName) ? moveTargetName : targetName;

            GameObject objToMove = GameObject.Find(actualObjectToMoveName);
            GameObject destObj = GameObject.Find(destination);

            if (objToMove != null && destObj != null)
            {
                objToMove.transform.position = destObj.transform.position;
                Debug.Log($"Объект {actualObjectToMoveName} перемещен в {destination}");
            }
            else
            {
                Debug.LogWarning($"Не удалось переместить. Цель: {actualObjectToMoveName}, Точка: {destination}");
            }
        }

        dialoguePanel.SetActive(true);
        isWaiting = false;

        ContinueDialogue();
    }

    private void PlayAnimationSequenceAction(List<string> parameters)
    {
        string target = GetParameterValue(parameters, "target");
        string animationsStr = GetParameterValue(parameters, "animations");

        if (!string.IsNullOrEmpty(target) && !string.IsNullOrEmpty(animationsStr) && AnimationManager.Instance != null)
        {
            string[] animationNames = animationsStr.Split(',');
            AnimationManager.Instance.PlaySequenceOnObject(target, animationNames.Select(s => s.Trim()).ToArray());
        }
    }

    private void CameraTargetAction(List<string> parameters)
    {
        string target = GetParameterValue(parameters, "target");
        if (!string.IsNullOrEmpty(target) && CameraController.Instance != null)
        {
            CameraController.Instance.SetTarget(target);
        }
    }

    private void AddQuestAction(List<string> parameters)
    {
        string id = GetParameterValue(parameters, "id");
        string desc = GetParameterValue(parameters, "desc");
        if (!string.IsNullOrEmpty(id) && QuestsManager.Instance != null)
            QuestsManager.Instance.AddQuest(id, desc);
    }

    private void AddQuestItemAction(List<string> parameters)
    {
        string id = GetParameterValue(parameters, "id");
        string desc = GetParameterValue(parameters, "desc");
        string itemId = GetParameterValue(parameters, "item_id");
        int amount = GetIntParameterValue(parameters, "amount", 1);
        if (!string.IsNullOrEmpty(id) && QuestsManager.Instance != null)
            QuestsManager.Instance.AddQuest(id, desc, itemId, amount);
    }

    private void CompleteQuestAction(List<string> parameters)
    {
        string id = GetParameterValue(parameters, "id");
        if (!string.IsNullOrEmpty(id) && QuestsManager.Instance != null)
            QuestsManager.Instance.CompleteQuest(id);
    }

    private void GiveItemAction(List<string> parameters)
    {
        string itemId = GetParameterValue(parameters, "item_id");
        int amount = GetIntParameterValue(parameters, "amount", 1);
        if (!string.IsNullOrEmpty(itemId)) InventoryManager.Instance.AddItem(itemId, amount);
    }

    private void TakeItemAction(List<string> parameters)
    {
        string itemId = GetParameterValue(parameters, "item_id");
        int amount = GetIntParameterValue(parameters, "amount", 1);
        if (!string.IsNullOrEmpty(itemId)) InventoryManager.Instance.RemoveItemFromActiveSlot(itemId, amount);
    }

    private void ActivateTriggerAction(List<string> parameters)
    {
        string tName = GetParameterValue(parameters, "trigger_name");
        GameObject trig = GameObject.Find(tName);
        if (trig != null && trig.GetComponent<Collider2D>()) trig.GetComponent<Collider2D>().enabled = true;
    }

    private void DeactivateObjectAction(List<string> parameters)
    {
        string oName = GetParameterValue(parameters, "object_name");
        GameObject obj = GameObject.Find(oName);
        if (obj != null) obj.SetActive(false);
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

    private void ChangeSceneAction(List<string> parameters)
    {
        string sName = GetParameterValue(parameters, "scene_name");
        if (!string.IsNullOrEmpty(sName)) UnityEngine.SceneManagement.SceneManager.LoadScene(sName);
    }

    private void QuestTextAction(List<string> parameters) { }

    private void TeleportPlayerAction(List<string> parameters)
    {
        string destinationName = GetParameterValue(parameters, "destination");


        string targetName = GetParameterValue(parameters, "target");
        if (string.IsNullOrEmpty(targetName)) targetName = "Player";

        GameObject targetObj = GameObject.Find(targetName);
        GameObject destinationObj = GameObject.Find(destinationName);

        if (targetObj != null && destinationObj != null)
        {
            targetObj.transform.position = destinationObj.transform.position;

            Debug.Log($"Телепортация {targetName} в точку {destinationName}");
        }
        else
        {
            Debug.LogWarning($"Не удалось телепортировать. Цель: {targetObj}, Точка: {destinationObj}");
        }
    }

    private void ChangeSpeedAction(List<string> parameters)
    {
        string val = GetParameterValue(parameters, "val");
        if (float.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float speed))
        {
            currentTypingSpeed = speed;
        }
    }

    private void UnlockAbilityAction(List<string> parameters) { }


    private string GetParameterValue(List<string> parameters, string key)
    {
        foreach (string param in parameters)
        {
            if (param.Trim().StartsWith(key + ":")) return param.Trim().Substring(key.Length + 1).Trim();
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
            if (value == "true") story.variablesState[varName] = true;
            else if (value == "false") story.variablesState[varName] = false;
            else story.variablesState[varName] = value;
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
            if (tag == "side:left") { portraitLeft.gameObject.SetActive(true); nameLeft.gameObject.SetActive(true); }
            else if (tag == "side:right") { portraitRight.gameObject.SetActive(true); nameRight.gameObject.SetActive(true); }
            else if (tag.StartsWith("speaker:"))
            {
                string sName = tag.Substring(8).Trim();
                if (portraitLeft.gameObject.activeSelf) nameLeft.text = sName;
                if (portraitRight.gameObject.activeSelf) nameRight.text = sName;
            }
            else if (tag.StartsWith("portrait:"))
            {
                string pName = tag.Substring(9).Trim();
                Sprite sprite = Resources.Load<Sprite>("Portraits/" + pName);
                if (sprite != null)
                {
                    if (portraitLeft.gameObject.activeSelf) portraitLeft.sprite = sprite;
                    if (portraitRight.gameObject.activeSelf) portraitRight.sprite = sprite;
                }
            }
        }
    }

    private void SetDefaultSpeaker(NPCData npcData)
    {
        portraitLeft.gameObject.SetActive(true);
        nameLeft.gameObject.SetActive(true);
        nameLeft.text = npcData.npcName;
        if (npcData.portrait != null) portraitLeft.sprite = npcData.portrait;

        currentVoiceSound = npcData.voiceSound;
        currentTypingSpeed = npcData.typingSpeed > 0 ? npcData.typingSpeed : 0.04f;
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
            btn.onClick.AddListener(() => { story.ChooseChoiceIndex(choiceIndex); ContinueDialogue(); });
        }
    }

    private void EndDialogue()
    {
        SaveGlobalVariables();
        isPlaying = false;
        isWaiting = false;
        dialoguePanel.SetActive(false);
        choicesContainer.gameObject.SetActive(false);

        if (playerController != null) playerController.enabled = true;

        if (CameraController.Instance != null) CameraController.Instance.SetTarget("Player");

        currentNPC = null;
    }

    public bool IsPlaying() => isPlaying;
}