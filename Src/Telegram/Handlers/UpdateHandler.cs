using Neurocommenting.Infrastructure;
using Neurocommenting.Settings;
using TL;

namespace Neurocommenting.Telegram.Handlers;

public class UpdateHandler
{
    private readonly TelegramClient telegramClient;
    private readonly AppSettings settings;

    public UpdateHandler(TelegramClient client, AppSettings settings)
    {
        this.telegramClient = client;
        this.settings = settings;
    }

    public async Task ExecuteHandler(Update update)
    {
        switch (update)
        {
            case UpdateNewChannelMessage unm:
                if (unm.message.From != null && telegramClient.ChannelsAndDiscussions.ContainsKey(unm.message.From.ID))
                {
                    var channelUsername = telegramClient.ChannelsUsernames[unm.message.From.ID];
                    var postMessage = unm.message.ToString().Split('>')[1].Trim();
                    
                    if (postMessage.StartsWith("TL.MessageMedia") || telegramClient.SkipPostsBeforeCommenting < settings.Value.SkipPostsBeforeCommenting
                    || postMessage.Split().Length < settings.Value.MinWordsInPost)
                    {
                        telegramClient.SkipPostsBeforeCommenting++;
                        Printer.PrintInfo($"Пропущен пост от @{channelUsername}" + Environment.NewLine
                        + $"Аккаунт ID: {telegramClient.Client.UserId}, текущее число пропущенных постов {telegramClient.SkipPostsBeforeCommenting}");
                        break;
                    }
                    else
                    {
                        telegramClient.SkipPostsBeforeCommenting = 0;
                        Printer.PrintInfo($"Вышел новый пост в канале @{channelUsername} со следующим содержимым: {unm.message.ToString()}");
                        var delayBeforeComment = settings.Value.DelayBeforeCommenting;
                        Printer.PrintInfo($"Задержка {delayBeforeComment} сек перед написанием комментария аккаунтом с ID: {telegramClient.Client.UserId}");
                        await Task.Delay(delayBeforeComment * 1000);
                        await telegramClient.WriteCommentAsync(unm.message);
                    }
                }
                break;
        }
    }
}
