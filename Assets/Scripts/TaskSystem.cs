using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
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

    [Header("Визуал после заданий 1 и 2")]
    [Tooltip("Кристаллы/цветок после заданий с индексом 0 и 1 — настройки на компоненте Task1CompletionVisuals.")]
    [SerializeField] private Task1CompletionVisuals task1CompletionVisuals;

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

        InterpreterEngine snapshotBefore = engine.Clone();

        bool executed = engine.Execute(code);

        if (!executed)
        {
            dialogueManager.ReplaceCurrentText("Ошибка синтаксиса. Попробуй ещё раз.");
            return;
        }

        CheckTask(code, snapshotBefore);
        codeInput.text = "";
    }

    void CheckTask(string userCode, InterpreterEngine snapshotBefore)
    {
        if (currentTaskIndex >= tasks.Length)
        {
            Debug.LogWarning("TaskSystem: нет больше заданий для проверки!");
            return;
        }

        TaskData task = tasks[currentTaskIndex];
        bool passed = false;

        if (!string.IsNullOrWhiteSpace(task.referenceCode))
        {
            passed = CheckTaskAgainstReference(task, snapshotBefore);
        }
        else
        {
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

            case TaskType.BooleanDoorRiddle:
                passed = CheckBooleanDoorRiddle(task);
                break;
            }
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
        int completedTaskIndex = currentTaskIndex;
        currentTaskIndex++;

        // Сбрасываем lastPrintedValue для следующего задания
        engine.lastPrintedValue = "";

        if (task1CompletionVisuals != null)
        {
            if (completedTaskIndex == 0 && task1CompletionVisuals.enableForTask1)
                task1CompletionVisuals.PlayTask1();
            else if (completedTaskIndex == 1 && task1CompletionVisuals.enableForTask2)
                task1CompletionVisuals.PlayTask2();
        }

        // Сообщаем DialogueManager что задание выполнено (после 1-го — пауза без панели, см. DialogueManager)
        dialogueManager.OnTaskCompleted(completedTaskIndex == 0);
    }

    bool CheckTaskAgainstReference(TaskData task, InterpreterEngine snapshotBefore)
    {
        var refEngine = snapshotBefore.Clone();
        string refCode = task.referenceCode.Trim();
        if (!refEngine.Execute(refCode))
        {
            Debug.LogWarning("TaskSystem: эталонный код (referenceCode) не выполняется — проверь синтаксис в задании.");
            return false;
        }

        List<object> refChanged = CollectChangedVariableValues(snapshotBefore.variables, refEngine.variables);
        List<object> stuChanged = CollectChangedVariableValues(snapshotBefore.variables, engine.variables);
        if (!MultisetEqualValues(refChanged, stuChanged))
            return false;

        return ReferencePrintMatches(task.taskType, engine.lastPrintedValue, refEngine.lastPrintedValue);
    }

    static List<object> CollectChangedVariableValues(
        Dictionary<string, object> snapshot,
        Dictionary<string, object> final)
    {
        var list = new List<object>();
        foreach (var kv in final)
        {
            if (!snapshot.TryGetValue(kv.Key, out object oldVal) || !ValuesEqual(oldVal, kv.Value))
                list.Add(kv.Value);
        }

        return list;
    }

    static bool MultisetEqualValues(List<object> a, List<object> b)
    {
        if (a.Count != b.Count)
            return false;

        var used = new bool[b.Count];
        for (int i = 0; i < a.Count; i++)
        {
            int found = -1;
            for (int j = 0; j < b.Count; j++)
            {
                if (used[j])
                    continue;
                if (ValuesEqual(a[i], b[j]))
                {
                    found = j;
                    break;
                }
            }

            if (found < 0)
                return false;
            used[found] = true;
        }

        return true;
    }

    static bool ValuesEqual(object a, object b)
    {
        if (a == null && b == null)
            return true;
        if (a == null || b == null)
            return false;

        if (TryGetNumeric(a, out float fa) && TryGetNumeric(b, out float fb))
            return Mathf.Approximately(fa, fb);

        if (TryGetBoolVariable(a, out bool ba) && TryGetBoolVariable(b, out bool bb))
            return ba == bb;

        return string.Equals(a.ToString(), b.ToString(), System.StringComparison.Ordinal);
    }

    static bool TryGetNumeric(object v, out float f)
    {
        f = 0f;
        if (v is float x)
        {
            f = x;
            return true;
        }

        if (v is int i)
        {
            f = i;
            return true;
        }

        if (v is double d)
        {
            f = (float)d;
            return true;
        }

        return float.TryParse(
            v?.ToString(),
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out f);
    }

    static bool ReferencePrintMatches(TaskType type, string studentPrint, string referencePrint)
    {
        string rs = referencePrint.Trim();
        string ss = studentPrint.Trim();
        if (string.IsNullOrEmpty(rs))
            return true;

        if (type == TaskType.PrintContains)
            return ss.IndexOf(rs, System.StringComparison.Ordinal) >= 0;

        return DoorPrintMatches(ss, rs);
    }

    bool CheckBooleanDoorRiddle(TaskData task)
    {
        if (string.IsNullOrEmpty(task.doorPrintIfLeft) || string.IsNullOrEmpty(task.doorPrintIfRight))
        {
            Debug.LogWarning("TaskSystem: BooleanDoorRiddle — задайте doorPrintIfLeft и doorPrintIfRight.");
            return false;
        }

        string output = engine.lastPrintedValue.Trim();
        if (string.IsNullOrEmpty(output))
            return false;

        // Любые имена переменных: ищем упорядоченную пару (левое утверждение, правое утверждение)
        // со значениями из задания; итог «левая честна» = left && !right; сверяем print.
        foreach (string kL in engine.variables.Keys)
        {
            if (!TryGetBoolVariable(engine.variables[kL], out bool leftVal))
                continue;

            foreach (string kR in engine.variables.Keys)
            {
                if (kR == kL)
                    continue;
                if (!TryGetBoolVariable(engine.variables[kR], out bool rightVal))
                    continue;
                if (leftVal != task.doorExpectedLeft || rightVal != task.doorExpectedRight)
                    continue;

                bool expectedTruthful = leftVal && !rightVal;
                string expectedPrint = expectedTruthful ? task.doorPrintIfLeft : task.doorPrintIfRight;
                if (DoorPrintMatches(output, expectedPrint))
                    return true;
            }
        }

        return false;
    }

    static bool DoorPrintMatches(string actual, string expected)
    {
        actual = actual.Trim();
        expected = expected.Trim();
        if (string.Equals(actual, expected, System.StringComparison.Ordinal))
            return true;

        string normA = Regex.Replace(actual, @"\s+", " ");
        string normE = Regex.Replace(expected, @"\s+", " ");
        return string.Equals(normA, normE, System.StringComparison.Ordinal);
    }

    static bool TryGetBoolVariable(object v, out bool result)
    {
        result = false;
        if (v is bool b)
        {
            result = b;
            return true;
        }

        if (bool.TryParse(v?.ToString(), out bool parsed))
        {
            result = parsed;
            return true;
        }

        return false;
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