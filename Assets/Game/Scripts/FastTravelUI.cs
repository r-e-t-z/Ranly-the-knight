using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FastTravelUI : MonoBehaviour
{
    public static FastTravelUI Instance;

    [Header("UI references")]
    public GameObject menuPanel;            // �������� ������ (SetActive true/false)
    public GameObject buttonPrefab;         // ������ ������ (UI Button)
    public Transform buttonContainer;       // Content � ScrollView ��� ������ ������ � VerticalLayout
    public Transform playerTransform;       // �������� ���� ������ ������ (Transform)

    private List<Terminal> allTerminals = new List<Terminal>();
    private Terminal currentTerminal;

    private void Awake()
    {
        // ������� singleton
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        // �������� ��� ��������� �� �����
        allTerminals.Clear();
        allTerminals.AddRange(FindObjectsOfType<Terminal>());

        // �������� ���� � ������
        if (menuPanel != null) menuPanel.SetActive(false);
    }

    public void OpenMenu(Terminal current)
    {
        currentTerminal = current;
        if (menuPanel == null || buttonPrefab == null || buttonContainer == null || playerTransform == null)
        {
            Debug.LogError("FastTravelUI: �� ��� ������ ������ � ����������.");
            return;
        }

        // �������� ������
        menuPanel.SetActive(true);

        // ������� ������ ������
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        // ������ ������ ��� ������� ��������� (����� ��������)
        foreach (var t in allTerminals)
        {
            if (t == currentTerminal) continue;

            GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
            // �� ������, ���� � ������� ��� Text � ������� ����� Text ��������� ������
            Text txt = btnObj.GetComponentInChildren<Text>();
            if (txt != null) txt.text = t.terminalName;
            else Debug.LogWarning("FastTravelUI: � buttonPrefab ��� Text ���������� � ��������.");

            Button btn = btnObj.GetComponent<Button>();
            if (btn == null)
            {
                Debug.LogError("FastTravelUI: buttonPrefab ������ ��������� ��������� Button.");
                continue;
            }

            Terminal localT = t; // ������ ��������� ���������� ��� ���������
            btn.onClick.AddListener(() => {
                TeleportTo(localT);
            });
        }
    }

    public void TeleportTo(Terminal target)
    {
        if (target == null || playerTransform == null)
            return;

        // ������������� ������ � ������� ���������
        // ���� � ���� Rigidbody2D � ����� ������������ ��� ������ transform.position
        playerTransform.position = target.transform.position;

        CloseMenu();
    }

    public void CloseMenu()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
    }

    // ����� � ������, ���� �� ����� ���� ���������/��������� ���������
    public void RefreshTerminals()
    {
        allTerminals.Clear();
        allTerminals.AddRange(FindObjectsOfType<Terminal>());
    }
}
