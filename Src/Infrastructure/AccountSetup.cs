using System.Text;
using Starksoft.Net.Proxy;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Neurocommenting.Infrastructure;

public static class AccountSetup
{
    public static async Task<WTelegram.Client> LoginAccountAsync(string pathToAccount)
    {
        AccountConfig config;
        var pathToAccountSettings = AppPaths.AccountSettingsFile(pathToAccount);
        using (FileStream fs = new FileStream(pathToAccountSettings, FileMode.OpenOrCreate))
        {
            try
            {
                config = JsonSerializer.Deserialize<AccountConfig>(fs);
            }
            catch (Exception exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                throw new Exception($"Ошибка в файле аккаунта Settings.json. Не найден параметр {exception.Message.Split().Last()}");
            }
        }

        var client = new WTelegram.Client(config.GetConfig);
        var proxyData = Proxy.Parse(config.Proxy);
        client.TcpHandler = async (address, port) =>
        {
            var proxy = new Socks5ProxyClient(proxyData.Ip, proxyData.Port, proxyData.Login, proxyData.Password);
            return proxy.CreateConnection(address, port);
        };
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
        
        var pathToAccountSettings = AppPaths.AccountSettingsFile(pathToAccount);
        JsonNode accountSettings = new System.Text.Json.Nodes.JsonObject();

        Console.WriteLine("Создайте приложение на сайте https://my.telegram.org для получения api_id и api_has");
        Console.WriteLine("Введите api_id: ");
        accountSettings["ApiId"] = Console.ReadLine();
        Console.WriteLine("Введите api_hash: ");
        accountSettings["ApiHash"] = Console.ReadLine();
        accountSettings["UserId"] = userId;
        Console.WriteLine("Введите номер телефона нового аккаунта: ");
        accountSettings["Phone"] = Console.ReadLine();
        accountSettings["Proxy"] = File.ReadAllText(AppPaths.GroupProxyFile(groupName));
        accountSettings["SessionPath"] = AppPaths.AccountSessionFile(pathToAccount);
        File.WriteAllText(pathToAccountSettings, accountSettings.ToJsonString(), Encoding.UTF8);
    }

    public static void CreateGroup()
    {
        Printer.PrintInfo("Создание новой группы аккаунтов...");
        Console.Write("Введите прокси Socks5 для группы (формат ip:port:login:password):");
        var inputProxy = Console.ReadLine();
        
        var groupProxy = Proxy.Parse(inputProxy);
        var pathToNewGroup = AppPaths.GroupFolder(groupProxy);

        if (Directory.Exists(pathToNewGroup))
            Printer.PrintError("Группа с таким прокси уже существует");
        else if (!Proxy.IsValid(inputProxy))
            Printer.PrintError($"Не удалось установить соединение с сервером телеграмм с помощью прокси {inputProxy}");
        else
        {
            Printer.PrintSuccess("Успешное подключение к серверам телеграм через указанный прокси!");
            Directory.CreateDirectory(pathToNewGroup);
            var pathToProxyGroup = AppPaths.GroupProxyFile(groupProxy);
            File.WriteAllText(pathToProxyGroup, inputProxy);
            Printer.PrintSuccess($"Успешно создана новая группа по пути: {pathToNewGroup}");
        }
    }
}