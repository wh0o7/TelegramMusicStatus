using TelegramMusicStatus.Config;
using TelegramMusicStatus.Models;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace TelegramMusicStatus.Services;

public interface IAIMPMusicService
{
}

public class AIMPMusicService : IAIMPMusicService
{
    private WebSocketServer _wssv;
    private static bool IsPlaying { get; set; }
    private static string? Artist { get; set; }
    private static string? TrackTitle { get; set; }
    private IConfig<MainConfig> _config;

    public AIMPMusicService(IConfig<MainConfig> config)
    {
        this._config = config;
    }

    private class APIService : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            var receivedMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<TrackInfoMessage>(e.Data);

            IsPlaying = receivedMessage.IsPlaying;
            Artist = receivedMessage.Artist;
            TrackTitle = receivedMessage.TrackTitle;
        }
    }


    public Task Init()
    {
        _wssv = new WebSocketServer(
            $"ws://{this._config.Entries.AimpWebSocket.Ip}:{this._config.Entries.AimpWebSocket.Port}");
        _wssv.AddWebSocketService<APIService>("/aimp");
        _wssv.Start();
        Console.WriteLine("WebSocket Server started.\nIP: " + _wssv.Address);
        Console.WriteLine("Port: " + _wssv.Port);
        return Task.CompletedTask;
    }

    public Task<(bool IsPlaying, string? Bio)> GetCurrentlyPlayingStatus()
    {
        return Task.FromResult((IsPlaying, $"{TrackTitle} - {Artist}"));
    }
}