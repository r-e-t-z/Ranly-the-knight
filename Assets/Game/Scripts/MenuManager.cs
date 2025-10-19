using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("������� ����")]
    public GameObject mainMenuPanel;    // ������ � �������� �������� ����

    [Header("���� ��������")]
    public GameObject settingsPanel;    // ������ � �����������


    void Start()
    {
        // ����������: ������� ���� �����, ��������� ������, ���� ��������
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    // ��� ������� ������ ��� ������� ������ "���������"
    public void OnSettingsButton()
    {
        // �������� ������� ����
        mainMenuPanel.SetActive(false);

        // ���������� ���������
        settingsPanel.SetActive(true);
    }

    // ��� ������� ������ ��� ������� ������ "�����" � ����������
    public void OnBackButton()
    {
        // ���������� ������� ����
        mainMenuPanel.SetActive(true);


        // �������� ���������
        settingsPanel.SetActive(false);
    }
}