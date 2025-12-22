using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Настройки")]
    public float runSpeed = 10f;
    public float moveSpeed = 5f;

    // Ссылка на компонент Аниматор
    public Animator animator;

    // Сюда больше не нужно перетаскивать отдельные спрайты!
    // public Sprite frontsprite; <-- Удали или забудь про эти переменные
    // public Sprite backsprite;
    // ...

    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // --- ИСПРАВЛЕНИЕ ---
        // Если забыл привязать в инспекторе, ищем ВНУТРИ дочерних объектов (Visuals)
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        // Если всё равно не нашли - ругаемся в консоль
        if (animator == null)
        {
            Debug.LogError("❌ ОШИБКА: Скрипт PlayerMovement не нашел Animator! Убедись, что на объекте Visuals есть компонент Animator.");
        }
    }

    void Update()
    {
        // 1. Ввод
        movement.x = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
        movement.y = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
        movement = movement.normalized;

        // 2. Определяем, бежим мы или идем
        // Если нажат Shift - используем runSpeed, иначе moveSpeed
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        // 3. Вычисляем множитель анимации
        // Если стоим -> 0
        // Если идем -> 1
        // Если бежим -> (runSpeed / moveSpeed), например 8/5 = 1.6
        float animationSpeedMultiplier = 0f;

        if (movement.sqrMagnitude > 0)
        {
            if (isRunning)
            {
                animationSpeedMultiplier = runSpeed / moveSpeed; // Например 1.6
            }
            else
            {
                animationSpeedMultiplier = 1f; // Обычная скорость
            }
        }

        // 4. Передаем в Аниматор
        if (animator != null)
        {
            // Теперь Speed управляет и переходом (0 -> 1), и скоростью шагов (1 -> 1.6)
            animator.SetFloat("Speed", animationSpeedMultiplier);

            if (movement.x != 0 || movement.y != 0)
            {
                animator.SetFloat("Horizontal", movement.x);
                animator.SetFloat("Vertical", movement.y);
            }
        }
    }

    void FixedUpdate()
    {
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : moveSpeed;
        rb.MovePosition(rb.position + movement * currentSpeed * Time.fixedDeltaTime);
    }
}