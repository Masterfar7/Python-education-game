using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NPCDialogue))]
public class NPCDialogueEditor : Editor
{
    private TextAsset dialogueSource;

    public override void OnInspectorGUI()
    {
        // Рисуем стандартный инспектор NPCDialogue
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Auto-fill DialogueLines", EditorStyles.boldLabel);

        dialogueSource = (TextAsset)EditorGUILayout.ObjectField(
            "Dialogue Text File",
            dialogueSource,
            typeof(TextAsset),
            false
        );

        if (dialogueSource != null && GUILayout.Button("Import from Text File"))
        {
            ImportDialogue((NPCDialogue)target, dialogueSource);
        }
    }

    private void ImportDialogue(NPCDialogue npc, TextAsset source)
    {
        string[] lines = source.text.Split('\n');
        if (lines.Length == 0)
        {
            Debug.LogWarning("Dialogue file is empty.");
            return;
        }

        DialogueLine[] dialogueLines = new DialogueLine[lines.Length];

        for (int i = 0; i < lines.Length; i++)
        {
            string raw = lines[i].Trim();
            if (string.IsNullOrEmpty(raw))
                continue;

            int colonIndex = raw.IndexOf(':');
            if (colonIndex <= 0)
            {
                Debug.LogWarning($"Строка {i + 1} без двоеточия: \"{raw}\"");
                continue;
            }

            string speakerPart = raw.Substring(0, colonIndex).Trim();
            string textPart = raw.Substring(colonIndex + 1).Trim();

            bool startTask = false;
            if (speakerPart.EndsWith("*"))
            {
                startTask = true;
                speakerPart = speakerPart.TrimEnd('*').Trim();
            }

            // Маппинг русских имён на enum
            DialogueSpeaker speaker;
            switch (speakerPart)
            {
                case "Кай":
                    speaker = DialogueSpeaker.Player;
                    break;
                case "Аргус":
                    speaker = DialogueSpeaker.Guide;
                    break;
                case "Статуя":
                case "Дух":
                    speaker = DialogueSpeaker.Statue;
                    break;
                default:
                    if (!System.Enum.TryParse(speakerPart, true, out speaker))
                    {
                        Debug.LogWarning($"Неизвестный говорящий \"{speakerPart}\" в строке {i + 1}, по умолчанию Кай.");
                        speaker = DialogueSpeaker.Player;
                    }
                    break;
            }

            dialogueLines[i] = new DialogueLine
            {
                speaker = speaker,
                text = textPart,
                startTaskAfterLine = startTask
            };
        }

        Undo.RecordObject(npc, "Import Dialogue");
        npc.dialogueLines = dialogueLines;
        EditorUtility.SetDirty(npc);

        Debug.Log($"Imported {dialogueLines.Length} dialogue lines into {npc.name}.");
    }
}