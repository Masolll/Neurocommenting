using Neurocommenting.Infrastructure;
using System.Text;
using Neurocommenting.IONet;
using Neurocommenting.Channels;
using Neurocommenting.Telegram.Handlers;
using TL;

namespace Neurocommenting.Telegram;

public class TelegramClientManager
{
    private List<TelegramClient> accounts;
    private readonly AppSettings settings;
    private readonly AiModelsManager aiModelsManager;

    public TelegramClientManager(AppSettings settings, AiModelsManager aiModelManager)
    {
        this.settings = settings;
        this.aiModelsManager = aiModelManager;
    }

    public async Task InitializeAccountsAsync()
    {
        accounts = new List<TelegramClient>();

        var pathToAccounts = AppPaths.AccountsFolder;
        var groups = Directory.GetDirectories(pathToAccounts, "Group*");
        foreach (var groupPath in groups)
        {
            var accountsCurrentGroup = Directory.GetDirectories(groupPath, "Account*");
            foreach (var accountPath in accountsCurrentGroup)
            {
                try
                {
                    var client = await AccountSetup.LoginAccountAsync(accountPath);
                    accounts.Add(new TelegramClient(client, settings, aiModelsManager));
                }
                catch (Exception exception)
                {
                    throw new Exception($"Ошибка при входе в аккаунт {accountPath}. Сообщение ошибки: {exception.Message}");
                }
            }
        }
        if (accounts.Count == 0)
        {
            throw new Exception("Нет ни одного рабочего аккаунта! Дальнейшая работа скрипта невозможна.");
        }
        Printer.PrintSuccess("Завершена инициализация аккаунтов");
    }

    public async Task InitializeJoinedChannelsAsync()
    {
        var initializeTasks = new List<Task>();
        foreach (var currentAccount in accounts)
        {
            initializeTasks.Add(currentAccount.InitializeJoinedChannelsAsync());
        }
        await Task.WhenAll(initializeTasks);
        Printer.PrintSuccess("Инициализация всех дискуссионных чатов завершена.");
    }

    public async Task JoinDiscussionChatsAsync()
    {
        var joinTasks = new List<Task>();
        ChannelsRepository.OpenFile();
        foreach (var currentAccount in accounts)
        {
            joinTasks.Add(currentAccount.JoinDiscussionChatsAsync());
        }
        await Task.WhenAll(joinTasks);
        ChannelsRepository.DisposeResources();
        Printer.PrintSuccess("Все аккаунты завершили вход в дискуссионные чаты.");
    }
    
    public void RunReceivingUpdates()
    {
        foreach (var account in accounts)
        {
            account.UpdateProvider = new UpdateHandler(account, settings);
            account.Client.WithUpdateManager(account.UpdateProvider.ExecuteHandler);
            Printer.PrintSuccess($"Аккаунт с ID: {account.Client.UserId} готов получать обновления!");
        }
    }

    public void PrintAccountsInfo()
    {
        var totalCountMonitorChats = 0;
        var totalCountComments = 0;
        var stringInfo = new StringBuilder();
        foreach (var account in accounts)
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
    }
}

