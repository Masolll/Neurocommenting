using Starksoft.Net.Proxy;

namespace Neurocommenting.Infrastructure;

public class Proxy
{
    public bool Enabled { get; set; }
    public string Ip { get; set; }
    public int Port { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }

    public static bool IsValid(Proxy proxyData)
    {
        try
        {
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
            return new Proxy()
            {
                Enabled = true,
                Ip = ip,
                Port = port,
                Login = login,
                Password = password
            };
        }
        catch
        {
            throw new ArgumentException(
                $"Ошибка при парсинге прокси {proxy}. Прокси должен быть в формате ip:port:login:password"
            );
        }
    }
}
