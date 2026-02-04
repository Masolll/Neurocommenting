using Neurocommenting.Infrastructure;
using Neurocommenting.IONet;
using TL;
using WTelegram;
using Neurocommenting.Channels;
using Neurocommenting.Telegram.Handlers;

namespace Neurocommenting.Telegram;

public class TelegramClient
{
    public WTelegram.Client Client { get; }
    
    public Dictionary<long, InputPeerChannel> ChannelsAndDiscussions { get; } = new Dictionary<long, InputPeerChannel>();
    public Dictionary<long, string> ChannelsUsernames { get; } = new Dictionary<long, string>();
    public UpdateHandler UpdateProvider { get; init; }
    public int CommentsCount { get; set; }
    public int SkipPostsBeforeCommenting { get; set; }

    private readonly AppSettings settings;
    private readonly AiModelsManager aiModelsManager;

    public TelegramClient(WTelegram.Client client, AppSettings settings, AiModelsManager aiModelsManager)
    {
        Client = client;
        this.settings = settings;
        this.aiModelsManager = aiModelsManager;
        UpdateProvider = new UpdateHandler(this, settings);
    }

    public async Task CheckCommentsLimitAsync()
    {
        if (CommentsCount % settings.Value.AccountCommentsLimit == 0)
        {
            var delay = settings.Value.DelayAfterCommentsLimit;
            Printer.PrintWarning($"Достигнут лимит комментариев аккаунтом с ID: {Client.UserId}. Ожидание {delay} секунд перед продолжением работы аккаунта.");
            await Task.Delay(delay * 1000);
        }
    }

    public async Task WriteCommentAsync(MessageBase discussionMessage)
    {
        try
        {
            if (ChannelsAndDiscussions.TryGetValue(discussionMessage.From.ID, out var commentGroup))
            {
                var comments = await Client.Messages_GetHistory(commentGroup);
                var lastPost = comments.Messages.FirstOrDefault(e => e.ID == discussionMessage.ID);
                if (lastPost != null)
                {
                    var replyPostMessage = new InputReplyToMessage() { reply_to_msg_id = lastPost.ID };
                    var postContent = discussionMessage.ToString().Split('>')[1].Trim();
                    var comment = await aiModelsManager.GenerateCommentAsync(postContent);
                    await Client.Messages_SendMessage(
                        peer: commentGroup,
                        message: comment,
                        random_id: Helpers.RandomLong(),
                        reply_to: replyPostMessage
                    );
                    Printer.PrintSuccess($"Успешно оставлен комментарий в канале @{ChannelsUsernames[discussionMessage.From.ID]}. Комментарий: {comment}");
                    CommentsCount++;
                    await CheckCommentsLimitAsync();
                }
            }
        }
        catch (RpcException exception) when (exception.Message.Contains("FLOOD_WAIT_X") || exception.Message.Contains("FROZEN_METHOD_INVALID")
        || exception.Message.Contains("USER_DEACTIVATED_BAN"))
        {
            Printer.PrintError($"Аккаунт с ID: {Client.UserId} получил ошибку {exception.Message}. Работа с этим аккаунтом завершена.");
            await Client.DisposeAsync();
        }
        catch (RpcException exception) when (exception.Code == 403 && exception.Message.Contains("CHAT_WRITE_FORBIDDEN")
        || exception.Code == 400 && exception.Message.Contains("USER_BANNED_IN_CHANNEL"))
        {
            var channelId = discussionMessage.From.ID;
            var channelUsername = ChannelsUsernames[channelId];
            var commentGroup = ChannelsAndDiscussions[channelId];
            await Client.Channels_LeaveChannel(commentGroup);
            ChannelsAndDiscussions.Remove(channelId);
            ChannelsUsernames.Remove(channelId);
            Printer.PrintError($"Аккаунту с ID: {Client.UserId} запретили писать комментарии для канала @{channelUsername}. Аккаунт больше не состоит в дискуссионном чате этого канала.");
        }
        catch (Exception exception) when (exception.Message.Contains("API IO.NET"))
        {
            Printer.PrintError($"Ошибка при генерации комментария. Сообщение с ошибкой: '{exception.Message}'");
        }
        catch (Exception exception)
        {
            Printer.PrintError($"Возникла неизвестна ошибка при написании комментария аккаунтом с ID: {Client.UserId}. Сообщение с ошибкой: {exception.Message}");
        }
    }

    public async Task InitializeJoinedChannelsAsync()
    {
        var commentChats = await Client.Messages_GetAllChats();
        foreach (var commentChat in commentChats.chats)
        {
            if (commentChat.Value is Channel commentChannel)
            {
                var inputPeerChannel = new InputChannel(commentChannel.id, commentChannel.access_hash);
                var channelFullInfo = await Client.Channels_GetFullChannel(inputPeerChannel);
                
                if (channelFullInfo.full_chat is ChannelFull channelFull
                && channelFullInfo.chats.TryGetValue(channelFull.linked_chat_id, out var linkedChannel)
                && linkedChannel is Channel mainChannel
                && mainChannel.MainUsername != null)
                {
                    ChannelsAndDiscussions[mainChannel.ID]= inputPeerChannel;
                    ChannelsUsernames[mainChannel.ID] = mainChannel.MainUsername;
                    Printer.PrintInfo($"Аккаунт с ID: {Client.UserId} зарегистрировал канал @{mainChannel.MainUsername}.");
                }
            }
            else
            {
                break;
            }
        }
    }

    public async Task JoinDiscussionChatsAsync()
    {
        var currentChannel = ChannelsRepository.GetNextChannel();
        var countJoinChannels = 0;
        while (currentChannel != null && countJoinChannels < settings.Value.AccountChannelsJoinLimit)
        {
            try
            {
                await JoinDiscussionChatAsync(currentChannel);
                countJoinChannels++;
            }
            catch (RpcException exception) when (exception.Code == 400 && exception.Message.Contains("USERNAME"))
            {
                Printer.PrintError($"Неверный юзернейм канала {currentChannel} или аккаунт с ID: {Client.UserId} заморожен.");
            }
            catch (Exception exception) when (exception.Message.Contains("An item with the same key has already been added."))
            {
                Printer.PrintError($"Попытка повторного входа в дискуссионный чат канала {currentChannel}");
            }
            catch (RpcException exception) when (exception.Code == 420 && exception.Message.Contains("FLOOD_WAIT_X"))
            {
                Printer.PrintError($"Аккаунт с ID: {Client.UserId} получил ошибку {exception.Message}. Вход в дискуссионные чаты прекращен для данного аккаунта.");
                break;
            }
            catch (Exception exception)
            {
                Printer.PrintError($"Аккаунт с ID: {Client.UserId} получил ошибку при входе в чат обсуждений канала {currentChannel}, сообщение с ошибкой: {exception.Message}");
            }
            currentChannel = ChannelsRepository.GetNextChannel();
        }
    }

    private async Task JoinDiscussionChatAsync(string linkChannel)
    {
        var channelUsername = ChannelsRepository.ConvertLinkToUsername(linkChannel);
        var channelInfo = await Client.Contacts_ResolveUsername(channelUsername);

        var inputPeerChannel = new InputChannel(channelInfo.Channel.ID, channelInfo.Channel.access_hash);
        var channelFullInfo = await Client.Channels_GetFullChannel(inputPeerChannel);

        if (ChannelsUsernames.ContainsKey(channelInfo.Channel.ID))
        {
            throw new Exception($"Аккаунт с ID: {Client.UserId} уже состоит в дискуссионном чате канала @{channelUsername}. Вход в дискуссионный чат не выполнен.");
        }

        if (channelFullInfo.full_chat is ChannelFull channelFull
            && channelFullInfo.chats.TryGetValue(channelFull.linked_chat_id, out var group)
            && group is Channel groupWithComment)
        {
            var delayBeforeJoin = settings.Value.DelayBeforeJoin;
            Printer.PrintInfo($"Задержка {delayBeforeJoin} сек перед входом в дискуссионный чат канала @{channelUsername} аккаунтом с ID: {Client.UserId}");
            await Task.Delay(delayBeforeJoin * 1000);
            await Client.Channels_JoinChannel(groupWithComment);

            Printer.PrintSuccess($"Аккаунт с ID: {Client.UserId} вошел в дискуссионный чат канала @{channelUsername}");
        }
        else
        {
            throw new Exception($"У канала @{channelUsername} отключены комментарии. Вход в дискуссионный чат не выполнен.");
        }
    }
}

