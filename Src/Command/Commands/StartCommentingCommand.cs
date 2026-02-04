using Neurocommenting.Telegram;
using Neurocommenting.Infrastructure;

namespace Neurocommenting.Command.Commands;

public class StartCommentingCommand : ICommand
{
    private TelegramClientManager receiver;
    
    public StartCommentingCommand(TelegramClientManager receiver)
    {
        this.receiver = receiver;
    }
    
    public async Task ExecuteAsync()
    {
        if (!receiver.CommentingIsStarted)
        {
            await receiver.InitializeAccountsIfNeededAsync();
            await receiver.InitializeJoinedChannelsAsync();
            receiver.RunReceivingUpdates();
        }
        else
        {
            Printer.PrintError("Нейрокомментинг уже запущен!");
        }
    }
}