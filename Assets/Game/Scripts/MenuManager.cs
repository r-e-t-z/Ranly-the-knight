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
        // Инициализация UI
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);

        // Настройка кнопок главного меню
        continueButton.onClick.AddListener(OnContinueButton);
        newGameButton.onClick.AddListener(OnNewGameButton);
        settingsButton.onClick.AddListener(OnSettingsButton);
        exitButton.onClick.AddListener(OnExitButton);

        // Настройка кнопки назад в настройках
        backButton.onClick.AddListener(OnBackButton);

        // Обновляем состояние кнопки "Продолжить"
        UpdateContinueButton();
    }

    void UpdateContinueButton()
    {
        // Скрываем/показываем кнопку в зависимости от наличия сохранения
        bool hasSave = SaveSystem.HasSaveData();
        continueButton.gameObject.SetActive(hasSave);

        // Если нужно оставить кнопку видимой но неактивной, используй:
        // continueButton.interactable = hasSave;
    }

    public void OnContinueButton()
    {
        // Всегда проверяем наличие сохранения перед загрузкой
        if (SaveSystem.HasSaveData())
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogWarning("Нет сохранения для загрузки!");
            // Можно показать сообщение игроку
            UpdateContinueButton(); // Обновляем на всякий случай
        }
    }

    public void OnNewGameButton()
    {
        // Удаляем сохранение при начале новой игры
        SaveSystem.DeleteSave();
        SceneManager.LoadScene(gameSceneName);
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
        Debug.Log("Выход из игры...");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // В билде - закрываем приложение
        Application.Quit();
        #endif
    }
}