using UnityEngine;

public class AnimationAutoDisable : MonoBehaviour
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void OnAnimationFinished()
    {
        if (animator != null)
        {
            animator.enabled = false; 
        }
    }
}
