using UnityEngine;
using System.IO;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue")]
public class Dialogue : ScriptableObject
{
    [System.Serializable]
    public class DialogueLine
    {
        public Character speaker;
        [TextArea(3, 8)]
        public string text;
    }

    [Header("Персонажи (до 4)")]
    public Character[] characters = new Character[4];

    [Header("Строки диалога")]
    public DialogueLine[] lines;

    [Header("Текстовый файл (опционально)")]
    public TextAsset textFile;

    // Метод для загрузки из файла 
    public void LoadFromTextFile()
    {

        string[] fileLines = textFile.text.Split('\n');
        List<DialogueLine> dialogueLines = new List<DialogueLine>();

        for (int i = 0; i < fileLines.Length; i++)
        {
            string line = fileLines[i].Trim();

            if (string.IsNullOrEmpty(line)) continue;

            DialogueLine dialogueLine = new DialogueLine();

            if (line.Contains(":"))
            {
                string[] parts = line.Split(':');
                if (parts.Length >= 2)
                {
                    string charName = parts[0].Trim();
                    string text = parts[1].Trim();

                    foreach (Character character in characters)
                    {
                        if (character != null && character.characterName == charName)
                        {
                            dialogueLine.speaker = character;
                            break;
                        }
                    }

                    dialogueLine.text = text;
                }
            }
            else
            {
                dialogueLine.text = line;
            }

            dialogueLines.Add(dialogueLine);
        }

        lines = dialogueLines.ToArray();
    }
}