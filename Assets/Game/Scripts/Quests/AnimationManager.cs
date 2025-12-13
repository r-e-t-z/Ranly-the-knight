using UnityEngine;
using System.Collections;
using System.Linq;

public class AnimationManager : MonoBehaviour
{
    public static AnimationManager Instance;

    void Awake()
    {
        Instance = this;
    }

    // --- НОВЫЕ МЕТОДЫ (С указанием объекта) ---

    public void PlayAnimation(string objectName, string animationName)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj != null)
        {
            Animator animator = obj.GetComponent<Animator>();
            if (animator != null)
            {
                animator.enabled = true;
                animator.Play(animationName);
            }
            else
            {
                Debug.LogWarning($"На объекте {objectName} нет Аниматора!");
            }
        }
        else
        {
            Debug.LogWarning($"Не найден объект с именем: {objectName}");
        }
    }

    public float GetAnimationLength(string objectName, string animationName)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj != null)
        {
            Animator animator = obj.GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                // Ищем клип в контроллере
                AnimationClip clip = animator.runtimeAnimatorController.animationClips
                    .FirstOrDefault(c => c.name == animationName);

                if (clip != null) return clip.length;
            }
        }
        return 0f;
    }

    // --- СТАРЫЕ МЕТОДЫ (Для совместимости, если где-то еще используются) ---
    public void PlayAnimation(string animationName)
    {
        // Пытаемся найти объект с таким же именем, как анимация
        PlayAnimation(animationName, animationName);
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

    public float GetAnimationLength(string animationName)
    {
        return GetAnimationLength(animationName, animationName);
    }
}