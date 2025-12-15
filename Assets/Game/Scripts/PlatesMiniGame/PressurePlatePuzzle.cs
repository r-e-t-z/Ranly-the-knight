using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlatePuzzle : MonoBehaviour
{
    public static PressurePlatePuzzle Instance;

    [Header("Puzzle Settings")]
    public List<PressurePlate> puzzlePlates = new List<PressurePlate>();
    public float sequenceDelay = 1f;
    public Transform playerStartPosition;

    [Header("Trigger Zone (Камень)")]
    // Сюда перетащи объект камня, чтобы проверять дистанцию
    public Transform triggerZoneObject;
    public float interactionRadius = 3.0f; // Радиус, в котором работает кнопка E

    [Header("False Plates")]
    public GameObject falsePlatesParent;
    public List<FalsePlate> falsePlates = new List<FalsePlate>();

    [Header("Camera Settings")]
    public Transform puzzleCameraPosition;
    // ЧЕМ БОЛЬШЕ ЭТО ЧИСЛО, ТЕМ МЕДЛЕННЕЕ КАМЕРА (0.1 = быстро, 1.0 = очень медленно)
    public float smoothTime = 0.8f;

    private bool isPuzzleSolved = false;
    private bool isShowingHint = false;

    private Camera mainCamera;
    private CameraController cameraController;
    private Transform playerTransform;
    private PlayerMovement playerMovement;

    private Vector3 initialCameraOffset;
    private Vector3 currentVelocity; // Для SmoothDamp

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cameraController = mainCamera.GetComponent<CameraController>();
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                playerMovement = player.GetComponent<PlayerMovement>();
                initialCameraOffset = mainCamera.transform.position - player.transform.position;
            }
        }

        foreach (PressurePlate plate in puzzlePlates) plate.Initialize(this);

        if (falsePlatesParent != null && falsePlates.Count == 0)
        {
            FalsePlate[] foundPlates = falsePlatesParent.GetComponentsInChildren<FalsePlate>();
            falsePlates.AddRange(foundPlates);
        }

        foreach (FalsePlate falsePlate in falsePlates) falsePlate.Initialize();
    }

    void Update()
    {
        // Если пазл решен или мультик уже идет - ничего не делаем
        if (isPuzzleSolved || isShowingHint) return;

        // Если нажали E и мы рядом с камнем
        if (Input.GetKeyDown(KeyCode.E) && IsPlayerNearStone())
        {
            StartCoroutine(PlayHintRoutine());
        }
    }

    private bool IsPlayerNearStone()
    {
        if (triggerZoneObject == null || playerTransform == null) return false;
        return Vector2.Distance(triggerZoneObject.position, playerTransform.position) <= interactionRadius;
    }

    private IEnumerator PlayHintRoutine()
    {
        isShowingHint = true;

        if (playerMovement != null) playerMovement.enabled = false;
        if (cameraController != null) cameraController.isLocked = true;

        Vector3 targetCamPos = puzzleCameraPosition.position;
        targetCamPos.z = mainCamera.transform.position.z;

        // --- ДВИЖЕНИЕ К ПАЗЛУ ---
        // Двигаемся, пока дистанция больше 0.1
        while (Vector3.Distance(mainCamera.transform.position, targetCamPos) > 0.1f)
        {
            mainCamera.transform.position = Vector3.SmoothDamp(
                mainCamera.transform.position,
                targetCamPos,
                ref currentVelocity,
                smoothTime
            );
            yield return null;
        }

        // --- АНИМАЦИЯ ---
        currentVelocity = Vector3.zero; // Сброс скорости
        yield return new WaitForSeconds(0.3f);

        for (int i = 0; i < puzzlePlates.Count; i++)
        {
            puzzlePlates[i].PlayActivationAnimation();
            yield return new WaitForSeconds(sequenceDelay);
        }

        yield return new WaitForSeconds(0.5f);

        // --- ДВИЖЕНИЕ ОБРАТНО ---
        Vector3 returnPos = playerTransform.position + initialCameraOffset;

        while (Vector3.Distance(mainCamera.transform.position, returnPos) > 0.1f)
        {
            // Обновляем цель (если игрок чуть сдвинулся физикой)
            returnPos = playerTransform.position + initialCameraOffset;

            mainCamera.transform.position = Vector3.SmoothDamp(
                mainCamera.transform.position,
                returnPos,
                ref currentVelocity,
                smoothTime
            );
            yield return null;
        }

        // Финальная доводка
        mainCamera.transform.position = returnPos;

        if (cameraController != null) cameraController.isLocked = false;
        if (playerMovement != null) playerMovement.enabled = true;

        isShowingHint = false;
    }

    public void OnPlateStepped(int plateIndex, bool isCorrectPlate)
    {
        if (isPuzzleSolved || isShowingHint) return;

        if (isCorrectPlate)
        {
            puzzlePlates[plateIndex].PlayRightStepAnimation();
            if (plateIndex == puzzlePlates.Count - 1) PuzzleCompleted();
        }
        else ResetPlayer();
    }

    public void OnFalsePlateStepped() { if (!isPuzzleSolved) ResetPlayer(); }

    private void ResetPlayer()
    {
        if (playerTransform != null && playerStartPosition != null)
        {
            Rigidbody2D rb = playerTransform.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
            playerTransform.position = playerStartPosition.position;
        }
        foreach (PressurePlate plate in puzzlePlates) plate.PlayWrongStepAnimation();
    }

    private void PuzzleCompleted()
    {
        isPuzzleSolved = true;
        Debug.Log("✅ ГОЛОВОЛОМКА ПРОЙДЕНА!");

        // Ищем скрипт зоны на объекте триггера
        if (triggerZoneObject != null)
        {
            PuzzleActivationZone zoneScript = triggerZoneObject.GetComponent<PuzzleActivationZone>();

            if (zoneScript != null)
            {
                zoneScript.DisableZone(); // Вызываем наш новый метод
            }
            else
            {
                // Если скрипт не найден, попробуем просто выключить объект
                // (Если камень - это только триггер, а не декорация)
                // triggerZoneObject.gameObject.SetActive(false); 

                Debug.LogWarning("⚠️ Не найден скрипт PuzzleActivationZone на камне!");
            }
        }
    }
}