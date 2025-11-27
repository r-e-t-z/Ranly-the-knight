using UnityEngine;

public class TriggerManager : MonoBehaviour
{
	public static TriggerManager Instance;

	void Awake()
	{
		Instance = this;
	}

	public void ActivateTrigger(string triggerName)
	{
		GameObject trigger = GameObject.Find(triggerName);
		if (trigger != null)
		{
			Collider2D collider = trigger.GetComponent<Collider2D>();
			if (collider != null) 
			{
				collider.enabled = true;
				Debug.Log($"✅ Активирован триггер: {triggerName}");
			}
			else
			{
				Debug.LogWarning($"❌ У объекта {triggerName} нет коллайдера");
			}
		}
		else
		{
			Debug.LogWarning($"❌ Не найден триггер: {triggerName}");
		}
	}

	public void DeactivateObject(string objectName)
	{
		GameObject obj = GameObject.Find(objectName);
		if (obj != null)
		{
			obj.SetActive(false);
			Debug.Log($"🚫 Деактивирован объект: {objectName}");
		}
		else
		{
			Debug.LogWarning($"❌ Не найден объект: {objectName}");
		}
	}
}