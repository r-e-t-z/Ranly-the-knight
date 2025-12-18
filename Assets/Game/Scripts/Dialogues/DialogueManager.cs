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
            // 1. Отключаем управление
            playerController.enabled = false;

            // 2. Тормозим физику
            Rigidbody2D rb = playerController.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            // 3. Ищем Аниматор (ТЕПЕРЬ ИЩЕМ И В ДОЧЕРНИХ ОБЪЕКТАХ)
            // Было: GetComponent<Animator>()
            // Стало: GetComponentInChildren<Animator>()
            Animator anim = playerController.GetComponentInChildren<Animator>();

            if (anim != null)
            {
                // Сбрасываем скорость
                anim.SetFloat("Speed", 0f);

                // Если есть другие параметры бега, сбросьте их тоже:
                // anim.SetBool("IsMoving", false);

                // Принудительно включаем Idle, чтобы Blend Tree выбрало сторону
                // Убедитесь, что стейт называется именно "Idle"
                anim.Play("Idle");

                anim.Update(0f);
            }
            else
            {
                Debug.LogWarning("DialogueManager: Аниматор игрока не найден! Проверьте, где висит компонент Animator.");
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

    // --- СИНХРОНИЗАЦИЯ ПЕРЕМЕННЫХ ---

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

    // --- УПРАВЛЕНИЕ ДИАЛОГОМ ---

    public void ContinueDialogue()
    {
        // Если ждем катсцену, не продолжаем
        if (isWaiting) return;

        foreach (Transform child in choicesContainer) Destroy(child.gameObject);

        if (story.canContinue)
        {
            string text = story.Continue();
            dialogueText.text = text.Trim();

            ProcessAllTags();

            // Обновляем визуал только если не ушли в режим ожидания
            if (!isWaiting) ApplyVisualTags();
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

    private void ExecuteAction(string actionType, List<string> parameters)
    {
        switch (actionType)
        {
            // === НОВЫЕ ДЕЙСТВИЯ ===
            case "play_cutscene":
                PlayCutsceneAction(parameters);
                break;
            case "camera_target":
                CameraTargetAction(parameters);
                break;
            // =======================

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

            default:
                Debug.LogWarning($"Неизвестное действие: {actionType}");
                break;
        }
    }

    // --- РЕАЛИЗАЦИЯ ДЕЙСТВИЙ ---

    // #action:play_cutscene animation_name:KnightAttack
    // #action:play_cutscene target:Player animation_name:Dissappear move_after:TeleportSpot_1
    private void PlayCutsceneAction(List<string> parameters)
    {
        string targetName = GetParameterValue(parameters, "target");       // Кто анимируется
        string animName = GetParameterValue(parameters, "animation_name"); // Какая анимация
        string destination = GetParameterValue(parameters, "move_after");  // Куда телепортироваться

        // НОВЫЙ ПАРАМЕТР: Кто именно должен телепортироваться
        string moveTargetName = GetParameterValue(parameters, "move_target");

        // Если target не указан, пробуем старую логику
        if (string.IsNullOrEmpty(targetName))
        {
            targetName = animName;
        }

        if (!string.IsNullOrEmpty(targetName) && !string.IsNullOrEmpty(animName))
        {
            // Передаем все параметры, включая новый moveTargetName
            StartCoroutine(PlayCutsceneRoutine(targetName, animName, destination, moveTargetName));
        }
    }

    // Обновленная корутина с отдельным аргументом для объекта перемещения
    private IEnumerator PlayCutsceneRoutine(string targetName, string animName, string destination, string moveTargetName)
    {
        isWaiting = true;
        dialoguePanel.SetActive(false);

        // 1. Запускаем анимацию (на том объекте, который указан в target)
        if (AnimationManager.Instance != null)
        {
            AnimationManager.Instance.PlayAnimation(targetName, animName);
        }

        // 2. Ждем пока она закончится
        float duration = 1.5f;
        if (AnimationManager.Instance != null)
        {
            duration = AnimationManager.Instance.GetAnimationLength(targetName, animName);
            if (duration <= 0) duration = 1.5f;
        }

        yield return new WaitForSeconds(duration);

        // 3. ТЕЛЕПОРТАЦИЯ
        if (!string.IsNullOrEmpty(destination))
        {
            // Логика выбора: если move_target указан явно -> берем его.
            // Если нет -> берем объект, который проигрывал анимацию (targetName).
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

    // #action:camera_target target:Ranly
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

    // Заглушки для твоих методов (реализуй если нужно)
    private void QuestTextAction(List<string> parameters) { }

    private void TeleportPlayerAction(List<string> parameters)
    {
        // Получаем имя точки назначения из параметров Ink
        string destinationName = GetParameterValue(parameters, "destination");

        // Получаем имя объекта, который нужно телепортировать (по умолчанию Player)
        string targetName = GetParameterValue(parameters, "target");
        if (string.IsNullOrEmpty(targetName)) targetName = "Player";

        GameObject targetObj = GameObject.Find(targetName);
        GameObject destinationObj = GameObject.Find(destinationName);

        if (targetObj != null && destinationObj != null)
        {
            // ВАЖНО: Если на игроке есть Rigidbody2D, лучше использовать MovePosition или временно отключить физику
            // Но для простого телепорта transform.position подойдет
            targetObj.transform.position = destinationObj.transform.position;

            Debug.Log($"Телепортация {targetName} в точку {destinationName}");
        }
        else
        {
            Debug.LogWarning($"Не удалось телепортировать. Цель: {targetObj}, Точка: {destinationObj}");
        }
    }

    private void UnlockAbilityAction(List<string> parameters) { }

    // --- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ---

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

        // Возвращаем камеру на игрока (если был CameraController)
        if (CameraController.Instance != null) CameraController.Instance.SetTarget("Player");

        currentNPC = null;
    }

    public bool IsPlaying() => isPlaying;
}