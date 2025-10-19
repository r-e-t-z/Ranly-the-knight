using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("Pause UI")]
    public Canvas pauseCanvas;
    public GameObject pausePanel;

    [Header("Settings UI")]
    public GameObject settingsPanel; // ������ �������� �� �������� ����

    [Header("Buttons")]
    public Button continueButton;
    public Button settingsButton;
    public Button menuButton;

    [Header("Settings Buttons")]
    public Button settingsBackButton; // ������ "�����" � ����������

    private bool isPaused = false;
    private bool inSettings = false;

    void Start()
    {
        // �������� ������ � ������
        if (pauseCanvas != null)
            pauseCanvas.enabled = false;

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        // ����������� ������ �����
        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueGame);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        if (menuButton != null)
            menuButton.onClick.AddListener(GoToMainMenu);

        // ����������� ������ ����� � ����������
        if (settingsBackButton != null)
            settingsBackButton.onClick.AddListener(CloseSettings);
    }

    void Update()
    {
        // ��������� ������� ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (inSettings)
            {
                // ���� � ���������� - ��������� ��
                CloseSettings();
            }
            else if (isPaused)
            {
                // ���� � ����� - ���������� ����
                ContinueGame();
            }
            else
            {
                // ���� ���� ���� - ������ �� �����
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        inSettings = false;

        // ������������� ����� ����
        Time.timeScale = 0f;

        // ���������� ������ �����
        if (pauseCanvas != null)
            pauseCanvas.enabled = true;

        // �������� ��������� ���� ���� �������
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        // ������ ������ �������
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ContinueGame()
    {
        isPaused = false;
        inSettings = false;

        // ������������ ����� ����
        Time.timeScale = 1f;

        // �������� ��� UI
        if (pauseCanvas != null)
            pauseCanvas.enabled = false;

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        // ���������� ������ (���� �����)
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void OpenSettings()
    {
        inSettings = true;

        // �������� ������ �����
        if (pausePanel != null)
            pausePanel.SetActive(false);

        // ���������� ���������
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    void CloseSettings()
    {
        inSettings = false;

        // ���������� ������ �����
        if (pausePanel != null)
            pausePanel.SetActive(true);

        // �������� ���������
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    void GoToMainMenu()
    {
        // ������������ ����� ����� ��������� ����
        Time.timeScale = 1f;

        // ��������� ������� ����
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    // ��� �������� ��������� ����� �� ������ ��������
    public bool IsGamePaused()
    {
        return isPaused;
    }
}