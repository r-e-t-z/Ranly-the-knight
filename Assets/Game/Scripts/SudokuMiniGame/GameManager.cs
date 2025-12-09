using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Settings")]
    public int gridSize = 3;
    public int targetSum = 15;

    [Header("Initial Values")]
    public string initialGrid = "000000000";

    [Header("Prefabs")]
    public GameObject cellPrefab;
    public GameObject numberPrefab;

    [Header("UI")]
    public Transform gridParent;
    public Transform numbersPanel;
    public Text targetText;
    public Image statusIndicator;
    public Button resetButton;
    public GameObject gamePanel;

    public AnimationClip treeAnimation;
    public GameObject treeObject;

    private Animator treeAnimator;

    private TableCell[,] gridCells;
    private MonoBehaviour playerController;

    public static GameManager Instance;

    void Awake()
    {
        Instance = this;
        
        gamePanel.SetActive(false);
        playerController = FindObjectOfType<PlayerMovement>();

        if (treeObject != null)
        {
            treeAnimator = treeObject.GetComponent<Animator>();

            if (treeAnimator != null)
            {
                treeAnimator.enabled = false;
            }
        }


    }
    public void StartMiniGame()
    {
        if(playerController != null)
        {
            playerController.enabled = false;
        }

        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetGrid);
        }

        gamePanel.SetActive(true);
        CreateGrid();
        CreateNumbers();
        SetupInitialValues();
        targetText.text = $"object.targetposition = {targetSum}";
    }

    void CreateGrid()
    {
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        GridLayoutGroup gridLayout = gridParent.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
            gridLayout = gridParent.gameObject.AddComponent<GridLayoutGroup>();

        gridLayout.cellSize = new Vector2(80, 80);
        gridLayout.spacing = new Vector2(5, 5);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = gridSize;

        gridCells = new TableCell[gridSize, gridSize];

        for (int i = 0; i < gridSize * gridSize; i++)
        {
            GameObject cell = Instantiate(cellPrefab, gridParent);
            TableCell tableCell = cell.GetComponent<TableCell>();

            int row = i / gridSize;
            int col = i % gridSize;
            gridCells[row, col] = tableCell;
        }
    }

    void CreateNumbers()
    {
        foreach (Transform child in numbersPanel)
            Destroy(child.gameObject);

        for (int i = 1; i <= 9; i++)
        {
            GameObject number = Instantiate(numberPrefab, numbersPanel);
            number.GetComponentInChildren<Text>().text = i.ToString();
        }
    }

    void SetupInitialValues()
    {
        for (int i = 0; i < gridSize * gridSize && i < initialGrid.Length; i++)
        {
            int row = i / gridSize;
            int col = i % gridSize;
            int value = int.Parse(initialGrid[i].ToString());

            if (gridCells[row, col] != null)
            {
                if (value > 0)
                {
                    gridCells[row, col].GetComponentInChildren<Text>().text = value.ToString();
                    gridCells[row, col].currentValue = value;
                    gridCells[row, col].isLocked = true;
                }
                else
                {
                    gridCells[row, col].GetComponentInChildren<Text>().text = "";
                    gridCells[row, col].currentValue = 0;
                    gridCells[row, col].isLocked = false;
                }
            }
        }
        statusIndicator.color = Color.red;
    }

    public void ResetGrid()
    {
        SetupInitialValues();
    }

    public void CheckSolution()
    {
        bool isSolved = true;

        for (int row = 0; row < gridSize; row++)
        {
            int rowSum = 0;
            for (int col = 0; col < gridSize; col++)
            {
                if (gridCells[row, col] != null)
                {
                    rowSum += gridCells[row, col].currentValue;
                }
            }
            if (rowSum != targetSum)
            {
                isSolved = false;
                break;
            }
        }

        if (isSolved)
        {
            for (int col = 0; col < gridSize; col++)
            {
                int colSum = 0;
                for (int row = 0; row < gridSize; row++)
                {
                    if (gridCells[row, col] != null)
                    {
                        colSum += gridCells[row, col].currentValue;
                    }
                }
                if (colSum != targetSum)
                {
                    isSolved = false;
                    break;
                }
            }
        }

        if (isSolved)
        {
            statusIndicator.color = Color.green;
            Invoke("EndMiniGame", 1.5f);
        }
        else
        {
            statusIndicator.color = Color.red;
        }
    }

    void EndMiniGame()
    {
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        gamePanel.SetActive(false);

        if (treeAnimator != null && treeAnimation != null)
        {
            treeAnimator.enabled = true;
            treeAnimator.Play(treeAnimation.name);
        }
    }

}