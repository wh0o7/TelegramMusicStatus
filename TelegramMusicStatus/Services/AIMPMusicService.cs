using Newtonsoft.Json;
using TelegramMusicStatus.Config;
using TelegramMusicStatus.Models;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace TelegramMusicStatus.Services;

public interface IAIMPMusicService : IMusicService
{
    new Task<(bool IsPlaying, string? Bio)> GetCurrentlyPlayingStatus();
    Task? Close();
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
        _wssv = new WebSocketServer(
            $"ws://{this._config.Entries.AimpWebSocket.Ip}:{this._config.Entries.AimpWebSocket.Port}");
        _wssv.AddWebSocketService<ApiService>("/aimp");
        _wssv.Log.Level = LogLevel.Info;
        _wssv.Start();
        Utils.WriteLine("WebSocket Server started.\nIP: " + _wssv.Address);
        Utils.WriteLine("Port: " + _wssv.Port);
    }

    private class ApiService : WebSocketBehavior
    {
        protected override void OnClose (CloseEventArgs e)
        {
            Console.WriteLine("Connection closed. IsPlaying is false.");
            (TrackTitle, Artist, IsPlaying) = (null,null, false);
        }
        
        protected override void OnMessage(MessageEventArgs e)
        {
            if (e.IsPing || string.IsNullOrEmpty(e.Data)) return;
            var message = JsonConvert.DeserializeObject<TrackInfoMessage>(e.Data);
            if (message is not null) (TrackTitle, Artist, IsPlaying) = message;
            Sessions.Broadcast($"Successfully retrieve:{TrackTitle}-{Artist} is {IsPlaying}");
        }
    }

    public Task Close()
    {
        _wssv.Stop();
        Utils.WriteLine("WebSocket Server stopped.");
        return Task.CompletedTask;
    }

    public Task<(bool IsPlaying, string? Bio)> GetCurrentlyPlayingStatus()
    {
        return Task.FromResult((IsPlaying, TrackTitle is null ? null : $"{TrackTitle} - {Artist}"));
    }
}