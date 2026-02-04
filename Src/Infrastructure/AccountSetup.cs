using System.Text;
using Starksoft.Net.Proxy;
using System.Text.Json;
using System.Text.Json.Nodes;
using Neurocommenting.Settings;

namespace Neurocommenting.Infrastructure;

public static class AccountSetup
{
    public static async Task<WTelegram.Client> LoginAccountAsync(string pathToAccount)
    {
        AccountConfig config;
        var pathToAccountSettings = AppPaths.AccountConfigFile(pathToAccount);
        try
        {
            config = JsonSerializer.Deserialize<AccountConfig>(File.ReadAllText(pathToAccountSettings));
            config.SessionPath = AppPaths.AccountSessionFile(pathToAccount);
        }
        catch (Exception exception)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            throw new Exception($"Ошибка в файле аккаунта Settings.json. Не найден параметр {exception.Message.Split().Last()}");
        }
        var client = new WTelegram.Client(config.GetConfig);
        
        var pathToGroup = Directory.GetParent(pathToAccount).FullName;
        var pathToGroupConfig = Path.Combine(pathToGroup, "Group.json");
        var groupConfig = JsonSerializer.Deserialize<GroupConfig>(File.ReadAllText(pathToGroupConfig));

        if (groupConfig.GroupProxy.Enabled)
        {
            client.TcpHandler = async (address, port) =>
            {
                var proxy = new Socks5ProxyClient(
                    groupConfig.GroupProxy.Ip, 
                    groupConfig.GroupProxy.Port, 
                    groupConfig.GroupProxy.Login, 
                    groupConfig.GroupProxy.Password);
                return proxy.CreateConnection(address, port);
            };
        }
        try
        {
            Printer.PrintInfo($"Попытка входа в аккаунт c ID: {config.UserId}");
            await client.LoginUserIfNeeded();
            Printer.PrintSuccess($"Успешный вход в аккаунт c ID: {client.UserId}");
            return client;
        }
        catch
        {
            await client.DisposeAsync();
            throw;
        }
    }
    
    public static void AddAccount()
    {
        Console.WriteLine("Введите название группы, в которую хотите добавить аккаунт");
        var groupName = Console.ReadLine();
        var pathToGroupFolder = AppPaths.GroupFolder(groupName);
        
        if (Directory.Exists(pathToGroupFolder))
        {
            CreateAccountInGroup(groupName);
            Printer.PrintSuccess("Добавление аккаунта успешно завершено!");
        }
        else
        {
            Printer.PrintError(@$"Ошибка: В папке {AppPaths.AccountsFolder} не найдена групповая папка с именем {groupName}");
        }
    }
    
    private static void CreateAccountInGroup(string groupName)
    {
        Printer.PrintInfo($"Создание нового аккаунта в {groupName}");
        Console.Write("Введите User ID нового аккаунта: ");
        var userId = Console.ReadLine();

        var pathToAccount = AppPaths.AccountFolder(groupName, userId);
        Directory.CreateDirectory(pathToAccount);
        
        var pathToAccountSettings = AppPaths.AccountConfigFile(pathToAccount);
        JsonNode accountSettings = new JsonObject();

        Console.WriteLine("Создайте приложение на сайте https://my.telegram.org для получения api_id и api_has");
        Console.WriteLine("Введите api_id: ");
        accountSettings["ApiId"] = Console.ReadLine();
        Console.WriteLine("Введите api_hash: ");
        accountSettings["ApiHash"] = Console.ReadLine();
        accountSettings["UserId"] = userId;
        Console.WriteLine("Введите номер телефона нового аккаунта: ");
        accountSettings["Phone"] = Console.ReadLine();
        File.WriteAllText(pathToAccountSettings, accountSettings.ToJsonString(), Encoding.UTF8);
    }

    public static void CreateGroup()
    {
        Printer.PrintInfo("Создание новой группы аккаунтов...");
        Console.WriteLine("Вы хотите использовать прокси для группы? (Y/N)");
        var groupProxy = new Proxy();
        var pathToNewGroup = AppPaths.GroupFolder("Group(NoProxy)");
        var groupConfigFile = AppPaths.GroupConfigFile("Group(NoProxy)");
        if (Console.ReadKey().Key == ConsoleKey.Y)
        {
            Console.Write("Введите прокси Socks5 для группы (формат ip:port:login:password):");
            var inputProxy = Console.ReadLine();
            groupProxy = Proxy.Parse(inputProxy);
            if (!Proxy.IsValid(groupProxy))
            {
                Printer.PrintError($"Не удалось установить соединение с сервером телеграмм с помощью прокси {inputProxy}. Создание группы остановлено.");
                return;
            }
            Printer.PrintSuccess("Успешное подключение к серверам телеграм через указанный прокси!");
            pathToNewGroup = AppPaths.GroupFolder(groupProxy);
            groupConfigFile = AppPaths.GroupConfigFile(groupProxy);
        }
        if (Directory.Exists(pathToNewGroup))
            Printer.PrintError($"Такая группа уже существует {pathToNewGroup}");
        else
        {
            Directory.CreateDirectory(pathToNewGroup);
            JsonNode groupConfig = new JsonObject();
            groupConfig["GroupProxy"] = JsonSerializer.SerializeToNode(groupProxy);
            File.WriteAllText(groupConfigFile, groupConfig.ToJsonString(), Encoding.UTF8);
            Printer.PrintSuccess($"Успешно создана новая группа по пути: {pathToNewGroup}");
        }
    }
}