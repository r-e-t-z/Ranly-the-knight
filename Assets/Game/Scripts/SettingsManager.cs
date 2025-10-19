using UnityEngine;
using UnityEngine.UI;

public class SimpleSettingsManager : MonoBehaviour
{
    [Header("Настройки звука")]
    public Slider volumeSlider;

    [Header("UI")]
    public GameObject settingsPanel;
    public Button backButton;

    // Dropdown будем находить автоматически
    private Dropdown screenModeDropdown;

    void Start()
    {
        // Находим Dropdown автоматически
        screenModeDropdown = settingsPanel.GetComponentInChildren<Dropdown>();

        // Инициализация громкости
        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
            volumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        // Инициализация режима экрана
        if (screenModeDropdown != null)
        {
            screenModeDropdown.value = Screen.fullScreen ? 0 : 1;
            screenModeDropdown.onValueChanged.AddListener(SetFullscreen);
        }

        // Кнопка назад
        if (backButton != null)
            backButton.onClick.AddListener(CloseSettings);
    }

    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetFullscreen(int isFullscreen)
    {
        bool fullscreen = (isFullscreen == 0);
        Screen.fullScreen = fullscreen;
    }

    public void CloseSettings()
    {
        PlayerPrefs.Save();
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }
}