using Swan;
using TelegramMusicStatus.Config;
using TelegramMusicStatus.Models;
using Yandex.Music.Api;
using Yandex.Music.Api.Common;
using Yandex.Music.Api.Common.Ynison;

namespace TelegramMusicStatus.Services;

public interface IYandexMusicService : IMusicService
{
    new Task<(bool IsPlaying, string? Bio)> GetCurrentlyPlayingStatus();
}

public sealed class YandexMusicService : IYandexMusicService
{
    private readonly YandexMusicApi _api;
    private YnisonPlayer _player;
    private readonly AuthStorage _storage;
    private YandexMusicState? _state = null;

    public YandexMusicService(IConfig<MainConfig> config)
    {
        var ym = config.Entries.YandexMusicAccount;

        if (ym is null) return;
        this._storage = new AuthStorage { DeviceId = Guid.NewGuid().ToString() };

        this._api = new YandexMusicApi();
        this._api.User.Authorize(this._storage, ym.Token);


        this.GetPlayer();
        
        Utils.WriteLine("Yandex Music client started!");
    }
    
    private void GetPlayer()
    {
        Utils.WriteLine($"Getting player...\n Current Yandex user is {this._api.User.GetLoginInfo(this._storage).Login}");
        this._player = this._api.Ynison.GetPlayer(this._storage);
        this._player.Connect();
        Thread.Sleep(5000);
        this._player.OnClose += PlayerOnOnClose;
    }

    private void PlayerOnOnClose(YnisonPlayer player, YnisonPlayer.CloseEventArgs args)
    {
        this.GetPlayer();
    }

    public async Task<(bool IsPlaying, string? Bio)> GetCurrentlyPlayingStatus()
    {
        try
        {
            var status = this._player.State.PlayerState?.Status;
            var track = this._player.Current;
            var bio = $"{track.Title} - {string.Join(", ", track.Artists.Select(a => a.Name))}";
            var now = DateTime.UtcNow;

            if (status == null || (status.Paused && this._state is not null && this._state.Id == track.Id && now > this._state.EstimatedFinish)) return (false, null);
            if (this._state is not null && this._state.Id == track.Id && now > this._state.FirstSeen && now < this._state.EstimatedFinish) return (true, bio);
            this._state = new YandexMusicState(status.Paused, track.Id, now, now.AddMilliseconds(track.DurationMs));

            return (true, bio);
        }
        catch (Exception ex)
        {
            Utils.WriteLine("Ynison boom\n" + ex.ToJson());
            return (false, null);
        }
    }

    private record YandexMusicState(bool IsPaused, string Id, DateTime FirstSeen, DateTime EstimatedFinish);
}