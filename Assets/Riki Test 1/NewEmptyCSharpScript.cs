using UnityEngine;
using System.Collections;

public class MannequinBreak : MonoBehaviour
{
    [Header("Main mannequin")]
    [SerializeField] private SpriteRenderer mainSprite;
    [SerializeField] private Collider2D mainCollider;

    [Header("Mannequin parts")]
    [SerializeField] private Rigidbody2D[] parts;

    [Header("Explosion settings")]
    [SerializeField] private float explosionForce = 20f;
    [SerializeField] private float torqueForce = 15f;
    [SerializeField] private float spawnOffset = 0.05f;

    [Header("Cleanup")]
    [SerializeField] private float disappearDelay = 5f;
    [SerializeField] private BoxCollider2D groundCollider;

    private bool isBroken = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && !isBroken)
        {
            BreakMannequin();
        }
    }

    private void BreakMannequin()
    {
        isBroken = true;

        // Скрываем цельный манекен
        if (mainSprite != null)
            mainSprite.enabled = false;

        if (mainCollider != null)
            mainCollider.enabled = false;

        foreach (Rigidbody2D part in parts)
        {
            if (part == null)
                continue;

            Transform t = part.transform;

            Vector3 worldPosition = t.position;
            Quaternion worldRotation = t.rotation;

            part.gameObject.SetActive(true);
            t.SetParent(null, true);

            t.position = worldPosition;
            t.rotation = worldRotation;

            part.bodyType = RigidbodyType2D.Dynamic;
            part.simulated = true;
            part.constraints = RigidbodyConstraints2D.None;

            part.linearVelocity = Vector2.zero;
            part.angularVelocity = 0f;
            part.Sleep();
            part.WakeUp();

            t.position += (Vector3)(Random.insideUnitCircle * spawnOffset);

            Vector2 direction = Random.insideUnitCircle.normalized;
            part.AddForce(direction * explosionForce, ForceMode2D.Impulse);
            part.AddTorque(Random.Range(-torqueForce, torqueForce), ForceMode2D.Impulse);
        }

        // Запуск таймера очистки
        StartCoroutine(CleanupRoutine());
    }

    private IEnumerator CleanupRoutine()
    {
        yield return new WaitForSeconds(disappearDelay);

        // Убираем части тела
        foreach (Rigidbody2D part in parts)
        {
            if (part != null)
            {
                part.gameObject.SetActive(false);
            }
        }

        // Отключаем BoxCollider пола
        if (groundCollider != null)
        {
            groundCollider.enabled = false;
        }
    }
}
