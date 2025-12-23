using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Главное меню")]
    public GameObject mainMenuPanel;
    public Button continueButton;
    public Button newGameButton;
    public Button settingsButton;
    public Button exitButton;

    [Header("Меню настроек")]
    public GameObject settingsPanel;
    public Button backButton;

    [Header("Настройки")]
    public string gameSceneName = "GameScene";

    void Start()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);

        continueButton.onClick.AddListener(OnContinueButton);
        newGameButton.onClick.AddListener(OnNewGameButton);
        settingsButton.onClick.AddListener(OnSettingsButton);
        exitButton.onClick.AddListener(OnExitButton);

        backButton.onClick.AddListener(OnBackButton);

        UpdateContinueButton();
    }

    void UpdateContinueButton()
    {
        bool hasSave = SaveSystem.HasSaveData();
        continueButton.gameObject.SetActive(hasSave);
    }

    public void OnContinueButton()
    {
        if (SaveSystem.HasSaveData())
        {
            // Вместо обычной загрузки сцены вызываем ПОЛНУЮ загрузку
            if (GameSaveManager.Instance != null)
            {
                GameSaveManager.Instance.LoadGame();
            }
            else
            {
                // Если менеджер не найден на сцене меню, попробуем найти его через Find
                GameSaveManager manager = FindObjectOfType<GameSaveManager>();
                if (manager != null)
                {
                    manager.LoadGame();
                }
                else
                {
                    Debug.LogError("Критическая ошибка: GameSaveManager не найден на сцене Главного меню!");
                }
            }
        }
    }

    public void OnNewGameButton()
    {
        if (GameSaveManager.Instance != null)
        {
            // Сбрасываем данные перед началом
            GameSaveManager.Instance.PrepareNewGame();
        }

        // Загружаем начальную сцену
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);
    }

    public void OnSettingsButton()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void OnBackButton()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    public void OnExitButton()
    {

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else

        Application.Quit();
        #endif
    }
}