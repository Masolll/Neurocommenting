using Neurocommenting.Infrastructure;
using Neurocommenting.IONet;
using Neurocommenting.Telegram;

namespace Neurocommenting;

public class Program
{
    static async Task Main()
    {
        WTelegram.Helpers.Log = Logger.Write;
        var commentingIsStarted = false;

        var settings = new AppSettings(AppPaths.AppSettingsFile);
        var aiModelsManager = new AiModelsManager(settings);
        var accountsManager = new TelegramClientManager(settings, aiModelsManager);

        Console.WriteLine("Нажмите клавишу:" + Environment.NewLine
            + "1 - Запустить нейрокомментинг" + Environment.NewLine
            + "2 - Войти в каналы из списка" + Environment.NewLine
            + "3 - Добавить новый аккаунт" + Environment.NewLine
            + "4 - Создать новую группу");
        var mode = Console.ReadKey().KeyChar;
        switch (mode)
        {
            case '1':
                commentingIsStarted = true;
                await accountsManager.InitializeAccountsAsync();
                await accountsManager.InitializeJoinedChannelsAsync();
                accountsManager.RunReceivingUpdates();
                Printer.PrintInfo("Ожидание выхода новых постов!");
                break;
            case '2':
                await accountsManager.InitializeAccountsAsync();
                await accountsManager.JoinDiscussionChatsAsync();
                break;
            case '3':
                AccountSetup.AddAccount();
                break;
            case '4':
                AccountSetup.CreateGroup();
                break;
            default:
                Printer.PrintError("Ошибка: Введена неверная комманда");
                break;
        }
            
        if (commentingIsStarted)
        {
            Console.WriteLine("Введите команду \"info\" для просмотра информации о работе аккаунтов.");
            while (true)
            {
                var command = Console.ReadLine();
                if (command == "info")
                {
                    accountsManager.PrintAccountsInfo();
                }
                else
                {
                    Printer.PrintError("Введена неизвестная команда. Для просмотра информации введите \"info\"");
                }
            }
        }

        Printer.PrintInfo("Скрипт завершил работу.");
    }
}