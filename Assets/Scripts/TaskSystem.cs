using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TaskSystem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField codeInput;
    [SerializeField] private Button runButton;

    [Header("Dialogue")]
    [SerializeField] private DialogueManager dialogueManager;

    [Header("Tasks (только для проверки, без описаний)")]
    [SerializeField] private TaskData[] tasks;

    [Header("AI Helper")]
    [SerializeField] private TaskAIAdvisor aiAdvisor;

    private int currentTaskIndex = 0;
    private InterpreterEngine engine = new InterpreterEngine();
    private bool waitingForAI = false;

    void Start()
    {
        runButton.onClick.AddListener(ExecuteCode);
    }

    // Сброс для нового диалога
    public void ResetTasks()
    {
        currentTaskIndex = 0;
        engine = new InterpreterEngine();
        waitingForAI = false;
    }

    void ExecuteCode()
    {
        if (waitingForAI)
            return;

        // Проверяем что мы в режиме задания
        if (!dialogueManager.IsTaskMode())
            return;

        string code = codeInput.text.Trim();

        if (string.IsNullOrEmpty(code))
            return;

        bool executed = engine.Execute(code);

        if (!executed)
        {
            dialogueManager.ReplaceCurrentText("Ошибка синтаксиса. Попробуй ещё раз.");
            return;
        }

        CheckTask(code);
        codeInput.text = "";
    }

    void CheckTask(string userCode)
    {
        if (currentTaskIndex >= tasks.Length)
        {
            Debug.LogWarning("TaskSystem: нет больше заданий для проверки!");
            return;
        }

        TaskData task = tasks[currentTaskIndex];
        bool passed = false;

        switch (task.taskType)
        {
            case TaskType.PrintExact:
                passed = engine.lastPrintedValue.Trim() == task.expectedString.Trim();
                break;

            case TaskType.PrintContains:
                passed = engine.lastPrintedValue.Contains(task.expectedString);
                break;

            case TaskType.VariableAssignment:
                passed = engine.variables.ContainsKey(task.variableName);
                break;

            case TaskType.ExpressionResult:
                if (engine.variables.ContainsKey(task.variableName))
                {
                    object raw = engine.variables[task.variableName];
                    float val;
                    if (raw is float f) val = f;
                    else if (raw is int i) val = i;
                    else if (float.TryParse(raw.ToString(), out float parsed)) val = parsed;
                    else break;

                    passed = Mathf.Approximately(val, task.expectedNumber);
                }

                if (!passed)
                {
                    if (float.TryParse(
                        engine.lastPrintedValue.Replace(',', '.'),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out float printed))
                    {
                        passed = Mathf.Approximately(printed, task.expectedNumber);
                    }
                }
                break;

            case TaskType.BooleanValue:
                if (engine.variables.ContainsKey(task.variableName))
                {
                    object rawBool = engine.variables[task.variableName];
                    if (rawBool is bool b)
                        passed = b == task.expectedBool;
                    else if (bool.TryParse(rawBool.ToString(), out bool parsedBool))
                        passed = parsedBool == task.expectedBool;
                }

                if (!passed)
                {
                    string lv = engine.lastPrintedValue;
                    if (task.expectedBool && lv.Contains("True"))
                        passed = true;
                    if (!task.expectedBool && lv.Contains("False"))
                        passed = true;
                }

                // Дополнительная защита на случай, если интерпретатор не сохранил переменную,
                // но код написан верно в явном виде.
                if (!passed)
                {
                    string normalizedCode = userCode.Replace(" ", "");
                    string expectedLiteral = task.expectedBool ? "True" : "False";

                    if (normalizedCode.Contains(task.variableName + "=" + expectedLiteral))
                        passed = true;
                }

                // Ещё более мягкая проверка: просто ищем в коде имя переменной и нужное слово True/False
                if (!passed)
                {
                    string lower = userCode.ToLowerInvariant();
                    if (lower.Contains(task.variableName.ToLowerInvariant()) &&
                        lower.Contains(task.expectedBool ? "true" : "false"))
                    {
                        passed = true;
                    }
                }
                break;
        }

        if (passed)
        {
            OnTaskPassed();
        }
        else
        {
            OnTaskFailed(task, userCode);
        }
    }

    void OnTaskPassed()
    {
        currentTaskIndex++;

        // Сбрасываем lastPrintedValue для следующего задания
        engine.lastPrintedValue = "";

        // Сообщаем DialogueManager что задание выполнено
        dialogueManager.OnTaskCompleted();
    }

    void OnTaskFailed(TaskData task, string userCode)
    {
        if (aiAdvisor != null)
        {
            waitingForAI = true;
            dialogueManager.ReplaceCurrentText("Думаю над твоим ответом...");

            aiAdvisor.EvaluateSolution(
                task,
                userCode,
                engine.lastPrintedValue,
                OnAIResult
            );
        }
        else
        {
            dialogueManager.ReplaceCurrentText("Не совсем верно. Попробуй ещё раз.");
        }
    }

    void OnAIResult(bool correct, string hint)
    {
        waitingForAI = false;

        if (!string.IsNullOrWhiteSpace(hint))
            dialogueManager.ReplaceCurrentText(hint);
        else
            dialogueManager.ReplaceCurrentText("Не совсем верно. Попробуй ещё раз.");
    }

    /// <summary>
    /// Админский метод: принудительно установить индекс текущего задания.
    /// Используется при перемотке диалога, чтобы система думала,
    /// что предыдущие задания уже пройдены.
    /// </summary>
    public void AdminSetCurrentTaskIndex(int index)
    {
        if (tasks == null || tasks.Length == 0)
        {
            currentTaskIndex = 0;
            return;
        }

        currentTaskIndex = Mathf.Clamp(index, 0, tasks.Length - 1);

        // На всякий случай очищаем lastPrintedValue,
        // чтобы старый вывод не мешал следующей проверке.
        engine.lastPrintedValue = "";
    }

#if UNITY_EDITOR
    [ContextMenu("Fill Default Tasks (Chapter 1)")]
    private void FillDefaultTasksChapter1()
    {
        tasks = new TaskData[4];

        tasks[0] = new TaskData
        {
            taskType = TaskType.VariableAssignment,
            variableName = "spirit_name"
        };

        tasks[1] = new TaskData
        {
            taskType = TaskType.PrintContains,
            expectedString = "Привет, "
        };

        tasks[2] = new TaskData
        {
            taskType = TaskType.ExpressionResult,
            variableName = "strike_power",
            expectedNumber = 63.75f
        };

        tasks[3] = new TaskData
        {
            taskType = TaskType.BooleanValue,
            variableName = "bool_switch",
            expectedBool = true
        };

        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}