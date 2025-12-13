using UnityEngine;

[ExecuteInEditMode]
public class ZSorter : MonoBehaviour
{
    [Header("Настройки Глубины")]
    // Отрицательное число = ближе к камере
    public float zOffset = 0f;

    [Header("Режим Редактора")]
    // Если галочка снята - в редакторе Z всегда будет 0 (удобно строить уровень)
    // Если галочка стоит - Z будет меняться (удобно проверять тени)
    public bool activeInEditor = false;

    void LateUpdate()
    {
        // Логика:
        // Если игра запущена (Application.isPlaying) -> ВСЕГДА работаем.
        // Если мы в редакторе -> работаем ТОЛЬКО если стоит галочка activeInEditor.

        bool shouldUpdateZ = Application.isPlaying || activeInEditor;

        Vector3 pos = transform.position;

        if (shouldUpdateZ)
        {
            // Меняем Z в зависимости от Y (Инвертированная логика для твоих шейдеров)
            // Умножаем на -1, чтобы при движении ВВЕРХ (Y+) Z уменьшался
            // (или убери -1f, если используешь прямую логику)
            pos.z = (pos.y * 1f) + zOffset;
        }
        else
        {
            // Если мы в редакторе и галочка снята -> Сбрасываем Z в ноль
            // (Чтобы удобно было двигать объекты)
            pos.z = 0f;
        }

        transform.position = pos;
    }
}