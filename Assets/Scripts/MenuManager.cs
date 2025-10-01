using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("Главное меню")]
    public GameObject mainMenuPanel;    // Панель с кнопками главного меню

    [Header("Меню настроек")]
    public GameObject settingsPanel;    // Панель с настройками


    void Start()
    {
        // Изначально: главное меню видно, настройки скрыты, блюр выключен
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    // Эту функцию вызови при нажатии кнопки "Настройки"
    public void OnSettingsButton()
    {
        // Скрываем главное меню
        mainMenuPanel.SetActive(false);

        // Показываем настройки
        settingsPanel.SetActive(true);
    }

    // Эту функцию вызови при нажатии кнопки "Назад" в настройках
    public void OnBackButton()
    {
        // Показываем главное меню
        mainMenuPanel.SetActive(true);


        // Скрываем настройки
        settingsPanel.SetActive(false);
    }
}