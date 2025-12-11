using UnityEngine;

[ExecuteInEditMode]
public class ZSorter : MonoBehaviour
{
    // Ќастройка: как сильно мен€ть Z.
    // ≈сли поставить отрицательное число (например -0.01), 
    // то чем выше ты по Y, тем меньше будет Z (то, что ты просил).
    public float sortFactor = -0.01f;

    // –учна€ поправка (если нужно чуть подвинуть слой)
    public float offsetZ = 0f;

    void LateUpdate()
    {
        Vector3 pos = transform.position;
        // ћен€ем только Z
        pos.z = (pos.y * sortFactor) + offsetZ;
        transform.position = pos;
    }
}