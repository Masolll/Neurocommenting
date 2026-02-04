using Neurocommenting.Infrastructure;
using Neurocommenting.IONet;
using Neurocommenting.Channels;
using Neurocommenting.Settings;
using TL;

namespace Neurocommenting.Telegram;

public class TelegramClientManager
{
    public List<TelegramClient> Accounts = new();
    public bool CommentingIsStarted;
    
    private readonly AppSettings settings;
    private readonly AiModelsManager aiModelsManager;

    public TelegramClientManager(AppSettings settings, AiModelsManager aiModelManager)
    {
        this.settings = settings;
        this.aiModelsManager = aiModelManager;
    }

    public async Task InitializeAccountsIfNeededAsync()
    {
        if (Accounts.Count == 0)
        {
            await InitializeAccountsAsync();
        }
    }

    private async Task InitializeAccountsAsync()
    {
        Accounts = new List<TelegramClient>();

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
                    Accounts.Add(new TelegramClient(client, settings, aiModelsManager));
                }
                catch (Exception exception)
                {
                    throw new Exception($"Ошибка при входе в аккаунт {accountPath}. Сообщение ошибки: {exception.Message}");
                }
            }
        }
        if (Accounts.Count == 0)
        {
            throw new Exception("Нет ни одного рабочего аккаунта! Дальнейшая работа скрипта невозможна.");
        }
        Printer.PrintSuccess("Завершена инициализация аккаунтов");
    }

    public async Task InitializeJoinedChannelsAsync()
    {
        var initializeTasks = new List<Task>();
        foreach (var currentAccount in Accounts)
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
        foreach (var currentAccount in Accounts)
        {
            joinTasks.Add(currentAccount.JoinDiscussionChatsAsync());
        }
        await Task.WhenAll(joinTasks);
        ChannelsRepository.DisposeResources();
        Printer.PrintSuccess("Все аккаунты завершили вход в дискуссионные чаты.");
    }
    
    public void RunReceivingUpdates()
    {
        foreach (var account in Accounts)
        {
            CommentingIsStarted = true;
            account.Client.WithUpdateManager(account.UpdateProvider.ExecuteHandler);
            Printer.PrintSuccess($"Аккаунт с ID: {account.Client.UserId} готов получать обновления!");
        }
    }
}

