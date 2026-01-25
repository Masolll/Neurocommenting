using Starksoft.Net.Proxy;

namespace Neurocommenting.Infrastructure;

public class Proxy
{
    public string Ip { get; set; }
    public int Port { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }

    public Proxy(string ip, int port, string login, string password)
    {
        Ip = ip;
        Port = port;
        Login = login;
        Password = password;
    }

    public static bool IsValid(string proxyLine)
    {
        try
        {
            var proxyData = Parse(proxyLine);
            var proxyClient = new Socks5ProxyClient(proxyData.Ip, proxyData.Port, proxyData.Login, proxyData.Password);
            var telegramServerIp = "149.154.167.50";
            var telegramServerPort = 443;
            using var stream = proxyClient.CreateConnection(telegramServerIp, telegramServerPort);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static Proxy Parse(string proxy)
    {
        try
        {
            var proxyParts = proxy.Split(':');
            var ip = proxyParts[0];
            var port = Int32.Parse(proxyParts[1]);
            var login = proxyParts[2];
            var password = proxyParts[3];
            return new Proxy(ip, port, login, password);
        }
        catch
        {
            throw new ArgumentException(
                $"Ошибка при парсинге прокси {proxy}. Прокси должен быть в формате login:password@ip:port"
            );
        }
    }
}
