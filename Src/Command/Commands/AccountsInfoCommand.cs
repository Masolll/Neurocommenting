using System.Text;
using Neurocommenting.Infrastructure;
using Neurocommenting.Telegram;

namespace Neurocommenting.Command.Commands;

public class AccountsInfoCommand : ICommand
{
    private TelegramClientManager receiver;

    public AccountsInfoCommand(TelegramClientManager receiver)
    {
        this.receiver = receiver;
    }
    
    public Task ExecuteAsync()
    {
        var totalCountMonitorChats = 0;
        var totalCountComments = 0;
        var stringInfo = new StringBuilder();
        stringInfo.AppendLine("Информация по всем запущенным аккаунтам:");
        foreach (var account in receiver.Accounts)
        {
            totalCountMonitorChats += account.ChannelsAndDiscussions.Count;
            totalCountComments += account.CommentsCount;
            stringInfo.AppendLine($"Аккаунт ID: {account.Client.UserId} {{");
            stringInfo.AppendLine($"    Оставил комментариев: {account.CommentsCount}");
            stringInfo.AppendLine($"    Отслеживает каналов: {account.ChannelsAndDiscussions.Count}");
            stringInfo.AppendLine("}");
        }
        stringInfo.AppendLine($"Всего комментариев: {totalCountComments}");
        stringInfo.AppendLine($"Всего каналов: {totalCountMonitorChats}");
        Printer.Print(stringInfo.ToString(), ConsoleColor.Magenta);
        
        return Task.CompletedTask;
    }
}