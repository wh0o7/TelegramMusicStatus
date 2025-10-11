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
    private YandexMusicApi _api;
    private YnisonPlayer? _player = null;
    private AuthStorage _storage;
    private YandexMusicState? _state;

    public YandexMusicService()
    {
        this.GetPlayer();

        Utils.WriteLine("Yandex Music client started!");
    }

    private void GetPlayer()
    {
        if (this._player is not null) this._player.Dispose();
        var ym = new Config<MainConfig>().Entries.YandexMusicAccount;

        if (ym is null) return;
        this._storage = new AuthStorage { DeviceId = Guid.NewGuid().ToString() };

        this._api = new YandexMusicApi();
        this._api.User.Authorize(this._storage, ym.Token);

        Utils.WriteLine($"Getting player...\n Current Yandex user is {this._api.User.GetUserAuth(this._storage).Result.Account.Login.ToJson()}");
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
            var userAuth = (await this._api.User.GetUserAuthAsync(this._storage)).Result.Account;
            if (userAuth is null || this._player == null)
            {
                Console.WriteLine($"(Yandex Music)   Smth null: \n 1. userAuth is {(userAuth is null ? string.Empty : "not")} null.\n2. player is {(this._player is null ? string.Empty : "not")} null  \n\n\n");
                this.GetPlayer();
            }

            if (this._player is null)
            {
                Console.WriteLine("(Yandex Music)  PLAYER IS NULL 2-ND TIME IN A ROW!!!!!!!!!");
                return (false, null);
            }

            var status = this._player.State.PlayerState?.Status;
            if (status is null)
            {
                this.GetPlayer();
            }

            status = this._player.State.PlayerState?.Status;
            var track = this._player.Current;
            var bio = $"{track.Title} - {string.Join(", ", track.Artists.Select(a => a.Name))}";
            var now = DateTime.UtcNow;

            if (status is null || status.Paused && this._state is not null && this._state.Id == track.Id && now > this._state.EstimatedFinish)
            {
                this.GetPlayer();
                Console.WriteLine("(Yandex Music)  STATUS IS NULL 2-ND TIME IN A ROW!!!!!!!!!" + $"(Yandex Music) [STATE]:{this._state?.ToJson() ?? "null"}\n (Yandex Music) [TIME_NOW]:{now}\n(Yandex Music) [PLAYER_STATE]:{this._player.State.PlayerState?.Status.ToJson()}");
                return (false, null);
            }

            if (this._state is not null && this._state.Id == track.Id && now > this._state.FirstSeen && now < this._state.EstimatedFinish) return (true, bio);
            this._state = new YandexMusicState(status.Paused, track.Id, now, now.AddMilliseconds(track.DurationMs));

            return (true, bio);
        }
        catch (Exception ex)
        {
            Utils.WriteLine("Ynison boom\n" + ex.ToJson());
            this.GetPlayer();
            return (false, null);
        }
    }

    private record YandexMusicState(bool IsPaused, string Id, DateTime FirstSeen, DateTime EstimatedFinish);
}