using Neurocommenting.Infrastructure;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Neurocommenting.IONet;

public class AiModelsManager
{
    public string CurrentModelId { get; private set; }
    private Http http;
    private readonly object lockObject = new();
    private readonly List<AiModel> modelsByPriority;
    private readonly AppSettings settings;

    public AiModelsManager(AppSettings settings)
    {
        this.settings = settings;
        http = new Http(settings.Value.ProxyForIONet);
        modelsByPriority = settings.Value.AiModels.OrderBy(e => e.Priority).ToList();
        CurrentModelId = modelsByPriority.First().Id;
    }

    public async Task<string> GenerateCommentAsync(string postContent)
    {
        var systemPrompt = GetSystemPrompt();
        var modelForGenerate = CurrentModelId;
        var requestBody = new RequestBodyDto(
            model: modelForGenerate,
            messages: new List<MessageDto>()
            {
                new MessageDto(role: "system", content: systemPrompt),
                new MessageDto(role: "user", content: $"Вот содержание поста: {postContent}")
            }
        );
        var requestBodyJson = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.intelligence.io.solutions/api/v1/chat/completions");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.Value.IONetApiKey);
        requestMessage.Content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json");
        var response = await http.Client.SendAsync(requestMessage);

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                BlockAndUpdateModel(modelForGenerate);
            }
            throw new Exception($"API IO.NET вернул ответ с кодом {(int)response.StatusCode}. Ответ от сервера: {await response.Content.ReadAsStringAsync()}");
        }

        var resultComment = await ExtractCommentTextAsync(response);
        return resultComment;
    }

    private string GetSystemPrompt()
    {
        var pathToPrompt = AppPaths.PromptFile;
        var prompt = File.ReadAllText(pathToPrompt);
        prompt += $"Тон промпта: {settings.Value.PromptTone}";
        return prompt;
    }

    private async Task<string> ExtractCommentTextAsync(HttpResponseMessage response)
    {
        var responseString = await response.Content.ReadAsStringAsync();
        var responseJsonObject = JsonNode.Parse(responseString).AsObject();
        if (responseJsonObject.TryGetPropertyValue("choices", out var choices))
        {
            var content = choices[0]["message"]["content"].GetValue<string>();
            var commentText = content.Split("</think>\n").Last();
            if (commentText.StartsWith("\n"))
            {
                commentText = commentText.Substring(1);
            }
            if (commentText.First() == '"' && commentText.Last() == '"')
            {
                commentText = commentText.Substring(1, commentText.Length - 2);
            }
            return commentText;
        }
        else
        {
            throw new Exception($"API IO.NET вернул неизвестный ответ: {responseString}");
        }
    }

    private void BlockAndUpdateModel(string blockModelId)
    {
        lock (lockObject)
        {
            BlockModel(blockModelId);
            UpdateModelId();
        }
    }

    private void BlockModel(string modelId)
    {
        var model = modelsByPriority.First(e => e.Id == modelId);
        if (!model.IsBlocked)
        {
            model.BlockUntill = DateTime.Now.AddDays(1);
            Printer.PrintError($"Превышен дневной лимит запросов к ИИ. Модель {CurrentModelId} заблокирована до {DateTime.Now.AddDays(1)}");
        }
    }

    private void UpdateModelId()
    {
        var currentModel = modelsByPriority.First(e => e.Id == CurrentModelId);
        if (currentModel.IsBlocked)
        {
            foreach (var model in modelsByPriority)
            {
                if (!model.IsBlocked)
                {
                    CurrentModelId = model.Id;
                    Printer.PrintSuccess($"ИИ модель обновлена на {model.Id}");
                    return;
                }
            }
            var nextModel = modelsByPriority.OrderBy(e => e.BlockUntill).First();
            Printer.PrintError($"Все ИИ модели в блоке. Ближайшая дата разблокировки: {nextModel.BlockUntill} модель: {nextModel.Id}");
        }
    }
}