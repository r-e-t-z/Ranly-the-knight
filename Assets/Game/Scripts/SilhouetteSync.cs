using UnityEngine;

public class SilhouetteSync : MonoBehaviour
{
    [Header("Ссылки")]
    // Спрайт основного игрока
    public SpriteRenderer mainRenderer;
    // Спрайт тени (который на дочернем объекте Visuals или Silhouette)
    public SpriteRenderer shadowRenderer;

    void LateUpdate()
    {
        if (mainRenderer == null || shadowRenderer == null) return;

        // 1. Копируем сам спрайт (картинку)
        // Это автоматически подхватит анимацию, если аниматор меняет спрайт на mainRenderer
        shadowRenderer.sprite = mainRenderer.sprite;

        // 2. Копируем отражение (Flip)
        shadowRenderer.flipX = mainRenderer.flipX;
        shadowRenderer.flipY = mainRenderer.flipY;

        // 3. Копируем цвет (опционально, если игрок может краснеть/мигать)
        // shadowRenderer.color = mainRenderer.color; 

        // 4. Копируем сортировку (если нужно, но у тени обычно свои настройки)
        // shadowRenderer.sortingOrder = mainRenderer.sortingOrder + 1;
    }
}