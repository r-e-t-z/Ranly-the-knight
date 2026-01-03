using UnityEngine;

public class SudokuMiniGameTrigger : MonoBehaviour
{
    [Header("Режим  работы")]
    public bool workOnlyOnce = false;
    public bool startOnEnter = true;
    public bool requirePressE = false;

    bool inRange = false;
    bool alreadyUsed = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (alreadyUsed && workOnlyOnce) return;
        if (other.CompareTag("Player"))
        {
            inRange = true;
            if (startOnEnter && !requirePressE) StartGame();
            else if (requirePressE) UIInteractPrompt.Instance.Show("Нажмите E", this.transform);

        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = false;
            UIInteractPrompt.Instance.Hide();
        }
    }

    void Update()
    {
        if(inRange && requirePressE && Input.GetKeyDown(KeyCode.E))
        {
            UIInteractPrompt.Instance.Hide();
            StartGame();
        }
    }

    void StartGame()
    {
        if (workOnlyOnce)
        {
            alreadyUsed = true;
            inRange = false;
        }

        GameManager.Instance.StartMiniGame();
    }
}
