using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableNumber : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	public int numberValue;
	private CanvasGroup canvasGroup;
	private Vector3 startPosition;

	void Start()
	{
		canvasGroup = GetComponent<CanvasGroup>();
		if (canvasGroup == null)
			canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
		numberValue = int.Parse(GetComponentInChildren<Text>().text);
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		canvasGroup.alpha = 0.6f;
		canvasGroup.blocksRaycasts = false;
		startPosition = transform.position;
	}

	public void OnDrag(PointerEventData eventData)
	{
		transform.position = eventData.position;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		canvasGroup.alpha = 1f;
		canvasGroup.blocksRaycasts = true;
		transform.position = startPosition;
	}
}