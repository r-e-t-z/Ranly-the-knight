using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TableCell : MonoBehaviour, IDropHandler
{
    private Text numberText;
    public int currentValue;
    public bool isLocked = false; 

    void Start()
    {
        numberText = GetComponentInChildren<Text>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (isLocked) return;

        GameObject draggedNumber = eventData.pointerDrag;
        if (draggedNumber == null) return;

        DraggableNumber draggable = draggedNumber.GetComponent<DraggableNumber>();
        if (draggable != null)
        {
            numberText.text = draggable.numberValue.ToString();
            currentValue = draggable.numberValue;
            FindObjectOfType<GameManager>().CheckSolution();
        }
    }
}