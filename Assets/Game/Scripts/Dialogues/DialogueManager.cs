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

    private int activeCutscenesCount = 0;

    private class ActionData
    {
        public string name;
        public List<string> paramsList = new List<string>();
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

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
        List<ActionData> actionsToExecute = new List<ActionData>();
        ActionData currentAction = null;

        foreach (string tag in currentTags)
        {
            string cleanTag = tag.Trim();
            if (cleanTag.StartsWith("action:"))
            {
                currentAction = new ActionData();
                currentAction.name = cleanTag.Substring(7).Trim();
                actionsToExecute.Add(currentAction);
            }
            else if (cleanTag == "delay" || cleanTag.StartsWith("delay:"))
            {
                currentAction = new ActionData { name = "delay" };
                if (cleanTag.Contains(":")) currentAction.paramsList.Add(cleanTag);
                actionsToExecute.Add(currentAction);
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

        foreach (var act in actionsToExecute)
        {
            ExecuteAction(act.name, act.paramsList);

            // Если действие включило режим ожидания (например, delay)
            while (isWaiting)
            {
                yield return null;
            }
        }

        // Если мы не в режиме ожидания #delay, показываем панель и печатаем текст
        if (!isWaiting)
        {
            dialoguePanel.SetActive(true);
            ApplyVisualTags();
            typingCoroutine = StartCoroutine(TypewriterRoutine(currentFullLine));
        }
    }

    private void ExecuteAction(string actionType, List<string> parameters)
    {
        switch (actionType)
        {
            case "play_cutscene": PlayCutsceneAction(parameters); break;
            case "delay": DelayAction(parameters); break;
            case "camera_target": CameraTargetAction(parameters); break;
            case "give_item": GiveItemAction(parameters); break;
            case "take_item": TakeItemAction(parameters); break;
            case "quest_add": AddQuestAction(parameters); break;
            case "quest_add_item": AddQuestItemAction(parameters); break;
            case "quest_complete": CompleteQuestAction(parameters); break;
            case "activate_trigger": ActivateTriggerAction(parameters); break;
            case "deactivate_object": DeactivateObjectAction(parameters); break;
            case "activate_object": ActivateObjectAction(parameters); break;
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
            activeCutscenesCount++;
            dialoguePanel.SetActive(false); // Скрываем панель сразу при запуске анимации
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

        activeCutscenesCount--;
    }

    private void DelayAction(List<string> parameters)
    {
        float time = -1f;
        string val = GetParameterValue(parameters, "time");
        if (!string.IsNullOrEmpty(val) && float.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float t))
        {
            time = t;
        }

        StartCoroutine(WaitRoutine(time));
    }

    private IEnumerator WaitRoutine(float time)
    {
        isWaiting = true;
        dialoguePanel.SetActive(false);

        if (time > 0)
        {
            yield return new WaitForSeconds(time);
        }
        else
        {
            // Ждем, пока все запущенные катсцены закончатся
            while (activeCutscenesCount > 0)
            {
                yield return null;
            }
        }

        isWaiting = false;

        // Показываем панель только если не осталось активных катсцен
        if (activeCutscenesCount <= 0)
        {
            dialoguePanel.SetActive(true);
        }
    }

    // --- ОСТАЛЬНЫЕ МЕТОДЫ (Typewriter, Inventory, etc.) ---
    // Они остались без изменений, я включил их, чтобы скрипт был целым

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

    private void UpdatePlayerStateVariables()
    {
        if (story == null) return;
        List<string> names = new List<string>(story.variablesState);
        foreach (string n in names)
        {
            if (n.StartsWith("has_item_"))
            {
                string[] p = n.Split('_');
                if (p.Length >= 4 && int.TryParse(p[3], out int amt))
                    story.variablesState[n] = CheckActiveSlotForItem(p[2], amt);
            }
        }
    }

    private bool CheckActiveSlotForItem(string id, int amt)
    {
        if (InventoryManager.Instance == null) return false;
        return InventoryManager.Instance.GetActiveSlotItemCount(id) >= amt;
    }

    private void RestoreGlobalVariables()
    {
        foreach (var v in globalVariables) { try { story.variablesState[v.Key] = v.Value; } catch { } }
    }

    private void SaveGlobalVariables()
    {
        if (story == null) return;
        foreach (string v in story.variablesState) globalVariables[v] = story.variablesState[v];
    }

    private void PlayAnimationSequenceAction(List<string> p)
    {
        string t = GetParameterValue(p, "target");
        string a = GetParameterValue(p, "animations");
        if (!string.IsNullOrEmpty(t) && !string.IsNullOrEmpty(a) && AnimationManager.Instance != null)
            AnimationManager.Instance.PlaySequenceOnObject(t, a.Split(',').Select(s => s.Trim()).ToArray());
    }

    private void CameraTargetAction(List<string> p)
    {
        string t = GetParameterValue(p, "target");
        if (!string.IsNullOrEmpty(t) && CameraController.Instance != null) CameraController.Instance.SetTarget(t);
    }

    private void AddQuestAction(List<string> p)
    {
        if (QuestsManager.Instance != null) QuestsManager.Instance.AddQuest(GetParameterValue(p, "id"), GetParameterValue(p, "desc"));
    }

    private void AddQuestItemAction(List<string> p)
    {
        if (QuestsManager.Instance != null) QuestsManager.Instance.AddQuest(GetParameterValue(p, "id"), GetParameterValue(p, "desc"), GetParameterValue(p, "item_id"), GetIntParameterValue(p, "amount", 1));
    }

    private void CompleteQuestAction(List<string> p)
    {
        if (QuestsManager.Instance != null) QuestsManager.Instance.CompleteQuest(GetParameterValue(p, "id"));
    }

    private void GiveItemAction(List<string> p)
    {
        if (InventoryManager.Instance != null) InventoryManager.Instance.AddItem(GetParameterValue(p, "item_id"), GetIntParameterValue(p, "amount", 1));
    }

    private void TakeItemAction(List<string> p)
    {
        if (InventoryManager.Instance != null) InventoryManager.Instance.RemoveItemFromActiveSlot(GetParameterValue(p, "item_id"), GetIntParameterValue(p, "amount", 1));
    }

    private void ActivateTriggerAction(List<string> p)
    {
        GameObject g = GameObject.Find(GetParameterValue(p, "trigger_name"));
        if (g != null && g.GetComponent<Collider2D>()) g.GetComponent<Collider2D>().enabled = true;
    }

    private void DeactivateObjectAction(List<string> p)
    {
        GameObject g = GameObject.Find(GetParameterValue(p, "object_name"));
        if (g != null) g.SetActive(false);
    }

    private void ActivateObjectAction(List<string> p)
    {
        string objName = GetParameterValue(p, "object_name");
        string parentName = GetParameterValue(p, "parent_name"); // Опциональный параметр

        GameObject target = null;

        // ВАРИАНТ 1: Если указан родитель (Самый надежный способ для выключенных объектов)
        if (!string.IsNullOrEmpty(parentName))
        {
            GameObject parent = GameObject.Find(parentName);
            if (parent != null)
            {
                // transform.Find находит даже выключенные дочерние объекты!
                Transform child = parent.transform.Find(objName);
                if (child != null) target = child.gameObject;
                else Debug.LogWarning($"Родитель '{parentName}' найден, но внутри нет '{objName}'");
            }
            else
            {
                Debug.LogWarning($"Родитель '{parentName}' не найден на сцене (он должен быть активен)");
            }
        }
        // ВАРИАНТ 2: Пытаемся найти просто по имени (Сработает ТОЛЬКО если объект уже активен, что редко имеет смысл)
        else
        {
            target = GameObject.Find(objName);
        }

        if (target != null)
        {
            target.SetActive(true);
            Debug.Log($"Объект '{objName}' активирован.");
        }
        else
        {
            Debug.LogWarning($"Не удалось найти объект '{objName}' для активации. Совет: если объект выключен, укажите его родителя через parent_name.");
        }
    }

    private void StartAnimationAction(List<string> p)
    {
        string a = GetParameterValue(p, "animation_name");
        string ans = GetParameterValue(p, "animation_names");
        if (!string.IsNullOrEmpty(ans)) AnimationManager.Instance.PlayMultipleAnimations(ans.Split(','));
        else if (!string.IsNullOrEmpty(a)) AnimationManager.Instance.PlayAnimation(a);
    }

    private void ChangeSceneAction(List<string> p)
    {
        string s = GetParameterValue(p, "scene_name");
        if (!string.IsNullOrEmpty(s)) UnityEngine.SceneManagement.SceneManager.LoadScene(s);
    }

    private void QuestTextAction(List<string> p) { }

    private void TeleportPlayerAction(List<string> p)
    {
        string dest = GetParameterValue(p, "destination");
        string t = GetParameterValue(p, "target");
        if (string.IsNullOrEmpty(t)) t = "Player";
        GameObject obj = GameObject.Find(t);
        GameObject dObj = GameObject.Find(dest);
        if (obj != null && dObj != null) obj.transform.position = dObj.transform.position;
    }

    private void ChangeSpeedAction(List<string> p)
    {
        if (float.TryParse(GetParameterValue(p, "val"), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float s)) currentTypingSpeed = s;
    }

    private void UnlockAbilityAction(List<string> p) { }

    private string GetParameterValue(List<string> p, string k)
    {
        foreach (string s in p) if (s.Trim().StartsWith(k + ":")) return s.Trim().Substring(k.Length + 1).Trim();
        return "";
    }

    private int GetIntParameterValue(List<string> p, string k, int d)
    {
        return int.TryParse(GetParameterValue(p, k), out int r) ? r : d;
    }

    private void ProcessSetTag(string t)
    {
        string[] p = t.Split(' ');
        if (p.Length == 2)
        {
            string v = p[1].ToLower();
            if (v == "true") story.variablesState[p[0].Substring(4)] = true;
            else if (v == "false") story.variablesState[p[0].Substring(4)] = false;
            else story.variablesState[p[0].Substring(4)] = p[1];
        }
    }

    private void ApplyVisualTags()
    {
        portraitLeft.gameObject.SetActive(false);
        portraitRight.gameObject.SetActive(false);
        nameLeft.gameObject.SetActive(false);
        nameRight.gameObject.SetActive(false);

        foreach (string t in story.currentTags)
        {
            if (t == "side:left") { portraitLeft.gameObject.SetActive(true); nameLeft.gameObject.SetActive(true); }
            else if (t == "side:right") { portraitRight.gameObject.SetActive(true); nameRight.gameObject.SetActive(true); }
            else if (t.StartsWith("speaker:"))
            {
                string s = t.Substring(8).Trim();
                if (portraitLeft.gameObject.activeSelf) nameLeft.text = s;
                if (portraitRight.gameObject.activeSelf) nameRight.text = s;
            }
            else if (t.StartsWith("portrait:"))
            {
                Sprite s = Resources.Load<Sprite>("Portraits/" + t.Substring(9).Trim());
                if (s != null)
                {
                    if (portraitLeft.gameObject.activeSelf) portraitLeft.sprite = s;
                    if (portraitRight.gameObject.activeSelf) portraitRight.sprite = s;
                }
            }
        }
    }

    private void SetDefaultSpeaker(NPCData n)
    {
        portraitLeft.gameObject.SetActive(true); nameLeft.gameObject.SetActive(true);
        nameLeft.text = n.npcName; if (n.portrait != null) portraitLeft.sprite = n.portrait;
        currentVoiceSound = n.voiceSound;
        currentTypingSpeed = n.typingSpeed > 0 ? n.typingSpeed : 0.04f;
    }

    private void ShowChoices()
    {
        choicesContainer.gameObject.SetActive(true);
        for (int i = 0; i < story.currentChoices.Count; i++)
        {
            Choice c = story.currentChoices[i];
            GameObject b = Instantiate(choiceButtonPrefab, choicesContainer);
            b.GetComponentInChildren<TMP_Text>().text = c.text;
            int idx = i;
            b.GetComponent<Button>().onClick.AddListener(() => { story.ChooseChoiceIndex(idx); ContinueDialogue(); });
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