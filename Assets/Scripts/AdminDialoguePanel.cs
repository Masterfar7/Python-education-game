using UnityEngine;
using TMPro;

/// <summary>
/// Простая админская панель: вводишь номер реплики и проматываешь
/// текущий активный диалог до этой строки.
/// Работает только если диалог уже запущен.
/// </summary>
public class AdminDialoguePanel : MonoBehaviour
{
    [SerializeField] private TMP_InputField lineIndexInput;

    public void SkipToLineFromInput()
    {
        if (DialogueManager.Instance == null)
            return;

        if (lineIndexInput == null)
            return;

        if (!int.TryParse(lineIndexInput.text, out int targetDialogueIndex))
            return;

        // Пользователь вводит номера с 1, индексы внутри кода с 0
        targetDialogueIndex = Mathf.Max(0, targetDialogueIndex - 1);
        DialogueManager.Instance.AdminSkipToLine(targetDialogueIndex);
    }
}

