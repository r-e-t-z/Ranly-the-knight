using UnityEngine;
using TMPro;

public class UIInteractPrompt : MonoBehaviour
{
    public static UIInteractPrompt Instance;

    public TMP_Text promptText;

    void Awake()
    {
        Instance = this;
        promptText.gameObject.SetActive(false);
    }

    public void Show(string text)
    {
        promptText.text = text;
        promptText.gameObject.SetActive(true);
    }

    public void Hide()
    {
        promptText.gameObject.SetActive(false);
    }
}
