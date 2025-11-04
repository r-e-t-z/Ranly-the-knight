using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Dialogue/Character")]
public class Character : ScriptableObject
{
    public string characterName;
    public Sprite portrait;
    public Color nameColor = Color.white;
}