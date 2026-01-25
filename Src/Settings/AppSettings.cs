using System.Text.Json;
using Neurocommenting.Infrastructure;

namespace Neurocommenting;

public class AppSettings
{
    public AppSettingsDto Value { get; init; }

    public AppSettings(string pathToSettings)
    {
        using (FileStream fs = new FileStream(pathToSettings, FileMode.OpenOrCreate))
        {
            try
            {
                Value = JsonSerializer.Deserialize<AppSettingsDto>(fs);
            }
            catch (Exception exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                throw new Exception($"Ошибка в файле настроек скрипта Settings.json. Не найден параметр {exception.Message.Split().Last()}");
            }
        }
    }

    public class AppSettingsDto
    {
        public required string PromptTone { get; set; }
        public required int MinWordsInPost { get; set; }
        public required int AccountCommentsLimit { get; set; }
        public required int AccountChannelsJoinLimit { get; set; }
        public required int DelayAfterCommentsLimit { get; set; }
        public required int DelayBeforeJoin { get; set; }
        public required int DelayBeforeCommenting { get; set; }
        public required int SkipPostsBeforeCommenting { get; set; }
        public required string IONetApiKey { get; set; }
        public required Proxy ProxyForIONet { get; set; }
        public required List<AiModel> AiModels { get; set; }
    }
}