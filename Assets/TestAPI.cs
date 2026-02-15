using UnityEngine;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class TestAPI : MonoBehaviour
{
    [ContextMenu("Test Cloudflare AI")]
    public async void TestCloudflareAI()
    {
        // Получите на https://dash.cloudflare.com/profile/api-tokens
        string apiToken = "v0evrauS54DVMbYhdhndrc-V9IDmlvkcYE4TjmkY";

        // Получите на https://dash.cloudflare.com (Account ID справа)
        string accountId = "a0b0bcb493fbd1d65b5be16394b43305";

        // Список моделей: https://developers.cloudflare.com/workers-ai/models/
        string model = "@cf/meta/llama-3.1-8b-instruct";  // Рекомендую

        string url = $"https://api.cloudflare.com/client/v4/accounts/{accountId}/ai/run/{model}";

        string json = @"{
            ""messages"": [
                {
                    ""role"": ""user"",
                    ""content"": ""Скажи привет на русском языке""
                }
            ]
        }";

        using (HttpClient client = new HttpClient())
        {
            Debug.Log("Sending request to Cloudflare AI...");

            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Authorization", $"Bearer {apiToken}");
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.SendAsync(request);
                string body = await response.Content.ReadAsStringAsync();

                Debug.Log($"Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    Debug.Log($"SUCCESS! Response:\n{body}");

                    // Парсим ответ
                    string result = ParseCloudflareResponse(body);
                    Debug.Log($"AI Answer: {result}");
                }
                else
                {
                    Debug.LogError($"Error {response.StatusCode}");
                    Debug.LogError($"Response: {body}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception: {e.Message}");
            }
        }
    }

    string ParseCloudflareResponse(string json)
    {
        try
        {
            // Cloudflare возвращает: {"result":{"response":"текст"}}
            int start = json.IndexOf("\"response\":\"");
            if (start == -1)
            {
                // Альтернативный формат: {"result":{"content":"текст"}}
                start = json.IndexOf("\"content\":\"");
                if (start == -1) return json;
                start += 11;
            }
            else
            {
                start += 12;
            }

            int end = start;
            bool escaped = false;

            for (int i = start; i < json.Length; i++)
            {
                if (json[i] == '\\' && !escaped)
                {
                    escaped = true;
                    continue;
                }

                if (json[i] == '"' && !escaped)
                {
                    end = i;
                    break;
                }

                escaped = false;
            }

            string result = json.Substring(start, end - start);

            return result
                .Replace("\\n", "\n")
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\");
        }
        catch
        {
            return json;
        }
    }
}