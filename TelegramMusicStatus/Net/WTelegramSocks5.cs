using Starksoft.Net.Proxy;
using TelegramMusicStatus.Config;
using WTelegram;

namespace TelegramMusicStatus.Net;

internal static class WTelegramSocks5
{
    public static void ApplyIfConfigured(Client client, TelegramSocks5Proxy? socks)
    {
        if (socks is null) return;

        client.TcpHandler = async (host, port) =>
        {
            var proxyClient = string.IsNullOrEmpty(socks.User)
                ? new Socks5ProxyClient(socks.Host, socks.Port)
                : new Socks5ProxyClient(socks.Host, socks.Port, socks.User, socks.Password ?? string.Empty);

            return await Task.Run(() => proxyClient.CreateConnection(host, port));
        };
    }
}