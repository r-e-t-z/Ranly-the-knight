using UnityEngine;

public class FalsePlate : MonoBehaviour
{
    [Header("False Plate Settings")]
    public Transform resetPosition;
    public bool usePuzzleStartPosition = true;

    private Transform puzzleStartPosition;

    public void Initialize(Transform puzzleStartPos)
    {
        puzzleStartPosition = puzzleStartPos;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player stepped on false plate!");
            ResetPlayer(other.gameObject);
        }
    }

    private void ResetPlayer(GameObject player)
    {
        Transform targetPosition = usePuzzleStartPosition ? puzzleStartPosition : resetPosition;

        if (targetPosition != null)
        {
            player.transform.position = targetPosition.position;
            Debug.Log($"Player reset to position: {targetPosition.name}");
        }

        PressurePlatePuzzle puzzle = FindObjectOfType<PressurePlatePuzzle>();
        if (puzzle != null)
        {
            puzzle.ResetPuzzle();
        }
    }
}