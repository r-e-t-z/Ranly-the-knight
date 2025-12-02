using System.Collections;
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

    public static PressurePlate Instance;

    void Awake()
    {
        Instance = this;
    }

    public void Initialize(PressurePlatePuzzle controller)
    {
        puzzleController = controller;
        animator = GetComponent<Animator>();

        if (animator != null)
        {
            animator.enabled = false;
        }
    }

    public void PlayActivationAnimation()
    {
        if (animator != null && activationAnimation != null)
        {
            animator.enabled = true;
            animator.Play(activationAnimation.name);
        }
    }

    public void PlayRightStepAnimation()
    {
        if (animator != null && rightStepAnimation != null)
        {
            animator.enabled = true;
            animator.Play(rightStepAnimation.name);
        }
    }

    public void PlayWrongStepAnimation()
    {
        if (animator != null && wrongStepAnimation != null)
        {
            animator.enabled = true;
            animator.Play(wrongStepAnimation.name);
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