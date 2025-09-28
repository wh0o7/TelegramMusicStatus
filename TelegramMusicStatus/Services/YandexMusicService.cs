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
    private readonly YnisonPlayer _player;
    private readonly AuthStorage _storage;

    public YandexMusicService(IConfig<MainConfig> config)
    {
        var ym = config.Entries.YandexMusicAccount;

        if (ym is null) return;
        this._storage = new AuthStorage { DeviceId = Guid.NewGuid().ToString() };

        this._api = new YandexMusicApi();
        this._api.User.Authorize(this._storage, ym.Token);
        
        this._player = this._api.Ynison.GetPlayer(this._storage);
        this._player.Connect();
        Thread.Sleep(5000);

        Utils.WriteLine("Yandex Music client started!");
    }

    public async Task<(bool IsPlaying, string? Bio)> GetCurrentlyPlayingStatus()
    {
        try
        {
            var status = this._player.State.PlayerState?.Status;
            if (status == null || status.Paused) return (false, null);
            
            var track = this._player.Current;
            var bio = $"{track.Title} - {string.Join(", ", track.Artists.Select(a => a.Name))}";

            return (true, bio);
        }
        catch (Exception ex)
        {
            Utils.WriteLine("Ynison boom\n" + ex.ToJson());
            return (false, null);
        }
    }
}