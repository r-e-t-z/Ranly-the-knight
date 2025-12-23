using UnityEngine;

public class QTEButton : MonoBehaviour
{
    public static GameObject Instance;

    void Awake()
    {
        // Как только префаб создается, кнопка записывает себя в глобальную переменную
        Instance = this.gameObject;

        // Сразу выключаем, чтобы не мешала, но ссылка в Instance останется!
        gameObject.SetActive(false);
    }
}