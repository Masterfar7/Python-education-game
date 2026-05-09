using UnityEngine;
using System.IO;
using System.Linq;

public class DialogueExporter : MonoBehaviour
{
    [Header("Перетащите сюда объект с диалогами:")]
    // ЗАМЕНИТЕ "ВАШ_СКРИПТ" НА НАЗВАНИЕ СКРИПТА, ГДЕ ЛЕЖИТ МАССИВ DIALOGUELINE
    public NPCDialogue source; 

    [ContextMenu("Сохранить диалог в TXT")]
    public void ExportDialogue()
    {
        if (source == null)
        {
            Debug.LogError("Вы не перетащили скрипт-источник в поле Source!");
            return;
        }

        // ЗАМЕНИТЕ "phrases" НА НАЗВАНИЕ ВАШЕГО МАССИВА (если он называется иначе)
        var dialogLines = source.dialogueLines; 

        if (dialogLines == null || dialogLines.Length == 0)
        {
            Debug.LogWarning("Массив пуст!");
            return;
        }

        // 1. Берем каждый элемент массива (line)
        // 2. Достаем из него переменную text
        // 3. Превращаем обратно в массив строк
        string[] textsOnly = dialogLines.Select(line => line.text).ToArray();

        // Объединяем все фразы, вставляя между ними пустую строку (\n\n)
        string finalContent = string.Join("\n\n", textsOnly);

        // Путь сохранения: папка проекта / Assets / DialoguesExport.txt
        string path = Path.Combine(Application.dataPath, "DialoguesExport.txt");

        File.WriteAllText(path, finalContent);

        Debug.Log("✅ Успешно! Файл сохранен по пути: " + path);
        
        // Обновляем окно проекта, чтобы файл появился сразу
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
}