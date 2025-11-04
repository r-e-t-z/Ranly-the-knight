using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("Главное меню")]
    public GameObject mainMenuPanel;

    [Header("Меню настроек")]
    public GameObject settingsPanel;


    void Start()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
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
}