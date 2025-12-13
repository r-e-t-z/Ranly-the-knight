using UnityEngine;
using System.Collections;
using System.Linq; // Нужно для поиска клипов

public class AnimationManager : MonoBehaviour
{
    public static AnimationManager Instance;

    void Awake()
    {
        Instance = this;
    }

    // Обычный запуск (без ожидания)
    public void PlayAnimation(string animationName)
    {
        GameObject obj = GameObject.Find(animationName);
        if (obj != null)
        {
            Animator animator = obj.GetComponent<Animator>();
            if (animator != null)
            {
                animator.enabled = true;
                animator.Play(animationName);
            }
        }
    }

    public void PlayMultipleAnimations(string[] animationNames)
    {
        StartCoroutine(PlayAnimationsSequentially(animationNames));
    }

    private IEnumerator PlayAnimationsSequentially(string[] animationNames)
    {
        foreach (string animationName in animationNames)
        {
            PlayAnimation(animationName);
            yield return new WaitForSeconds(0.1f);
        }
    }

    // НОВЫЙ МЕТОД: Возвращает длительность анимации
    public float GetAnimationLength(string animationName)
    {
        GameObject obj = GameObject.Find(animationName);
        if (obj != null)
        {
            Animator animator = obj.GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                // Ищем клип с таким же именем в контроллере
                AnimationClip clip = animator.runtimeAnimatorController.animationClips.FirstOrDefault(c => c.name == animationName);
                if (clip != null)
                {
                    return clip.length;
                }
            }
        }
        return 0f; 
    }
}