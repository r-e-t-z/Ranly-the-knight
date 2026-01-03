using UnityEngine;

public class MenuCleanup : MonoBehaviour
{
    void Awake()
    {
        // 1. Убиваем игрока и всё, что с ним связано (его родительский контейнер)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Если игрок вложен в контейнер (например, PersistentRoot), удаляем весь корень
            Destroy(player.transform.root.gameObject);
        }

        // 2. Убиваем игровые системы, которые НЕ НУЖНЫ в меню
        if (InventoryManager.Instance != null)
            Destroy(InventoryManager.Instance.gameObject);

        if (DialogueManager.Instance != null)
            Destroy(DialogueManager.Instance.gameObject);

        if (QuestsManager.Instance != null)
            Destroy(QuestsManager.Instance.gameObject);

        // ВАЖНО: GameSaveManager.Instance мы НЕ ТРОГАЕМ! 
        // Он должен жить, чтобы загрузить игру.

        // 3. Настройка состояния
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 1f;

        Debug.Log("Меню очищено. Системы инвентаря и игрока удалены. Менеджер сохранений сохранен.");
    }
}