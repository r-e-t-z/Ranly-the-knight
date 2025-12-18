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
                foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
                {
                    if (clip.name == animationName || clip.name.EndsWith(animationName))
                    {
                        return clip.length;
                    }
                }
            }
        }
        return 0f;
    }

    public void PlayAnimation(string animationName)
    {
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

    public void PlaySequenceOnObject(string objectName, string[] animationNames)
    {
        StartCoroutine(PlaySequenceRoutine(objectName, animationNames));
    }

    private IEnumerator PlaySequenceRoutine(string objectName, string[] anims)
    {
        foreach (string anim in anims)
        {
            PlayAnimation(objectName, anim);
            float duration = GetAnimationLength(objectName, anim);
            if (duration <= 0) duration = 0.5f;
            yield return new WaitForSeconds(duration);
        }
    }

    public float GetAnimationLength(string animationName)
    {
        return GetAnimationLength(animationName, animationName);
    }
}