using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FastTravelUI : MonoBehaviour
{
    public static FastTravelUI Instance;

    [Header("UI references")]
    public GameObject menuPanel;    
    public GameObject buttonPrefab;         
    public Transform buttonContainer;      
    public Transform playerTransform;       

    private List<Terminal> allTerminals = new List<Terminal>();
    private Terminal currentTerminal;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        allTerminals.Clear();
        allTerminals.AddRange(FindObjectsOfType<Terminal>());

        if (menuPanel != null) menuPanel.SetActive(false);
    }

    public void OpenMenu(Terminal current)
    {
        currentTerminal = current;
        if (menuPanel == null || buttonPrefab == null || buttonContainer == null || playerTransform == null)
        {
            return;
        }

        menuPanel.SetActive(true);

        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        foreach (var t in allTerminals)
        {
            if (t == currentTerminal) continue;

            GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);

            Text txt = btnObj.GetComponentInChildren<Text>();
            if (txt != null) txt.text = t.terminalName;

            Button btn = btnObj.GetComponent<Button>();
            if (btn == null)
            {
                continue;
            }

            Terminal localT = t;
            btn.onClick.AddListener(() => {
                TeleportTo(localT);
            });
        }
    }

    public void TeleportTo(Terminal target)
    {
        if (target == null || playerTransform == null)
            return;

        playerTransform.position = target.transform.position;

        CloseMenu();
    }

    public void CloseMenu()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
    }

    public void RefreshTerminals()
    {
        allTerminals.Clear();
        allTerminals.AddRange(FindObjectsOfType<Terminal>());
    }
}
