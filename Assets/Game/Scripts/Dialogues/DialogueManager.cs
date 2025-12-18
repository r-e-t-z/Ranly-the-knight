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
    private bool isWaiting = false;
    private NPCData currentNPC;
    private MonoBehaviour playerController;

    private Dictionary<string, object> globalVariables = new Dictionary<string, object>();

    private class ActionData
    {
        public string name;
        public List<string> paramsList;
    }

    void Awake()
    {
        Instance = this;
        if (choicesContainer != null) choicesContainer.gameObject.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        playerController = FindObjectOfType<PlayerMovement>();
    }

    void Update()
    {
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
        }

        story = new Story(inkJSON.text);
        currentNPC = npcData;

        RestoreGlobalVariables();
        UpdatePlayerStateVariables();

        if (!string.IsNullOrEmpty(startKnot)) story.ChoosePathString(startKnot);
        if (npcData != null) SetDefaultSpeaker(npcData);

        dialoguePanel.SetActive(true);
        isPlaying = true;

        ContinueDialogue();
    }

    private void UpdatePlayerStateVariables()
    {
        if (story == null) return;
        List<string> variableNames = new List<string>(story.variablesState);
        foreach (string varName in variableNames)
        {
            if (varName.StartsWith("has_item_"))
            {
                string[] parts = varName.Split('_');
                if (parts.Length >= 4 && int.TryParse(parts[3], out int amt))
                {
                    bool hasItem = CheckActiveSlotForItem(parts[2], amt);
                    story.variablesState[varName] = hasItem;
                }
            }
        }
    }

    private bool CheckActiveSlotForItem(string itemId, int requiredAmount)
    {
        if (InventoryManager.Instance == null) return false;
        return InventoryManager.Instance.GetActiveSlotItemCount(itemId) >= requiredAmount;
    }

    private void RestoreGlobalVariables()
    {
        foreach (var variable in globalVariables)
        {
            if (story.variablesState.Contains(variable.Key))
                try { story.variablesState[variable.Key] = variable.Value; } catch { }
        }
    }

    private void SaveGlobalVariables()
    {
        if (story == null) return;
        foreach (string variableName in story.variablesState)
            globalVariables[variableName] = story.variablesState[variableName];
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
            StartCoroutine(ProcessTagsRoutine());
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

    private IEnumerator ProcessTagsRoutine()
    {
        List<string> currentTags = story.currentTags;
        List<ActionData> actionsToRun = new List<ActionData>();
        ActionData currentAction = null;

        foreach (string tag in currentTags)
        {
            string cleanTag = tag.Trim();
            if (cleanTag.StartsWith("action:"))
            {
                currentAction = new ActionData();
                currentAction.name = cleanTag.Substring(7).Trim();
                currentAction.paramsList = new List<string>();
                actionsToRun.Add(currentAction);
            }
            else if (currentAction != null && cleanTag.Contains(":"))
            {
                currentAction.paramsList.Add(cleanTag);
            }
            else if (cleanTag.StartsWith("set_"))
            {
                ProcessSetTag(cleanTag);
            }
        }

        foreach (var action in actionsToRun)
        {
            ExecuteAction(action.name, action.paramsList);

            while (isWaiting)
            {
                yield return null;
            }
        }

        if (!isWaiting)
        {
            dialoguePanel.SetActive(true);
            ApplyVisualTags();
            typingCoroutine = StartCoroutine(TypewriterRoutine(currentFullLine));
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
                AudioClip clip = currentVoiceSound != null ? currentVoiceSound : defaultTypingSound;
                if (clip != null) { audioSource.pitch = Random.Range(0.95f, 1.05f); audioSource.PlayOneShot(clip); }
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
            default: Debug.LogWarning($"Unknown action: {actionType}"); break;
        }
    }

    private void PlayCutsceneAction(List<string> parameters)
    {
        string targetName = GetParameterValue(parameters, "target");
        string animName = GetParameterValue(parameters, "animation_name");
        string destination = GetParameterValue(parameters, "move_after");
        string moveTargetName = GetParameterValue(parameters, "move_target");

        if (string.IsNullOrEmpty(targetName)) targetName = animName;

        if (!string.IsNullOrEmpty(targetName) && !string.IsNullOrEmpty(animName))
        {
            isWaiting = true;
            dialoguePanel.SetActive(false);
            StartCoroutine(PlayCutsceneRoutine(targetName, animName, destination, moveTargetName));
        }
    }

    private IEnumerator PlayCutsceneRoutine(string targetName, string animName, string destination, string moveTargetName)
    {
        if (AnimationManager.Instance != null)
            AnimationManager.Instance.PlayAnimation(targetName, animName);

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
                objToMove.transform.position = destObj.transform.position;
        }

        isWaiting = false;
    }

    private void PlayAnimationSequenceAction(List<string> parameters)
    {
        string target = GetParameterValue(parameters, "target");
        string anims = GetParameterValue(parameters, "animations");
        if (!string.IsNullOrEmpty(target) && !string.IsNullOrEmpty(anims) && AnimationManager.Instance != null)
            AnimationManager.Instance.PlaySequenceOnObject(target, anims.Split(',').Select(s => s.Trim()).ToArray());
    }

    private void CameraTargetAction(List<string> parameters)
    {
        string target = GetParameterValue(parameters, "target");
        if (!string.IsNullOrEmpty(target) && CameraController.Instance != null)
            CameraController.Instance.SetTarget(target);
    }

    private void AddQuestAction(List<string> parameters)
    {
        if (QuestsManager.Instance != null) QuestsManager.Instance.AddQuest(GetParameterValue(parameters, "id"), GetParameterValue(parameters, "desc"));
    }

    private void AddQuestItemAction(List<string> parameters)
    {
        if (QuestsManager.Instance != null) QuestsManager.Instance.AddQuest(GetParameterValue(parameters, "id"), GetParameterValue(parameters, "desc"), GetParameterValue(parameters, "item_id"), GetIntParameterValue(parameters, "amount", 1));
    }

    private void CompleteQuestAction(List<string> parameters)
    {
        if (QuestsManager.Instance != null) QuestsManager.Instance.CompleteQuest(GetParameterValue(parameters, "id"));
    }

    private void GiveItemAction(List<string> parameters)
    {
        if (InventoryManager.Instance != null) InventoryManager.Instance.AddItem(GetParameterValue(parameters, "item_id"), GetIntParameterValue(parameters, "amount", 1));
    }

    private void TakeItemAction(List<string> parameters)
    {
        if (InventoryManager.Instance != null) InventoryManager.Instance.RemoveItemFromActiveSlot(GetParameterValue(parameters, "item_id"), GetIntParameterValue(parameters, "amount", 1));
    }

    private void ActivateTriggerAction(List<string> parameters)
    {
        GameObject g = GameObject.Find(GetParameterValue(parameters, "trigger_name"));
        if (g != null && g.GetComponent<Collider2D>()) g.GetComponent<Collider2D>().enabled = true;
    }

    private void DeactivateObjectAction(List<string> parameters)
    {
        GameObject g = GameObject.Find(GetParameterValue(parameters, "object_name"));
        if (g != null) g.SetActive(false);
    }

    private void StartAnimationAction(List<string> parameters)
    {
        string a = GetParameterValue(parameters, "animation_name");
        string asq = GetParameterValue(parameters, "animation_names");
        if (!string.IsNullOrEmpty(asq)) AnimationManager.Instance.PlayMultipleAnimations(asq.Split(','));
        else if (!string.IsNullOrEmpty(a)) AnimationManager.Instance.PlayAnimation(a);
    }

    private void ChangeSceneAction(List<string> parameters)
    {
        string s = GetParameterValue(parameters, "scene_name");
        if (!string.IsNullOrEmpty(s)) UnityEngine.SceneManagement.SceneManager.LoadScene(s);
    }

    private void QuestTextAction(List<string> parameters) { }

    private void TeleportPlayerAction(List<string> parameters)
    {
        string dest = GetParameterValue(parameters, "destination");
        string target = GetParameterValue(parameters, "target");
        if (string.IsNullOrEmpty(target)) target = "Player";
        GameObject tObj = GameObject.Find(target);
        GameObject dObj = GameObject.Find(dest);
        if (tObj != null && dObj != null) tObj.transform.position = dObj.transform.position;
    }

    private void ChangeSpeedAction(List<string> parameters)
    {
        if (float.TryParse(GetParameterValue(parameters, "val"), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float s)) currentTypingSpeed = s;
    }

    private void UnlockAbilityAction(List<string> parameters) { }

    private string GetParameterValue(List<string> parameters, string key)
    {
        foreach (string param in parameters)
            if (param.Trim().StartsWith(key + ":")) return param.Trim().Substring(key.Length + 1).Trim();
        return "";
    }

    private int GetIntParameterValue(List<string> parameters, string key, int defaultValue)
    {
        return int.TryParse(GetParameterValue(parameters, key), out int result) ? result : defaultValue;
    }

    private void ProcessSetTag(string tag)
    {
        string[] parts = tag.Split(' ');
        if (parts.Length == 2)
        {
            string v = parts[1].ToLower();
            if (v == "true") story.variablesState[parts[0].Substring(4)] = true;
            else if (v == "false") story.variablesState[parts[0].Substring(4)] = false;
            else story.variablesState[parts[0].Substring(4)] = parts[1];
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
                string s = tag.Substring(8).Trim();
                if (portraitLeft.gameObject.activeSelf) nameLeft.text = s;
                if (portraitRight.gameObject.activeSelf) nameRight.text = s;
            }
            else if (tag.StartsWith("portrait:"))
            {
                Sprite sp = Resources.Load<Sprite>("Portraits/" + tag.Substring(9).Trim());
                if (sp != null)
                {
                    if (portraitLeft.gameObject.activeSelf) portraitLeft.sprite = sp;
                    if (portraitRight.gameObject.activeSelf) portraitRight.sprite = sp;
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
            int idx = i;
            btn.onClick.AddListener(() => { story.ChooseChoiceIndex(idx); ContinueDialogue(); });
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