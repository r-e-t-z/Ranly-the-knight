using UnityEngine;

public class Teleport : MonoBehaviour
{
	public Transform targetPosition;

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			other.transform.position = targetPosition.position;
		}
	}
}
