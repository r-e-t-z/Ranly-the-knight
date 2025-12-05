using UnityEngine;
using UnityEngine.SceneManagement;
public class PanelController : MonoBehaviour
{
    [Header("Панель")]
    public GameObject panel;
    public string sceneName;

    [Header("Настройки триггера")]
    public bool workOnlyOnce = false;
    private string playerTag = "Player";

    [Header("Управление игроком")]
    public bool disablePlayerControl = true;

    public bool goToMenu = false;

    private PlayerMovement playerController;
    private bool alreadyTriggered = false;

    void Start()
    {
        playerController = FindObjectOfType<PlayerMovement>();

        if (panel != null)
            panel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && (!workOnlyOnce || !alreadyTriggered))
        {
            if (panel != null)
                panel.SetActive(true);

            if (disablePlayerControl && playerController != null)
                playerController.enabled = false;

            if (goToMenu)
            {
                Invoke("LoadMenu", 5f);
            }

            alreadyTriggered = true;
        }
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene(sceneName);
    }

    public void HidePanel()
    {
        if (panel != null)
            panel.SetActive(false);

        if (disablePlayerControl && playerController != null)
            playerController.enabled = true;
    }

    public void ResetTrigger()
    {
        alreadyTriggered = false;
    }
}