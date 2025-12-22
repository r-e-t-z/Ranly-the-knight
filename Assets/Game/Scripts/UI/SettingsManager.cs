using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Настройки звука")]
    public Slider volumeSlider;

    [Header("UI Панели")]
    public GameObject settingsPanel;
    public Button backButton;

    // В новых версиях Unity лучше использовать TMPro.TMP_Dropdown, 
    // но оставляю стандартный для совместимости с твоим кодом
    private Dropdown screenModeDropdown;

    void Start()
    {
        screenModeDropdown = settingsPanel.GetComponentInChildren<Dropdown>();

        // 1. Загружаем громкость. Если записи нет — ставим 0.5f (50%)
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 0.5f);

        if (volumeSlider != null)
        {
            // Настраиваем границы ползунка на всякий случай
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;

            // Ставим ползунок в нужное положение
            volumeSlider.value = savedVolume;

            // Применяем громкость в саму систему звука сразу при старте
            AudioListener.volume = savedVolume;

            // Подписываемся на изменения
            volumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        if (screenModeDropdown != null)
        {
            screenModeDropdown.value = Screen.fullScreen ? 0 : 1;
            screenModeDropdown.onValueChanged.AddListener(SetFullscreen);
        }

        if (backButton != null)
            backButton.onClick.AddListener(CloseSettings);
    }

    public void SetMasterVolume(float volume)
    {
        // Применяем громкость к движку
        AudioListener.volume = volume;
        // Сохраняем значение
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetFullscreen(int isFullscreen)
    {
        bool fullscreen = (isFullscreen == 0);
        Screen.fullScreen = fullscreen;
    }

    public void CloseSettings()
    {
        // Принудительно сохраняем все PlayerPrefs на диск
        PlayerPrefs.Save();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        // При открытии освежаем значение ползунка
        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
        }
    }
}