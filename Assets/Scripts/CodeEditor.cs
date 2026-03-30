using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI редактора кода на префабе CodeEditor. Запуск проверки кода делает TaskSystem на том же объекте — run не подключаем.
/// </summary>
[Serializable]
public class CodeEditorTask
{
    public string taskDescription;
    public string expectedOutput;
}

public class CodeEditor : MonoBehaviour
{
    [SerializeField] private TMP_InputField codeInput;
    [SerializeField] private TMP_Text outputText;
    [SerializeField] private Button runButton;
    [SerializeField] private Button clearButton;
    [SerializeField] private ScrollRect outputScrollRect;
    [SerializeField] private TMP_Text taskText;
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private GameObject hintPanel;

    [SerializeField] private string taskDescription;
    [SerializeField] private string expectedAnswer;
    [SerializeField] private string apiToken;
    [SerializeField] private string accountId;
    [SerializeField] private string aiModel;
    [SerializeField] private int maxOutputLines = 20;

    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private CodeEditorTask[] tasks;
    [SerializeField] private GameObject interpreterCanvas;

    void Start()
    {
        if (taskText != null && !string.IsNullOrEmpty(taskDescription))
            taskText.text = taskDescription;

        if (clearButton != null)
            clearButton.onClick.AddListener(ClearOutput);

        if (hintPanel != null)
            hintPanel.SetActive(false);
    }

    void Update()
    {
        if (dialogueManager != null && interpreterCanvas != null)
            interpreterCanvas.SetActive(dialogueManager.IsTaskMode());
    }

    void ClearOutput()
    {
        if (codeInput != null)
            codeInput.text = "";
        if (outputText != null)
            outputText.text = "";
    }
}
