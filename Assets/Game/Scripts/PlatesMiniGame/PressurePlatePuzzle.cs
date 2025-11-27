using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlatePuzzle : MonoBehaviour
{
    [Header("Puzzle Settings")]
    public List<PressurePlate> puzzlePlates = new List<PressurePlate>();
    public float sequenceDelay = 1f;
    public Transform playerStartPosition;

    [Header("Trigger Zone")]
    public GameObject triggerZone;

    [Header("False Plates Settings")]
    public GameObject falsePlatesParent;
    public List<FalsePlate> falsePlates = new List<FalsePlate>();

    private bool isSequencePlaying = false;
    private bool isPuzzleActive = false;
    private int currentStep = 0;
    private PlayerMovement playerMovement;
    private Transform playerTransform;

    void Start()
    {
        playerMovement = FindObjectOfType<PlayerMovement>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        foreach (PressurePlate plate in puzzlePlates)
        {
            plate.Initialize(this);
        }

        if (falsePlatesParent != null && falsePlates.Count == 0)
        {
            FindFalsePlatesAutomatically();
        }

        foreach (FalsePlate falsePlate in falsePlates)
        {
            falsePlate.Initialize(playerStartPosition);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && IsPlayerInTriggerZone() && !isSequencePlaying && !isPuzzleActive)
        {
            StartPuzzleSequence();
        }
    }

    private bool IsPlayerInTriggerZone()
    {
        if (triggerZone == null || playerTransform == null) return false;

        Collider2D triggerCollider = triggerZone.GetComponent<Collider2D>();
        if (triggerCollider != null)
        {
            return triggerCollider.OverlapPoint(playerTransform.position);
        }
        return false;
    }

    private void FindFalsePlatesAutomatically()
    {
        FalsePlate[] foundPlates = falsePlatesParent.GetComponentsInChildren<FalsePlate>();
        falsePlates.AddRange(foundPlates);
        Debug.Log($"Found {falsePlates.Count} false plates automatically");
    }

    private void StartPuzzleSequence()
    {
        isSequencePlaying = true;
        isPuzzleActive = true;
        currentStep = 0;

        float totalBlockTime = sequenceDelay * puzzlePlates.Count;
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
            StartCoroutine(EnablePlayerMovementAfterDelay(totalBlockTime));
        }

        StartCoroutine(PlayPlateSequence());
    }

    private IEnumerator EnablePlayerMovementAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
        isSequencePlaying = false;
        Debug.Log("Player movement enabled - you can now step on plates");
    }

    private IEnumerator PlayPlateSequence()
    {
        Debug.Log("Starting puzzle sequence...");

        for (int i = 0; i < puzzlePlates.Count; i++)
        {
            float delayForThisPlate = sequenceDelay * i;
            StartCoroutine(PlayPlateAnimationWithDelay(i, delayForThisPlate));
        }

        yield return null;
    }

    private IEnumerator PlayPlateAnimationWithDelay(int plateIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        puzzlePlates[plateIndex].PlayActivationAnimation();
    }

    public void OnPlateStepped(int plateIndex, bool isCorrectStep)
    {
        if (!isPuzzleActive)
        {
            // Наступили на плиту ДО нажатия E - возвращаем в начало
            ReturnPlayerToStart();
            PlayAllWrongStepAnimations();
            Debug.Log("Stepped on plate before starting puzzle. Returning to start.");
            return;
        }

        if (isSequencePlaying)
        {
            Debug.Log("Sequence is still playing, ignoring step");
            return;
        }

        if (isCorrectStep)
        {
            if (plateIndex == currentStep)
            {
                puzzlePlates[plateIndex].PlayRightStepAnimation();
                currentStep++;
                Debug.Log($"Correct step! Current progress: {currentStep}/{puzzlePlates.Count}");

                if (currentStep >= puzzlePlates.Count)
                {
                    PuzzleCompleted();
                }
            }
            else
            {
                // Неправильный порядок - возвращаем в начало
                ReturnPlayerToStart();
                PlayAllWrongStepAnimations();
                ResetPuzzle();
                Debug.Log($"Wrong order! Expected {currentStep}, got {plateIndex}. Returning to start.");
            }
        }
        else
        {
            // Наступили на ложную плиту - возвращаем в начало
            ReturnPlayerToStart();
            PlayAllWrongStepAnimations();
            ResetPuzzle();
            Debug.Log("Stepped on false plate. Returning to start.");
        }
    }

    private void ReturnPlayerToStart()
    {
        if (playerTransform != null && playerStartPosition != null)
        {
            playerTransform.position = playerStartPosition.position;
            Debug.Log("Player returned to start position");
        }
    }

    private void PlayAllWrongStepAnimations()
    {
        foreach (PressurePlate plate in puzzlePlates)
        {
            plate.PlayWrongStepAnimation();
        }
    }

    private void PuzzleCompleted()
    {
        Debug.Log("Puzzle Completed! Well done!");
        isPuzzleActive = false;
    }

    public void ResetPuzzle()
    {
        StopAllCoroutines();
        isSequencePlaying = false;
        isPuzzleActive = false;
        currentStep = 0;

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        Debug.Log("Puzzle Reset - start over");
    }

    public bool IsPuzzleActive()
    {
        return isPuzzleActive;
    }

    public bool IsSequencePlaying()
    {
        return isSequencePlaying;
    }
}