using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("Панели")]
    public GameObject pausePanel;    // Панель паузы
    public GameObject settingsPanel; // Панель настроек

    [Header("Кнопки паузы")]
    public Button settingsButton;
    public Button continueButton;
    public Button menuButton;

    [Header("Кнопка назад в настройках")]
    public Button backButton;

    private bool isPaused = false;

    void Start()
    {
        // ВСЕГДА скрываем обе панели в начале
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);

        // Настраиваем кнопки
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
                // Если в настройках - закрываем их
                if (settingsPanel.activeSelf)
                {
                    CloseSettings();
                }
                // Если в паузе - продолжаем игру
                else
                {
                    ContinueGame();
                }
            }
            else
            {
                // Если игра идет - ставим на паузу
                PauseGame();
            }
        }
    }

    void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // ОСТАНАВЛИВАЕМ время

        // Показываем ТОЛЬКО панель паузы
        pausePanel.SetActive(true);
        settingsPanel.SetActive(false);

        Debug.Log("Игра на паузе");
    }

    void ContinueGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // ВОЗОБНОВЛЯЕМ время

        // Скрываем ВСЕ панели
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);

        Debug.Log("Игра продолжена");
    }

    void OpenSettings()
    {
        // Скрываем паузу, показываем настройки
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);

        Debug.Log("Открыты настройки");
    }

    void CloseSettings()
    {
        Debug.Log("=== CloseSettings вызван ===");
        Debug.Log("settingsPanel активна: " + settingsPanel.activeSelf);
        Debug.Log("pausePanel активна: " + pausePanel.activeSelf);

        // Скрываем настройки
        settingsPanel.SetActive(false);
        Debug.Log("settingsPanel скрыта");

        // Показываем паузу
        pausePanel.SetActive(true);
        Debug.Log("pausePanel показана");

        Debug.Log("pausePanel теперь активна: " + pausePanel.activeSelf);
    }

    void GoToMainMenu()
    {
        Time.timeScale = 1f; // Восстанавливаем время перед загрузкой меню
        Debug.Log("Выход в главное меню");
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}