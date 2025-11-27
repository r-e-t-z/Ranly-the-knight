using UnityEngine;

[CreateAssetMenu(fileName = "New NPC", menuName = "Dialogue/NPC Data")]
public class NPCData : ScriptableObject
{
    [Header("Basic Info")]
    public string npcName;
    public Sprite portrait;

    [Header("Ink File")]
    public TextAsset inkFile;
}