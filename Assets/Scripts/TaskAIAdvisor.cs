using UnityEngine;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System;

public class TaskAIAdvisor : MonoBehaviour
{
    // ХАРДКОД, как в TestAPI.cs – чтобы точно работало
    // Если потом захочешь, можно вынести в настройки/ScriptableObject.
    private const string ApiToken = "v0evrauS54DVMbYhdhndrc-V9IDmlvkcYE4TjmkY";
    private const string AccountId = "a0b0bcb493fbd1d65b5be16394b43305";
    private const string Model = "@cf/meta/llama-3.1-8b-instruct";

    [Serializable]
    private class AiJudgeResult
    {
        public bool correct;
        public string hint;
        public string error_type;
    }

    public const string ERROR_NONE = "none";
    public const string ERROR_SYNTAX = "syntax_error";
    public const string ERROR_RUNTIME = "runtime_error";
    public const string ERROR_LOGIC = "logic_error";

    /// <summary>
    /// Запрос к ИИ: верен ли ответ на задание, короткая подсказка и тип ошибки.
    /// callback(correct, hint, errorType)
    /// </summary>
    public async void EvaluateSolution(TaskData task, string userCode, string engineOutput,
        Action<bool, string, string> callback)
    {
        string url =
            $"https://api.cloudflare.com/client/v4/accounts/{AccountId}/ai/run/{Model}";

        string referencePart = string.IsNullOrWhiteSpace(task.referenceCode)
            ? ""
            : $"\nЭталон для проверки (не выдавай его дословно игроку):\n{task.referenceCode}\n\n";

        string prompt =
            "Ты помощник по Python внутри обучающей игры. " +
            "Твоя задача — оценить, верно ли выполнено задание, дать КОРОТКУЮ подсказку и классифицировать ошибку.\n\n" +
            "Типы ошибок:\n" +
            "- syntax_error: неверный синтаксис Python\n" +
            "- runtime_error: ошибка выполнения (деление на 0, обращение к несуществующей переменной и т.д.)\n" +
            "- logic_error: синтаксис верный, но логика неверна\n" +
            "- none: задание выполнено верно\n\n" +
            $"Задание: {task.taskDescription}\n\n" +
            referencePart +
            $"Код игрока:\n{userCode}\n\n" +
            $"Последний вывод интерпретатора (print): {engineOutput}\n\n" +
            "Ответь строго в JSON вида: {\"correct\":true/false,\"hint\":\"краткая подсказка\",\"error_type\":\"syntax_error|runtime_error|logic_error|none\"}.";

        // Аккуратно экранируем строку для JSON: кавычки, обратные слэши, переносы строк.
        string contentEscaped = prompt
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "")
            .Replace("\n", "\\n");

        string json =
            "{ \"messages\": [ { \"role\": \"user\", \"content\": \"" +
            contentEscaped +
            "\" } ] }";

        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Authorization", $"Bearer {ApiToken}");
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.SendAsync(request);
                string body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Debug.LogError($"TaskAIAdvisor HTTP error: {response.StatusCode}\n{body}");
                    callback?.Invoke(false, "Не удалось связаться с ИИ. Попробуй ещё раз чуть позже.", ERROR_NONE);
                    return;
                }

                string aiText = ExtractAIText(body);

                bool correct = false;
                string hint = "Попробуй ещё раз. Подумай, что именно проверяет задание.";
                string errorType = ERROR_NONE;

                try
                {
                    AiJudgeResult parsed = JsonUtility.FromJson<AiJudgeResult>(aiText);
                    if (parsed != null)
                    {
                        correct = parsed.correct;
                        if (!string.IsNullOrWhiteSpace(parsed.hint))
                            hint = parsed.hint.Trim();
                        if (!string.IsNullOrWhiteSpace(parsed.error_type))
                            errorType = parsed.error_type;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("TaskAIAdvisor parse error: " + e.Message + "\nAI TEXT:\n" + aiText);
                }

                callback?.Invoke(correct, hint, errorType);
            }
            catch (Exception ex)
            {
                Debug.LogError("TaskAIAdvisor exception: " + ex.Message);
                callback?.Invoke(false, "Ошибка связи с ИИ. Проверь интернет.", ERROR_NONE);
            }
        }
    }

    // Достаём текст модели из ответа Cloudflare AI (result.response или content)
    string ExtractAIText(string json)
    {
        try
        {
            int responseIndex = json.IndexOf("\"response\"");
            if (responseIndex != -1)
            {
                return ExtractStringValue(json, responseIndex);
            }

            int contentIndex = json.IndexOf("\"content\"");
            if (contentIndex != -1)
            {
                return ExtractStringValue(json, contentIndex);
            }

            return json;
        }
        catch
        {
            return json;
        }
    }

    string ExtractStringValue(string json, int keyIndex)
    {
        int colonIndex = json.IndexOf(":", keyIndex);
        int startQuote = json.IndexOf("\"", colonIndex + 1);
        int endQuote = startQuote + 1;

        bool escape = false;

        for (int i = startQuote + 1; i < json.Length; i++)
        {
            if (json[i] == '\\' && !escape)
            {
                escape = true;
                continue;
            }

            if (json[i] == '"' && !escape)
            {
                endQuote = i;
                break;
            }

            escape = false;
        }

        string result = json.Substring(startQuote + 1, endQuote - startQuote - 1);

        return result
            .Replace("\\n", "\n")
            .Replace("\\\"", "\"")
            .Replace("\\\\", "\\");
    }
}

