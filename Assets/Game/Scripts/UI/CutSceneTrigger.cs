using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CutSceneTrigger : MonoBehaviour
{
    public Canvas canvas;
    public VideoPlayer videoPlayer;

    private MonoBehaviour playerController;
    private bool used = false;

    private void Start()
    {
        playerController = FindObjectOfType<PlayerMovement>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used) return;
        if (!other.CompareTag("Player")) return;

        used = true;
        canvas.gameObject.SetActive(true);

        if (playerController != null)
            playerController.enabled = false;

        videoPlayer.Play();
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        canvas.enabled = false;

        if (playerController != null)
            playerController.enabled = true;
    }
}

