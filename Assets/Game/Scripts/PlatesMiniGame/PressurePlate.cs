using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [Header("Plate Settings")]
    public int plateIndex;
    public bool isCorrectPlate = true;

    [Header("Animations")]
    public AnimationClip activationAnimation;
    public AnimationClip rightStepAnimation;
    public AnimationClip wrongStepAnimation;

    private PressurePlatePuzzle puzzleController;
    private Animator animator;

    public void Initialize(PressurePlatePuzzle controller)
    {
        puzzleController = controller;
        animator = GetComponent<Animator>();
        if (animator != null) animator.enabled = false;
    }

    // --- ИСПРАВЛЕНИЕ: ПРИНУДИТЕЛЬНЫЙ ПЕРЕЗАПУСК АНИМАЦИИ ---

    public void PlayActivationAnimation()
    {
        if (animator != null && activationAnimation != null)
        {
            animator.enabled = true;
            // Play(Имя, Слой, Время). 0f означает "начать с 0 секунды"
            animator.Play(activationAnimation.name, -1, 0f);
        }
    }

    public void PlayRightStepAnimation()
    {
        if (animator != null && rightStepAnimation != null)
        {
            animator.enabled = true;
            animator.Play(rightStepAnimation.name, -1, 0f);
        }
    }

    public void PlayWrongStepAnimation()
    {
        if (animator != null && wrongStepAnimation != null)
        {
            animator.enabled = true;
            animator.Play(wrongStepAnimation.name, -1, 0f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (puzzleController != null)
            {
                puzzleController.OnPlateStepped(plateIndex, isCorrectPlate);
            }
        }
    }
}