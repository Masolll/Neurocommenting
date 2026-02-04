using Neurocommenting.Infrastructure;
using Neurocommenting.Telegram;

namespace Neurocommenting.Command.Commands;

public class JoinChannelsCommand : ICommand
{
    private TelegramClientManager receiver;
    
    public JoinChannelsCommand(TelegramClientManager receiver)
    {
        this.receiver = receiver;
    }
    
    public async Task ExecuteAsync()
    {
        await receiver.InitializeAccountsIfNeededAsync();
        await receiver.InitializeJoinedChannelsAsync();
        await receiver.JoinDiscussionChatsAsync();
        if (receiver.CommentingIsStarted)
        {
            Printer.PrintInfo("Чтобы аккаунт начал отслеживать новые каналы перезапустите нейрокомментинг!");
        }
    }
}