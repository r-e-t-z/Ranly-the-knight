using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("Панели")]
    public GameObject pausePanel;
    public GameObject settingsPanel;

    [Header("Кнопки паузы")]
    public Button settingsButton;
    public Button continueButton;
    public Button menuButton;

    [Header("Кнопка назад в настройках")]
    public Button backButton;

    private bool isPaused = false;

    void Start()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);

        settingsButton.onClick.AddListener(OpenSettings);
        continueButton.onClick.AddListener(ContinueGame);
        menuButton.onClick.AddListener(GoToMainMenu);
        backButton.onClick.AddListener(CloseSettings);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                if (settingsPanel.activeSelf)
                {
                    CloseSettings();
                }
                else
                {
                    ContinueGame();
                }
            }
            else
            {
                PauseGame();
            }
        }
    }

    void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        pausePanel.SetActive(true);
        settingsPanel.SetActive(false);

        Debug.Log("Игра на паузе");
    }

    void ContinueGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);

        Debug.Log("Игра продолжена");
    }

    void OpenSettings()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    void CloseSettings()
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    void GoToMainMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}