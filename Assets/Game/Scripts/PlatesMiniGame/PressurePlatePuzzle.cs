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

    [Header("Camera Settings")]
    public Transform puzzleCameraPosition;
    public float cameraMoveSpeed = 5f;

    private Camera mainCamera;
    private MonoBehaviour cameraFollowScript;
    private Vector3 cameraStartPosition;
    private bool isCameraMovingToPuzzle = false;
    private bool isCameraMovingBack = false;

    private bool isSequencePlaying = false;
    private bool isPuzzleActive = false;
    private int currentStep = 0;
    private PlayerMovement playerMovement;
    private Transform playerTransform;
    private Coroutine cameraReturnCoroutine;

    public static PressurePlatePuzzle Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        playerMovement = FindObjectOfType<PlayerMovement>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cameraFollowScript = mainCamera.GetComponent<MonoBehaviour>();
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

        if (isCameraMovingToPuzzle && puzzleCameraPosition != null)
        {
            MoveCameraToPuzzle();
        }

        if (isCameraMovingBack && playerTransform != null)
        {
            MoveCameraBackToPlayer();
        }
    }

    void MoveCameraToPuzzle()
    {
        Vector3 targetPos = puzzleCameraPosition.position;
        mainCamera.transform.position = Vector3.MoveTowards(
            mainCamera.transform.position,
            targetPos,
            cameraMoveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(mainCamera.transform.position, targetPos) < 0.01f)
        {
            mainCamera.transform.position = targetPos;
            isCameraMovingToPuzzle = false;

            StartCoroutine(PlayPlateSequence());
        }
    }

    void MoveCameraBackToPlayer()
    {
        if (playerTransform == null) return;

        Vector3 targetPos = new Vector3(
            playerTransform.position.x,
            playerTransform.position.y,
            mainCamera.transform.position.z
        );

        mainCamera.transform.position = Vector3.MoveTowards(
            mainCamera.transform.position,
            targetPos,
            cameraMoveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(mainCamera.transform.position, targetPos) < 0.01f)
        {
            isCameraMovingBack = false;

            if (cameraFollowScript != null)
            {
                cameraFollowScript.enabled = true;
            }
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
    }

    private void StartPuzzleSequence()
    {
        isSequencePlaying = true;
        isPuzzleActive = true;
        currentStep = 0;

        cameraStartPosition = mainCamera.transform.position;

        if (cameraFollowScript != null)
        {
            cameraFollowScript.enabled = false;
        }

        isCameraMovingToPuzzle = true;
        isCameraMovingBack = false;

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
    }

    private IEnumerator PlayPlateSequence()
    {
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < puzzlePlates.Count; i++)
        {
            float delayForThisPlate = sequenceDelay * i;
            StartCoroutine(PlayPlateAnimationWithDelay(i, delayForThisPlate));
        }

        float totalSequenceTime = sequenceDelay * puzzlePlates.Count;
        yield return new WaitForSeconds(totalSequenceTime + 0.5f);

        StartCameraReturn();

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
        isSequencePlaying = false;
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
            ReturnPlayerToStart();
            PlayAllWrongStepAnimations();
            ResetPuzzle();
            return;
        }

        if (isSequencePlaying)
        {
            return;
        }

        if (isCorrectStep)
        {
            if (plateIndex == currentStep)
            {
                puzzlePlates[plateIndex].PlayRightStepAnimation();
                currentStep++;

                if (currentStep >= puzzlePlates.Count)
                {
                    PuzzleCompleted();
                }
            }
            else
            {
                ReturnPlayerToStart();
                PlayAllWrongStepAnimations();
                ResetPuzzle();
            }
        }
        else
        {
            ReturnPlayerToStart();
            PlayAllWrongStepAnimations();
            ResetPuzzle();
        }
    }

    private void ReturnPlayerToStart()
    {
        if (playerTransform != null && playerStartPosition != null)
        {
            playerTransform.position = playerStartPosition.position;
        }
    }

    private void PuzzleCompleted()
    {
        isPuzzleActive = false;
        Invoke("StartCameraReturn", 1f);
    }

    private void StartCameraReturn()
    {
        isCameraMovingBack = true;
        isCameraMovingToPuzzle = false;
    }

    private void PlayAllWrongStepAnimations()
    {
        foreach (PressurePlate plate in puzzlePlates)
        {
            plate.PlayWrongStepAnimation();
        }
    }

    public void ResetPuzzle()
    {
        PlayAllWrongStepAnimations();
        StopAllCoroutines();

        StartCameraReturn();

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        isSequencePlaying = false;
        isPuzzleActive = false;
        currentStep = 0;
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