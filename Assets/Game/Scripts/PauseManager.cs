using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("Pause UI")]
    public Canvas pauseCanvas;
    public GameObject pausePanel;

    [Header("Settings UI")]
    public GameObject settingsPanel; // Панель настроек из главного меню

    [Header("Buttons")]
    public Button continueButton;
    public Button settingsButton;
    public Button menuButton;

    [Header("Settings Buttons")]
    public Button settingsBackButton; // Кнопка "Назад" в настройках

    private bool isPaused = false;
    private bool inSettings = false;

    void Start()
    {
        // Скрываем панели в начале
        if (pauseCanvas != null)
            pauseCanvas.enabled = false;

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        // Настраиваем кнопки паузы
        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueGame);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        if (menuButton != null)
            menuButton.onClick.AddListener(GoToMainMenu);

        // Настраиваем кнопку назад в настройках
        if (settingsBackButton != null)
            settingsBackButton.onClick.AddListener(CloseSettings);
    }

    void Update()
    {
        // Проверяем нажатие ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (inSettings)
            {
                // Если в настройках - закрываем их
                CloseSettings();
            }
            else if (isPaused)
            {
                // Если в паузе - продолжаем игру
                ContinueGame();
            }
            else
            {
                // Если игра идет - ставим на паузу
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        inSettings = false;

        // Останавливаем время игры
        Time.timeScale = 0f;

        // Показываем панель паузы
        if (pauseCanvas != null)
            pauseCanvas.enabled = true;

        // Скрываем настройки если были открыты
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        // Делаем курсор видимым
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ContinueGame()
    {
        isPaused = false;
        inSettings = false;

        // Возобновляем время игры
        Time.timeScale = 1f;

        // Скрываем все UI
        if (pauseCanvas != null)
            pauseCanvas.enabled = false;

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        // Возвращаем курсор (если нужно)
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void OpenSettings()
    {
        inSettings = true;

        // Скрываем панель паузы
        if (pausePanel != null)
            pausePanel.SetActive(false);

        // Показываем настройки
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
        
    }

    void CloseSettings()
    {
        inSettings = false;

        // Показываем панель паузы
        if (pausePanel != null)
            pausePanel.SetActive(true);

        // Скрываем настройки
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    void GoToMainMenu()
    {
        // Возобновляем время перед загрузкой меню
        Time.timeScale = 1f;

        // Загружаем главное меню
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    // Для проверки состояния паузы из других скриптов
    public bool IsGamePaused()
    {
        return isPaused;
    }
}