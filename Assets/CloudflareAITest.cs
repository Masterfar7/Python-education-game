using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Threading.Tasks;

public class CloudflareAITest : MonoBehaviour
{
    [Header("Cloudflare Settings")]
    public string apiToken;
    public string accountId;
    public string model = "@cf/meta/llama-3.1-8b-instruct";

    [TextArea(3, 6)]
    public string testPrompt = "Скажи короткое приветствие на русском.";

    [ContextMenu("TEST AI")]
    public void TestAI()
    {
        _ = SendTestRequest();
    }

    async Task SendTestRequest()
    {
        string url =
            $"https://api.cloudflare.com/client/v4/accounts/{accountId}/ai/run/{model}";

        string json =
            "{\"messages\":[{\"role\":\"user\",\"content\":\"" +
            testPrompt.Replace("\"", "'") +
            "\"}]}";

        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiToken.Trim());

            Debug.Log("=== SENDING REQUEST ===");
            Debug.Log("URL: " + url);
            Debug.Log("JSON: " + json);

            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            Debug.Log("=== RESPONSE STATUS ===");
            Debug.Log(request.responseCode);

            Debug.Log("=== RESPONSE BODY ===");
            Debug.Log(request.downloadHandler.text);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("REQUEST ERROR: " + request.error);
                return;
            }

            string extracted = ExtractAIText(request.downloadHandler.text);

            Debug.Log("=== EXTRACTED TEXT ===");
            Debug.Log(extracted);
        }
    }

    // ? Универсальный парсер для Cloudflare Llama
    string ExtractAIText(string json)
    {
        try
        {
            // Вариант 1: result.response
            int responseIndex = json.IndexOf("\"response\"");
            if (responseIndex != -1)
            {
                return ExtractStringValue(json, responseIndex);
            }

            // Вариант 2: result.output[0].content
            int contentIndex = json.IndexOf("\"content\"");
            if (contentIndex != -1)
            {
                return ExtractStringValue(json, contentIndex);
            }

            return "Не удалось найти текст ответа.\n\nRAW:\n" + json;
        }
        catch (System.Exception e)
        {
            return "Ошибка парсинга: " + e.Message;
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