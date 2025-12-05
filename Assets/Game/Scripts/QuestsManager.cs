using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuestsManager : MonoBehaviour
{
    public static QuestsManager Instance;
    [Header("Ui")]

    public GameObject questPanel;
    public GameObject questTextButtonPrefab;
    public Transform choicesPanel;

    public List<string> questsTexts = new List<string>();

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            OpenQuestsList();
            
        }
    }

    public void OpenQuestsList()
    {
        bool newState = !questPanel.activeInHierarchy;
        if (newState == false)
        {
            DeleteQuestsList();
            questPanel.SetActive(false);
        }
        else
        {
            ShowQuestsText();
            questPanel.SetActive(true);
        }
    }

    public void ShowQuestsText()
    {
        foreach(string questText in questsTexts)
        {
            GameObject button = Instantiate(questTextButtonPrefab, choicesPanel);
            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
            buttonText.text = questText;
            Debug.Log("ShowQuestsText - Это делается");
        }
    }

    public void DeleteQuestsList()
    {
        foreach (Transform child in choicesPanel)
        {
            Destroy(child.gameObject);
        }

    }

    public void AddQuestsTexts(string questText)
    {    
        questsTexts.Add(questText);
        Debug.Log("AddQuestsTexts - Это делается");
    }
}
