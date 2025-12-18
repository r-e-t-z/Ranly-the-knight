using UnityEngine;

[CreateAssetMenu(fileName = "New NPC", menuName = "Dialogue/NPC Data")]
public class NPCData : ScriptableObject
{
    [Header("Basic Info")]
    public string npcName;
    public Sprite portrait;

    [Header("Voice Settings")]
    [Tooltip("Звук, который проигрывается при появлении букв")]
    public AudioClip voiceSound;

    [Tooltip("Задержка между буквами (секунды). 0.05 = нормально.")]
    [Range(0.01f, 0.2f)]
    public float typingSpeed = 0.04f;

    [Header("Ink File")]
    public TextAsset inkFile;
}