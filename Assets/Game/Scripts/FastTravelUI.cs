using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FastTravelUI : MonoBehaviour
{
    public static FastTravelUI Instance;

    [Header("UI references")]
    public GameObject menuPanel;            // корневая панель (SetActive true/false)
    public GameObject buttonPrefab;         // префаб кнопки (UI Button)
    public Transform buttonContainer;       // Content в ScrollView или пустой объект с VerticalLayout
    public Transform playerTransform;       // перетащи сюда объект игрока (Transform)

    private List<Terminal> allTerminals = new List<Terminal>();
    private Terminal currentTerminal;

    private void Awake()
    {
        // Простой singleton
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        // Собираем все терминалы из сцены
        allTerminals.Clear();
        allTerminals.AddRange(FindObjectsOfType<Terminal>());

        // Скрываем меню в начале
        if (menuPanel != null) menuPanel.SetActive(false);
    }

    public void OpenMenu(Terminal current)
    {
        currentTerminal = current;
        if (menuPanel == null || buttonPrefab == null || buttonContainer == null || playerTransform == null)
        {
            Debug.LogError("FastTravelUI: не все ссылки заданы в инспекторе.");
            return;
        }

        // Включаем панель
        menuPanel.SetActive(true);

        // Удаляем старые кнопки
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        // Создаём кнопку для каждого терминала (кроме текущего)
        foreach (var t in allTerminals)
        {
            if (t == currentTerminal) continue;

            GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
            // На случай, если в префабе нет Text — пробуем взять Text компонент внутри
            Text txt = btnObj.GetComponentInChildren<Text>();
            if (txt != null) txt.text = t.terminalName;
            else Debug.LogWarning("FastTravelUI: в buttonPrefab нет Text компонента в дочерних.");

            Button btn = btnObj.GetComponent<Button>();
            if (btn == null)
            {
                Debug.LogError("FastTravelUI: buttonPrefab должен содержать компонент Button.");
                continue;
            }

            Terminal localT = t; // захват локальной переменной для замыкания
            btn.onClick.AddListener(() => {
                TeleportTo(localT);
            });
        }
    }

    public void TeleportTo(Terminal target)
    {
        if (target == null || playerTransform == null)
            return;

        // Телепортируем игрока в позицию терминала
        // Если у тебя Rigidbody2D — можно использовать его вместо transform.position
        playerTransform.position = target.transform.position;

        CloseMenu();
    }

    public void CloseMenu()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
    }

    // Вызов в случае, если во время игры появились/удалились терминалы
    public void RefreshTerminals()
    {
        allTerminals.Clear();
        allTerminals.AddRange(FindObjectsOfType<Terminal>());
    }
}
