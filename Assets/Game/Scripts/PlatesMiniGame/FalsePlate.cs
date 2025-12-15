using UnityEngine;

public class FalsePlate : MonoBehaviour
{
    // Метод инициализации теперь пустой, но оставим его для совместимости,
    // если ты вызываешь его где-то еще.
    public void Initialize(Transform unused = null)
    {
        // Нам больше не нужно хранить позицию здесь, менеджер всё знает
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Просто сообщаем боссу, что случилась беда
            if (PressurePlatePuzzle.Instance != null)
            {
                PressurePlatePuzzle.Instance.OnFalsePlateStepped();
            }
        }
    }
}