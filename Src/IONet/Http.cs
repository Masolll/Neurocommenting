using System.Net;
using Neurocommenting.Infrastructure;

namespace Neurocommenting.IONet;

public class Http
{
    public HttpClient Client { get; init; }

    public Http(Proxy proxy)
    {
        var webProxy = new WebProxy($"socks5://{proxy.Ip}:{proxy.Port}")
        {
            Credentials = new NetworkCredential(proxy.Login, proxy.Password)
        };
        Client = new HttpClient(
            new SocketsHttpHandler() { Proxy = webProxy, UseProxy = true }
        );
    }
}
