using UnityEngine;
using System.Collections.Generic;
using TMPro;          
using UnityEngine.UI; 

[System.Serializable]
public class Quest
{
    public string id;            
    public string description;   
    public bool isCompleted;     

    public string requiredItemID; 
    public int requiredAmount;  
    public int currentAmount;    

    public Quest(string id, string desc, string itemID = "", int amount = 0)
    {
        this.id = id;
        this.description = desc;
        this.requiredItemID = itemID;
        this.requiredAmount = amount;
        this.isCompleted = false;
        this.currentAmount = 0;
    }

    public string GetDisplayText()
    {
        if (string.IsNullOrEmpty(requiredItemID) || isCompleted)
        {
            return description;
        }

        else
        {
            return $"{description} ({currentAmount}/{requiredAmount})";
        }
    }
}

public class QuestsManager : MonoBehaviour
{
    public static QuestsManager Instance;

    [Header("Настройки Журнала (UI)")]
    public GameObject questPanel;    
    public Transform choicesPanel;      
    public GameObject questButtonPrefab; 

    [Header("Текст на экране (HUD)")]
    public TMP_Text activeQuestHUD;    

    public List<Quest> allQuests = new List<Quest>();

    private Quest trackedQuest;

    void Awake()
    {
        Instance = this;
        if (activeQuestHUD != null) activeQuestHUD.text = "";
    }

    void Start()
    {
        InventoryManager.OnInventoryChanged += OnInventoryUpdate;
    }

    void OnDestroy()
    {
        InventoryManager.OnInventoryChanged -= OnInventoryUpdate;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            ToggleQuestsList();
        }
    }

    private void OnInventoryUpdate(InventorySlot[] slots)
    {
        bool needsUpdate = false;

        foreach (var quest in allQuests)
        {
            if (!quest.isCompleted && !string.IsNullOrEmpty(quest.requiredItemID))
            {
                int count = InventoryManager.Instance.GetItemCount(quest.requiredItemID);

                if (quest.currentAmount != count)
                {
                    quest.currentAmount = count;
                    needsUpdate = true;
                }
            }
        }

        if (needsUpdate)
        {
            UpdateHUD();
            if (questPanel.activeInHierarchy) ShowQuestsInJournal();
        }
    }

    public void ToggleQuestsList()
    {
        bool isActive = questPanel.activeInHierarchy;
        if (isActive)
        {
            questPanel.SetActive(false);
            DeleteQuestsList();
        }
        else
        {
            questPanel.SetActive(true);
            ShowQuestsInJournal();
        }
    }

    public void AddQuest(string id, string desc, string itemID = "", int amount = 0)
    {
        if (allQuests.Exists(x => x.id == id)) return;

        Quest newQuest = new Quest(id, desc, itemID, amount);

        if (!string.IsNullOrEmpty(itemID))
        {
            newQuest.currentAmount = InventoryManager.Instance.GetItemCount(itemID);
        }

        allQuests.Add(newQuest);

        TrackQuest(newQuest);
    }

    public void CompleteQuest(string id)
    {
        Quest quest = allQuests.Find(x => x.id == id);
        if (quest != null)
        {
            quest.isCompleted = true;

            if (trackedQuest == quest)
            {
                activeQuestHUD.text = "";
                trackedQuest = null;
            }

            if (questPanel.activeInHierarchy) ShowQuestsInJournal();
        }
    }

    public void TrackQuest(Quest quest)
    {
        if (quest.isCompleted) return;

        trackedQuest = quest;
        UpdateHUD();
    }

    private void UpdateHUD()
    {
        if (activeQuestHUD == null) return;

        if (trackedQuest != null && !trackedQuest.isCompleted)
        {
            activeQuestHUD.text = trackedQuest.GetDisplayText();
        }
        else
        {
            activeQuestHUD.text = "";
        }
    }

    public void ShowQuestsInJournal()
    {
        DeleteQuestsList();

        foreach (Quest quest in allQuests)
        {
            GameObject buttonObj = Instantiate(questButtonPrefab, choicesPanel);
            TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>();
            Button btn = buttonObj.GetComponent<Button>();

            if (quest.isCompleted)
            {

                buttonText.text = $"<s>{quest.description}</s> <color=black></color>";
                buttonText.color = Color.gray;
                btn.interactable = false; 
            }
            else
            {

                buttonText.text = quest.GetDisplayText();
                if (trackedQuest == quest) buttonText.color = Color.black;

                else buttonText.color = Color.white;


                btn.onClick.AddListener(() => {
                    TrackQuest(quest);
                    ShowQuestsInJournal();
                });
            }
        }
    }

    public void DeleteQuestsList()
    {
        foreach (Transform child in choicesPanel)
        {
            Destroy(child.gameObject);
        }
    }
}