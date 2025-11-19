using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class Puzzle3x3 : MonoBehaviour
{
    [Header("UI — слоты (Image)")]
    public Image[] slots = new Image[9];

    [Header("Перетаскиваемые цифры")]
    public TMP_Text[] draggableNumbers;

    [Header("Индикатор")]
    public Image indicator;

    [Header("Параметры головоломки")]
    public int targetSum = 10;

    [Header("Дерево")]
    public Transform tree;
    public Transform treeTargetPosition;

    private TMP_Text draggedNumber;
    private bool puzzleSolved = false;

    private GraphicRaycaster raycaster;
    private EventSystem eventSystem;

    void Awake()
    {
        raycaster = FindObjectOfType<GraphicRaycaster>();
        eventSystem = EventSystem.current;
    }

    void Update()
    {
        if (draggedNumber != null)
        {
            draggedNumber.transform.position = Input.mousePosition;
        }
    }

    public void BeginDrag(BaseEventData data)
    {
        PointerEventData pointer = (PointerEventData)data;

        if (pointer.pointerDrag != null)
        {
            draggedNumber = pointer.pointerDrag.GetComponent<TMP_Text>();
        }
    }

    public void EndDrag(BaseEventData data)
    {
        if (draggedNumber == null) return;

        Image slot = GetSlotUnderCursor();   // <- вот тут магия

        if (slot != null)
        {
            TMP_Text slotText = slot.GetComponentInChildren<TMP_Text>();
            slotText.text = draggedNumber.text;
        }

        draggedNumber = null;

        CheckPuzzle();
    }

    // === НОВАЯ ФУНКЦИЯ ПОИСКА UI-ОБЪЕКТА ПОД КУРСОРОМ ===
    Image GetSlotUnderCursor()
    {
        PointerEventData pointer = new PointerEventData(eventSystem);
        pointer.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointer, results);

        foreach (RaycastResult r in results)
        {
            Image img = r.gameObject.GetComponent<Image>();
            if (img != null)
            {
                // проверяем что это один из слотов
                foreach (Image s in slots)
                {
                    if (img == s)
                        return img;
                }
            }
        }

        return null;
    }

    void CheckPuzzle()
    {
        // строки
        for (int row = 0; row < 3; row++)
        {
            int sum = 0;
            for (int col = 0; col < 3; col++)
            {
                int index = row * 3 + col;
                TMP_Text txt = slots[index].GetComponentInChildren<TMP_Text>();
                if (!int.TryParse(txt.text, out int num)) { indicator.color = Color.red; return; }
                sum += num;
            }
            if (sum != targetSum) { indicator.color = Color.red; return; }
        }

        // столбцы
        for (int col = 0; col < 3; col++)
        {
            int sum = 0;
            for (int row = 0; row < 3; row++)
            {
                int index = row * 3 + col;
                TMP_Text txt = slots[index].GetComponentInChildren<TMP_Text>();
                if (!int.TryParse(txt.text, out int num)) { indicator.color = Color.red; return; }
                sum += num;
            }
            if (sum != targetSum) { indicator.color = Color.red; return; }
        }

        indicator.color = Color.green;

        if (!puzzleSolved)
        {
            puzzleSolved = true;
            tree.position = treeTargetPosition.position;
        }
    }
}
