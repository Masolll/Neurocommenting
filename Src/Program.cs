using Neurocommenting.Infrastructure;
using Neurocommenting.IONet;
using Neurocommenting.Telegram;
using Neurocommenting.Command;
using Neurocommenting.Command.Commands;
using Neurocommenting.Settings;

namespace Neurocommenting;

class Program
{
    static async Task Main()
    {
        WTelegram.Helpers.Log = Logger.Write;

        var settings = new AppSettings(AppPaths.AppSettingsFile);
        var aiModelsManager = new AiModelsManager(settings);
        var accountsManager = new TelegramClientManager(settings, aiModelsManager);

        var invoker = new CommandInvoker();
        invoker.Set('1', new StartCommentingCommand(accountsManager));
        invoker.Set('2', new  JoinChannelsCommand(accountsManager));
        invoker.Set('3', new AddAccountCommand());
        invoker.Set('4', new CreateGroupCommand());
        invoker.Set('i', new AccountsInfoCommand(accountsManager));

        while (true)
        {
            Console.WriteLine("Нажмите клавишу:" + Environment.NewLine
                + "1 - Запустить нейрокомментинг" + Environment.NewLine
                + "2 - Войти в каналы из списка" + Environment.NewLine
                + "3 - Добавить новый аккаунт" + Environment.NewLine
                + "4 - Создать новую группу" + Environment.NewLine
                + "i - Посмотреть информацию о запущенных аккаунтах");
            var mode = Console.ReadKey().KeyChar;
            await invoker.Run(mode);
        }
    }
}