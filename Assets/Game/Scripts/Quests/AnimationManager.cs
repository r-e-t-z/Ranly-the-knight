using UnityEngine;
using System.Collections;

public class AnimationManager : MonoBehaviour
{
	public static AnimationManager Instance;

	void Awake()
	{
		Instance = this;
	}

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
				Debug.Log($"🎬 Проиграна анимация: {animationName}");
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
			yield return new WaitForSeconds(0.1f); // Маленькая задержка между анимациями
		}
	}
}