using UnityEngine;
using TMPro;

public class UIInteractPrompt : MonoBehaviour
{
    public static UIInteractPrompt Instance;
    public TMP_Text promptText;

    [Header("Настройки смещения")]
    public Vector3 offset = new Vector3(0, 1.5f, 0); // Высота над объектом

    private Transform targetObject;
    private RectTransform rectTransform;
    private Camera mainCam;
    private GameObject player;

    void Awake()
    {
        if (Instance == null) Instance = this;
        
        rectTransform = promptText.GetComponent<RectTransform>();
        mainCam = Camera.main;
        promptText.gameObject.SetActive(false);
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Теперь метод Show принимает еще и Transform объекта
    public void Show(string text, Transform target)
    {
        if (promptText == null) return;

        targetObject = target;
        promptText.text = text;
        promptText.gameObject.SetActive(true);

        if (mainCam == null) mainCam = Camera.main;
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");

        // Сразу обновляем позицию, чтобы не было "прыжка" на один кадр
        UpdatePosition();
    }

    public void Hide()
    {
        targetObject = null;
        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (targetObject != null && promptText.gameObject.activeSelf)
        {
            UpdatePosition();
        }
    }

    private void UpdatePosition()
    {
        
        if (targetObject == null || mainCam == null) return;

        if(player.transform.position.x < targetObject.transform.position.x)
        {
            offset = new Vector3(2, 1.5f, 0);
        }
        else
        {
            offset = new Vector3(-1, 1.5f, 0);
        }

        Vector3 screenPos = mainCam.WorldToScreenPoint(targetObject.position + offset);
        
        rectTransform.position = screenPos;
    }
}