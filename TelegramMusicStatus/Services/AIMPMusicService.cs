using Newtonsoft.Json;
using TelegramMusicStatus.Config;
using TelegramMusicStatus.Models;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace TelegramMusicStatus.Services;

public interface IAIMPMusicService : IMusicService
{
    Task Close();
}

public class AIMPMusicService : IAIMPMusicService
{
    private readonly WebSocketServer _wssv;
    private static bool IsPlaying { get; set; }
    private static string? Artist { get; set; }
    private static string? TrackTitle { get; set; }
    private readonly IConfig<MainConfig> _config;

    public AIMPMusicService(IConfig<MainConfig> config)
    {
        this._config = config;
        var ws = this._config.Entries.AimpWebSocket
                 ?? throw new InvalidOperationException("AimpWebSocket is not configured.");
        this._wssv = new WebSocketServer($"ws://{ws.Ip}:{ws.Port}");
        this._wssv.AddWebSocketService<ApiService>("/aimp");
        this._wssv.Log.Level = LogLevel.Info;
        this._wssv.Start();
        Utils.WriteLine("WebSocket Server started.\nIP: " + this._wssv.Address);
        Utils.WriteLine("Port: " + this._wssv.Port);
    }

    private class ApiService : WebSocketBehavior
    {
        protected override void OnClose(CloseEventArgs e)
        {
            Console.WriteLine("Connection closed. IsPlaying is false.");
            (TrackTitle, Artist, IsPlaying) = (null, null, false);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            if (e.IsPing || string.IsNullOrEmpty(e.Data)) return;
            var message = JsonConvert.DeserializeObject<TrackInfoMessage>(e.Data);
            if (message is not null) (TrackTitle, Artist, IsPlaying) = message;
            this.Sessions.Broadcast($"Successfully retrieve:{TrackTitle}-{Artist} is {IsPlaying}");
        }
    }

    public Task Close()
    {
        this._wssv.Stop();
        Utils.WriteLine("WebSocket Server stopped.");
        return Task.CompletedTask;
    }

    public Task<(bool IsPlaying, string? Bio)> GetCurrentlyPlayingStatus()
    {
        return Task.FromResult((IsPlaying, TrackTitle is null ? null : $"{TrackTitle} - {Artist}"));
    }
}