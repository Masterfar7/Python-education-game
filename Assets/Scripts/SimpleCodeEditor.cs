using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

public class SimplePythonInterpreter : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_InputField codeInput;
    public TMP_Text outputText;
    public Button runButton;
    public Button clearButton;
    public ScrollRect outputScrollRect;

    [Header("Task System UI")]
    public TMP_Text taskText;
    public TMP_Text hintText;
    public GameObject hintPanel;

    [Header("Current Task")]
    public string taskDescription = "Выведи на экран слово Hello";
    public string expectedAnswer = "print(\"Hello\")";

    [Header("Cloudflare AI Settings")]
    public string apiToken = "v0evrauS54DVMbYhdhndrc-V9IDmlvkcYE4TjmkY";
    public string accountId = "a0b0bcb493fbd1d65b5be16394b43305";
    public string aiModel = "@cf/meta/llama-3.1-8b-instruct";

    [Header("Settings")]
    public int maxOutputLines = 50;

    // Переменные интерпретатора
    private Dictionary<string, object> variables = new Dictionary<string, object>();
    private List<string> outputHistory = new List<string>();
    private List<string> codeHistory = new List<string>();
    private Dictionary<string, Delegate> builtinFunctions = new Dictionary<string, Delegate>();
    private bool isAIProcessing = false;

    void Start()
    {
        InitializeInterpreter();
        SetupUI();
        ShowTask();
    }

    void ShowTask()
    {
        if (taskText != null)
            taskText.text = $"Задание: {taskDescription}";

        if (hintPanel != null)
            hintPanel.SetActive(false);
    }

    void InitializeInterpreter()
    {
        variables.Clear();
        RegisterBuiltinFunctions();

        variables["PI"] = Mathf.PI;
        variables["E"] = Mathf.Exp(1f);
        variables["True"] = true;
        variables["False"] = false;
        variables["None"] = null;
    }

    void RegisterBuiltinFunctions()
    {
        builtinFunctions["print"] = new Action<object[]>(Print);
        builtinFunctions["len"] = new Func<object, int>(Len);
        builtinFunctions["abs"] = new Func<float, float>(Mathf.Abs);
        builtinFunctions["sqrt"] = new Func<float, float>(Mathf.Sqrt);
        builtinFunctions["sin"] = new Func<float, float>(Mathf.Sin);
        builtinFunctions["cos"] = new Func<float, float>(Mathf.Cos);
        builtinFunctions["tan"] = new Func<float, float>(Mathf.Tan);
        builtinFunctions["pow"] = new Func<float, float, float>(Mathf.Pow);
        builtinFunctions["round"] = new Func<float, int, float>(Round);

        builtinFunctions["create_cube"] = new Action<float, float, float, float>(CreateCube);
        builtinFunctions["create_sphere"] = new Action<float, float, float>(CreateSphere);
        builtinFunctions["create_cylinder"] = new Action<float, float, float>(CreateCylinder);
        builtinFunctions["set_color"] = new Action<float, float, float, float>(SetBackgroundColor);
        builtinFunctions["get_time"] = new Func<float>(GetTime);
        builtinFunctions["random"] = new Func<float, float, float>(RandomRange);

        builtinFunctions["help"] = new Action(ShowHelp);
        builtinFunctions["clear"] = new Action(ClearScene);
        builtinFunctions["list_vars"] = new Action(ListVariables);
    }

    void SetupUI()
    {
        runButton.onClick.AddListener(ExecuteCode);
        clearButton.onClick.AddListener(ClearOutput);

        codeInput.onSubmit.AddListener(OnCodeSubmitted);
        codeInput.lineType = TMP_InputField.LineType.MultiLineNewline;

        codeInput.Select();
        codeInput.ActivateInputField();
    }

    public void ExecuteCode()
    {
        string code = codeInput.text.Trim();

        if (string.IsNullOrEmpty(code))
        {
            PrintError("Введите код!");
            return;
        }

        codeHistory.Add(code);
        if (codeHistory.Count > 100) codeHistory.RemoveAt(0);

        PrintOutput($">>> {code}");

        bool hasError = false;

        try
        {
            object result = Interpret(code);
            if (result != null)
            {
                PrintOutput($"{result}");
            }
        }
        catch (Exception e)
        {
            PrintError($"Ошибка: {e.Message}");
            hasError = true;
        }

        // === ПРОВЕРКА ЗАДАНИЯ ===
        CheckTask(code, hasError);

        codeInput.text = "";
        codeInput.Select();
        codeInput.ActivateInputField();
    }

    // ========== СИСТЕМА ЗАДАНИЙ + AI ==========

    void CheckTask(string userCode, bool hadError)
    {
        string normalizedUser = userCode;
        string normalizedExpected = expectedAnswer;

        if (normalizedUser == normalizedExpected)
        {
            PrintOutput("<color=green>Правильно! Молодец!</color>");
            HideHint();
        }
        else
        {
            PrintOutput("<color=yellow>Не совсем так... Смотри подсказку.</color>");
            AskAIForHint(userCode);
        }
    }

    string NormalizeCode(string code)
    {
        return code
            .Replace(" ", "")
            .Replace("'", "\"")
            .ToLower()
            .Trim();
    }

    async void AskAIForHint(string userCode)
    {
        if (isAIProcessing) return;

        isAIProcessing = true;
        ShowHint("Думаю...");

        try
        {
            string hint = await GetHintFromCloudflare(userCode);
            ShowHint(hint);
        }
        catch (Exception e)
        {
            ShowHint($"Ошибка AI: {e.Message}");
        }
        finally
        {
            isAIProcessing = false;
        }
    }

    async Task<string> GetHintFromCloudflare(string userCode)
    {
        // URL нужно формировать правильно - без @ проблемы
        string url = "https://api.cloudflare.com/client/v4/accounts/" +
            accountId + "/ai/run/@cf/meta/llama-3.1-8b-instruct";

        Debug.Log($"URL: {url}");

        string prompt = "You are a programming teacher. " +
            "Do NOT show the correct answer. " +
            "Give a SHORT hint in 1-2 sentences in Russian. " +
            "Task: " + taskDescription.Replace("\"", "'") + ". " +
            "Student wrote: " + userCode.Replace("\"", "'") + ". " +
            "Give a small hint without showing the answer:";

        string json = "{\"messages\":[{\"role\":\"user\",\"content\":\"" +
            prompt.Replace("\\", "\\\\").Replace("\n", " ").Replace("\r", " ") +
            "\"}]}";

        Debug.Log($"JSON: {json}");

        using (HttpClient client = new HttpClient())
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + apiToken.Trim());
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.SendAsync(request);
            string body = await response.Content.ReadAsStringAsync();

            Debug.Log($"Status: {response.StatusCode}");
            Debug.Log($"Body: {body}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"API error: {response.StatusCode} - {body}");
            }

            return ParseCloudflareResponse(body);
        }
    }

    string EscapeJson(string text)
    {
        return text
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }

    string ParseCloudflareResponse(string json)
    {
        try
        {
            int start = json.IndexOf("\"response\":\"");
            if (start == -1)
            {
                start = json.IndexOf("\"response\": \"");
                if (start == -1) return "Попробуй ещё раз!";
                start += 13;
            }
            else
            {
                start += 12;
            }

            int end = start;
            bool escaped = false;

            for (int i = start; i < json.Length; i++)
            {
                if (json[i] == '\\' && !escaped) { escaped = true; continue; }
                if (json[i] == '"' && !escaped) { end = i; break; }
                escaped = false;
            }

            return json.Substring(start, end - start)
                .Replace("\\n", "\n")
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\");
        }
        catch
        {
            return "Подумай ещё немного!";
        }
    }

    void ShowHint(string text)
    {
        if (hintPanel != null)
            hintPanel.SetActive(true);

        if (hintText != null)
            hintText.text = text;
    }

    void HideHint()
    {
        if (hintPanel != null)
            hintPanel.SetActive(false);
    }

    // ========== ИНТЕРПРЕТАТОР ==========

    object Interpret(string code)
    {
        if (code.StartsWith("#")) return null;

        Match assignment = Regex.Match(code, @"^\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.+)$");
        if (assignment.Success)
        {
            string varName = assignment.Groups[1].Value;
            string expression = assignment.Groups[2].Value;
            object value = EvaluateExpression(expression);
            variables[varName] = value;
            return null;
        }

        Match functionCall = Regex.Match(code, @"^\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*\((.*)\)\s*$");
        if (functionCall.Success)
        {
            string funcName = functionCall.Groups[1].Value;
            string argsStr = functionCall.Groups[2].Value;
            return CallFunction(funcName, argsStr);
        }

        return EvaluateExpression(code);
    }

    object EvaluateExpression(string expression)
    {
        expression = expression.Trim();

        if (string.IsNullOrEmpty(expression)) return null;

        if (expression.StartsWith("\"") && expression.EndsWith("\""))
            return expression.Substring(1, expression.Length - 2);

        if (expression == "True") return true;
        if (expression == "False") return false;

        if (variables.ContainsKey(expression))
            return variables[expression];

        if (float.TryParse(expression, NumberStyles.Float, CultureInfo.InvariantCulture, out float number))
            return number;

        return EvaluateArithmetic(expression);
    }

    object EvaluateArithmetic(string expression)
    {
        try
        {
            expression = expression.Replace(" ", "");

            while (expression.Contains("("))
            {
                Match inner = Regex.Match(expression, @"\(([^()]+)\)");
                if (inner.Success)
                {
                    object innerResult = EvaluateArithmetic(inner.Groups[1].Value);
                    expression = expression.Replace(inner.Groups[0].Value, innerResult.ToString());
                }
            }

            Match mulDiv = Regex.Match(expression, @"([\d\.]+)([\*/])([\d\.]+)");
            while (mulDiv.Success)
            {
                float a = float.Parse(mulDiv.Groups[1].Value);
                float b = float.Parse(mulDiv.Groups[3].Value);
                float result = mulDiv.Groups[2].Value == "*" ? a * b : a / b;
                expression = expression.Replace(mulDiv.Groups[0].Value, result.ToString());
                mulDiv = Regex.Match(expression, @"([\d\.]+)([\*/])([\d\.]+)");
            }

            Match addSub = Regex.Match(expression, @"([\d\.]+)([\+\-])([\d\.]+)");
            while (addSub.Success)
            {
                float a = float.Parse(addSub.Groups[1].Value);
                float b = float.Parse(addSub.Groups[3].Value);
                float result = addSub.Groups[2].Value == "+" ? a + b : a - b;
                expression = expression.Replace(addSub.Groups[0].Value, result.ToString());
                addSub = Regex.Match(expression, @"([\d\.]+)([\+\-])([\d\.]+)");
            }

            return float.Parse(expression);
        }
        catch
        {
            throw new Exception($"Не могу вычислить: {expression}");
        }
    }

    List<object> ParseArguments(string argsStr)
    {
        List<object> args = new List<object>();
        if (string.IsNullOrEmpty(argsStr.Trim())) return args;

        List<string> argStrings = new List<string>();
        string currentArg = "";
        bool inQuotes = false;

        for (int i = 0; i < argsStr.Length; i++)
        {
            char c = argsStr[i];
            if (c == '"' && (i == 0 || argsStr[i - 1] != '\\'))
            {
                inQuotes = !inQuotes;
                currentArg += c;
            }
            else if (c == ',' && !inQuotes)
            {
                argStrings.Add(currentArg.Trim());
                currentArg = "";
            }
            else
            {
                currentArg += c;
            }
        }

        if (!string.IsNullOrEmpty(currentArg.Trim()))
            argStrings.Add(currentArg.Trim());

        foreach (string argStr in argStrings)
            args.Add(EvaluateExpression(argStr));

        return args;
    }

    object CallFunction(string funcName, string argsStr)
    {
        if (!builtinFunctions.ContainsKey(funcName))
            throw new Exception($"Функция '{funcName}' не найдена");

        List<object> args = ParseArguments(argsStr);

        try
        {
            Delegate func = builtinFunctions[funcName];

            if (func is Action<object[]>)
            {
                ((Action<object[]>)func)(args.ToArray());
                return null;
            }

            if (func is Func<object, int> func1 && args.Count == 1)
                return func1(args[0]);

            if (func is Func<float, float, float> func2 && args.Count == 2)
                return func2(Convert.ToSingle(args[0]), Convert.ToSingle(args[1]));

            if (func is Func<float, float> func3 && args.Count == 1)
                return func3(Convert.ToSingle(args[0]));

            if (func is Action action && args.Count == 0)
            {
                action();
                return null;
            }

            throw new Exception($"Неверные аргументы для {funcName}");
        }
        catch (Exception e)
        {
            throw new Exception($"Ошибка {funcName}: {e.Message}");
        }
    }

    #region Built-in Functions

    void Print(object[] args)
    {
        string output = "";
        foreach (var arg in args)
            output += (arg == null ? "None" : arg.ToString()) + " ";
        PrintOutput(output.Trim());
    }

    int Len(object obj)
    {
        if (obj is string str) return str.Length;
        throw new Exception("len() ожидает строку");
    }

    float Round(float value, int decimals = 0) => (float)Math.Round(value, decimals);

    void CreateCube(float x = 0, float y = 0, float z = 0, float size = 1)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(x, y, z);
        cube.transform.localScale = Vector3.one * size;
        cube.name = $"Cube_{DateTime.Now:HHmmss}";
        cube.GetComponent<Renderer>().material.color = new Color(
            UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
        PrintOutput($"Создан куб в ({x}, {y}, {z})");
    }

    void CreateSphere(float x = 0, float y = 0, float z = 0)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = new Vector3(x, y, z);
        sphere.name = $"Sphere_{DateTime.Now:HHmmss}";
        sphere.GetComponent<Renderer>().material.color = new Color(
            UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
        PrintOutput($"Создана сфера в ({x}, {y}, {z})");
    }

    void CreateCylinder(float x = 0, float y = 0, float z = 0)
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.position = new Vector3(x, y, z);
        cylinder.name = $"Cylinder_{DateTime.Now:HHmmss}";
        cylinder.GetComponent<Renderer>().material.color = new Color(
            UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
        PrintOutput($"Создан цилиндр в ({x}, {y}, {z})");
    }

    void SetBackgroundColor(float r, float g, float b, float a = 1)
    {
        Camera.main.backgroundColor = new Color(r, g, b, a);
        PrintOutput($"Цвет фона: ({r}, {g}, {b})");
    }

    float GetTime() => Time.time;
    float RandomRange(float min, float max) => UnityEngine.Random.Range(min, max);

    void ShowHelp()
    {
        PrintOutput("=== Справка ===");
        PrintOutput("print('текст'), create_cube(x,y,z,size)");
        PrintOutput("create_sphere(x,y,z), set_color(r,g,b,a)");
        PrintOutput("help(), clear(), list_vars()");
    }

    void ClearScene()
    {
        var toDestroy = new List<GameObject>();
        foreach (var obj in FindObjectsOfType<GameObject>())
            if (obj.name.StartsWith("Cube_") || obj.name.StartsWith("Sphere_") || obj.name.StartsWith("Cylinder_"))
                toDestroy.Add(obj);

        foreach (var obj in toDestroy) Destroy(obj);
        PrintOutput($"Удалено {toDestroy.Count} объектов");
    }

    void ListVariables()
    {
        PrintOutput("=== Переменные ===");
        foreach (var kvp in variables)
            if (!kvp.Key.StartsWith("_"))
                PrintOutput($"  {kvp.Key} = {kvp.Value}");
    }

    #endregion

    #region UI Methods

    void OnCodeSubmitted(string code)
    {
        if (Input.GetKey(KeyCode.Return) && !Input.GetKey(KeyCode.LeftShift))
            ExecuteCode();
    }

    void PrintOutput(string message)
    {
        outputHistory.Add(message);
        if (outputHistory.Count > maxOutputLines) outputHistory.RemoveAt(0);
        outputText.text = string.Join("\n", outputHistory);
        if (outputScrollRect != null) StartCoroutine(ScrollToBottom());
    }

    System.Collections.IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        if (outputScrollRect != null) outputScrollRect.verticalNormalizedPosition = 0f;
    }

    void PrintError(string error)
    {
        outputHistory.Add($"<color=red>{error}</color>");
        PrintOutput("");
    }

    void ClearOutput()
    {
        outputHistory.Clear();
        outputText.text = "";
        PrintOutput("Очищено");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5)) ExecuteCode();
        else if (Input.GetKeyDown(KeyCode.F1)) ClearOutput();
        else if (Input.GetKeyDown(KeyCode.F2)) ShowHelp();
        else if (Input.GetKeyDown(KeyCode.F3)) ListVariables();
    }

    #endregion
}